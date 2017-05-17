using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class MenuPlayerSelection : MonoBehaviour 
{
	public Button startButton;

	public GameObject[] playersIcon = new GameObject[0];

	private GameObject[] playersReady = new GameObject[4];
	public Player[] _rewiredPlayer = new Player[4];

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i < _rewiredPlayer.Length; i++)
			_rewiredPlayer [i] = ReInput.players.GetPlayer (i);

		ReInput.ControllerConnectedEvent += (ControllerStatusChangedEventArgs obj) => RefreshIcons ();
		ReInput.ControllerDisconnectedEvent += (ControllerStatusChangedEventArgs obj) => RefreshIcons ();

		RefreshIcons ();
	}

	void OnEnable ()
	{
		RefreshIcons ();

		for (int i = 0; i < playersReady.Length; i++)
			playersReady [i] = playersIcon [i].transform.GetChild (1).gameObject;

		foreach (GameObject ready in playersReady)
			ready.SetActive (false);

		CheckCanPlay ();
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

				SoundsManager.Instance.PlaySound (SoundsManager.Instance.buttonPlayer);

				CheckCanPlay ();
			}
		}
	}

	bool CheckCanPlay ()
	{
		bool canPlay = true;

		for (int i = 0; i < ReInput.controllers.joystickCount; i++)
			if(i < 4)
			if (!playersReady [i].activeSelf)
				canPlay = false;
		
		if (ReInput.controllers.joystickCount < 2)
			canPlay = false;

		if (canPlay)
		{
			startButton.interactable = true;
			MenuManager.Instance.eventSystem.SetSelectedGameObject (null);
			MenuManager.Instance.eventSystem.SetSelectedGameObject (startButton.gameObject);
		}
		else
		{
			
			startButton.interactable = false;
		}
		
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
