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
	public float gravity = 10;

	[Header("Push")]
	public float pushDuration = 0.5f;
	public float pushForce = 150f;
	public float pushCooldown = 1f;
    public GameObject pushAOE;

    [HideInInspector]
    public float frozenTime = 0f;

    private List<GameObject> _triggered = new List<GameObject>();
	private Rigidbody _rigidbody;
    private MeshRenderer _rend;
    private Color _tempColor = new Color();
	private float _dashSpeedTemp;
    private Flag _flag;

	[HideInInspector]
	public Vector3 _movement;
    public float speedBoost;

	private Vector3 _previousMovement;
	private MouseState _previousMouseState;

	public event EventHandler OnAttack;
	public event EventHandler OnFrozen;
    public event EventHandler OnUnfrozen;
    public event EventHandler OnSave;
    public event EventHandler OnCapture;
	public event EventHandler OnStun;
	public event EventHandler OnDash;
    public event EventHandler OnDashEnd;
	public event EventHandler OnMoving;
	public event EventHandler OnStopMoving;

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
		if (GameManager.Instance.gameState != GameState.Victory && GameManager.Instance.gameState != GameState.Pause)
		{
			//Movement Vector
			_movement = new Vector3(rewiredPlayer.GetAxisRaw("Move Horizontal"), 0f, rewiredPlayer.GetAxisRaw("Move Vertical"));
			_movement.Normalize();

			LookForward ();
		}

		if (GameManager.Instance.gameState != GameState.Playing)
			return;

        if (mouseState == MouseState.Normal)
        {
            //Push
            if (rewiredPlayer.GetButtonDown("Action 2") && pushState == PushState.CanPush)
                Push();

			//Dash
			if (rewiredPlayer.GetButtonDown("Action 1") && dashState == DashState.CanDash)
				StartCoroutine(Dash());
        }

		if(_previousMovement != _movement)
		{
			if(_previousMovement == Vector3.zero && _movement != Vector3.zero && mouseState != MouseState.Frozen)
			{
				if (OnMoving != null)
					OnMoving ();
			}
			if(_previousMovement != Vector3.zero && _movement == Vector3.zero || mouseState == MouseState.Frozen && _previousMouseState != MouseState.Frozen)
			{
				if (OnStopMoving != null)
					OnStopMoving ();
			}
		}

		_previousMouseState = mouseState;
		_previousMovement = _movement;
	}

	void FixedUpdate ()
	{
		if (GameManager.Instance.gameState != GameState.Playing)
			return;
		
		if (mouseState == MouseState.Normal)
			Movement ();

		Gravity ();
	}

	void Gravity ()
	{
		_rigidbody.AddForce (Vector3.down * gravity, ForceMode.Acceleration);
	}

	void Movement ()
	{
		//Movement
		_rigidbody.MovePosition (_rigidbody.position + _movement * (speed + speedBoost) * Time.fixedDeltaTime);
	}

	void LookForward ()
	{
		transform.LookAt (transform.position + _movement);
	}


	void Push ()
	{
		if (OnAttack != null)
			OnAttack();

		pushState = PushState.Pushing;
        _triggered.Clear();
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

		if (OnDash != null)
			OnDash();

		_dashSpeedTemp = dashSpeed;

		DOTween.To (()=> _dashSpeedTemp, x=> _dashSpeedTemp = x, 0, dashDuration).SetEase (Ease.OutQuad);

		while (_dashSpeedTemp != 0)
		{
			_rigidbody.velocity = transform.forward * _dashSpeedTemp;

			yield return new WaitForFixedUpdate();
		}

		dashState = DashState.Cooldown;

		if (OnDashEnd != null)
			OnDashEnd ();

		yield return new WaitForSeconds (dashCooldown);

		dashState = DashState.CanDash;
	}

    //PushTrigger
    void OnTriggerEnter (Collider col)
    {
        foreach(GameObject go in _triggered)
        {
            if (col.gameObject == go)
                return;
        }

        _triggered.Add(col.gameObject);

        //button
        if (col.tag == "Button")
        {
            col.GetComponent<Interrupter>().Activate(); 
        }

        //movable
        if (col.gameObject.layer == LayerMask.NameToLayer("Movable") || col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            rb.AddForce((rb.position - transform.position).normalized * pushForce, ForceMode.Impulse);
        }

		//Cat
		if (col.tag == "Cat")
		{
			if (col.GetComponent<Cat> ().catstate == CatState.Stunned)
				return;
            col.GetComponent<Cat>().StartCoroutine("Stun");

            if (OnStun != null)
                OnStun();
        }

        //flag
        if(col.tag == "Flag")
        {
            _flag = col.GetComponent<Flag>();
        }
    }

    void OnTriggerExit(Collider col)
    {
        //flag
        if (col.tag == "Flag")
        {
            _flag = null;
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

				col.gameObject.GetComponent<Cat>().OnHitVoid ();

                if (OnFrozen != null)
                    OnFrozen();

                if(_flag != null)
                {
                    _flag.mouseList.Remove(this);
                }

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
                col.gameObject.GetComponent<Mouse>().OnSaveVoid();

                _rend.material.color = _tempColor;

				if (OnUnfrozen != null)
					OnUnfrozen ();

                mouseState = MouseState.Normal;
            }
        }

    }

    public void OnSaveVoid ()
    {
        if (OnSave != null)
            OnSave();
    }

    public void OnCaptureVoid()
    {
        if (OnCapture != null)
            OnCapture();
    }
}
