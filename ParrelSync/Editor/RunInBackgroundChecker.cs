
using ParrelSync;
using UnityEditor;

internal class RunInBackgroundChecker
{
    const string StopShowingKey = "ParrelSync_StopShowRunInBackgroundWarning_";
    const string AlreadyShowedWarningKey = "ParrelSync_AlreadyShowedRunInBackgroundWarning";

    [InitializeOnLoadMethod]
    static void Init()
    {
        if (ClonesManager.IsClone())
        {
            return;
        }

        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        EditorApplication.update -= OnEditorUpdate;

        if (!PlayerSettings.runInBackground && !SessionState.GetBool(AlreadyShowedWarningKey, false))
        {
            var projectSettings = ParrelSyncProjectSettings.GetSerializedSettings().targetObject as ParrelSyncProjectSettings;

            string actualStopShowingKey = StopShowingKey + projectSettings.ProjectId;
            bool stopShowing = EditorPrefs.GetBool(actualStopShowingKey, false);

            if (stopShowing)
            {
                return;
            }

            int button = EditorUtility.DisplayDialogComplex(
                                       ClonesManager.ProjectName + ": Run In Background Disabled",
                                       "Run In Background is disabled in player settings.\n\n" +
                                       "Communication between source project and clone projects may " +
                                       "not behave correctly.\n\n" +
                                       "Do you want to enable it?",
                                       "Yes", "Don't show again", "No"
                                       );

            // 0 - Yes
            // 1 - Don't show again
            // 2 - No
            switch (button)
            {
                case 0:
                    PlayerSettings.runInBackground = true;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    break;
                case 1:
                    EditorPrefs.SetBool(actualStopShowingKey, true);
                    break;
                case 2:
                    break;
            }

            SessionState.SetBool(AlreadyShowedWarningKey, true);
        }
    }
}
