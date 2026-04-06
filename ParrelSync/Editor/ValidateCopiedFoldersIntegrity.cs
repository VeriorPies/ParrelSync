namespace ParrelSync
{
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.Text;
    using System.Security.Cryptography;
    using System.IO;
    using System.Linq;

    [InitializeOnLoad]
    public class ValidateCopiedFoldersIntegrity
    {
        const string SessionStateKey = "ValidateCopiedFoldersIntegrity_Init";
        /// <summary>
        /// Called once on editor startup.
        /// Validate copied folders integrity in clone project
        /// </summary>
        static ValidateCopiedFoldersIntegrity()
        {
            if (!SessionState.GetBool(SessionStateKey, false))
            {
                SessionState.SetBool(SessionStateKey, true);
                if (!ClonesManager.IsClone()) { return; }

                ValidatePackagesFiles(ClonesManager.GetCurrentProjectPath(), ClonesManager.GetOriginalProjectPath());
            }
        }

        public static void ValidateFolder(string targetRoot, string originalRoot, string folderName)
        {
            var targetFolderPath = Path.Combine(targetRoot, folderName);
            var targetFolderHash = CreateMd5ForFolder(targetFolderPath);

            var originalFolderPath = Path.Combine(originalRoot, folderName);
            var originalFolderHash = CreateMd5ForFolder(originalFolderPath);

            if (targetFolderHash != originalFolderHash)
            {
                Debug.Log("ParrelSync: Detected changes in '" + folderName + "' directory. Updating cloned project...");
                FileUtil.ReplaceDirectory(originalFolderPath, targetFolderPath);
            }
        }

        /// <summary>
        /// Validates only the root-level files in the Packages directory (manifest.json, packages-lock.json, etc.).
        /// Subdirectories are symlinked and don't need validation.
        /// </summary>
        public static void ValidatePackagesFiles(string targetRoot, string originalRoot)
        {
            var targetPackagesPath = Path.Combine(targetRoot, "Packages");
            var originalPackagesPath = Path.Combine(originalRoot, "Packages");

            if (!Directory.Exists(originalPackagesPath) || !Directory.Exists(targetPackagesPath))
                return;

            foreach (var originalFilePath in Directory.GetFiles(originalPackagesPath))
            {
                var fileName = Path.GetFileName(originalFilePath);
                var targetFilePath = Path.Combine(targetPackagesPath, fileName);

                bool needsCopy = !File.Exists(targetFilePath) ||
                                 File.ReadAllText(originalFilePath) != File.ReadAllText(targetFilePath);

                if (needsCopy)
                {
                    Debug.Log("ParrelSync: Detected changes in 'Packages/" + fileName + "'. Updating cloned project...");
                    File.Copy(originalFilePath, targetFilePath, true);
                }
            }
        }

        static string CreateMd5ForFolder(string path)
        {
            // assuming you want to include nested folders
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                 .OrderBy(p => p).ToList();

            MD5 md5 = MD5.Create();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];

                // hash path
                string relativePath = file.Substring(path.Length + 1);
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                byte[] contentBytes = File.ReadAllBytes(file);
                if (i == files.Count - 1)
                    md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                else
                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
            }

            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }
    }
}