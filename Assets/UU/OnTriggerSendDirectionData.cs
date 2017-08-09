using UnityEngine;
using System.Collections;

public class OnTriggerSendDirectionData : OnTriggerSendMessages {

	Vector3 direction;

	protected override bool ValidTriggerObject (Transform other) {
		if (base.ValidTriggerObject(other)) {
			direction = other.transform.position - transform.position;
			direction.Normalize();	
			return true;
		}
		else {
			return false;
		}
	}

	protected override void SendMessages (TriggerMessage.TriggerEvent triggerEvent) {
		foreach (TriggerMessage m in triggerMessages) {
			if (!m.triggerEvent.Equals(triggerEvent)) continue;

			//	send messages to listed behaviors, if required
			if (m.sendToBehaviors) {
				foreach (MonoBehaviour b in behaviors) b.SendMessage(m.name, direction);
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

	protected override void SendMessageToObject (Transform messageTarget, string message, TriggerMessage.SendType mode) {
		switch (mode) {
		case TriggerMessage.SendType.SEND:
			messageTarget.SendMessage(message, direction, SendMessageOptions.DontRequireReceiver);
			break;
			
		case TriggerMessage.SendType.UP:
			messageTarget.SendMessageUpwards(message, direction, SendMessageOptions.DontRequireReceiver);
			break;
			
		case TriggerMessage.SendType.DOWN:
			messageTarget.BroadcastMessage(message, direction, SendMessageOptions.DontRequireReceiver);
			break;
			
		case TriggerMessage.SendType.UP_AND_DOWN:
			Transform p = messageTarget.parent;
			if (p) p.SendMessageUpwards(message, direction, SendMessageOptions.DontRequireReceiver);
			messageTarget.BroadcastMessage(message, direction, SendMessageOptions.DontRequireReceiver);
			break;
		}
	}
}
