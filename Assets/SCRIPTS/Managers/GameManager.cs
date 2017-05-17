using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Rewired;

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

	public List<int> _controllerNumbers = new List<int> ();
	private List<Transform> _spawnsTemp = new List<Transform> ();
    public List<Mouse> _mouses = new List<Mouse>();
    private float _timer;
    private Cat _cat;
	private Transform _playersParent;

    public static GameManager Instance;

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
		SpawnPlayers ();
		StatsManager.Instance.InitPlayerList();
		StatsManager.Instance.EventSubscriber();

		_timer = timer;

		foreach(GameObject go in mouses)
		{
			_mouses.Add(go.GetComponent<Mouse>());
		}

		_cat = cat.GetComponent<Cat>();

		gameState = GameState.Playing;
	}

	void Update ()
	{
		Timer ();

		/*if (Input.GetKeyDown(KeyCode.Space))
		if (gameState == GameState.Playing || gameState != GameState.Victory)
            Restart();*/
	}

	void Timer ()
	{
		if (gameState != GameState.Playing)
			return;

		//  -TIMER-
		if(_timer <= 0f)
		{
			//  -CAT VICTORY-
			Victory(false);
			timerText.text = "0.00";
		}
		else
		{
			_timer -= Time.deltaTime;
			timerText.text = _timer.ToString("F2");
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

		cat = Instantiate (catPrefab, spawn.position, catPrefab.transform.rotation, _playersParent) as GameObject;
		cat.GetComponent<Cat> ().SetupRewired (controllerNumber);
	}

	void SpawnMouse (int controllerNumber)
	{
		Transform spawn = _spawnsTemp [Random.Range (0, _spawnsTemp.Count)];
		_spawnsTemp.Remove (spawn);

		mouses.Add (Instantiate (mousePrefab, spawn.position, mousePrefab.transform.rotation, _playersParent) as GameObject);
		mouses [mouses.Count - 1].GetComponent<Mouse> ().SetupRewired (controllerNumber);
		mouses [mouses.Count - 1].GetComponent<Renderer> ().material.color = Random.ColorHSV ();
	}

    public void CheckButton()
    {
        foreach(Interrupter b in buttons)
        {
            if (!b.active)
                return;
        }

        //  -MOUSE VICTORY-
        Victory(true);
    }

    public void CheckMouse()
    {
        foreach(Mouse m in _mouses)
        {
            if(m.mouseState == MouseState.Normal)
            {
                return;
            }
        }

        Victory(false);
    }

    void Victory(bool mouse)
    {
        Time.timeScale = 0f;

        //mouses stats
        foreach (Mouse mo in _mouses)
        {
            StatsManager.Instance.playerList[mo.controllerNumber].mouseDuration += timer - _timer;
            if(mouse)
                StatsManager.Instance.playerList[mo.controllerNumber].win++;
            else
                StatsManager.Instance.playerList[mo.controllerNumber].lose++;
        }

        //cat stats
        StatsManager.Instance.playerList[_cat.controllerNumber].catDuration += timer - _timer;
        if(mouse)
            StatsManager.Instance.playerList[_cat.controllerNumber].lose++;
        else
            StatsManager.Instance.playerList[_cat.controllerNumber].win++;

        TournamentManager.Instance.StartCoroutine("RoundEnd");
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}