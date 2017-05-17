﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class MenuPlayerSelection : MonoBehaviour 
{
	public Button startButton;

	public GameObject[] playersIcon = new GameObject[0];

	public GameObject[] playersReady = new GameObject[0];

	public Player[] _rewiredPlayer = new Player[4];

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i < _rewiredPlayer.Length; i++)
			_rewiredPlayer [i] = ReInput.players.GetPlayer (i);

		for (int i = 0; i < playersReady.Length; i++)
			playersReady [i] = playersIcon [i].transform.GetChild (2).gameObject;

		ReInput.ControllerConnectedEvent += (ControllerStatusChangedEventArgs obj) => RefreshIcons ();
		ReInput.ControllerDisconnectedEvent += (ControllerStatusChangedEventArgs obj) => RefreshIcons ();

		RefreshIcons ();
	}

	void OnEnable ()
	{
		RefreshIcons ();

		foreach (GameObject ready in playersReady)
			ready.SetActive (false);
	}

	void Update ()
	{
		for(int i = 0; i < _rewiredPlayer.Length; i++)
		{
			if(_rewiredPlayer [i].GetButtonDown ("Start"))
			{
				if (playersReady [i].activeSelf)
					playersReady [i].SetActive (false);
				else
					playersReady [i].SetActive (true);

				CheckCanPlay ();
			}
		}
	}

	bool CheckCanPlay ()
	{
		bool canPlay = true;

		foreach (GameObject g in playersReady)
			if (!g.activeSelf)
				canPlay = false;

		if (canPlay)
			startButton.interactable = true;
		else
			startButton.interactable = false;

		return canPlay;
	}

	void RefreshIcons ()
	{
		foreach (GameObject icon in playersIcon)
			icon.SetActive (false);

		for(int i = 0; i < ReInput.controllers.joystickCount; i++)
			playersIcon [i].SetActive (true);
	}
}
