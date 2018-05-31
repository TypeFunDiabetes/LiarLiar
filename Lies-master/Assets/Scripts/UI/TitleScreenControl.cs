using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenControl : MonoBehaviour {

	public void GotoMenu()
	{
		GameController.instance.LoadMainMenu ();
	}
}
