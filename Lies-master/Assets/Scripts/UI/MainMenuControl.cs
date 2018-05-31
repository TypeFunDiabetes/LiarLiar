using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour
{
	public int maxPlayers = 2;
	public Text _players;

	private int playerCount;

	void Start ()
	{
		playerCount = 2;
		UpdatePlayerDisplay ();
	}

	void UpdatePlayerDisplay ()
	{
		_players.text = playerCount.ToString ();
	}

	public void IncreasePlayerCount ()
	{
		playerCount++;
		if ( playerCount > maxPlayers )
			playerCount = maxPlayers;

		UpdatePlayerDisplay ();
	}

	public void DecreasePlayerCount ()
	{
		playerCount--;
		if ( playerCount < 2 )
			playerCount = 2;

		UpdatePlayerDisplay ();
	}

	public void StartGame1 ()
	{
		GameController.instance.StartGame (playerCount,5);
	}

	public void StartGame2 ()
	{
		GameController.instance.StartGame (playerCount,2);
	}

	public void ExitGame()
	{
		Application.Quit ();
	}
}
