using UnityEditor;
using UnityEngine;
namespace ParrelSync
{
    /// <summary>
    /// For preventing assets being modified from the clone instance.
    /// </summary>
    public class ParrelSyncAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        /// <summary>
        /// EditorPrefs key backing the "Do not show again" choice of the blocking dialog.
        /// </summary>
        public const string WarningOptOutKey = "ParrelSyncAssetModificationWarningOptOut";

        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (ClonesManager.IsClone() && Preferences.AssetModPref.Value)
            {
                if (paths != null && paths.Length > 0 && !EditorQuit.IsQuiting)
                {
                    EditorUtility.DisplayDialog(
                        ClonesManager.ProjectName + ": Asset modifications saving detected and blocked",
                        "Asset modifications saving are blocked in the clone instance. \n\n" +
                        "This is a clone of the original project. \n" +
                        "Making changes to asset files via the clone editor is not recommended. \n" +
                        "Please use the original editor window if you want to make changes to the project files.\n\n" +
                        "Choosing \"Do not show again\" only hides this dialog. Saving stays blocked and the console warning is still logged.",
                        "OK",
                        DialogOptOutDecisionType.ForThisMachine,
                        WarningOptOutKey
                    );
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