using Godot;
using System;

public partial class Obstacle : RigidBody2D
{
	[Export]
	public bool IsPaused = false;
	[Export]
	public float Speed { get; set; } = 200;
	[Export]
	public Vector2 SpawnPosition { get; set; } = new Vector2(800, 0);
	private CollisionShape2D _collisionShape;

	public override void _Ready()
	{
		base._Ready();
		Position = SpawnPosition;
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		GravityScale = 0; 
		AddToGroup("Obstacles");
		GD.Print("Obstacle ready at position: " + Position);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (IsPaused) return;
		ApplySpeed(delta);
	}

	private void ApplySpeed(double delta)
	{
		Vector2 velocity = new Vector2(-Speed, 0);
		LinearVelocity = velocity;
	}

	public void Pause()
	{
		Freeze = !Freeze;
		IsPaused = Freeze;
	}


}
