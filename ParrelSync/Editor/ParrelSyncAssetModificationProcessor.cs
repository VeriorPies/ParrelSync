using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ParrelSync
{
    public class ParrelSyncAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (ProjectCloner.IsClone()) {
                EditorUtility.DisplayDialog(
                ProjectCloner.ProjectName + ": Asset modifying blocked",
                "Asset modifying detected and blocked. \n" +
                "This is a clone of the original project. \n" +
                "Please use the original editor instance if you want to make changes the project files.",
                "ok");
                foreach (var path in paths)
                {
                    Debug.Log("Trying to save " + path);
                }
                return new string[0] {};
            }
            return paths;
        }
    }
}