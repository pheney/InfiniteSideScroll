using UnityEngine;
using System.Collections;

public class BasicMoveInputListener : MonoBehaviour, BasicInput.IBasicInputListener {
	
	public	float		amplify	=	1;
	public	bool		enableHorizontalInput	=	true;
	public	bool		enableVerticalInput		=	false;

	private	Transform	_transform;
	private	Vector3		_delta;

	//	LIFECYCLE

	void Awake () {
		_transform	=	GetComponent<Transform>();
	}

	void Start () {
		_delta		=	Vector3.zero;
	}
	
	void Update () {
		_transform.position += _delta;
		_delta = Vector3.zero;
	}

	//	LISTENER

	public const string ADJUST_AMPLIFY_BY_PERCENT = "OnAddPercentageToAmp";
	public void OnAddPercentageToAmp (float percent) {
		//amplify	*= (1 + percent);
	}
	
	//	INTERFACE - IBasicInputListener

	public void OnAxisInput (Vector2 input)
	{
		_delta = Vector2.zero;
		if (enableHorizontalInput) _delta.x += amplify * input.x;
		if (enableVerticalInput) _delta.z += amplify * input.y;
	}
}
