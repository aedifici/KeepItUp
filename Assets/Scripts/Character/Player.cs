using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* 
 *   PLAYER
 * ------------------------------------------
 *   Takes all player input, sending intent to  
 *   controller. Handles all other player action, 
 *   stats, and logistics.
 * 
 *   Handles all player animation as well.
 */

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (Animator))]
public class Player : MonoBehaviour {
	// External.
	[HideInInspector] public  bool    isPaused;
	[HideInInspector] public  bool    isDroppedIn;
	[HideInInspector] public  bool    isHop;				// Whether or not the player is coming up from a head hop.
	[HideInInspector] public  bool    isFastFalling;		// Toggle for if the player is falling. Reset on below collision.
	[HideInInspector] public  bool    isHittingBall;		// Cannot hit the score again in this state, and prevents another hitstun event.
	[HideInInspector] public  Vector3 velocity;				// The current velocity vector of the player. Modified and sent to the playerController.
	// Controls.
	public  AudioSource jumpSound;
	public  AudioSource headHopSound;
	public  AudioSource knockDownSound;
	public  bool      onDebug;								// Whether or not debug is enabled.
	public  bool      isPlayerTwo;							// Whether or not this player is the second player.
	public  float     moveSpeed;         					// Pixel horizontal movement per frame.
	public  float     ballHitForce;							// The baseline force that a ballHit adds.
	public  float     ballHitCooldown;						// Time in seconds before the player can hit the ball again.
	public  float     hopStunDuration;						// How many seconds hopStun lasts.
	public  float     knockDownDuration;					// How many seconds a knockdown lasts until the player can move.
	public  float     hopVulnerableTime;					// How long the victim of a head hop is vulnerable to knock back when landing.
	public  float	  maxJumpHeight;         				// Heighest the player can jump.
	public  float     minJumpHeight;						// Lowest the player can jump.
	public  float     minHopHeight;							// Lowest the player can hop.
	public  float     timeToJump;      						// Time for jump to reach apex.
	public  float     fastFallSpeed;						// How much faster you fall if fast-fall is on.
	public  float     analogJumpSensitivity;				// The amount of analog movement (0.0f-1.0f) that will trigger a jump.
	public  float     aTimeAir;      						// Acceleration time (smoothing) in air (longer).
	public  float	  aTimeGround;      					// Acceleration time (smoothing) on ground.
	// References.
	public  Transform ballHitPivot;							// The transform of the pivot point that a ballHit take it's launch angle relative to.
	// Debug.
	public  Text	  debugText;							// [DEBUG] a canvas UI text object that can be updated with debug info.
	// Runtime.
	private Vector2   input;								// Stores the movement input vector (x and y).
	private float	  gravity;								// Downward acceleration. Calculated at start.
	private float	  maxJumpVelocity;						// Jump variables calculated at start using max/min jumpHeight public vars.
	private float	  minJumpVelocity;						// ...
	private float     minHopVelocity;						// ...
	private float     velocityXSmoothing;					// Used to smooth horizontal movement. Used in update.
	// Toggles.
	private bool      isStunned;							// Used to pause Update when stunned.
	private bool      isLeft;								// Movement direction.
	private bool      isBouncing;							// Victory dance.
	private bool      isKnockDownWaiting;					// Turned on at knockback and back on after a short time to guarantee no knockback animation interuption.
	private bool      isFallingVulnerable;					// When the player is head hopped they fall vulnerable for some time. Hitting the ground while vulnerable causes a knockback.
	private bool      isKnockedDown;						// Whether or not the player is knocked over. Determines when to end knockOver animation on input.
	private bool      isButtonJump;							// Whether or not the A button was used to jump (and therefore, let go to shorten the jump).
	private bool      isAnalogReset;						// Whether or not the analog has been reset as a jump option.
	// Dependancies.
	private PlayerController controller;
	private Animator         anim;

	void Start () {
		// Setup.
		controller = GetComponent<PlayerController> ();
		anim       = GetComponent<Animator> ();
		// Calculate gravity and jumpVelocity based on jumpHeight and timeToJump(apex).
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJump, 2);
		maxJumpVelocity = Mathf.Abs (gravity) * timeToJump;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
		minHopVelocity  = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minHopHeight);
		// Initial set.
		if (isPlayerTwo) {
			isLeft = true;
		}
		isAnalogReset = true;
		isPaused = true;
	}

	//-----------------------------------------------------------------------------------------------------------------  Update 
	void Update () {
		if (isDroppedIn || isPaused) {
			velocity.y += gravity * Time.deltaTime;
			controller.Move (velocity * Time.deltaTime, this);
			velocity.x = 0;
			if (controller.collisions.below) {
				isDroppedIn = false;
			}
		}
		if (isBouncing) {
			//velocity.y += gravity / 2 * Time.deltaTime;
			controller.Move (velocity * Time.deltaTime, this);
			velocity.x = 0;
			if (controller.collisions.below) {
				velocity.y = maxJumpHeight;
			}
		}
		if (isStunned) return;
		if (isPaused)  return;

		// DEBUG INPUTS
		if (onDebug) GetDebugInput ();

		// Knockdown animation.
		if (isKnockDownWaiting) {
			input.x = 0;
		} else if (isKnockedDown && !isKnockDownWaiting && (input.x != 0 || !controller.collisions.below)) {
			isKnockedDown = false;
			anim.SetTrigger ("endKnockOver");
		}

		// Prevent stacking gravity and reset jump state.
		if (controller.collisions.below) {
			velocity.y = 0;
			isHop = false;
			isFastFalling = false;

			// Knock over if you land while vulnerable to knockOver.
			if (isFallingVulnerable) {
				KnockDown ();
				isFallingVulnerable = false;
			}
		}

		// Walking animation state.
		if (Mathf.Abs (input.x) > 0 && controller.collisions.below) {
			anim.SetBool ("isMoving", true);
			anim.speed = 4f * (Mathf.Abs (input.x));
		} else {
			anim.SetBool ("isMoving", false);
			anim.speed = 1f;
		}

		// Get analog  input based on player.
		if (!isPlayerTwo)
			input = new Vector2 (Input.GetAxisRaw ("Horizontal_1"), Input.GetAxisRaw ("Vertical_1"));
		else 
			input = new Vector2 (Input.GetAxisRaw ("Horizontal_2"), Input.GetAxisRaw ("Vertical_2"));

		// Flip character based on horizontal movement.
		if (isLeft && input.x > 0) {
			transform.localScale = new Vector3 ( 2, transform.localScale.y, transform.localScale.z);
			isLeft = false;
		} else if (!isLeft && input.x < 0) {
			transform.localScale = new Vector3 (-2, transform.localScale.y, transform.localScale.z);
			isLeft = true;
		}

		// Animate jump based on vertical speed.
		if (velocity.y > 0 && velocity.y < 5 )
			anim.SetInteger ("jumpState", 2);
		else if (velocity.y < 0)
			anim.SetInteger ("jumpState", 3);
		if (controller.collisions.below) {
			if (anim.GetInteger ("jumpState") > 0) {
				anim.SetInteger ("jumpState", 0);
				anim.SetTrigger ("quickCrouch");
			} else {
				anim.SetInteger ("jumpState", 0);
			}
		}

		// Check for analog reset.
		if (input.y <= 0) {
			isAnalogReset = true;
		}

		// Check for jump.
		if (((isPlayerTwo) ? Input.GetButtonDown ("Jump_2") : Input.GetButtonDown ("Jump_1")) && controller.collisions.below) {
			isButtonJump = true;
			velocity.y = maxJumpVelocity;
			anim.speed = 1f;
			anim.SetInteger ("jumpState", 1);
			jumpSound.Stop ();
			jumpSound.Play ();
		} else if (input.y > analogJumpSensitivity && isAnalogReset && controller.collisions.below) {
			velocity.y = maxJumpVelocity;
			anim.speed = 1f;
			anim.SetInteger ("jumpState", 1);
			isAnalogReset = false;
			jumpSound.Stop ();
			jumpSound.Play ();
		} 

		// Check for button short jump (let go).
		if (((isPlayerTwo) ? Input.GetButtonUp ("Jump_2") : Input.GetButtonUp ("Jump_1"))) {
			isButtonJump = false;
			if (isHop && velocity.y > minHopVelocity) {
				velocity.y = minHopVelocity;
			} else if (!isHop && velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
			// Check for analog short jump (no longer up).
		} else if (!isButtonJump && input.y <= 0) {
			if (isHop && velocity.y > minHopVelocity) {
				velocity.y = minHopVelocity;
			} else if (!isHop && velocity.y > minJumpVelocity) {
				velocity.y = minJumpVelocity;
			}
		}

		// Fast-fall.
		if (velocity.y < 0 && input.y < -analogJumpSensitivity) {
			isFastFalling = true;
		}

		// Apply input to player velocity (with smoothing).
		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, 
			targetVelocityX, 
			ref (velocityXSmoothing),
			(controller.collisions.below)? aTimeGround : aTimeAir);

		// Apply gravity & fast-fall.
		if (isFastFalling) {
			velocity.y = fastFallSpeed;
		} 
		velocity.y += gravity * Time.deltaTime;

		// Call controller to execute movement.
		controller.Move (velocity * Time.deltaTime, this);

		// DEBUG panel update.
		if (onDebug) {
			debugText.text = ("isFallingVulnerable: " + ((isFallingVulnerable) ? "yes" : "no"));
		}
	}

	//-----------------------------------------------------------------------------------------------------------------  External methods
	// HOPSTUN (When you are hopper)
	public void HopStunOn (float stunTime) {
		Debug.Log ("Playing knockdownSound");
		headHopSound.Stop ();
		headHopSound.Play ();
		isStunned = true;
		anim.SetTrigger ("quickCrouch");
		Invoke ("HopStunOff", stunTime);
	}
	public void HopStunOn () {
		headHopSound.Stop ();
		headHopSound.Play ();
		isStunned = true;
		anim.SetTrigger ("quickCrouch");
		Invoke ("HopStunOff", hopStunDuration);
	}
	public void HopStunOff () {
		isStunned = false;
	}

	// HITSTUN (When you are hoppee) or other stun without animation.
	public void HitstunOnCustom (float stunTime, bool noAnim) {
		isStunned = true;
		// Adapt hitstun animation depending on ground or air.
		if (!noAnim) {
			if (controller.collisions.below) {
				anim.SetTrigger ("groundStun");
			} else {
				anim.SetTrigger ("fallBack");
			}
		}
		Invoke ("HitstunOff", stunTime);
	}
	public void HitstunOn (bool noAnim) {
		isStunned = true;
		// Adapt hitstun animation depending on ground or air.
		if (!noAnim) {
			if (controller.collisions.below) {
				anim.SetTrigger ("groundStun");
			} else {
				anim.SetTrigger ("fallBack");
			}
		}
		Invoke ("HitstunOff", hopStunDuration);
	}

	// Knock down. When the player is knocked down they are forced to be paused and fall over. 
	// They remain in the knockdown position until they make an input.
	public void KnockDown () {
		knockDownSound.Stop ();
		knockDownSound.Play ();
		velocity.x = 0;
		isKnockedDown = true;
		isKnockDownWaiting = true;
		isStunned = true;
		anim.speed = 1f;
		input.x = 0; // Zero the input to prevent instant end of animation.
		anim.SetTrigger ("knockOver");
		Invoke ("HitstunOff", knockDownDuration);
		Invoke ("EndKnockDownWaiting", knockDownDuration);
	}
	private void EndKnockDownWaiting () {
		isKnockDownWaiting = false;
	}

	public void JumpUp () {
		velocity.y = minJumpVelocity;
		Invoke ("JumpUp", 1f);
	}

	// Force a jump.
	public void ForceJump () {
		// Reset jump state.
		isFastFalling = false;
		velocity.y = maxJumpVelocity;
		controller.collisions.below = false;
		// Force jump.
		isHop = true;
		anim.speed = 1f;
		anim.SetInteger ("jumpState", 1);
		if (((isPlayerTwo) ? Input.GetButton ("Jump_2") : Input.GetButton ("Jump_1"))) {
			isButtonJump = true;
		}
	}

	public void ForceFastFall () {
		isFastFalling = true;
		velocity.y = 0;
		if (!controller.collisions.below) {
			isFallingVulnerable = true;
			Invoke ("FallSafely", hopVulnerableTime);
		}
	}

	public void FallSafely () {
		isFallingVulnerable = false;
	}

	public void BallHit () {
		ResetState ();
		// Set state.
		isHittingBall = true;
		// Hitstun.
		HitstunOnCustom (hopStunDuration * 0.75f, true);
		// Animation.
		anim.SetTrigger ("ballHit");
		Invoke ("EndBallHitAnim", 0.3f);
		Invoke ("StopHittingBall", ballHitCooldown);
	}

	public void Win () {
		velocity.y = minJumpHeight;
		velocity.x = 0;
		isBouncing = true;
		anim.SetTrigger ("win");
	}

	public void ResetState () {
		// Reset all state booleans.
		isStunned = false;
		isKnockDownWaiting = false;
		isFallingVulnerable = false;
		isKnockedDown = false;
	}

	//-----------------------------------------------------------------------------------------------------------------  Internal methods
	// If debug mode is on, look for any debug keys that do special debug stuff.
	private void GetDebugInput () {
		// Test hitstun with "1" alphanumeric key.
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			Debug.Log ("Testing Hitstun");
			HitstunOnCustom (0.5f, false);
		}
		// Test knockDown with "2" alphanumeric key.
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			Debug.Log ("Testing Knockback");
			KnockDown ();
		}
		// Test knockDown with "3" alphanumeric key.
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			Debug.Log ("Testing ballHit animation");
			anim.SetTrigger ("ballHit");
			Invoke ("EndBallHitAnim", 0.3f);
		}
	}
	// Invokes.
	private void EndBallHitAnim () {
		anim.SetTrigger ("endBallHit");
	}
	private void StopHittingBall () {
		isHittingBall = false;
	}
	private void HitstunOff () {
		isStunned = false;
	}
}
