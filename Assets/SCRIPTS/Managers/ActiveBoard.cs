using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActiveBoard : MonoBehaviour {

    public List<string> TitleList = new List<string>();
    public List<string> StatsList = new List<string>();

    private List<playerStats> _playerList = new List<playerStats>();
    private List<Leaderboard> _leaderboardList = new List<Leaderboard>();

	void OnEnable () {
        for(int i = 0; i < GameManager.Instance.playersCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
            _leaderboardList.Add(transform.GetChild(i).GetComponent<Leaderboard>());
        }
        StartCoroutine(delayedHF());
	}

    IEnumerator delayedHF()
    {
        yield return new WaitForSeconds(1f);
        _playerList.AddRange(StatsManager.Instance.playerList);
        HF();
    }

    public void HF()
    {
        _playerList.OrderBy(x => x.frozenDuration).ToList();
        if(_playerList[0].frozenDuration != 0f)
            _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[0] + "\n" + StatsList[0] + " - " + (int)(_playerList[0].frozenDuration / 60f) + ":" + ((int)(_playerList[0].frozenDuration % 60)).ToString("00"));

        _playerList.OrderBy(x => x.catDuration).ToList();
            _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[1] + "\n" + StatsList[1] + " - " + (int)(_playerList[0].catDuration / 60f) + ":" + ((int)(_playerList[0].catDuration % 60)).ToString("00"));

        _playerList.OrderBy(x => x.unfrozenDuration).ToList();
        _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[2] + "\n" + StatsList[2] + " - " + ((int)(_playerList[0].unfrozenDuration / (_playerList[0].mouseDuration) * 100f)).ToString("00") + "%");

        _playerList.OrderBy(x => x.freezeCount).ToList();
        if (_playerList[0].freezeCount != 0f)
            _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[3] + "\n" + StatsList[3] + " - " + _playerList[0].freezeCount);

        _playerList.OrderBy(x => x.frozenCount).ToList();
        if (_playerList[0].frozenCount != 0f)
            _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[4] + "\n" + StatsList[4] + " - " + _playerList[0].frozenCount);

        _playerList.OrderBy(x => x.saveCount).ToList();
        if (_playerList[0].saveCount != 0f)
            _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[5] + "\n" + StatsList[5] + " - " + _playerList[0].saveCount);

        _playerList.OrderBy(x => x.captureCount).ToList();
        if (_playerList[0].captureCount != 0f)
            _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[6] + "\n" + StatsList[6] + " - " + _playerList[0].captureCount);

        _playerList.OrderBy(x => x.win).ToList();
        _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[7] + "\n" + StatsList[7] + " - " + _playerList[0].win);

        _playerList.OrderBy(x => x.lose).ToList();
        _leaderboardList[_playerList[0].controllerNumber].FeedHF(TitleList[8] + "\n" + StatsList[8] + " - " + _playerList[0].lose);

        _playerList.OrderBy(x => x.stunCount).ToList();
        if (_playerList[0].stunCount != 0f)
            _leaderboardList[_playerList[0].stunCount].FeedHF(TitleList[9] + "\n" + StatsList[9] + " - " + _playerList[0].stunCount);
    }


}
