using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class FollowPositionPhysics : MonoBehaviour {

	public GameObject target;
	public float toVelocity = 2.5f;	//	what's this?
	public float maxVelocity = 15;
	public float maxForce = 40;
	public float gain = 5;
	
	private	Rigidbody _rigidbody;
	
	void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
	}

	void FixedUpdate () {

		Vector3 distance = target.transform.position - transform.position;
		Vector3 goalVelocity = Vector3.ClampMagnitude(toVelocity * distance, maxVelocity);
		Vector3 errorVelocity = goalVelocity - _rigidbody.velocity;
		Vector3 force = Vector3.ClampMagnitude(Random.Range(0.9f, 1.1f) * gain * errorVelocity, maxForce);
		_rigidbody.AddForce(force);
	}
}
