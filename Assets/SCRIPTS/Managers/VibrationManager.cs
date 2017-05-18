using UnityEngine;
using System.Collections;
using Rewired;
using DG.Tweening;
using System;
using System.Collections.Generic;

public enum FeedbackType { CatDash, MouseDash, CatStun, MouseAttack, MouseFreeze, MouseUnfreeze, Victory, FlagTaken, Default }

public class VibrationManager : Singleton<VibrationManager> 
{
	public List<VibrationSettings> vibrationList = new List<VibrationSettings>();

	[Header ("Vibration Debug")]
	//Left Heavy
	public float[] playersLeftMotor = new float[4];
	//Left Ligth
	public float[] playersRightMotor = new float[4];
	public int[] playersVibrationCount = new int[4];

	[Header ("Cat Dash")]
	public float catDistanceLimit = 10f;

	[Header ("Test")]
	public bool test = false;
	public FeedbackType whichFeedbackTest = FeedbackType.Default;

	private float _timeToStopVibration = 1f;
	private bool _applicationIsQuitting = false;
	private Player[] _rewiredPlayers = new Player[4];

	// Use this for initialization
	void Start () 
	{
		TournamentManager.Instance.OnNextRound += Subscribe;
		GameManager.Instance.OnVictory += SlowlyStopVibration;
		TournamentManager.Instance.OnEndMode += SlowlyStopVibration;

		for (int i = 0; i < _rewiredPlayers.Length; i++)
			_rewiredPlayers [i] = ReInput.players.GetPlayer (i);
	}

	// Update is called once per frame
	void Update () 
	{
		if (GameManager.Instance._cat != null)
			CatDashing ();
		
		for(int i = 0; i < _rewiredPlayers.Length; i++)
		{
			if (_rewiredPlayers [i] == null)
				continue;

			if (_rewiredPlayers [i].controllers.joystickCount == 0 || !_rewiredPlayers [i].controllers.Joysticks [0].supportsVibration)
				continue;

			_rewiredPlayers [i].controllers.Joysticks [0].SetVibration(playersLeftMotor[i], playersRightMotor[i]);
		}

		if(test)
		{
			test = false;
			Vibrate (1, whichFeedbackTest);
		}
	}

	void Subscribe ()
	{
		//Cat
		Cat cat = GameManager.Instance._cat;

		cat.OnStunned += () => Vibrate (cat.controllerNumber, FeedbackType.CatStun);
		cat.OnDash += () => Vibrate (cat.controllerNumber, FeedbackType.CatDash);
		cat.OnDashEnd += () => Vibrate (cat.controllerNumber, 0, 0, 0.2f);

		//Mouses
		foreach (Mouse m in GameManager.Instance._mouses) 
		{
			m.OnAttack += () => Vibrate (m.controllerNumber, FeedbackType.MouseAttack);
			m.OnDash += () => Vibrate (m.controllerNumber, FeedbackType.MouseDash);
			m.OnFrozen += () => Vibrate (m.controllerNumber, FeedbackType.MouseFreeze);
			m.OnUnfrozen += () => Vibrate (m.controllerNumber, FeedbackType.MouseUnfreeze);
		}
	}

	void CatDashing ()
	{
		for(int i = 0; i < GameManager.Instance._mouses.Count; i++)
		{
			int controllerNumber = GameManager.Instance._mouses [i].controllerNumber;

			if (_rewiredPlayers [controllerNumber] == null)
				continue;

			if (_rewiredPlayers [controllerNumber].controllers.joystickCount == 0 || !_rewiredPlayers [controllerNumber].controllers.Joysticks [0].supportsVibration)
				continue;

			if (Vector3.Distance (GameManager.Instance.cat.transform.position, GameManager.Instance._mouses [i].transform.position) > catDistanceLimit)
				continue;

			if(GameManager.Instance.gameState != GameState.Playing)
			{
				playersLeftMotor [controllerNumber] = 0;
				continue;
			}

			if(GameManager.Instance._cat.dashState == DashState.Dashing || GameManager.Instance._cat.dashState == DashState.DashAim)
				playersLeftMotor [controllerNumber] = 1;
			else
				playersLeftMotor [controllerNumber] = 0;
		}
	}

	public void Vibrate (int whichPlayer, FeedbackType whichVibration)
	{
		if (GameManager.Instance.gameState != GameState.Playing)
			return;

		float leftMotorForce = 0;
		float rightMotorForce = 0;
		float vibrationDuration = 0;

		bool exactType = true;

		for(int i = 0; i < vibrationList.Count; i++)
		{
			if(vibrationList[i].whichVibration == whichVibration)
			{
				leftMotorForce = vibrationList [i].leftMotorForce;
				rightMotorForce = vibrationList [i].rightMotorForce;
				vibrationDuration = vibrationList [i].vibrationDuration;
				exactType = true;
				break;
			}
		}

		if(!exactType)
		{
			leftMotorForce = vibrationList [0].leftMotorForce;
			rightMotorForce = vibrationList [0].rightMotorForce;
			vibrationDuration = vibrationList [0].vibrationDuration;
		}

		Vibrate (whichPlayer, leftMotorForce, rightMotorForce, vibrationDuration);
	}

	public void Vibrate (int whichPlayer, float leftMotor, float rightMotor, float duration)
	{
		if (GameManager.Instance.gameState != GameState.Playing)
			return;
		
		StartCoroutine (Vibration (whichPlayer, leftMotor, rightMotor, duration));
	}

	public void Vibrate (int whichPlayer, float leftMotor, float rightMotor, float duration, float startDuration, float stopDuration, Ease easeType = Ease.Linear)
	{
		if (GameManager.Instance.gameState != GameState.Playing)
			return;

		StartCoroutine (Vibration (whichPlayer, leftMotor, rightMotor, duration, startDuration, stopDuration, easeType));
	}

	public void VibrateBurst (int whichPlayer, int burstNumber, float leftMotor, float rightMotor, float burstDuration, float durationBetweenBurst)
	{
		if (GameManager.Instance.gameState != GameState.Playing)
			return;
		
		StartCoroutine (VibrationBurst (whichPlayer, burstNumber, leftMotor, rightMotor, burstDuration, durationBetweenBurst));
	}

	IEnumerator Vibration (int whichPlayer, float leftMotor, float rightMotor, float duration)
	{
		DOTween.Kill ("Vibration" + whichPlayer);

		playersLeftMotor [whichPlayer] = leftMotor;
		playersRightMotor [whichPlayer] = rightMotor;

		playersVibrationCount [whichPlayer]++;

		if(duration > 0)
			yield return new WaitForSecondsRealtime (duration);

		if(playersVibrationCount [whichPlayer] == 1)
			StopVibration (whichPlayer);

		playersVibrationCount [whichPlayer]--;

		yield break;
	}

	IEnumerator Vibration (int whichPlayer, float leftMotor, float rightMotor, float duration, float startDuration, float stopDuration, Ease easeType = Ease.Linear)
	{
		DOTween.Kill ("Vibration" + whichPlayer);

		Tween myTween = DOTween.To(()=> playersLeftMotor [whichPlayer], x=> playersLeftMotor [whichPlayer] = x, leftMotor, startDuration).SetEase(easeType).SetId("Vibration" + whichPlayer);
		DOTween.To(()=> playersRightMotor [whichPlayer], x=> playersRightMotor [whichPlayer] = x, rightMotor, startDuration).SetEase(easeType).SetId("Vibration" + whichPlayer);

		playersVibrationCount [whichPlayer]++;

		yield return myTween.WaitForCompletion ();

		if(duration > 0)
			yield return new WaitForSecondsRealtime (duration);

		myTween = DOTween.To(()=> playersLeftMotor [whichPlayer], x=> playersLeftMotor [whichPlayer] = x, 0, stopDuration).SetEase(easeType).SetId("Vibration" + whichPlayer);
		DOTween.To(()=> playersRightMotor [whichPlayer], x=> playersRightMotor [whichPlayer] = x, 0, stopDuration).SetEase(easeType).SetId("Vibration" + whichPlayer);

		yield return myTween.WaitForCompletion ();

		if(playersVibrationCount [whichPlayer] == 1)
			StopVibration (whichPlayer);

		playersVibrationCount [whichPlayer]--;
	}

	IEnumerator VibrationBurst (int whichPlayer, int burstNumber, float leftMotor, float rightMotor, float burstDuration, float durationBetweenBurst)
	{
		DOTween.Kill ("Vibration" + whichPlayer);

		for (int i = 0; i < burstNumber; i++)
		{
			playersLeftMotor [whichPlayer] = leftMotor;
			playersRightMotor [whichPlayer] = rightMotor;

			playersVibrationCount [whichPlayer]++;

			if(burstDuration > 0)
				yield return new WaitForSecondsRealtime (burstDuration);

			if(playersVibrationCount [whichPlayer] == 1)
				StopVibration (whichPlayer);

			playersVibrationCount [whichPlayer]--;

			if(durationBetweenBurst > 0)
				yield return new WaitForSecondsRealtime (durationBetweenBurst);
		}

		yield break;
	}

	public void StopVibration (int whichPlayer)
	{
		if(!_applicationIsQuitting)
		{
			playersLeftMotor [whichPlayer] = 0;
			playersRightMotor [whichPlayer] = 0;

			/*switch (whichPlayer)
			{
			case 0:
				foreach(Joystick j in gamepad1.controllers.Joysticks) 
				{
					if(!j.supportsVibration && j != null) continue;
					j.SetVibration(0, 0);
				}
				break;
			case 1:
				foreach(Joystick j in gamepad2.controllers.Joysticks) 
				{
					if(!j.supportsVibration && j != null) continue;
					j.SetVibration(0, 0);
				}
				break;
			case 2:
				foreach(Joystick j in gamepad3.controllers.Joysticks) 
				{
					if(!j.supportsVibration && j != null) continue;
					j.SetVibration(0, 0);
				}
				break;
			case 3:
				foreach(Joystick j in gamepad4.controllers.Joysticks) 
				{
					if(!j.supportsVibration && j != null) continue;
					j.SetVibration(0, 0);
				}
				break;
			}*/
		}
	}

	public void StopAllVibration ()
	{
		StopAllCoroutines ();

		/*foreach(Joystick j in gamepad1.controllers.Joysticks) 
		{
			if(!j.supportsVibration && j != null) continue;
			j.SetVibration(0, 0);
		}

		foreach(Joystick j in gamepad2.controllers.Joysticks) 
		{
			if(!j.supportsVibration && j != null) continue;
			j.SetVibration(0, 0);
		}

		foreach(Joystick j in gamepad3.controllers.Joysticks) 
		{
			if(!j.supportsVibration && j != null) continue;
			j.SetVibration(0, 0);
		}

		foreach(Joystick j in gamepad4.controllers.Joysticks) 
		{
			if(!j.supportsVibration && j != null) continue;
			j.SetVibration(0, 0);
		}
*/
		DOTween.Kill ("Vibration0");
		DOTween.Kill ("Vibration1");
		DOTween.Kill ("Vibration2");
		DOTween.Kill ("Vibration3");
	}

	void SlowlyStopVibration ()
	{
		StopAllCoroutines ();

		DOTween.Kill ("Vibration0");
		DOTween.Kill ("Vibration1");
		DOTween.Kill ("Vibration2");
		DOTween.Kill ("Vibration3");

		DOTween.To(()=> playersLeftMotor [0], x=> playersLeftMotor [0] = x, 0, _timeToStopVibration).SetId("Vibration" + 0);
		DOTween.To(()=> playersRightMotor [0], x=> playersRightMotor [0] = x, 0, _timeToStopVibration).SetId("Vibration" + 0);

		DOTween.To(()=> playersLeftMotor [1], x=> playersLeftMotor [1] = x, 0, _timeToStopVibration).SetId("Vibration" + 1);
		DOTween.To(()=> playersRightMotor [1], x=> playersRightMotor [1] = x, 0, _timeToStopVibration).SetId("Vibration" + 1);

		DOTween.To(()=> playersLeftMotor [2], x=> playersLeftMotor [2] = x, 0, _timeToStopVibration).SetId("Vibration" + 2);
		DOTween.To(()=> playersRightMotor [2], x=> playersRightMotor [2] = x, 0, _timeToStopVibration).SetId("Vibration" + 2);

		DOTween.To(()=> playersLeftMotor [3], x=> playersLeftMotor [3] = x, 0, _timeToStopVibration).SetId("Vibration" + 3);
		DOTween.To(()=> playersRightMotor [3], x=> playersRightMotor[3] = x, 0, _timeToStopVibration).SetId("Vibration" + 3);

	}

	void OnApplicationQuit ()
	{
		_applicationIsQuitting = true;

		StopAllVibration ();
	}
}

[Serializable]
public class VibrationSettings
{
	public FeedbackType whichVibration = FeedbackType.Default;

	[Range (0,1)]
	public float leftMotorForce;
	[Range (0,1)]
	public float rightMotorForce;
	public float vibrationDuration;
}