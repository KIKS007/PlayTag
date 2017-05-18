using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerDisplay : MonoBehaviour 
{
    Text display;
	public float fadeDuration;
    public float fadeDelay = 1;

	void Start () 
	{
		display = transform.GetChild(0).GetComponent<Text>();
		display.gameObject.SetActive(true);
		if (transform.parent.tag == "Cat")
		{
			Cat co = transform.parent.GetComponent<Cat>();
			display.text = "J" + (co.controllerNumber + 1);
		}
		else
		{
			Mouse mo = transform.parent.GetComponent<Mouse>();
			display.text = "J" + (mo.controllerNumber + 1);
		}

		Transform player = transform.parent;
		display.color = player.GetComponent<Renderer> ().material.color;

		transform.SetParent (null);

		display.rectTransform.DOAnchorPos3D (Vector3.forward * -50, fadeDuration + fadeDelay).SetRelative ().OnUpdate (()=> {
			transform.position = player.position;
		});
		DOVirtual.DelayedCall (fadeDelay, () => display.DOColor (new Color (0f, 0f, 0f, 0f), fadeDuration).OnComplete (() => Destroy (gameObject)));
    }
}
