using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace ParrelSync
{
    /// <summary>
    /// To add value caching for <see cref="EditorPrefs"/> functions
    /// </summary>
    public class BoolPreference
    {
        public string key { get; private set; }
        public bool defaultValue { get; private set; }
        public BoolPreference(string key, bool defaultValue)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        private bool? valueCache = null;

        public bool Value
        {
            get
            {
                if (valueCache == null)
                    valueCache = EditorPrefs.GetBool(key, defaultValue);

                return (bool)valueCache;
            }
            set
            {
                if (valueCache == value)
                    return;

                EditorPrefs.SetBool(key, value);
                valueCache = value;
                Debug.Log("Editor preference updated. key: " + key + ", value: " + value);
            }
        }

        public void ClearValue()
        {
            EditorPrefs.DeleteKey(key);
            valueCache = null;
        }
    }
    
    
    /// <summary>
    /// To add value caching for <see cref="EditorPrefs"/> functions
    /// </summary>
    public class ListOfStringsPreference
    {
        private static string serializationToken = "|||";
        public string Key { get; private set; }
        public ListOfStringsPreference(string key)
        {
            Key = key;
        }
        public List<string> GetStoredValue()
        {
            return this.Deserialize(EditorPrefs.GetString(Key));
        }
        public void SetStoredValue(List<string> strings)
        {
            EditorPrefs.SetString(Key, this.Serialize(strings));
        }
        public void ClearStoredValue()
        {
            EditorPrefs.DeleteKey(Key);
        }
        public string Serialize(List<string> data)
        {
            string result = string.Empty;
            foreach (var item in data)
            {
                if (item.Contains(serializationToken))
                {
                    Debug.LogError("Unable to serialize this value ["+item+"], it contains the serialization token ["+serializationToken+"]");
                    continue;
                }

                result += item + serializationToken;
            }
            return result;
        }
        public List<string> Deserialize(string data)
        {
            return data.Split(serializationToken).ToList();
        }
    }
    public class Preferences : EditorWindow
    {
        internal static void InitWindow()
        {
            Preferences window = (Preferences)EditorWindow.GetWindow(typeof(Preferences));
            window.titleContent = new GUIContent(ClonesManager.ProjectName + " " +
                ParrelSyncLocalization.Text("Preferences", "偏好设置"));
            window.minSize = new Vector2(550, 300);
            window.Show();
        }

        /// <summary>
        /// Disable asset saving in clone editors?
        /// </summary>
        public static BoolPreference AssetModPref = new BoolPreference("ParrelSync_DisableClonesAssetSaving", true);

        /// <summary>
        /// In addition of checking the existence of UnityLockFile, 
        /// also check is the is the UnityLockFile being opened.
        /// </summary>
        public static BoolPreference AlsoCheckUnityLockFileStaPref = new BoolPreference("ParrelSync_CheckUnityLockFileOpenStatus", true);

        /// <summary>
        /// A list of folders to create sybolic links for,
        /// useful for data that lives outside of the assets folder
        /// eg. Wwise project data
        /// </summary>
        public static ListOfStringsPreference OptionalSymbolicLinkFolders = new ListOfStringsPreference("ParrelSync_OptionalSymbolicLinkFolders");
        
        private void OnGUI()
        {
            if (GUILayout.Button(ParrelSyncLocalization.Text(
                "Restore asset saving warning", "恢复资产保存警告")))
            {
                EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine,
                    ParrelSyncAssetModificationProcessor.WarningOptOutKey, false);
            }
            EditorGUILayout.HelpBox(ParrelSyncLocalization.Text(
                "Language and dialog opt-out settings are shared by editors on this machine. Hiding the warning does not allow asset saving or suppress Console logs.",
                "语言和“不再提示”设置由本机编辑器共享。隐藏警告不会允许保存，也不会关闭控制台日志。"),
                MessageType.Info);

            if (ClonesManager.IsClone())
            {
                EditorGUILayout.HelpBox(
                        ParrelSyncLocalization.Text(
                            "This is a clone project. Please use the original project editor to change preferences.",
                            "当前项目是克隆，请在主项目编辑器中修改以下偏好设置。"),
                        MessageType.Info);
                return;
            }

            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label(ParrelSyncLocalization.Text("Preferences", "偏好设置"));
            GUILayout.BeginVertical("GroupBox");

            AssetModPref.Value = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    ParrelSyncLocalization.Text(
                        "(recommended) Disable asset saving in clone editors- require re-open clone editors",
                        "禁止克隆编辑器保存资产（推荐；修改后需重新打开克隆）"),
                    ParrelSyncLocalization.Text(
                        "Disable asset saving in clone editors so all assets can only be modified from the original project editor",
                        "阻止克隆编辑器保存共享资产，确保只从主项目编辑器修改资产。")
                ),
                AssetModPref.Value);

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                AlsoCheckUnityLockFileStaPref.Value = EditorGUILayout.ToggleLeft(
                    new GUIContent(
                        ParrelSyncLocalization.Text(
                            "Also check UnityLockFile lock status while checking clone projects running status",
                            "判断克隆是否运行时，同时检查 UnityLockFile 的锁定状态"),
                        ParrelSyncLocalization.Text(
                            "Disable this can slightly increase Clones Manager window performance, but will lead to in-correct clone project running status" +
                            "(the Clones Manager window show the clone project is still running even it's not) if the clone editor crashed",
                            "关闭可略微提高窗口效率，但克隆编辑器崩溃后可能仍被误判为运行中。")
                    ),
                    AlsoCheckUnityLockFileStaPref.Value);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("GroupBox");
            GUILayout.Label(ParrelSyncLocalization.Text("Optional Folders to Symbolically Link", "额外共享目录（目录链接）"));
            GUILayout.Space(5);

            // cache the current value
            List<string> optionalFolderPaths = OptionalSymbolicLinkFolders.GetStoredValue();
            bool optionalFolderPathsAreDirty = false;
            
            // append a new row if full
            if (optionalFolderPaths.Last() != "")
            {
                optionalFolderPaths.Add("");
            }

            var projectPath = ClonesManager.GetCurrentProjectPath();
            for (int i = 0; i < optionalFolderPaths.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(optionalFolderPaths[i], EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (GUILayout.Button(ParrelSyncLocalization.Text("Select Folder", "选择目录"), GUILayout.Width(100)))
                {
                    var result = EditorUtility.OpenFolderPanel(ParrelSyncLocalization.Text(
                        "Select Folder to Symbolically Link...", "选择要共享的项目内目录"), "", "");
                    if (result.Contains(projectPath))
                    {
                        optionalFolderPaths[i] = result.Replace(projectPath,"");
                        optionalFolderPathsAreDirty = true;
                    }
                    else if( result != "")
                    {
                        Debug.LogWarning(ParrelSyncLocalization.Text(
                            "Symbolic Link folder must be within the project directory", "共享目录必须位于项目目录内。"));
                    }
                }
                if (GUILayout.Button(ParrelSyncLocalization.Text("Clear", "清除"), GUILayout.Width(100)))
                {
                    optionalFolderPaths[i] = "";
                    optionalFolderPathsAreDirty = true;
                }
                GUILayout.EndHorizontal();
            }

            // only set the preference if the value is marked dirty
            if (optionalFolderPathsAreDirty)
            {
                optionalFolderPaths.RemoveAll(str=> str == "");
                OptionalSymbolicLinkFolders.SetStoredValue(optionalFolderPaths);
            }
            
            GUILayout.EndVertical();
            
            if (GUILayout.Button(ParrelSyncLocalization.Text("Reset to default", "恢复默认设置")))
            {
                AssetModPref.ClearValue();
                AlsoCheckUnityLockFileStaPref.ClearValue();
                OptionalSymbolicLinkFolders.ClearStoredValue();
                EditorUtility.SetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine,
                    ParrelSyncAssetModificationProcessor.WarningOptOutKey, false);
                Debug.Log("Editor preferences cleared");
            }
            GUILayout.EndVertical();
        }
    }
}
