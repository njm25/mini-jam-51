using Godot;
using System;

public partial class Camera : Camera2D
{
	[Export]
	public Vector2 CameraOffset { get; set; } = new Vector2(300, -150);
	public override void _Ready()
	{
		base._Ready();
		this.Position = CameraOffset;
	}

}
