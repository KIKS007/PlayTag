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

public enum PoopState
{
    CanPoop,
    Cooldown
}

public class Cat : MonoBehaviour 
{
	[Header("Controller Number")]
	public int controllerNumber = -1;
	public Player rewiredPlayer; // The Rewired Player

	[Header("States")]
	public CatState catstate = CatState.Normal;
	public DashState dashState = DashState.CanDash;
    public PoopState poopState = PoopState.CanPoop;

	[Header("Movement")]
	public float speed = 18;
	public float aimingSpeed = 5f;
	public float gravity = 10;

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
	public float dashCooldown = 1f;

    [Header("Poop")]
    public GameObject poopPrefab;
    public float poopCooldown = 3f;

	[Header("Dash Target")]
	public LayerMask wallMask = 1 << 10;
	public Transform dashTarget;

	[Header("Dash Line Renderer")]
	public LineRenderer dashLineRenderer;

    [HideInInspector]
    public float speedBoost;

    private Rigidbody _rigidbody;
	private Vector3 _movement;
	private float _dashSpeedTemp;
	private Vector3 _previousMovement;
	private CatState _previousCatState;

	public event EventHandler OnDash;
	public event EventHandler OnDashAiming;
	public event EventHandler OnDashEnd;
	public event EventHandler OnMoving;
	public event EventHandler OnStopMoving;
	public event EventHandler OnStunned;
	public event EventHandler OnStunnedEnd;
	public event EventHandler OnHit;

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
		if (GameManager.Instance.gameState != GameState.Victory && GameManager.Instance.gameState != GameState.Pause)
		{
			//Movement Vector
			_movement = new Vector3(rewiredPlayer.GetAxisRaw("Move Horizontal"), 0f, rewiredPlayer.GetAxisRaw("Move Vertical"));
			_movement.Normalize();

			LookForward ();
		}

		if (GameManager.Instance.gameState != GameState.Playing)
			return;

		//Dash
		if (rewiredPlayer.GetButtonDown("Action 1") && dashState == DashState.CanDash && catstate != CatState.Stunned)
			StartCoroutine(DashAim());
		

        //Poop
        if (rewiredPlayer.GetButtonDown("Action 2") && poopState == PoopState.CanPoop)
            StartCoroutine(Poop());

        LookForward ();

		if(_previousMovement != _movement)
		{
			if(_previousMovement == Vector3.zero && _movement != Vector3.zero && catstate != CatState.Stunned)
			{
				if (OnMoving != null)
					OnMoving ();
			}
			if(_previousMovement != Vector3.zero && _movement == Vector3.zero || catstate == CatState.Stunned && _previousCatState != CatState.Stunned)
			{
				if (OnStopMoving != null)
					OnStopMoving ();
			}
		}

		_previousCatState = catstate;
		_previousMovement = _movement;
	}

	void FixedUpdate ()
	{
		if (GameManager.Instance.gameState != GameState.Playing)
			return;
		
		Movement ();

		Gravity ();
	}

	void Gravity ()
	{
		_rigidbody.AddForce (Vector3.down * gravity, ForceMode.Acceleration);
	}

	void Movement ()
	{
		if (catstate == CatState.Stunned)
			return;

		//Movement
		if (dashState != DashState.Dashing && dashState != DashState.DashEnd && dashState != DashState.DashAim)
			_rigidbody.MovePosition (_rigidbody.position + _movement * (speed + speedBoost) * Time.fixedDeltaTime);

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

		if (OnDashAiming != null)
			OnDashAiming ();

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

			dashLineRenderer.SetPosition (0, new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z) - transform.forward * 0.3f);
			dashLineRenderer.SetPosition (1, dashTarget.position);

			yield return new WaitForEndOfFrame ();
		}

		dashLineRenderer.gameObject.SetActive (false);
		dashTarget.gameObject.SetActive (false);

		//Debug.Log (holdTime);

		StartCoroutine (Dash ());
	}

    IEnumerator Poop()
    {
        poopState = PoopState.Cooldown;

        GameObject poop = Instantiate(poopPrefab, TournamentManager.Instance.playersParent.transform);
        poop.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        yield return new WaitForSeconds(poopCooldown);

        poopState = PoopState.CanPoop;
    }

	protected virtual IEnumerator Dash ()
	{
		_dashSpeedTemp = dashSpeed;

		if (OnDash != null)
			OnDash ();

		dashState = DashState.Dashing;

		float distance = Vector3.Distance (transform.position, dashTarget.position);
		float duration = distance / dashSpeed;

		DOVirtual.DelayedCall (duration * timeReductionFactor, ()=> dashState = DashState.Cooldown);

		Vector3 dashTargetTemp = dashTarget.position;
		Vector3 movementTemp = (dashTargetTemp - transform.position).normalized;

		while (dashState != DashState.Cooldown)
		{
			if (catstate == CatState.Stunned)
				yield break;

			movementTemp = dashTargetTemp - transform.position;

			if(_rigidbody.velocity.magnitude < 1)
				movementTemp.Normalize ();

			//if (Vector3.Distance (transform.position, dashTargetTemp) > 0.5f)
				
			_rigidbody.velocity = movementTemp * _dashSpeedTemp;
			yield return new WaitForFixedUpdate();
		}

		_rigidbody.velocity = Vector3.zero;

		if (OnDashEnd != null)
			OnDashEnd ();

		yield return new WaitForSeconds(dashCooldown);

		dashState = DashState.CanDash;
	}

	public virtual IEnumerator Stun ()
	{
		catstate = CatState.Stunned;

		if (OnStunned != null)
			OnStunned ();
		
		StopCoroutine (DashAim ());
		StopCoroutine (Dash ());

		dashState = DashState.CanDash;
		dashLineRenderer.gameObject.SetActive (false);
		dashTarget.gameObject.SetActive (false);


		Color initialColor = GetComponent<Renderer> ().material.color;
		GetComponent<Renderer> ().material.color = Color.black;

		yield return new WaitForSeconds (stunDuration);

		if (OnMoving != null)
			OnMoving ();

		if (OnStunnedEnd != null)
			OnStunnedEnd ();

		GetComponent<Renderer> ().material.color = initialColor;

		dashState = DashState.CanDash;
		catstate = CatState.Normal;
	}

	public void OnHitVoid ()
	{
		if (OnHit != null)
			OnHit ();
	}
}
