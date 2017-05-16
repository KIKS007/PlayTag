using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Behavior_Rotate : Behavior
{
	[Header ("Rotate")]
	public Vector3 relativeRotation;

	protected override void Init ()
	{
		base.Init ();

		if (playerOnStart)
			PlayForwards ();
	}

	public override void PlayForwards ()
	{
		base.PlayForwards ();

		_tween = _rigidbody.DORotate (relativeRotation, speed).SetSpeedBased ().SetRelative ().SetEase (ease).SetLoops (loopCount, loopType).OnStepComplete (()=> 
			{
				OnCallbackToggle (onCompleteToggle);

				OnCallbackPlay (onCompletePlay);
				
			}).SetAutoKill (false);
	}
}
