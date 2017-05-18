using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MenuButton : MonoBehaviour, ISubmitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
	[Header ("Target Menu")]
	public MenuComponent targetMenu;
	public bool canComeBack = true;
	private bool _selected;
	private RectTransform _rect;
	private Vector3 _initialScale;
	private Button _button;

	void Start ()
	{
		_rect = GetComponent<RectTransform> ();
		_button = GetComponent<Button> ();
		_initialScale = transform.localScale;
	}

	void Update ()
	{
		if(MenuManager.Instance.eventSystem.currentSelectedGameObject != gameObject && _selected)
			Unselect ();

		if(!_button.IsInteractable () && _selected)
			Unselect ();
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
		if (!GetComponent<Button> ().IsInteractable ())
			return;
		
		if (targetMenu != null)
			MenuManager.Instance.ShowMenu (targetMenu, canComeBack);

		SoundsManager.Instance.PlaySound (SoundsManager.Instance.buttonSubmit);
	}

	public void OnSelect (BaseEventData eventData)
	{

		Select ();
	}

	public void OnDeselect (BaseEventData eventData)
	{
		Unselect ();
	}

	void Select ()
	{
		_selected = true;
		_rect.DOScale (_initialScale * 1.2f, 0.2f).SetEase (Ease.OutQuad);
	}

	void Unselect ()
	{
		_selected = false;
		_rect.DOScale (_initialScale, 0.2f).SetEase (Ease.OutQuad);
	}
}
