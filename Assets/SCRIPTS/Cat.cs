using System.Collections;
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
	public Transform dashTarget;
	public float dashSpeed = 80;
	public float timeToMaxDuration;
	public float dashMinDuration;
	public float dashMaxDuration;

	[Header("Dash End")]
	public float dashEndDuration = 0.2f;
	public float dashCooldown = 1f;

	[Header("Dash Line Renderer")]
	public LineRenderer dashLineRenderer;
	public float lengthFactor = 1;

	[Header("Wrap")]
	public float xWidth;
	public float yWidth;

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

		Wrap ();
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

	/*protected virtual IEnumerator DashAim ()
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

		dashLineRenderer.gameObject.SetActive (false);

		//Debug.Log (holdTime);

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

		//Debug.Log ("Duration : " + duration);
		//Debug.Log ("% : " + (holdTime / timeToMaxDuration));

		DOVirtual.DelayedCall (duration, ()=> StartCoroutine(DashEnd()));

		while (dashState != DashState.Cooldown)
		{
			_rigidbody.velocity = movementTemp * _dashSpeedTemp;
			_dashSpeedTemp *= dashingAddedSpeed;
			yield return new WaitForFixedUpdate();
		}
	}*/

	protected virtual IEnumerator DashAim ()
	{
        dashState = DashState.DashAim;

		float holdTime = 0;

		dashLineRenderer.gameObject.SetActive (true);
		dashTarget.gameObject.SetActive (true);

		while(rewiredPlayer.GetButton ("Action 1"))
		{
			holdTime = rewiredPlayer.GetButtonTimePressed ("Action 1");

			if (holdTime >= timeToMaxDuration)
				break;

			dashTarget.position = transform.position + transform.forward * lengthFactor * (holdTime / timeToMaxDuration);

			dashLineRenderer.SetPosition (0, transform.position);
			dashLineRenderer.SetPosition (1, dashTarget.position);

			yield return new WaitForEndOfFrame ();
		}

		dashLineRenderer.gameObject.SetActive (false);
		dashTarget.gameObject.SetActive (false);

		//Debug.Log (holdTime);

		_dashSpeedTemp = dashSpeed;

		dashState = DashState.Dashing;


		if (holdTime > timeToMaxDuration)
			holdTime = timeToMaxDuration;

		float duration = (holdTime / timeToMaxDuration) * dashMaxDuration;

		//		if (duration < dashMinDuration)
		//			duration = dashMinDuration;
		//		

		if (duration > dashMaxDuration)
			duration = dashMaxDuration;

		//Debug.Log ("Duration : " + duration);
		//Debug.Log ("% : " + (holdTime / timeToMaxDuration));

		float distance = Vector3.Distance (transform.position, dashTarget.position);
		duration = distance / dashSpeed;

		DOVirtual.DelayedCall (duration, ()=> StartCoroutine(DashEnd()));

		DOTween.To (()=> _dashSpeedTemp, x=> _dashSpeedTemp = x, 0, duration).SetEase (Ease.Linear);

		Vector3 dashTargetTemp = dashTarget.position;
		Vector3 movementTemp = dashTargetTemp - transform.position;

		while (dashState != DashState.Cooldown)
		{
			movementTemp = dashTargetTemp - transform.position;
			_rigidbody.velocity = movementTemp * _dashSpeedTemp;
			yield return new WaitForFixedUpdate();
		}
	}

	protected virtual IEnumerator DashEnd()
	{
		dashState = DashState.DashEnd;

		//DOTween.To (()=> _dashSpeedTemp, x=> _dashSpeedTemp = x, 0, dashEndDuration).SetEase (Ease.OutQuad);

		yield return new WaitForSeconds (dashEndDuration);

		dashState = DashState.Cooldown;

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

	void Wrap ()
	{
		if (transform.position.x < -xWidth)
			transform.DOMoveX (xWidth, 0);
		else if(transform.position.x > xWidth)
			transform.DOMoveX (-xWidth, 0);

		if (transform.position.z < -yWidth)
			transform.DOMoveZ (yWidth, 0);
		else if(transform.position.z > yWidth)
			transform.DOMoveZ (-yWidth, 0);
	}
}
