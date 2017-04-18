using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* 
 *   REF (REFERENCE) DUMP 
 * ------------------------------------------
 *   Holds all external references for the
 *   GameManager to get back upon starting
 *   a new match.
 */

public class RefDump : MonoBehaviour {

	public  Text        timerText;
	public  Text        timerOutline;
	public  Text        girlScoreText;
	public  Text        girlScoreOutline;
	public  Text        boyScoreText;
	public  Text        boyScoreOutline;
	public  Text        countdownText;
	public  Text        countdownOutline;
	public  Image       WinIconL1;
	public  Image       WinIconL2;
	public  Image       WinIconR1;
	public  Image       WinIconR2;
	public  GameObject  spotlightBoy;
	public  GameObject  spotlightGirl;
	public  AudioSource music;
	public  AudioSource announcerClip;
	public  EndMessages endMessageClips;

	public  Player      boy;
	public  Player      girl;
	public  Ball        ball;
	public  GameObject  arrow;
	public  GameObject  UI;

	void Start () {
		GameManager game = GameObject.Find ("GameManager").GetComponent<GameManager> ();
		game.Setup ();
	}
}
