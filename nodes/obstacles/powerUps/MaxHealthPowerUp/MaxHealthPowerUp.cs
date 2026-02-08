using Godot;
using System;

public partial class MaxHealthPowerUp : PowerUp
{
	public override void _Ready()
	{
		base._Ready();
	}

	public override void Interact()
	{
		Player player = _obstacleManager._gameManager._player;
		DestroyLabelText = "+1 Max Health";
		player.MaxHealth++;
	}

}
