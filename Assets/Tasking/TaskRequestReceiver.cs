using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PLib;

public class TaskRequestReceiver : MonoBehaviour, TaskRequestSender.ITaskListener {

	[Tooltip("Number of messages to keep in queue")]
	public	int			maxQueue		=	1;

	[Tooltip("Fire the Task Message at this proximity to the Task Destination")]
	public	float		executeDistance	=	1;	//	meters

	private	Transform	_transform;
	private	GameObject	_recipient;
	private	Vector3		_destination;
	private	string		_message;
	private	bool		_sendPending;
	
	private	List<TaskRequestSender.Task>	_queue;

	//	LIFECYCLE

	void Start () {
		_queue = new List<TaskRequestSender.Task>();
		_transform = transform;
		_destination = _transform.position;
		_sendPending = false;
	}

	void Update () {
		if (!_sendPending) {
			NextTask();
			return;
		}
		SendMessage(Directable.MOVE_TO, _destination, SendMessageOptions.DontRequireReceiver);
		if (Vector3.SqrMagnitude(_destination - _transform.position) > executeDistance * executeDistance) return;

		_recipient.SendMessage(_message, SendMessageOptions.DontRequireReceiver);
		SendMessage(Directable.MOVE_TO, _transform.position + _transform.forward * 0.1f * executeDistance, SendMessageOptions.DontRequireReceiver);
		_sendPending = false;
	}

	//	INTERFACE	

	public void OnTaskReceived (TaskRequestSender.Task task)
	{
		if (_queue.Count == maxQueue) return;
		_queue.Add(task);
	}

	public void StartGame () {
		AbortTasks();
	}

	public static string ABORT_TASK = "AbortTasks";
	public void AbortTasks () {
		_queue.Clear();
		_destination = _transform.position;
		_sendPending = false;
	}

	//	HELPER

	private void NextTask () {
		if (_queue.Count == 0) return;
		SetCurrentTask(_queue[0]);
		_queue.RemoveAt(0);
	}

	private void SetCurrentTask (TaskRequestSender.Task task) {
		_recipient = task.Recipient;
		_destination = _recipient.transform.position;
		_message = task.Message;
		_sendPending = true;
	}
}
