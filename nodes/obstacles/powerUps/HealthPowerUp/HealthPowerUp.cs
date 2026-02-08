using Godot;
using System;

public partial class HealthPowerUp : PowerUp
{
	public override void _Ready()
	{
		base._Ready();
	}

	public override void Interact()
	{
		Player player = _obstacleManager._gameManager._player;
		if (player.Health < player.MaxHealth)
		{
			DestroyLabelText = "+1 Health";
			player.Health++;
		}
	}

}
