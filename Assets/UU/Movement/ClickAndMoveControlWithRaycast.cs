using UnityEngine;
using System.Collections;
using PLib;
using PLib.Rand;
using PLib.Math;

public class ClickAndMoveControlWithRaycast : MonoBehaviour {

	public	float		turnSpeed		=	120;	//	deg/sec
	public	float		moveSpeed		=	5;		//	m/sec
	public	float		maxAngleForMove	=	60;		//	deg
	public	LayerMask	moveLayer		=	1;
	public	float		stopDistance	=	0.2f;	//	meters
	public	float		collisionDistance	=	0.5f;	//	meters
	public	LayerMask	collisionLayer	=	-1;
	public	Transform	collisionSensor;

	private	Vector3		targetPosition;
	private	float		cosAngle;

	void Start () {
		targetPosition = transform.position + PRand.RandomHorizontalVector(0.01f);
		cosAngle = Mathf.Cos(Mathf.Deg2Rad * maxAngleForMove);
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit info;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), 
			                    out info, Mathf.Infinity, moveLayer)) {
				targetPosition = info.point;
			}
		}

		Vector3		moveDirection = targetPosition - transform.position;
		float		moveDistance = moveDirection.magnitude;
		RaycastHit	moveInfo;
		if (Physics.Raycast(collisionSensor.position, moveDirection, out moveInfo, moveDistance, collisionLayer)) {
			moveDirection = PMath.CollisionSurfaceTangent(moveDirection, moveInfo.normal).normalized;
			Vector3 point = moveInfo.point;
			point.y = 0;
			targetPosition = point + moveDirection * moveSpeed * Time.deltaTime;
		}
		Quaternion	goalRotation = Quaternion.LookRotation(moveDirection);
		transform.rotation = Quaternion.RotateTowards (transform.rotation, goalRotation, turnSpeed * Time.deltaTime);

		if (Vector3.Dot(transform.forward, moveDirection.normalized) < cosAngle) return;
		if (Vector3.SqrMagnitude(moveDirection) < stopDistance * stopDistance) return;
		transform.Translate(Vector3.forward * Mathf.Min(moveSpeed * Time.deltaTime, stopDistance * stopDistance * moveDirection.magnitude));
	}
}
