using UnityEngine;
using System.Collections;

public class MortarFlightControl : MonoBehaviour {

	public	float	turnRate = 30;

	void Update () {
		Quaternion goalRotation = Quaternion.LookRotation(Vector3.down);
		transform.rotation = Quaternion.Lerp(transform.rotation, goalRotation, turnRate * Time.deltaTime);
	}
}
