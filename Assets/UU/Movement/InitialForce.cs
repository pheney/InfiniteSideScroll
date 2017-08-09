using UnityEngine;
using System.Collections;
using PLib;
using PLib.Rand;

public class InitialForce : MonoBehaviour {

	public	Vector3	direction		=	Vector3.forward;
	public	bool	randomDirection	=	true;
	public	float	speed			=	5;
	public	bool	randomSpeed		=	true;

	void Start () {
		Vector3 dir = randomDirection? PRand.RandomVector3() : transform.TransformDirection(direction);
		float s = randomSpeed? Random.value * speed : speed;
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.AddForce (dir * s, ForceMode.VelocityChange);
		Destroy(this);	
	}

}
