using System;
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

    public float respawnTime = 5f;

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

    public void Die(String damageByPlayer)
    {
        UIController.instance.deathText.text = "You were killed by " + damageByPlayer;

        if (player != null)
        {
            StartCoroutine(DieCo());
        }
    }

    IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIController.instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTime);
        UIController.instance.deathScreen.SetActive(false);
        SpawnPlayer();
    }

    void MakeInstance()
    {
        if (instance == null)
            instance = this;
    }
}
