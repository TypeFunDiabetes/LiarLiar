using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnIntroControl : MonoBehaviour
{
	public Text _GetReadyMessage;
	public GameObject _roundButton;

	public void Start ()
	{
		int aPlayer = GameController.instance.GetCurrentDescriberID () + 1;
		_GetReadyMessage.text = "Team " + aPlayer.ToString()+"\nready?";
	}

	public void StartTurn()
	{
		_roundButton.SetActive (false);
		GameController.instance.StartTurn ();
	}
}
