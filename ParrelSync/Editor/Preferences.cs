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
    public class StringArrayPreference
    {
        public string key { get; private set; }
        public string[] defaultValue { get; private set; }
        public StringArrayPreference(string key, string[] defaultValue)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        private string[]? valueCache = null;

        public string[] Value
        {
            get
            {
                if (valueCache == null)
                    valueCache = Deserialize(EditorPrefs.GetString(key));
                
                return valueCache;
            }
            set
            {
                if (valueCache == value)
                    return;
                
				EditorPrefs.SetString(key, this.Serialize(value));
                valueCache = value;
                Debug.Log("Editor preference updated. key: " + key + ", value: " + this.Serialize(value));
            }
        }

        public void ClearValue()
        {
            EditorPrefs.DeleteKey(key);
            valueCache = null;
        }

        public string Serialize(string[] data)
        {
            string result = string.Empty;
            foreach (var item in data)
            {
                if (item.Contains("|"))
                {
                    // throw error
                }

                result += item + "|||";
            }

            return result;
        }
        public string[] Deserialize(string data)
        {
            return data.Split("|||");
        }
    }
    public class Preferences : EditorWindow
    {
        [MenuItem("ParrelSync/Preferences", priority = 1)]
        private static void InitWindow()
        {
            Preferences window = (Preferences)EditorWindow.GetWindow(typeof(Preferences));
            window.titleContent = new GUIContent(ClonesManager.ProjectName + " Preferences");
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
        /// In addition of checking the existence of UnityLockFile, 
        /// also check is the is the UnityLockFile being opened.
        /// </summary>
        public static StringArrayPreference OptionalSymbolicLinkFolders = new StringArrayPreference("ParrelSync_OptionalSymbolicLinkFolders", new string[]{""});

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

            OptionalSymbolicLinkFolders.Value = new string[]
            {
                EditorGUILayout.TextField(
                    new GUIContent(
                        "Folder 01",
                        "tolltip"),
                    OptionalSymbolicLinkFolders.Value[0])
            };
            
            GUILayout.EndVertical();
            if (GUILayout.Button("Reset to default"))
            {
                AssetModPref.ClearValue();
                AlsoCheckUnityLockFileStaPref.ClearValue();
                Debug.Log("Editor preferences cleared");
            }
            GUILayout.EndVertical();
        }
    }
}
