using Godot;
using System;

public partial class GameManager : Node2D
{
	public GameState GameState;

	private Main _main;
	public Player _player;
	private Camera _camera;
	public MenuManager _menuManager;
	public ObstacleManager _obstacleManager;
	private WorldLines _worldLines;
	private WaterLine _waterLine;
	const string PLAYER_PATH = "res://nodes/Player/Player.tscn";
	const string CAMERA_PATH = "res://nodes/Camera/Camera.tscn";

	#region Lifecycle
	public override void _Ready()
	{
		base._Ready();
		_main = GetParent<Main>();
		LoadMenuManager();
		_menuManager.Navigate(_menuManager._startMenu);
	}
	#endregion

	#region Loading/Unloading
	
	private void LoadMenuManager()
	{
		MenuManager menuManager = new MenuManager();
		_menuManager = menuManager;
		AddChild(_menuManager);
	}

	
	private void LoadCamera()
	{
		var cameraScene = GD.Load<PackedScene>(CAMERA_PATH);
		_camera = cameraScene.Instantiate<Camera>();
		AddChild(_camera);
	}

	private void UnloadCamera()
	{
		if (_camera != null)
		{
			_camera.QueueFree();
			_camera = null;
		}
	}

	private void LoadPlayer()
	{
		var playerScene = GD.Load<PackedScene>(PLAYER_PATH);
		_player = playerScene.Instantiate<Player>();
		AddChild(_player);
	}

	private void UnloadPlayer()
	{
		if (_player != null)
		{
			_player.QueueFree();
			_player = null;
		}
	}

	private void LoadObstacleManager()
	{
		ObstacleManager obstacleManager = new ObstacleManager();
		_obstacleManager = obstacleManager;
		AddChild(_obstacleManager);
	}

	private void UnloadObstacleManager()
	{
		if (_obstacleManager != null)
		{
			_obstacleManager.QueueFree();
			_obstacleManager = null;
		}
	}

	private void LoadWorldLines()
	{
		_worldLines = new WorldLines();
		_worldLines.ZIndex = -10;
		_worldLines._player = _player;
		AddChild(_worldLines);
	}

	private void UnloadWorldLines()
	{
		if (_worldLines != null)
		{
			_worldLines.QueueFree();
			_worldLines = null;
		}
	}

	private void LoadWaterLine()
	{
		_waterLine = new WaterLine();
		_waterLine.ZIndex = 10;
		_waterLine._player = _player;
		AddChild(_waterLine);
	}

	private void UnloadWaterLine()
	{
		if (_waterLine != null)
		{
			_waterLine.QueueFree();
			_waterLine = null;
		}
	}

	#endregion

	#region Game Logic
	public void EndGame(string reason = "Game Over")
	{
		GameState = GameState.GameOver;

		// Extract score before the player is freed
		int finalScore = _player?.Score ?? 0;
		bool isNewHighScore = HighScoreManager.TrySetHighScore(finalScore);

		_menuManager.Close();
		_menuManager._retryMenu._deathReason.Text = reason;
		_menuManager._retryMenu.ShowScore(finalScore, HighScoreManager.HighScore, isNewHighScore);
		_menuManager.Navigate(_menuManager._retryMenu);

		UnloadCamera();
		UnloadPlayer();
		UnloadWorldLines();
		UnloadWaterLine();
		UnloadObstacleManager();
	}

	public void StartGame()
	{
		GameState = GameState.Playing;
		_menuManager.Close();
		
		LoadPlayer();
		LoadCamera();
		LoadWorldLines();
		LoadWaterLine();
		LoadObstacleManager();
	}

	public void PauseGame()
	{
		GameState = GameState == GameState.Paused 
			? GameState.Playing 
			: GameState.Paused;

		_player.IsPaused = GameState == GameState.Paused;
		_obstacleManager.Pause();

		if (GameState == GameState.Paused)
		{
			_menuManager.Navigate(_menuManager._pauseMenu);
			_player._scoreStopwatch.Stop();
			_player.SetMusicMuted(true);
		}
		else
		{
			_menuManager.Close();
			_player._scoreStopwatch.Start();
			_player.SetMusicMuted(false);
		}
			
	}

	#endregion

}

public enum GameState
{
	Start,
	Playing,
	Paused,
	GameOver
}
