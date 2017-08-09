using UnityEngine;
using System.Collections;

public class CameraMatchPosition : MonoBehaviour {

	[Tooltip("Axis to match. Inspector Assigned")]
	public Vector3 offset;

	[Tooltip("m/s, Inspector Assigned")]
	public float speed;

	public string controllerTag = "GameController";

	[SerializeField]
	private GameObject target;
	private GameObject manager;

	void Start () {
		manager = GameObject.FindGameObjectWithTag(controllerTag);
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			target = manager.GetComponent<IssPlayerSpawner>().GetPlayer();
		}	
	}

	void LateUpdate () {
		if (target == null) return;

		AdjustPosition();
	}

	private void ForcePosition () {
		Vector3 targetPosition = target.transform.position;
		Vector3 position = transform.position;
		position.y = targetPosition.y;
		transform.position = position;
	}

	private void AdjustPosition () {

		Vector3 targetPosition = target.transform.position;
		Vector3 goalPosition = transform.position;

		if (offset.x != 0) goalPosition.x = targetPosition.x + offset.x;
		if (offset.y != 0) goalPosition.y = targetPosition.y + offset.y;
		if (offset.z != 0) goalPosition.z = targetPosition.z + offset.z;

		transform.position = Vector3.Lerp(transform.position, goalPosition, speed * Time.deltaTime);
	}
}
