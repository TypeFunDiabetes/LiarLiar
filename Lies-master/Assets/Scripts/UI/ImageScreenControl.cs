using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageScreenControl : MonoBehaviour
{
	public Text _truthOrLieText;

	void Start ()
	{
		bool truth = GameController.instance.GetTruthStatus ();
		if ( truth )
		{
			_truthOrLieText.text = "Tell the truth!";
		}
		else
		{
			_truthOrLieText.text = "Tell a lie!";
		}
	}

	public void FinishedImage ()
	{
		GameController.instance.FinishedViewing ();
	}
}
