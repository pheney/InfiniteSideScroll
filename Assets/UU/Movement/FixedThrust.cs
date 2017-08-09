using UnityEngine;
using System.Collections;

public class FixedThrust : MonoBehaviour {
	
	public	float	speed			=	2;		//	m/sec
	public	float	speedVariance	=	0.1f;	//	+/- m/sec to the speed
	public	Vector3	directionVector	=	Vector3.forward;
	
	//	cache	//
	
	private	Transform	_transform;
	private	Vector3		_thrust;
	private	float		_speedFactor;
	private	float		_currentSpeed;
	
	//	LIFECYCLE
	void Start () {
		_transform		=	transform;
		_currentSpeed	=	speed + Random.Range (-speedVariance, speedVariance);
		_thrust			=	directionVector * _currentSpeed;
		_speedFactor	=	1;
	}
	
	//	LIFECYCLE
	void FixedUpdate () {
		_transform.Translate (_thrust * _speedFactor * Time.deltaTime);
	}
	
	public static string RESET_SPEED = "OnResetSpeed";
	public void OnResetSpeed (float speedFactor) {
		_speedFactor	=	1;
		OnSetSpeed(speed + Random.Range (-speedVariance, speedVariance));
	}

	public static string SET_SPEED = "OnSetSpeed";
	public void OnSetSpeed (float newSpeed) {
		_currentSpeed	=	newSpeed;
		_thrust			=	directionVector * _currentSpeed;
	}
	
	public static string FACTOR_SPEED = "OnFactorSpeed";
	public void OnFactorSpeed (float speedFactor) {
		_speedFactor	=	speedFactor;
	}
}
