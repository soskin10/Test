#if UNITY_EDITOR
using Project.Scripts.Constants;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Bootstrap.Editor
{
    [InitializeOnLoad]
    public static class BootstrapSceneLoader
    {
        private const string SessionKey = "BootstrapSceneLoader.SceneToRestore";

        
        static BootstrapSceneLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                var activePath = SceneManager.GetActiveScene().path;

                if (activePath != GetBootScenePath())
                {
                    SessionState.SetString(SessionKey, activePath);

                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(GetBootScenePath());
                }
                else
                {
                    SessionState.EraseString(SessionKey);
                }
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                var sceneToRestore = SessionState.GetString(SessionKey, "");

                if (!string.IsNullOrEmpty(sceneToRestore))
                {
                    EditorSceneManager.OpenScene(sceneToRestore);
                    SessionState.EraseString(SessionKey);
                }
            }
        }

        private static string GetBootScenePath()
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
                if (sceneName == SceneNames.Boot)
                    return scene.path;
            }

            return "";
        }
    }
}
#endif