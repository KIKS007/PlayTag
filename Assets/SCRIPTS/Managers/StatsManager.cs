using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatsManager : MonoBehaviour
{
    public List<playerStats> playerList = new List<playerStats>();
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


    void Start () {
		
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

        //Mouses
        foreach (GameObject g in GameManager.Instance.mouses)
        {
            Mouse mouseScript = g.GetComponent<Mouse>();

            mouseScript.OnSave += () => playerList[mouseScript.controllerNumber].saveCount++;
            mouseScript.OnCapture += () => playerList[mouseScript.controllerNumber].captureCount++;
            mouseScript.OnFrozen += () => playerList[mouseScript.controllerNumber].frozenCount++;
            mouseScript.OnStun += () => playerList[mouseScript.controllerNumber].stunCount++;

            mouseScript.OnFrozen += () => playerList[catControllerNumber].freezeCount++;
        }
    }
}

[Serializable]
public class playerStats
{
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
