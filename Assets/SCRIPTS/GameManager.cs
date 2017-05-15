using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class GameManager : Singleton<GameManager> 
{
	[Header ("Players")]
	public int playersCount = 0;
	public GameObject cat;
	public List<GameObject> mouses = new List<GameObject> ();

	[Header ("Prefabs")]
	public GameObject catPrefab;
	public GameObject mousePrefab;

	[Header ("Spawns")]
	public List<Transform> spawns = new List<Transform> ();

	public List<int> _controllerNumbers = new List<int> ();
	private List<Transform> _spawnsTemp = new List<Transform> ();

	// Use this for initialization
	void Start () 
	{
		SpawnPlayers ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void SpawnPlayers ()
	{
		_controllerNumbers.Clear ();
		mouses.Clear ();
		_spawnsTemp.Clear ();
		_spawnsTemp.AddRange (spawns);

		if (ReInput.controllers.joystickCount == 0)
		{
			Debug.LogWarning ("No Joystick!");

			playersCount = 2;

			for(int i = 0; i < 2; i++)
				_controllerNumbers.Add (i);
		}
		else
		{
			for(int i = 0; i < ReInput.controllers.joystickCount; i++)
			{
				if(i < 4)
					_controllerNumbers.Add (i);
			}

			playersCount = ReInput.controllers.joystickCount;

			if (playersCount > 4)
				playersCount = 4;

			if (playersCount == 1)
			{
				playersCount = 2;
				_controllerNumbers.Add (1);
			}
		}

		for(int i = 0; i < playersCount; i++)
		{
			int randomControllerNumber = _controllerNumbers [Random.Range (0, _controllerNumbers.Count)];
			_controllerNumbers.Remove (randomControllerNumber);

			if (i == 0)
				SpawnCat (randomControllerNumber);
			else
				SpawnMouse (randomControllerNumber);
		}
	}

	void SpawnCat (int controllerNumber)
	{
		Transform spawn = _spawnsTemp [Random.Range (0, _spawnsTemp.Count)];
		_spawnsTemp.Remove (spawn);

		cat = Instantiate (catPrefab, spawn.position, catPrefab.transform.rotation) as GameObject;
		cat.GetComponent<Cat> ().SetupRewired (controllerNumber);
	}

	void SpawnMouse (int controllerNumber)
	{
		Transform spawn = _spawnsTemp [Random.Range (0, _spawnsTemp.Count)];
		_spawnsTemp.Remove (spawn);

		mouses.Add (Instantiate (mousePrefab, spawn.position, mousePrefab.transform.rotation) as GameObject);
		mouses [mouses.Count - 1].GetComponent<Mouse> ().SetupRewired (controllerNumber);
		mouses [mouses.Count - 1].GetComponent<Renderer> ().material.color = Random.ColorHSV ();
	}
}
