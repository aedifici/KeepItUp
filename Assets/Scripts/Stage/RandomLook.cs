using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 *   RANDOM LOOK
 * ------------------------------------------
 *   Handle's the scientist's look and write
 *   animation cycle (which is randomized).
 * 
 *   This uses hard-coded literals currently.
 */

public class RandomLook : MonoBehaviour {
	// Run-time.
	private float timer;
	private float lookTime;
	// Dependancy.
	private Animator anim;

	void Start () {
		anim = GetComponent<Animator> ();
		lookTime = Random.Range (5f, 18f);
	}

	void Update () {
		timer += Time.deltaTime;
		if (timer >= lookTime) {
			anim.SetTrigger ("look");
			lookTime = Random.Range (5f, 18f);
			timer = 0;
		}
	}
}
