using Godot;
using System.Collections.Generic;

public partial class WaterLine : Node2D
{
	public Player _player;
	private float _time = 0f;

	// ── Gerstner wave components ──────────────────────────────────
	// Each wave: amplitude (px), wavelength (px), phase speed (px/s)
	private const float A1 = 7f;
	private const float WAVELENGTH1 = 140f;
	private const float SPEED1 = 120f;

	private const float A2 = 3f;
	private const float WAVELENGTH2 = 60f;
	private const float SPEED2 = 160f;

	// Steepness / longitudinal factor Q ∈ [0,1].
	// 1 = full Gerstner (sharp crests, flat troughs).
	// 0 = plain sinusoidal (no horizontal displacement).
	private const float Q = 0.85f;

	// How densely we sample the wave for drawing (pixels between samples).
	private const float SAMPLE_STEP = 2f;

	// Padding beyond the visible viewport edges so the wave doesn't pop in.
	private const float PAD = 60f;

	// ──────────────────────────────────────────────────────────────

	public override void _Process(double delta)
	{
		if (_player == null) return;

		if (!_player.IsPaused)
		{
			float speedMultiplier = _player._gameManager._obstacleManager.GetSpeedMultiplier();
			_time += (float)delta * speedMultiplier;
		}

		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_player == null) return;

		var camera = GetViewport().GetCamera2D();
		if (camera == null) return;

		var viewportSize = GetViewportRect().Size;
		var center = camera.GetScreenCenterPosition();

		float left  = center.X - viewportSize.X / 2f - PAD;
		float right = center.X + viewportSize.X / 2f + PAD;
		float baseY = _player._waterLineY;

		// ── Build the displaced Gerstner curve ──
		var points = new List<Vector2>();

		for (float x0 = left; x0 <= right; x0 += SAMPLE_STEP)
		{
			Vector2 d = GerstnerDisplacement(x0, _time);
			points.Add(new Vector2(x0 + d.X, baseY + d.Y));
		}

		if (points.Count < 2) return;

		// Draw the wave outline
		DrawPolyline(points.ToArray(), Colors.Black, 1f, false);
	}

	/// <summary>
	/// 2‑D Gerstner displacement with longitudinal component.
	///
	///   x(x₀,t) = x₀  −  Σ Q·Aᵢ · sin(kᵢ·x₀ − ωᵢ·t)
	///   y(x₀,t) =       −  Σ   Aᵢ · cos(kᵢ·x₀ − ωᵢ·t)
	///
	/// (Godot Y‑down: negative dy → upward crest)
	/// Returns the (dx, dy) offset to apply to the rest position (x₀, baseY).
	/// </summary>
	private Vector2 GerstnerDisplacement(float x0, float t)
	{
		float dx = 0f;
		float dy = 0f;

		// Wave component 1
		float k1 = Mathf.Tau / WAVELENGTH1;
		float w1 = k1 * SPEED1;
		float phase1 = k1 * x0 + w1 * t;
		dx += -Q * A1 * Mathf.Sin(phase1);
		dy += -A1 * Mathf.Cos(phase1);

		// Wave component 2
		float k2 = Mathf.Tau / WAVELENGTH2;
		float w2 = k2 * SPEED2;
		float phase2 = k2 * x0 + w2 * t;
		dx += -Q * A2 * Mathf.Sin(phase2);
		dy += -A2 * Mathf.Cos(phase2);

		return new Vector2(dx, dy);
	}
}
