using Godot;
using System;

public partial class Player : RigidBody2D
{

	[Export]
	public bool IsPaused = false;

	[Export]
	public int JumpPower { get; set; } = 30;

	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		Freeze = IsPaused;

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
		GameManager gameManager = GetParent<GameManager>();
		gameManager.PauseGame();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (IsPaused) return;
		base._PhysicsProcess(delta);

		if (Input.IsActionPressed("ui_accept"))
			ApplyCentralImpulse(new Vector2(0, -JumpPower));

	}

	public void KillPlayer()
	{
		GameManager gameManager = GetParent<GameManager>();
		gameManager.EndGame();
	}

}
