using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableDebug : MonoBehaviour {

	public GameObject currentSelection;

	private EventSystem _eventSystem;

	// Use this for initialization
	void Start () 
	{
		_eventSystem = GetComponent<EventSystem> ();
	}
	
	// Update is called once per frame
	void Update () {
		currentSelection = _eventSystem.currentSelectedGameObject;
	}
}
