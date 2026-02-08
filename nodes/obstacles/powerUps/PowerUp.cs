using Godot;
using System;

public partial class PowerUp : Obstacle
{

	public override void _Ready()
	{
		DESTROY_SOUND_PATH = "res://assets/powerup.mp3";
		base._Ready();
		DoesDamage = false;
		BypassIFrame = true;
		BobUpAndDown = true;
	}


	protected override void Destroy(bool playSound = true)
	{
		Label destroyLabel = new Label();
		destroyLabel.Text = DestroyLabelText;
		destroyLabel.AddThemeColorOverride("font_color", Colors.Black);
		var font = GD.Load<Font>("res://assets/mspain.ttf");
		if (font != null)
			destroyLabel.AddThemeFontOverride("font", font);
		destroyLabel.Position = new Vector2(Position.X + 20, Position.Y - 20);
		GetParent().AddChild(destroyLabel);

		Timer labelTimer = new Timer();
		labelTimer.WaitTime = 2.0f;
		labelTimer.OneShot = true;
		labelTimer.Timeout += () =>
		{
			destroyLabel.QueueFree();
		};
		destroyLabel.AddChild(labelTimer);
		labelTimer.Start();

		base.Destroy(playSound);
	}

}
