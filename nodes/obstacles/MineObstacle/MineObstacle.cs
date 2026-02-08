using Godot;
using System;

public partial class MineObstacle : Obstacle
{
	private Area2D _pushRange;
	private bool _isPlayerInRange = false;
	private Player player => _obstacleManager._gameManager._player;
	public override void _Ready()
	{
		base._Ready();
		_pushRange = GetNode<Area2D>("SpeakerPushRange");
		_pushRange.BodyEntered += OnBodyEntered;
		_pushRange.BodyExited += OnBodyExited;
		
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			_isPlayerInRange = true;
		}
	}
	private void OnBodyExited(Node2D body)
	{
		if (body is Player)
		{
			_isPlayerInRange = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (player == null) return;
		if (!player.IsUsingSpeaker || !_isPlayerInRange) return; 
		Vector2 direction = (Position - player.Position).Normalized();
		Position += direction * 200 * (float)delta;
	
	}



}
