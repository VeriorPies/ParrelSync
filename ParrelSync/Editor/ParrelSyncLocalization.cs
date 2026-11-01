using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ParrelSync
{
    [InitializeOnLoad]
    public static class ParrelSyncLocalization
    {
        private const string LanguageKey = "ParrelSync_UseChineseV2";
        private const string RegisteredMenusKey = "ParrelSync_LocalizedMenus";
        private static readonly List<string> registeredMenus = new List<string>();
        private static readonly MethodInfo addMenuItem = typeof(Menu).GetMethod("AddMenuItem",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
            new[] { typeof(string), typeof(string), typeof(bool), typeof(int), typeof(Action), typeof(Func<bool>) }, null);
        private static readonly MethodInfo removeMenuItem = typeof(Menu).GetMethod("RemoveMenuItem",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
        private static readonly MethodInfo updateAllMenus = typeof(EditorUtility).GetMethod("Internal_UpdateAllMenus",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        private static bool useChinese;
        private static double nextPreferenceCheck;

        static ParrelSyncLocalization()
        {
            useChinese = EditorPrefs.GetBool(LanguageKey, false);
            registeredMenus.AddRange(SessionState.GetString(RegisteredMenusKey, string.Empty)
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
            EditorApplication.update += CheckLanguagePreference;
            EditorApplication.delayCall += RebuildMenus;
        }

        public static bool UseChinese
        {
            get { return useChinese; }
            set
            {
                EditorPrefs.SetBool(LanguageKey, value);
                if (useChinese == value)
                    return;

                useChinese = value;
                ScheduleMenuRebuild();
                RepaintWindows();
            }
        }

        public static string Text(string english, string chinese)
        {
            return useChinese ? chinese : english;
        }

        private static void ScheduleMenuRebuild()
        {
            EditorApplication.delayCall -= RebuildMenus;
            EditorApplication.delayCall += RebuildMenus;
        }

        private static void RebuildMenus()
        {
            // Unity 2022 exposes dynamic menus through internal APIs.
            if (addMenuItem == null || removeMenuItem == null)
            {
                Debug.LogError("ParrelSync: Dynamic menu APIs are unavailable in this editor.");
                return;
            }

            foreach (string path in registeredMenus)
                removeMenuItem.Invoke(null, new object[] { path });
            registeredMenus.Clear();
            SessionState.EraseString(RegisteredMenusKey);

            AddMenu(Text("Clones Manager", "克隆管理器"), 0, ClonesManagerWindow.InitWindow);
            AddMenu(Text("Preferences", "偏好设置"), 1, Preferences.InitWindow);
            AddMenu(Text("GitHub/View this project on GitHub", "GitHub/项目主页"), 10,
                NonCore.OtherMenuItem.OpenGitHub);
            AddMenu(Text("GitHub/View FAQ", "GitHub/常见问题"), 11,
                NonCore.OtherMenuItem.OpenFAQ);
            AddMenu(Text("GitHub/View Issues", "GitHub/问题反馈"), 12,
                NonCore.OtherMenuItem.OpenGitHubIssues);
            AddMenu(Text("Check for update", "检查更新"), 20, Update.UpdateChecker.CheckForUpdate);
            AddMenu(Text("Language/Simplified Chinese", "语言/简体中文"), 40,
                () => UseChinese = true, useChinese);
            AddMenu(Text("Language/English", "语言/英语"), 41,
                () => UseChinese = false, !useChinese);
            updateAllMenus?.Invoke(null, null);
        }

        private static void AddMenu(string name, int priority, Action execute, bool isChecked = false)
        {
            string path = "ParrelSync/" + name;
            addMenuItem.Invoke(null, new object[] { path, string.Empty, isChecked, priority, execute, null });
            registeredMenus.Add(path);
            SessionState.SetString(RegisteredMenusKey, string.Join("\n", registeredMenus));
        }

        private static void CheckLanguagePreference()
        {
            if (EditorApplication.timeSinceStartup < nextPreferenceCheck)
                return;

            nextPreferenceCheck = EditorApplication.timeSinceStartup + 1d;
            // EditorPrefs is shared with clone editors; pick up their language changes.
            bool storedValue = EditorPrefs.GetBool(LanguageKey, false);
            if (storedValue == useChinese)
                return;

            useChinese = storedValue;
            ScheduleMenuRebuild();
            RepaintWindows();
        }

        private static void RepaintWindows()
        {
            foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window is ClonesManagerWindow)
                    window.titleContent = new GUIContent(Text("Clones Manager", "克隆管理器"));
                else if (window is Preferences)
                    window.titleContent = new GUIContent("ParrelSync " + Text("Preferences", "偏好设置"));

                window.Repaint();
            }
        }
    }
}
