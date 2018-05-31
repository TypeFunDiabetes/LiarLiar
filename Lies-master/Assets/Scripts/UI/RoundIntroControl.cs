using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundIntroControl : MonoBehaviour
{
	public Text _roundText;
	public GameObject _roundButton;

	void Start ()
	{
		_roundText.text = "Get ready for\nRound " + GameController.instance.GetCurrentRoundNum ();
	}

	public void StartRound()
	{
		_roundButton.SetActive (false);
		GameController.instance.StartRound ();

	}
}
