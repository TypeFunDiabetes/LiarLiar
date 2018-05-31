Welcome!

Hopefully this doc will make it a bit easier to see how the game is structured and what it's doing.. good luck!

NOTE:
You MUST run the game from the TitleScene scene. At the moment, you will NOT be able to start the game from anywhere other than the TitleScene scene. See the note in the 'scripts' section for reasoning ;)



SCENES

Each part of the game is split into a different scene. Normally I like to work in a single scene, but I figured that splitting it up like this would make replacing the UI easier and easier for adding new game states later (such as adding network lobbies etc at a later date).


TitleScene  - Has the first instance of GameController in it to take control of scene loading etc.

MenuScene - The main menu

TurnIntroScene - This is the one you currently see at the start of the game that shows 'Team 1 ready?' etc.

ImageScene - When this scene loads, PolyImageManager (attached to a gameobject in there) will download the Poly model from Google. The Poly model just gets spawned wherever the ImageManager gameObject is positioned in the scene, so if you were going to make this AR you could just reposition ImageManager gameObject to wherever you want on your surface. The spawned model will automatically be made of child object of ImageManager by the PolyImageManager script (line 149, in the ImportAssetCallback() function)

PassThePhone - Just a little message screen to tell the user to give the phone to the other team

GuessScene - Where the 'other' team gets to say whether they thought it was a guess or lie

TurnResultsScreen - Shows whether the guess was correct or wrong

CurrentScoreScene - Shows the current scores for both teams

GameOverScene - Shown when the target score is reached and the game ends.



SCRIPTS

There are two main scripts in the Scripts folder:
GameController.cs
PolyImageManager.cs

All of the other scripts are UI (in the Scripts/UI folder) or come from the Google Poly libs.

NOTE: The GAMECONTROLLER prefab in the Prefabs folder is there for convenience. At the moment, the gamecontroller is initialized in the TitleScene scene, which means that to play the game you have to run it from TitleScene. Trying to run the game from anywhere that doesn't have the GAMECONTROLLER prefab in it will error out unless you drop a new GAMECONTROLLER prefab into the scene you're trying to start from. This is because game controller deals not just with the game, but all of the scene loading and everything around the game too.

GameController.cs takes care of all of the game states and it's set to never destroy once it's created. It's also a Singleton so if another instance tries to spawn, it'll wipe it out so that there's only ever one instance running. As there's only one instance FOREVER, you need to be a bit wary of resetting variables when a new game starts because they'll be alive/hold their values right through the menus and throughout the whole app.

All of the game flow is in GameController.cs, mostly controlled by the state of currentGameState and targetGameState. To move the game to a different state, you just set targetGameState. In the Update loop, we look to see if currentGameState or targetGameState are different to one and other and, if they are, it'll call UpdateGameState() to kick the next state into life. So, for the start of each state you'll find scene loading or state setup in the Switch/case inside UpdateGameState().

The UI calls to GameController when buttons are pressed (firing functions in the 'public' section of GameController). The UI is almost completely seperate from everything else, except for these button calls and a few 'get stuff' calls in the scripts (like getting the number of the current player and scores etc.) but it should be a breeze to switch out. Each UI scene has a 'Control' script that deals with everything to do with its UI .. eg. TitleScreenControl.cs, ImageScreenControl.cs, MainMenuControl.cs etc.

At the very bottom of the GameController.cs script are two additional classes, one to hold individual player data and the other to hold data about the game in progress. As mentioned earlier, game controller exits right from the title screen through the whole game, so I find it's easier to store per-game data in a nice GameData class you can just destroy or recreate whenever you need it without affecting GameController or needing to reset individual variables.



CHANGING THE GAME CONFIG

The number of players and 'score to win' are both set when the game is started, passed into the StartGame function of GameController by the MainMenuController.cs script. Specifically, the functions StartGame1() and StartGame2() in MainMenuController. For example:
GameController.instance.StartGame (playerCount,5);
^ where playerCount is how many players we want (2 by default) and the 5 is what score has to be reached by a team to win the game.

A little note about players and the game in its current form:
Players are basically objects held in a list. I've done this so that it can be built out to be able to use networked players in the array, too, in the future. Currently, though, adding extra players should be easy (just increase that number in the StartGame call above) but on a single device there's only one 'guesser' at a time yet the describer should be open to everyone in the room .. so it doesn't really allow for more players to be guessers in its current form. The multiplayer/networked version should do a better job with this, though, allowing for more teams and you'd just take the first answer from the first team to answer.


A LOOKOUT! THING ABOUT POLY

I found on the Android device that the PolyToolkitManager.cs script provided by Google will crashout. This is because it tries to initialize PolyApi each time the scene loads and the PolyToolkitManager object is initialized in its Awake function. To fix it, I added a line to see if PolyApi is initialized before calling its Init. The updated Awake function looks like this:

		void Awake ()
		{
			if ( Application.isPlaying )
			{
				// Initialize the Poly Toolkit runtime API, if necessary.
				// (This is a no-op if it was already initialized by the developer).
				if ( PolyApi.IsInitialized == false )  // <-- ADDED BY JEFF(!!) you need to do this to stop it crapping out on the device!
					PolyApi.Init ();
			}

			// Set shader keywords from the settings. This only needs to be done once during runtime
			// (since the settings can't change when the app is running).
			// The settings might change while in edit mode, though, which is why we install the
			// Update() hook below.
			SetKeywordFromSettings ();
		}
		
		
Any questions, feel free to buzz me!
Jeff.
