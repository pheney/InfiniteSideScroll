using UnityEngine;
using System.Collections;

public class Drift : MonoBehaviour {

	public	float		defaultDriftSpeed		=	1;
	public	Vector3		defaultDriftDirection	=	Vector3.right;
	private	Transform	_transform;
	private	Vector3		_driftVector;	//	direction AND speed
	private	Vector3		_driftDirection;
	private	float		_driftSpeed;
	private	bool		_isDrifting;

	void Start () {
		_transform = GetComponent<Transform>();
		_isDrifting = true;
		_driftSpeed = defaultDriftSpeed;
		_driftDirection = defaultDriftDirection;
		UpdateDriftVector();
	}

	void Update () {
		if (!_isDrifting) return;
		_transform.Translate(_driftVector * Time.deltaTime);
	}

	private void UpdateDriftVector () {
		_driftVector = _driftDirection * _driftSpeed;
	}

	public const string SET_DRIFT_ACTOVE = "OnSetDriftActive";
	public void OnSetDriftActive (bool active) {
		_isDrifting = active;
	}
	
	public const string SET_DRIFT_DIRECTION = "OnSetDriftDirection";
	public void OnSetDriftDirection (Vector3 direction) {
		_driftDirection = direction;
		UpdateDriftVector();
	}
	
	public const string SET_DRIFT_SPEED = "OnSetDriftSpeed";
	public void OnSetDriftSpeed (float speed) {
		_driftSpeed = speed;
		UpdateDriftVector();
	}

	public const string SET_DRIFT_VECTOR = "OnSetDrift";
	public void OnSetDrift (Vector3 driftVector) {
		_driftVector = driftVector;
		_driftDirection = _driftVector.normalized;
		_driftSpeed = _driftVector.magnitude;
	}
}
