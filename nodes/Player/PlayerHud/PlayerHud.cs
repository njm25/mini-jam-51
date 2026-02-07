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
		+ "\nScore: " + _player.Score.ToString();
	}
   
	
}
