using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Rewired;
using DG.Tweening;

public enum GameState
{
    Playing,
    Menu,
    Victory,
    Pause
}

public delegate void EventHandler();

public class GameManager : MonoBehaviour
{
	[Header ("States")]
	public GameState gameState = GameState.Playing;

	[Header ("Players")]
	public int playersCount = 0;
	public GameObject cat;
	public List<GameObject> mouses = new List<GameObject> ();

	[Header ("Color")]
	public Color catColor;
	public Color[] mousesColor = new Color[4];

    [Header("Scores")]
    public int catWinScore;
    public int mousesWinScore;

	[Header ("Prefabs")]
	public GameObject catPrefab;
	public GameObject mousePrefab;

	[Header ("Spawns")]
	public List<Transform> spawns = new List<Transform> ();

    [Header("Button")]
    public List<Interrupter> buttons;

    [Header("Settings")]
    public float timer;
    public Text timerText;

	private List<int> _controllerNumbers = new List<int> ();
	private List<int> _previousCats = new List<int> ();
	private List<Transform> _spawnsTemp = new List<Transform> ();
	[HideInInspector]
	public List<Mouse> _mouses = new List<Mouse>();
    private float _timer;
	[HideInInspector]
	public Cat _cat;
	[HideInInspector]
	public Transform _playersParent;
	private int _timerSoundCount = 4;

    public static GameManager Instance;

	public event EventHandler OnVictory;
	public event EventHandler OnMouseVictory;
	public event EventHandler OnCatVictory;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

	void Start ()
	{
		if (gameState == GameState.Playing)
			Setup ();
    }

	public void Setup ()
	{
		Time.timeScale = 1f;

		_mouses.Clear ();

		_timerSoundCount = 4;

		SpawnPlayers ();

		_timer = timer;

		foreach(GameObject go in mouses)
		{
			_mouses.Add(go.GetComponent<Mouse>());
		}

		_cat = cat.GetComponent<Cat>();

		DOVirtual.DelayedCall (MenuManager.Instance.startDelay, ()=> gameState = GameState.Playing);
	}

	void Update ()
	{
		Timer ();
	}

	void Timer ()
	{
		if (gameState != GameState.Playing)
			return;

		//  -TIMER-
		if(_timer <= 0f && gameState != GameState.Victory)
		{
			//  -CAT VICTORY-

			//Debug.Log ("Timer Victory");

			Victory(false);
			timerText.text = "0.00";
		}
		else
		{
			_timer -= Time.deltaTime;
			timerText.text = _timer.ToString("F2");

			if(_timer < 6f)
			{
				for(int i = 4; i > 0; i--)
				{
					if(_timer <= i && _timerSoundCount == i)
					{
						_timerSoundCount--;
						SoundsManager.Instance.PlaySound (SoundsManager.Instance.timerLastSeconds1);
						break;
					}
				}
				
				if(_timer <= 0 && _timerSoundCount == 0)
				{
					_timerSoundCount--;
					SoundsManager.Instance.PlaySound (SoundsManager.Instance.timerLastSeconds2);
				}
			}
		}
		//

	}

	void SpawnPlayers ()
	{
		_playersParent = GameObject.Find ("PlayersParent").transform;

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
				_controllerNumbers.Clear ();
				_controllerNumbers.Add (0);
				_controllerNumbers.Add (1);
			}
		}

		TournamentManager.Instance.roundCount = playersCount * 2;

		if (_previousCats.Count == playersCount)
			_previousCats.Clear ();

		for(int i = 0; i < playersCount; i++)
		{
			int randomControllerNumber = _controllerNumbers [Random.Range (0, _controllerNumbers.Count)];

			if (i == 0)
			{
				do
				{
					randomControllerNumber = _controllerNumbers [Random.Range (0, _controllerNumbers.Count)];
				}
				while (_previousCats.Contains (randomControllerNumber));

				_previousCats.Add (randomControllerNumber);
				SpawnCat (randomControllerNumber);
			}
			else
				SpawnMouse (randomControllerNumber);

			_controllerNumbers.Remove (randomControllerNumber);
		}
	}

	void SpawnCat (int controllerNumber)
	{
		Transform spawn = _spawnsTemp [Random.Range (0, _spawnsTemp.Count)];
		_spawnsTemp.Remove (spawn);

		cat = Instantiate (catPrefab, spawn.position, catPrefab.transform.rotation, _playersParent) as GameObject;
		cat.GetComponent<Cat> ().SetupRewired (controllerNumber);
		cat.GetComponent<Renderer> ().material.color = catColor;
	}

	void SpawnMouse (int controllerNumber)
	{
		Transform spawn = _spawnsTemp [Random.Range (0, _spawnsTemp.Count)];
		_spawnsTemp.Remove (spawn);

		mouses.Add (Instantiate (mousePrefab, spawn.position, mousePrefab.transform.rotation, _playersParent) as GameObject);
		mouses [mouses.Count - 1].GetComponent<Mouse> ().SetupRewired (controllerNumber);
		mouses [mouses.Count - 1].GetComponent<Renderer> ().material.color = mousesColor [controllerNumber];
	}

    public void CheckButton()
    {
		if (gameState == GameState.Victory)
			return;
		
        foreach(Interrupter b in buttons)
        {
            if (!b.active)
                return;
        }

		//Debug.Log ("Button Victory");

        //  -MOUSE VICTORY-
        Victory(true);
    }

    public void CheckMouse()
    {
//		Debug.Log ("Check Mouse");

        foreach(Mouse m in _mouses)
        {
            if(m.mouseState == MouseState.Normal)
            {
                return;
            }
        }

//		Debug.Log ("Freeze Victory");

        Victory(false);
    }

    void Victory(bool mouse)
    {
		gameState = GameState.Victory;

        Time.timeScale = 0f;

        //mouses stats
        foreach (Mouse mo in _mouses)
        {
            StatsManager.Instance.playerList[mo.controllerNumber].mouseDuration += timer - _timer;
            StatsManager.Instance.playerList[mo.controllerNumber].frozenDuration += mo.frozenTime;
            if (mouse)
            {
                StatsManager.Instance.playerList[mo.controllerNumber].win++;
                StatsManager.Instance.playerList[mo.controllerNumber].score += StatsManager.Instance.mouseWinScore;
            }
            else
                StatsManager.Instance.playerList[mo.controllerNumber].lose++;
        }

        //cat stats
        StatsManager.Instance.playerList[_cat.controllerNumber].catDuration += timer - _timer;
        if(mouse)
            StatsManager.Instance.playerList[_cat.controllerNumber].lose++;
        else
        { 
            StatsManager.Instance.playerList[_cat.controllerNumber].win++;
            StatsManager.Instance.playerList[_cat.controllerNumber].score += StatsManager.Instance.catWinScore;
        }

        if (OnVictory != null)
			OnVictory ();

		if (OnMouseVictory != null && mouse)
			OnMouseVictory ();

		if (OnCatVictory != null && !mouse)
			OnCatVictory ();

		StartCoroutine (VictoryCoroutine (mouse));
    }

	IEnumerator VictoryCoroutine (bool mouse)
	{
		yield return MenuManager.Instance.StartCoroutine("EndMenu", !mouse);

		TournamentManager.Instance.StartCoroutine("RoundEnd");
	}

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}