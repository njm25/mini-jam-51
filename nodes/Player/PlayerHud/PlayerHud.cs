using Godot;
using System;

public partial class PlayerHud : Node
{
	public Player _player;
	public Label _label;
	public override void _Ready()
	{
		base._Ready();
		_player = GetParent<CanvasLayer>().GetParent<Player>();
		_label = GetNode<Label>("HealthLabel");
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		_label.Text = "Lives left: " + _player.Health.ToString()  + "/" + _player.MaxHealth.ToString()
		+ "\nScore: " + _player.Score.ToString() 
		+"\nObstacle Mode: " + _player._gameManager._obstacleManager._currentMode.ToString();

		if(_player._gameManager._obstacleManager._currentMode == SpawnMode.Confine)
		{
			_label.Text += "\nConfine Mode: " + _player._gameManager._obstacleManager._currentConfineMode.ToString();
		}
		if  (_player._gameManager._obstacleManager._currentMode == SpawnMode.Pattern)
		{
			_label.Text += "\nPattern Mode: " + _player._gameManager._obstacleManager._currentPatternMode.ToString();
		}
	}
   
	
}
