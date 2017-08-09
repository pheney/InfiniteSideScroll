using UnityEngine;
using System.Collections;

//	Based on the original MouseLook.js file shipped with Unity.
//	Modified to accept input in the form of messages so that AI could
//	use this script as well.
//	Modified to accept messages for zoom to adjust mouse sensitivity when zoomed in.


/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLookWithListener : MonoBehaviour {

	public	bool	acceptPlayerInput	=	true;
	public	KeyCode	inputActiveKey		=	KeyCode.LeftShift;
	public	enum	RotationAxes { 
		MouseXAndY = 0, 
		MouseX = 1, 
		MouseY = 2 
	}
	public	RotationAxes	axes		=	RotationAxes.MouseXAndY;
	public	float	sensitivityX		=	15F;
	private	float	_initSensitivityX;
	public	float	sensitivityY		=	15F;
	private	float	_initSensitivityY;

	public	float	minimumX			=	-360F;
	public	float	maximumX			=	360F;

	public	float	minimumY			=	-60F;
	public	float	maximumY			=	60F;

			float	rotationY			=	0F;
	private	bool	_isMyNetworkView;
	private Rigidbody _rigidbody;
	
	void Start ()
	{
		NetworkView	netView	=	transform.root.GetComponent<NetworkView>();
		_isMyNetworkView	=	netView == null || netView.isMine;

		_initSensitivityX	=	sensitivityX;
		_initSensitivityY	=	sensitivityY;

		// Make the rigid body not change rotation
		_rigidbody = GetComponent<Rigidbody>();
		if (_rigidbody)
			_rigidbody.freezeRotation = true;
	}
	
	void Update () {
		if (acceptPlayerInput 
		    && (inputActiveKey == KeyCode.None || Input.GetKey(inputActiveKey))
		    && _isMyNetworkView) OnMouseLookUpdate(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
	}
	
	public static string UPDATE_LOOK	=	"OnMouseLookUpdate";
	
	//	listener
	public void OnMouseLookUpdate (Vector2 mouseInputVector)
	{
		if (axes == RotationAxes.MouseXAndY)
		{
			float rotationX = transform.localEulerAngles.y + mouseInputVector.x * sensitivityX;
			
			rotationY += mouseInputVector.y * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, mouseInputVector.x * sensitivityX, 0);
		}
		else
		{
			rotationY += mouseInputVector.y * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
	}

	//	listener for zoom
	public void SetZoomLevel (int zoomLevel) {
		sensitivityX	=	_initSensitivityX / (zoomLevel * zoomLevel);
		sensitivityY	=	_initSensitivityY / (zoomLevel * zoomLevel);
	}
	
}