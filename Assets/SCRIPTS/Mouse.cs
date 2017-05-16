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
	public float pushForce = 150f;
	public float pushCooldown = 1f;

	[Header("Wrap")]
	public float xWidth;
	public float yWidth;

	private Rigidbody _rigidbody;
    private List<Rigidbody> _pushableList = new List<Rigidbody>();
    private Button _button;
	private float _dashSpeedTemp;

	[HideInInspector]
	public Vector3 _movement;

	void Start () 
	{
		_rigidbody = GetComponent<Rigidbody> ();
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

        Wrap ();
	}

	void FixedUpdate ()
	{
		Movement ();
	}

	void Movement ()
	{
		//Movement
		if (pushState != PushState.Pushing)
			_rigidbody.MovePosition(_rigidbody.position + _movement * speed * Time.fixedDeltaTime);
	}

	void LookForward ()
	{
		transform.LookAt (transform.position + _movement);
	}


	void Push ()
	{
		pushState = PushState.Pushing;

        //button
        if (_button != null)
            _button.Activate();

        //movable
        foreach(Rigidbody rb in _pushableList)
        {
            rb.AddForce((rb.position - transform.position).normalized * pushForce, ForceMode.Impulse);
        }

		StartCoroutine(PushEnd());
	}

    //cooldown
	protected virtual IEnumerator PushEnd ()
	{
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
            _button = col.GetComponent<Button>();
        }

        //movable
        if (col.gameObject.layer == 8)
        {
            _pushableList.Add(col.GetComponent<Rigidbody>());
        }

		//Cat
		if (col.tag == "Cat")
		{
			_pushableList.Add(col.GetComponent<Rigidbody>());
		}
    }

    void OnTriggerExit (Collider col)
    {
        //button
        if(col.tag == "Button")
        {
            _button = null;
        }

        //movable
        if (col.gameObject.layer == 8)
        {
            _pushableList.Remove(col.GetComponent<Rigidbody>());
        }

		//Cat
		if (col.tag == "Cat")
		{
			_pushableList.Remove(col.GetComponent<Rigidbody>());
		}
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

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Cat")
        {
            if (mouseState == MouseState.Normal)
            {
                mouseState = MouseState.Frozen;
                _movement = Vector3.zero;
                GameManager.Instance.CheckMouse();
            }
        }

        if (col.gameObject.tag == "Mouse")
        {
            if (mouseState == MouseState.Frozen)
            {
                mouseState = MouseState.Normal;
            }
        }
    }
}
