using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public TMP_Text overheatedMessage;
    public Slider weaponTempSlider;

    public GameObject deathScreen;
    public TMP_Text deathText;

    public Slider healthSlider;

    public TMP_Text killsText, deathsText;

    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;

    public GameObject endScreen;

    void Awake()
    {
        MakeInstance();
    }

    // Start is called before the first frame update
    void Start()
    {

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
