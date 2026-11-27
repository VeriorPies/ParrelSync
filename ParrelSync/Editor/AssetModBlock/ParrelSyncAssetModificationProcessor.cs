using UnityEditor;
using UnityEngine;
namespace ParrelSync
{
    /// <summary>
    /// For preventing assets being modified from the clone instance.
    /// </summary>
    public class ParrelSyncAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (ClonesManager.IsClone() && Preferences.AssetModPref.Value)
            {
                if (paths != null && paths.Length > 0 && !EditorQuit.IsQuiting)
                {
                    AssetModBlockedDialog.ShowIfEnabled();
                    foreach (var path in paths)
                    {
                        Debug.Log("Attempting to save " + path + " are blocked.");
                    }
                }
                return new string[0] { };
            }
            return paths;
        }
    }
}