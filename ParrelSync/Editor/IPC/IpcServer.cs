using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ParrelSync.Threading.Tasks;

namespace ParrelSync.Ipc
{
    internal static class IpcServer
    {
        private static readonly Dictionary<EndPoint, IpcConnectionToClient> _clients = new Dictionary<EndPoint, IpcConnectionToClient>();

        internal static Dictionary<int, IpcMessageDelegate> handlers =
            new Dictionary<int, IpcMessageDelegate>();

        private static CancellationTokenSource _cancellationSource;

        public static event Action<IpcConnectionToClient> ClientConnected;

        private static TcpListener activeListener;

        public static int ClientCount => _clients.Count;

        public static void RegisterHandler<T>(Action<IpcConnectionToClient, T> handler) where T : struct, IIpcMessage
        {
            int id = IpcMessageId<T>.Id;
            handlers[id] = IpcMessages.WrapHandler(handler);
        }

        public static void Start()
        {
            _cancellationSource = new CancellationTokenSource();
            AcceptConnectionLoopAsync(_cancellationSource.Token).Forget();
            HearbeatLoopAsync(_cancellationSource.Token).Forget();
        }

        private static async Task HearbeatLoopAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested)
            {
                SendToAll(new HeartbeatMessage());
                await Task.Delay(1000, cancellationToken);
            }
        }

        public static void Stop()
        {
            _cancellationSource.Cancel();
            _cancellationSource = null;
            activeListener?.Stop();
            activeListener?.Server.Dispose();

            foreach (var kvp in _clients)
            {
                kvp.Value.Dispose();
            }

            _clients.Clear();
        }

        private static async Task AcceptConnectionLoopAsync(CancellationToken cancellationToken)
        {
            activeListener = new TcpListener(IPAddress.Loopback, IpcPortGetter.GetPort());
            activeListener.Start();
            while (!cancellationToken.IsCancellationRequested)
            {
                // accept connections on the main thread
                TcpClient connectedClient = await activeListener.AcceptTcpClientAsync();

                IpcConnectionToClient clientConnection = new IpcConnectionToClient(new TcpConnection(connectedClient, OnMessageReceived, OnClientDisconnected, (messageId) =>
                {
                    if (messageId == IpcMessageId<AssemblyReloadMessage>.Id)
                    {
                        AssemblyReloadWaitHandler.IncrementCurrentCount();
                    }
                }));

                _clients[connectedClient.Client.RemoteEndPoint] = clientConnection;
                ClientConnected?.Invoke(clientConnection);
            }
        }

        private static void OnClientDisconnected(EndPoint endPoint)
        {
            _clients[endPoint].Dispose();
            _clients.Remove(endPoint);
        }

        private static void OnMessageReceived(EndPoint endPoint, QueuedIpcMessage message)
        {
            IpcConnectionToClient connectionToClient = _clients[endPoint];

            if (handlers.TryGetValue(message.messageId, out IpcMessageDelegate handler))
            {
                handler(connectionToClient, message);
            }
        }

        public static void SendToAll<T>(T message) where T : struct, IIpcMessage
        {
            foreach (var kvp in _clients)
            {
                kvp.Value.Send(message);
            }
        }
    }
}
