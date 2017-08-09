/// <summary>
/// Basic hover, including simple 'bob' effect. 
/// </summary>
using UnityEngine;
using System.Collections;
using PLib;
using PLib.Math;

public class BasicHover : MonoBehaviour {

	public	float	bobMagnitude	=	1;
	public	float	bobFrequency	=	1;
	private Vector3	initialPosition;
	private	float	dirt;

	//	LIFECYCLE
	void Start () {
		initialPosition	=	transform.localPosition;
		dirt			=	Random.value * PMath.TAU;
	}

	//	LIFECYCLE
	void LateUpdate () {
		transform.localPosition	=	initialPosition + Vector3.up * bobMagnitude * Mathf.Sin(dirt + bobFrequency * Time.time) * Time.fixedDeltaTime;
	}
}
