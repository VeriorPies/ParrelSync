using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ParrelSync
{
    /// <summary>
    /// For adding value caching for <see cref="EditorPrefs"/> functions
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

    public class PreferencesWindow : EditorWindow
    {
        [MenuItem("ParrelSync/Preferences", priority = 1)]
        private static void InitWindow()
        {
            PreferencesWindow window = (PreferencesWindow)EditorWindow.GetWindow(typeof(PreferencesWindow));
            window.titleContent = new GUIContent(ClonesManager.ProjectName + " Preferences");
            window.Show();
        }

        public BoolPreference AssetModPref = new BoolPreference("ParrelSync_DisableClonesAssetSaving", true);

        public BoolPreference ClonProOpenStasPref = new BoolPreference("ParrelSync_ShownClonesOpenStatus", true);
        public BoolPreference UnityLockFileOPenStasPref = new BoolPreference("ParrelSync_CheckUnityLockFileOpenStatus", true);

        private void OnGUI()
        {
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
                UnityLockFileOPenStasPref.SetValue(
                   EditorGUILayout.ToggleLeft("Check is UnityLockFile opened",
                   UnityLockFileOPenStasPref.GetValue())
                );
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
            if (GUILayout.Button("Reset to default"))
            {
                AssetModPref.ClearValue();
                ClonProOpenStasPref.ClearValue();
                UnityLockFileOPenStasPref.ClearValue();
                Debug.Log("Editor preferences cleared");
            }
            GUILayout.EndVertical();
        }
    }
}