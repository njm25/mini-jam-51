using Godot;
using System;

public partial class PauseMenu : Control
{
	public override void _Ready()
	{
		base._Ready();
		VBoxContainer container = GetNode<VBoxContainer>("VBoxContainer");
		Button resumeButton = container.GetNode<Button>("ResumeButton");    
		resumeButton.Pressed += OnResumeButtonPressed;
		Button quitButton = container.GetNode<Button>("QuitButton");    
		quitButton.Pressed += QuitGame;
		
	}
	private void OnResumeButtonPressed()
	{
		var _gameManager = GetParent<CanvasLayer>().GetParent<GameManager>();
		_gameManager.PauseGame();
	}

	private void QuitGame()
	{
		GetTree().Quit();
	}

}
