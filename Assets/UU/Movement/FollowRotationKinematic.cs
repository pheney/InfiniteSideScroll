using UnityEngine;
using System.Collections;
using PLib;
using PLib.Rand;

public class FollowRotationKinematic : MonoBehaviour {
	
	public	Transform	target;
	public	float		maxTorque = 90;	//	deg/s
	public	float		dirtMagnitude	=	10;	//	deg/s
	
	private	Transform	_transform;
	
	void Start () {
		_transform = transform;
	}
	
	void Update () {
		_transform.rotation = Quaternion.RotateTowards(_transform.rotation, target.rotation, maxTorque * Time.deltaTime);
		float dirt = dirtMagnitude * Time.deltaTime;
		_transform.Rotate (PRand.RandomPosToNeg(dirt), PRand.RandomPosToNeg(dirt), PRand.RandomPosToNeg(dirt));
	}
}
