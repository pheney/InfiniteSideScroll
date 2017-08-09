using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicInputControl : MonoBehaviour, BasicGameManager.IGameStartListener, 
												BasicGameManager.IGameOverListener {
	
	public	float					touchSensitivity	=	1;
	public	float					inputMultiple		=	1;
	public	List<GameObject>		inputListeners;

	protected	static	float		horizontalAmp	=	1.5f;
	protected	static	float		verticalAmp		=	4;
	protected	static	bool		_acceptInput;
	private		List<GameObject>	_newInputListeners;

	void Awake () {
		if (inputListeners == null) inputListeners = new List<GameObject>();
		_newInputListeners = new List<GameObject>();
	}

	protected virtual void Update () {
		if (!_acceptInput) return;
		Vector2 input = GetInput ();
		if (input.Equals(Vector2.zero)) return;
		SendInput(input);
	}

	protected Vector2 GetInput() {
		if (!_acceptInput) return Vector2.zero;
		float	dx = Input.GetAxis("Horizontal") * horizontalAmp * inputMultiple;
		float	dy = Mathf.Abs(Input.GetAxis("Vertical")) * verticalAmp * inputMultiple;
		return Vector2.right * dx + Vector2.up * dy + GetTouchInput();
	}
	
	private Vector2 GetTouchInput () {
		if (Input.touchCount == 0) return Vector2.zero;
		return Input.GetTouch(0).deltaPosition.x * touchSensitivity * Vector2.right + Vector2.up;
	}

	protected void SendInput (Vector2 input) {

		inputListeners.RemoveAll(x => x == null);
		inputListeners.AddRange(_newInputListeners);
		_newInputListeners.Clear();

		foreach (GameObject g in inputListeners) {
			if (g) g.SendMessage(BasicInput.SEND_BASIC_AXIS_INPUT, input, SendMessageOptions.DontRequireReceiver);
		}
	}

	//	listener

	public static string ADD_INPUT_LISTENER = "AddInputListener";
	public void AddInputListener (GameObject newListener) {
		_newInputListeners.Add(newListener);
	}

	//	INTERFACE - IGameStartListener

	public virtual void OnGameStart ()
	{
		_acceptInput = true;
	}

	//	INTERFACE - IGameOverListener

	public virtual void OnGameOver ()
	{
		_acceptInput = false;
	}
}

public class BasicInput {

	public static string SEND_BASIC_AXIS_INPUT = "OnAxisInput";
	public interface IBasicInputListener {
		void OnAxisInput (Vector2 input);
	}
}
