using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : Interrupter
{

    public float timeToCaptureSolo = 5.0f;
    public float multiplePlayerFactor = 0.3f;
    
    private float _remainingTime;
    [HideInInspector]
    public List<Mouse> mouseList;

	protected override void Start () {
        _remainingTime = timeToCaptureSolo;
        _rend = GetComponent<Renderer>();
	}
	

	void Update () 
	{
		if(mouseList.Count > 0 && !active)
        {
            _remainingTime -= Time.deltaTime * (1 + (multiplePlayerFactor * (mouseList.Count - 1)));
            _rend.material.color = new Color((1f - (_remainingTime / timeToCaptureSolo) / 2f), _rend.material.color.g, _rend.material.color.b);
        }

        if(_remainingTime <= 0)
        {
            active = true;
            _rend.material.color = Color.red;

            foreach(Mouse mo in mouseList)
            {
                mo.OnCaptureVoid();
            }

            GameManager.Instance.CheckButton();
        }
	}

    //Trigger
    public void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Mouse")
        {
            Mouse mo = col.GetComponent<Mouse>();
            if (mo.mouseState == MouseState.Normal)
            {
                mouseList.Add(mo);
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if(col.tag == "Mouse")
        {
            Mouse mo = col.GetComponent<Mouse>();
            if (mo.mouseState == MouseState.Normal)
            {
                mouseList.Remove(mo);
            }
        }
    }

    public override void Activate()
    {
        
    }
}
