using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
	#region Public variables

	public bool didInit;

	[System.NonSerialized]
	public bool answeredRight;

	public static GameController instance = null;

	public enum GameState { initializing, mainMenu, turnIntro, showingImage, guessing, passPhone, showTurnResult, showGameResult };

	public static GameState currentState;
	public static GameState targetState;

	public GameData currentGame;

	#endregion

	#region Private variables

	private static bool created = false;

	private int describePlayer;
	private int totalPlayers;
	private int scoreToWin;

	#endregion

	#region Set up and initialization

	void Awake ()
	{
		if ( !created )
		{
			DontDestroyOnLoad (this.gameObject);
			created = true;

			//Check if instance already exists
			if ( instance == null )
			{
				//if not, set instance to this
				instance = this;
			}
			else
			{
				if ( instance != this )
					Destroy (gameObject);
			}

			// make sure we start everything from the title scene..
			if ( SceneManager.GetActiveScene () != SceneManager.GetSceneByName ("TitleScene") )
			{
				SceneManager.LoadScene ("TitleScene");
			}
		}
	}

	void Start ()
	{
		if ( !didInit )
			Init ();
	}

	void Init ()
	{
		didInit = true;

		currentState = GameState.initializing;
		targetState = GameState.initializing;
	}

	void SetupPlayers ()
	{
		currentGame.PlayerList = new List<Player> ();

		for ( int i = 0; i < currentGame.totalPlayers; i++ )
		{
			Player aPlayer = new Player ();
			aPlayer.ID = i;
			aPlayer.isNetworked = false;

			currentGame.PlayerList.Add (aPlayer);
		}
	}

	void SetupGame ()
	{
		Debug.Log ("GameController>SetupGame() called.");

		// here, we set up a new game (using GameData class to store our game info) and reset the current round counter..
		currentGame = new GameData ();
		currentGame.totalPlayers = totalPlayers;

		// spawn some players
		SetupPlayers ();

		// who will be the first describer the game??
		// describePlayer = Random.Range (0, currentGame.PlayerList.Count);  // <-- Uncomment to randomize who will be first to describe
		describePlayer = 0; // <-- Uncomment to keep it fixed at player 1 starting each time
		
		//targetState = GameState.roundIntro;	// <-- uncomment for a round-based game
		targetState = GameState.turnIntro;      // <-- uncomment for a non-round based game
	}

	#endregion

	#region Main loop and Game state changes

	void Update ()
	{
		if ( targetState != currentState )
			UpdateGameState ();
	}

	void UpdateGameState ()
	{
		switch ( targetState )
		{
			case GameState.mainMenu:
				SceneManager.LoadScene ("MenuScene");
				currentState = GameState.mainMenu;
				break;

			case GameState.turnIntro:
				SceneManager.LoadScene ("TurnIntroScene");
				currentState = GameState.turnIntro;
				break;

			case GameState.showingImage:
				SceneManager.LoadScene ("ImageScene");
				currentState = GameState.showingImage;
				break;

			case GameState.passPhone:
				// increase the player we're dealing with
				currentGame.currentPlayer++;

				// make sure we haven't just gone over the actual number of players
				if ( currentGame.currentPlayer >= currentGame.PlayerList.Count )
					currentGame.currentPlayer = 0;

				// go ahead and load the pass the phone scene now!
				SceneManager.LoadScene ("PassThePhone");
				currentState = GameState.passPhone;
				break;

			case GameState.guessing:
				SceneManager.LoadScene ("GuessScene");
				currentState = GameState.guessing;
				break;

			case GameState.showTurnResult:
				SceneManager.LoadScene ("TurnResultsScene");
				currentState = GameState.showTurnResult;
				break;

			case GameState.showGameResult:
				// here, we need to check the scores before we load the next scene..
				int aWin = CheckForWin ();
				if ( aWin > -1 )
				{
					currentGame.winner = aWin;
					SceneManager.LoadScene ("GameOverScene");
				}
				else
				{
					SceneManager.LoadScene ("CurrentScoreScene");
				}
				currentState = GameState.showGameResult;
				break;
		}
	}

	#endregion

	#region Guess processing

	void PlayerGuessTrue ()
	{
		Debug.Log ("GameController>PlayerGuessTrue() called for player " + currentGame.currentPlayer.ToString () + ".");

		if ( currentGame.isTruth )
		{
			Debug.Log ("GameController>PlayerGuessTrue()>CORRECT answer detected.");

			// the answer is RIGHT
			answeredRight = true;
			currentGame.PlayerList[currentGame.currentPlayer].rightAnswers++;
			currentGame.PlayerList[currentGame.currentPlayer].score++;
		}
		else
		{
			Debug.Log ("GameController>PlayerGuessTrue()>WRONG answer detected.");

			// the answer is WRONG
			answeredRight = false;
			currentGame.PlayerList[currentGame.currentPlayer].wrongAnswers++;
			Player other = GetCurrentDescriber ();
			other.score++;
		}

		// move on to the next state
		targetState = GameState.showTurnResult;
	}

	void PlayerGuessFalse ()
	{
		Debug.Log ("GameController>PlayerGuessFalse() called for player " + currentGame.currentPlayer.ToString () + ".");

		if ( currentGame.isTruth )
		{
			Debug.Log ("GameController>PlayerGuessFalse()>WRONG answer detected.");

			// the answer is WRONG
			answeredRight = false;
			currentGame.PlayerList[currentGame.currentPlayer].wrongAnswers++;

			// since the answer is wrong, award the describer a point instead of 'this' player
			Player other = GetCurrentDescriber ();
			other.score++;
		}
		else
		{
			Debug.Log ("GameController>PlayerGuessFalse()>RIGHT answer detected.");

			// the answer is RIGHT
			answeredRight = true;

			// update right answer count on the current player
			currentGame.PlayerList[currentGame.currentPlayer].rightAnswers++;
			currentGame.PlayerList[currentGame.currentPlayer].score++;
		}

		// move on to the next state
		targetState = GameState.showTurnResult;
	}

	#endregion

	#region Turn updates

	int CheckForWin ()
	{
		// go through all players to see if anyone has 2 points or more..
		int whichPlayer = -1;

		for ( int i = 0; i < currentGame.PlayerList.Count; i++ )
		{
			if ( currentGame.PlayerList[i].score >= scoreToWin )
				whichPlayer = i + 1;
		}

		return whichPlayer;
	}

	void PlayerTurnUpdate ()
	{
		Debug.Log ("GameController>PlayerTurnUpdate()>currentGame.totalPlayers=" + currentGame.totalPlayers);

		// first, we find the player whose turn it is to describe..
		for ( int i = 0; i < currentGame.totalPlayers; i++ )
		{
			if ( currentGame.PlayerList[i].isMyTurnToDescribe )
				describePlayer = i;
		}

		// now we clear the describe flag on that player and move it to the next..
		currentGame.PlayerList[describePlayer].isMyTurnToDescribe = false;

		// now we move the describer to the next player
		int newDescriber = describePlayer + 1;

		Debug.Log ("GameController>PlayerTurnUpdate()>newDescriber = " + newDescriber);

		// .. but make sure that the index doesn't go too high (loop it around if it does)
		if ( newDescriber >= currentGame.totalPlayers )
			newDescriber = 0;

		currentGame.PlayerList[newDescriber].isMyTurnToDescribe = true;

		Debug.Log ("GameController>PlayerTurnUpdate()>Set describer to " + newDescriber + ".");
	}

	Player GetCurrentDescriber ()
	{
		// first, we find the player whose turn it is to describe..
		for ( int i = 0; i < currentGame.totalPlayers; i++ )
		{
			if ( currentGame.PlayerList[i].isMyTurnToDescribe )
				describePlayer = i;
		}
		return currentGame.PlayerList[describePlayer];
	}

	void NextRound ()
	{
		// first, call the function to move on the player pointer so that the describer will be the next player in line
		PlayerTurnUpdate ();

		currentGame.currentPlayer = GetCurrentDescriberID ();

		// with the next player lined up, we can move on to the next round

		//targetState = GameState.roundIntro;	// <-- uncomment for a round-based game
		targetState = GameState.turnIntro;      // <-- uncomment for a non-round based game
	}

	#endregion

	#region Public Functions

	// -----------------------------------------------------------------------------------------------
	// PUBLIC
	// -----------------------------------------------------------------------------------------------

	public int GetScore ( int whichPlayer )
	{
		return currentGame.PlayerList[whichPlayer].score;
	}

	public void LoadMainMenu ()
	{
		targetState = GameState.mainMenu;
	}

	public void StartGame ( int howManyPlayers, int howManyToWin )
	{
		Debug.Log ("GameController>StartGame(" + howManyPlayers.ToString () + ") called.");
		currentState = GameState.initializing;
		totalPlayers = howManyPlayers;
		scoreToWin = howManyToWin;

		SetupGame ();
	}

	public void StartRound ()
	{
		// get the current player assigned to be the first describer
		currentGame.currentPlayer = GetCurrentDescriberID ();
		targetState = GameState.turnIntro;
	}

	public void StartTurn ()
	{
		// first we decide whether or not this turn will be truthful
		currentGame.isTruth = false;
		if ( Random.Range (0, 100) > 50 )
			currentGame.isTruth = true;

		targetState = GameState.showingImage;
	}

	public void FinishedViewing ()
	{
		targetState = GameState.passPhone;
	}

	public void FinishedPassing ()
	{
		targetState = GameState.guessing;
	}

	public void PlayerChoseTruth ()
	{
		PlayerAnsweredTrue ();
	}

	public void PlayerChoseLie ()
	{
		PlayerAnsweredFalse ();
	}

	public int GetCurrentPlayerNum ()
	{
		return currentGame.currentPlayer + 1;
	}

	public int GetCurrentRoundNum ()
	{
		return currentGame.roundNum + 1;
	}

	public int GetCurrentDescriberID ()
	{
		return GetCurrentDescriber ().ID;
	}

	public bool GetTruthStatus ()
	{
		return currentGame.isTruth;
	}

	public void PlayerAnsweredTrue ()
	{
		PlayerGuessTrue ();
	}

	public void PlayerAnsweredFalse ()
	{
		PlayerGuessFalse ();
	}

	public void TurnResultsEnded ()
	{
		targetState = GameState.showGameResult;
	}

	public void RoundResultsEnded ()
	{
		NextRound ();
	}

	public bool DidAnswerRight ()
	{
		return answeredRight;
	}

	public int GetWinner()
	{
		return currentGame.winner;
	}

	public void GameOverDone()
	{
		LoadMainMenu ();
	}

	#endregion

}

public class GameData
{
	public int roundNum;
	public int totalPlayers;
	public int currentPlayer;
	public bool isTruth;
	public int winner;

	public List<Player> PlayerList;
}

public class Player
{
	public int ID;
	public bool isNetworked;

	public bool isMyTurnToDescribe;

	public int score;
	public string name;
	public int wrongAnswers;
	public int rightAnswers;
}