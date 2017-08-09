using UnityEngine;
using System.Collections;

public class FollowPositionKinematic : MonoBehaviour {

	public	Transform	target;
	public	float		maxVelocity = 10;	//	m/s
	public	float		dirtMagnitude	=	1;	//	m/s

	private	Transform	_transform;

	void Start () {
		_transform = transform;
	}

	void Update () {
		Vector3	moveVector = target.position - _transform.position;
		float	sqrVelocity = Vector3.SqrMagnitude(moveVector);
		if (sqrVelocity > maxVelocity) {
			moveVector = moveVector.normalized * maxVelocity;
		} 

		_transform.position += moveVector * Time.deltaTime;
		_transform.position += Random.onUnitSphere * dirtMagnitude * Time.deltaTime;
	}
}
