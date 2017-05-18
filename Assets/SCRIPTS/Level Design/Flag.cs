using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : Interrupter
{

    public float timeToCaptureSolo = 5.0f;
    public float multiplePlayerFactor = 0.3f;
    public int requiredPlayers = 1;
    public Renderer loadingRenderer;
    
    private float _remainingTime;
    public List<Mouse> mouseList;

	protected override void Start () {
        _remainingTime = timeToCaptureSolo;
        _rend = loadingRenderer;
	}
	

	void Update () 
	{
		if (active)
			return;
		
		if(mouseList.Count > requiredPlayers - 1 && !active)
        {
            _remainingTime -= Time.deltaTime * (1 + (multiplePlayerFactor * (mouseList.Count - 1)));
            _rend.material.SetFloat("_angle", _remainingTime / timeToCaptureSolo);
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

	public void OnTriggerStay (Collider col)
	{
		if (active)
			return;

		mouseList.Clear ();

		if(col.tag == "Mouse")
		{
			Mouse mo = col.GetComponent<Mouse>();

			if (mo.mouseState == MouseState.Normal && !mouseList.Contains (mo))
				mouseList.Add (mo);
			
			else if (mouseList.Contains (mo))
				mouseList.Remove (mo);
		}
	}

    public void OnTriggerExit(Collider col)
    {
		if (active)
			return;

		if(col.tag == "Mouse")
        {
            Mouse mo = col.GetComponent<Mouse>();
            if (mo.mouseState == MouseState.Normal)
            {
                mouseList.Remove(mo);
            }
        }
    }
}
