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
	public float aimingSpeed = 5f;
	public float dashingAddedSpeed = 0.5f;

	[Header("Dash")]
	public bool kikiDashEnabled = true;
	public float dashSpeed = 80;
	public float dashCooldown = 1f;

	[Header("Kiki Dash")]
	public float dashEndDuration = 0.2f;

	[Header("GD Dash")]
	public float timeToMaxDuration;
	public float dashMinDuration;
	public float dashMaxDuration;

	[Header("Dash Line Renderer")]
	public LineRenderer dashLineRenderer;
	public float lengthFactor = 1;

	private Rigidbody _rigidbody;
	private Vector3 _movement;
	private float _dashSpeedTemp;

	void Start () 
	{
		_rigidbody = GetComponent<Rigidbody> ();
		rewiredPlayer = ReInput.players.GetPlayer(controllerNumber);
		dashLineRenderer.gameObject.SetActive (false);
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

		if (dashState == DashState.DashAim)
			_rigidbody.MovePosition(_rigidbody.position + _movement * aimingSpeed * Time.fixedDeltaTime);
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

		_dashSpeedTemp = dashSpeed;

		while (dashState != DashState.Cooldown)
		{
			_rigidbody.velocity = movementTemp * _dashSpeedTemp;

			yield return new WaitForFixedUpdate();
		}
	}

	protected virtual IEnumerator DashEnd()
	{
		dashState = DashState.DashEnd;

		DOTween.To (()=> _dashSpeedTemp, x=> _dashSpeedTemp = x, 0, dashEndDuration).SetEase (Ease.OutQuad);

		yield return new WaitForSeconds (dashEndDuration);

		dashState = DashState.Cooldown;

		yield return new WaitForSeconds(dashCooldown);

		dashState = DashState.CanDash;
	}

	protected virtual IEnumerator DashAim ()
	{
		dashState = DashState.DashAim;

		float holdTime = 0;

		dashLineRenderer.gameObject.SetActive (true);

		while(rewiredPlayer.GetButton ("Action 1"))
		{
			holdTime = rewiredPlayer.GetButtonTimePressed ("Action 1");

			if (holdTime >= timeToMaxDuration)
				break;

			dashLineRenderer.SetPosition (0, transform.position);
			dashLineRenderer.SetPosition (1, transform.position + transform.forward * lengthFactor * (holdTime / timeToMaxDuration));

			yield return new WaitForEndOfFrame ();
		}

		Debug.Log (transform.position);

		dashLineRenderer.gameObject.SetActive (false);

		Debug.Log (holdTime);

		_dashSpeedTemp = dashSpeed;
		
		dashState = DashState.Dashing;
		
		Vector3 movementTemp = transform.forward;

		if (holdTime > timeToMaxDuration)
			holdTime = timeToMaxDuration;

		float duration = (holdTime / timeToMaxDuration) * dashMaxDuration;
		
//		if (duration < dashMinDuration)
//			duration = dashMinDuration;
//		
		if (duration > dashMaxDuration)
			duration = dashMaxDuration;

		Debug.Log ("Duration : " + duration);
		Debug.Log ("% : " + (holdTime / timeToMaxDuration));
		
		DOVirtual.DelayedCall (duration, ()=> StartCoroutine(DashEnd()));
		
		while (dashState != DashState.Cooldown)
		{
			_rigidbody.velocity = movementTemp * _dashSpeedTemp;
			_dashSpeedTemp *= dashingAddedSpeed;
			yield return new WaitForFixedUpdate();
		}

		Debug.Log (transform.position);
	}
}
