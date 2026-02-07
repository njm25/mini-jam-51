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
		GD.Print(" adding health");
		if (player.Health < player.MaxHealth)
			player.Health++;
	}

}
