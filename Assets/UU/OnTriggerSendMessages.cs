using UnityEngine;
using System.Collections;
using PLib;
using PLib.General;

public class OnTriggerSendMessages : MonoBehaviour {

	public	bool		enableEnter			=	true;
	public	bool		enableExit			=	true;
	public	bool		enableStay			=	false;

	public	bool		triggerEnterOnInit	=	false;
	public	bool		triggerExitOnInit	=	false;

	public	string[]	activatingTags;
	public	LayerMask	activatingLayers	=	(LayerMask)0xfff;	//	everything

	public enum TriggerMethodType { TAGS, LAYERS, BOTH, NONE };
	public TriggerMethodType triggerMethod = TriggerMethodType.TAGS;
	
	public	TriggerMessage[]	triggerMessages;

	[Tooltip("Behaviors that will always recieve messages")]
	public	MonoBehaviour[]	behaviors;

	[Tooltip("Anyting in here will also receive messages")]
	public	GameObject[]	monitoringObjects;

	protected	Transform	triggeringObject;

	//	LIFECYCLE

	void Start () {
		if (triggerEnterOnInit) SendMessages(TriggerMessage.TriggerEvent.ENTER);
		if (triggerExitOnInit) SendMessages(TriggerMessage.TriggerEvent.EXIT);
	}

	//	EVENTS

	public void OnTriggerEnter (Collider other) {
		if (!enabled) return;
		if (!enableEnter) return;
		if (!ValidTriggerObject(other.transform)) return;

		triggeringObject = other.transform;
		SendMessages(TriggerMessage.TriggerEvent.ENTER);
	}

	public void OnTriggerExit (Collider other) {
		if (!enabled) return;
		if (!enableExit) return;
		if (!ValidTriggerObject(other.transform)) return;

		triggeringObject = other.transform;
		SendMessages(TriggerMessage.TriggerEvent.EXIT);
	}
	
	public void OnTriggerStay (Collider other) {
		if (!enabled) return;
		if (!enableStay) return;
		if (!ValidTriggerObject(other.transform)) return;

		triggeringObject = other.transform;
		SendMessages(TriggerMessage.TriggerEvent.STAY);
	}

	//	HELPERS

	protected virtual bool ValidTriggerObject (Transform other) {
		if ((triggerMethod.Equals(TriggerMethodType.TAGS) ||
		     triggerMethod.Equals(TriggerMethodType.BOTH)) && !activatingTags.Contains(other.tag)) return false;
		if ((triggerMethod.Equals(TriggerMethodType.LAYERS) ||
		     triggerMethod.Equals(TriggerMethodType.BOTH)) && !activatingLayers.Contains(other.gameObject.layer)) return false;
		return true;
	}

	protected virtual void SendMessages (TriggerMessage.TriggerEvent triggerEvent) {
		foreach (TriggerMessage m in triggerMessages) {
			if (!m.triggerEvent.Equals(triggerEvent)) continue;

			//	send messages to listed behaviors, if required
			if (m.sendToBehaviors) {
				foreach (MonoBehaviour b in behaviors) b.SendMessage(m.name);
			}

			//	send messages to this game object, if required
			if (!m.sendToSelf.Equals(TriggerMessage.SendType.NONE)) {
				SendMessageToObject (transform, m.name, m.sendToSelf);
			}

			//	send messages to monitoring game objects, if required
			if (!m.sendToMonitor.Equals(TriggerMessage.SendType.NONE)) {
				foreach (GameObject g in monitoringObjects) {
					SendMessageToObject (g.transform, m.name, m.sendToMonitor);
				}
			}

			//	send messages to triggering game object, if required
			if (!m.sendToOther.Equals(TriggerMessage.SendType.NONE)) {
				SendMessageToObject (triggeringObject, m.name, m.sendToOther);
			}
		}
	}

	protected virtual void SendMessageToObject (Transform messageTarget, string message, TriggerMessage.SendType mode) {
		switch (mode) {
		case TriggerMessage.SendType.SEND:
			messageTarget.SendMessage(message, SendMessageOptions.DontRequireReceiver);
			break;
			
		case TriggerMessage.SendType.UP:
			messageTarget.SendMessageUpwards(message, SendMessageOptions.DontRequireReceiver);
			break;
			
		case TriggerMessage.SendType.DOWN:
			messageTarget.BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
			break;
			
		case TriggerMessage.SendType.UP_AND_DOWN:
			Transform p = messageTarget.parent;
			if (p) p.SendMessageUpwards(message, SendMessageOptions.DontRequireReceiver);
			messageTarget.BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
			break;
		}
	}
}

[System.Serializable]
public class TriggerMessage {
	public	string			name;
	public	enum			TriggerEvent { ENTER, EXIT, STAY }
	public	TriggerEvent	triggerEvent;
	public	bool			sendToBehaviors	=	true;
	public	enum 			SendType { NONE, SEND, UP, DOWN, UP_AND_DOWN }
	public	SendType		sendToSelf = SendType.SEND;
	public	SendType		sendToOther = SendType.NONE;
	public	SendType		sendToMonitor = SendType.SEND;
}
