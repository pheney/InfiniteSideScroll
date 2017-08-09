using UnityEngine;
using System.Collections;

/// <summary>
/// 2016-5-18
/// Accepts input via OnAxisInput message.
/// Converts input to linear movement along X, Y or XY.
/// </summary>
public class OnMessageMove : MonoBehaviour, BasicInput.IBasicInputListener {

	[Tooltip("m/s")]
	public	float	amp		=	1;
	public	Vector3	effectAxis	=	Vector3.up;

	[Tooltip("Select axis to use from the input vector")]
	public Axis inputAxis;
	public enum Axis { X_AXIS, Y_AXIS, XY_AXIS }

	public void Update () {
		transform.position += inVector * amp;
		inVector = Vector3.zero;
	}

	//	INTERFACE

	private	Vector3 inVector = Vector3.zero;

	public void OnAxisInput (Vector2 input)
	{
		switch (inputAxis) {
			case Axis.X_AXIS:
				inVector = effectAxis * input.x;	
				break;			
			case Axis.Y_AXIS:
				inVector = effectAxis * input.y;
				break;
			case Axis.XY_AXIS:
				inVector = Vector3.right * effectAxis.x * input.x
						+ Vector3.forward * effectAxis.z *input.y;
				break;
		}
	}
}