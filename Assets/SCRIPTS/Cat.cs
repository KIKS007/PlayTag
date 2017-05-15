using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;

public enum CatState
{
	Dead
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
	public DashState dashState = DashState.CanDash;

	[Header("Movement")]
	public float speed = 18;

	[Header("Dash")]
	public bool kikiDashEnabled = true;
	public float dashSpeed = 80;
	public float dashCooldown = 1f;

	[Header("Kiki Dash")]
	public float dashEndDuration = 0.2f;

	[Header("GD Dash")]
	public float dashThreshold;
	public float timeToMaxDuration;
	public float dashMinDuration;
	public float dashMaxDuration;

	private Rigidbody _rigidbody;
	private Vector3 _movement;
	private float dashSpeedTemp;

	void Start () 
	{
		_rigidbody = GetComponent<Rigidbody> ();
		rewiredPlayer = ReInput.players.GetPlayer(controllerNumber);
	}
	
	void Update () 
	{
		//Movement Vector
		_movement = new Vector3(rewiredPlayer.GetAxisRaw("Move Horizontal"), 0f, rewiredPlayer.GetAxisRaw("Move Vertical"));
		_movement.Normalize();

		DashInput ();

		LookForward ();
	}

	void DashInput ()
	{
		if(kikiDashEnabled)
		{
			//Dash
			if (rewiredPlayer.GetButtonDown("Action 1") && dashState == DashState.CanDash && _movement != Vector3.zero)
				StartCoroutine(Dash());
			
			if (rewiredPlayer.GetButtonUp("Action 1") && dashState == DashState.Dashing)
				StartCoroutine(DashEnd());
		}
		else
		{
			//Dash
			if (rewiredPlayer.GetButtonDown("Action 1") && dashState == DashState.CanDash)
				StartCoroutine(DashAim());
		}
	}

	void FixedUpdate ()
	{
		Movement ();
	}

	void Movement ()
	{
		//Movement
		if (dashState != DashState.Dashing && dashState != DashState.DashEnd && dashState != DashState.DashAim)
			_rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
	}

	void LookForward ()
	{
		if (dashState != DashState.Dashing && dashState != DashState.DashEnd)
			transform.LookAt (transform.position + _movement);
	}

	protected virtual IEnumerator Dash()
	{
		dashState = DashState.Dashing;

		Vector3 movementTemp = new Vector3(rewiredPlayer.GetAxisRaw("Move Horizontal"), 0f, rewiredPlayer.GetAxisRaw("Move Vertical"));
		movementTemp = movementTemp.normalized;

		dashSpeedTemp = dashSpeed;

		while (dashState != DashState.Cooldown)
		{
			_rigidbody.velocity = movementTemp * dashSpeedTemp;

			yield return new WaitForFixedUpdate();
		}
	}

	protected virtual IEnumerator DashEnd()
	{
		dashState = DashState.DashEnd;

		DOTween.To (()=> dashSpeedTemp, x=> dashSpeedTemp = x, 0, dashEndDuration).SetEase (Ease.OutQuad);

		yield return new WaitForSeconds (dashEndDuration);

		dashState = DashState.Cooldown;

		yield return new WaitForSeconds(dashCooldown);

		dashState = DashState.CanDash;
	}

	protected virtual IEnumerator DashAim ()
	{
		dashState = DashState.DashAim;

		float holdTime = 0;

		while(rewiredPlayer.GetButton ("Action 1"))
		{
			holdTime = rewiredPlayer.GetButtonTimePressed ("Action 1");

			if (holdTime >= timeToMaxDuration)
				break;
			
			yield return new WaitForEndOfFrame ();
		}

		Debug.Log (holdTime);

		if(holdTime < dashThreshold)
		{
			Debug.Log ("Small Dash");

			dashState = DashState.Dashing;

			Vector3 movementTemp = transform.forward;

			float dashSpeedTemp = dashSpeed;
			float futureTime = Time.time + dashMinDuration;
			float start = futureTime - Time.time;

			//DOVirtual.DelayedCall (dashMinDuration, ()=> StartCoroutine(DashEnd()));

			float duration = 0;

			while (duration <= dashMinDuration)
			{
				duration += Time.fixedDeltaTime;
				_rigidbody.velocity = movementTemp * dashSpeedTemp;
				yield return new WaitForFixedUpdate();
			}
		}
		else
		{
			Debug.Log ("Big Dash");

			if (holdTime > timeToMaxDuration)
				holdTime = timeToMaxDuration;

			dashState = DashState.Dashing;

			Vector3 movementTemp = transform.forward;

			Debug.Log ("% : " + (holdTime / timeToMaxDuration));
			Debug.Log ("Force : " + (holdTime / timeToMaxDuration) * dashMaxDuration + dashMinDuration);

			float dashSpeedTemp = dashSpeed;
			float futureTime = Time.time + (holdTime / timeToMaxDuration) * dashMaxDuration + dashMinDuration;
			float start = futureTime - Time.time;

			DOVirtual.DelayedCall ((holdTime / timeToMaxDuration) * dashMaxDuration + dashMinDuration, ()=> StartCoroutine(DashEnd()));

			while (Time.time <= futureTime)
			{
				_rigidbody.velocity = movementTemp * dashSpeedTemp * Time.fixedDeltaTime;
				yield return new WaitForFixedUpdate();
			}
		}

		dashState = DashState.CanDash;
	}
}
