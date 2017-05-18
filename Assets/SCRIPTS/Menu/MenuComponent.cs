using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuComponent : MonoBehaviour 
{
	[Header ("Selectable")]
	public GameObject selectable;

	[Header ("Previous Menu")]
	public MenuComponent previousMenu;

	[Header ("Above Menu")]
	public MenuComponent aboveMenu;

	[Header ("Below Menu")]
	public List<MenuComponent> belowMenu;

	[HideInInspector]
	public GameObject _previousSelection;
	[HideInInspector]
	public RectTransform _rect;

	// Use this for initialization
	void Awake () 
	{
		_rect = GetComponent<RectTransform> ();

		aboveMenu = transform.GetComponentInParent<MenuComponent> ();

		foreach (Transform t in transform)
			if (t.GetComponent<MenuComponent> () != null)
				belowMenu.Add (t.GetComponent<MenuComponent> ());
	}
}
