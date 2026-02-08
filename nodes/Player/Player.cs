using Godot;
using System;
using System.Diagnostics;

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
	[Export]
	public int MaxHealth { get; set; } = 3;
	[Export]
	public bool IsInvincible { get; set; } = false;
	[Export]
	public float IFrameDuration { get; set; } = 2f;
	[Export]
	public float AirLevel { get; set; } = 50f;
	[Export]
	public float AirDepletionRate { get; set; } = 2f;
	[Export] 
	public float AirReplenishRate { get; set; } = 30f;
	[Export]
	public float MaxAir { get; set; } = 50f;
	[Export]
	public float WaterLineOffset { get; set; } = 200f;
	[Export]
	public float BreathableOffset { get; set; } = 25f;
	public float _waterLineY => -(GetViewportRect().Size.Y - WaterLineOffset);
	public GameManager _gameManager;
	private CollisionShape2D _collisionShape;
	private Area2D _area2D;
	private PlayerHud _hud;
	public Stopwatch _scoreStopwatch = new Stopwatch();
	private Timer _iFrameTimer;
	public int Score => (int)(_scoreStopwatch.Elapsed.TotalSeconds / 3);
	private const string HUD_PATH = "res://nodes/Player/PlayerHud/PlayerHud.tscn";

	#region Lifecycle

	public override void _Ready()
	{
		base._Ready();
		_gameManager = GetParent<GameManager>();
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		_area2D = GetNode<Area2D>("Area2D");
		_area2D.BodyEntered += BodyEntered;
		_scoreStopwatch.Start();
		Position = new Vector2(20, 0);
		_iFrameTimer = new Timer();
		AddChild(_iFrameTimer);
		_iFrameTimer.OneShot = true;
		_iFrameTimer.WaitTime = IFrameDuration;
		_iFrameTimer.Timeout += () => IFrameOver();
		LoadHud();
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsActionJustPressed("ui_cancel"))
		{
			HandlePauseInput();
		}

		if (IsPaused) return;

		if (Input.IsActionJustPressed("kill"))
		{
			KillPlayer();
		}
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

		if (Input.IsActionPressed("ui_accept") && Position.Y >= _waterLineY)
		{
			v.Y += -JumpPower;
		}

		Velocity = v;
		MoveAndSlide();

		// Air management
		bool isBreathable = Position.Y < _waterLineY + BreathableOffset;
		if (isBreathable)
		{
			AirLevel = Mathf.Min(AirLevel + AirReplenishRate * (float)delta, MaxAir);
		}
		else
		{
			AirLevel -= AirDepletionRate * (float)delta;
			if (AirLevel <= 0)
			{
				AirLevel = 0;
				TakeDamage();
			}
		}
	}

	private void BodyEntered(Node2D body)
	{
		if (body.IsInGroup("Obstacles"))
		{
			Obstacle obstacle = body as Obstacle;
			if (IsInvincible && !obstacle.BypassIFrame) return;

			_gameManager._obstacleManager.ActObstacle(obstacle);
		}
	}

	public void TakeDamage()
	{
		if (IsInvincible) return;
		Health--;
		if (Health <= 0)
		{
			KillPlayer();
		}
		else
		{
			IsInvincible = true;
			_iFrameTimer.Start();
			Modulate = new Color(1, 1, 1, 0.5f); 
		}
	}

	private void IFrameOver()
	{
		IsInvincible = false;
		Modulate = new Color(1, 1, 1, 1); 

	}

	public void KillPlayer()
	{
		_gameManager.EndGame();
	}

	#endregion

	#region Loading/Unloading

	private void LoadHud()
	{
		var hudScene = GD.Load<PackedScene>(HUD_PATH);
		_hud = hudScene.Instantiate<PlayerHud>();
		
		CanvasLayer canvasLayer = new CanvasLayer();
		AddChild(canvasLayer);
		canvasLayer.AddChild(_hud);
	}

	#endregion

}
