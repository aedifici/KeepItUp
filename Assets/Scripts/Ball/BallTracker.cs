using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 *   BALL TRACKER 
 * ------------------------------------------
 *   Constantly keeps track of the ball's
 *   position and updates the UI arrow when 
 *   necessary.
 */

[RequireComponent (typeof (SpriteRenderer))]
public class BallTracker : MonoBehaviour {
	// References.
	public  Transform ball; 							// Star of the show.
	// Control.
	public  float     maxDistance;						// How far it takes for the hand to be at it's max transparency.
	public  float     minAlpha;							// How transparent the hand can get.
	// Runtime.
	private int       currentSector;					// The sector of the screen that the ball is currently residing in (0-8 inc. where 0 is center).
	private Vector2   screen;							// The camera's width and height (x, y) in Unity units.
	// Dependancies.
	private Camera cam;
	private SpriteRenderer rend;

	private void Start () {
		cam = Camera.main;
		rend = GetComponent<SpriteRenderer> ();
	}

	private void Update () {
		// Get camera width/height.
		screen.y = 2 * Camera.main.orthographicSize;
		screen.x = screen.y * Camera.main.aspect;

		// Find what sector. 
		// Check horizontal (x).
		if (ball.position.x < cam.transform.position.x - (screen.x / 2)) {
			// Can be 1, 5, or 7 depending on vertical (y).
			if (ball.position.y > cam.transform.position.y + (screen.y / 2)) {
				// Sector 1.
				currentSector = 1;
			}
			else if (ball.position.y < cam.transform.position.y - (screen.y / 2)) {
				// Sector 7.
				currentSector = 7;
			}
			else {
				// Sector 5.
				currentSector = 8;
			}
		}
		else if (ball.position.x > cam.transform.position.x + (screen.x / 2)) {
			// Can be 3, 4, or 5 depending on vertical (y).
			if (ball.position.y > cam.transform.position.y + (screen.y / 2)) {
				// Sector 3.
				currentSector = 3;
			}
			else if (ball.position.y < cam.transform.position.y - (screen.y / 2)) {
				// Sector 5.
				currentSector = 5;
			}
			else {
				// Sector 4.
				currentSector = 4;
			}
		}
		else {
			// Can be 2, 0, or 6 depending on vertical (y).
			if (ball.position.y > cam.transform.position.y + (screen.y / 2)) {
				// Sector 2.
				currentSector = 2;
			}
			else if (ball.position.y < cam.transform.position.y - (screen.y / 2)) {
				// Sector 6.
				currentSector = 6;
			}
			else {
				// Sector 0.
				currentSector = 0;
			}
		}

		// Place the indicator accordingly.
		rend.enabled = true;
		if (currentSector == 1) {
			// Top left.
			transform.position = new Vector3 (cam.transform.position.x - (screen.x / 2) + 0.5f, cam.transform.position.y + (screen.y / 2) - 0.5f, transform.position.z);
			transform.eulerAngles = new Vector3 (0,0,45f);
		} else if (currentSector == 3) {	
			// Top right.
			transform.position = new Vector3 (cam.transform.position.x + (screen.x / 2) - 0.5f, cam.transform.position.y + (screen.y / 2) - 0.5f, transform.position.z);
			transform.eulerAngles = new Vector3 (0,0,-45f);
		} else if (currentSector == 5) {
			// Bottom right.
			transform.position = new Vector3 (cam.transform.position.x + (screen.x / 2) - 0.5f, cam.transform.position.y - (screen.y / 2) + 0.5f, transform.position.z);
			transform.eulerAngles = new Vector3 (0,0,-120f);
		} else if (currentSector == 7) {	
			// Bottom left.
			transform.position = new Vector3 (cam.transform.position.x - (screen.x / 2) + 0.5f, cam.transform.position.y - (screen.y / 2) + 0.5f, transform.position.z);
			transform.eulerAngles = new Vector3 (0,0,120f);
		} else if (currentSector == 2) {
			// Above.
			transform.eulerAngles = new Vector3 (0,0,0);
			transform.position = new Vector3 (ball.transform.position.x, cam.transform.position.y + (screen.y / 2) - 0.5f, transform.position.z);
		} else if (currentSector == 4) {
			// Rightward.
			transform.eulerAngles = new Vector3 (0,0,-90f);
			transform.position = new Vector3 (cam.transform.position.x + (screen.x / 2) - 0.5f, ball.transform.position.y, transform.position.z);
		} else if (currentSector == 6) {
			// Below.
			transform.eulerAngles = new Vector3 (0,0,180f);
			transform.position = new Vector3 (ball.transform.position.x, cam.transform.position.y - (screen.y / 2) + 0.5f, transform.position.z);
		} else if (currentSector == 8) {
			// Leftward.
			transform.eulerAngles = new Vector3 (0,0,90f);
			transform.position = new Vector3 (cam.transform.position.x - (screen.x / 2) + 0.5f, ball.transform.position.y, transform.position.z);
		} else {
			rend.enabled = false;
		}

		// Adjust alpha based on distance to ball.
		float distanceToHand = Vector3.Distance (transform.position, ball.position);
		float percentage = Mathf.Clamp (maxDistance / distanceToHand, minAlpha, 1);
		rend.color = new Color (1,1,1,percentage);
	}
}
