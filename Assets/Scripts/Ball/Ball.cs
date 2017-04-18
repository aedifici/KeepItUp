using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 *   BALL
 * ------------------------------------------
 *   Calculates ball physics and then calls
 *   BallController to execute.
 */

[RequireComponent (typeof (BallController))]
public class Ball : MonoBehaviour {
	// External.
	[HideInInspector] public bool isPaused;
	// References.
	public AudioSource[] hitSounds;
	public GameObject outlinePink;
	public GameObject outlineBlue;
	public Color      blueTint;
	public Color      pinkTint;
	public Color      startTint;
	public bool       ignoreCollision;
	// Controls.
	public  float     gravity;								// The rate at which the ball accelerates down each frame.
	public  float     addedHitForceRatio;		 			// The ratio of the player's y velocity that's added to a ballHit force.
	public  float     bounceDecay;							// How much the ball's velocity decays vertically each bounce.
	public  float     horizontalDecay;						// How much the ball's velocity decays horizontally over time.
	public  float     minVerticalVelocity;					// The minimum velocity that the ball can bounce vertically.
	public  float     minHorizontalVelocity;				// The minimum velocity that the ball can move horizontally.
	public  float     ballHitCooldown;						// How many seconds until the ball can be hit again.
	// Runtime.
	private Vector3   velocity;								// The current velocity vector of the player. Modified and sent to the playerController.
	private bool      isHit;								// If the ball has recently been hit. Cannot be hit again until this is false (see ballHitCooldown).
	private bool      isStunned;							// Whether or not the ball is stunned (no Update).
	// Dependancies.
	private BallController controller;	
	private SpriteRenderer ballSprite;
	private GameManager    game;

	void Start () {
		controller = GetComponent<BallController> ();
		ballSprite = GetComponentInChildren<SpriteRenderer> ();
		game = GameObject.FindObjectOfType (typeof (GameManager)) as GameManager;
		outlineBlue.SetActive (false);
		outlinePink.SetActive (false);
		ballSprite.color = startTint;
		// DEBUG horizontal start velocity.
		//velocity.x = 1.8f;
		isPaused = true;
	}

	void Update () {
		if (isStunned) return;

		// Check for bounces.
		if (controller.collisions.below) {
			velocity.y *= -1;
			velocity.y -= bounceDecay;
			if (velocity.y < minVerticalVelocity) {
				velocity.y = minVerticalVelocity;
			}
		}
		// Decay horizontal velocity to air friction.
		if (velocity.x > 0) {
			velocity.x -= horizontalDecay * Time.deltaTime;
			if (velocity.x < minHorizontalVelocity) {
				velocity.x = minHorizontalVelocity;
			}
		} else if (velocity.x < 0) {
			velocity.x += horizontalDecay * Time.deltaTime;
			if (velocity.x > -minHorizontalVelocity) {
				velocity.x = -minHorizontalVelocity;
			}
		}
		// Apply gravity.
		velocity.y += gravity * Time.deltaTime;

		// Rotate based on magnitude of velocity.
		ballSprite.transform.Rotate (new Vector3 (0,0,velocity.magnitude * 100 * Time.deltaTime));

		// Call controller.
		controller.Move (velocity * Time.deltaTime, this);
	}

	//-----------------------------------------------------------------------------------------------------------------  Public methods
	public void BounceLeft () {
		// Guarantee leftward movement.
		velocity.x = -Mathf.Abs (velocity.x);
	}

	public void BounceRight () {
		// Guarantee rightward movement.
		velocity.x =  Mathf.Abs (velocity.x);
	}

	public void BallHit (float magnitude, Vector3 forcePointPos, Vector3 addedForce, bool stun, bool wasPlayerOne) {

		if (isHit) return;

		int whichBall = Random.Range (0,3);

		hitSounds[whichBall].Stop ();
		hitSounds[whichBall].Play ();
		// Give the ball a velocity vector of magnitude magnitude 
		//  in the direction of the vector from forcePoint to the ball.
		Vector3 fixedPosition = new Vector3 (forcePointPos.x, forcePointPos.y, transform.position.z);
		Vector3 force = (transform.position - fixedPosition).normalized * magnitude;
		velocity.x = force.x;
		velocity.y = force.y + Mathf.Clamp (addedForce.y * addedHitForceRatio, 0, float.MaxValue);

		if (wasPlayerOne) {
			outlineBlue.SetActive (false);
			outlinePink.SetActive (true);
			ballSprite.color = pinkTint;
			game.AddScore (1);
		} else {
			outlineBlue.SetActive (true);
			outlinePink.SetActive (false);
			ballSprite.color = blueTint;
			game.AddScore (2);
		}

		// Hitstun.
		if (stun) {
			HitstunOn (0.2f);
		}

		// Ensure the call cannot be hit again for some time.
		isHit = true;
		Invoke ("CanBeHit", ballHitCooldown);
	}

	public void CanBeHit () {
		isHit = false;
	}
		
	public void HitstunOn (float stunTime) {
		isStunned = true;
		Invoke ("HitstunOff", stunTime);
	}

	public void HitstunOff () {
		isStunned = false;
	}
}
