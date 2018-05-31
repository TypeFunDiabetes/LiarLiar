using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuessControl : MonoBehaviour
{
	public Text teamLabel;

	void Start ()
	{
		teamLabel.text = "Team " + GameController.instance.GetCurrentPlayerNum ().ToString();
	}

	public void PlayerAnsweredTrue()
	{
		GameController.instance.PlayerAnsweredTrue ();
	}

	public void PlayerAnsweredFalse ()
	{
		GameController.instance.PlayerAnsweredFalse ();
	}
}
