using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityProjectCloner;

namespace UnityProjectCloner
{
	public class ProjectClonerWindow : EditorWindow
	{
		[MenuItem("NetworkingClone/Create Clone")]
		public static void initGUI()
		{
			string projectPath = ProjectCloner.FindCurrentProjectPath();
			Project currentProject = new Project(projectPath);
			Project nextProject = new Project(projectPath + "_clone");

			Debug.Log("Start project:\n" + currentProject);
			Debug.Log("Clone project:\n" + nextProject);


			ProjectCloner.CreateProjectFolder(nextProject);
			ProjectCloner.CopyLibrary(currentProject, nextProject);

			ProjectCloner.linkFolders(currentProject.assetPath, nextProject.assetPath);
			ProjectCloner.linkFolders(currentProject.projectSettingsPath, nextProject.projectSettingsPath);
			ProjectCloner.linkFolders(currentProject.packagesPath, nextProject.packagesPath);
			ProjectCloner.linkFolders(currentProject.autoBuildPath, nextProject.autoBuildPath);
		}

		//TODO make a nice GUI editorwindow to manage/delete the clone

		//TODO add support for multiple clones
	}

	
}
