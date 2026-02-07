using Godot;
using System;

public partial class StartMenu : Control
{
	private GameManager _gameManager;
	private Button _startButton;
	public override void _Ready()
	{
		base._Ready();
		_gameManager = GetParent<CanvasLayer>().GetParent<GameManager>();
		VBoxContainer container = GetNode<VBoxContainer>("VBoxContainer");
		_startButton = container.GetNode<Button>("StartButton");
		_startButton.Pressed += OnStartButtonPressed;
		Button quitButton = container.GetNode<Button>("QuitButton");
		quitButton.Pressed += OnQuitButtonPressed;
	}

	private void OnStartButtonPressed()
	{
		_gameManager.StartGame();
	}
	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}


}
