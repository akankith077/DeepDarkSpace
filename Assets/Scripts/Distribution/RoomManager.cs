using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine.UI;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // Singleton class used to manage participation in the networked room 
    // and to provide the related callback functions
    public static RoomManager instance;

    public Toggle muteToggle;
    public TMP_Dropdown microphoneDropdown;

    private GameObject viewingSetup;
    private string[] availableMicrophones;
    private Recorder userRecorder;

    void Start()
    {
        instance = this;
        bool vrMode = PlayerPrefs.GetInt(GlobalSettings.vrEnabledPrefKey) != 0.0;
        
        string folderStem = "";
        if (vrMode) folderStem = GlobalSettings.vrResourcesPath;
        else folderStem = GlobalSettings.desktopResourcesPath;

        viewingSetup = Instantiate(Resources.Load<GameObject>(folderStem + "ViewingSetup"));
        viewingSetup.name = "ViewingSetup";

        if (!UserManager.localUserInstance)
        {
            string avatarType = PlayerPrefs.GetString(GlobalSettings.avatarPrefKey);
            // Instantiates the local user's avatar, thereby also creating and launching the script "UserManager" for this user
            GameObject avatar = PhotonNetwork.Instantiate(folderStem + avatarType, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, 0);
            avatar.name = avatarType;
        }

        InitializeGUI();

        if (PhotonNetwork.IsMasterClient)
        {
            // Since the master client already called OnJoinedRoom() in the launcher scene,
            // we have to call it again here; other clients will directly trigger the
            // OnJoinedRoom() callback in this class
            OnJoinedRoom();
        }
    }

    /* GUI Functions */
    public void InitializeGUI()
    {
        //userRecorder = UserManager.localUserInstance.GetComponent<Recorder>();
        //muteToggle.isOn = !userRecorder.TransmitEnabled;
        microphoneDropdown.ClearOptions();

        availableMicrophones = Microphone.devices;
        foreach (string device in availableMicrophones)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = device;
            microphoneDropdown.options.Add(option);
        }
        microphoneDropdown.RefreshShownValue();
    }

    public void OnMuteMicrophoneToggle(bool isChecked)
    {
        //userRecorder.TransmitEnabled = !isChecked;
    }

    public void OnMicrophoneDropdownChanged(int selectedItem)
    {
        //userRecorder.UnityMicrophoneDevice = availableMicrophones[selectedItem];
        //userRecorder.RestartRecording();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /* Photon Callbacks */
    public override void OnJoinedRoom()
    {
        string connectedRoomName = PhotonNetwork.CurrentRoom.Name;
        Debug.Log("Successfully connected to room " + connectedRoomName + ". Have fun!");
        Debug.Log("There are " + (PhotonNetwork.CurrentRoom.PlayerCount-1) + " other participants in this room.");
    }

    public override void OnLeftRoom()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("{0} has entered the room.", other.NickName);
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("{0} has left the room.", other.NickName);
    }
}
