using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/* 
 *   GAME MANAGER 
 * ------------------------------------------
 *   Manages the score, UI, win/loss & scenes.
 */

public class GameManager : MonoBehaviour {
	// External.
	[HideInInspector] public float   timeLeft;		// How much time is left in the match. This is set in Setup();
	[HideInInspector] public int     boyWins;		// How many round wins the boy has. 2 means he wins the match.
	[HideInInspector] public int     girlWins;		// How many round wins the girl has. 2 means she wins the match.
	//References.	
	public  RefDump reference;						// The refDump is where all the references the GameManager needs are kept...
	// Control.										//  ... so that when loading in a new scene it can get those back.
	public  float  roundTime;						// How many seconds in a round.
	public  float  dropInDelay;						// Time in seconds it takes for the robots, UI, and ball to be dropped in.
	public  float  startDelay;						// Time in seconds after drop-in that players are given control.
	public  float  UIDropInTime;					// How long it takes for the UI to slide in.
	// Runtime.
	private int    boyScore;						// The boy's score (ball hits) for this round. Used to determine the winner.
	private int    girlScore;						// The girl's score (ball hits) for this round. Used to determine the winner.
	private bool   hasStarted;						// Whether or not the match has started. 
	private bool   isRoundOver;						// Whether the round is over.
	private bool   isGameOver;						// Whether the match (game) is over.

	void Start () {
		// Keep the same GameManager instance across play sessions so the match wins are kept.
		DontDestroyOnLoad(this);
		// Delete duplicates on sceneLoad.
		if (FindObjectsOfType(GetType()).Length > 1 || SceneManager.GetActiveScene().name == "Start")
		{
			Destroy(gameObject);
		}
	}
	// Called externally to setup and begin the match.
	public void Setup () {
		// Get the references from the RefDump.
		reference = GameObject.Find ("RefDump").GetComponent<RefDump> ();
		// Set up GameManager initial state.
		hasStarted = false;
		isRoundOver = false;
		girlScore = 0;
		boyScore = 0;
		timeLeft = roundTime;
		// Drop in after the drop-in delay.
		Invoke ("DropIn", dropInDelay);
		// Set up the UI.
		reference.boyScoreText.text     = boyScore.ToString();
		reference.boyScoreOutline.text  = boyScore.ToString();
		reference.girlScoreText.text    = girlScore.ToString();
		reference.girlScoreOutline.text = girlScore.ToString();
		reference.timerText.text = timeLeft.ToString("F0");
		reference.timerOutline.text = timeLeft.ToString("F0");
		// Update win icons based on wins. (2 in case of best 3 of 5).
		if (boyWins >= 1) {
			reference.WinIconR2.enabled = true;
		}
		if (boyWins >= 2) {
			reference.WinIconR1.enabled = true;
		}
		if (girlWins >= 1) {
			reference.WinIconL1.enabled = true;
		}
		if (girlWins >= 2) {
			reference.WinIconL2.enabled = true;
		}
	}

	//-----------------------------------------------------------------------------------------------------------------  Update
	void Update () {
		// Escape input to escape to start menu if not on start menu.
		if (Input.GetButtonDown ("Exit") && SceneManager.GetActiveScene().name != "Start" && reference.boy.isPaused == false) {
			SceneManager.LoadScene ("Start");
		}
		// State gates to prevent updating timer if not started or round ended.
		if (!hasStarted) return;
		if (isRoundOver) return;

		// Update the timer.
		timeLeft -= Time.deltaTime;
		// Check for round finish.
		if (timeLeft <= 0) {
			timeLeft = 0;
			// Fade out the music.
			StartCoroutine (FadeOut (reference.music, 0.8f));

			// Update the timer text.
			reference.timerText.text = timeLeft.ToString("F0");
			reference.timerOutline.text = timeLeft.ToString("F0");

			// Pause everything.
			reference.boy.isPaused = true;
			reference.girl.isPaused = true;
			//reference.girl.transform.gameObject.layer = 0;
			//reference.boy.transform.gameObject.layer = 0;

			// Declare winner and loser.
			if (boyScore > girlScore) {
				reference.girl.KnockDown ();
				reference.boy.Win ();
				boyWins += 1;
				reference.endMessageClips.PlayAMessage (false);
			} else if (girlScore > boyScore) {
				reference.boy.KnockDown ();
				reference.girl.Win ();
				girlWins += 1;
				reference.endMessageClips.PlayAMessage (true);
			} 
			// Update win icons.
			if (boyWins >= 1) {
				reference.WinIconR2.enabled = true;
			}
			if (boyWins >= 2) {
				reference.WinIconR1.enabled = true;
			}
			if (girlWins >= 1) {
				reference.WinIconL1.enabled = true;
			}
			if (girlWins >= 2) {
				reference.WinIconL2.enabled = true;
			}

			// End the round.
			reference.ball.ignoreCollision = true;
			reference.ball.isPaused = true;
			isRoundOver = true;

			// Check for match win or round win and transition scene accordingly.
			if (boyWins >= 2 || girlWins >= 2) {
				isGameOver = true;
				boyWins = 0;
				girlWins = 0;
				// Start coroutine to transition out at end of sound clip, to the start screen. 
				StartCoroutine (TransitionRound (true));
			} else {
				// Start coroutine to transition out at end of sound clip.
				StartCoroutine (TransitionRound (false));
			}
		}
		if (reference != null) {
			// Update the timer text each update.
			reference.timerText.text = timeLeft.ToString("F0");
			reference.timerOutline.text = timeLeft.ToString("F0");
		}
	}

	//-----------------------------------------------------------------------------------------------------------------  Methods
	public void AddScore (int player) {
		if (player == 1) {
			girlScore++;
		} else if (player == 2) {
			boyScore++;
		} else {
			Debug.Log ("ERROR - " + player + " is an invalid player.");
		}
		// Update text.
		reference.boyScoreText.text     = boyScore.ToString();
		reference.boyScoreOutline.text  = boyScore.ToString();
		reference.girlScoreText.text    = girlScore.ToString();
		reference.girlScoreOutline.text = girlScore.ToString();
	}

	private void DropIn () {
		reference.boy.isDroppedIn  = true;
		reference.girl.isDroppedIn = true;
		StartCoroutine (DropInTheUI ());
		StartCoroutine (Countdown ());
		Invoke ("StartTheGame", startDelay);
	}

	private void StartTheGame () {
		reference.music.Play ();
		reference.boy.isPaused   = false;
		reference.girl.isPaused  = false;
		reference.ball.isPaused  = false;
		hasStarted               = true;
		reference.arrow.SetActive (true);
	}

	//-----------------------------------------------------------------------------------------------------------------  Coroutines
	private IEnumerator Countdown () {
		if (reference != null) {
			reference.announcerClip.Play ();
			// Initialize.
			reference.countdownText.enabled = true;
			reference.countdownOutline.enabled = true;
			reference.countdownText.text = "3";
			reference.countdownOutline.text = "3";
		}
		// Wait one second.
		float initialTime = Time.time;
		while (Time.time - initialTime < 0.7f) {
			yield return 1;
		}
		if (reference != null) {
			reference.countdownText.text = "2";
			reference.countdownOutline.text = "2";
		}
		initialTime = Time.time;
		while (Time.time - initialTime < 0.7f) {
			yield return 1;
		}
		if (reference != null) {
			reference.countdownText.text = "1";
			reference.countdownOutline.text = "1";
		}
		initialTime = Time.time;
		while (Time.time - initialTime < 0.7f) {
			yield return 1;
		}
		if (reference != null) {
			reference.countdownText.text = "KEEP";
			reference.countdownOutline.text = "KEEP";
		}
		initialTime = Time.time;
		while (Time.time - initialTime < 0.7f) {
			yield return 1;
		}
		if (reference != null) {
			reference.countdownText.text = "IT";
			reference.countdownOutline.text = "IT";
		}
		initialTime = Time.time;
		while (Time.time - initialTime < 0.7f) {
			yield return 1;
		}
		if (reference != null) {
			reference.countdownText.text = "UP";
			reference.countdownOutline.text = "UP";
		}
		initialTime = Time.time;
		while (Time.time - initialTime < 0.7f) {
			yield return 1;
		}
		if (reference != null) {
			reference.countdownText.enabled = false; 
			reference.countdownOutline.enabled = false;
		}
	}

	// Fade out music.
	public static IEnumerator FadeOut (AudioSource audioSource, float FadeTime) {
		float startVolume = audioSource.volume;

		while (audioSource.volume > 0) {
			audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

			yield return null;
		}

		audioSource.Stop ();
		audioSource.volume = startVolume;
	}
		
	private IEnumerator DropInTheUI () {
		float initialTime = Time.time;
		Vector3 start = new Vector3 ();
		if (reference != null) start = reference.UI.transform.localPosition;
		Vector3 end   = new Vector3 (0, 0, 0);
		// Lerp the camera's position from start to end.
		while (Time.time - initialTime < UIDropInTime) {
			// Smooth lerp formula.
			float t = (Time.time - initialTime) / UIDropInTime;
			t = t * t * t * (t * (6f * t - 15f) + 10f);
			// Execute.
			if (reference != null) reference.UI.transform.localPosition = Vector3.Lerp (start, end, t);
			yield return 1;
		}
		// Finalize.
		if (reference != null) reference.UI.transform.localPosition = end;
	}

	private IEnumerator TransitionRound (bool isGameEnd) {
		// Wait until clip is no longer playing.
		while (reference.endMessageClips.isPlaying)	{
			yield return 1;
		}
		// Pause a little longer, then change scenes.
		float initialTime = Time.time;
		while (Time.time - initialTime < 1.5f) {
			yield return 1;
		}
		if (isGameEnd) {
			SceneManager.LoadScene ("Start");
		} else {
			SceneManager.LoadScene ("NextRound");
		}
	}
}
