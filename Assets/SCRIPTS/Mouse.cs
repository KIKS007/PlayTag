using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public enum MouseState
{
	Dead
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

	[Header("Movement")]
	public float speed = 16;

	[Header("Push")]
	public float pushForce = 150f;
	public float pushCooldown = 1f;

	private Rigidbody _rigidbody;
    private List<Rigidbody> _pushableList = new List<Rigidbody>();
    private Button _button;

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

        //Push
        if (rewiredPlayer.GetButtonDown("Action 1") && pushState == PushState.CanPush)
            Push();

		LookForward ();
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
    }
}
