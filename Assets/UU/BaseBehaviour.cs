using UnityEngine;
using PLib;
using PLib.Logging;

/// <summary>
/// Loggable base class. Any class that extends this class automatically extends MonoBehavior
/// and has built in console logging capability.
/// </summary>
public class BaseBehaviour : MonoBehaviour {

	public	LogEvents	console;

	[System.Serializable]
	public class LogEvents {
		public	bool		logResults		=	false;
		public	bool		logMethods		=	false;
		public	bool		logLifeCycle	=	false;
		public	bool		logUpdates		=	false;
		public	bool		logRender		=	false;
		public	bool		logGui			=	false;
		public	bool		logMouse		=	false;
		public	bool		logEditor		=	false;
		public	bool		logNetwork		=	false;
		public	bool		logApplication	=	false;
		public	bool		logCollision	=	false;
		public	bool		logTriggers		=	false;
	}
	private		string		_consoleTag;
	protected	Transform	_transform;
	protected	Rigidbody	_rigidbody;
	protected	Collider	_collider;
	protected	NetworkView	_networkView;
	protected	bool		_isLocal;

	//////////////////
	//	LIFECYCLE	//
	//////////////////

	#region Lifecycle

	protected virtual void Awake () {
		_consoleTag		=	gameObject.ConsoleTag(this.GetType().ToString());
		if (console.logLifeCycle) LogMethodEntry(this.MethodName());
		_transform		=	transform;
		_rigidbody		=	GetComponent<Rigidbody>();
		_collider		=	GetComponent<Collider>();
		//_networkView	=	GetComponent<NetworkView>();
		//_isLocal		=	_networkView && _networkView.isMine ? true : false;
	}
	
	protected virtual void Start () {
		if (console.logLifeCycle) LogMethodEntry(this.MethodName());
	}
	
	protected virtual void Reset () {
		if (console.logLifeCycle) LogMethodEntry(this.MethodName());
	}

	protected virtual void OnEnable () {
		if (console.logLifeCycle) LogMethodEntry(this.MethodName());
	}

	protected virtual void OnDisable () {
		if (console.logLifeCycle) LogMethodEntry(this.MethodName());
	}

	protected virtual void OnDestroy () {
		if (console.logLifeCycle) LogMethodEntry(this.MethodName());
	}
	
	protected virtual void Update () {
		if (console.logUpdates) LogMethodEntry(this.MethodName());
	}

	protected virtual void LateUpdate () {
		if (console.logUpdates) LogMethodEntry(this.MethodName());
	}

	protected virtual void FixedUpdate () {
		if (console.logUpdates) LogMethodEntry(this.MethodName());
	}

	#endregion

	//////////////////
	//	UI EVENTS	//
	//////////////////

	#region UI Events

	/// <summary>
	/// OnGUI is called for rendering and handling GUI events.
	/// </summary>
	protected virtual void OnGUI () {
		if (console.logGui) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnMouseDown is called when the user has pressed the mouse button while over the GUIElement or Collider.
	/// </summary>
	protected virtual void OnMouseDown () {
		if (console.logMouse) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnMouseDrag is called when the user has clicked on a GUIElement or Collider and is still holding down the mouse.
	/// </summary>
	protected virtual void OnMouseDrag () {
		if (console.logMouse) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called when the mouse enters the GUIElement or Collider.
	/// </summary>
	protected virtual void OnMouseEnter () {
		if (console.logMouse) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called when the mouse is not any longer over the GUIElement or Collider.
	/// </summary>
	protected virtual void OnMouseExit () {
		if (console.logMouse) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called every frame while the mouse is over the GUIElement or Collider.
	/// </summary>
	protected virtual void OnMouseOver () {
		if (console.logMouse) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnMouseUp is called when the user has released the mouse button.
	/// </summary>
	protected virtual void OnMouseUp () {
		if (console.logMouse) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnMouseUpAsButton is only called when the mouse is released over the same GUIElement or Collider as it was pressed.
	/// </summary>
	protected virtual void OnMouseUpAsButton () {
		if (console.logMouse) LogMethodEntry(this.MethodName());
	}

	#endregion
	
	//////////////////////
	//	EDITOR EVENTS	//
	//////////////////////

	#region Editor Events

	/// <summary>
	/// Implement OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn.
	/// </summary>
	protected virtual void OnDrawGizmos () {
		if (console.logEditor) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Implement this OnDrawGizmosSelected if you want to draw gizmos only if the object is selected.
	/// </summary>
	protected virtual void OnDrawGizmosSelected () {
		if (console.logEditor) LogMethodEntry(this.MethodName());
	}

	#endregion

	//////////////////////
	//	PHYSICS EVENTS	//
	//////////////////////

	#region Physics Events

	/// <summary>
	/// OnParticleCollision is called when a particle hits a collider. This can be used 
	/// to apply damage to a game object when hit by particles.	
	/// <p><b>Legacy particle system:</b>
	/// <p>This message is sent to all scripts attached to the <i>WorldParticleCollider</i> and to the 
	/// Collider that was hit. The message is only sent if you enable sendCollisionMessage 
	/// in the inspector of the <i>WorldParticleCollider</i>.
	/// <p><b>Shuriken particle system:</b>
	/// <p>This message is sent to scripts attached to particle systems and to the Collider that 
	/// was hit.
	/// <p>When OnParticleCollision is invoked from a script attached to a GameObject with 
	/// a Collider the GameObject parameter represents the ParticleSystem. The Collider receives 
	/// at most one message per particle system that collided with it in any given frame even 
	/// when the particle system struck the Collider with multiple particles in the current frame. 
	/// In order to retrieve detailed information about all the collisions caused by the 
	/// ParticleSystem <i>ParticleSystem.GetCollisionEvents</i> must be used to retrieve the array of 
	/// <i>ParticleSystem.CollisionEvent</i>.
	/// <p>When OnParticleCollision is invoked from a script attached to a ParticleSystem the 
	/// GameObject parameter represents a GameObject with an attached Collider struck by the 
	/// ParticleSystem. The ParticleSystem will receive at most one message per Collider that is 
	/// struck. As above <i>ParticleSystem.GetCollisionEvents</i> must be used to retrieve all the 
	/// collisions incident on the GameObject.
	/// <p>Messages are only sent if you enable <u>Send Collision Messages</u> in the inspector 
	/// of the particle system collision module. 
	/// <p>OnParticleCollision can be a co-routine, simply use the yield statement in the function.
	/// </summary>
	protected virtual void OnParticleCollision (GameObject other) {
		if (console.logTriggers || console.logCollision) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnControllerColliderHit is called when the controller hits a collider while performing a Move.
	/// </summary>
	protected virtual void OnControllerColliderHit (ControllerColliderHit hit) {
		if (console.logTriggers || console.logCollision) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnTriggerEnter is called when the Collider other enters the trigger.
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnTriggerEnter (Collider other) {
		if (console.logTriggers) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnTriggerStay is called once per frame for every Collider other that is touching the trigger.
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnTriggerStay (Collider other) {
		if (console.logTriggers) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnTriggerExit is called when the Collider other has stopped touching the trigger.
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnTriggerExit (Collider other) {
		if (console.logTriggers) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.	
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnCollisionEnter (Collision other) {
		if (console.logCollision) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider.
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnCollisionStay (Collision other) {
		if (console.logCollision) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider.
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnCollisionExit (Collision other) {
		if (console.logCollision) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Sent when another object enters a trigger collider attached to this object (2D physics only).
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnTriggerEnter2D (Collider2D other) {
		if (console.logTriggers) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Sent each frame where another object is within a trigger collider attached to this object (2D physics only).
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnTriggerStay2D (Collider2D other) {
		if (console.logTriggers) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Sent when another object leaves a trigger collider attached to this object (2D physics only).
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnTriggerExit2D (Collider2D other) {
		if (console.logTriggers) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Sent when an incoming collider makes contact with this object's collider (2D physics only).
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnCollisionEnter2D (Collision2D other) {
		if (console.logCollision) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Sent each frame where a collider on another object is touching this object's collider (2D physics only).
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnCollisionStay2D (Collision2D other) {
		if (console.logCollision) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Sent when a collider on another object stops touching this object's collider (2D physics only).
	/// </summary>
	/// <param name="other">Other.</param>
	protected virtual void OnCollisionExit2D (Collision2D other) {
		if (console.logCollision) LogMethodEntry(this.MethodName());
	}

	#endregion

	//////////////////////
	//	RENDERER EVENTS	//
	//////////////////////

	#region Renderer Events

	/// <summary>
	/// OnBecameVisible is called when the renderer became visible by any camera.
	/// </summary>
	protected virtual void OnBecameVisible () {
		if (console.logRender) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnBecameInvisible is called when the renderer is no longer visible by any camera.
	/// </summary>
	protected virtual void OnBecameInvisible () {
		if (console.logRender) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnPreRender is called before a camera starts rendering the scene. This function is 
	/// called only if the script is attached to the camera and is enabled. Note that if you 
	/// change camera's viewing parameters (e.g. fieldOfView) here, they will only take effect 
	/// the next frame. Do that in OnPreCull instead. OnPreRender can be a co-routine, simply 
	/// use the yield statement in the function.
	/// </summary>
	protected virtual void OnPreRender () {
		if (console.logRender) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// OnPostRender is called after a camera finished rendering the scene. This function is 
	/// called only if the script is attached to the camera and is enabled. OnPostRender can 
	/// be a co-routine, simply use the yield statement in the function. OnPostRender is called 
	/// after the camera renders all its objects. If you want to do something after all cameras 
	/// and GUI is rendered, use WaitForEndOfFrame coroutine.
	/// </summary>
	protected virtual void OnPostRender () {
		if (console.logRender) LogMethodEntry(this.MethodName());
	}

	#endregion

	//////////////////////////////
	//	MASTER SERVER EVENTS	//
	//////////////////////////////

	#region Master Server Events

	/// <summary>
	/// Raises the master server event event.
	/// </summary>
	/// <param name="msEvent">Ms event.</param>
	protected virtual void OnMasterServerEvent (MasterServerEvent msEvent) {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called on clients or servers when there is a problem connecting to the MasterServer
	/// </summary>
	/// <param name="info">Info.</param>
	protected virtual void OnFailedToConnectToMasterServer (NetworkConnectionError info) {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}

	#endregion

	//////////////////////
	//	NETWORK EVENTS	//
	//////////////////////

	#region Network Events
	
	/// <summary>
	/// Called on the client when you have successfully connected to a server.
	/// </summary>
	protected virtual void OnConnectedToServer	 () {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called on the client when the connection was lost or you disconnected from the server, andbut also on the server when the connection has disconnected.
	/// </summary>
	/// <param name="info">Info.</param>
	protected virtual void OnDisconnectedFromServer (NetworkDisconnection info)  {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}
	
	/// <summary>
	/// Called on the client when a connection attempt fails for some reason.
	/// </summary>
	/// <param name="error">Error.</param>
	protected virtual void OnFailedToConnect (NetworkConnectionError error) {
		if (console.logUpdates) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called on objects which have been network instantiated with Network.Instantiate.
	/// </summary>
	/// <param name="info">Info.</param>
	protected virtual void OnNetworkInstantiate (NetworkMessageInfo info) {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called on the server whenever a new player has successfully connected.
	/// </summary>
	/// <param name="player">Player.</param>
	protected virtual void OnPlayerConnected (NetworkPlayer player) {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called on the server whenever a player is disconnected from the server.
	/// </summary>
	/// <param name="player">Player.</param>
	protected virtual void OnPlayerDisconnected (NetworkPlayer player) {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Used to customize synchronization of variables in a script watched by a network view.
	/// Requires an attached NetworkView set to observe this script
	/// </summary>
	/// <param name="stream">Stream.</param>
	/// <param name="info">Info.</param>
	protected virtual void OnSerializeNetworkView (BitStream stream, NetworkMessageInfo info) {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Called on the server whenever a Network.InitializeServer was invoked and has completed.
	/// </summary>
	protected virtual void OnServerInitialized () {
		if (console.logNetwork) LogMethodEntry(this.MethodName());
	}

	#endregion
	
	//////////////////////////
	//	APPLICATION EVENTS	//
	//////////////////////////

	#region Application Events
	
	/// <summary>
	/// This function is called after a new level was loaded.
	/// </summary>
	protected virtual void OnLevelWasLoaded () {
		if (console.logApplication) LogMethodEntry(this.MethodName());
	}
	
	/// <summary>
	/// Sent to all game objects when the player pauses.
	/// </summary>
	protected virtual void OnApplicationPause () {
		if (console.logApplication) LogMethodEntry(this.MethodName());
	}
	
	/// <summary>
	/// Sent to all game objects when the player gets or loses focus.
	/// </summary>
	protected virtual void OnApplicationFocus () {
		if (console.logApplication) LogMethodEntry(this.MethodName());
	}

	/// <summary>
	/// Sent to all game objects before the application is quit.
	/// </summary>
	protected virtual void OnApplicationQuit () {
		if (console.logApplication) LogMethodEntry(this.MethodName());
	}

	#endregion

	//////////////////
	//	HELPERS		//
	//////////////////

	#region Helpers
	
	public virtual void LogResults(string info) {
		if (console.logResults) PLog.Echo((info).PrependRuntime());
	}

	public virtual void LogMethodEntry(string methodName) {
		if (console.logMethods) PLog.Echo((_consoleTag + methodName).PrependRuntime());
	}

	public virtual void ResetScript(string className) {
		if (className != this.name) return;
		if (console.logMethods) LogMethodEntry(this.MethodName());
		this.Reset();
	}
		
	protected void AbortInvoke (string InvokedMethod) {
		if (IsInvoking(InvokedMethod)) CancelInvoke (InvokedMethod);
	}

	#endregion

}

public static class UnityMessage {
	
	#region Lifecycle
	public static string AWAKE = "Awake";
	public static string START = "Start";
	public static string RESET = "Reset";
	public static string ON_ENABLE = "OnEnable";
	public static string ON_DISABLE = "OnDisable";
	public static string DESTROY = "Destroy";
	public static string UPDATE = "Update";
	public static string LATE_UPDATE = "LateUpdate";
	public static string FIXED_UPDATE = "FixedUpdate";	
	#endregion
	
	//////////////////
	//	UI EVENTS	//
	//////////////////
	
	#region UI Events
	public static string ON_GUI = "OnGUI";
	public static string ON_MOUSE_DOWN = "OnMouseDown";
	public static string ON_MOUSE_DRAG = "OnMouseDrag";
	public static string ON_MOUSE_ENTER = "OnMouseEnter";
	public static string ON_MOUSE_EXIT = "OnMouseExit";
	public static string ON_MOUSE_OVER = "OnMouseOver";
	public static string ON_MOUSE_UP = "OnMouseUp";
	public static string ON_MOUSE_UP_AS_BUTTON = "OnMouseUpAsButton";	
	#endregion
	
	//////////////////////
	//	EDITOR EVENTS	//
	//////////////////////
	
	#region Editor Events
	public static string ON_DRAW_GIZMOS = "OnDrawGizmos";
	public static string ON_DRAW_GIZMOS_SELECTED = "OnDrawGizmosSelected";	
	#endregion
	
	//////////////////////
	//	PHYSICS EVENTS	//
	//////////////////////
	
	#region Physics Events
	public static string ON_PARTICLE_COLLISION = "OnParticleCollision";
	public static string ON_CONTROLLER_COLLIDER_HIT = "OnControllerColliderHit";
	public static string ON_TRIGGER_ENTER = "OnTriggerEnter";
	public static string ON_TRIGGER_STAY = "OnTriggerStay";
	public static string ON_TRIGGER_EXIT = "OnTriggerExit";
	public static string ON_COLLISION_ENTER = "OnCollisionEnter";
	public static string ON_COLLISION_STAY = "OnCollisionStay";
	public static string ON_COLLISION_EXIT = "OnCollisionExit";
	public static string ON_TRIGGER_ENTER_2D = "OnTriggerEnter2D";
	public static string ON_TRIGGER_STAY_2D = "OnTriggerStay2D";
	public static string ON_TRIGGER_EXIT_2D = "OnTriggerExit2D";
	public static string ON_COLLISISON_ENTER_2D = "OnCollisionEnter2D";
	public static string ON_COLLISION_STAY_2D = "OnCollisionStay2D";
	public static string ON_COLLISION_EXIT_2D = "OnCollisionExit2D";	
	#endregion
	
	//////////////////////
	//	RENDERER EVENTS	//
	//////////////////////
	
	#region Renderer Events
	public static string ON_BECAME_VISIBLE = "OnBecameVisible";
	public static string ON_BECAME_INVISIBLE = "OnBecameInvisible";
	public static string ON_PRE_RENDER = "OnPreRender";
	public static string ON_POST_RENDER = "OnPostRender";
	#endregion
	
	//////////////////////////////
	//	MASTER SERVER EVENTS	//
	//////////////////////////////
	
	#region Master Server Events
	public static string ON_MASTER_SERVER_EVENT = "OnMasterServerEvent";
	public static string ON_FAILED_TO_CONNECT_TO_MASTER_SERVER = "OnFailedToConnectToMasterServer";
	#endregion
	
	//////////////////////
	//	NETWORK EVENTS	//
	//////////////////////
	
	#region Network Events
	public static string ON_CONNECTED_TO_SERVER = "OnConnectedToServer";
	public static string ON_DISCONNECTED_FROM_SERVER = "OnDisconnectedFromServer";
	public static string ON_FAILED_TO_CONNECT = "OnFailedToConnect";
	public static string ON_NETWORK_INSTANTIATE = "OnNetworkInstantiate";
	public static string ON_PLAYER_CONNECTED = "OnPlayerConnected";
	public static string ON_PLAYER_DISCONNECTED = "OnPlayerDisconnected";
	public static string ON_SERIALIZE_NETWORK_VIEW = "OnSerializeNetworkView";
	public static string ON_SERVER_INITIALIZED = "OnServerInitialized";	
	#endregion
	
	//////////////////////////
	//	APPLICATION EVENTS	//
	//////////////////////////
	
	#region Application Events
	public static string ON_LEVEL_WAS_LOADED = "OnLevelWasLoaded";
	public static string ON_APPLICATION_PAUSE = "OnApplicationPause";
	public static string ON_APPLICATION_FOCUS = "OnApplicationFocus";
	public static string ON_APPLICATION_QUIT = "OnApplicationQuit";	
	#endregion
	
	//////////////////
	//	HELPERS		//
	//////////////////
	
	#region Helpers
	public static string RESET_SCRIPT = "ResetScript";
	public static string ABORT_INVOKE = "InvokedMethod";
	#endregion
	
}