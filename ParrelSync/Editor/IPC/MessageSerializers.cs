using System;
using UnityEngine;
using ParrelSync.Buffers;
using ParrelSync.Serialization;

namespace ParrelSync.Ipc
{
    internal class MessageSerializers : MonoBehaviour
    {
        internal static void RegisterSerializers()
        {
            IpcMessages<PlayModeChangedMessage>.serializationMethod = SerializePlayModeChangedMessage;
            IpcMessages<PlayModeChangedMessage>.deserializationMethod = DeserializePlayModeChangedMessage;

            IpcMessages<AssemblyReloadMessage>.serializationMethod = SerializeAssemblyReloadMessage;
            IpcMessages<AssemblyReloadMessage>.deserializationMethod = DeserializeAssemblyReloadMessage;

            IpcMessages<HeartbeatMessage>.serializationMethod = SerializeHeartbeatMessage;
            IpcMessages<HeartbeatMessage>.deserializationMethod = DeserializeHeartbeatMessage;

            IpcMessages<SourceProjectSavedMessage>.serializationMethod = SerializeSourceProjectSavedMessage;
            IpcMessages<SourceProjectSavedMessage>.deserializationMethod = DeserializeSourceProjectSavedMessage;
        }

        private static AssemblyReloadMessage DeserializeAssemblyReloadMessage(QueuedIpcMessage message)
        {
            return new AssemblyReloadMessage() { };
        }

        private static QueuedIpcMessage SerializeAssemblyReloadMessage(AssemblyReloadMessage message)
        {
            int id = IpcMessageId<AssemblyReloadMessage>.Id;
            int messageSize = 0;

            return new QueuedIpcMessage(id, messageSize, Array.Empty<byte>());
        }

        private static HeartbeatMessage DeserializeHeartbeatMessage(QueuedIpcMessage message)
        {
            return new HeartbeatMessage() { };
        }

        private static QueuedIpcMessage SerializeHeartbeatMessage(HeartbeatMessage message)
        {
            int id = IpcMessageId<HeartbeatMessage>.Id;
            int messageSize = 0;

            return new QueuedIpcMessage(id, messageSize, Array.Empty<byte>());
        }

        private static QueuedIpcMessage SerializePlayModeChangedMessage(PlayModeChangedMessage message)
        {
            int id = IpcMessageId<PlayModeChangedMessage>.Id;
            int messageSize = sizeof(bool);

            byte[] payload = ArrayPoolHelper.Rent(messageSize);

            int position = 0;
            SerializationHelpers.WriteBool(message.InPlayMode, payload, ref position);

            return new QueuedIpcMessage(id, messageSize, payload);
        }

        private static PlayModeChangedMessage DeserializePlayModeChangedMessage(QueuedIpcMessage message)
        {
            int position = 0;
            SerializationHelpers.TryReadBool(message.payload, message.messageSize, ref position, out bool inPlayMode);
            return new PlayModeChangedMessage() { InPlayMode = inPlayMode };
        }

        private static SourceProjectSavedMessage DeserializeSourceProjectSavedMessage(QueuedIpcMessage message)
        {
            return new SourceProjectSavedMessage() { };
        }

        private static QueuedIpcMessage SerializeSourceProjectSavedMessage(SourceProjectSavedMessage message)
        {
            int id = IpcMessageId<SourceProjectSavedMessage>.Id;
            int messageSize = 0;

            return new QueuedIpcMessage(id, messageSize, Array.Empty<byte>());
        }
    }
}
