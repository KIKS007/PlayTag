using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, ISubmitHandler, IPointerClickHandler
{
	[Header ("Target Menu")]
	public MenuComponent targetMenu;
	public bool canComeBack = true;

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
			MenuManager.Instance.ShowMenu (targetMenu, canComeBack);
		else
			Debug.LogWarning ("No Target Menu on " + gameObject.name);
	}
}
