﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;

public enum CatState
{
	Normal,
	Stunned
}

public enum DashState
{
	CanDash,
	DashAim,
	Dashing,
	DashEnd,
	Cooldown
}

public class Cat : MonoBehaviour 
{
	[Header("Controller Number")]
	public int controllerNumber = -1;
	[HideInInspector]
	public Player rewiredPlayer; // The Rewired Player

	[Header("States")]
	public CatState catstate = CatState.Normal;
	public DashState dashState = DashState.CanDash;

	[Header("Movement")]
	public float speed = 18;
	public float aimingSpeed = 5f;

	[Header("Stun")]
	public float stunDuration = 1;

	[Header("Lookat")]
	public float normalLerp;
	public float aimingLerp;

	[Header("Dash")]
	public float dashSpeed = 80;
	public float dashLengthFactor = 10;
	public float timeToMaxDuration;
	public float timeReductionFactor = 0.7f;

	[Header("Dash Target")]
	public LayerMask wallMask = 1 << 10;
	public Transform dashTarget;

	[Header("Dash End")]
	public float dashEndDuration = 0.2f;
	public float dashCooldown = 1f;

	[Header("Dash Line Renderer")]
	public LineRenderer dashLineRenderer;

	private Rigidbody _rigidbody;
	private Vector3 _movement;
	private float _dashSpeedTemp;

	void Start () 
	{
		_rigidbody = GetComponent<Rigidbody> ();
		rewiredPlayer = ReInput.players.GetPlayer(controllerNumber);
		dashLineRenderer.gameObject.SetActive (false);
	}

	public void SetupRewired (int number)
	{
		controllerNumber = number;
		rewiredPlayer = ReInput.players.GetPlayer(controllerNumber);
	}

	void Update () 
	{
		//Movement Vector
		_movement = new Vector3(rewiredPlayer.GetAxisRaw("Move Horizontal"), 0f, rewiredPlayer.GetAxisRaw("Move Vertical"));
		_movement.Normalize();

		//Dash
		if (rewiredPlayer.GetButtonDown("Action 1") && dashState == DashState.CanDash && catstate != CatState.Stunned)
			StartCoroutine(DashAim());
		
		LookForward ();
	}

	void FixedUpdate ()
	{
		Movement ();
	}

	void Movement ()
	{
		if (catstate == CatState.Stunned)
			return;

		//Movement
		if (dashState != DashState.Dashing && dashState != DashState.DashEnd && dashState != DashState.DashAim)
			_rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);

		if (dashState == DashState.DashAim)
			_rigidbody.MovePosition(_rigidbody.position + _movement * aimingSpeed * Time.fixedDeltaTime);
	}

	void LookForward ()
	{
		if (dashState != DashState.Dashing && dashState != DashState.DashEnd && _movement != Vector3.zero)
		{
			Quaternion rotation = Quaternion.LookRotation ( _movement, Vector3.up);
			float lerp = dashState == DashState.DashAim ? aimingLerp : normalLerp;

			transform.rotation = Quaternion.Lerp  (transform.rotation, rotation, lerp);
		}
	}
		
	protected virtual IEnumerator DashAim ()
	{
		dashState = DashState.DashAim;

		_rigidbody.velocity = Vector3.zero;

		float holdTime = 0;

		dashLineRenderer.gameObject.SetActive (true);
		dashTarget.gameObject.SetActive (true);

		while(rewiredPlayer.GetButton ("Action 1"))
		{
			if (catstate == CatState.Stunned)
				yield break;

			holdTime = rewiredPlayer.GetButtonTimePressed ("Action 1");

			if (holdTime >= timeToMaxDuration)
				break;

			Vector3 positionTemp = transform.position + transform.forward * dashLengthFactor * (holdTime / timeToMaxDuration);

			RaycastHit hit;

			if(Physics.Linecast (transform.position, positionTemp, out hit, wallMask))
				dashTarget.position = hit.point;
			else
				dashTarget.position = positionTemp;

			dashLineRenderer.SetPosition (0, transform.position);
			dashLineRenderer.SetPosition (1, dashTarget.position);

			yield return new WaitForEndOfFrame ();
		}

		dashLineRenderer.gameObject.SetActive (false);
		dashTarget.gameObject.SetActive (false);

		//Debug.Log (holdTime);

		StartCoroutine (Dash ());
	}

	protected virtual IEnumerator Dash ()
	{
		_dashSpeedTemp = dashSpeed;

		dashState = DashState.Dashing;

		float distance = Vector3.Distance (transform.position, dashTarget.position);
		float duration = distance / dashSpeed;

		DOVirtual.DelayedCall (duration * timeReductionFactor, ()=> dashState = DashState.Cooldown);

		Vector3 dashTargetTemp = dashTarget.position;
		Vector3 movementTemp = (dashTargetTemp - transform.position).normalized;

		while (dashState != DashState.Cooldown)
		{
			movementTemp = dashTargetTemp - transform.position;

			if(_rigidbody.velocity.magnitude < 1)
				movementTemp.Normalize ();

			//if (Vector3.Distance (transform.position, dashTargetTemp) > 0.5f)
				
				_rigidbody.velocity = movementTemp * _dashSpeedTemp;
			yield return new WaitForFixedUpdate();
		}

		_rigidbody.velocity = Vector3.zero;

		yield return new WaitForSeconds(dashCooldown);

		dashState = DashState.CanDash;
	}

	public virtual IEnumerator Stun ()
	{
		catstate = CatState.Stunned;

		Color initialColor = GetComponent<Renderer> ().material.color;
		GetComponent<Renderer> ().material.color = Color.black;

		yield return new WaitForSeconds (stunDuration);

		GetComponent<Renderer> ().material.color = initialColor;

		catstate = CatState.Normal;
	}
}