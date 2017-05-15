using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour {

    public float timeToCaptureSolo = 5.0f;

    private int players = 0;
    private float _remainingTime;

	void Start () {
        _remainingTime = timeToCaptureSolo;
	}
	

	void Update () {
		if(players < 0)
        {
            _remainingTime -= Time.deltaTime * players;
        }
	}

    //Trigger
    public void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Mouse")
        {
            players++;
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if(col.tag == "Mouse")
        {
            players--;
        }
    }
}
