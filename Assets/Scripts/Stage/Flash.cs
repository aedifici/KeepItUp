using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* 
 *   FLASH (START MENU)
 * ------------------------------------------
 *   The flashing "Press Anything" button on
 *   the Start scene. Handles the flash 
 *   animation and input to continue/escape.
 */

public class Flash : MonoBehaviour {
	// Control.
	public float flashTime;
	// Run-time.
	private float timer;
	private bool isOn = true;
	// Dependancy.
	private SpriteRenderer rend;

	void Start () {
		rend = GetComponent<SpriteRenderer> ();
	}

	void Update () {
		// Inputs to escape or begin.
		if (Input.GetButtonDown ("Exit")) {
			Application.Quit ();
		} else if (Input.anyKey) {
			SceneManager.LoadScene ("BeginMatch");
		}
		// Flash animation.
		timer += Time.deltaTime;
		if (timer >= flashTime) {
			timer -= flashTime;
			if (isOn) {
				rend.enabled = false;
				isOn = false;
			} else {
				rend.enabled = true;
				isOn = true;
			}
		}
	}
}
