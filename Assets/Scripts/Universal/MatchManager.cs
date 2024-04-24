using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;

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
        if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene(SceneTag.SCENES_MAIN_MENU);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
}
