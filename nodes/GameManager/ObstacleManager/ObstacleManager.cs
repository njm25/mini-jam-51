using Godot;
using System;
using System.Collections.Generic;

public partial class ObstacleManager : Node
{
	[Export]
	public float SpawnInterval = 2.0f;
	public List<Obstacle> _obstacles = new List<Obstacle>();
	private const string MINE_OBSTACLE_PATH = "res://nodes/obstacles/MineObstacle/MineObstacle.tscn";
	private const string HEALTH_POWERUP_PATH = "res://nodes/obstacles/powerUps/HealthPowerUp/HealthPowerUp.tscn";
	public GameManager _gameManager;
	private Timer _spawnTimer;

	public override void _Ready()
	{
		base._Ready();
		_gameManager = GetParent<GameManager>();
		LoadSpawnTimer();
	}

	private void LoadSpawnTimer()
	{
		_spawnTimer = new Timer();
		_spawnTimer.Timeout += TimerEnd;
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Autostart = true;
		AddChild(_spawnTimer);
	}
	
	// to-do figure out spawning mechanics
	private void TimerEnd()
	{
		if (GD.Randf() < 0.03f)
		{
			SpawnObstacle(ObstacleType.HealthPowerUp, SpawnType.Random);
			return;
		}

		ObstacleType type = ObstacleType.Mine;
		SpawnObstacle(type, SpawnType.TargetPlayer);
		SpawnObstacle(type, SpawnType.Random);

	}

	private Obstacle SpawnObstacle(ObstacleType obstacleType, SpawnType spawnType)
	{
		Obstacle obstacle = new Obstacle();
		switch (obstacleType)
		{
			case ObstacleType.Mine:
				obstacle = SpawnMine();
				break;
			case ObstacleType.HealthPowerUp:
				obstacle = SpawnHealthPowerUp();
				break;
			default:
				break;
		}
		switch (spawnType)
		{
			case SpawnType.Random:
				obstacle.Position = new Vector2(800, (float)GD.RandRange(-50, -GetViewport().GetVisibleRect().Size.Y + 200));
				break;
			case SpawnType.TargetPlayer:
				obstacle.Position = new Vector2(800, _gameManager._player.Position.Y);
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

	private Obstacle SpawnHealthPowerUp()
	{
		var healthPowerUpScene = GD.Load<PackedScene>(HEALTH_POWERUP_PATH);
		HealthPowerUp healthPowerUp = healthPowerUpScene.Instantiate<HealthPowerUp>();
		AddChild(healthPowerUp);
		return healthPowerUp;
	}

	public void Pause()
	{
		_spawnTimer.Paused = !_spawnTimer.Paused;
		foreach (Obstacle obstacle in _obstacles)
			obstacle.Pause();
	}

	public void ClearObstacles()
	{
		foreach (Obstacle obstacle in _obstacles)
			obstacle.QueueFree();
		_obstacles.Clear();
	}

	public void ActObstacle(Obstacle obstacle)
	{
		if (obstacle == null) return;
		obstacle.Act();
		_obstacles.Remove(obstacle);
	}
}

public enum ObstacleType
{
	Mine,
	HealthPowerUp
}

public enum SpawnType
{
	TargetPlayer,
	Random
}
