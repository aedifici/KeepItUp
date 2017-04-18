using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 *   GRADIENT (GENERATOR)
 * ------------------------------------------
 *   Requires a vertex colored shader. 
 *   Using a start and end color, generates
 *   a linear gradient.
 */

public class Gradient : MonoBehaviour {

	public Color startColor;
	public Color endColor;

	void Start () {
		MeshFilter meshFilter = GetComponent<MeshFilter> ();
		Color[] colors = new Color [meshFilter.mesh.vertices.Length];
		colors[0] = startColor;
		colors[1] = endColor;
		colors[2] = startColor;
		colors[3] = endColor;
		meshFilter.mesh.colors = colors;
	}
}
