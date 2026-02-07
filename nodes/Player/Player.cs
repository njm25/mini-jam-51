using Godot;
using System;

public partial class Player : CharacterBody2D
{

	[Export]
	public bool IsPaused = false;

	[Export]
	public float JumpPower { get; set; } = 30f;

	[Export]
	public float Gravity { get; set; } = 1200f;

	[Export]
	public int Health { get; set; } = 3;
	private GameManager _gameManager;
	private CollisionShape2D _collisionShape;
	private Area2D _area2D;

	#region Lifecycle

	public override void _Ready()
	{
		base._Ready();
		_gameManager = GetParent<GameManager>();
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		_area2D = GetNode<Area2D>("Area2D");
		_area2D.BodyEntered += BodyEntered;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		// to-do figure out how to do freeze now that were not using rigidybody2d
		// Freeze = IsPaused;

		if (Input.IsActionJustPressed("ui_cancel"))
		{
			HandlePauseInput();
		}

		if (Input.IsActionJustPressed("kill"))
		{
			KillPlayer();
		}

		if (IsPaused) return;
	}

	private void HandlePauseInput()
	{
		_gameManager.PauseGame();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (IsPaused) return;
		base._PhysicsProcess(delta);

		Vector2 v = Velocity;
		v.X = 0;
		v.Y += Gravity * (float)delta;

		if (Input.IsActionPressed("ui_accept"))
		{
			v.Y += -JumpPower;
		}

		Velocity = v;
		MoveAndSlide();
	}

	private void BodyEntered(Node2D body)
	{
		if (body.IsInGroup("Obstacles"))
		{
			Obstacle obstacle = body as Obstacle;
			_gameManager._obstacleManager.RemoveObstacle(obstacle);
			Health--;
			if (Health <= 0)
			{
				KillPlayer();
			}
		}
	}

	public void KillPlayer()
	{
		_gameManager.EndGame();
	}


	#endregion

}
