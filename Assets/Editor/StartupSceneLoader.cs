using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;


[InitializeOnLoad]
public static class StartupSceneLoader
{
   static StartupSceneLoader()
   {
        EditorApplication.playModeStateChanged += LoadStartuScene;
   }

    private static void LoadStartuScene(PlayModeStateChange state)
    {
        if(state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        }

        if(state == PlayModeStateChange.EnteredPlayMode)
        {
            if(EditorSceneManager.GetActiveScene().buildIndex != 0)
            {
                EditorSceneManager.LoadScene(0);
            }
        }
    }
}
