// using System;
// using UnityEditor;
// using UnityEditor.SceneManagement;
// using UnityEngine.SceneManagement;
//
// [InitializeOnLoad]
// public static class DefaultSceneLoader
// {
//     static DefaultSceneLoader()
//     {
//         EditorApplication.playModeStateChanged += LoadDefaultScene;
//     }
//         
//     static void LoadDefaultScene(PlayModeStateChange state)
//     {
//         switch (state)
//         {
//             case PlayModeStateChange.ExitingEditMode:
//                 EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ();
//                 break;
//             case PlayModeStateChange.EnteredPlayMode:
//                 if (SceneManager.GetActiveScene().buildIndex != 0)
//                     SceneManager.LoadScene (0);
//                 break;
//         }
//     }
// }