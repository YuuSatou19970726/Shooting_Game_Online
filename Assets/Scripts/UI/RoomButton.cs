using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class RoomButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text buttonText;
    private RoomInfo roomInfo;

    public void SetButtonDetail(RoomInfo inputInfo)
    {
        roomInfo = inputInfo;
        buttonText.text = roomInfo.Name;
    }

    public void OpenRoom()
    {
        Laucher.instance.JoinRoom(roomInfo);
        // PhotonNetwork.JoinRoom(roomInfo.Name);
    }

}
