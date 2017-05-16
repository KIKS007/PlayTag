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
	public PushState pushState= PushState.CanPush;
    public MouseState mouseState = MouseState.Normal;

	[Header("Movement")]
	public float speed = 16;

	[Header("Push")]
	public float pushForce = 150f;
	public float pushCooldown = 1f;
    public GameObject pushAOE;

	[Header("Wrap")]
	public float xWidth;
	public float yWidth;

	private Rigidbody _rigidbody;
    private MeshRenderer _rend;
    private Color _tempColor = new Color();

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
            if (rewiredPlayer.GetButtonDown("Action 1") && pushState == PushState.CanPush)
                Push();

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
        pushAOE.SetActive(true);


		StartCoroutine(PushEnd());
	}

    //cooldown
	protected virtual IEnumerator PushEnd ()
	{
        pushState = PushState.Cooldown;

		yield return new WaitForSeconds(pushCooldown);
        pushAOE.SetActive(false);

		pushState = PushState.CanPush;
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
			Rigidbody rb = col.GetComponent<Rigidbody>();
            rb.AddForce((rb.position - transform.position).normalized * pushForce, ForceMode.Impulse);
            col.GetComponent<Cat>().StartCoroutine("Stun");
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
