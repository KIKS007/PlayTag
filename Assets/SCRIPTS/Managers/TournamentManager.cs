using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TournamentManager : MonoBehaviour 
{

	[HideInInspector]
    public int roundCount;
	public List<string> levelPool = new List<string>();
    public GameObject playersParent;
	private int _currentRound;
	private List<string> _levelPool = new List<string>();
    private string _currentScene;

    public static TournamentManager Instance;

	public event EventHandler OnNextRound;
	public event EventHandler OnStartGame;
	public event EventHandler OnEndMode;

    void Awake()
    {
        if (Instance == null)
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
		_levelPool.AddRange (levelPool);
        _currentRound = 0;

		MenuManager.Instance.OnMainMenu += MainMenu;
		MainMenu ();
	}

	public void MainMenu ()
	{
		StartCoroutine (MainMenuCoroutine ());
	}

	IEnumerator MainMenuCoroutine ()
	{
		if (!SceneManager.GetSceneByName (levelPool [0]).isLoaded)
			yield return SceneManager.LoadSceneAsync (levelPool [0], LoadSceneMode.Additive);
	}
	
	void Update () {
		
	}

    public IEnumerator RoundEnd()
    {
        if(_currentRound < roundCount)
        {
			//Debug.Log("Next Round");

            yield return 0;
            StartCoroutine(NextRound());
        }
        else
        {
			MenuManager.Instance.ShowMenu(MenuManager.Instance.leadeboardMenu);
			MenuManager.Instance.timerCanvas.SetActive (false);

			//Debug.Log("End tournament");

			if (OnEndMode!= null)
				OnEndMode ();
			
			yield return 0;
        }
    }

	public IEnumerator UnloadLevel ()
	{
		if(_currentScene != null)
			yield return SceneManager.UnloadSceneAsync(_currentScene);

		_currentScene = null;
	}

	public void StartGame ()
	{
		_levelPool.AddRange (levelPool);
		_currentRound = 0;

		if (OnStartGame != null)
			OnStartGame ();

		StartCoroutine (NextRound ());
	}

    public IEnumerator NextRound()
    {
		if(_levelPool.Count == 0)
			_levelPool.AddRange (levelPool);

        string randScene = _levelPool [Random.Range (0, _levelPool.Count)];

		_levelPool.Remove(randScene);

        if (_currentScene != null)
            yield return SceneManager.UnloadSceneAsync(_currentScene);

		if (!SceneManager.GetSceneByName (randScene).isLoaded)
			yield return SceneManager.LoadSceneAsync (randScene, LoadSceneMode.Additive);

        _currentScene = randScene;
		_currentRound++;

        //get buttons
        GameManager.Instance.buttons.Clear();
        GameObject[] tempgo = GameObject.FindGameObjectsWithTag("Flag");
        foreach(GameObject go in tempgo)
        {
            GameManager.Instance.buttons.Add(go.GetComponent<Interrupter>());
        }

        //get spawns
        GameManager.Instance.spawns.Clear();
        tempgo = GameObject.FindGameObjectsWithTag("Spawn");
        foreach (GameObject go in tempgo)
        {
            GameManager.Instance.spawns.Add(go.transform);
        }

        playersParent = GameObject.Find("PlayersParent");

		GameManager.Instance.Setup ();

		if (OnNextRound != null)
			OnNextRound ();
    }
}
