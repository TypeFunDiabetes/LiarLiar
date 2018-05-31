using UnityEngine;
using System.Collections;

public class Waver : MonoBehaviour
{
	public Vector3 moveAxis;
	private float sineAmount;

	public float bobbingSpeed = 0.1f;
	public float bobbingAmount = 2f;

	private float timer = 0.0f;

	private Transform myTransform;
	private Vector3 originalPosition;

	void Start ()
	{
		myTransform = transform;
		originalPosition = myTransform.position;
	}

	void Update ()
	{
		myTransform.position = originalPosition + ( ( sineAmount * moveAxis ) * bobbingAmount );

		sineAmount = Mathf.Abs(FastMath.FastTrig.Sin (timer)); // Mathf.Sin (timer);
		timer = timer + bobbingSpeed * Time.deltaTime;

		if ( timer > Mathf.PI * 2 )
		{
			timer = timer - ( Mathf.PI * 2 );
		}
	}

}
