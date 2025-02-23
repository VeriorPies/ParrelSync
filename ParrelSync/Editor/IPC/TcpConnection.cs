using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System;
using ParrelSync.Threading;
using ParrelSync.Threading.Tasks;

namespace ParrelSync.Ipc
{
    internal class TcpConnection : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private readonly TcpPipe _pipe;
        private readonly Action<EndPoint, QueuedIpcMessage> _messageReceivedCallback;
        private readonly Action<EndPoint> _disconnectedCallback;
        private readonly Action<int> _afterSentMessageCallback;
        private readonly EndPoint _endPoint;
        private readonly AsyncQueue<QueuedIpcMessage> _sendMessageQueue = new AsyncQueue<QueuedIpcMessage>();

        public EndPoint EndPoint => _endPoint;

        public TcpConnection(TcpClient tcpClient, Action<EndPoint, QueuedIpcMessage> messageRecievedCallback, Action<EndPoint> disconnectedCallback, Action<int> afterSentMessageCallback = null)
        {
            _tcpClient = tcpClient;
            _endPoint = _tcpClient.Client.RemoteEndPoint;
            _messageReceivedCallback = messageRecievedCallback;
            _disconnectedCallback = disconnectedCallback;
            _afterSentMessageCallback = afterSentMessageCallback;
            _pipe = new TcpPipe(tcpClient);
            RunMessageLoopsAsync().Forget();
        }

        public async Task RunMessageLoopsAsync()
        {
            Task<bool> readTask = ReadLoopAsync(_cancellationSource.Token).DetectDisconnectAsync();
            Task<bool> writeTask = WriteLoopAsync(_cancellationSource.Token).DetectDisconnectAsync();


            Task<bool> finishedTask = await Task.WhenAny(readTask, writeTask);

            if (finishedTask == readTask)
            {
                bool needToDisconnect = await readTask;
                if (needToDisconnect)
                {
                    _disconnectedCallback(_endPoint);
                }

                await writeTask;
            }
            else
            {
                bool needToDisconnect = await writeTask;
                if (needToDisconnect)
                {
                    _disconnectedCallback(_endPoint);
                }

                await readTask;
            }
        }

        public async Task ReadLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (QueuedIpcMessage message = await _pipe.ReadMessageAsync(cancellationToken))
                {
                    _messageReceivedCallback(_endPoint, message);
                }
            }
        }

        public async Task WriteLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (QueuedIpcMessage message = await _sendMessageQueue.DequeueAsync(cancellationToken).ConfigureAwait(false))
                {
                    await _pipe.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
                    _afterSentMessageCallback?.Invoke(message.messageId);
                }
            }
        }

        public void Send(QueuedIpcMessage message)
        {
            _sendMessageQueue.Enqueue(message);
        }

        public void Dispose()
        {
            _cancellationSource.Cancel();
            _tcpClient.Dispose();
        }
    }
}