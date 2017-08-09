using UnityEngine;
using UnityEngine.EventSystems;
using PLib;
using PLib.Rand;

public class ClickAndMoveControl : MonoBehaviour, Directable.IDirectable {

	public	float		turnSpeed		=	120;	//	deg/sec
	public	float		moveSpeed		=	5;		//	m/sec
	public	float		maxAngleForMove	=	60;		//	deg
	public	LayerMask	moveLayer		=	1;
	public	float		stopDistance	=	0.2f;	//	meters

	private	Vector3		targetPosition;
	private	float		cosAngle;

	void Start () {
		targetPosition = transform.position + PRand.RandomHorizontalVector(0.01f);
		cosAngle = Mathf.Cos(Mathf.Deg2Rad * maxAngleForMove);
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			if (!EventSystem.current.IsPointerOverGameObject()) {
				RaycastHit info;
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), 
				                    out info, Mathf.Infinity, moveLayer)) {
					targetPosition = info.point;
					SendMessage(TaskRequestReceiver.ABORT_TASK, SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		Vector3		moveDirection = targetPosition - transform.position;
		Quaternion	goalRotation = Quaternion.LookRotation(moveDirection);
		transform.rotation = Quaternion.RotateTowards (transform.rotation, goalRotation, turnSpeed * Time.deltaTime);

		if (Vector3.Dot(transform.forward, moveDirection.normalized) < cosAngle) return;
		if (Vector3.SqrMagnitude(moveDirection) < stopDistance * stopDistance) return;
		transform.Translate(Vector3.forward * Mathf.Min(moveSpeed * Time.deltaTime, stopDistance * stopDistance * moveDirection.magnitude));
	}

	//	INTERFACE

	public void OnMoveToPosition (Vector3 goalPosition)
	{
		targetPosition = goalPosition;
	}

	public void OnOrientToDirection (Quaternion goalDirection) {}
	public void OnAlignWithTransform (Transform goalTransform) {}
}
