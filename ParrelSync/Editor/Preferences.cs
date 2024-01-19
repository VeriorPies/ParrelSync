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
            return data.Split(new string[] { serializationToken }, System.StringSplitOptions.None).ToList();
        }
    }
    public class Preferences : EditorWindow
    {
        [MenuItem("ParrelSync/Preferences", priority = 1)]
        private static void InitWindow()
        {
            Preferences window = (Preferences)EditorWindow.GetWindow(typeof(Preferences));
            window.titleContent = new GUIContent(ClonesManager.ProjectName + " Preferences");
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
            if (ClonesManager.IsClone())
            {
                EditorGUILayout.HelpBox(
                        "This is a clone project. Please use the original project editor to change preferences.",
                        MessageType.Info);
                return;
            }

            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Preferences");
            GUILayout.BeginVertical("GroupBox");

            AssetModPref.Value = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "(recommended) Disable asset saving in clone editors- require re-open clone editors",
                    "Disable asset saving in clone editors so all assets can only be modified from the original project editor"
                ),
                AssetModPref.Value);

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                AlsoCheckUnityLockFileStaPref.Value = EditorGUILayout.ToggleLeft(
                    new GUIContent(
                        "Also check UnityLockFile lock status while checking clone projects running status",
                        "Disable this can slightly increase Clones Manager window performance, but will lead to in-correct clone project running status" +
                        "(the Clones Manager window show the clone project is still running even it's not) if the clone editor crashed"
                    ),
                    AlsoCheckUnityLockFileStaPref.Value);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("GroupBox");
            GUILayout.Label("Optional Folders to Symbolically Link");
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
                if (GUILayout.Button("Select Folder", GUILayout.Width(100)))
                {
                    var result = EditorUtility.OpenFolderPanel("Select Folder to Symbolically Link...", "", "");
                    if (result.Contains(projectPath))
                    {
                        optionalFolderPaths[i] = result.Replace(projectPath,"");
                        optionalFolderPathsAreDirty = true;
                    }
                    else if( result != "")
                    {
                        Debug.LogWarning("Symbolic Link folder must be within the project directory");
                    }
                }
                if (GUILayout.Button("Clear", GUILayout.Width(100)))
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
            
            if (GUILayout.Button("Reset to default"))
            {
                AssetModPref.ClearValue();
                AlsoCheckUnityLockFileStaPref.ClearValue();
                OptionalSymbolicLinkFolders.ClearStoredValue();
                Debug.Log("Editor preferences cleared");
            }
            GUILayout.EndVertical();
        }
    }
}
