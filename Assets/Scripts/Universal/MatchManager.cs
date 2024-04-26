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

    private List<LeaderboardPlayer> leaderboardPlayers = new();

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
        Event();
    }

    public void Event()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (UIController.instance.leaderboard.activeInHierarchy)
                UIController.instance.leaderboard.SetActive(false);
            else
                ShowLeaderboard();
        }
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

    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStats,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void UpdateStatsReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (statType)
                {
                    case 0: // kills
                        allPlayers[i].kills += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : kills " + allPlayers[i].kills);
                        break;
                    case 1: // deaths
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : deaths " + allPlayers[i].deaths);
                        break;
                }

                if (i == index)
                    UpdateStatsDisplay();

                if (UIController.instance.leaderboard.activeInHierarchy)
                    ShowLeaderboard();

                break;
            }
        }
    }

    void UpdateStatsDisplay()
    {
        if (allPlayers.Count > index)
        {
            UIController.instance.killsText.text = "Kills: " + allPlayers[index].kills;
            UIController.instance.deathsText.text = "Deaths: " + allPlayers[index].deaths;
        }
        else
        {
            UIController.instance.killsText.text = "Kills: " + 0;
            UIController.instance.deathsText.text = "Deaths: " + 0;
        }
    }

    void ShowLeaderboard()
    {
        UIController.instance.leaderboard.SetActive(true);
        foreach (LeaderboardPlayer leaderboardPlayer in leaderboardPlayers)
        {
            Destroy(leaderboardPlayer.gameObject);
        }
        leaderboardPlayers.Clear();

        UIController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        foreach (PlayerInfo player in sorted)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(
                UIController.instance.leaderboardPlayerDisplay,
                UIController.instance.leaderboardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);
            newPlayerDisplay.gameObject.SetActive(true);
            leaderboardPlayers.Add(newPlayerDisplay);
        }
    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new();

        while (sorted.Count < players.Count)
        {
            int highes = -1;
            PlayerInfo selectedPlayer = players[0];

            foreach (PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if (player.kills > highes)
                    {
                        selectedPlayer = player;
                        highes = player.kills;
                    }
                }
            }

            sorted.Add(selectedPlayer);
        }

        return sorted;
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
