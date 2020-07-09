using System.Collections;
using System.Collections.Generic;
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

        public bool GetValue()
        {
            if (valueCache == null)
                valueCache = EditorPrefs.GetBool(key, defaultValue);

            return (bool)valueCache;
        }

        public void SetValue(bool value)
        {
            if (valueCache == value)
                return;

            EditorPrefs.SetBool(key, value);
            valueCache = value;
            Debug.Log("Editor preference updated. key: " + key + ", value: " + value);
        }

        public void ClearValue()
        {
            EditorPrefs.DeleteKey(key);
            valueCache = null;
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
        /// Show Project clones open status?
        /// </summary>
        public static BoolPreference ClonProOpenStasPref = new BoolPreference("ParrelSync_ShownClonesOpenStatus", true);
        /// <summary>
        /// If show project clones open status, 
        /// in addition of checking the existence of UnityLockFile, 
        /// also check is the lock file opened by another program.
        /// </summary>
        public static BoolPreference UnityLockFileOpenStasPref = new BoolPreference("ParrelSync_CheckUnityLockFileOpenStatus", true);

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

            AssetModPref.SetValue(
                 EditorGUILayout.ToggleLeft("Disable asset saving in clone editors (recommended)",
                 AssetModPref.GetValue())
            );

            ClonProOpenStasPref.SetValue(
                EditorGUILayout.ToggleLeft("Show clone project open status in clone manager",
                ClonProOpenStasPref.GetValue())
            );

            if (ClonProOpenStasPref.GetValue())
            {
                EditorGUI.indentLevel++;
                UnityLockFileOpenStasPref.SetValue(
                   EditorGUILayout.ToggleLeft("Check is UnityLockFile opened",
                   UnityLockFileOpenStasPref.GetValue())
                );
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
            if (GUILayout.Button("Reset to default"))
            {
                AssetModPref.ClearValue();
                ClonProOpenStasPref.ClearValue();
                UnityLockFileOpenStasPref.ClearValue();
                Debug.Log("Editor preferences cleared");
            }
            GUILayout.EndVertical();
        }
    }
}