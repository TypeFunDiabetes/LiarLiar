using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassPhoneControl : MonoBehaviour
{
	public Text _GetReadyMessage;
	
	public void Start ()
	{
		int aPlayer = GameController.instance.GetCurrentPlayerNum () ;
		_GetReadyMessage.text = "Pass the\nphone to\nTeam " + aPlayer.ToString ();
	}

	public void Passed ()
	{
		GameController.instance.FinishedPassing ();
	}
}

