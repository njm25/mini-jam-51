using Godot;
using System;

public partial class RetryMenu : Control
{
	private GameManager _gameManager;
	public Label _deathReason;
	private Label _scoreLabel;
	private Label _highScoreLabel;

	public override void _Ready()
	{
		base._Ready();
		_gameManager = GetParent<CanvasLayer>().GetParent<MenuManager>().GetParent<GameManager>();
		VBoxContainer container = GetNode<VBoxContainer>("VBoxContainer");
		Button retryButton = container.GetNode<Button>("RetryButton");
		retryButton.Pressed += OnRetryButtonPressed;
		Button quitButton = container.GetNode<Button>("QuitButton");
		quitButton.Pressed += OnQuitButtonPressed;
		_deathReason = GetNode<Label>("DeathReason");
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_highScoreLabel = GetNode<Label>("HighScoreLabel");
	}

	public void ShowScore(int score, int highScore, bool isNewHighScore)
	{
		_scoreLabel.Text = $"Score: {score}";
		_highScoreLabel.Text = isNewHighScore
			? $"New High Score: {highScore}!"
			: $"High Score: {highScore}";
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
