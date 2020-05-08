using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ParrelSync
{
    /// <summary>
    /// Provides Unity Editor window for ProjectCloner.
    /// </summary>
	public class ClonesManagerWindow : EditorWindow
    {
        /// <summary>
        /// True if currently open project is a clone.
        /// </summary>
        public bool isClone
        {
            get { return ClonesManager.IsClone(); }
        }

        /// <summary>
        /// Returns true if project clone exists.
        /// </summary>
        public bool isCloneCreated
        {
            get { return ClonesManager.GetCloneProjectsPath().Count >= 1; }
        }

        [MenuItem("ParrelSync/Clones Manager")]
        private static void InitWindow()
        {
            ClonesManagerWindow window = (ClonesManagerWindow)EditorWindow.GetWindow(typeof(ClonesManagerWindow));
            window.titleContent = new GUIContent("Clones Manager");
            window.Show();
        }

        private void OnGUI()
        {
            if (isClone)
            {
                /// If it is a clone project...

                //Clone project custom argument.
                string argument = "";
                string argumentFilePath = Path.Combine(ClonesManager.GetCurrentProjectPath(), ClonesManager.ArgumentFileName);
                if (File.Exists(argumentFilePath))
                {
                    argument = File.ReadAllText(argumentFilePath, System.Text.Encoding.UTF8);
                }
                string argumentTextField = EditorGUILayout.TextField("Arguments", argument, EditorStyles.textField);
                File.WriteAllText(argumentFilePath, argumentTextField, System.Text.Encoding.UTF8);

                //Find out the original project name and show help box
                string originalProjectPath = ClonesManager.GetOriginalProjectPath();
                if (originalProjectPath == string.Empty)
                {
                    /// If original project cannot be found, display warning message.
                    string thisProjectName = ClonesManager.GetCurrentProject().name;
                    string supposedOriginalProjectName = ClonesManager.GetCurrentProject().name.Replace("_clone", "");
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
                    GUILayout.BeginVertical("HelpBox");
                    GUILayout.Label("Clones of this Project");
                    /// If clone(s) is created, we can either open it or delete it.
                    var cloneProjectsPath = ClonesManager.GetCloneProjectsPath();
                    for (int i = 0; i < cloneProjectsPath.Count; i++)
                    {

                        GUILayout.BeginVertical("GroupBox");
                        string cloneProjectPath = cloneProjectsPath[i];
                        EditorGUILayout.LabelField("Clone " + i);

                        EditorGUILayout.TextField("Clone project path", cloneProjectPath, EditorStyles.textField);

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Arguments", GUILayout.Width(70));
                        if (GUILayout.Button("?", GUILayout.Width(30)))
                        {
                            //ToDo
                        }
                        GUILayout.EndHorizontal();

                        string argument = "";
                        string argumentFilePath = Path.Combine(cloneProjectPath, ClonesManager.ArgumentFileName);

                        if (File.Exists(argumentFilePath))
                        {
                            argument = File.ReadAllText(argumentFilePath, System.Text.Encoding.UTF8);
                        }
                        string argumentTextField = EditorGUILayout.TextArea(argument, GUILayout.Height(50), 
                            GUILayout.MaxWidth(300));
                        File.WriteAllText(argumentFilePath, argumentTextField, System.Text.Encoding.UTF8);
                                                                      

                        
                        
                        EditorGUILayout.Space(20);

                        if (GUILayout.Button("Open in New Editor"))
                        {
                            ClonesManager.OpenProject(cloneProjectPath);
                        }
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Delete"))
                        {
                            bool delete = EditorUtility.DisplayDialog(
                                "Delete the clone?",
                                "Are you sure you want to delete the clone project '" + ClonesManager.GetCurrentProject().name + "_clone'? If so, you can always create a new clone from ProjectCloner window.",
                                "Delete",
                                "Cancel");
                            if (delete)
                            {
                                ClonesManager.DeleteClone(cloneProjectPath);
                            }
                        }

                        //Offer a solution to user in-case they are stuck with deleting project
                        if (GUILayout.Button("?", GUILayout.Width(30)))
                        {
                            var openUrl = EditorUtility.DisplayDialog("Can't delete clone?",
                            "Sometime clone can't be deleted due to it's still being opened by another unity instance running in the background." +
                            "\nYou can read this answer from ServerFault on how to find and kill the process.", "Open Answer");
                            if (openUrl)
                            {
                                Application.OpenURL("https://serverfault.com/a/537762");
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();

                    }
                    GUILayout.EndVertical();
                    //Have difficulty with naming
                    //GUILayout.Label("Other", EditorStyles.boldLabel);
                    if (GUILayout.Button("Add new clone"))
                    {
                        ClonesManager.CreateCloneFromCurrent();
                    }
                }
                else
                {
                    /// If no clone created yet, we must create it.
                    EditorGUILayout.HelpBox("No project clones found. Create a new one!", MessageType.Info);
                    if (GUILayout.Button("Create new clone"))
                    {
                        ClonesManager.CreateCloneFromCurrent();
                    }
                }
            }
        }
    }
}
