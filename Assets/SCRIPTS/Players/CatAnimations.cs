﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatAnimations : MonoBehaviour 
{
	public ParticleSystem stunFX;
	private Cat _catScript;
	private Animator _catAnimator;

	// Use this for initialization
	void Start () 
	{
		_catScript = GetComponent<Cat> ();
		_catAnimator = transform.GetComponentInChildren <Animator> ();

		_catScript.OnDashAiming += () => _catAnimator.SetTrigger ("aiming");
		_catScript.OnDash += () => _catAnimator.SetTrigger ("dash");
		_catScript.OnDashEnd += () => _catAnimator.SetTrigger ("dashEnd");
		_catScript.OnStopMoving += () => _catAnimator.SetTrigger ("dashEnd");
		_catScript.OnStunned += () => _catAnimator.SetTrigger ("dashEnd");
		_catScript.OnStunned += () => _catAnimator.SetBool ("walking", false);

		_catScript.OnStunned += () => stunFX.Play ();

		_catScript.OnMoving += () => _catAnimator.SetBool ("walking", true);
		_catScript.OnStopMoving += () => _catAnimator.SetBool ("walking", false);
	}
}
