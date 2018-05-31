using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverControl : MonoBehaviour
{
	public Text _finalScore;
	public Text _winnerText;

	void Start ()
	{
		int player1Score = GameController.instance.GetScore (0);
		int player2Score = GameController.instance.GetScore (1);

		_finalScore.text = "FINAL SCORES\nTEAM 1: " + player1Score.ToString () + "\nTEAM 2: " + player2Score.ToString ();
		_winnerText.text = "Team " + GameController.instance.GetWinner () + "\nWINS!";
	}

	public void GameOverDone ()
	{
		GameController.instance.GameOverDone ();
	}
}
