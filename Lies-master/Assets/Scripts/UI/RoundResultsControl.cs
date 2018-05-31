using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundResultsControl : MonoBehaviour
{
	public Text playerScore;

	void Start ()
	{
		int player1Score = GameController.instance.GetScore (0);
		int player2Score = GameController.instance.GetScore (1);

		playerScore.text = "TEAM 1\n" + player1Score.ToString () + "\n\nTEAM 2\n" + player2Score.ToString ();
	}

	public void RoundResultsDone()
	{
		GameController.instance.RoundResultsEnded ();
	}
}
