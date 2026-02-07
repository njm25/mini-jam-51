using Godot;
using System;

public partial class PowerUp : Obstacle
{

	public override void _Ready()
	{
		base._Ready();
		DoesDamage = false;
	}


}
