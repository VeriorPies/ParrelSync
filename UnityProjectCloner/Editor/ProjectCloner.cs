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
    /// <summary>
    /// Contains all required methods for creating a linked clone of the Unity project.
    /// </summary>
    public class ProjectCloner
    {
        /// <summary>
        /// Name used for an identifying file created in the clone project directory.
        /// </summary>
        /// <remarks>
        /// (!) Do not change this after the clone was created, because then connection will be lost.
        /// </remarks>
        public const string CloneFileName = ".clone";

        /// <summary>
        /// Suffix added to the end of the project clone name when it is created.
        /// </summary>
        /// <remarks>
        /// (!) Do not change this after the clone was created, because then connection will be lost.
        /// </remarks>
        public const string CloneNameSuffix = "_clone";

        #region Managing clones
        /// <summary>
        /// Creates clone from the project currently open in Unity Editor.
        /// </summary>
        /// <returns></returns>
        public static Project CreateCloneFromCurrent()
        {
            if (IsClone())
            {
                Debug.LogError("This project is already a clone. Cannot clone it.");
                return null;
            }

            string currentProjectPath = ProjectCloner.GetCurrentProjectPath();
            return ProjectCloner.CreateCloneFromPath(currentProjectPath);
        }

        /// <summary>
        /// Creates clone of the project located at the given path.
        /// </summary>
        /// <param name="sourceProjectPath"></param>
        /// <returns></returns>
        public static Project CreateCloneFromPath(string sourceProjectPath)
        {
            Project sourceProject = new Project(sourceProjectPath);
            Project cloneProject = new Project(sourceProjectPath + ProjectCloner.CloneNameSuffix);

            Debug.Log("Start project name: " + sourceProject);
            Debug.Log("Clone project name: " + cloneProject);

            ProjectCloner.CreateProjectFolder(cloneProject);
            ProjectCloner.CopyLibraryFolder(sourceProject, cloneProject);

            ProjectCloner.LinkFolders(sourceProject.assetPath, cloneProject.assetPath);
            ProjectCloner.LinkFolders(sourceProject.projectSettingsPath, cloneProject.projectSettingsPath);
            ProjectCloner.LinkFolders(sourceProject.packagesPath, cloneProject.packagesPath);
            ProjectCloner.LinkFolders(sourceProject.autoBuildPath, cloneProject.autoBuildPath);

            ProjectCloner.RegisterClone(cloneProject);

            return cloneProject;
        }

        /// <summary>
        /// Registers a clone by placing an identifying ".clone" file in its root directory.
        /// </summary>
        /// <param name="cloneProject"></param>
        private static void RegisterClone(Project cloneProject)
        {
            /// Add clone identifier file.
            string identifierFile = Path.Combine(cloneProject.projectPath, ProjectCloner.CloneFileName);
            File.Create(identifierFile).Dispose();

            /// Add collabignore.txt to stop the clone from messing with Unity Collaborate if it's enabled. Just in case.
            string collabignoreFile = Path.Combine(cloneProject.projectPath, "collabignore.txt");
            File.WriteAllText(collabignoreFile, "*"); /// Make it ignore ALL files in the clone.
        }

        /// <summary>
        /// Opens a project located at the given path (if one exists).
        /// </summary>
        /// <param name="projectPath"></param>
        public static void OpenProject(string projectPath)
        {
            if (!Directory.Exists(projectPath))
            {
                Debug.LogError("Cannot open the project - provided folder (" + projectPath + ") does not exist.");
                return;
            }
            if (projectPath == ProjectCloner.GetCurrentProjectPath())
            {
                Debug.LogError("Cannot open the project - it is already open.");
                return;
            }

            string fileName = EditorApplication.applicationPath;
            string args = "-projectPath \"" + projectPath + "\"";
            Debug.Log("Opening project \"" + fileName + " " + args + "\"");
            ProjectCloner.StartHiddenConsoleProcess(fileName, args);
        }

        /// <summary>
        /// Deletes the clone of the currently open project, if such exists.
        /// </summary>
        public static void DeleteClone()
        {
            /// Clone won't be able to delete itself.
            if (ProjectCloner.IsClone()) return;

            string cloneProjectPath = ProjectCloner.GetCloneProjectPath();

            ///Extra precautions.
            if (cloneProjectPath == string.Empty) return;
            if (cloneProjectPath == ProjectCloner.GetOriginalProjectPath()) return;
            if (cloneProjectPath.EndsWith(ProjectCloner.CloneNameSuffix)) return;

            /// Delete the clone project folder.
            throw new System.NotImplementedException();
            // TODO: implement proper project deletion;
            //       appears that using FileUtil.DeleteFileOrDirectory(...) on symlinks affects the contents of linked folders
            //       (because this script self-deleted itself and half of the Assets folder when I tested it :D)
            //       there must be another, safe method to delete the clone folder and symlinks without touching the original
            {
                /*
                EditorUtility.DisplayProgressBar("Deleting clone...", "Deleting '" + ProjectCloner.GetCloneProjectPath() + "'", 0f);
                try
                {
                     FileUtil.DeleteFileOrDirectory(cloneProjectPath);
                }
                catch (IOException)
                {
                     EditorUtility.DisplayDialog("Could not delete clone", "'" + ProjectCloner.GetCurrentProject().name + "_clone' may be currently open in another unity Editor. Please close it and try again.", "OK");
                }
                EditorUtility.ClearProgressBar();
                */
            }
        }
        #endregion

        #region Creating project folders
        /// <summary>
        /// Creates an empty folder using data in the given Project object
        /// </summary>
        /// <param name="project"></param>
        public static void CreateProjectFolder(Project project)
        {
            string path = project.projectPath;
            Debug.Log("Creating new empty folder at: " + path);
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Copies the full contents of the unity library. We want to do this to avoid the lengthy reserialization of the whole project when it opens up the clone.
        /// </summary>
        /// <param name="sourceProject"></param>
        /// <param name="destinationProject"></param>
        public static void CopyLibraryFolder(Project sourceProject, Project destinationProject)
        {
            if (Directory.Exists(destinationProject.libraryPath))
            {
                Debug.LogWarning("Library copy: destination path already exists! ");
                return;
            }

            Debug.Log("Library copy: " + destinationProject.libraryPath);
            ProjectCloner.CopyDirectoryWithProgressBar(sourceProject.libraryPath, destinationProject.libraryPath, "Cloning project '" + sourceProject.name + "'. ");
        }
        #endregion

        #region Creating symlinks
        /// <summary>
        /// Creates a symlink between destinationPath and sourcePath (Mac version).
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        private static void CreateLinkMac(string sourcePath, string destinationPath)
        {
            Debug.LogWarning("This hasn't been tested yet! I am mac-less :( Please chime in on the github if it works for you.");

            string cmd = "ln " + string.Format("\"{0}\" \"{1}\"", destinationPath, sourcePath);
            Debug.Log("Mac hard link " + cmd);

            ProjectCloner.StartHiddenConsoleProcess("/bin/bash", cmd);
        }

        /// <summary>
        /// Creates a symlink between destinationPath and sourcePath (Windows version).
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        private static void CreateLinkWin(string sourcePath, string destinationPath)
        {
            string cmd = "/C mklink /J " + string.Format("\"{0}\" \"{1}\"", destinationPath, sourcePath);
            Debug.Log("Windows junction: " + cmd);
            ProjectCloner.StartHiddenConsoleProcess("cmd.exe", cmd);
        }

        //TODO avoid terminal calls and use proper api stuff. See below for windows! 
        ////https://docs.microsoft.com/en-us/windows/desktop/api/ioapiset/nf-ioapiset-deviceiocontrol
        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern bool DeviceIoControl(System.IntPtr hDevice, uint dwIoControlCode,
        //	System.IntPtr InBuffer, int nInBufferSize,
        //	System.IntPtr OutBuffer, int nOutBufferSize,
        //	out int pBytesReturned, System.IntPtr lpOverlapped);

        /// <summary>
        /// Create a link / junction from the real project to it's clone.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        public static void LinkFolders(string sourcePath, string destinationPath)
        {
            if ((Directory.Exists(destinationPath) == false) && (Directory.Exists(sourcePath) == true))
            {
                switch (Application.platform)
                {
                    case (RuntimePlatform.WindowsEditor):
                        CreateLinkWin(sourcePath, destinationPath);
                        break;
                    case (RuntimePlatform.OSXEditor):
                        CreateLinkMac(sourcePath, destinationPath);
                        break;
                    case (RuntimePlatform.LinuxEditor):
                        throw new System.NotImplementedException("No linux support yet :(");
                        break;
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
        #endregion

        #region Utility methods
        /// <summary>
        /// Returns true is the project currently open in Unity Editor is a clone.
        /// </summary>
        /// <returns></returns>
        public static bool IsClone()
        {
            /// The project is a clone if its root directory contains an empty file named ".clone".
            string cloneFilePath = Path.Combine(ProjectCloner.GetCurrentProjectPath(), ProjectCloner.CloneFileName);
            bool isClone = File.Exists(cloneFilePath);
            return isClone;
        }

        /// <summary>
        /// Get the path to the current unityEditor project folder's info
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentProjectPath()
        {
            return Application.dataPath.Replace("/Assets", "");
        }

        /// <summary>
        /// Return a project object that describes all the paths we need to clone it.
        /// </summary>
        /// <returns></returns>
        public static Project GetCurrentProject()
        {
            string pathString = ProjectCloner.GetCurrentProjectPath();
            return new Project(pathString);
        }

        /// <summary>
        /// Returns the path to the original project.
        /// If currently open project is the original, returns its own path.
        /// If the original project folder cannot be found, retuns an empty string.
        /// </summary>
        /// <returns></returns>
        public static string GetOriginalProjectPath()
        {
            if (IsClone())
            {
                /// If this is a clone...
                /// Original project path can be deduced by removing the suffix from the clone's path.
                string cloneProjectPath = ProjectCloner.GetCurrentProject().projectPath;
                string originalProjectPath = cloneProjectPath.Remove(cloneProjectPath.Length - ProjectCloner.CloneNameSuffix.Length);

                if (Directory.Exists(originalProjectPath)) return originalProjectPath;
                return string.Empty;
            }
            else
            {
                /// If this is the original, we return its own path.
                return ProjectCloner.GetCurrentProjectPath();
            }
        }

        /// <summary>
        /// Returns the path to the clone project.
        /// If currently open project is the clone, returns its own path.
        /// If the clone does not exist yet, retuns an empty string.
        /// </summary>
        /// <returns></returns>
        public static string GetCloneProjectPath()
        {
            if (IsClone())
            {
                /// If this is the clone, we return its own path.
                return ProjectCloner.GetCurrentProjectPath();
            }
            else
            {
                /// If this is the original...
                /// Clone project path can be deduced by add the suffix to the original's path.
                string originalProjectPath = ProjectCloner.GetCurrentProject().projectPath;
                string cloneProjectPath = originalProjectPath + ProjectCloner.CloneNameSuffix;

                if (Directory.Exists(cloneProjectPath)) return cloneProjectPath;
                return string.Empty;
            }
        }

        /// <summary>
        /// Copies directory located at sourcePath to destinationPath. Displays a progress bar.
        /// </summary>
        /// <param name="source">Directory to be copied.</param>
        /// <param name="destination">Destination directory (created automatically if needed).</param>
        /// <param name="progressBarPrefix">Optional string added to the beginning of the progress bar window header.</param>
        public static void CopyDirectoryWithProgressBar(string sourcePath, string destinationPath, string progressBarPrefix = "")
        {
            var source = new DirectoryInfo(sourcePath);
            var destination = new DirectoryInfo(destinationPath);

            long totalBytes = 0;
            long copiedBytes = 0;

            ProjectCloner.CopyDirectoryWithProgressBarRecursive(source, destination, ref totalBytes, ref copiedBytes, progressBarPrefix);
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Copies directory located at sourcePath to destinationPath. Displays a progress bar.
        /// Same as the previous method, but uses recursion to copy all nested folders as well.
        /// </summary>
        /// <param name="source">Directory to be copied.</param>
        /// <param name="destination">Destination directory (created automatically if needed).</param>
        /// <param name="totalBytes">Total bytes to be copied. Calculated automatically, initialize at 0.</param>
        /// <param name="copiedBytes">To track already copied bytes. Calculated automatically, initialize at 0.</param>
        /// <param name="progressBarPrefix">Optional string added to the beginning of the progress bar window header.</param>
        private static void CopyDirectoryWithProgressBarRecursive(DirectoryInfo source, DirectoryInfo destination, ref long totalBytes, ref long copiedBytes, string progressBarPrefix = "")
        {
            /// Directory cannot be copied into itself.
            if (source.FullName.ToLower() == destination.FullName.ToLower())
            {
                Debug.LogError("Cannot copy directory into itself.");
                return;
            }

            /// Calculate total bytes, if required.
            if (totalBytes == 0)
            {
                totalBytes = ProjectCloner.GetDirectorySize(source, true, progressBarPrefix);
            }

            /// Create destination directory, if required.
            if (!Directory.Exists(destination.FullName))
            {
                Directory.CreateDirectory(destination.FullName);
            }

            /// Copy all files from the source.
            foreach (FileInfo file in source.GetFiles())
            {
                try
                {
                    file.CopyTo(Path.Combine(destination.ToString(), file.Name), true);
                }
                catch (IOException)
                {
                    /// Some files may throw IOException if they are currently open in Unity editor.
                    /// Just ignore them in such case.
                }

                /// Account the copied file size.
                copiedBytes += file.Length;

                /// Display the progress bar.
                float progress = (float)copiedBytes / (float)totalBytes;
                bool cancelCopy = EditorUtility.DisplayCancelableProgressBar(
                    progressBarPrefix + "Copying '" + source.FullName + "' to '" + destination.FullName + "'...",
                    "(" + (progress * 100f).ToString("F2") + "%) Copying file '" + file.Name + "'...",
                    progress);
                if (cancelCopy) return;
            }

            /// Copy all nested directories from the source.
            foreach (DirectoryInfo sourceNestedDir in source.GetDirectories())
            {
                DirectoryInfo nextDestingationNestedDir = destination.CreateSubdirectory(sourceNestedDir.Name);
                ProjectCloner.CopyDirectoryWithProgressBarRecursive(sourceNestedDir, nextDestingationNestedDir, ref totalBytes, ref copiedBytes, progressBarPrefix);
            }
        }

        /// <summary>
        /// Calculates the size of the given directory. Displays a progress bar.
        /// </summary>
        /// <param name="directory">Directory, which size has to be calculated.</param>
        /// <param name="includeNested">If true, size will include all nested directories.</param>
        /// <param name="progressBarPrefix">Optional string added to the beginning of the progress bar window header.</param>
        /// <returns>Size of the directory in bytes.</returns>
        private static long GetDirectorySize(DirectoryInfo directory, bool includeNested = false, string progressBarPrefix = "")
        {
            EditorUtility.DisplayProgressBar(progressBarPrefix + "Calculating size of directories...", "Scanning '" + directory.FullName + "'...", 0f);

            /// Calculate size of all files in directory.
            long filesSize = directory.EnumerateFiles().Sum((FileInfo file) => file.Length);

            /// Calculate size of all nested directories.
            long directoriesSize = 0;
            if (includeNested)
            {
                IEnumerable<DirectoryInfo> nestedDirectories = directory.EnumerateDirectories();
                foreach (DirectoryInfo nestedDir in nestedDirectories)
                {
                    directoriesSize += ProjectCloner.GetDirectorySize(nestedDir, true, progressBarPrefix);
                }
            }

            return filesSize + directoriesSize;
        }

        /// <summary>
        /// Starts process in the system console, taking the given fileName and args.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="args"></param>
        private static void StartHiddenConsoleProcess(string fileName, string args)
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = args;

            process.Start();
        }
        #endregion
    }
}
