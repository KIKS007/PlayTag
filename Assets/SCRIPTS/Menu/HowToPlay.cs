using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class HowToPlay : MonoBehaviour {

    public List<GameObject> slideList = new List<GameObject>();

    public Player _rewiredPlayer;
    public Player _rewiredSystem;

    private int _index;

	void Start () {
        _rewiredPlayer = ReInput.players.GetPlayer(0);
        _rewiredSystem = ReInput.players.SystemPlayer;
        _index = 0;
    }

	void Update () {

        if (_rewiredPlayer.GetButtonDown("Move Horizontal") || _rewiredSystem.GetButtonDown("Move Horizontal"))
        {
            slideList[_index].SetActive(false);
            if (_index < slideList.Count -1)
            {
                _index++;
            }
            else
            {
                _index = 0;
            }
            slideList[_index].SetActive(true);
        }

        if (_rewiredPlayer.GetNegativeButtonDown("Move Horizontal") || _rewiredSystem.GetNegativeButtonDown("Move Horizontal"))
        {
            slideList[_index].SetActive(false);
            if (_index != 0)
            {
                _index--;
            }
            else
            {
                _index = slideList.Count - 1;
            }
            slideList[_index].SetActive(true);
        }
    }
}
