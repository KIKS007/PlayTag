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

	[Header ("Mode Start")]
	public float startDelay = 1f;

	[Header ("Menus")]
	public EventSystem eventSystem;
	public GameObject menuCanvas;
	public GameObject timerCanvas;
	public MenuComponent mainMenu;
	public MenuComponent leadeboardMenu;
	public MenuComponent pauseMenu;

	[Header ("End Mode")]
	public float endMenuDelay = 1;
	public float endMenuDuration;
	public MenuComponent endMode;
	public GameObject catWon;
	public GameObject mousesWon;

	[Header ("Menu State")]
	public MenuComponent currentMenu;

	public Player _rewiredPlayer1;

	public event EventHandler OnMainMenu;

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

		if (_rewiredPlayer1.GetButtonDown ("Pause"))
			Pause ();

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
			if (_rewiredPlayer1.GetButtonDown ("Back") && currentMenu != null && currentMenu.previousMenu != null)
			{
				SoundsManager.Instance.PlaySound (SoundsManager.Instance.buttonCancel);
				ShowMenu (currentMenu.previousMenu, false);
			}
		}
	}

	public void Pause ()
	{
		if(GameManager.Instance.gameState == GameState.Playing)
		{
			SoundsManager.Instance.PlaySound (SoundsManager.Instance.buttonSubmit);
			//Pause
			GameManager.Instance.gameState = GameState.Pause;
			Time.timeScale = 0;
			pauseMenu.gameObject.SetActive (true);
			
			pauseMenu.GetComponent<RectTransform> ().DOAnchorPos (onScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> 
				{
					currentMenu = pauseMenu.GetComponent<MenuComponent> ();
					eventSystem.SetSelectedGameObject (null);
					eventSystem.SetSelectedGameObject (pauseMenu.GetComponent<MenuComponent> ().selectable);

				}).SetUpdate (true);
			
		}
		
		else if(GameManager.Instance.gameState == GameState.Pause)
		{
			SoundsManager.Instance.PlaySound (SoundsManager.Instance.buttonCancel);
			//Unpause
			pauseMenu.GetComponent<RectTransform> ().DOAnchorPos (offScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=> 
				{
					eventSystem.SetSelectedGameObject (null);
					pauseMenu.gameObject.SetActive (false);
					Time.timeScale = 1;
					currentMenu = null;
					GameManager.Instance.gameState = GameState.Playing;

				}).SetUpdate (true);
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
		currentMenu = menu;
	}

	public void MainMenu ()
	{
		StartCoroutine (MainMenuCoroutine ());
	}

	IEnumerator MainMenuCoroutine ()
	{
		yield return TournamentManager.Instance.StartCoroutine ("UnloadLevel");

		Time.timeScale = 1;
		GameManager.Instance.gameState = GameState.Menu;
		timerCanvas.gameObject.SetActive (false);

		ShowMenu (mainMenu);
	}

	public void ShowMenu (MenuComponent targetMenu, bool back = true)
	{
		if(currentMenu != null)
		{
			MenuComponent previousMenu = currentMenu;
			previousMenu._previousSelection = eventSystem.currentSelectedGameObject;

			if(back)
				targetMenu.previousMenu = previousMenu;

			eventSystem.SetSelectedGameObject (null);

			currentMenu = targetMenu;

			targetMenu.gameObject.SetActive (true);

			previousMenu._rect.DOAnchorPos (offScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=>
				{
					previousMenu.gameObject.SetActive (false);

					targetMenu._rect.DOAnchorPos (onScreenPosition, menuAnimationDuration).SetEase (menuEase);

					if(targetMenu._previousSelection != null)
						eventSystem.SetSelectedGameObject (targetMenu._previousSelection);

					else if(targetMenu.selectable != null)
						eventSystem.SetSelectedGameObject (targetMenu.selectable);
					
				}).SetUpdate (true);
		}
		else
		{
			currentMenu = targetMenu;

			targetMenu.gameObject.SetActive (true);
			eventSystem.SetSelectedGameObject (null);

			targetMenu._rect.DOAnchorPos (onScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=>
				{
					if(targetMenu._previousSelection != null)
						eventSystem.SetSelectedGameObject (targetMenu._previousSelection);

					else if(targetMenu.selectable != null)
						eventSystem.SetSelectedGameObject (targetMenu.selectable);
					
				}).SetUpdate (true);
		}

		if(targetMenu == mainMenu)
		{
			if(OnMainMenu != null)
				OnMainMenu ();
			
			targetMenu.previousMenu = null;
		}
	}

	public void Hide (MenuComponent targetMenu)
	{
		targetMenu._previousSelection = eventSystem.currentSelectedGameObject;

		targetMenu._rect.DOAnchorPos (offScreenPosition, menuAnimationDuration).SetEase (menuEase).OnComplete (()=>
			{
				targetMenu.gameObject.SetActive (false);
				eventSystem.SetSelectedGameObject (null);
				currentMenu = null;
			}).SetUpdate (true);
	}

	public IEnumerator EndMenu (bool cat)
	{
		yield return new WaitForSecondsRealtime (endMenuDelay);

		catWon.SetActive (false);
		mousesWon.SetActive (false);

		if (cat)
			catWon.SetActive (true);
		else
			mousesWon.SetActive (true);

		ShowMenu (endMode, false);

		yield return new WaitForSecondsRealtime (menuAnimationDuration);

		yield return new WaitForSecondsRealtime (endMenuDuration);

		Hide (endMode);

		yield return new WaitForSecondsRealtime (menuAnimationDuration * 2);
	}

	public void Quit ()
	{
		Application.Quit ();
	}
}
