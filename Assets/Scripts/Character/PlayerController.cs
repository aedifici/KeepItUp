using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 *   PLAYER CONTROLLER
 * ------------------------------------------
 *   Executes collisions and movement mechanics
 *   related to the player.
 */

[RequireComponent (typeof (BoxCollider2D))]
public class PlayerController : MonoBehaviour {
	// Constants.
	const   float          skinWidth          = 0.015f;
	// Controls.
	public  float          maxClimbAngle      = 80; 		// Maximum angle the player can climb before falling.
	public  float          maxDescendAngle    = 75;			// Maximum angle the player can descend before just falling. 
	public  int            horizontalRayCount = 4;			// How many rays the player collider fires horizontally...
	public  int			   verticalRayCount   = 4;			// ... and vertically. Higher improves accuracy but lowers performance.
	public  LayerMask      collisionMask;					// What layers the player's collider will collide with.
	public  CollisionInfo  collisions;						// The collision handler struct, made public so the player can check it directly.
	// Runtime.
	private float          horizontalRaySpacing;			// How far apart rays are horizontally...
	private float          verticalRaySpacing;				// ... vertically.
	private BoxCollider2D  hitbox;							// The boxCollider around the player.
	private RaycastOrigins raycastOrigins;					// Runtime struct to dynamically store the locations that rays fire from.

	// Raycast struct. 
	private struct RaycastOrigins {  						// Storage struct for the positions
		public Vector2 topLeft;      						//  of each of the boxCollider's 
		public Vector2 topRight;     						//  4 corners, to become the 
		public Vector2 bottomLeft;   						//  origin points of the collision
		public Vector2 bottomRight;  						//  raycasts.
	}

	// Collision struct. Keeps track of where collisions are currently detected.
	public struct CollisionInfo {
		public bool    above;
		public bool    below;
		public bool    left;
		public bool    right;
		public bool    climbSlope;
		public bool    descendSlope;
		public float   angle;
		public float   angleOld;
		public Vector3 velocityOld;

		public void Reset () {
			above = false;
			below = false;
			left  = false;
			right = false;
			climbSlope   = false;
			descendSlope = false;

			angleOld = angle;
			angle = 0;
		}
	}

	void Start () {
		// Setup. 
		hitbox = GetComponent<BoxCollider2D> ();
		CalculateRaySpacing  ();
	}

	//-----------------------------------------------------------------------------------------------------------------  Move
	// Takes velocity from Player, applies any coliisions, and then uses it to translate the Player.
	public void Move (Vector3 velocity, Player player) {
		// Update origin points of rays and reset collisions struct.
		UpdateRaycastOrigins ();
		collisions.Reset     ();
		collisions.velocityOld = velocity;
		// Check for collisions, adjusting velocity if one is found.
		if (velocity.y  < 0) DescendSlope         (ref velocity); 
		if (velocity.x != 0) HorizontalCollisions (ref velocity);
		if (velocity.y != 0) VerticalCollisions   (ref velocity, ref player);
		// Move the player using updated velocity.
		transform.Translate  (velocity);
	}

	//-----------------------------------------------------------------------------------------------------------------  Horizontal Collisions
	// Casts rays left and right to check for horizontal collisions.
	private void HorizontalCollisions (ref Vector3 velocity) {
		// Get direction of velocity, horizontally, as either 1 (right) or -1 (left)
		// then adjust the velocity to set a positive adjusted ray length.
		float directionX = Mathf.Sign (velocity.x);
		float rayLength  = Mathf.Abs (velocity.x) + skinWidth;

		// Draw rays
		for (int i = 0; i < horizontalRayCount; i++) {
			// Set starting ray position based on direction of player, then adjust origin based on iteration.
			Vector2 rayOrigin = (directionX == -1)? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			// Fire a 2D physics raycast to check for collision with anything within the layer mask. True on hit.
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			// Draw as visible red in debug.
			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);
			// On hit, adjust velocity.
			if (hit) {
				// Find the slope angle (same as between hit normal and global up).
				float slope = Vector2.Angle (hit.normal, Vector2.up);
				// On a climbable slope, adjust velocity differently.
				if (i == 0 && slope <= maxClimbAngle) {
					// Fix simultaneous slope collisions (V shape).
					if (collisions.descendSlope) {
						collisions.descendSlope = false;
						velocity = collisions.velocityOld;
					}
					// Ensure proper slope collision (close gap from ray hit distance).
					float distanceToSlope = 0;
					// Check for new slope.
					if (slope != collisions.angleOld) {
						distanceToSlope = hit.distance - skinWidth;
						velocity.x -= distanceToSlope * directionX;
					}
					ClimbSlope (ref velocity, slope);
					velocity.x += distanceToSlope * directionX;
				}
				// Only check other rays for collisions if not on slope.
				if (!collisions.climbSlope || slope > maxClimbAngle) {
					// Adjust velocity.
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength  = hit.distance;
					// Fix collisions while on a slope.
					if (collisions.climbSlope) { 
						velocity.y = Mathf.Tan (collisions.angle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
					}
					// Update collisions.
					collisions.left  = directionX == -1;
					collisions.right = directionX ==  1;
				}
			}
		}
	}

	//-----------------------------------------------------------------------------------------------------------------  Vertical Collisions
	// Casts rays up and down to check for vertical collisions.
	private void VerticalCollisions (ref Vector3 velocity, ref Player player) {
		// Get direction of velocity, vertically, as either 1 (up) or -1 (down)
		// then adjust the velocity to set a positive adjusted ray length.
		float directionY = Mathf.Sign (velocity.y);
		float rayLength  = Mathf.Abs (velocity.y) + skinWidth;

		// Draw rays.
		for (int i = 0; i < verticalRayCount; i++) {
			// Set first ray origin based on direction of ray, then adjust origin based on iteration.
			Vector2 rayOrigin = (directionY == -1)? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
			// Fire a 2D physics raycast to check for collision with anything within the layer mask. True on hit.
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
			// Draw as visible red in debug.
			Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red);
			// On hit, adjust velocity.
			if (hit) {
				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength  = hit.distance;
				// Fix collisions while on a slope.
				if (collisions.climbSlope) { 
					velocity.x = velocity.y / Mathf.Tan (collisions.angle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}
				// Update collisions.
				collisions.below = directionY == -1;
				collisions.above = directionY ==  1;

				// Check for head hop on a victim.
				if (collisions.below && hit.collider.gameObject.tag == "Player") {
					// Send a ForceJump to the jumper and an Interupt to the victim.
					player.ForceJump ();
					player.HopStunOn ();
					Player otherPlayer = hit.collider.gameObject.GetComponent<Player> ();
					otherPlayer.ForceFastFall ();
					otherPlayer.HitstunOn (false);
				}
			}
		}
		// Fix frame skip between slopes.
		if (collisions.climbSlope) {
			// Create and cast a new ray.
			float directionX = Mathf.Sign (velocity.x);
			rayLength = Mathf.Abs (velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1)? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
			// On hit, adjust to new ray.
			if (hit) {
				float slope = Vector2.Angle (hit.normal, Vector2.up);
				// Check for new slope and adjust accordingly.
				if (slope != collisions.angle) {
					velocity.x = (hit.distance - skinWidth) * directionX;
					collisions.angle = slope;
				}
			}
		}
	}
		
	//-----------------------------------------------------------------------------------------------------------------  Climb Slope
	// Adjusts velocity based on a slope angle, going up.
	private void ClimbSlope (ref Vector3 velocity, float angle) {
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (angle * Mathf.Deg2Rad) * moveDistance; 
		// Check if not jumping on slope before adjusting.
		if (velocity.y < climbVelocityY) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (angle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			// Apply lower collision (enable jump) and indicate slope collision.
			collisions.below = true;
			collisions.climbSlope = true;
			collisions.angle = angle;
		}
	}

	//-----------------------------------------------------------------------------------------------------------------  Descend Slope
	// Adjusts velocity based on a slope angle, going down.
	private void DescendSlope (ref Vector3 velocity) {
		// Cast a ray down, either right or left corner based on horizontal direction.
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (directionX == -1)? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
		// On hit, adjust velocity.
		if (hit) {
			float angle = Vector2.Angle (hit.normal, Vector2.up);
			// Check for valid slope then adjust to hit.
			if (angle != 0 && angle <= maxDescendAngle) {
				if (Mathf.Sign (hit.normal.x) == directionX) {
					if (hit.distance - skinWidth <= Mathf.Tan (angle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x)) {
						float moveDistance = Mathf.Abs (velocity.x);
						float descendVelocityY = Mathf.Sin (angle * Mathf.Deg2Rad) * moveDistance; 
						velocity.x = Mathf.Cos (angle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descendVelocityY;
						// Update collisions.
						collisions.angle        = angle;
						collisions.descendSlope = true;
						collisions.below        = true;
					}
				}
			}
		}
	}

	//-----------------------------------------------------------------------------------------------------------------  Update Raycast Origins
	// Updates the origin points of the collision rays along the box collider.
	private void UpdateRaycastOrigins () {
		// Get bounds of the BoxCollider and expand by skinwidth.
		Bounds bounds = hitbox.bounds;
		bounds.Expand (skinWidth * -2);
		// Set new origins using bounds.
		raycastOrigins.bottomLeft  = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft     = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight    = new Vector2 (bounds.max.x, bounds.max.y);
	}

	//-----------------------------------------------------------------------------------------------------------------  Calculate Ray Spacing
	// Calculates the count and spaces between each ray along the horizontal or
	// vertical edges of the box collider.
	private void CalculateRaySpacing () {
		// Get bounds of the BoxCollider and expand by skinwidth.
		Bounds bounds = hitbox.bounds;
		bounds.Expand (skinWidth * -2);
		// Ensure there are at least 2 rays in each direction.
		horizontalRayCount   = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount     = Mathf.Clamp (verticalRayCount,   2, int.MaxValue);
		// Calculate spacing based on bound size along edge and ray count.
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing   = bounds.size.x / (verticalRayCount   - 1);
	}
}
