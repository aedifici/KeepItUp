using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/* 
 *   NEXT SCENE 
 * ------------------------------------------
 *   Handles the scene transition from the 
 *   "NextScene" transition scene.
 */

public class NextScene : MonoBehaviour {
	// Controls.
	public bool  isBegin;
	public float timeToShowText;
	public float timeToHold;
	public float timeToMoveOn;
	// References.
	public AudioSource message;
	public Text text;

	void Start () {
		Invoke ("ShowText", timeToShowText);
		if (isBegin) {
			GameObject game = GameObject.FindObjectOfType (typeof (GameManager)) as GameObject;
			if (game) {
				Destroy (game);
			}
		}
	}

	void ShowText () {
		if (isBegin) message.Play ();
		text.enabled = true;
		Invoke ("HideText", timeToHold);
	}

	void HideText () {
		text.enabled = false;
		Invoke ("MoveOn", timeToMoveOn);
	}

	void MoveOn () {
		SceneManager.LoadScene ("Test");
	}
}
