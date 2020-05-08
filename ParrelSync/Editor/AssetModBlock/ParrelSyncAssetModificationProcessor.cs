using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ParrelSync
{
    /// <summary>
    /// For preventing assets being modified from the clone instance.
    /// </summary>
    public class ParrelSyncAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (ClonesManager.IsClone())
            {
                if (!EditorQuit.IsQuiting)
                {
                    EditorUtility.DisplayDialog(
                        ClonesManager.ProjectName + ": Asset modifications saving detected and blocked",
                        "Asset modifications saving are blocked in the clone instance. \n\n" +
                        "This is a clone of the original project. \n" +
                        "Making changes to asset files via the clone editor is not recommended. \n"+
                        "Please use the original editor instance if you want to make changes the project files.",
                        "ok"
                    );
                    foreach (var path in paths)
                    {
                        Debug.Log("Attempting to save " + path + " are blocked.");
                    }
                }
                return new string[0] { };
            }
            return paths;
        }
    }
}