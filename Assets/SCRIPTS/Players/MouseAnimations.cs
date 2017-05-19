using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseAnimations : MonoBehaviour 
{
	public GameObject[] mousesMeshes;
	public ParticleSystem dashFX;
	public Color dashOff = Color.red;
	public Color frozenColor = Color.black;

	private Mouse _mouseScript;
	private Animator _mouseAnimator;
	private ParticleSystem _attackFX;
	private Color dashOn;
	private Color initialColor;
	public Renderer rend;

	// Use this for initialization
	void Start () 
	{
		_mouseScript = GetComponent<Mouse> ();

		foreach (GameObject m in mousesMeshes)
			m.SetActive (false);

		for (int i = 0; i < mousesMeshes.Length; i++)
		{
			if (i == _mouseScript.controllerNumber)
			{
				mousesMeshes [i].SetActive (true);
				rend = mousesMeshes [i].transform.GetComponentInChildren<Renderer> ();
			}
				
			else
				Destroy (mousesMeshes [i]);
		}

		dashOn = rend.material.GetColor ("_emmit_color");
		initialColor = rend.material.GetColor ("_Colorforce");

		_mouseAnimator = transform.GetComponentInChildren <Animator> ();
		_attackFX = _mouseAnimator.transform.GetComponentInChildren <ParticleSystem> ();

		_mouseScript.OnAttack += () =>
		{ 
			GameObject fx = Instantiate (_attackFX.gameObject, _attackFX.transform.position, _attackFX.transform.rotation, GameManager.Instance._playersParent) as GameObject;

			fx.GetComponent<ParticleSystem> ().Play ();
			_mouseAnimator.SetTrigger ("attack");
		};

		_mouseScript.OnFrozen += () => _mouseAnimator.SetTrigger ("freeze");
		_mouseScript.OnUnfrozen += () => _mouseAnimator.SetTrigger ("iddle");
		_mouseScript.OnDash += () => _mouseAnimator.SetTrigger ("dash");

		_mouseScript.OnFrozen += () => rend.material.SetColor ("_Colorforce", frozenColor);
		_mouseScript.OnUnfrozen += () => rend.material.SetColor ("_Colorforce", initialColor);


		_mouseScript.OnDash += () => dashFX.Play ();

		_mouseScript.OnDash += () => rend.material.SetColor ("_emmit_color", dashOff);

		_mouseScript.OnCanDash += () => rend.material.SetColor ("_emmit_color", dashOn);


		_mouseScript.OnDashEnd += () => _mouseAnimator.SetTrigger ("iddle");

		_mouseScript.OnMoving += () => _mouseAnimator.SetBool ("walking", true);
		_mouseScript.OnStopMoving += () => _mouseAnimator.SetBool ("walking", false);
	}
}
