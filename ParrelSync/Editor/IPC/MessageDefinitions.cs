
namespace ParrelSync.Ipc
{
    public struct PlayModeChangedMessage : IIpcMessage
    {
        public bool InPlayMode;
    }

    public struct AssemblyReloadMessage : IIpcMessage
    {
    }

    public struct SourceProjectSavedMessage : IIpcMessage
    {
    }

    public struct HeartbeatMessage : IIpcMessage
    {
    }
}