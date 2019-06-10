using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityProjectCloner;
using System.IO;

namespace UnityProjectCloner
{
    /// <summary>
    /// Provides Unity Editor window for ProjectCloner.
    /// </summary>
	public class ProjectClonerWindow : EditorWindow
	{
        /// <summary>
        /// True if currently open project is a clone.
        /// </summary>
        public bool isClone
        {
            get { return ProjectCloner.IsClone(); }
        }

        /// <summary>
        /// Returns true if project clone exists or currently open project is a clone itself.
        /// </summary>
        public bool isCloneCreated
        {
            get { return ProjectCloner.GetCloneProjectPath() != string.Empty; }
        }

        [MenuItem("Tools/Project Cloner")]
        private static void InitWindow()
        {
            ProjectClonerWindow window = (ProjectClonerWindow)EditorWindow.GetWindow(typeof(ProjectClonerWindow));
            window.titleContent = new GUIContent("Project Cloner");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Clone settings", EditorStyles.boldLabel);

            if (isClone)
            {
                /// If it is a clone project...
                string originalProjectPath = ProjectCloner.GetOriginalProjectPath();
                if (originalProjectPath == string.Empty)
                {
                    /// If original project cannot be found, display warning message.
                    string thisProjectName = ProjectCloner.GetCurrentProject().name;
                    string supposedOriginalProjectName = ProjectCloner.GetCurrentProject().name.Replace("_clone", "");
                    EditorGUILayout.HelpBox(
                        "This project is a clone, but the link to the original seems lost.\nYou have to manually open the original and create a new clone instead of this one.\nThe original project should have a name '" + supposedOriginalProjectName + "', if it was not changed.", 
                        MessageType.Warning);
                }
                else
                {
                    /// If original project is present, display some usage info.
                    EditorGUILayout.HelpBox(
                        "This project is a clone of the project '" + Path.GetFileName(originalProjectPath) + "'.\nIf you want to make changes the project files or manage clones, please open the original project through Unity Hub.", 
                        MessageType.Info);
                }
            }
            else
            {
                /// If it is an original project...
                if (isCloneCreated)
                {
                    /// If clone is created, we can either open it or delete it.
                    string cloneProjectPath = ProjectCloner.GetCloneProjectPath();
                    EditorGUILayout.TextField("Clone project path", cloneProjectPath, EditorStyles.textField);
                    if (GUILayout.Button("Open clone project"))
                    {
                        ProjectCloner.OpenProject(cloneProjectPath);
                    }

                    if (GUILayout.Button("Delete clone"))
                    {
                        bool delete = EditorUtility.DisplayDialog(
                            "Delete the clone?", 
                            "Are you sure you want to delete the clone project '" + ProjectCloner.GetCurrentProject().name + "_clone'? If so, you can always create a new clone from ProjectCloner window.", 
                            "Delete", 
                            "Cancel");
                        if (delete)
                        {
                            // TODO: implement proper project deletion
                            throw new System.NotImplementedException();
                            ProjectCloner.DeleteClone();
                        }
                    }
                }
                else
                {
                    /// If no clone created yet, we must create it.
                    EditorGUILayout.HelpBox("No project clones found. Create a new one!", MessageType.Info);
                    if (GUILayout.Button("Create new clone"))
                    {
                        ProjectCloner.CreateCloneFromCurrent();
                    }
                }
            }
        }
    }
}
