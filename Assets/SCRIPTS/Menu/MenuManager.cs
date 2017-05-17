using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Rewired;
using UnityEngine.EventSystems;

public class MenuManager : Singleton<MenuManager> 
{
	[Header ("Menu Animations")]
	public Ease menuEase;
	public Vector2 offScreenPosition;
	public Vector2 onScreenPosition;
	public float menuAnimationDuration = 0.5f;

	[Header ("Menus")]
	public EventSystem eventSystem;
	public GameObject menuCanvas;
	public GameObject timerCanvas;
	public MenuComponent mainMenu;
	public RectTransform pauseMenu;

	[Header ("Menu State")]
	public MenuComponent currentMenu;
	public MenuComponent previousMenu;

	public Player _rewiredPlayer1;

	// Use this for initialization
	void Start () 
	{
		GameManager.Instance.gameState = GameState.Menu;

		//Dont forget to put keyboard on 1st player
		_rewiredPlayer1 = ReInput.players.GetPlayer(0);

		HideAll ();

		ShowInstantMenu (mainMenu);
	}

	// Update is called once per frame
	void Update ()
	{
		BackInput ();

		PauseInput ();

		NothingSelected ();
	}

	void NothingSelected ()
	{
		if (GameManager.Instance.gameState == GameState.Playing)
			return;

		if (eventSystem.currentSelectedGameObject == null && currentMenu != null && currentMenu.selectable != null)
			eventSystem.SetSelectedGameObject (currentMenu.selectable);
	}

	void BackInput ()
	{
		if(GameManager.Instance.gameState == GameState.Menu)
		{
			if (_rewiredPlayer1.GetButtonDown ("Back") && previousMenu != null)
				ShowMenu (currentMenu, previousMenu);
		}
	}

	public void PauseInput ()
	{
		if (_rewiredPlayer1.GetButtonDown ("Pause"))
		{
			if(GameManager.Instance.gameState == GameState.Playing)
			{
				//Pause
				Time.timeScale = 0;
				pauseMenu.gameObject.SetActive (true);

				pauseMenu.DOAnchorPos (onScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> 
					{
						if(pauseMenu.GetComponent<MenuComponent> ()._previousSelection != null)
						{
							eventSystem.SetSelectedGameObject (null);
							eventSystem.SetSelectedGameObject (pauseMenu.GetComponent<MenuComponent> ()._previousSelection);
						}
					}).SetUpdate (true);

			}

			else if(GameManager.Instance.gameState == GameState.Pause)
			{
				//Unpause
				Time.timeScale = 0;
				pauseMenu.gameObject.SetActive (true);

				pauseMenu.DOAnchorPos (offScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> 
					{
						eventSystem.SetSelectedGameObject (null);
						Time.timeScale = 1;
						pauseMenu.gameObject.SetActive (false);
					}).SetUpdate (true);
			}
		}
	}

	public void StartGame ()
	{
		Hide (currentMenu);

		DOVirtual.DelayedCall (menuAnimationDuration, ()=> 
			{
				timerCanvas.gameObject.SetActive (true);
				TournamentManager.Instance.StartGame ();
			}).SetUpdate (true);
	}

	public void HideAll ()
	{
		foreach (Transform t in menuCanvas.transform)
		{
			t.gameObject.SetActive (false);
			t.GetComponent<RectTransform> ().anchoredPosition = offScreenPosition;
		}
	}

	public void ShowInstantMenu (MenuComponent menu)
	{
		menu.gameObject.SetActive (true);
		menu.GetComponent<RectTransform> ().anchoredPosition = onScreenPosition;
	}

	public void MainMenu ()
	{
		ShowMenu (mainMenu);
		Time.timeScale = 1;
		GameManager.Instance.gameState = GameState.Menu;
		timerCanvas.gameObject.SetActive (false);
	}

	public void ShowMenu (MenuComponent currentMenu, MenuComponent targetMenu)
	{
		currentMenu._previousSelection = eventSystem.currentSelectedGameObject;

		previousMenu = currentMenu;
		this.currentMenu = targetMenu;

		targetMenu.gameObject.SetActive (true);

		currentMenu._rect.DOAnchorPos (offScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=>
		{
				targetMenu._rect.DOAnchorPos (onScreenPosition, menuAnimationDuration).SetEase (menuEase);
				currentMenu.gameObject.SetActive (false);

				if(targetMenu._previousSelection != null)
				{
					eventSystem.SetSelectedGameObject (null);
					eventSystem.SetSelectedGameObject (targetMenu._previousSelection);
				}

				else if(targetMenu.selectable != null)
				{
					eventSystem.SetSelectedGameObject (null);
					eventSystem.SetSelectedGameObject (targetMenu.selectable);
				}
			}).SetUpdate (true);
	}

	public void ShowMenu (MenuComponent targetMenu)
	{
		this.currentMenu = targetMenu;

		targetMenu.gameObject.SetActive (true);

		targetMenu._rect.DOAnchorPos (onScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=>
			{
				if(targetMenu._previousSelection != null)
				{
					eventSystem.SetSelectedGameObject (null);
					eventSystem.SetSelectedGameObject (targetMenu._previousSelection);
				}

				else if(targetMenu.selectable != null)
				{
					eventSystem.SetSelectedGameObject (null);
					eventSystem.SetSelectedGameObject (targetMenu.selectable);
				}
			}).SetUpdate (true);
	}


	public void Hide (MenuComponent targetMenu)
	{
		targetMenu._previousSelection = eventSystem.currentSelectedGameObject;

		targetMenu._rect.DOAnchorPos (offScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=>
			{
				targetMenu.gameObject.SetActive (false);
				eventSystem.SetSelectedGameObject (null);
				this.currentMenu = null;
			}).SetUpdate (true);
	}

	public void Quit ()
	{
		Application.Quit ();
	}
}
