using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class ForceThrust : MonoBehaviour {

	public	float		magnitude	=	10;
	public	Vector3		direction	=	Vector3.forward;
	public	ForceMode	mode		=	ForceMode.Force;
	public	bool		continuous	=	true;

	private Transform myTransform;
	private Rigidbody rb;

	void Start () {
		myTransform = transform;
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate () {
		rb.AddForce(myTransform.TransformDirection(direction) * magnitude * Time.fixedDeltaTime, mode);
		if (!continuous) this.enabled = false;
	}
	
	public static string SET_FORCE_MAGNITUDE = "OnSetForceMagnitude";
	public void OnSetForceMagnitude (float m) {
		magnitude = m;
		this.enabled = true;
	}

	public static string SET_FORCE_MODE = "OnSetForceMode";
	public void OnSetForceMode (ForceMode mode) {
		this.mode = mode;
		this.enabled = true;
	}
}
