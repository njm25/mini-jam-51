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
	[Export]
	public bool BobUpAndDown { get; set; } = false;
	[Export]
	public float BobAmount { get; set; } = 10f;
	[Export]
	public string DestroyLabelText { get; set; }
	[Export]
	public float DestroyDb { get; set; } = 4f;
	private CollisionShape2D _collisionShape;
	public ObstacleManager _obstacleManager;
	private Timer _destroyTimer;
	private float _bobTime = 0f;
	public string DESTROY_SOUND_PATH = "res://assets/explosion.wav";

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
		_destroyTimer.Timeout += () => Destroy(playSound: false);
		AddChild(_destroyTimer);
		_destroyTimer.Start();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		if (IsPaused) return;
		ApplySpeed(delta);
		ClampToWaterLine();
	}

	private void ApplySpeed(double delta)
	{
		float bobVelocityY = 0f;
		if (BobUpAndDown)
		{
			_bobTime += (float)delta;
			bobVelocityY = Mathf.Sin(_bobTime * Mathf.Pi * 2f) * BobAmount;
		}

		Vector2 velocity = new Vector2(-Speed, bobVelocityY);
		LinearVelocity = velocity;
	}

	private void ClampToWaterLine()
	{
		float waterLineY = -(GetViewportRect().Size.Y - 200f);
		if (Position.Y < waterLineY)
		{
			Position = new Vector2(Position.X, waterLineY);
			if (LinearVelocity.Y < 0)
				LinearVelocity = new Vector2(LinearVelocity.X, 0);
		}
	}

	public void Pause()
	{
		Freeze = !Freeze;
		IsPaused = Freeze;
		_destroyTimer.Paused = IsPaused;
	}

	public void Act()
	{   
		Interact();
		Destroy(playSound: true);
	}

	public virtual void Interact()
	{
		if (DoesDamage)
		{
			Player player = _obstacleManager._gameManager._player;
			player.TakeDamage(DamageType.Obstacle);
		}
	}

	protected virtual void Destroy(bool playSound = true)
	{
		if (_obstacleManager._obstacles.Contains(this))
			_obstacleManager._obstacles.Remove(this);
		if (playSound && DESTROY_SOUND_PATH != "")
		{
			AudioStreamPlayer2D destroySoundPlayer = new AudioStreamPlayer2D();
			GetTree().Root.AddChild(destroySoundPlayer);
			destroySoundPlayer.Stream = GD.Load<AudioStream>(DESTROY_SOUND_PATH);
			destroySoundPlayer.GlobalPosition = GlobalPosition;
			destroySoundPlayer.VolumeDb = DestroyDb;
			destroySoundPlayer.Play();
			destroySoundPlayer.Finished += () => destroySoundPlayer.QueueFree();
		}
		QueueFree();
	}

}
