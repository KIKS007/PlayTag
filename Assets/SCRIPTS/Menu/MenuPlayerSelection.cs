using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class MenuPlayerSelection : MonoBehaviour 
{
	public GameObject[] playersIcon = new GameObject[0];

	public GameObject[] playersReady = new GameObject[0];

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i < playersReady.Length; i++)
			playersReady [i] = playersIcon [i].transform.GetChild (2);

		ReInput.ControllerConnectedEvent += (ControllerStatusChangedEventArgs obj) => RefreshIcons ();
		ReInput.ControllerDisconnectedEvent += (ControllerStatusChangedEventArgs obj) => RefreshIcons ();

		RefreshIcons ();
	}
	
	void OnEnable ()
	{
		RefreshIcons ();
	}

	void RefreshIcons ()
	{
		foreach (GameObject icon in playersIcon)
			icon.SetActive (false);

		for(int i = 0; i < ReInput.controllers.joystickCount; i++)
			playersIcon [i].SetActive (true);
	}
}
