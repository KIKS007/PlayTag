using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Rewired;

public class StatsManager : MonoBehaviour
{
    public List<playerStats> playerList = new List<playerStats>();

    [Header ("Score Values")]
    public int mouseWinScore;
    public int catWinScore;
    public int saveScore;
    public int captureScore;
    public int stunScore;
    public int freezeScore;

    public static StatsManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start () 
	{
		TournamentManager.Instance.OnStartGame += InitPlayerList;
		TournamentManager.Instance.OnNextRound += EventSubscriber;
	}

    public void InitPlayerList ()
    {
        playerList.Clear();

		for(int i = 0; i < GameManager.Instance.playersCount; i++)
            playerList.Add(new playerStats());
    }

    public void EventSubscriber()
    {
        int catControllerNumber = GameManager.Instance.cat.GetComponent<Cat>().controllerNumber;
		playerList [catControllerNumber].isCat = true;

        //Mouses
        foreach (GameObject g in GameManager.Instance.mouses)
        {
            Mouse mouseScript = g.GetComponent<Mouse>();

			playerList [mouseScript.controllerNumber].isCat = false;

            mouseScript.OnSave += () => { playerList[mouseScript.controllerNumber].saveCount++; playerList[mouseScript.controllerNumber].score += saveScore; };
            mouseScript.OnCapture += () => { playerList[mouseScript.controllerNumber].captureCount++; playerList[mouseScript.controllerNumber].score += captureScore; };
            mouseScript.OnFrozen += () => playerList[mouseScript.controllerNumber].frozenCount++;
            mouseScript.OnStun += () => { playerList[mouseScript.controllerNumber].stunCount++; playerList[mouseScript.controllerNumber].score += stunScore; };

            mouseScript.OnFrozen += () => { playerList[catControllerNumber].freezeCount++; playerList[catControllerNumber].score += freezeScore; };
        }
    }
}

[Serializable]
public class playerStats
{
	public bool isCat;

    [Header("General")]
    public int score;
    public int win;
    public int lose;
    public float dashDistance;

    [Header("Mouse")]
    public float mouseDuration;
    public float frozenDuration;
    public int frozenCount;
    public int saveCount;
    public int captureCount;
    public int stunCount;

    [Header("Cat")]
    public float catDuration;
    public int freezeCount;

}
