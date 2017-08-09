using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PLib;
using PLib.Logging;

public class LevelManager : BaseBehaviour, GameStateMonitor.IGameStateMonitorable {
	
	public	Transform[]	allTargets;
	public	List<Transform> levelEventListeners;

	private LevelStateMonitor.LevelState _levelState, _prevState;

	protected override void Awake () {
		SetLevelState(LevelStateMonitor.LevelState.None);
	}

	protected override void Update () {
		if (_levelState.Equals(LevelStateMonitor.LevelState.None)) return;
		base.Update();

		switch (_levelState) {
		case LevelStateMonitor.LevelState.StartLevel:
			OnLevelStart();
			break;
		case LevelStateMonitor.LevelState.ResetLevel:
			OnLevelReset();
			break;
		case LevelStateMonitor.LevelState.RunLevel:
			OnLevelRunning();
			break;
		case LevelStateMonitor.LevelState.ExitLevel:
			OnLevelExit();
			break;
		}
	}

	//	Level State Helpers	//

	/// <summary>
	/// Used when the player starts playing the level 
	/// </summary>
	private void OnLevelStart () {
		LogMethodEntry(this.MethodName());

		//	Do stuff...
		//	example, start invoked methods

		SetLevelState(LevelStateMonitor.LevelState.ResetLevel);
	}
	
	/// <summary>
	/// Used to re-initialize a level when a player restarts a level
	/// </summary>
	private void OnLevelReset () {
		LogMethodEntry(this.MethodName());
		
		//	Do stuff...
		//	example, clear lists, reset bonuses
		
		
		//	reset all items in the level that listen for the 'level reset' event
		UpdateLevelListeners();

		//	move to the next state
		SetLevelState(LevelStateMonitor.LevelState.RunLevel);
	}

	/// <summary>
	/// Called every Update as the game plays
	/// </summary>
	private void OnLevelRunning () {
		LogMethodEntry(this.MethodName());

		//	GAME JUST ENDED
		//	Inform GameManager that the game just ended
		if (false) {
			GameManager.Instance.EndGame();
			return;
		}

		//	GAME IS OVER, BUT DIDN'T *JUST* END
		//	TODO: DISPLAY 'GAME OVER' UI (showing score, etc)

		//	GAME IS RUNNING
		//	TODO: UPDATE GLOBAL LISTS, BONUSES, ETC

		//	PLAYER INPUT INDICATES 'RETURN TO TITLE SCREEN'
		if (false) {
			GameManager.Instance.Title();
		}
	}

	/// <summary>
	/// Used to cleanup when a level is unloaded
	/// </summary>
	private void OnLevelExit () {
		LogMethodEntry(this.MethodName());

		//	Do stuff...
		//	example, delete temporary objects, garbage cleanup

		SetLevelState(LevelStateMonitor.LevelState.None);
	}

	//	Helpers	//

	/// <summary>
	/// Determines whether player's game has ended.
	/// </summary>
	/// <returns><c>true</c> if the game is over; otherwise, <c>false</c>.</returns>
	private bool IsGameOver() {
		return false;
	}

	private void SetLevelState (LevelStateMonitor.LevelState state) {
		LogMethodEntry(this.MethodName() + "(" + state.ToString() + ")");
		_prevState = _levelState;
		_levelState = state;
	}

	private void UpdateLevelListeners () {
		LogMethodEntry(this.MethodName());
		foreach (Transform t in levelEventListeners) {
			t.BroadcastMessage(LevelStateMonitor.LEVEL_STATE_UPDATE, _levelState);
		}
	}

	//	INTERFACES	//

	public void OnGameStateUpdate (GameStateMonitor.GameState state)
	{
		LogMethodEntry(this.MethodName() + "(" + state.ToString() + ")");
		switch (state) {
		case GameStateMonitor.GameState.GameStart:
			SetLevelState(LevelStateMonitor.LevelState.StartLevel);
			break;
		case GameStateMonitor.GameState.Title:
			SetLevelState(LevelStateMonitor.LevelState.ExitLevel);
			break;
		}
	}
}

//////////////////////
//	Interfaces		//
//////////////////////

#region Interfaces

public static class LevelStateMonitor {
	
	public enum LevelState { StartLevel, ResetLevel, RunLevel, ExitLevel, None }
	
	public const string LEVEL_STATE_UPDATE = "OnLevelStateUpdate";
	
	public interface ILevelStateMonitorable {
		void OnLevelStateUpdate (LevelState newState);
	}
}

#endregion