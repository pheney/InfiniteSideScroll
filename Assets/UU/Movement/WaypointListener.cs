using UnityEngine;
using System.Collections;

public class WaypointListener : MonoBehaviour {
	
	public	string	waypointTag		=	"Waypoint";
	
	public	int	destinationId			=	-1;
	
	void OnTriggerEnter (Collider otherObject) {
		
		Transform	waypoint	=	otherObject.transform;
		
		//	ignore trigger object because it is NOT a waypoint
		if (!waypoint.tag.Equals(waypointTag)) return;
		
		//	ignore the waypoint if it's ID does not match the ID we are looking for
		//	exception: if we don't have an ID to look for, then use this waypoint
		if (destinationId > -1 && !waypoint.GetInstanceID().Equals(destinationId)) return;
		
		//	creep is to be destroyed at this waypoint, so 
		//	there's no point in requesting the next waypoint
		if (waypoint.GetComponent<WaypointControl>().destroyArriveCreeps) return;
		
		//	pull the next destination from this waypoint
		Transform	wp	=	waypoint.GetComponent<WaypointControl>().GetNextWaypoint();
		
		//	pass the new waypoint to the rest of this object
		SendMessage ("SetTarget", wp);
		
		//	update the destination ID
		destinationId	= wp.GetInstanceID();
	}
	
	//	listener	//
	
	public void TeleportToPosition (Vector3 position) {
		transform.position	=	position;
	}
}
