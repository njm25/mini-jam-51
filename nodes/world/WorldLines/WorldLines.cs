using Godot;

public partial class WorldLines : Node2D
{
	public Player _player;
	private float _scrollOffset = 0f;

	private const float SCROLL_SPEED = 100f;

	// Repeating pixel pattern: Y offsets to create a subtle pixelated wave.
	// 0 = baseline, -1 = one pixel up, 1 = one pixel down.
	private static readonly int[] PATTERN = {
		0, 0, 0, 0, -1, -1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0
	};

	public override void _Process(double delta)
	{
		if (_player == null) return;

		if (!_player.IsPaused)
		{
			_scrollOffset += SCROLL_SPEED * (float)delta;
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

		float left = center.X - viewportSize.X / 2f - 20f;
		float right = center.X + viewportSize.X / 2f + 20f;
		float bottom = center.Y + viewportSize.Y / 2f + 20f;

		// White fill below ground (Y=0 downward)
		float groundY = 0f;
		if (groundY < bottom)
		{
			DrawRect(new Rect2(left, groundY, right - left, bottom - groundY), Colors.White);
		}

		// Draw scrolling pixelated lines
		float waterLineY = _player._waterLineY;
		DrawPixelLine(left, right, waterLineY);
		DrawPixelLine(left, right, groundY);
	}

	private void DrawPixelLine(float left, float right, float baseY)
	{
		int patternLen = PATTERN.Length;

		for (float x = left; x <= right; x += 1f)
		{
			// Shift pattern index by scroll offset to create movement
			int idx = ((int)(x + _scrollOffset) % patternLen + patternLen) % patternLen;
			float y = baseY + PATTERN[idx];
			DrawRect(new Rect2(x, y, 1f, 1f), Colors.Black);
		}
	}
}
