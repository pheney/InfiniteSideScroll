using UnityEngine;
using System.Collections;
using PLib;
using PLib.Math;

public class InitMassByDensity : MonoBehaviour {

	public	float	density		=	1;
	private	Rigidbody	rb;

	void Start () {
		rb = GetComponent<Rigidbody>();
		if (!rb) return;
		rb.mass = PMath.GetMassOfSphere(transform.localScale.magnitude, density);
	}
}
