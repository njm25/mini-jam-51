using Godot;
using System;

public partial class Obstacle : RigidBody2D
{
	[Export]
	public bool IsPaused = false;
	[Export]
	public float Speed { get; set; } = 200;
	[Export]
	public bool DoesDamage { get; set; } = true;
	[Export]
	public bool BypassIFrame { get; set; } = false;
	private CollisionShape2D _collisionShape;
	public ObstacleManager _obstacleManager;
	private Timer _destroyTimer;

	public override void _Ready()
	{
		base._Ready();
		_obstacleManager = GetParent<ObstacleManager>();
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		GravityScale = 0;
		// Scale speed based on how long the game has been running
		Speed *= _obstacleManager.GetSpeedMultiplier();
		AddToGroup("Obstacles");
		_destroyTimer = new Timer();
		_destroyTimer.WaitTime = 10f;
		_destroyTimer.OneShot = true;
		_destroyTimer.Timeout += Destroy;
		AddChild(_destroyTimer);
		_destroyTimer.Start();
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

	public void Act()
	{   
		Interact();
		Destroy();
	}

	public virtual void Interact()
	{
		if (DoesDamage)
		{
			Player player = _obstacleManager._gameManager._player;
			player.TakeDamage();
		}
	}

	private void Destroy()
	{
		QueueFree();
		if (_obstacleManager._obstacles.Contains(this))
			_obstacleManager._obstacles.Remove(this);

		GD.Print("Destroyed obstacle");
	}

}
