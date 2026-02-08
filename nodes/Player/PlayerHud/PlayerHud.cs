using Godot;
using System;

public partial class PlayerHud : Node
{
	public Player _player;
	public Label _label;
	private ColorRect _waterLine;
	private ColorRect _zeroLine;

	public override void _Ready()
	{
		base._Ready();
		_player = GetParent<CanvasLayer>().GetParent<Player>();
		_label = GetNode<Label>("HealthLabel");

		_waterLine = new ColorRect();
		_waterLine.Color = new Color(0, 0.5f, 1, 0.5f);
		_waterLine.Size = new Vector2(GetViewport().GetVisibleRect().Size.X, 2);
		AddChild(_waterLine);

		_zeroLine = new ColorRect();
		_zeroLine.Color = new Color(1, 0, 0, 0.5f);
		_zeroLine.Size = new Vector2(GetViewport().GetVisibleRect().Size.X, 2);
		AddChild(_zeroLine);
	}
 
	public override void _Process(double delta)
	{
		base._Process(delta);

		// Update water line screen position
		Vector2 waterWorldPos = new Vector2(0, _player._waterLineY);
		Vector2 screenPos = _player.GetCanvasTransform() * waterWorldPos;
		_waterLine.Position = new Vector2(0, screenPos.Y);
		_waterLine.Size = new Vector2(GetViewport().GetVisibleRect().Size.X, 2);

		// Update Y=0 line screen position
		Vector2 zeroWorldPos = new Vector2(0, 0);
		Vector2 zeroScreenPos = _player.GetCanvasTransform() * zeroWorldPos;
		_zeroLine.Position = new Vector2(0, zeroScreenPos.Y);
		_zeroLine.Size = new Vector2(GetViewport().GetVisibleRect().Size.X, 2);

		_label.Text = "Lives left: " + _player.Health.ToString()  + "/" + _player.MaxHealth.ToString()
		+ "\nScore: " + _player.Score.ToString() 
		+"\nAir level: " + _player.AirLevel.ToString() + "/" + _player.MaxAir.ToString()
		+"\nSpeaker Battery: " + _player.SpeakerCharge.ToString() + "/" + _player.MaxSpeakerCharge.ToString()
		+ "\nIs using speaker: " + _player.IsUsingSpeaker.ToString()
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
