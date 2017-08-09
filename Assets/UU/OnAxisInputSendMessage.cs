using UnityEngine;
using System.Collections;

public class OnAxisInputSendMessage : MonoBehaviour {

	public	Vector2	inputAplify	=	Vector2.one;
	public	enum Axis { Horizontal, Vertical, Both, None };
	public	Axis	inputAxis;

	[Tooltip("Behaviors on this GameObject that will recieve messages")]
	public	MonoBehaviour[]	behaviors;
	
	[Tooltip("Other GameObjects to receive messages")]
	public	GameObject[]	gameObjects;

	private bool blockInput;
	private Vector2 blockDirection;

	public void OnBlockInputBegin(Vector3 blockDirection) {
		blockInput = true;
		this.blockDirection = blockDirection;
	}

	public void OnBlockInputEnd(Vector3 blockDirection) {
		blockInput = false;
	}

	void Update () {
		if (inputAxis.Equals(Axis.None)) return;

		Vector2	inputVector = Vector3.zero;

		//	capture the input and build the input vector

		if (inputAxis.Equals(Axis.Horizontal) || inputAxis.Equals(Axis.Both)) {
			inputVector += Vector2.right * Input.GetAxis("Horizontal");
		}
		
		if (inputAxis.Equals(Axis.Vertical) || inputAxis.Equals(Axis.Both)) {
			inputVector += Vector2.up * Input.GetAxis("Vertical");
		}

		inputVector.Scale(inputAplify);

		//	apply input blocking

		if (blockInput) {
			if (blockDirection.x > 0 && inputVector.x > 0) inputVector.x = 0;
			if (blockDirection.x < 0 && inputVector.x < 0) inputVector.x = 0;
			if (blockDirection.y > 0 && inputVector.y > 0) inputVector.y = 0;
			if (blockDirection.y < 0 && inputVector.y < 0) inputVector.y = 0;
		}

		//	send input message to recievers
		SendInput(inputVector);
	}

	private void SendInput (Vector2 inputVector) {
        if (inputVector.Equals(Vector2.zero)) return;
		foreach (MonoBehaviour b in behaviors) b.SendMessage(AxisInputReceivable.AXIS_INPUT, inputVector);
		foreach (GameObject g in gameObjects) {
			//	2016-5-9
			//	Changed SendMessage to BroadcastMessage to work with InfiniteSideScroller project
			g.BroadcastMessage(AxisInputReceivable.AXIS_INPUT, inputVector, SendMessageOptions.DontRequireReceiver);
		}
	}
}

//	INTERFACE

public class AxisInputReceivable {
	public const string AXIS_INPUT = "OnAxisInput";
	public interface IAxisInputReceivable {
		void OnAxisInput (Vector2 inputVector);
		void OnInputEnabled (bool enableInput);
	}
}