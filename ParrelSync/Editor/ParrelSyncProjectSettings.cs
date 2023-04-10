using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParrelSync
{
    // Class name and File name MUST be identical
    public class ParrelSyncProjectSettings : ScriptableObject
    {
        public const string ParrelSyncSettingsPath = "Assets/Editor/ParrelSyncProjectSettings.asset";
        [SerializeField] private List<string> m_OptionalSymbolicLinkFolders;

        private static ParrelSyncProjectSettings GetOrCreateSettings()
        {
            ParrelSyncProjectSettings projectSettings;
            if(File.Exists(ParrelSyncSettingsPath))
            {
                projectSettings = AssetDatabase.LoadAssetAtPath<ParrelSyncProjectSettings>(ParrelSyncSettingsPath);
                
                if (projectSettings == null) 
                    Debug.LogError("File Exists, but failed to load: " + ParrelSyncSettingsPath);
                
                return projectSettings;
            }
            
            projectSettings = CreateInstance<ParrelSyncProjectSettings>();
            projectSettings.m_OptionalSymbolicLinkFolders = new List<string>();
            AssetDatabase.CreateAsset(projectSettings, ParrelSyncSettingsPath);
            AssetDatabase.SaveAssets();
            return projectSettings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    public class ParrelSyncSettingsProvider : SettingsProvider
    {
        private SerializedObject _parrelSyncProjectSettings;

        private class Styles
        {
            public static readonly GUIContent SymlinkSectionHeading = new GUIContent("Optional Folders to Symbolically Link");
        }

        private ParrelSyncSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope)
        {
        }

        private static bool IsSettingsAvailable()
        {
            return File.Exists(ParrelSyncProjectSettings.ParrelSyncSettingsPath);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the ParrelSyncSettings element in the Settings window.
            _parrelSyncProjectSettings = ParrelSyncProjectSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            var property = _parrelSyncProjectSettings.FindProperty("m_OptionalSymbolicLinkFolders");
            if( property is null || !property.isArray || property.arrayElementType != "string") 
                return;

            var optionalFolderPaths = new List<string>(property.arraySize);
            for (var i = 0; i < property.arraySize; ++i)
            {
                optionalFolderPaths.Add(property.GetArrayElementAtIndex(i).stringValue);
            }
            optionalFolderPaths.Add("");
            
            GUILayout.BeginVertical("GroupBox");
            GUILayout.Label(Styles.SymlinkSectionHeading);
            GUILayout.Space(5);
            var projectPath = ClonesManager.GetCurrentProjectPath();
            var optionalFolderPathsIsDirty = false;
            for (var i = 0; i < optionalFolderPaths.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(optionalFolderPaths[i], EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    var result = EditorUtility.OpenFolderPanel("Select Folder to Symbolically Link...", "", "");
                    if (result.Contains(projectPath))
                    {
                        optionalFolderPaths[i] = result.Replace(projectPath,"");
                        optionalFolderPathsIsDirty = true;
                    }
                    else if( result != "")
                    {
                        Debug.LogWarning("Symbolic Link folder must be within the project directory");
                    }
                }
                if (GUILayout.Button("Clear", GUILayout.Width(60)))
                {
                    optionalFolderPaths[i] = "";
                    optionalFolderPathsIsDirty = true;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            if (!optionalFolderPathsIsDirty) 
                return;
            
            optionalFolderPaths.RemoveAll(str=> str == "");
            property.arraySize = optionalFolderPaths.Count;
            for (var i = 0; i < property.arraySize; ++i)
            {
                property.GetArrayElementAtIndex(i).stringValue = optionalFolderPaths[i];
            }
            _parrelSyncProjectSettings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateParrelSyncSettingsProvider()
        {
            if (!IsSettingsAvailable()) return null;
            var provider = new ParrelSyncSettingsProvider("Project/ParrelSync", SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            return provider;
        }
    }
}