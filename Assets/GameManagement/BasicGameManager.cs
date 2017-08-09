using UnityEngine;
using System.Collections;

public class BasicGameManager : MonoBehaviour {

	public	bool				anyClickStart	=	true;
	public	bool				anyClickTitle	=	true;

	public	ActionOnMessage[]	actions;
	public	enum GameEventMessage { Title, StartGame, GameOver }

	public static string RESET_GAME = "OnGameReset";
	public interface IGameResetListener {
		void OnGameReset ();
	}

	public static string START_GAME = "OnGameStart";
	public interface IGameStartListener {
		void OnGameStart ();
	}

	public static string END_GAME = "OnGameOver";
	public interface IGameOverListener {
		void OnGameOver ();
	}

	private	GameEventMessage	lastMessage;

	//	message listener

	public void OnGameOver (bool isOver) {
		SendGameEventMessage(GameEventMessage.GameOver);
	}
	
	void Start () {
		SendGameEventMessage (GameEventMessage.Title);
	}

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			if (anyClickStart) StartButtonPressed();
			if (anyClickTitle) TitleButtonPressed();
		}
	}

	//	listener for a "start button"
	public void StartButtonPressed () {
		if (lastMessage.Equals(GameEventMessage.Title)) {
			SendGameEventMessage (GameEventMessage.StartGame);
			return;
		}
	}

	//	listener for a "quit button"
	public void TitleButtonPressed () {
		if (lastMessage.Equals(GameEventMessage.GameOver)) {
			SendGameEventMessage (GameEventMessage.Title);
			return;
		}
	}

	private void SendGameEventMessage (GameEventMessage message) {
		lastMessage = message;
		foreach (ActionOnMessage action in actions) {
			if (action.OnMessage.Equals(message)) action.Execute();
		}
	}

	[System.Serializable]
	public class ActionOnMessage {
		public GameEventMessage OnMessage;

		public GameObject[]		activateGameObjects;
		public GameObject[]		deactivateGameObjects;

		public	string[]		customMessages;
		public	MonoBehaviour[]	listeners;

		public void Execute () {
			foreach (GameObject g in deactivateGameObjects) g.SetActive(false);
			foreach (GameObject g in activateGameObjects) g.SetActive(true);
						
			foreach (MonoBehaviour b in listeners) {
				foreach (string message in customMessages) b.SendMessage(message, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

}
