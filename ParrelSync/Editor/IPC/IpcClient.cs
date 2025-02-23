using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ParrelSync.Threading.Tasks;

namespace ParrelSync.Ipc
{
    internal static class IpcClient
    {
        internal static readonly Dictionary<int, IpcMessageDelegate> handlers =
            new Dictionary<int, IpcMessageDelegate>();

        public static IpcConnectionToServer Connection { get; private set; }
        public static bool IsConnected => Connection != null;

        internal static event Action Connected;

        public static void RegisterHandler<T>(Action<T> handler) where T : struct, IIpcMessage
        {
            int id = IpcMessageId<T>.Id;

            void HandlerWrapped(IpcConnection _, T value) => handler(value);

            handlers[id] = IpcMessages.WrapHandler<IpcConnection, T>(HandlerWrapped);
        }

        static CancellationTokenSource _cancellationTokenSource;
        static CancellationTokenSource _hearbeatCancellationSource;
        static TcpClient _activeTcpClient;

        private static bool _isStopped;

        public static void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            ConnectLoopAsync(_cancellationTokenSource.Token).Forget();
        }

        public static void Stop()
        {
            _activeTcpClient?.Dispose();
            _activeTcpClient = null;
            Connection?.Dispose();
        }

        private static async Task HearbeatLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Send(new HeartbeatMessage());
                await Task.Delay(1000, cancellationToken);
            }
        }

        private static async Task ConnectLoopAsync(CancellationToken cancellationToken)
        {
            _activeTcpClient = new TcpClient();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _activeTcpClient.ConnectAsync(IPAddress.Loopback, IpcPortGetter.GetPort());
                    Connection = new IpcConnectionToServer(new TcpConnection(_activeTcpClient, OnMessageReceived, OnDisconnected));

                    _hearbeatCancellationSource = new CancellationTokenSource();
                    HearbeatLoopAsync(_hearbeatCancellationSource.Token).Forget();
                    Connected?.Invoke();
                    break;
                }
                catch (SocketException)
                {
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        private static void OnDisconnected(EndPoint point)
        {
            _hearbeatCancellationSource.Cancel();
            _hearbeatCancellationSource = null;
            Connection.Dispose();
            Connection = null;

            if (!_isStopped)
            {
                // go back into connection loop when disconnecting unless manually stopped
                ConnectLoopAsync(_cancellationTokenSource.Token).Forget();
            }
        }

        private static void OnMessageReceived(EndPoint endPoint, QueuedIpcMessage message)
        {
            if (handlers.TryGetValue(message.messageId, out IpcMessageDelegate handler))
            {
                handler(null, message);
            }
        }

        public static void Send<T>(T message) where T : struct, IIpcMessage
        {
            if (IsConnected)
            {
                Connection.Send(message);
            }
            else
            {
                UnityEngine.Debug.LogWarning("IPC message not sent, not connected.");
            }
        }
    }
}
