using UnityEngine;
using System.Collections;

public class BasicHoverForce : MonoBehaviour {
	
	public	float	turbulance		=	0.05f;
	public	float	force;
	private	Rigidbody	_rigidbody;

	void Awake () {
		_rigidbody = GetComponent<Rigidbody>();
	}

	//	LIFECYCLE
	void FixedUpdate () {
		force =  _rigidbody.mass * 9.81f + turbulance *  Random.Range(-1,1f);
		_rigidbody.AddForce(Vector3.up * force);
	}
}
