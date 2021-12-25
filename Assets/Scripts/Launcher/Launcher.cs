using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using TMPro;
using System.Linq;
using System;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Manages the logic of the main menu, initializes distribution and
    // connects to the rooms

    // GUI References
    private TMP_InputField playerNameInputField;
    private Toggle vrEnabledToggle;
    private Button[] buttons;
    private ToggleGroup colorToggleGroup;
    private TMP_Dropdown avatarDropdown;


    void Awake()
    {
        InitializeButtons();
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToMasterServer();
        }
        PhotonNetwork.SendRate = GlobalSettings.networkUpdateRate;
        PhotonNetwork.SerializationRate = GlobalSettings.networkUpdateRate;
        Instantiate(Resources.Load<GameObject>("Desktop/ViewingSetup"));
    }
    
    public void InitializeButtons()
    {
        buttons = GameObject.Find("RoomButtonPanel").GetComponentsInChildren<Button>();
        
        if (buttons.Length != GlobalSettings.defaultRooms.Length)
        {
            Debug.LogError("Please provide the same number of rooms and buttons.");
        }

        for (int i = 0; i < buttons.Length; ++i)
        {
            buttons[i].gameObject.name = GlobalSettings.defaultRooms[i];
            string buttonText = GlobalSettings.defaultRooms[i] + " (0/" + GlobalSettings.maxPlayersPerRoom + ")";
            buttons[i].gameObject.GetComponentInChildren<TMP_Text>().text = buttonText;
        }
    }


    public void ConnectToMasterServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = GlobalSettings.gameVersion;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("You are connected to the master server.");
        PhotonNetwork.JoinLobby();
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }
    }

    void Start()
    {
        playerNameInputField = GameObject.Find("PlayerNameField").GetComponent<TMP_InputField>();
        vrEnabledToggle = GameObject.Find("VREnabledToggle").GetComponent<Toggle>();
        colorToggleGroup = GameObject.Find("ColorToggleGroup").GetComponent<ToggleGroup>();
        avatarDropdown = GameObject.Find("AvatarDropdown").GetComponent<TMP_Dropdown>();

        if (!playerNameInputField)
        {
            Debug.LogError("Could not find all UI components. Please check PlayerNameField and RoomNameField.");
        }

        if (PlayerPrefs.HasKey(GlobalSettings.playerNamePrefKey))
        {
            playerNameInputField.text = PlayerPrefs.GetString(GlobalSettings.playerNamePrefKey);
        }

        if (PlayerPrefs.HasKey(GlobalSettings.vrEnabledPrefKey))
        {
            vrEnabledToggle.isOn = PlayerPrefs.GetInt(GlobalSettings.vrEnabledPrefKey) != 0;
        }

        avatarDropdown.ClearOptions();
        foreach (string avatar in GlobalSettings.availableAvatars)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = avatar;
            avatarDropdown.options.Add(option);
        }

        if (PlayerPrefs.HasKey(GlobalSettings.avatarPrefKey))
        {
            string storedAvatarName = PlayerPrefs.GetString(GlobalSettings.avatarPrefKey);
            int dropdownIndex = Array.IndexOf(GlobalSettings.availableAvatars, storedAvatarName);
            if (dropdownIndex != -1)
                avatarDropdown.value = dropdownIndex;
            else
                avatarDropdown.value = 0;
        }
        avatarDropdown.RefreshShownValue();

        if (PlayerPrefs.HasKey(GlobalSettings.colorPrefKey))
        {
            Toggle colorToggle = colorToggleGroup.transform.Find(PlayerPrefs.GetString(GlobalSettings.colorPrefKey)).GetComponent<Toggle>();
            colorToggle.isOn = true;
        }
    }

    public void OnButtonClicked(Button button)
    {
        string playerName = playerNameInputField.text;
        string roomName = button.name;
        bool vrEnabled = vrEnabledToggle.isOn;
        string playerColor = getToggledColor();

        if (playerName == "" || roomName == "")
        {
            Debug.LogError("Please enter valid names for player and room.");
            return;
        }

        PhotonNetwork.NickName = playerName;
        PlayerPrefs.SetString(GlobalSettings.playerNamePrefKey, playerName);
        PlayerPrefs.SetInt(GlobalSettings.vrEnabledPrefKey, (vrEnabled ? 1 : 0));
        PlayerPrefs.SetString(GlobalSettings.colorPrefKey, playerColor);
        PlayerPrefs.SetString(GlobalSettings.avatarPrefKey, avatarDropdown.options[avatarDropdown.value].text);
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = GlobalSettings.maxPlayersPerRoom }, TypedLobby.Default);
    }

    public string getToggledColor()
    {
        string ColorName = colorToggleGroup.ActiveToggles().First().name;
        return ColorName;
    }

    public override void OnCreatedRoom()
    {   
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Dictionary<string, int> usersPerRoom = new Dictionary<string, int>();
        foreach (RoomInfo info in roomList)
        {
            usersPerRoom.Add(info.Name, info.PlayerCount);
        }

        for (int i = 0; i < buttons.Length; ++i)
        {
            string roomName = buttons[i].name;
            if (usersPerRoom.ContainsKey(roomName))
            {
                string buttonText = GlobalSettings.defaultRooms[i] + " (" + usersPerRoom[roomName] + "/" + GlobalSettings.maxPlayersPerRoom + ")";
                buttons[i].gameObject.GetComponentInChildren<TMP_Text>().text = buttonText;
            }
            
        }
    }
}
