using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : Interrupter
{

    public float timeToCaptureSolo = 5.0f;

    private int players = 0;
    private float _remainingTime;

	protected override void Start () {
        _remainingTime = timeToCaptureSolo;
        _rend = GetComponent<Renderer>();
	}
	

	void Update () {
		if(players > 0 && !active)
        {
            _remainingTime -= Time.deltaTime * players;
        }

        if(_remainingTime <= 0)
        {
            active = true;
            _rend.material.color = Color.red;
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

    public override void Activate()
    {
        
    }
}
