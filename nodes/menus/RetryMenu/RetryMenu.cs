using Godot;
using System;

public partial class RetryMenu : Control
{
	private GameManager _gameManager;
	public override void _Ready()
	{
		base._Ready();
		_gameManager = GetParent<CanvasLayer>().GetParent<GameManager>();
		VBoxContainer container = GetNode<VBoxContainer>("VBoxContainer");
		Button retryButton = container.GetNode<Button>("RetryButton");
		retryButton.Pressed += OnRetryButtonPressed;
		Button quitButton = container.GetNode<Button>("QuitButton");
		quitButton.Pressed += OnQuitButtonPressed;
	}
	private void OnRetryButtonPressed()
	{
		_gameManager.StartGame();
	}
	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
