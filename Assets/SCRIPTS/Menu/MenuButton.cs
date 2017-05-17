using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
		if (!GetComponent<Button> ().IsInteractable ())
			return;
		
		if (targetMenu != null)
			MenuManager.Instance.ShowMenu (targetMenu, canComeBack);

		SoundsManager.Instance.PlaySound (SoundsManager.Instance.buttonSubmit);
	}
}
