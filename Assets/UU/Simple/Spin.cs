using UnityEngine;
using System.Collections;
using PLib;
using PLib.Rand;

public class Spin : MonoBehaviour {
	
	public	Transform	spinTransform;
	public	bool		randomizeAxis		=	false;
	public	Vector3		spinAxis			=	Vector3.up;
	public	float		revolutionsPerSecond	=	0;
	public	float		spinVariance		=	0;	//	deg/sec +/- to the spinRate
	public	bool		oscillate			=	false;
	
	private	float		_spinRate;

	//	LIFECYCLE
	void Start () {
		if (!spinTransform) spinTransform = transform;
		if (randomizeAxis) {
			spinAxis = new Vector3 (Random.value, Random.value, Random.value);
			spinAxis.Normalize();
		}

		spinTransform.Rotate (spinAxis, Random.Range (0, 180));
		_spinRate	=	PRand.RandomSign() * (360 * revolutionsPerSecond + Random.Range (-spinVariance, spinVariance));
		if (oscillate) {
			Debug.Log ("WARNING: 'oscillate' option of Spinner class is not implemented");
		}
	}
	
	//	LIFECYCLE
	void Update () {
		if (oscillate) {
			spinTransform.Rotate (spinAxis, _spinRate * Time.deltaTime);
		} else {
			spinTransform.Rotate (spinAxis, _spinRate * Time.deltaTime);
		}
	}

	//	Helper
	public void Sim (float timeStep) {
		if (oscillate) {
			spinTransform.Rotate (spinAxis, _spinRate * timeStep);
		} else {
			spinTransform.Rotate (spinAxis, _spinRate * timeStep);
		}
	}
}