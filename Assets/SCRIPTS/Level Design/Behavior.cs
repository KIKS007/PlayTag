using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent (typeof(Rigidbody))]
public class Behavior : MonoBehaviour 
{
	[Header ("Settings")]
	public float speed = 5;
	public bool playerOnStart = false;

	[Header ("Ease")]
	public Ease ease = Ease.Linear;

	[Header ("Loops")]
	[Range (-1, 50)]
	public int loopCount = 0;
	public LoopType loopType = LoopType.Yoyo;

	[Header ("Test")]
	public bool toggle;

	[Header ("OnComplete")]
	public List<Behavior> onCompleteToggle = new List<Behavior> ();
	public List<Behavior> onCompletePlay = new List<Behavior> ();

	protected Rigidbody _rigidbody;
	protected bool _forwards = false;
	protected Tween _tween = null;

	// Use this for initialization
	protected void Start () 
	{
		_rigidbody = GetComponent<Rigidbody> ();
		_rigidbody.isKinematic = true;
		_rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		gameObject.layer = LayerMask.NameToLayer ("Wall");

		Init ();
	}

	void Update ()
	{
		if(toggle)
		{
			toggle = false;
			Toggle ();
		}
	}

	protected virtual void Init ()
	{
		
	}

	public virtual void PlayForwards ()
	{
		_forwards = true;
	}

	public virtual void Toggle ()
	{
		StopAllCoroutines ();
		StartCoroutine (ToggleCoroutine ());
	}

	IEnumerator ToggleCoroutine ()
	{
		if (_tween != null && _tween.IsPlaying ())
			yield return new WaitWhile (()=>_tween.IsPlaying ());

		if (_forwards)
		{
			if(_tween != null)
			{
				_forwards = false;
				_tween.PlayBackwards ();
			}
		}
		else
		{
			if (_tween != null)
			{
				_forwards = true;
				_tween.PlayForward ();
			}
			else
				PlayForwards ();
		}

		yield return 0;
	}

	protected virtual void OnCallbackToggle (List<Behavior> behaviors)
	{
		if (behaviors.Count == 0)
			return;

		foreach (Behavior b in behaviors)
			if(b != null)
				b.Toggle ();
	}

	protected virtual void OnCallbackPlay (List<Behavior> behaviors)
	{
		if (behaviors.Count == 0)
			return;

		foreach (Behavior b in behaviors)
			if(b != null)
				b.PlayForwards ();
	}
}
