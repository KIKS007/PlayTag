using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseAnimations : MonoBehaviour 
{
	public GameObject[] mousesMeshes;

	private Mouse _mouseScript;
	private Animator _mouseAnimator;
	private ParticleSystem _attackFX;

	// Use this for initialization
	void Start () 
	{
		_mouseScript = GetComponent<Mouse> ();

		foreach (GameObject m in mousesMeshes)
			m.SetActive (false);

		for (int i = 0; i < mousesMeshes.Length; i++)
		{
			if (i == _mouseScript.controllerNumber)
				mousesMeshes [i].SetActive (true);
			else
				Destroy (mousesMeshes [i]);
		}

		_mouseAnimator = transform.GetComponentInChildren <Animator> ();
		_attackFX = transform.GetComponentInChildren <ParticleSystem> ();

		_mouseScript.OnAttack += () =>
		{ 
			GameObject fx = Instantiate (_attackFX.gameObject, _attackFX.transform.position, _attackFX.transform.rotation, GameManager.Instance._playersParent) as GameObject;

			fx.GetComponent<ParticleSystem> ().Play ();
			_mouseAnimator.SetTrigger ("attack");
		};

		_mouseScript.OnFrozen += () => _mouseAnimator.SetTrigger ("freeze");
		_mouseScript.OnUnfrozen += () => _mouseAnimator.SetTrigger ("iddle");
		_mouseScript.OnDash += () => _mouseAnimator.SetTrigger ("dash");
		_mouseScript.OnDashEnd += () => _mouseAnimator.SetTrigger ("iddle");

		_mouseScript.OnMoving += () => _mouseAnimator.SetBool ("walking", true);
		_mouseScript.OnStopMoving += () => _mouseAnimator.SetBool ("walking", false);
	}
}
