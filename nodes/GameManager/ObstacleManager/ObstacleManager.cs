using Godot;
using System;
using System.Collections.Generic;

public partial class ObstacleManager : Node
{
	[Export]
	public float SpawnInterval = 2.0f;
	List<Obstacle> _obstacles = new List<Obstacle>();
	private const string MINE_OBSTACLE_PATH = "res://nodes/obstacles/MineObstacle/MineObstacle.tscn";

	private Timer _spawnTimer;

	public override void _Ready()
	{
		base._Ready();
		LoadSpawnTimer();
	}

	private void LoadSpawnTimer()
	{
		_spawnTimer = new Timer();
		_spawnTimer.Timeout += SpawnRandomObstacle;
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Autostart = true;
		AddChild(_spawnTimer);
	}

	private void SpawnRandomObstacle()
	{
		ObstacleType type = ObstacleType.Mine;

		SpawnObstacle(type);
	}

	private Obstacle SpawnObstacle(ObstacleType type)
	{
		Obstacle obstacle = new Obstacle();
		switch (type)
		{
			case ObstacleType.Mine:
				obstacle = SpawnMine();
				break;
			default:
				GD.PrintErr("Unsupported obstacle type: " + type);
				break;
		}
		_obstacles.Add(obstacle);
		return obstacle;
	}

	public Obstacle SpawnMine()
	{
		var mineScene = GD.Load<PackedScene>(MINE_OBSTACLE_PATH);
		MineObstacle mineObstacle = mineScene.Instantiate<MineObstacle>();
		AddChild(mineObstacle);
		return mineObstacle;
		
	}

	public void PauseObstacles()
	{
		foreach (Obstacle obstacle in _obstacles)
			obstacle.Pause();
	}

	public void ClearObstacles()
	{
		foreach (Obstacle obstacle in _obstacles)
			obstacle.QueueFree();
		_obstacles.Clear();
	}

	public void RemoveObstacle(Obstacle obstacle)
	{
		if (obstacle == null) return;
		_obstacles.Remove(obstacle);
		obstacle.QueueFree();
	}
}

public enum ObstacleType
{
	Mine
}
