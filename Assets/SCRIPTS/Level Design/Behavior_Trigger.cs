using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Trigger : MonoBehaviour 
{
	public enum TriggerType { Toggle, Interrupter }
	public TriggerType triggerType;

	[Header ("Start")]
	public bool activatedOnStart = true;

	[Header ("Activation")]
	public bool cat = true;
	public bool mouses = true;

	[Header ("Behaviors")]
	public List<Behavior> behaviorsList = new List<Behavior> ();

	[Header ("Interactions")]
	public List<Behavior_Trigger> toggleList;
	public List<Behavior_Trigger> activateList;
	public List<Behavior_Trigger> deactivateList;

	// Use this for initialization
	void Start () 
	{
		if (activatedOnStart)
			Activate ();
	}

	void Effect ()
	{
		foreach (Behavior b in behaviorsList)
			if (b != null)
				b.Toggle ();

		foreach (Behavior_Trigger t in toggleList)
			if (t != null)
				t.Toggle ();

		foreach (Behavior_Trigger t in activateList)
			if (t != null)
				t.Activate ();

		foreach (Behavior_Trigger t in deactivateList)
			if (t != null)
				t.Deactivate ();

		if (triggerType == TriggerType.Interrupter)
			Deactivate ();
	}

	public void Activate ()
	{
		gameObject.SetActive (true);
	}

	public void Deactivate ()
	{
		gameObject.SetActive (false);
	}

	public void Toggle ()
	{
		if(gameObject.activeSelf)
			gameObject.SetActive (false);
		else
			gameObject.SetActive (true);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Cat" && cat)
			Effect ();

		if (collision.gameObject.tag == "Mouse" && mouses)
			Effect ();
	}
}
