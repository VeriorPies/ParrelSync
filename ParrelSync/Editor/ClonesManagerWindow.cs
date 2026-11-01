using UnityEngine;
using UnityEditor;
using System.IO;

namespace ParrelSync
{
    /// <summary>
    ///Clones manager Unity editor window
    /// </summary>
	public class ClonesManagerWindow : EditorWindow
    {
        /// <summary>
        /// Returns true if project clone exists.
        /// </summary>
        public bool isCloneCreated
        {
            get { return ClonesManager.GetCloneProjectsPath().Count >= 1; }
        }

        internal static void InitWindow()
        {
            ClonesManagerWindow window = (ClonesManagerWindow)EditorWindow.GetWindow(typeof(ClonesManagerWindow));
            window.titleContent = new GUIContent(ParrelSyncLocalization.Text("Clones Manager", "克隆管理器"));
            window.Show();
        }

        /// <summary>
        /// For storing the scroll position of clones list
        /// </summary>
        Vector2 clonesScrollPos;

        private void OnGUI()
        {
            /// If it is a clone project...
            if (ClonesManager.IsClone())
            {
                //Find out the original project name and show the help box
                string originalProjectPath = ClonesManager.GetOriginalProjectPath();
                if (originalProjectPath == string.Empty)
                {
                    /// If original project cannot be found, display warning message.
                    EditorGUILayout.HelpBox(
                        ParrelSyncLocalization.Text(
                            "This project is a clone, but the link to the original seems lost.\nYou have to manually open the original and create a new clone instead of this one.\n",
                            "当前项目是克隆，但无法找到主项目。\n请手动打开主项目并重新创建克隆。\n"),
                        MessageType.Warning);
                }
                else
                {
                    /// If original project is present, display some usage info.
                    EditorGUILayout.HelpBox(
                        string.Format(ParrelSyncLocalization.Text(
                            "This project is a clone of '{0}'.\nTo modify project files or manage clones, open the original project through Unity Hub.",
                            "当前项目是“{0}”的克隆。\n如需修改项目文件或管理克隆，请通过 Hub 打开主项目。"),
                            Path.GetFileName(originalProjectPath)),
                        MessageType.Info);
                }

                //Clone project custom argument.
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(ParrelSyncLocalization.Text("Arguments", "启动参数"), GUILayout.Width(70));
                if (GUILayout.Button("?", GUILayout.Width(20)))
                {
                    Application.OpenURL(ExternalLinks.CustomArgumentHelpLink);
                }
                GUILayout.EndHorizontal();

                string argumentFilePath = Path.Combine(ClonesManager.GetCurrentProjectPath(), ClonesManager.ArgumentFileName);
                //Need to be careful with file reading / writing since it will effect the deletion of
                //  the clone project(The directory won't be fully deleted if there's still file inside being read or write).
                //The argument file will be deleted first at the beginning of the project deletion process
                //to prevent any further being read and write.
                //Will need to take some extra cautious if want to change the design of how file editing is handled.
                if (File.Exists(argumentFilePath))
                {
                    string argument = File.ReadAllText(argumentFilePath, System.Text.Encoding.UTF8);
                    string argumentTextAreaInput = EditorGUILayout.TextArea(argument,
                        GUILayout.Height(50),
                        GUILayout.MaxWidth(300)
                    );
                    File.WriteAllText(argumentFilePath, argumentTextAreaInput, System.Text.Encoding.UTF8);
                }
                else
                {
                    EditorGUILayout.LabelField(ParrelSyncLocalization.Text("No argument file found.", "未找到参数文件。"));
                }
            }
            else// If it is an original project...
            {
                if (isCloneCreated)
                {
                    GUILayout.BeginVertical("HelpBox");
                    GUILayout.Label(ParrelSyncLocalization.Text("Clones of this Project", "当前项目的克隆"));

                    //List all clones
                    clonesScrollPos =
                         EditorGUILayout.BeginScrollView(clonesScrollPos);
                    var cloneProjectsPath = ClonesManager.GetCloneProjectsPath();
                    for (int i = 0; i < cloneProjectsPath.Count; i++)
                    {

                        GUILayout.BeginVertical("GroupBox");
                        string cloneProjectPath = cloneProjectsPath[i];

                        bool isOpenInAnotherInstance = ClonesManager.IsCloneProjectRunning(cloneProjectPath);

                        if (isOpenInAnotherInstance == true)
                            EditorGUILayout.LabelField(ParrelSyncLocalization.Text("Clone ", "克隆 ") + i +
                                ParrelSyncLocalization.Text(" (Running)", "（运行中）"), EditorStyles.boldLabel);
                        else
                            EditorGUILayout.LabelField(ParrelSyncLocalization.Text("Clone ", "克隆 ") + i);


                        GUILayout.BeginHorizontal();
                        EditorGUILayout.TextField(ParrelSyncLocalization.Text("Clone project path", "克隆项目路径"), cloneProjectPath, EditorStyles.textField);
                        if (GUILayout.Button(ParrelSyncLocalization.Text("View Folder", "打开目录"), GUILayout.Width(80)))
                        {
                            ClonesManager.OpenProjectInFileExplorer(cloneProjectPath);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(ParrelSyncLocalization.Text("Arguments", "启动参数"), GUILayout.Width(70));
                        if (GUILayout.Button("?", GUILayout.Width(20)))
                        {
                            Application.OpenURL(ExternalLinks.CustomArgumentHelpLink);
                        }
                        GUILayout.EndHorizontal();

                        string argumentFilePath = Path.Combine(cloneProjectPath, ClonesManager.ArgumentFileName);
                        //Need to be careful with file reading/writing since it will effect the deletion of
                        //the clone project(The directory won't be fully deleted if there's still file inside being read or write).
                        //The argument file will be deleted first at the beginning of the project deletion process 
                        //to prevent any further being read and write.
                        //Will need to take some extra cautious if want to change the design of how file editing is handled.
                        if (File.Exists(argumentFilePath))
                        {
                            string argument = File.ReadAllText(argumentFilePath, System.Text.Encoding.UTF8);
                            string argumentTextAreaInput = EditorGUILayout.TextArea(argument,
                                GUILayout.Height(50),
                                GUILayout.MaxWidth(300)
                            );
                            File.WriteAllText(argumentFilePath, argumentTextAreaInput, System.Text.Encoding.UTF8);
                        }
                        else
                        {
                            EditorGUILayout.LabelField(ParrelSyncLocalization.Text("No argument file found.", "未找到参数文件。"));
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();


                        EditorGUI.BeginDisabledGroup(isOpenInAnotherInstance);

                        if (GUILayout.Button(ParrelSyncLocalization.Text("Open in New Editor", "在新编辑器中打开")))
                        {
                            ClonesManager.OpenProject(cloneProjectPath);
                        }

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(ParrelSyncLocalization.Text("Delete", "删除")))
                        {
                            bool delete = EditorUtility.DisplayDialog(
                                ParrelSyncLocalization.Text("Delete the clone?", "删除克隆？"),
                                string.Format(ParrelSyncLocalization.Text(
                                    "Are you sure you want to delete the clone project '{0}'?",
                                    "确定要删除克隆项目“{0}”吗？"), Path.GetFileName(cloneProjectPath)),
                                ParrelSyncLocalization.Text("Delete", "删除"),
                                ParrelSyncLocalization.Text("Cancel", "取消"));
                            if (delete)
                            {
                                ClonesManager.DeleteClone(cloneProjectPath);
                            }
                        }
                        GUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                        GUILayout.EndVertical();

                    }
                    EditorGUILayout.EndScrollView();

                    if (GUILayout.Button(ParrelSyncLocalization.Text("Add new clone", "添加克隆")))
                    {
                        ClonesManager.CreateCloneFromCurrent();
                    }

                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    /// If no clone created yet, we must create it.
                    EditorGUILayout.HelpBox(ParrelSyncLocalization.Text(
                        "No project clones found. Create a new one!", "尚未创建克隆，请创建一个新克隆。"), MessageType.Info);
                    if (GUILayout.Button(ParrelSyncLocalization.Text("Create new clone", "创建克隆")))
                    {
                        ClonesManager.CreateCloneFromCurrent();
                    }
                }
            }
        }
    }
}
