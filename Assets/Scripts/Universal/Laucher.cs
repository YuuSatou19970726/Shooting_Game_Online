using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Laucher : MonoBehaviourPunCallbacks
{
    public static Laucher instance;

    [SerializeField]
    private GameObject loadingScreen;
    [SerializeField]
    private TMP_Text loadingText;

    [SerializeField]
    private GameObject menuButtons;

    // Create New Room
    [Tooltip("Create Room")]
    [SerializeField]
    private GameObject createRoomScreen;
    [SerializeField]
    private TMP_InputField roomNameInput;

    // My Room
    [Tooltip("My Room")]
    [SerializeField]
    private GameObject roomScreen;
    [SerializeField]
    private TMP_Text roomNameText, playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    // Error
    [Tooltip("Error")]
    [SerializeField]
    private GameObject errorScreen;
    [SerializeField]
    private TMP_Text errorText;

    // List Room
    [Tooltip("Room List")]
    [SerializeField]
    private GameObject roomBrowserScreen;
    [SerializeField]
    private RoomButton theRoomButton;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();

    [Tooltip("InputField User Name")]
    [SerializeField]
    private GameObject nameInputScreen;
    [SerializeField]
    private TMP_InputField nameInput;
    private bool hasSetNick;

    [SerializeField]
    private string levelToPlay;
    [SerializeField]
    private GameObject startButton;

    [SerializeField]
    private GameObject roomTestButton;

    void Awake()
    {
        MakeInstance();
    }
    // Start is called before the first frame update
    void Start()
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Network...";

        PhotonNetwork.ConnectUsingSettings();

#if UNITY_EDITOR
        roomTestButton.SetActive(true);
#endif
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    public void OpenRoomCreate()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 8
        };

        PhotonNetwork.CreateRoom(roomNameInput.text, options);

        CloseMenus();
        loadingText.text = "Creating Room...";
        loadingScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Room";
        loadingScreen.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenus();
        loadingText.text = "Joining Room";
        loadingScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void ListAllPlayer()
    {
        foreach (TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayerNames.Add(newPlayerLabel);
        }
    }

    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;
            PlayerPrefs.SetString(PlayerPref.PLAYER_NAME, nameInput.text);

            CloseMenus();
            menuButtons.SetActive(true);
            hasSetNick = true;
        }
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(levelToPlay);
    }

    public void QuickJoin()
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 8
        };
        PhotonNetwork.CreateRoom("Test", options);
        CloseMenus();
        loadingText.text = "Create Room";
        loadingScreen.SetActive(true);
    }

    // override
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        PhotonNetwork.AutomaticallySyncScene = true;

        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (!hasSetNick)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey(PlayerPref.PLAYER_NAME))
                nameInput.text = PlayerPrefs.GetString(PlayerPref.PLAYER_NAME);
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString(PlayerPref.PLAYER_NAME);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayer();

        if (PhotonNetwork.IsMasterClient)
            startButton.gameObject.SetActive(true);
        else
            startButton.gameObject.SetActive(false);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed To Create Room: " + message;
        CloseMenus();
        errorScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();

        theRoomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.SetButtonDetail(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        allPlayerNames.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayer();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
            startButton.gameObject.SetActive(true);
        else
            startButton.gameObject.SetActive(false);
    }

    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
}
