using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBlink : MonoBehaviour {

    public GameObject lightPool;
    public float timeBetweenBlink = 5f;
    public float blinkLuck = 50f;

	public void StartBlink () {
        StartCoroutine(Blink());
	}

    protected virtual IEnumerator Blink()
    {
        yield return new WaitForSeconds(timeBetweenBlink);
        float rand = Random.Range(0f, 100f);
        if(rand <= blinkLuck)
        {
            if (lightPool.activeSelf)
                gameObject.SetActive(false);
            else
                gameObject.SetActive(true);
        }
    }
}
