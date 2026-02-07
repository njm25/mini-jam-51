using Godot;
using System;

public partial class GameManager : Node2D
{
	public GameState GameState { get; set; }

	private Main _main;
	private PauseMenu _pauseMenu;
	private StartMenu _startMenu;
	private RetryMenu _retryMenu;
	private Player _player;
	private Camera _camera;
	private CanvasLayer _canvasLayer;

	const string START_MENU_PATH = "res://nodes/menus/StartMenu/StartMenu.tscn";
	const string RETRY_MENU_PATH = "res://nodes/menus/RetryMenu/RetryMenu.tscn";
	const string PAUSE_MENU_PATH = "res://nodes/menus/PauseMenu/PauseMenu.tscn";
	const string PLAYER_PATH = "res://nodes/Player/Player.tscn";
	const string CAMERA_PATH = "res://nodes/Camera/Camera.tscn";

	public override void _Ready()
	{
		base._Ready();
		_main = GetParent<Main>();
		Initialize();
	}

	private void Initialize()
	{
		CanvasLayer canvasLayer = new CanvasLayer();
		AddChild(canvasLayer);
		_canvasLayer = canvasLayer;

		var cameraScene = GD.Load<PackedScene>(CAMERA_PATH);
		_camera = cameraScene.Instantiate<Camera>();
		_canvasLayer.AddChild(_camera);
		
		var startMenuScene = GD.Load<PackedScene>(START_MENU_PATH);
		_startMenu = startMenuScene.Instantiate<StartMenu>();
		_startMenu.Position = _camera.Offset;
		_canvasLayer.AddChild(_startMenu);

		var retryMenuScene = GD.Load<PackedScene>(RETRY_MENU_PATH);
		_retryMenu = retryMenuScene.Instantiate<RetryMenu>();
		_startMenu.Position = _camera.Offset;
		_retryMenu.Visible = false;
		_canvasLayer.AddChild(_retryMenu);

		var pauseMenuScene = GD.Load<PackedScene>(PAUSE_MENU_PATH);
		_pauseMenu = pauseMenuScene.Instantiate<PauseMenu>();
		_pauseMenu.Position = _camera.Offset;
		_pauseMenu.Visible = false;
		_canvasLayer.AddChild(_pauseMenu);

	}

	public void EndGame()
	{
		GameState = GameState.GameOver;
		_retryMenu.Visible = true;

		if (_player != null)
		{
			_player.QueueFree();
			_player = null;
		}
	}

	public void StartGame()
	{
		GameState = GameState.Playing;
		_retryMenu.Visible = false;
		_startMenu.Visible = false;
		
		var playerScene = GD.Load<PackedScene>(PLAYER_PATH);
		_player = playerScene.Instantiate<Player>();
		AddChild(_player);
		
	}

	public void PauseGame()
	{
		GameState = GameState == GameState.Paused 
			? GameState.Playing 
			: GameState.Paused;

		_pauseMenu.Visible = GameState == GameState.Paused;
		_player.IsPaused = GameState == GameState.Paused;
	}

}

public enum GameState
{
	Start,
	Playing,
	Paused,
	GameOver
}
