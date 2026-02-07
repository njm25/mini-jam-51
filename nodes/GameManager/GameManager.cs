using Godot;
using System;

public partial class GameManager : Node2D
{
	public GameState GameState;

	private Main _main;
	private Player _player;
	private Camera _camera;
	public MenuManager _menuManager;
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

	#endregion

	#region Game Logic
	public void EndGame()
	{
		GameState = GameState.GameOver;
		_menuManager.Close();
		_menuManager.Navigate(_menuManager._retryMenu);

		UnloadCamera();
		UnloadPlayer();
	}

	public void StartGame()
	{
		GameState = GameState.Playing;
		_menuManager.Close();
		
		LoadPlayer();
		LoadCamera();
	}

	public void PauseGame()
	{
		GameState = GameState == GameState.Paused 
			? GameState.Playing 
			: GameState.Paused;

		_player.IsPaused = GameState == GameState.Paused;

		if (GameState == GameState.Paused)
			_menuManager.Navigate(_menuManager._pauseMenu);
		else
			_menuManager.Close();
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
