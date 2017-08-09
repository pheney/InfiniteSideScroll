using UnityEngine;
using System.Collections;
using PLib;
using PLib.Math;

public class CollisionDetection : MonoBehaviour {

	public	float		detectionRadius	=	1;	//	meters
	public	LayerMask	layerMask		=	-1;	//	everything
	public	float		offset			=	0.5f;	//	meters

	private	Transform	_transform;
	private	Vector3		_lastPosition;

	void Start () {
		_transform		=	transform;
		_lastPosition	=	_transform.position;
	}

	void Update () {
		if (_transform.position != _lastPosition) {

			Vector3 moveDirection = _transform.position - _lastPosition;

			RaycastHit info;
			if (Physics.Raycast(_lastPosition + Vector3.up * offset, moveDirection, out info, detectionRadius, layerMask)) {

				Vector3 tangent = PMath.CollisionSurfaceTangent(moveDirection, info.normal);

				if (!Physics.Raycast(_lastPosition, tangent, detectionRadius, layerMask)) {
					_transform.position = _lastPosition + tangent * Vector3.Project(moveDirection, tangent).magnitude;
				} else {
					_transform.position = _lastPosition;
				}
			}
		}
		_lastPosition = _transform.position;
	}
}
