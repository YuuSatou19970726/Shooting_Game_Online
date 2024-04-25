using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public enum EventCodes : byte
{
    NewPlayer,
    ListPlayers,
    UpdateStats
}

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;

    public List<PlayerInfo> allPlayers = new();
    private int index;
    public EventCodes theEvent;

    void Awake()
    {
        MakeInstance();
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckConnected();
    }


    void CheckConnected()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Bạn không đang ở trong một phòng. Không thể gửi sự kiện.");
            return;
        }

        if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene(SceneTag.SCENES_MAIN_MENU);
        else
            NewPlayerSend(PhotonNetwork.NickName);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }

    public void NewPlayerReceive(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0], (int)dataReceived[1], (int)dataReceived[2], (int)dataReceived[3]);
        allPlayers.Add(player);

        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        object[] packege = new object[allPlayers.Count];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            packege[i] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            packege,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void ListPlayersReceive(object[] dataReceived)
    {
        allPlayers.Clear();

        for (int i = 0; i < dataReceived.Length; i++)
        {
            object[] piece = (object[])dataReceived[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
            );

            allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i;
            }
        }
    }

    public void UpdateStatsSend()
    {

    }

    public void UpdateStatsReceive(object[] dataReceived)
    {

    }

    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            Debug.Log("Received event " + theEvent);

            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStats:
                    UpdateStatsReceive(data);
                    break;
            }
        }
    }

    // override
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}
