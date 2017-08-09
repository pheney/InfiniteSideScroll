/// <summary>
/// Calculate and apply gravity force for all rigidbodies on the selected layers, against
/// all other rigidbodies.
/// TODO -- use a LayerMask instead of a string
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PLib;
using PLib.Math;
using PLib.General;

public class GravityPull : MonoBehaviour {

	public	string		gravityLayer;
	public	float		gravityMultiplier	=	(float)PMath.UNIVERSAL_GRAVITATIONAL_CONSTANT;
	private	List<Rigidbody>	bodies;
	
	void Start () {
		bodies = new List<Rigidbody>();
		foreach (GameObject g in PUtil.FindOnLayer(gravityLayer)) {
			Rigidbody rb = g.GetComponent<Rigidbody>();
			if (rb) { 
				this.AddGravityBody(rb);
			}
		}
	}

	void FixedUpdate () {
		bodies.RemoveNulls();
		foreach (Rigidbody bodyA in bodies) {
			if (bodyA == null) continue;
			foreach (Rigidbody bodyB in bodies) {
				if (bodyA.Equals(bodyB) || bodyB == null) continue;
				Vector3	direction = (bodyB.position - bodyA.position).normalized;
				float	magnitude = bodyA.GravitationalAccelerationToward(bodyB) * gravityMultiplier;
				bodyA.AddForce(direction * magnitude * Time.fixedDeltaTime);
				bodyB.AddForce(-direction * magnitude * Time.fixedDeltaTime);
			}
		}
	}

	//	Listener

	public static string ADD_GRAVITY_BODY = "AddGravityBody";
	public void AddGravityBody (Rigidbody body) {
		bodies.AddUnique(body);
	}
}
