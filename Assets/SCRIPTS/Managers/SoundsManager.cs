using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio;

public class SoundsManager : Singleton<SoundsManager> 
{
	[Header ("UI")]
	[SoundGroup]
	public string buttonSubmit;
	[SoundGroup]
	public string buttonCancel;
	[SoundGroup]
	public string buttonPlayer;
	[SoundGroup]
	public string endGameJingle;
	[SoundGroup]
	public string timerLastSeconds1;
	[SoundGroup]
	public string timerLastSeconds2;

	[Header ("Cat")]
	[SoundGroup]
	public string catDash;
	[SoundGroup]
	public string catHit;
	[SoundGroup]
	public string catDashAim;
	[SoundGroup]
	public string catWalking;

	[Header ("Mouses")]
	[SoundGroup]
	public string mouseAttack;
	[SoundGroup]
	public string mouseDash;
	[SoundGroup]
	public string mouseFreeze;
	[SoundGroup]
	public string mouseUnfreeze;
	[SoundGroup]
	public string mouseWalking;

	[Header ("Environement")]
	[SoundGroup]
	public string flagBegin;
	[SoundGroup]
	public string flagActivated;
	[SoundGroup]
	public string propsBounce;
	[SoundGroup]
	public string portal;


	// Use this for initialization
	void Start () 
	{
		TournamentManager.Instance.OnNextRound += Subscribe;
		GameManager.Instance.OnVictory += () => PlaySound (endGameJingle);
		GameManager.Instance.OnVictory += () => MasterAudio.StopAllOfSound (catWalking);

		//Music
		MenuManager.Instance.OnMainMenu += () => MasterAudio.StartPlaylist ("Menu");
		TournamentManager.Instance.OnEndMode += () => MasterAudio.StartPlaylist ("Score");
		TournamentManager.Instance.OnStartGame += () => MasterAudio.StartPlaylist ("Game");
	}

	void Subscribe ()
	{
		//Cat
		Cat cat = GameManager.Instance._cat;

		cat.OnDash += () => {
			MasterAudio.StopSoundGroupOfTransform (cat.transform, catDashAim);
			PlaySoundFollow (catDash, cat.transform);
		};
		cat.OnDashAiming += () => PlaySoundFollow (catDashAim, cat.transform);
		cat.OnMoving += () => PlaySoundFollow (catWalking, cat.transform);
		cat.OnStopMoving += () => MasterAudio.StopSoundGroupOfTransform (cat.transform, catWalking);
		cat.OnHit += () => PlaySoundFollow (catHit, cat.transform);

		//Mouses
		foreach(Mouse m in GameManager.Instance._mouses)
		{
			m.OnAttack += () => PlaySoundFollow (mouseAttack, m.transform);
			m.OnDash += () => PlaySoundFollow (mouseDash, m.transform);
			m.OnFrozen += () => PlaySoundFollow (mouseFreeze, m.transform);
			m.OnUnfrozen += () => PlaySoundFollow (mouseUnfreeze, m.transform);
		}
	}

	public void PlaySound (string sound)
	{
		MasterAudio.PlaySoundAndForget (sound);
	}

	public void PlaySoundFollow (string sound, Transform follow)
	{
		MasterAudio.PlaySound3DFollowTransformAndForget (sound, follow);
	}

	public void PlaySoundTransform (string sound, Transform follow)
	{
		MasterAudio.PlaySound3DAtTransformAndForget (sound, follow);
	}
}
