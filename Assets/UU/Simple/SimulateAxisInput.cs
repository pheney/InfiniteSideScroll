using UnityEngine;
using System.Collections;

public class SimulateAxisInput : MonoBehaviour {

	[Tooltip("Simulate 'horizontal axis' input from keys or mouse")]
	[Range(-1,1)]
	public	float horizontalInput;

	[Tooltip("Simulate 'vertical axis' input from keys or mouse")]
	[Range(-1,1)]
	public	float verticalInput;

	public	enum InputType { Continuous, Periodic }
	public	InputType	inputType	=	InputType.Continuous;

	[Tooltip("(sec) Duration the input pauses.")]
	public	float		inputDelay = 3;

    [Tooltip("(sec) Duration of input. Only necessary for 'Periodic' input")]
    public  float       inputDuration=2;

	public	GameObject[]	receivingObjects;
	public	Behaviour[]		receivingBehaviours;

	private	float	beginTime;
	private bool	pause;
    
    void OnEnable()
    {
        beginTime = 0;
        pause = false;
        if (inputType.Equals(InputType.Periodic)) {
            InvokeRepeating(DELAY_INPUT, 0, inputDelay + inputDuration);
		}
	}

    void OnDisable()
    {
        if (IsInvoking(DELAY_INPUT)) CancelInvoke(DELAY_INPUT);
    }

	//	UNITY API
	void Update () {
		SendInput();
	}

	//	internal

	private void SendInput () {
		if (pause) return;
		if (Time.time < beginTime) return;

		//	update simulated input vector
		Vector2 inputVector = Vector2.right * horizontalInput + Vector2.up * verticalInput;

		//	abort if there is no input
		if (inputVector.Equals(Vector2.zero)) return;

        foreach (var g in receivingObjects) {
			g.SendMessage(BasicInput.SEND_BASIC_AXIS_INPUT, inputVector, SendMessageOptions.DontRequireReceiver);
		}

		foreach (var b in receivingBehaviours) {
			b.SendMessage(BasicInput.SEND_BASIC_AXIS_INPUT, inputVector, SendMessageOptions.DontRequireReceiver);
		}
    }

    //	MESSAGE LISTENER

    public static string PAUSE_INPUT = "OnPauseInputSim";

	public void OnPauseInputSim() {
		pause = !pause;
	}

	public static string DELAY_INPUT = "OnDelayInputSim";

	public void OnDelayInputSim() {
		OnDelayInputSim(inputDelay);
	}

	public void OnDelayInputSim(float pauseDuration) {
		beginTime = Time.time + pauseDuration;
	}
}
