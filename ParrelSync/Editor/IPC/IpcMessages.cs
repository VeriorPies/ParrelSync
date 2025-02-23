using System;

namespace ParrelSync.Ipc
{
    internal interface IIpcMessage { }

    internal delegate void IpcMessageDelegate(IpcConnection conn, QueuedIpcMessage reader);

    internal static class IpcMessageId<T> where T : struct, IIpcMessage
    {
        public static readonly int Id = CalculateId();

        static int CalculateId() => typeof(T).FullName.GetStableHashCode();
    }

    internal static class IpcMessages
    {
        public static IpcMessageDelegate WrapHandler<C, T>(Action<C, T> handler)
            where T : struct, IIpcMessage
            where C : IpcConnection
        {
            return (conn, queuedMessage) =>
            {
                var serialized = new QueuedIpcMessage(queuedMessage.messageId, queuedMessage.messageSize, queuedMessage.payload);
                T message = IpcMessages<T>.deserializationMethod(serialized);
                handler((C)conn, message);
            };
        }
    }

    internal static class IpcMessages<T> where T : struct, IIpcMessage
    {
        internal static Func<QueuedIpcMessage, T> deserializationMethod;
        internal static Func<T, QueuedIpcMessage> serializationMethod;
    }
}