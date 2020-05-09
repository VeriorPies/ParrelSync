namespace ParrelSync.NonCore
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class OtherMenuItem
    {
        [MenuItem("ParrelSync/GitHub/View this project on GitHub", priority = 10)]
        private static void OpenGitHub()
        {
            Application.OpenURL("https://github.com/314pies/ParrelSync");
        }
        [MenuItem("ParrelSync/GitHub/View Issues", priority = 10)]
        private static void OpenGitHubIssues()
        {
            Application.OpenURL("https://github.com/314pies/ParrelSync/issues");
        }
    }
}