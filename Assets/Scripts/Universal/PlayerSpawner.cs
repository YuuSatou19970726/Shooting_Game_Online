using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;

    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject deathEffect;

    private GameObject player;

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
        if (PhotonNetwork.IsConnected)
            SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        SpawnPlayer();
    }

    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
}
