using UnityEngine;
using System.Collections;

// Require a character controller to be attached to the same game object
//@script RequireComponent (CharacterMotor)
//@script AddComponentMenu ("Character/FPS Input Controller")
		
[RequireComponent (typeof (FPSCharacterMotor))]
public class FPSInputControllerCS : MonoBehaviour {

	public	bool	acceptPlayerInput	=	true;
	private FPSCharacterMotor motor;
	
	private NetworkView mNetworkView;
	private Transform mTransform;
	
	void Awake () {
		motor = GetComponent<FPSCharacterMotor>();;
	}
	
	void Start () {
		
		mNetworkView = GetComponent<NetworkView>();
		mTransform = transform;
	}
	
	void Update () {
		UpdateMovement(Vector3.zero);
		UpdateJump(false);
		if (mNetworkView.isMine && acceptPlayerInput) {
			UpdateMovement(new Vector3 (Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")));
			UpdateJump(Input.GetButton("Jump"));
		}
	}
	
	public	static string	UpdateMovmentMessage	=	"UpdateMovement";
	
	public void UpdateMovement (Vector3 inputVector) {
		
		// Get the input vector from keyboard or analog stick
		var directionVector = inputVector;
		
		if (directionVector != Vector3.zero) {
			// Get the length of the directon vector and then normalize it
			// Dividing by the length is cheaper than normalizing when we already have the length anyway
			var directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
			
			// Make sure the length is no bigger than 1
			directionLength = Mathf.Min(1, directionLength);
			
			// Make the input vector more sensitive towards the extremes and less sensitive in the middle
			// This makes it easier to control slow speeds when using analog sticks
			directionLength = directionLength * directionLength;
			
			// Multiply the normalized direction vector by the modified length
			directionVector = directionVector * directionLength;
		}
		
		// Apply the direction to the CharacterMotor
		motor.inputMoveDirection = mTransform.rotation * directionVector;
	}
	
	public static string UpdateJumpMessage = "UpdateJump";
	
	public void UpdateJump (bool jumpInput) {
		motor.inputJump = jumpInput;
	}
}
