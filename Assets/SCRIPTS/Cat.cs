using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public enum CatState
{
	Dead
}

public enum DashState
{
	CanDash,
	Dashing,
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
	public float dashSpeed = 80;
	public float dashDuration = 0.2f;
	public float dashCooldown = 1f;
	public AnimationCurve dashEase;

	private Rigidbody _rigidbody;
	[HideInInspector]
	public Vector3 _movement;

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

		//Dash
		if (rewiredPlayer.GetButtonDown("Action 1") && dashState == DashState.CanDash && _movement != Vector3.zero)
			StartCoroutine(Dash());

		LookForward ();
	}

	void FixedUpdate ()
	{
		Movement ();
	}

	void Movement ()
	{
		//Movement
		if (dashState != DashState.Dashing)
			_rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
	}

	void LookForward ()
	{
		transform.LookAt (transform.position + _movement);
	}

	protected virtual IEnumerator Dash()
	{
		dashState = DashState.Dashing;

		Vector3 movementTemp = new Vector3(rewiredPlayer.GetAxisRaw("Move Horizontal"), 0f, rewiredPlayer.GetAxisRaw("Move Vertical"));
		movementTemp = movementTemp.normalized;

		float dashSpeedTemp = dashSpeed;
		float futureTime = Time.time + dashDuration;
		float start = futureTime - Time.time;

		StartCoroutine(DashEnd());

		while (Time.time <= futureTime)
		{
			dashSpeedTemp = dashEase.Evaluate((futureTime - Time.time) / start) * dashSpeed;
			_rigidbody.velocity = movementTemp * dashSpeedTemp * Time.fixedDeltaTime * 200 * 1 / Time.timeScale;

			yield return new WaitForFixedUpdate();
		}
	}

	protected virtual IEnumerator DashEnd()
	{
		yield return new WaitForSeconds(dashDuration);

		dashState = DashState.Cooldown;

		yield return new WaitForSeconds(dashCooldown);

		dashState = DashState.CanDash;
	}
}
