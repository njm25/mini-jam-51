using Godot;
using System;

public partial class SpeakerBatteryPowerUp : PowerUp
{
	[Export]
	public float SpeakerChargeAmount { get; set; } = 20f;
	public override void _Ready()
	{
		base._Ready();
	}

	public override void Interact()
	{
		Player player = _obstacleManager._gameManager._player;
		player.SpeakerCharge = Math.Min(player.SpeakerCharge + SpeakerChargeAmount, player.MaxSpeakerCharge);
	}

}
