using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using UnityProjectCloner;

namespace UnityProjectCloner
{
	public class ProjectCloner
	{
		/// <summary>
		/// Get the path to the current unityEditor project folder's info
		/// </summary>
		/// <returns></returns>
		public static string FindCurrentProjectPath()
		{
			string projectPath = Application.dataPath.Replace("/Assets", "");
			return projectPath;
		}


		/// <summary>
		/// Return a project object that describes all the paths we need to clone it.
		/// </summary>
		/// <returns></returns>
		public static Project GetCurrentProject()
		{
			string pathString = Application.dataPath;
			Project currentProject = new Project(pathString);
			return currentProject;
		}


		/// <summary>
		/// Creates an empty folder using data in the given Project object
		/// </summary>
		/// <param name="project"></param>
		public static void CreateProjectFolder(Project project)
		{
			string path = project.projectPath;

			//if (System.IO.Directory.Exists(path) == false)
			//{
			Debug.Log("creating new folder at: " + path);
			System.IO.Directory.CreateDirectory(path);
			//}
			//else
			//{
			//	throw new System.Exception("Path already exists: " + path);
			//}
		}


		/// <summary>
		/// Copies the full contents of the unity library. We want to do this to avoid the lengthy reserialization of the whole project when it opens up the clone.
		/// </summary>
		/// <param name="sourceProject"></param>
		/// <param name="destinationProject"></param>
		public static void CopyLibrary(Project sourceProject, Project destinationProject)
		{
			if (System.IO.Directory.Exists(destinationProject.libraryPath) == false)
			{
				Debug.Log("Copying Library data to " + destinationProject.libraryPath);
				FileUtil.CopyFileOrDirectory(sourceProject.libraryPath, destinationProject.libraryPath);
			}
			else
			{
				Debug.LogWarning("Library path already exists: " + destinationProject.libraryPath);
			}
		}


		/// <summary>
		/// Create a link / junction from the real project to it's clone.
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="destinationPath"></param>
		public static void linkFolders(string sourcePath, string destinationPath)
		{
			if (System.IO.Directory.Exists(destinationPath) == false)
			{
				switch (Application.platform)
				{
					case (RuntimePlatform.WindowsEditor):
						createLinkWin(sourcePath, destinationPath);
						break;
					case (RuntimePlatform.OSXEditor):
						createLinkMac(sourcePath, destinationPath);
						break;
					case (RuntimePlatform.LinuxEditor):
						throw new System.NotImplementedException("No linux support yet :(");
					//break;
					default:
						Debug.LogWarning("Not in a known editor. Where are you!?");
						break;
				}
			}
			else
			{
				Debug.LogWarning("Skipping Asset link, it already exists: " + destinationPath);
			}
		}


		private static void createLinkWin(string sourcePath, string destinationPath)
		{
			string cmd = "/C mklink /J " + string.Format("\"{0}\" \"{1}\"", destinationPath, sourcePath);
			Debug.Log("Windows hard link " + cmd);

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = cmd;
			process.StartInfo = startInfo;
			process.Start();
		}


		private static void createLinkMac(string sourcePath, string destinationPath)
		{
			string cmd = "ln " + string.Format("\"{0}\" \"{1}\"", destinationPath, sourcePath);
			Debug.Log("Mac hard link " + cmd);

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "/bin/bash";
			startInfo.Arguments = cmd;
			process.StartInfo = startInfo;
			process.Start();
		}

		//TODO avoid terminal calls and use proper api stuff. See below for windows! 

		////https://docs.microsoft.com/en-us/windows/desktop/api/ioapiset/nf-ioapiset-deviceiocontrol
		//[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		//private static extern bool DeviceIoControl(System.IntPtr hDevice, uint dwIoControlCode,
		//	System.IntPtr InBuffer, int nInBufferSize,
		//	System.IntPtr OutBuffer, int nOutBufferSize,
		//	out int pBytesReturned, System.IntPtr lpOverlapped);


	}
}
