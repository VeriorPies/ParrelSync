using ParrelSync.Ipc;
using UnityEditor;

namespace ParrelSync
{    
    internal static class EditorSyncManager
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            MessageSerializers.RegisterSerializers();

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnQuiting;

            if (!ClonesManager.IsClone())
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChangedInSource;
                IpcServer.ClientConnected += OnClientConnectedToServer;
                IpcServer.Start();
            }
            else
            {
                IpcClient.RegisterHandler<PlayModeChangedMessage>(OnPlayModeChangedFromSource);
                IpcClient.RegisterHandler<AssemblyReloadMessage>(OnAssemblyReloadFromSource);
                IpcClient.Start();
            }
        }

        private static void OnAssemblyReloadFromSource(AssemblyReloadMessage message)
        {
            AssetDatabase.Refresh();
        }

        private static void OnBeforeAssemblyReload()
        {
            if (!ClonesManager.IsClone())
            {
                IpcServer.SendToAll(new AssemblyReloadMessage());

                int clientCount = IpcServer.ClientCount;

                // onlu wait until all assembly reload messages are sent if there are connected clones
                if (clientCount > 0)
                {
                    AssemblyReloadWaitHandler.SetCountAndWait(clientCount);
                }
            }
            
            Teardown();
        }

        private static void OnQuiting()
        {
            Teardown();
        }

        private static void Teardown()
        {
            if (!ClonesManager.IsClone())
            {
                IpcServer.Stop();
            }
            else
            {
                IpcClient.Stop();
            }
        }

        private static void OnClientConnectedToServer(IpcConnectionToClient client)
        {
            // sync the play mode state when a clone connects
            PlayModeChangedMessage message = new PlayModeChangedMessage()
            {
                InPlayMode = EditorApplication.isPlayingOrWillChangePlaymode
            };
            client.Send(message);
        }

        private static void OnPlayModeStateChangedInSource(PlayModeStateChange change)
        {
            // send the play mode state on change to connected clones

            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                IpcServer.SendToAll(new PlayModeChangedMessage() { InPlayMode = false });
            }
            else if (change == PlayModeStateChange.ExitingEditMode)
            {
                IpcServer.SendToAll(new PlayModeChangedMessage() { InPlayMode = true });
            }
        }

        private static void OnPlayModeChangedFromSource(PlayModeChangedMessage message)
        {
            if (message.InPlayMode)
            {
                EditorApplication.EnterPlaymode();
            }
            else
            {
                EditorApplication.ExitPlaymode();
            }
        }
    }
}