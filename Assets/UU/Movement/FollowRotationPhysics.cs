﻿using UnityEngine;
using System.Collections;

//	Source: PushyPixels, project "07PhysicsFollower"
//	I'm not going to re-invent the wheel since this already works

//Based on: http://answers.unity3d.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html
//and: http://answers.unity3d.com/questions/195698/stopping-a-rigidbody-at-target.html?sort=oldest
[RequireComponent(typeof(Rigidbody))]
public class FollowRotationPhysics : MonoBehaviour
{
	public float toVel = 2.5f;
	public float maxVel = 15.0f;
	public float maxForce = 40.0f;
	public float gain = 5f;
	public Transform target;
	public bool forceSphericalTensor = false;

	private	Rigidbody _rigidbody;
	
	void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();

		if(forceSphericalTensor)
		{
			_rigidbody.inertiaTensorRotation = Quaternion.identity;
			_rigidbody.inertiaTensor = Vector3.one;
			_rigidbody.centerOfMass = Vector3.zero;
		}
	}
	
	void FixedUpdate()
	{
		UpdateAngularVelocity(target.rotation);
	}
	
	void UpdateAngularVelocity(Quaternion desired)
	{
		//Warning: CopyPasta
		Vector3 z = Vector3.Cross(transform.forward, desired * Vector3.forward);
		Vector3 y = Vector3.Cross(transform.up, desired * Vector3.up);
		
		float thetaZ = Mathf.Asin(z.magnitude);
		float thetaY = Mathf.Asin(y.magnitude);
		
		Vector3 wZ = z.normalized * thetaZ;
		Vector3 wY = y.normalized * thetaY;
		Vector3 wZwY = Vector3.ClampMagnitude(toVel * (wZ + wY), maxVel);
		
		Quaternion q = transform.rotation * _rigidbody.inertiaTensorRotation;
		Vector3 T = q * Vector3.Scale(_rigidbody.inertiaTensor, Quaternion.Inverse(q) * wZwY);
		
		Vector3 error = T - _rigidbody.angularVelocity;
		Vector3 force = Vector3.ClampMagnitude(gain * error, maxForce);
		
		if(force != Vector3.zero)
		{
			_rigidbody.AddTorque(force);
		}
	}
}