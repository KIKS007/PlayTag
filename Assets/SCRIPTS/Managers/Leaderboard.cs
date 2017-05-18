using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Leaderboard : MonoBehaviour {

    public float duration;
    public float imageHeightMax;
    public float pillarHeigthMax;
    public Text scoreText;
    public Text HF1Text;
    public Text HF2Text;
    public RectTransform playerImage;
    public RectTransform pillar;
    public int _bestScore = 0;
    private int _score;
    private int _scoreTemp;

    // Use this for initialization
    void OnEnable () {
        _score = StatsManager.Instance.playerList[transform.GetSiblingIndex()].score;

		foreach(playerStats p in StatsManager.Instance.playerList)
        {
            p.unfrozenDuration = p.mouseDuration - p.frozenDuration;

            if(_bestScore < p.score)
            {
                _bestScore = p.score;
            }
        }

        DOTween.To(() => _scoreTemp, (x)=> _scoreTemp = x, _score, duration).OnUpdate(() => scoreText.text = _scoreTemp.ToString("0000")).SetUpdate(true);
        playerImage.DOAnchorPosY( (_score * imageHeightMax) / _bestScore, duration).SetUpdate(true);
        pillar.DOSizeDelta(new Vector2(pillar.sizeDelta.x, (_score * pillarHeigthMax) / _bestScore), duration).SetUpdate(true).OnComplete(
            () => {
                //rank pop
            });
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void FeedHF(string t)
    {
        if (HF1Text.text == "")
        {
            HF1Text.text = t;
        }
        else if (HF2Text.text == "")
        {
            HF2Text.text = t;
        }
    }
}
