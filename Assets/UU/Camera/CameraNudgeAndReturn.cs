using UnityEngine;
using System.Collections;

public class CameraNudgeAndReturn : MonoBehaviour, BasicInput.IBasicInputListener {

	[Tooltip("Camera look-ahead (or lag) based on input")]
	[Range(-10,10)]
	public	float	nudgeDistance = 5;

	[Tooltip("Camera move speed, m/s")]
	[Range(1, 10)]
	public	float	nudgeSpeed = 3;

	private Vector3 basePosition, goalPosition;

	public void Start () {
		basePosition = transform.position;
		goalPosition = basePosition;
	}

	public void LateUpdate() {
		transform.position = Vector3.Slerp(transform.position, goalPosition, nudgeSpeed * Time.deltaTime);
	}

	public void OnAxisInput (Vector2 input) {		
		goalPosition = basePosition + Vector3.right * input.x * nudgeDistance;
		goalPosition.y = transform.position.y;
	}
}
