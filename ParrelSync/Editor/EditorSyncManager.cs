using ParrelSync.Ipc;
using System;
using UnityEditor;
using UnityEditor.Callbacks;

namespace ParrelSync
{
    internal class AssetSaveListener : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (!ClonesManager.IsClone())
            {
                EditorSyncManager.SendSourceProjectSavedMessage();
            }
        }
    }

    internal static class EditorSyncManager
    {
        private static bool ignoreNextAssemblyReload = false;

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
                IpcClient.RegisterHandler<SourceProjectSavedMessage>(OnSourceProjectSavedFromSource);
                IpcClient.RegisterHandler<PlayModeChangedMessage>(OnPlayModeChangedFromSource);
                IpcClient.RegisterHandler<AssemblyReloadMessage>(OnAssemblyReloadFromSource);
                IpcClient.Connected += OnClientConnect;
                IpcClient.Start();
            }
        }

        private static void OnClientConnect()
        {
            AssetDatabase.Refresh();
        }

        internal static void SendSourceProjectSavedMessage()
        {
            IpcServer.SendToAll(new SourceProjectSavedMessage());
        }

        private static void OnSourceProjectSavedFromSource(SourceProjectSavedMessage saved)
        {
            AssetDatabase.Refresh();
        }

        private static void OnAssemblyReloadFromSource(AssemblyReloadMessage message)
        {
            AssetDatabase.Refresh();
        }

        private static void OnBeforeAssemblyReload()
        {
            if (!ClonesManager.IsClone() && !ignoreNextAssemblyReload)
            {
                int clientCount = IpcServer.ClientCount;

                // only wait until all assembly reload messages are sent if there are connected clones
                if (clientCount > 0)
                {
                    IpcServer.SendToAll(new AssemblyReloadMessage());
                    AssemblyReloadWaitHandler.SetCountAndWait(clientCount);
                }

                ignoreNextAssemblyReload = false;
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
            if (!Preferences.SyncPlayModePref.Value)
            {
                return;
            }

            // sync the play mode state when a clone connects
            PlayModeChangedMessage message = new PlayModeChangedMessage()
            {
                InPlayMode = EditorApplication.isPlayingOrWillChangePlaymode
            };
            client.Send(message);
        }

        private static void OnPlayModeStateChangedInSource(PlayModeStateChange change)
        {
            if (!Preferences.SyncPlayModePref.Value)
            {
                return;
            }

            // send the play mode state on change to connected clones
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                IpcServer.SendToAll(new PlayModeChangedMessage() { InPlayMode = false });
            }
            else if (change == PlayModeStateChange.ExitingEditMode)
            {
                if (!EditorSettings.enterPlayModeOptionsEnabled ||
                    !EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload))
                {
                    ignoreNextAssemblyReload = true;
                }

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