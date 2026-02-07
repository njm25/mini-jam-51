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
	private CollisionShape2D _collisionShape;
	public ObstacleManager _obstacleManager;
	private Timer _destroyTimer;

	public override void _Ready()
	{
		base._Ready();
		_obstacleManager = GetParent<ObstacleManager>();
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		GravityScale = 0; 
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

	public virtual void Interact()
	{
		if (DoesDamage)
		{
			Player player = _obstacleManager._gameManager._player;
			player.Health--;
			if (player.Health <= 0)
			{
				player.KillPlayer();
			}
		}
	}

	public void Act()
	{   
		Interact();
		Destroy();
	}

	private void Destroy()
	{
		QueueFree();
		GD.Print("Destroyed obstacle");
	}

}
