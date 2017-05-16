using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Behavior_Move : Behavior
{
	[Header ("Move")]
	public Vector3 relativePosition;

	protected override void Init ()
	{
		base.Init ();

		if (playerOnStart)
			PlayForwards ();
	}

	public override void PlayForwards ()
	{
		base.PlayForwards ();

		_tween = _rigidbody.DOMove (relativePosition, speed).SetSpeedBased ().SetRelative ().SetEase (ease).SetLoops (loopCount, loopType).OnStepComplete (()=> 
			{
				OnCallbackToggle (onCompleteToggle);

				OnCallbackPlay (onCompletePlay);
				
			}).SetAutoKill (false);
	}
}
