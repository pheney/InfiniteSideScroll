/// <summary>
/// Missile flight control. Provides an erratic flight.
/// </summary>
using UnityEngine;
using PLib.Logging;

public class MissileControl : BaseBehaviour {

	public		string		targetTag		=	"Enemy";
	public		float		acceleration	=	.05f;		//	m/s/s
	public		float		maxVelocity		=	.1f;		//	m/s
	private		float		_curVelocity;
	
	public		float		rollSpeed		=	180;		//	deg/sec
	public		float		pitchSpeed		=	180;		//	deg/sec
	
	public		float		seekRange		=	250;		//	meters; sense target range
	public		float		seekPeriod		=	.5f;		//	seconds; how often the seeker 'radar' pings
	private		float		_seekTime;
	public		float		detRange		=	3;			//	meters; to explode
	public		int			detDamage		=	100;		//	explosive damage

	public		bool		avoidTerrain	=	false;
	public		float		terrainDetectionPeriod	=	.51f;	//	seconds
	private		float		_terrainDetectionTime;
	public		float		minTerrainDistance		=	300;	//	meters

	private		Vector3		_targetVector;
	private		float		_curRoll;
	private		float		_curPitch;
	private		float		_goalRoll;
	private		float		_goalPitch;
	private		float		_rollDelta;
	private		float		_pitchDelta;
	private		bool		_flip;
	private		float		_flipChance		=	.05f;
	public		float		flipDuration	=	.5f;		//	seconds; duration of flip (+/- 10%)

	private		Transform	_targetTransform;
	private		Transform	_thisTransform;
	
	//	LIFECYCLE
	protected override void Start () {
		base.Start();
		_thisTransform		=	transform;
		_flip				=	false;
		_seekTime			=	0;
	}
	
	//	LIFECYCLE
	protected override void Update () {
		base.Update();
		if (!_targetTransform && (Time.time > _seekTime)) {
			FindTarget(targetTag);
			_seekTime	=	Time.time + seekPeriod;
		}

		// add 'dirt' to its flight
		_thisTransform.Rotate(Vector3.forward * Random.Range(-1f,1f) * 2.5f);
		_thisTransform.Rotate(Vector3.right * Random.Range(-1f,1f) * .5f);
		
		if (Random.value < _flipChance && !IsInvoking("UnFlip") && !IsInvoking("NoFlip")) {
			Flip();
		}
		
		if (!_targetTransform) return;
		
		// update the direction vector to the target
		_targetVector		=	(_targetTransform.position - _thisTransform.position);
		
		UpdateRoll();
		UpdatePitch();
		if (avoidTerrain) UpdateTerrainSensing();

		//	check for impact
		if (Vector3.SqrMagnitude (_targetVector) < detRange ) {
			//	send self-destruct message
			//GetComponent<AutoDestruct>().SelfDestruct();		

			//	send message to target to deliver damage
			//_targetTransform.BroadcastMessage ("AttackDamage", .5f *(Random.value + 1) * detDamage, SendMessageOptions.DontRequireReceiver);
		}
	}
	
	//	LIFECYCLE
	protected override void FixedUpdate () {
		base.FixedUpdate();
		UpdateVelocity();
	}

	//////////////////
	//	Helpers		//
	//////////////////

	private void Flip () {
		_flip			=	true;
		_flipChance		*=	.95f;
		float	_UnFlipDelay	=	Random.value * .1f + _flipChance * 45;
		_UnFlipDelay			*=	flipDuration;
		Invoke ("UnFlip", _UnFlipDelay);
	}
	
	private void UnFlip () { 
		_flip	=	false;
		float	_NoFlipDelay	=	Random.value * .1f + _flipChance * 45;
		_NoFlipDelay			*=	flipDuration;
		Invoke ("NoFlip", _NoFlipDelay);
	}
	
	private void NoFlip () { }
	
	private void UpdateRoll () {

		_rollDelta		=	Vector3.Dot ( _thisTransform.TransformDirection(Vector3.right), _targetVector.normalized);
		if (_flip) _rollDelta *= -1;
		
		// update the roll (z-axis) rotation
		_thisTransform.Rotate(Vector3.forward * -_rollDelta * rollSpeed * Time.deltaTime);
	}
	
	private void UpdatePitch () {

		_pitchDelta		=	-Vector3.Dot ( _thisTransform.TransformDirection(Vector3.up), (Vector3.right * _targetVector.x +Vector3.up * _targetVector.y + Vector3.forward * _targetVector.z).normalized);
		if (_flip) _pitchDelta *= -1;
		
		// update the pitch (x-axis) rotation
		_thisTransform.Rotate (Vector3.right, _pitchDelta);
	}
	
	private void UpdateVelocity () {
		
		// update velocity by adding acceleration
		if (_curVelocity < maxVelocity) _curVelocity	+=	acceleration * Time.deltaTime;
		
		// move missile
		if (Mathf.Abs(_curVelocity) > .01f) _thisTransform.Translate (Vector3.forward * _curVelocity);
	}

	/// <summary>
	/// TODO
	/// </summary>
	private void UpdateTerrainSensing() {}

	//////////////////////////
	//	Event Listeners		//
	//////////////////////////
	
	//	Terrain Avoidance trigger -- uses a large trigger attached to the missile to sense
	//	and avoid anything on the terrain layer
	protected override void OnTriggerEnter(Collider other) {
		if (!other.gameObject.tag.Equals("Terrain")) return;
		if (Time.time < _terrainDetectionTime) return;
		base.OnTriggerEnter(other);

		_terrainDetectionTime	=	Time.time + terrainDetectionPeriod;
				
		// TODO find distance to nearest point on terrain
		Vector3	nearestPoint	=	_thisTransform.position;
				
		if (Vector3.SqrMagnitude(nearestPoint - _thisTransform.position) > minTerrainDistance*minTerrainDistance) return;
		else {
			LogMethodEntry("Warning! Missile dangerously close to the ground...");
		}
	}
		
	//////////////////////////
	//	Message Listeners	//
	//////////////////////////

	//	message listener
	public void FindTarget(string targetTag) {
		LogMethodEntry(this.MethodName());
		GameObject[] go = GameObject.FindGameObjectsWithTag(targetTag);
		if (go == null || go.Length == 0) return;
		
		float sqrSeekRange	=	seekRange * seekRange;
		// only choose a target that is already in range
		for (int i = go.Length ; i > 0 ; i--) {
			if (Vector3.SqrMagnitude (go[i-1].transform.position - _thisTransform.position) < sqrSeekRange) {
				_targetTransform = go[i-1].transform;
				break;
			}
		}
	}

	//	message listener
	public void InitialVelocity (float initialVelocity) {
		LogMethodEntry(this.MethodName());
		_curVelocity	=	initialVelocity;
	}
	
	//	message listener
	public void SetTarget (Transform target) {
		LogMethodEntry(this.MethodName());
		_targetTransform	=	target;
	}
}
