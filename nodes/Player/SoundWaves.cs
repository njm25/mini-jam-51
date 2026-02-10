using Godot;
using System;
using System.Collections.Generic;

public partial class SoundWaves : Node2D
{
	private const int MAX_RINGS = 4;
	private const float RING_SPEED = 120f;
	private const float SPAWN_INTERVAL = 0.12f;
	private const float MAX_RADIUS = 200f;
	private const float PIXEL_SIZE = 2.5f;
	private const int POINTS_PER_RING = 24;

	private List<float> _rings = new List<float>();
	private float _spawnTimer = 0f;
	private bool _active = false;

	public override void _Process(double delta)
	{
		base._Process(delta);

		var player = GetParent<Player>();
		bool shouldBeActive = player.IsUsingSpeaker && !player.IsPaused;

		if (shouldBeActive && !_active)
		{
			_active = true;
			_rings.Add(10f);
			_spawnTimer = 0f;
		}
		else if (!shouldBeActive && _active)
		{
			_active = false;
		}

		if (_active)
		{
			_spawnTimer += (float)delta;
			if (_spawnTimer >= SPAWN_INTERVAL)
			{
				_rings.Add(10f);
				_spawnTimer = 0f;
			}
		}

		for (int i = _rings.Count - 1; i >= 0; i--)
		{
			_rings[i] += RING_SPEED * (float)delta;
			if (_rings[i] > MAX_RADIUS)
			{
				_rings.RemoveAt(i);
			}
		}

		QueueRedraw();
	}

	public override void _Draw()
	{
		if ((_active || _rings.Count > 0) == false) return;

		foreach (float radius in _rings)
		{
			float alpha = 1f - (radius / MAX_RADIUS);
			Color color = new Color(0f, 0f, 0f, alpha * 0.8f);

			for (int i = 0; i < POINTS_PER_RING; i++)
			{
				float angle = (i / (float)POINTS_PER_RING) * Mathf.Tau;
				float x = Mathf.Cos(angle) * radius;
				float y = Mathf.Sin(angle) * radius;

				// Snap to pixel grid for pixely look
				x = Mathf.Round(x / PIXEL_SIZE) * PIXEL_SIZE;
				y = Mathf.Round(y / PIXEL_SIZE) * PIXEL_SIZE;

				DrawRect(new Rect2(x - PIXEL_SIZE / 2f, y - PIXEL_SIZE / 2f, PIXEL_SIZE, PIXEL_SIZE), color);
			}
		}
	}
}
