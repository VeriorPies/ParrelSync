using ParrelSync.Buffers;
using System;

namespace ParrelSync.Ipc
{
    internal readonly struct QueuedIpcMessage : IDisposable
    {
        public readonly int messageId;
        public readonly int messageSize;
        public readonly byte[] payload;

        public QueuedIpcMessage(int messageId, int messageSize, byte[] payload)
        {
            this.messageId = messageId;
            this.messageSize = messageSize;
            this.payload = payload;
        }

        public QueuedIpcMessage Clone()
        {
            byte[] newPayload = ArrayPoolHelper.Rent(messageSize);
            Array.Copy(payload, newPayload, messageSize);

            return new QueuedIpcMessage(messageId, messageSize, newPayload);
        }

        public void Dispose()
        {
            if (payload.Length > 0)
            {
                ArrayPoolHelper.Return(payload);
            }
        }
    }
}