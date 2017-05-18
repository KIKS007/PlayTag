using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PoopZone : MonoBehaviour {

    public float catSpeedModifier;
    public float mouseSpeedModifier;
    public float lifeTime;
    public float birthTime;
    public float deathTime;
    
    private Vector3 _scale;

    void Start()
    {
        _scale = transform.localScale;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        transform.DOScale(_scale, birthTime).OnComplete(() => DOVirtual.DelayedCall(lifeTime, () => transform.DOScale(0f, deathTime).OnComplete(() => Destroy(gameObject))));
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (mouseSpeedModifier != 0f && col.tag == "Mouse")
            {
                Mouse m = col.GetComponent<Mouse>();
                m.speedBoost = mouseSpeedModifier;
                m.dashState = DashState.Cooldown;
            }
            else if (catSpeedModifier != 0f)
            {
                col.GetComponent<Cat>().speedBoost = catSpeedModifier;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (mouseSpeedModifier != 0f && col.tag == "Mouse")
            {
                Mouse m = col.GetComponent<Mouse>();
                m.speedBoost = 0;
                m.dashState = DashState.CanDash;
            }
            else if (catSpeedModifier != 0f)
            {
                col.GetComponent<Cat>().speedBoost = 0f;
            }
        }
    }
}
