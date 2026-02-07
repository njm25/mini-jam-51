using Godot;
using System;

public partial class Main : Node
{
	const string GAME_MANAGER_PATH = "res://nodes/GameManager/GameManager.tscn";

	public override void _Ready()
	{
		base._Ready();
		
		var gameManagerScene = GD.Load<PackedScene>(GAME_MANAGER_PATH);
		var gameManager = gameManagerScene.Instantiate<GameManager>();
		AddChild(gameManager);
	
	} 

}
