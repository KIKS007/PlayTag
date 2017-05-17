using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TournamentManager : MonoBehaviour {

    public int roundCount;
	public List<string> levelPool = new List<string>();
	private int _currentRound;
	private List<string> _levelPool = new List<string>();
    private string _currentScene;

    public static TournamentManager Instance;

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
	}
	
	void Update () {
		
	}

    public IEnumerator RoundEnd()
    {
        if(_currentRound < roundCount)
        {
			Debug.Log("Next Round");

            //yield return new WaitWhile(() =>true);
            yield return 0;
            StartCoroutine(NextRound());
        }
        else
        {
			yield return SceneManager.UnloadSceneAsync(_currentScene);

			_currentScene = null;

			Debug.Log("End tournament");
			MenuManager.Instance.MainMenu ();
        }
    }

	public void StartGame ()
	{
		_levelPool.AddRange (levelPool);
		_currentRound = 0;

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

		GameManager.Instance.Setup ();
    }
}
