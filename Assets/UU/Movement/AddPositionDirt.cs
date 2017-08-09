using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class AddPositionDirt : MonoBehaviour {

	public	float	magnitude	=	1;	//	force magnitude
	public	ForceMode	mode	=	ForceMode.Force;
	private	Rigidbody	_rigidbody;

	void Start () {
		_rigidbody = GetComponent<Rigidbody>();
	}

	void FixedUpdate () {
		_rigidbody.AddForce(Random.onUnitSphere * magnitude, mode);
	}
}
