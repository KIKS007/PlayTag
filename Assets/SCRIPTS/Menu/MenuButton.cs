using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, ISubmitHandler, IPointerClickHandler
{
	[Header ("Target Menu")]
	public MenuComponent targetMenu;
	[HideInInspector]
	public MenuComponent currentMenu;

	// Use this for initialization
	void Start () 
	{
		if(currentMenu == null)
			currentMenu = transform.GetComponentInParent<MenuComponent> ();
	}

	public void OnSubmit (BaseEventData eventData)
	{
		Show ();
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		Show ();
	}

	void Show ()
	{
		if (targetMenu != null)
			MenuManager.Instance.ShowMenu (currentMenu, targetMenu);
		else
			Debug.LogWarning ("No Target Menu on " + gameObject.name);
	}
}
