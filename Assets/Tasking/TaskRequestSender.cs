using UnityEngine;
using System.Collections;

public class TaskRequestSender : MonoBehaviour {

	public	GameObject	defaultActor;
	public	Task		_task;

	//	listener
	
	public static string SEND_TASK_TO = "SendTaskTo";
	public void SendTaskTo (GameObject actor) {
		actor.SendMessage(ASSIGN_TASK, _task, SendMessageOptions.DontRequireReceiver);
	}

	//	listener
	
	public static string SEND_TASK_TO_DEFAULT = "SendTaskToDefaultActor";
	public void SendTaskToDefaultActor () {
		SendTaskTo(defaultActor);
	}

	public static string ASSIGN_TASK = "OnTaskReceived";
	public interface ITaskListener {
		void OnTaskReceived (Task task);
	}

	[System.Serializable]
	public class Task {
		public string Message;
		public GameObject Recipient;
	}
}
