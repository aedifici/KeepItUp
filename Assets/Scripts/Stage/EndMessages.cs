using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 *   END MESSAGES 
 * ------------------------------------------
 *   Handles the ending message and keeps 
 *   track of whether it's still playing.
 */

public class EndMessages : MonoBehaviour {
	// References.
	public AudioSource girlAlternate;
	public AudioSource[] clips;
	// External.
	[HideInInspector] public bool isPlaying;
	// Run-time.
	private bool isGirl;

	// Called externally to pause then play a clip.
	public void PlayAMessage (bool isGirl) {
		this.isGirl = isGirl;
		StartCoroutine ( BeginClip (2f));
	}

	private IEnumerator BeginClip (float initialWait) {
		isPlaying = true;

		float initialTime = Time.time;
		while (Time.time - initialTime < initialWait) {
			yield return 1;
		}

		int whichOne = Random.Range (0, clips.Length);
		AudioSource currentClip;
		if (whichOne == 0) {
			if (isGirl) {
				currentClip = girlAlternate;
			} else {
				currentClip = clips[0];
			}
		} else {
			currentClip = clips[whichOne];
		}
		currentClip.Play();

		// Check for end of clip then update isPlaying.
		while (currentClip.isPlaying) {
			yield return 1;
		}
		isPlaying = false;
	}
}
