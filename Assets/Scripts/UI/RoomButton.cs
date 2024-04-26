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
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Bạn chưa kết nối với máy chủ Photon.");
            return;
        }

        // Kiểm tra nếu phòng đang mở
        if (roomInfo.IsOpen)
        {
            Laucher.instance.JoinRoom(roomInfo);
        }
        else
        {
            Debug.Log($"Phòng {roomInfo.Name} đã đóng.");
        }

        // Kiểm tra nếu phòng đã bị xóa khỏi danh sách
        if (roomInfo.RemovedFromList)
        {
            Debug.Log($"Phòng {roomInfo.Name} đã bị xóa khỏi danh sách.");
        }
    }

}
