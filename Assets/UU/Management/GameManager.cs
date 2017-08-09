/// <summary>
/// Game manager. This keeps track of the overall game state 
/// and sends data to the LevelManager.
/// </summary>
using UnityEngine;
using System.Collections;
using PLib;
using PLib.Logging;

public class GameManager : BaseBehaviour {

	[Tooltip("Inspector Assigned")]
	public	GameObject	UIManagerObject;

	private static GameManager instance;
	public static GameManager Instance {get;set;}

	private	static GameStateMonitor.GameState _gameState, _prevState;
	private LevelManager _levelManager;

	protected override void Awake () {
		if (!Instance) Instance = this;
		base.Awake();
		_levelManager = GetComponent<LevelManager>();
	}

	protected override void Start () {
		base.Start();
		Title ();
	}

	//	HELPERS	//

	
	protected void SetGameState (GameStateMonitor.GameState state) {
		LogMethodEntry(this.MethodName() + "(" + state.ToString() + ")");
		_prevState = _gameState;
		_gameState = state;

		//	update the UI screens
		if (UIManagerObject) UIManagerObject.BroadcastMessage(GameStateMonitor.GAME_STATE_UPDATE, state);

		//	update the LevelManager (attached to this game object)
		BroadcastMessage(GameStateMonitor.GAME_STATE_UPDATE, state);
	}

	/// <summary>
	/// Used by the title screen UI to start the game.
	/// </summary>
	public void StartGame () {
		LogMethodEntry(this.MethodName());
		SetGameState (GameStateMonitor.GameState.GameStart);
	}

	/// <summary>
	/// Used by the running game to notify the GameManager that the current game has ended.
	/// </summary>
	public void EndGame () {
		LogMethodEntry(this.MethodName());
		SetGameState (GameStateMonitor.GameState.GameOver);
	}

	/// <summary>
	/// Used by the "Game Over" UI to notify the GameManager to go to the title screen.
	/// </summary>
	public void Title () {
		LogMethodEntry(this.MethodName());
		SetGameState (GameStateMonitor.GameState.Title);
	}
}

//////////////////////
//	Interfaces		//
//////////////////////

#region Interfaces

public static class GameStateMonitor {
	
	public enum GameState { Title, GameStart, GameRunning, GameOver, All, None }
	
	public const string GAME_STATE_UPDATE = "OnGameStateUpdate";
	
	public interface IGameStateMonitorable {
		void OnGameStateUpdate (GameState newState);
	}
}

#endregion
