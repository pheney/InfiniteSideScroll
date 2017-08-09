using UnityEngine;
using System.Collections;

public class DistanceBasedThrust : MonoBehaviour {

	public Transform target;
	public	Vector3	velocityVector	=	Vector3.forward;
	public	float	farSpeed		=	5;		//	m/sec, speed at far point
	public	float	nearSpeed		=	20;		//	m/sec, speed at near point
	public	float	minDistance		=	5;		//	m, distance at which we hit nearSpeed
	public	float	maxDistance		=	100;	//	m, distance at which we hit farSpeed

	private Transform myTransform;

	//	for debuging
	[SerializeField]
	private	float	speed;
	[SerializeField]
	private	float	dist;

	void Start () {
		myTransform = transform;
		if (minDistance == maxDistance) maxDistance += 1;
	}

	void Update () {
		Vector3 direction = transform.TransformDirection(velocityVector);
				dist	=	Vector3.Distance(target.position, myTransform.position);

		float	distRatio_numerator = dist - minDistance;
		float	distRatio_denominator = maxDistance - minDistance;
		float	distRatio = distRatio_numerator / distRatio_denominator;
		float	speedRange = farSpeed - nearSpeed;
				speed = distRatio * speedRange + nearSpeed;

		//	override the speed calculation for extreme near/far distances
		if (dist < minDistance) speed = nearSpeed;
		if (dist > maxDistance) speed = farSpeed;

		myTransform.position += direction * speed * Time.deltaTime;
	}
}
