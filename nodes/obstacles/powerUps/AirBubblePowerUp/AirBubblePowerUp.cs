using Godot;
using System;

public partial class AirBubblePowerUp : PowerUp
{
	public override void _Ready()
	{
		base._Ready();
	}

	public override void Interact()
	{
		Player player = _obstacleManager._gameManager._player;
		player.AirLevel = player.MaxAir;
	}

}
