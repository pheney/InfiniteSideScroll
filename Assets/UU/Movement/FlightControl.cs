/// <summary>
/// Input control for flying vehicles. Handles Roll and Pitch but not Yaw. Has message
/// listeners for use on AI vehicles as well.
/// </summary>
using UnityEngine;
using System.Collections;

public class FlightControl : MonoBehaviour {
	
	public		float		acceleration	=	.1f;		//	m/s/s
	public		float		maxVelocity		=	.2f;		//	m/s
	public		float		pitchSpeed		=	90;			//	deg/sec
	public		float		rollSpeed		=	180;		//	deg/sec
	public		KeyCode		throttleUpKey	=	KeyCode.Q;
	public		KeyCode		throttleDownKey	=	KeyCode.Z;
	public		float		_curVelocity;
	
	private		Transform	_thisTransform;
	private		float		_pitchDelta;
	private		float		_rollDelta;
	
	// Use this for initialization
	void Start () {
		_thisTransform		=	transform;
		_curVelocity		=	0;
	}
	
	//	LIFECYCLE
	void Update () {
		if (Mathf.Abs(_pitchDelta) > .025f) {
			// pitch craft up or down (in relative direction)
			_thisTransform.Rotate(Vector3.right * _pitchDelta * pitchSpeed * Time.deltaTime);
		}
		if (Mathf.Abs(_rollDelta) > .025f) {
			// roll craft left or right (in relative direction)
			_thisTransform.Rotate(Vector3.forward * -_rollDelta * rollSpeed * Time.deltaTime);
		}
		if (Input.GetKey(throttleUpKey)) {
			// adjust throttle up
			_curVelocity	+=	acceleration * Time.deltaTime;
		}
		if (Input.GetKey(throttleDownKey)) {
			// adjust throttle own
			_curVelocity	+=	-acceleration * Time.deltaTime;
		}
		_curVelocity = Mathf.Clamp (_curVelocity, -maxVelocity * .5f, maxVelocity);
		
		// move craft
		if (Mathf.Abs(_curVelocity) > .01f) _thisTransform.Translate (Vector3.forward * _curVelocity);
		
		// reset input values
		_rollDelta			=	Input.GetAxis("Horizontal");
		_pitchDelta			=	Input.GetAxis("Vertical");
	}

	//	Listener for AI input
	public void InputHAxis (float h) {
		_rollDelta		=	h;
	}
	
	//	Listener for AI input
	public void InputVAxis (float v) {
		_pitchDelta		=	v;
	}
}
