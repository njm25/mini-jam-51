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
	[Export]
	public float SpeakerCharge { get; set; } = 100;
	[Export]
	public float MaxSpeakerCharge { get; set; } = 100;
	[Export]
	public float SpeakerChargeDepletionRate { get; set; } = 20f;
	public bool IsUsingSpeaker => Input.IsActionPressed("use") && SpeakerCharge > 0;
	public float _waterLineY => -(GetViewportRect().Size.Y - WaterLineOffset);
	public GameManager _gameManager;
	private CollisionShape2D _collisionShape;
	private Area2D _area2D;
	private PlayerHud _hud;
	public Stopwatch _scoreStopwatch = new Stopwatch();
	private Timer _iFrameTimer;
	private AnimatedSprite2D _swimmingSprite;
	private AnimatedSprite2D _iFrameSprite;
	public int Score => (int)(_scoreStopwatch.Elapsed.TotalSeconds / 3);
	private const string HUD_PATH = "res://nodes/Player/PlayerHud/PlayerHud.tscn";

	#region Lifecycle

	public override void _Ready()
	{
		base._Ready();
		_gameManager = GetParent<GameManager>();
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		_swimmingSprite = GetNode<AnimatedSprite2D>("SwimmingSprite");
		_swimmingSprite.Play();
		_iFrameSprite = GetNode<AnimatedSprite2D>("IFrameSprite");
		_iFrameSprite.Play();
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
			KillPlayer(DamageType.Suicide);
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

		// battery usage
		if (Input.IsActionPressed("use") && SpeakerCharge > 0)
		{
			SpeakerCharge -= SpeakerChargeDepletionRate * (float)delta;
			if (SpeakerCharge < 0)
			{
				SpeakerCharge = 0;
			}
		}

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
				TakeDamage(DamageType.AirLevel);
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

	public void TakeDamage(DamageType damageType)
	{
		if (IsInvincible) return;
		Health--;
		if (Health <= 0)
		{
			KillPlayer(damageType);
		}
		else
		{
			SpawnDamageLabel();
			IsInvincible = true;
			_iFrameTimer.Start();
			Modulate = new Color(1, 1, 1, 0.5f); 
			_iFrameSprite.Visible = true;
			_swimmingSprite.Visible = false;
		}
	}

	private void SpawnDamageLabel()
	{
		Label damageLabel = new Label();
		damageLabel.Text = "-1 Health";
		damageLabel.AddThemeColorOverride("font_color", Colors.Black);
		var font = GD.Load<Font>("res://assets/mspain.ttf");
		if (font != null)
			damageLabel.AddThemeFontOverride("font", font);
		damageLabel.Position = new Vector2(Position.X + 20, Position.Y - 20);
		GetParent().AddChild(damageLabel);

		Timer labelTimer = new Timer();
		labelTimer.WaitTime = 2.0f;
		labelTimer.OneShot = true;
		labelTimer.Timeout += () => damageLabel.QueueFree();
		damageLabel.AddChild(labelTimer);
		labelTimer.Start();
	}

	private void IFrameOver()
	{
		IsInvincible = false;
		Modulate = new Color(1, 1, 1, 1); 
		_iFrameSprite.Visible = false;
		_swimmingSprite.Visible = true;
	}

	public void KillPlayer(DamageType damageType)
	{
		string reason = damageType switch
		{
			DamageType.Obstacle => "Killed by Obstacle",
			DamageType.AirLevel => "Drowned",
			DamageType.Suicide => "Committed Suicide",
			_ => throw new NotImplementedException(),
		};
		
		_gameManager.EndGame(reason);
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

public enum DamageType
{
	Obstacle,
	AirLevel,
	Suicide
}
