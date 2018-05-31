using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnResultsControl : MonoBehaviour {

	public GameObject _correctUI;
	public GameObject _incorrectUI;

	public void Start ()
	{
		if(GameController.instance.DidAnswerRight())
		{
			_correctUI.SetActive (true);
			_incorrectUI.SetActive (false);
		} else
		{
			_correctUI.SetActive (false);
			_incorrectUI.SetActive (true);
		}
	}

	public void EndTurnResults()
	{
		GameController.instance.TurnResultsEnded ();
	}
}
