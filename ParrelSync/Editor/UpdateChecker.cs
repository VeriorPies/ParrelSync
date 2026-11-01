using System;
using UnityEditor;
using UnityEngine;
namespace ParrelSync.Update
{
    /// <summary>
    /// A simple update checker
    /// </summary>
    public class UpdateChecker
    {
        //const string LocalVersionFilePath = "Assets/ParrelSync/VERSION.txt";
        public const string LocalVersion = "1.5.2";
        internal static void CheckForUpdate()
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                try
                {
                    //This won't work with UPM packages
                    //string localVersionText = AssetDatabase.LoadAssetAtPath<TextAsset>(LocalVersionFilePath).text;

                    string localVersionText = LocalVersion;
                    Debug.Log("Local version text : " + LocalVersion);

                    string latesteVersionText = client.DownloadString(ExternalLinks.RemoteVersionURL);
                    Debug.Log("latest version text got: " + latesteVersionText);
                    string messageBody = ParrelSyncLocalization.Text("Current Version: ", "当前版本：") + localVersionText +"\n"
                                         + ParrelSyncLocalization.Text("Latest Version: ", "最新版本：") + latesteVersionText + "\n";
                    var latestVersion = new Version(latesteVersionText);
                    var localVersion = new Version(localVersionText);

                    if (latestVersion > localVersion)
                    {
                        Debug.Log("There's a newer version");
                        messageBody += ParrelSyncLocalization.Text("There's a newer version available", "发现新版本。");
                        if(EditorUtility.DisplayDialog(
                            ParrelSyncLocalization.Text("Check for update.", "检查更新"), messageBody,
                            ParrelSyncLocalization.Text("Get latest release", "获取最新版本"),
                            ParrelSyncLocalization.Text("Close", "关闭")))
                        {
                            Application.OpenURL(ExternalLinks.Releases);
                        }
                    }
                    else
                    {
                        Debug.Log("Current version is up-to-date.");
                        messageBody += ParrelSyncLocalization.Text("Current version is up-to-date.", "当前已是最新版本。");
                        EditorUtility.DisplayDialog(ParrelSyncLocalization.Text("Check for update.", "检查更新"),
                            messageBody, ParrelSyncLocalization.Text("OK", "确定"));
                    }
                    
                }
                catch (Exception exp)
                {
                    Debug.LogError("Error with checking update. Exception: " + exp);
                    EditorUtility.DisplayDialog(ParrelSyncLocalization.Text("Update Error", "更新检查失败"),
                        ParrelSyncLocalization.Text("Error with checking update. \nSee console for more details.",
                            "检查更新时发生错误。\n详情请查看控制台。"),
                        ParrelSyncLocalization.Text("OK", "确定")
                    );
                }
            }
        }
    }
}
