using UnityEngine;
using System.Collections;
using PLib;
using PLib.Rand;

public class BouncePhysics : MonoBehaviour {

	public	float	rndAnglularVelocity	=	20;
	public	float	dragMultiple		=	16;
	public	float	velocityLoss		=	0.5f;
	public enum BounceType { RANDOM, REFLECT };
	public	BounceType bounceMethod		=	BounceType.RANDOM;

	private	float	_currentVelocity;
	private	Rigidbody	_rigidbody;

	void Start () {
		_rigidbody		=	GetComponent<Rigidbody>();
		initialDrag		=	_rigidbody.drag;
	}

	void Reset () {
		_rigidbody.drag		=	initialDrag;
		_currentVelocity	=	rndAnglularVelocity;
	}

	private	float	initialDrag;

	void OnCollisionEnter (Collision other) {
		_rigidbody.drag	*=	dragMultiple;
		switch (bounceMethod) {
		case BounceType.RANDOM:
			_rigidbody.AddForce (PRand.RandomVector3(_currentVelocity));
			break;
		case BounceType.REFLECT:
			_rigidbody.AddForce (Vector3.Reflect(_rigidbody.velocity.normalized, Vector3.up));
			break;
		}
		_currentVelocity	*=	velocityLoss;
	}
}
