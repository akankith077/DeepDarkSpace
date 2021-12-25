#if UNITY_EDITOR
using Photon.Pun;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayStateNotifier
{
    // Makes play mode always start in the Launcher scene and returns to the
    // previously opened scene when exiting play mode

    static PlayStateNotifier()
    {
        EditorApplication.playModeStateChanged += OnPlaymodeChanged;
    }

    static void OnPlaymodeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            PlayerPrefs.SetString("loadedSceneBeforePlay", EditorSceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene("Assets/Scenes/Launcher.unity");
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }

            Debug.Log("Disconnected from Photon network before termination.");
            PhotonNetwork.Disconnect();
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (PlayerPrefs.HasKey("loadedSceneBeforePlay"))
            {
                EditorSceneManager.OpenScene(PlayerPrefs.GetString("loadedSceneBeforePlay"));
                PlayerPrefs.DeleteKey("loadedSceneBeforePlay");
            }
        }
    }
}
#endif