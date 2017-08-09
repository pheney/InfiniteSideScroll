using UnityEngine;
using System.Collections;
using PLib;
using PLib.General;
using PLib.Rand;

public class SimpleHomingControl : MonoBehaviour {
	
	public	bool			debugRayToTarget		=	false;
	public	bool			artillerySystem			=	false;		//	extract position from homingTarget
	public	GuidanceSystem	guidanceSystem			=	GuidanceSystem.LookAt;
	public	bool			randomizeLaunchRotation	=	false;
	
	public	float			levelAdjustment	=	.05f;		//	% improvement to TurnRate and Roll/Pitch/Yaw
	
	//	used for the intermediate guidance system
	public	float			singleTurnRate	=	3;		//	deg/sec (used for axiis)
	
	//	used for the advanced guidance system
	public	float			rollRate		=	360;
	public	float			pitchRate		=	180;
	public	float			yawRate			=	90;		//	deg/sec
	public	bool			enableGlitch	=	false;
	public	float			glitchDuration	=	.2f;	//	seconds
	public	bool			enableCorkscrew	=	true;	//	craft rolls when pointed at target
	
	public	bool			enableRetarget	=	false;
	
	public	Transform		homingTarget;


	//	cache	//
	
	private	Transform	_transform;
	
	private	float		_maxSingleTurnRate;
	private	float		_cosRollAngle, _rollAngle;
	private	float		_cosPitchAngle, _pitchAngle;
	private	float		_cosYawAngle, _yawAngle;
	private	Vector3		_maneuverVector;
	private	Vector3		_maneuverMask		=	Vector3.zero;
	private	Vector3		_targetPoint;
	
	private	float		_eventTime;
	
	// Use this for initialization
	void Start () {
		_transform	=	transform;
		_maxSingleTurnRate	=	singleTurnRate	+3;
		
		if (!guidanceSystem.Equals(GuidanceSystem.LookAt)) {
			
			//	start the missile randomly rolled on its long axis
			if (randomizeLaunchRotation) {
				_transform.GetRoot().Rotate (0,0, 360 * Random.value);
			}
			if (guidanceSystem.Equals(GuidanceSystem.SeparateRollYawPitchRates)) {
				_eventTime		=	Time.time + Mathf.Clamp (glitchDuration + .1f * Random.Range (.85f, 1.15f), .25f, .45f);
				_maneuverMask	=	Vector3.one * .1f;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!homingTarget && !artillerySystem)	{
			if (enableRetarget) 
				BroadcastMessage ("ActivateSensor");
			return;
		}
		
		//	update target position for guided weapons
		if (!artillerySystem) {
			_targetPoint	=	homingTarget.position;
		}
		
		//	Switch off guidance for artillery rounds that miss their target.
		//	This allows them to just continue straight without making a sharp turn before impact.
		//	Note Vector.DOT < .9 is ~ 25 degrees, so if the target is more than 25 degrees off angle,
		//	then the missiles just fly straight until they hit something or run out of fuel.
		//	TODO	confirm this isn't a problem given the missiles are launched indirectly and so
		//			are already pointing slightly off target
		if (artillerySystem
			& Vector3.Dot (transform.forward, (_targetPoint - transform.position).normalized) < .9f) {
				_targetPoint	=	transform.forward * 10;
			}
		
		switch (guidanceSystem) {
		case GuidanceSystem.LookAt:
				UpdateTrivialGuidance();
			break;
		case GuidanceSystem.SingleTurnRate:
				UpdateIntermediateGuidance();
				break;
		case GuidanceSystem.SeparateRollYawPitchRates:
				UpdateAdvancedGuidance();
				if (enableGlitch) UpdateGlitch();
				break;
		}
		
		if (homingTarget && debugRayToTarget) {
			Debug.DrawLine (_transform.position, _targetPoint, Color.magenta);
		}
	}
	
	private void UpdateTrivialGuidance() {
		//_transform.LookAt(homingTarget);
		_transform.LookAt(_targetPoint);
	}
	
	private void UpdateIntermediateGuidance() {
		//Quaternion	goalRotation	=	Quaternion.LookRotation (homingTarget.position - _transform.position);
		Quaternion	goalRotation	=	Quaternion.LookRotation (_targetPoint - _transform.position);
		float		deltaAngle		=	Quaternion.Angle (_transform.rotation, goalRotation);
		float		rotationSpeed	=	Mathf.Clamp (deltaAngle, -singleTurnRate, singleTurnRate);
		
		_transform.rotation	=	Quaternion.Slerp (_transform.rotation, goalRotation, rotationSpeed * Time.deltaTime);
	}
	
	private void UpdateAdvancedGuidance() {
		//Vector3		goalLookVector	=	homingTarget.position - _transform.position;
		Vector3		goalLookVector	=	_targetPoint - _transform.position;
					goalLookVector.Normalize();
		
		//	if the missile is pointing directly at the target,
		//	then don't compute maneuver becuase it throws lots of NaN's
		//	NOTE -- this is a cool effect, but it doesn't stop the NaN's
		if (enableCorkscrew) {
		
			//	.9 is 25.8 deg, .99 is 8 deg, .999 is 2.56 deg, .9999 is .8 deg
			if (Vector3.Dot (_transform.forward, goalLookVector) > .9999f) {
				_transform.Rotate (Vector3.forward, .5f * rollRate * Time.deltaTime);
				return;
			}
		}
		
		//	find deltaPitch
		_cosPitchAngle	=	Vector3.Dot (_transform.up, goalLookVector);	//	0 when aligned
		_cosPitchAngle	=	Mathf.Approximately (_cosPitchAngle, 0) ? _cosPitchAngle = .001f : _cosPitchAngle;
		_pitchAngle		=	Mathf.Rad2Deg * Mathf.Acos (_cosPitchAngle) -90;
		if (_pitchAngle > 180) _pitchAngle	=	_pitchAngle - 360;
		
		
		//	pitch
		_maneuverVector	=	Vector3.right * Mathf.Clamp (_pitchAngle * pitchRate, -pitchRate, pitchRate) * Time.deltaTime;
		_maneuverVector.Scale(_maneuverMask);
		if (!_maneuverVector.Equals(Vector3.zero))	
			_transform.Rotate (_maneuverVector);
		
		//	find deltaYaw
		_cosYawAngle		=	Vector3.Dot (_transform.right, goalLookVector);	//	0 when aligned
		_cosYawAngle	=	Mathf.Approximately (_cosYawAngle, 0) ? _cosYawAngle = .001f : _cosYawAngle;
		_yawAngle		=	Mathf.Rad2Deg * Mathf.Acos (_cosYawAngle) +90;
		if (_yawAngle > 180) _yawAngle	=	_yawAngle - 360;
		
		//	yaw
		_maneuverVector	=	Vector3.up * Mathf.Clamp (_yawAngle * yawRate, -yawRate, yawRate) * Time.deltaTime;
		_maneuverVector.Scale(_maneuverMask);
		if (!_maneuverVector.Equals(Vector3.zero))	
			_transform.Rotate (_maneuverVector);
		
		//	find deltaRoll
		
		//	deltaRoll be determined based on which of the craft's 2 other turn values (pitch and yaw)
		//	are higher. The craft should always be trying to roll in such a way that pitch/yaw can
		//	be used to fullest effect.
		
		if (pitchRate > yawRate) {
			//	roll to maximize pitch
			_cosRollAngle	=	Vector3.Dot (_transform.right, goalLookVector);	//	0 when aligned
			_cosRollAngle	=	Mathf.Approximately (_cosRollAngle, 0) ? _cosRollAngle = .001f : _cosRollAngle;
			_rollAngle		=	Mathf.Rad2Deg * Mathf.Acos (_cosRollAngle) -90;
		} else {
			//	roll to maximize yaw
			_cosRollAngle	=	Vector3.Dot (_transform.up, goalLookVector);	//	0 when aligned
			_cosRollAngle	=	Mathf.Approximately (_cosRollAngle, 0) ? _cosRollAngle = .001f : _cosRollAngle;
			_rollAngle		=	Mathf.Rad2Deg * Mathf.Acos (_cosRollAngle) +90;
		}
		
		//	roll
		if (_rollAngle > 180) _rollAngle	=	_rollAngle - 360;
		_maneuverVector	=	Vector3.forward * Mathf.Clamp (_rollAngle * rollRate, -rollRate, rollRate) * Time.deltaTime;
		_maneuverVector.Scale(_maneuverMask);
		if (!_maneuverVector.Equals(Vector3.zero))	
			_transform.Rotate (_maneuverVector);
	}
	
	private void UpdateGlitch() {
		if (Time.time < _eventTime) return;
		_eventTime		=	Time.time + glitchDuration * (1 + Random.Range(-.25f,-.25f));
		_maneuverMask	=	Vector3.one - PRand.RandomOrthoVector();
	}
	
	//	message listener	//
	
	public void SetTarget (Transform target) {
		homingTarget	=	target;
		if (artillerySystem) {
			_targetPoint	=	new Vector3(homingTarget.position.x, 
											homingTarget.position.y, 
											homingTarget.position.z);
		} else {
			_targetPoint	=	homingTarget.position;
		}
	}
	
	public void SetLevel (int level) {
		switch (guidanceSystem) {
		case GuidanceSystem.SingleTurnRate:
			//	old (uses "levelAdjustment" as a raw level multiple
			//singleTurnRate	=	Mathf.Clamp (singleTurnRate + level * levelAdjustment, 0, _maxSingleTurnRate);
			//	new (uses "percntImprovePerLevel" as an actual percentage
			singleTurnRate	=	Mathf.Clamp (singleTurnRate * (1 + level * levelAdjustment), 0, _maxSingleTurnRate);
			break;
		case GuidanceSystem.SeparateRollYawPitchRates:
			pitchRate		=	Mathf.Clamp (pitchRate * (1 + level * levelAdjustment), 0, 720);
			rollRate		=	Mathf.Clamp (rollRate * (1 + level * levelAdjustment), 0, 540);
			yawRate			=	Mathf.Clamp (yawRate * (1 + level * levelAdjustment), 0, 360);
			break;
		default:
			//	there's no improvement possible for "LookAt" because
			//	it is instantaneous
			break;
		}
	}
	
	
	public void ImproveManeuver (float improvement) {
		switch (guidanceSystem) {
		case GuidanceSystem.SingleTurnRate:
			//singleTurnRate	=	Mathf.Clamp (singleTurnRate * (1 + improvement), 0, _maxSingleTurnRate);
			//	previous line always sets singleTurnRate to 0 --why??
			//	following line works as expected (although the max value isn't capped)
			singleTurnRate	=	singleTurnRate * (1 + improvement);
			break;
		case GuidanceSystem.SeparateRollYawPitchRates:
			pitchRate		=	Mathf.Clamp (pitchRate * (1 + improvement), 0, 720);
			rollRate		=	Mathf.Clamp (rollRate * (1 + improvement), 0, 540);
			yawRate			=	Mathf.Clamp (yawRate * (1 + improvement), 0, 360);
			break;
		default:
			//	there's no improvement possible for "LookAt" because
			//	it is instantaneous
			break;
		}
	}
	
	
}

public enum GuidanceSystem {
	LookAt,
	SingleTurnRate,
	SeparateRollYawPitchRates
}