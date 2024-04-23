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

    // List Room
    [Tooltip("List Room")]
    [SerializeField]
    private GameObject roomScreen;
    [SerializeField]
    private TMP_Text roomNameText;

    // Error
    [Tooltip("Error")]
    [SerializeField]
    private GameObject errorScreen;
    [SerializeField]
    private TMP_Text errorText;

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
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
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

    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed To Create Room: " + message;
        CloseMenus();
        errorScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
}
