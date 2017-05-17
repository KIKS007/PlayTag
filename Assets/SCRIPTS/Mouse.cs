using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;

public enum MouseState
{
    Normal,
	Frozen
}

public enum PushState
{
	CanPush,
	Pushing,
	Cooldown
}

public class Mouse : MonoBehaviour 
{
	[Header("Controller Number")]
	public int controllerNumber = -1;
	[HideInInspector]
	public Player rewiredPlayer; // The Rewired Player

	[Header("States")]
	public PushState pushState = PushState.CanPush;
    public MouseState mouseState = MouseState.Normal;
	public DashState dashState = DashState.CanDash;

	[Header("Dash")]
	public float dashSpeed = 80;
	public float dashDuration;
	public float dashCooldown = 1f;

	[Header("Movement")]
	public float speed = 16;

	[Header("Push")]
	public float pushDuration = 0.5f;
	public float pushForce = 150f;
	public float pushCooldown = 1f;
    public GameObject pushAOE;

	private Rigidbody _rigidbody;
    private MeshRenderer _rend;
    private Color _tempColor = new Color();
	private float _dashSpeedTemp;

	[HideInInspector]
	public Vector3 _movement;

	void Start () 
	{
		_rigidbody = GetComponent<Rigidbody> ();
        _rend = GetComponent<MeshRenderer>();
		rewiredPlayer = ReInput.players.GetPlayer(controllerNumber);
	}

	public void SetupRewired (int number)
	{
		controllerNumber = number;
		rewiredPlayer = ReInput.players.GetPlayer(controllerNumber);
	}
	
	void Update () 
	{
        if (mouseState == MouseState.Normal)
        {
            //Movement Vector
            _movement = new Vector3(rewiredPlayer.GetAxisRaw("Move Horizontal"), 0f, rewiredPlayer.GetAxisRaw("Move Vertical"));
            _movement.Normalize();

            //Push
            if (rewiredPlayer.GetButtonDown("Action 2") && pushState == PushState.CanPush)
                Push();

			//Dash
			if (rewiredPlayer.GetButtonDown("Action 1") && dashState == DashState.CanDash)
				StartCoroutine(Dash());

            LookForward();
        }
	}

	void FixedUpdate ()
	{
		Movement ();
	}

	void Movement ()
	{
		//Movement
		_rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
	}

	void LookForward ()
	{
		transform.LookAt (transform.position + _movement);
	}


	void Push ()
	{
		pushState = PushState.Pushing;
        pushAOE.SetActive(true);


		StartCoroutine(PushEnd());
	}

    //cooldown
	protected virtual IEnumerator PushEnd ()
	{
		yield return new WaitForSeconds(pushDuration);
		
		pushAOE.SetActive(false);

		pushState = PushState.Cooldown;

		yield return new WaitForSeconds(pushCooldown);

		pushState = PushState.CanPush;
	}

	protected virtual IEnumerator Dash()
	{
		dashState = DashState.Dashing;

		_dashSpeedTemp = dashSpeed;

		DOTween.To (()=> _dashSpeedTemp, x=> _dashSpeedTemp = x, 0, dashDuration).SetEase (Ease.OutQuad);

		while (_dashSpeedTemp != 0)
		{
			_rigidbody.velocity = transform.forward * _dashSpeedTemp;

			yield return new WaitForFixedUpdate();
		}

		dashState = DashState.Cooldown;

		yield return new WaitForSeconds (dashCooldown);

		dashState = DashState.CanDash;
	}

    //PushTrigger
    void OnTriggerEnter (Collider col)
    {
        //button
        if (col.tag == "Button")
        {
            col.GetComponent<Interrupter>().Activate(); 
        }

        //movable
        if (col.gameObject.layer == 8)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            rb.AddForce((rb.position - transform.position).normalized * pushForce, ForceMode.Impulse);
        }

		//Cat
		if (col.tag == "Cat")
		{
			if (col.GetComponent<Cat> ().catstate == CatState.Stunned)
				return;
			
			Rigidbody rb = col.GetComponent<Rigidbody>();
            rb.AddForce((rb.position - transform.position).normalized * pushForce, ForceMode.Impulse);
            col.GetComponent<Cat>().StartCoroutine("Stun");
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Cat")
        {
			if (mouseState == MouseState.Normal && col.gameObject.GetComponent<Cat> ().catstate != CatState.Stunned)
            {
                mouseState = MouseState.Frozen;
                _movement = Vector3.zero;

                //frozen color change
                _tempColor = _rend.material.color;
                _rend.material.color = Color.blue;

                GameManager.Instance.CheckMouse();
            }
        }

        if (col.gameObject.tag == "Mouse")
        {
            if (mouseState == MouseState.Frozen)
            {
                _rend.material.color = _tempColor;

                mouseState = MouseState.Normal;
            }
        }
    }
}
