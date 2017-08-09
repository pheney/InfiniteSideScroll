using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PLib;
using PLib.Rand;
using PLib.General;

//	Hide compiler warnings regarding private fields that are declared but never used
#pragma warning disable 414
[ExecuteInEditMode]
public class WaypointControl : MonoBehaviour {
	
	public	int					id;
	public	bool				lookAtNextWaypoint		=	true;
	public	string				creepTag				=	"Enemy";
	public	bool				enableTeleportToNextWP	=	false;
	public	bool				destroyArriveCreeps		=	false;
	public	bool				enablePlayerDamage		=	false;
	public	List<Transform>		nextWaypointList;
	public	string				gameControllerTag		=	"GameController";
	public	Color				defaultWaypointColor	=	Color.red;
	
	private	static	Vector3		offset;
	private	static	Transform	gameController;
	
	void Awake () {
		offset			=	PRand.RandomVector3(.05f);
		gameController	=	GameObject.FindGameObjectWithTag(gameControllerTag).transform;
	}
	
	void Start () {
		id	=	gameObject.GetInstanceID();
		if (lookAtNextWaypoint) {
			transform.LookAt (nextWaypointList[0]);
		}
	}
	
	//	always draw the following gizmos
	void OnDrawGizmos() {
		
		Gizmos.color	=	defaultWaypointColor;
		
		//	draw a small sphere to identify the way point
		Gizmos.DrawSphere (transform.position, .5f);
		//	draw a red line to all way points this wp points to
		if (enableTeleportToNextWP) return;
		DrawWaypointGizmos(Vector3.zero);
	}
	
	//	only draw these gizmos when the object is selected
	void OnDrawGizmosSelected() {
		Gizmos.color	=	Color.green;
		
		//	draw a small sphere to identify the way point
		Gizmos.DrawSphere (transform.position, .5f);
		//	draw a green line to all way points this wp points to
		DrawWaypointGizmos(offset);
	}
	
	private void DrawWaypointGizmos(Vector3 offset) {
		
		for (int i = nextWaypointList.Count ; i > 0 ; i--) {
			Gizmos.DrawLine (transform.position + offset,
				nextWaypointList[i-1].position + offset);
		}
	}
	
	void OnTriggerEnter (Collider other) {
		Transform	enemyUnit	=	other.GetRoot();
		
		if (!enemyUnit.tag.Equals(creepTag)) return;
		
		if (enableTeleportToNextWP) {
			//	notify creep that it is to teleport to the next waypoint
			enemyUnit.SendMessage ("TeleportToPosition", GetNextWaypoint().position, SendMessageOptions.DontRequireReceiver);
		}
		
		if (enablePlayerDamage) {
			//	Notify game manager that a creep attack succeeded
			//	TODO - comment out until review is complete
			//gameController.SendMessage ("PlayerDamaged", enemyUnit.GetComponent<PlayerRewards>().resources, SendMessageOptions.DontRequireReceiver);
		}
		
		if (destroyArriveCreeps) {
			//	creep only makes one pass accross the board
			enemyUnit.SendMessage("SilentDestruct");
			//Debug.Log (UU.FormatTime(Time.time) + " : " + UU.GetRoot(other).ToString() + " silently destroyed");
		}
	}
	
	//	getter	//
	
	public Transform GetNextWaypoint () {
		return nextWaypointList[Random.Range(0, nextWaypointList.Count)];
	}
}
