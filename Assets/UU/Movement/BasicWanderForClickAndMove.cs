using UnityEngine;
using System.Collections;

public class BasicWanderForClickAndMove : MonoBehaviour {

	public	float		maxIdleDuration	=	3;	//	sec

	private	Transform	_transform;
	private	float		_lastMoveTime;
	private	Quaternion	_lastRotation;
	private	Vector3		_lastPosition;

	void Start () {
		_transform = transform;
	}

	void Update () {

		if (!_lastPosition.Equals(_transform.position)) return;
		if (!_lastRotation.Equals(_transform.rotation)) return;
		if (Time.time < _lastMoveTime + maxIdleDuration) return;

		//	we don't get here unless we haven't moved in {maxIdleDuration} seconds

		//	todo -- start short-distance wander coroutine

		_lastPosition = _transform.position;
		_lastRotation = _transform.rotation;
	}
}
