using Godot;
using System;
using System.Collections.Generic;

public partial class ObstacleManager : Node
{
	[Export]
	public float SpawnInterval = 2.0f;
	[Export]
	public float PatternSpawnInterval = 0.15f;
	[Export]
	public float ModeSwitchInterval = 30.0f;

	// Spawn bounds
	[Export]
	public float CeilingOffset = 200f;
	[Export]
	public float FloorOffset = 20f;

	// Difficulty scaling
	[Export]
	public float MinSpawnInterval = 0.6f;
	[Export]
	public float SpawnIntervalDecayRate = 0.008f; // per second, how fast standard spawn interval shrinks
	[Export]
	public float SpeedScaleRate = 0.5f; // extra speed per second of elapsed time
	[Export]
	public float MaxSpeedMultiplier = 3.0f;
	private float _elapsedTime = 0f;

	public List<Obstacle> _obstacles = new List<Obstacle>();
	private const string MINE_OBSTACLE_PATH = "res://nodes/obstacles/MineObstacle/MineObstacle.tscn";
	private const string HEALTH_POWERUP_PATH = "res://nodes/obstacles/powerUps/HealthPowerUp/HealthPowerUp.tscn";
	public GameManager _gameManager;

	private Timer _spawnTimer;
	private Timer _modeSwitchTimer;

	// Mode state
	public SpawnMode _currentMode = SpawnMode.Standard;
	private bool _isStandardTurn = true;
	public ConfineMode _currentConfineMode;
	public PatternMode _currentPatternMode;
	private int _patternStep = 0;
	private float _screenHeight;
	private bool _isPaused = false;

	// Computed spawn bounds
	private float _spawnMinY;   // ceiling edge
	private float _spawnMaxY;   // floor edge
	private float _spawnRange;  // total vertical spawn range

	public override void _Ready()
	{
		base._Ready();
		_gameManager = GetParent<GameManager>();
		_screenHeight = GetViewport().GetVisibleRect().Size.Y;
		_spawnMinY = -_screenHeight + CeilingOffset;
		_spawnMaxY = -FloorOffset;
		_spawnRange = _spawnMaxY - _spawnMinY;
		LoadSpawnTimer();
		LoadModeSwitchTimer();
	}

	public override void _Process(double delta)
	{
		if (_isPaused) return;
		_elapsedTime += (float)delta;

		// In standard mode, gradually shorten the spawn interval
		if (_currentMode == SpawnMode.Standard)
		{
			float scaledInterval = SpawnInterval - SpawnIntervalDecayRate * _elapsedTime;
			scaledInterval = Mathf.Max(scaledInterval, MinSpawnInterval);
			_spawnTimer.WaitTime = scaledInterval;
		}
	}

	/// <summary>
	/// Returns the current speed multiplier based on elapsed time.
	/// Starts at 1.0 and increases linearly, capped at MaxSpeedMultiplier.
	/// </summary>
	public float GetSpeedMultiplier()
	{
		return Mathf.Min(1.0f + SpeedScaleRate * (_elapsedTime / 60f), MaxSpeedMultiplier);
	}

	#region Timer Setup

	private void LoadSpawnTimer()
	{
		_spawnTimer = new Timer();
		_spawnTimer.Timeout += OnSpawnTimerTimeout;
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Autostart = true;
		AddChild(_spawnTimer);
	}

	private void LoadModeSwitchTimer()
	{
		_modeSwitchTimer = new Timer();
		_modeSwitchTimer.Timeout += OnModeSwitchTimeout;
		_modeSwitchTimer.WaitTime = ModeSwitchInterval;
		_modeSwitchTimer.Autostart = true;
		_modeSwitchTimer.OneShot = false;
		AddChild(_modeSwitchTimer);
	}

	#endregion

	#region Mode Switching

	private void OnModeSwitchTimeout()
	{
		_isStandardTurn = !_isStandardTurn;
		if (_isStandardTurn)
			EnterStandardMode();
		else
			EnterRandomMode();
	}

	private void EnterStandardMode()
	{
		_currentMode = SpawnMode.Standard;
		_spawnTimer.WaitTime = SpawnInterval;
		_patternStep = 0;
		GD.Print("Mode: Standard");
	}

	private void EnterRandomMode()
	{
		_patternStep = 0;

		if (GD.Randf() < 0.5f)
		{
			_currentMode = SpawnMode.Confine;
			var modes = Enum.GetValues(typeof(ConfineMode));
			_currentConfineMode = (ConfineMode)modes.GetValue((int)(GD.Randi() % modes.Length));
			GD.Print($"Mode: Confine ({_currentConfineMode})");
		}
		else
		{
			_currentMode = SpawnMode.Pattern;
			_currentPatternMode = PatternMode.Stairs;
			GD.Print($"Mode: Pattern ({_currentPatternMode})");
		}

		_spawnTimer.WaitTime = PatternSpawnInterval;
	}

	private void ResetMode()
	{
		_currentMode = SpawnMode.Standard;
		_isStandardTurn = true;
		_patternStep = 0;
		_elapsedTime = 0f;
		_isPaused = false;
		_spawnTimer.WaitTime = SpawnInterval;
		_modeSwitchTimer.Stop();
		_modeSwitchTimer.Start();
	}

	#endregion

	#region Spawn Dispatch

	private void OnSpawnTimerTimeout()
	{
		switch (_currentMode)
		{
			case SpawnMode.Standard:
				SpawnStandard();
				break;
			case SpawnMode.Pattern:
				SpawnPattern();
				break;
			case SpawnMode.Confine:
				SpawnConfine();
				break;
		}
	}

	#endregion

	#region Standard Mode

	private void SpawnStandard()
	{
		if (GD.Randf() < 0.03f)
		{
			SpawnObstacle(ObstacleType.HealthPowerUp, SpawnType.Random);
			return;
		}

		SpawnObstacle(ObstacleType.Mine, SpawnType.TargetPlayer);
		SpawnObstacle(ObstacleType.Mine, SpawnType.Random);
	}

	#endregion

	#region Pattern Mode

	private void SpawnPattern()
	{
		float y = CalculatePatternY(_currentPatternMode, _patternStep);
		_patternStep++;
		if (float.IsNaN(y)) return; // gap tick, skip spawn
		SpawnObstacleAtPosition(ObstacleType.Mine, new Vector2(800, y));
	}

	private float CalculatePatternY(PatternMode mode, int step)
	{
		float centerY = (_spawnMinY + _spawnMaxY) / 2f;

		float y;
		switch (mode)
		{
			case PatternMode.Stairs:
			{
				// Alternating flights with gap ticks for navigability
				int stairCount = 8;
				int minesPerStep = 2;
				int gapPerStep = 3;
				int ticksPerStep = minesPerStep + gapPerStep;
				int stepsPerFlight = stairCount * ticksPerStep;
				int flightIndex = step / stepsPerFlight;
				int posInFlight = step % stepsPerFlight;
				int currentStair = posInFlight / ticksPerStep;
				int tickInStair = posInFlight % ticksPerStep;

				if (tickInStair >= minesPerStep)
					return float.NaN; // gap tick

				float stepSize = _spawnRange / stairCount;
				if (flightIndex % 2 == 0)
					y = _spawnMaxY - currentStair * stepSize;
				else
					y = _spawnMinY + currentStair * stepSize;
				break;
			}
			default:
				y = centerY;
				break;
		}

		return Mathf.Clamp(y, _spawnMinY, _spawnMaxY);
	}

	#endregion

	#region Confine Mode

	private void SpawnConfine()
	{
		float centerY = (_spawnMinY + _spawnMaxY) / 2f;
		float halfGap = _spawnRange * 0.22f; // player-sized gap between the two walls
		int convergeSteps = 30;
		float convergeProgress = Mathf.Min(_patternStep / (float)convergeSteps, 1f);

		// Converge from raw edges toward center gap
		float baseTop = Mathf.Lerp(_spawnMaxY, centerY + halfGap, convergeProgress);
		float baseBottom = Mathf.Lerp(_spawnMinY, centerY - halfGap, convergeProgress);

		float topY, bottomY;

		switch (_currentConfineMode)
		{
			case ConfineMode.Straight:
			{
				// Converge then hold steady
				topY = baseTop;
				bottomY = baseBottom;
				break;
			}
			case ConfineMode.ZigZag:
			{
				// After convergence, zigzag with triangle wave starting at 0
				float offset = 0f;
				if (_patternStep >= convergeSteps)
				{
					int zigStep = _patternStep - convergeSteps;
					int quarterPeriod = 15;
					int fullPeriod = quarterPeriod * 4;
					int pos = zigStep % fullPeriod;
					float t;
					if (pos < quarterPeriod)
						t = pos / (float)quarterPeriod;
					else if (pos < 3 * quarterPeriod)
						t = 1f - (pos - quarterPeriod) / (float)quarterPeriod;
					else
						t = -1f + (pos - 3 * quarterPeriod) / (float)quarterPeriod;
					offset = t * _spawnRange * 0.25f;
				}
				topY = baseTop + offset;
				bottomY = baseBottom + offset;
				break;
			}
			case ConfineMode.Continuous:
			{
				// After convergence, smooth sine starting at 0
				float offset = 0f;
				if (_patternStep >= convergeSteps)
				{
					int sineStep = _patternStep - convergeSteps;
					offset = Mathf.Sin(sineStep * 0.08f) * _spawnRange * 0.25f;
				}
				topY = baseTop + offset;
				bottomY = baseBottom + offset;
				break;
			}
			default:
				topY = centerY + halfGap;
				bottomY = centerY - halfGap;
				break;
		}

		topY = Mathf.Clamp(topY, _spawnMinY, _spawnMaxY);
		bottomY = Mathf.Clamp(bottomY, _spawnMinY, _spawnMaxY);

		SpawnObstacleAtPosition(ObstacleType.Mine, new Vector2(800, topY));
		SpawnObstacleAtPosition(ObstacleType.Mine, new Vector2(800, bottomY));
		_patternStep++;
	}

	#endregion

	#region Spawning Helpers

	private Obstacle SpawnObstacleAtPosition(ObstacleType obstacleType, Vector2 position)
	{
		Obstacle obstacle = InstantiateObstacle(obstacleType);
		if (obstacle == null) return null;
		obstacle.Position = position;
		_obstacles.Add(obstacle);
		return obstacle;
	}

	private Obstacle SpawnObstacle(ObstacleType obstacleType, SpawnType spawnType)
	{
		Obstacle obstacle = InstantiateObstacle(obstacleType);
		if (obstacle == null) return null;

		switch (spawnType)
		{
			case SpawnType.Random:
				obstacle.Position = new Vector2(1000, (float)GD.RandRange(_spawnMinY, _spawnMaxY));
				break;
			case SpawnType.TargetPlayer:
				obstacle.Position = new Vector2(1000, _gameManager._player.Position.Y);
				break;
		}

		_obstacles.Add(obstacle);
		return obstacle;
	}

	private Obstacle InstantiateObstacle(ObstacleType obstacleType)
	{
		switch (obstacleType)
		{
			case ObstacleType.Mine:
				return SpawnMine();
			case ObstacleType.HealthPowerUp:
				return SpawnHealthPowerUp();
			default:
				return null;
		}
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

	#endregion

	#region Pause / Clear

	public void Pause()
	{
		_isPaused = !_isPaused;
		_spawnTimer.Paused = _isPaused;
		_modeSwitchTimer.Paused = _isPaused;
		foreach (Obstacle obstacle in _obstacles)
			obstacle.Pause();
	}

	public void ClearObstacles()
	{
		foreach (Obstacle obstacle in _obstacles)
			obstacle.QueueFree();
		_obstacles.Clear();
		ResetMode();
	}

	public void ActObstacle(Obstacle obstacle)
	{
		if (obstacle == null) return;
		obstacle.Act();
		_obstacles.Remove(obstacle);
	}

	#endregion
}

#region Enums

public enum SpawnMode
{
	Standard,
	Confine,
	Pattern
}

public enum ConfineMode
{
	ZigZag,
	Straight,
	Continuous
}

public enum PatternMode
{
	Stairs,
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

#endregion
