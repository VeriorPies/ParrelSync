using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ParrelSync
{
    public class Preference : EditorWindow
    {
        [MenuItem("ParrelSync/Preferences", priority = 1)]
        private static void InitWindow()
        {
            Preference window = (Preference)EditorWindow.GetWindow(typeof(Preference));
            window.titleContent = new GUIContent(ClonesManager.ProjectName + " Preferences");
            window.Show();
        }

        const string AssetModProtectionKey = "ParrelSync_AssetModProtect";
        bool EnableAssetModProtection { get { return EditorPrefs.GetBool(AssetModProtectionKey, true); } }
        private void OnGUI()
        {
            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Preferences");
            GUILayout.BeginVertical("GroupBox");
            bool IsAssetModProtectionEnabled = EnableAssetModProtection;
            IsAssetModProtectionEnabled = GUILayout.Toggle(IsAssetModProtectionEnabled,
                "Disable asset modification saving in clone editors.");
            if(IsAssetModProtectionEnabled != EnableAssetModProtection)
            {
                EditorPrefs.SetBool(AssetModProtectionKey, IsAssetModProtectionEnabled);
                Debug.Log("ParrelSync editor preference updated. EnableAssetModProtection: " 
                    + EnableAssetModProtection);
            }
            GUILayout.EndVertical();
            if (GUILayout.Button("Clear Preferences"))
            {
                EditorPrefs.DeleteKey(AssetModProtectionKey);
                Debug.Log("Removed editor preference: " + AssetModProtectionKey);
            }
            GUILayout.EndVertical();
        }
    }
}