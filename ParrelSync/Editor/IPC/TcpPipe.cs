using ParrelSync.Buffers;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using ParrelSync.Serialization;

namespace ParrelSync.Ipc
{
    internal class TcpPipe
    {
        private readonly NetworkStream _stream;

        private byte[] _buffer = new byte[1024];
        private const int HEADER_SIZE = sizeof(int) * 2;

        public TcpPipe(TcpClient client)
        {
            _stream = client.GetStream();
        }

        public async Task<QueuedIpcMessage> ReadMessageAsync(CancellationToken cancellationToken)
        {
            int readPosition = 0;

            int bytesRead = 0;

            do
            {
                bytesRead += await _stream.ReadAsync(_buffer, bytesRead, HEADER_SIZE - bytesRead).ConfigureAwait(false);
            }
            while (bytesRead < HEADER_SIZE);

            SerializationHelpers.TryReadInt(_buffer, bytesRead, ref readPosition, out int messageId);
            SerializationHelpers.TryReadInt(_buffer, bytesRead, ref readPosition, out int payloadSize);

            // we have the header values so just reset the buffer to the start
            readPosition = 0;
            bytesRead = 0;

            if (_buffer.Length < payloadSize)
            {
                ResizeBuffer(payloadSize, bytesRead);
            }

            if (payloadSize > 0)
            {
                do
                {
                    bytesRead += await _stream.ReadAsync(_buffer, bytesRead, payloadSize).ConfigureAwait(false);
                }
                while (bytesRead < payloadSize);
            }

            byte[] payload = ArrayPoolHelper.Rent(payloadSize);
            Array.Copy(_buffer, payload, bytesRead);

            return new QueuedIpcMessage(messageId, payloadSize, payload);
        }

        public async Task SendMessageAsync(QueuedIpcMessage message, CancellationToken cancellationToken)
        {
            byte[] headerBuffer = ArrayPoolHelper.Rent(HEADER_SIZE);
            try
            {
                int position = 0;

                SerializationHelpers.WriteInt(message.messageId, headerBuffer, ref position);
                SerializationHelpers.WriteInt(message.messageSize, headerBuffer, ref position);

                await _stream.WriteAsync(headerBuffer, 0, HEADER_SIZE).ConfigureAwait(false);
                await _stream.WriteAsync(message.payload, 0, message.messageSize).ConfigureAwait(false);
            }
            finally
            {
                ArrayPoolHelper.Return(headerBuffer);
            }
        }

        private void ResizeBuffer(int requiredSize, int bytesRead)
        {
            int desiredSize = _buffer.Length;

            while (desiredSize < requiredSize)
            {
                desiredSize *= 2;
            }

            byte[] newBuffer = new byte[desiredSize];

            Array.Copy(_buffer, newBuffer, bytesRead);
            _buffer = newBuffer;
        }
    }
}