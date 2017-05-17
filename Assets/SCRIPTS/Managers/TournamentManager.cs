using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TournamentManager : MonoBehaviour {

    public int roundCount;
    public List<string> levelPool = new List<string>();
    private List<string> _levelPool = new List<string>();
    private string _currentScene;
    private int _currentRound;

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

    void Start () {
        _levelPool = levelPool;
        _currentRound = 0;
	}
	
	void Update () {
		
	}

    public IEnumerator RoundEnd()
    {
        if(_currentRound < roundCount - 1)
        {
            //yield return new WaitWhile(() =>true);
            yield return 0;
            StartCoroutine(NextRound());
        }
        else
        {
            Debug.Log("end tournament");
        }
    }

    IEnumerator NextRound()
    {
        string randScene = _levelPool[Random.Range(0, _levelPool.Count - 1)];

        if (_currentScene != null)
        {
            yield return SceneManager.UnloadSceneAsync(_currentScene);

            //manage levelpool
            _levelPool.Remove(_currentScene);
            if(_levelPool.Count == 0)
            {
                _levelPool = levelPool;
            }
        }

        yield return SceneManager.LoadSceneAsync (randScene);

        _currentScene = randScene;

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

    }
}
