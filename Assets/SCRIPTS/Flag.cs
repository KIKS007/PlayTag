using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : Interrupter
{

    public float timeToCaptureSolo = 5.0f;
    public float multiplePlayerFactor = 0.3f;

    private int players = 0;
    private float _remainingTime;

	protected override void Start () {
        _remainingTime = timeToCaptureSolo;
        _rend = GetComponent<Renderer>();
	}
	

	void Update () {
		if(players > 0 && !active)
        {
            _remainingTime -= Time.deltaTime * (1 + (multiplePlayerFactor * players));
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
