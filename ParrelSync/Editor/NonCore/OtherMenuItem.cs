namespace ParrelSync.NonCore
{
    using UnityEditor;
    using UnityEngine;

    public class OtherMenuItem
    {
        internal static void OpenGitHub()
        {
            Application.OpenURL(ExternalLinks.GitHubHome);
        }

        internal static void OpenFAQ()
        {
            Application.OpenURL(ExternalLinks.FAQ);
        }

        internal static void OpenGitHubIssues()
        {
            Application.OpenURL(ExternalLinks.GitHubIssue);
        }
    }
}
