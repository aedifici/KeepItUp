using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 *   STAGE CAMERA CONTROLLER
 * ------------------------------------------
 *   Looks at both players and scales the 
 *   camera to adjust.
 */

[RequireComponent (typeof (Camera))]
public class StageCameraController : MonoBehaviour {
	// References.
	public  Transform  player1;
	public  Transform  player2;
	public  GameObject boundaryL;
	public  GameObject boundaryR;
	// Controls.
	public  float      stageWidth;							// How many unity units wide the stage is.
	public  float      translateSpeed;						// The speed at which the camera adjusts to player center.
	public  float      scaleSpeed;							// The speed at which the camera changes it's scale.
	public  float      verticalOffset;						// The camera y-axis position relative to camera ortho-size.
	public  float      sizeMargin;							// Amount to scale the camera past the players.
	public  float      ceilingMargin;						// The margin to keep above the tallest player.
	public  float      minSize;								// The smallest the camera can get.
	public  float      maxSize;								// The largest the camera can get.
	public  float      minDistance;							// The closest players can be that affects camera scale.
	public  float      maxDistance;							// The farthest players can be that affects camera scale.
	// Runtime.
	private float      playerDistance;						// The distance between the players.
	private float      distancePercent;						// The percentage of playerDistance to the max distance.
	private float      targetCamSize;						// The cameraSize that the camera updates towards, at the desired scale speed.
	private float      targetHeight;						// The vertical position that the camera updates towards, at the desired translate speed.
	private Vector3    playerAveragePos;					// The position exactly in the middle between the players, ignoring vertical.
	private Vector2    screen;								// The screen width (x) and height (y) in unity units. Calculated each frame.
	// Dependancies.
	private Camera     cam;									// The main camera component.

	void Start () {
		cam = GetComponent<Camera> ();
	}

	void Update () {
		//-----------------------------------------------------------------------------------------------------------------  Scale size
		// Calculate desired cam size based on character distance.
		// Find horizontal distance between players, clamped to min/max.
		playerDistance = Mathf.Clamp (Mathf.Abs (player1.position.x - player2.position.x), 0, maxDistance);

		// Player height scaling.
		// Find height of each player and don't let playerDistance get lower than that height + a margin.
		playerDistance = Mathf.Clamp (playerDistance, player1.transform.position.y - transform.position.y + 1f + ceilingMargin, maxDistance);
		playerDistance = Mathf.Clamp (playerDistance, player2.transform.position.y - transform.position.y + 1f + ceilingMargin, maxDistance);

		// Find percentage from min to max distance.
		distancePercent = playerDistance / maxDistance;

		// Scale desired cam size from min to max camera size based on percentage.
		targetCamSize = Mathf.Clamp (((maxSize + minSize) * distancePercent) + sizeMargin, minSize, maxSize);

		// Constantly move towards desired size.
		cam.orthographicSize = Mathf.SmoothStep (cam.orthographicSize, targetCamSize, scaleSpeed * Time.deltaTime);

		// Adjust cam vertical relative to size.
		transform.position = new Vector3 (transform.position.x, Mathf.SmoothStep (transform.position.y, targetCamSize - verticalOffset, scaleSpeed * Time.deltaTime), transform.position.z);
	
		// Calculate screen size in unity units.
		screen.y = 2 * Camera.main.orthographicSize;
		screen.x = screen.y * Camera.main.aspect;

		//-----------------------------------------------------------------------------------------------------------------  Adjust position
		// Update the center (average) position of both players, with no vertical or depth variation from current camera position.
		playerAveragePos = new Vector3 ((player1.position.x + player2.position.x)/2, transform.position.y, transform.position.z);

		// Clamp movement to prevent going past level limit.
		// Check left bound limit.
		if (playerAveragePos.x + (screen.x / 2f) > stageWidth / 2f) {
			playerAveragePos = new Vector3 ((stageWidth / 2f) - (screen.x / 2f), playerAveragePos.y, playerAveragePos.z);
		// Check right bound limit.
		} else if (playerAveragePos.x - (screen.x / 2f) < -stageWidth / 2f) {
			playerAveragePos = new Vector3 ((-stageWidth / 2f) + (screen.x / 2f), playerAveragePos.y, playerAveragePos.z);
		}
		// Smoothly move towards that center point.
		transform.position = Vector3.MoveTowards (transform.position, playerAveragePos, translateSpeed * Time.deltaTime);

		// Adjust boundaries to edges of camera.
		boundaryL.transform.position = new Vector3 (transform.position.x - screen.x / 2f - 0.5f, boundaryL.transform.position.y, boundaryL.transform.position.z);
		boundaryR.transform.position = new Vector3 (transform.position.x + screen.x / 2f + 0.5f, boundaryR.transform.position.y, boundaryR.transform.position.z);
		/* [REMOVED] Either this block, or player height scaling, but never both.
		// Adjust height relative to highest player.
		targetHeight = (transform.position.y + screen.y / 2) - ceilingMargin;
		if (player1.position.y > player2.position.y) {
			if (player1.position.y > targetHeight) 
				transform.position = new Vector3 (transform.position.x, transform.position.y + (player1.position.y - targetHeight), transform.position.z);
		} else {
			if (player2.position.y > targetHeight) 
				transform.position = new Vector3 (transform.position.x, transform.position.y + (player2.position.y - targetHeight), transform.position.z);
		}
		*/
	}
}
