using Godot;
using System;

public partial class ButtonComponent : Button
{
	private AudioStreamPlayer _hoverSound;
	private AudioStreamPlayer _clickSound;

	public override void _Ready()
	{
		_hoverSound = GetNode<AudioStreamPlayer>("HoverSound"); // Adjust path
		_clickSound = GetNode<AudioStreamPlayer>("ClickSound"); // Adjust path
		_hoverSound.MaxPolyphony = 16;
		// Connect the signals via code
		Pressed += OnStartButtonPressed;
		MouseEntered += OnStartButtonMouseEntered;
	}
	private void OnStartButtonPressed()
	{
		_clickSound.Play();
	}
	private void OnStartButtonMouseEntered()
	{
		_hoverSound.Play();
	}

}
