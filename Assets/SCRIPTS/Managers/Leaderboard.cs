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
	private float _imagePos;
	private float _pillarHeight;

	void OnAwake () {
		_imagePos = playerImage.anchoredPosition.y;
		_pillarHeight = pillar.sizeDelta.y;
	}

	// Use this for initialization
	void OnEnable () {

		playerImage.anchoredPosition = new Vector2(playerImage.anchoredPosition.x ,_imagePos);
		pillar.sizeDelta = new Vector2(pillar.sizeDelta.x, _pillarHeight);

        _score = StatsManager.Instance.playerList[transform.GetSiblingIndex()].score;

		foreach(playerStats p in StatsManager.Instance.playerList)
        {
            if(_bestScore < p.score)
            {
                _bestScore = p.score;
            }
        }

        DOTween.To(() => _scoreTemp, (x)=> _scoreTemp = x, _score, duration).OnUpdate(() => scoreText.text = _scoreTemp.ToString("0000")).SetUpdate(true);
        playerImage.DOAnchorPosY( (_score * imageHeightMax) / _bestScore, duration).SetUpdate(true);
        pillar.DOSizeDelta(new Vector2(pillar.sizeDelta.x, (_score * pillarHeigthMax) / _bestScore), duration).SetUpdate(true);
        
	}
}
