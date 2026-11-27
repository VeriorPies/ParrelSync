using UnityEditor;
using UnityEngine;
namespace ParrelSync
{
    /// <summary>
    /// Modal dialog shown when asset saving is blocked in a clone instance.
    /// Includes a "Don't show this message again" checkbox.
    /// </summary>
    public class AssetModBlockedDialog : EditorWindow
    {
        private const string Message =
            "Asset modifications saving are blocked in the clone instance. \n\n" +
            "This is a clone of the original project. \n" +
            "Making changes to asset files via the clone editor is not recommended. \n" +
            "Please use the original editor window if you want to make changes to the project files.";

        private bool dontShowAgain = false;

        /// <summary>
        /// Shows the dialog unless the user has opted out of it.
        /// </summary>
        public static void ShowIfEnabled()
        {
            if (!Preferences.ShowAssetModBlockedDialogPref.Value)
                return;

            AssetModBlockedDialog window = CreateInstance<AssetModBlockedDialog>();
            window.titleContent = new GUIContent(ClonesManager.ProjectName + ": Asset modifications saving detected and blocked");
            Vector2 size = new Vector2(480, 170);
            window.minSize = size;
            window.maxSize = size;
#if UNITY_2020_1_OR_NEWER
            Rect main = EditorGUIUtility.GetMainWindowPosition();
#else
            Rect main = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
#endif
            window.position = new Rect(
                main.x + (main.width - size.x) * 0.5f,
                main.y + (main.height - size.y) * 0.5f,
                size.x, size.y);
            window.ShowModal();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(Message, EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            dontShowAgain = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "Don't show this message again",
                    "Can be re-enabled from ParrelSync > Preferences in the original project"),
                dontShowAgain);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK", GUILayout.Width(80)))
            {
                if (dontShowAgain)
                    Preferences.ShowAssetModBlockedDialogPref.Value = false;
                Close();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
    }
}
