using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Leaderboard : MonoBehaviour 
{
    public GameObject gold;
    public GameObject silver;
    public GameObject bronze;

    public float duration;
    public float imageHeightMax;
    public float pillarHeigthMax;
    public Text scoreText;
    public Text HF1Text;
    public Text HF2Text;
    public RectTransform playerImage;
    public RectTransform pillar;
    public int _bestScore = 0;
    public int _rank;
    private int _score;
    private int _scoreTemp;

    // Use this for initialization
    void OnEnable () {
        gold.SetActive(false);
        silver.SetActive(false);
        bronze.SetActive(false);

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
                switch (_rank)
                {
                    case 0 :
                        gold.transform.localScale = Vector3.zero; gold.SetActive(true); gold.transform.DOScale(1f, 1f);
                        break;
                    case 1 :
                        silver.transform.localScale = Vector3.zero; silver.SetActive(true); silver.transform.DOScale(1f, 1f);
                        break;
                    case 2 :
                        bronze.transform.localScale = Vector3.zero; bronze.SetActive(true); bronze.transform.DOScale(1f, 1f);
                        break;
                }
            });
        
	}

    public void FeedHF(string t)
    {
        if (HF1Text.text == "")
        {
            HF1Text.text = t;
            //HF1Text.DOText(t, 1f);
        }
        else if (HF2Text.text == "")
        {
            HF2Text.text = t;
            //HF2Text.DOText(t, 1f);
        }
    }
}
