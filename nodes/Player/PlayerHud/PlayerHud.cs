using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHud : Node
{
	public Player _player;

	// Template sprites (hidden in scene, used for textures)
	private Sprite2D _filledHeartTemplate;
	private Sprite2D _emptyHeartTemplate;
	private Sprite2D _progressBarTemplate;

	// Dynamic heart sprites
	private List<Sprite2D> _heartSprites = new List<Sprite2D>();
	private int _lastMaxHealth = -1;

	// Air bar
	private Label _airLabel;
	private ColorRect _airBarBg;
	private ColorRect _airBarFill;
	private Sprite2D _airBarBorder;

	// Battery bar
	private Label _batteryLabel;
	private ColorRect _batteryBarBg;
	private ColorRect _batteryBarFill;
	private Sprite2D _batteryBarBorder;

	// Labels
	private Label _healthLabel;
	private Label _scoreLabel;
	private Font _font;

	// Layout constants
	private const float MARGIN_X = 16f;
	private const float MARGIN_Y = 16f;
	private const float HEART_SPACING = 4f;
	private const float ROW_SPACING = 8f;
	private const float LABEL_HEIGHT = 20f;
	private const float BAR_WIDTH = 150f;
	private const float BAR_HEIGHT = 20f;

	public override void _Ready()
	{
		base._Ready();
		_player = GetParent<CanvasLayer>().GetParent<Player>();
		_font = GD.Load<Font>("res://assets/mspain.ttf");

		// Get template sprites
		_filledHeartTemplate = GetNode<Sprite2D>("FilledHeart");
		_emptyHeartTemplate = GetNode<Sprite2D>("EmptyHeart");
		_progressBarTemplate = GetNode<Sprite2D>("ProgressBar");

		// --- Score (centered at top) ---
		_scoreLabel = new Label();
		_scoreLabel.Text = "0";
		_scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_scoreLabel.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
		_scoreLabel.AddThemeFontSizeOverride("font_size", 28);
		if (_font != null)
			_scoreLabel.AddThemeFontOverride("font", _font);
		AddChild(_scoreLabel);

		float currentY = MARGIN_Y;

		// --- Health row ---
		_healthLabel = CreateLabel("Health", new Vector2(MARGIN_X, currentY));
		AddChild(_healthLabel);
		currentY += LABEL_HEIGHT;

		// Hearts will be built dynamically in _Process
		float heartH = _filledHeartTemplate.Texture.GetHeight();
		currentY += heartH + ROW_SPACING;

		// --- Air Level row ---
		_airLabel = CreateLabel("Air", new Vector2(MARGIN_X, currentY));
		AddChild(_airLabel);
		currentY += LABEL_HEIGHT;

		Vector2 barTextureSize = _progressBarTemplate.Texture.GetSize();
		float barScaleX = BAR_WIDTH / barTextureSize.X;
		float barScaleY = BAR_HEIGHT / barTextureSize.Y;

		_airBarBg = new ColorRect();
		_airBarBg.Color = new Color(0.15f, 0.15f, 0.15f, 0.6f);
		_airBarBg.Position = new Vector2(MARGIN_X, currentY);
		_airBarBg.Size = new Vector2(BAR_WIDTH, BAR_HEIGHT);
		AddChild(_airBarBg);

		_airBarFill = new ColorRect();
		_airBarFill.Color = new Color(0.4f, 0.75f, 1f, 0.85f); // light blue
		_airBarFill.Position = new Vector2(MARGIN_X, currentY);
		_airBarFill.Size = new Vector2(BAR_WIDTH, BAR_HEIGHT);
		AddChild(_airBarFill);

		_airBarBorder = new Sprite2D();
		_airBarBorder.Texture = _progressBarTemplate.Texture;
		_airBarBorder.Centered = false;
		_airBarBorder.Scale = new Vector2(barScaleX, barScaleY);
		_airBarBorder.Position = new Vector2(MARGIN_X, currentY);
		AddChild(_airBarBorder);

		currentY += BAR_HEIGHT + ROW_SPACING;

		// --- Battery Level row ---
		_batteryLabel = CreateLabel("Speaker", new Vector2(MARGIN_X, currentY));
		AddChild(_batteryLabel);
		currentY += LABEL_HEIGHT;

		_batteryBarBg = new ColorRect();
		_batteryBarBg.Color = new Color(0.15f, 0.15f, 0.15f, 0.6f);
		_batteryBarBg.Position = new Vector2(MARGIN_X, currentY);
		_batteryBarBg.Size = new Vector2(BAR_WIDTH, BAR_HEIGHT);
		AddChild(_batteryBarBg);

		_batteryBarFill = new ColorRect();
		_batteryBarFill.Color = new Color(0.4f, 0.9f, 0.4f, 0.85f); // light green
		_batteryBarFill.Position = new Vector2(MARGIN_X, currentY);
		_batteryBarFill.Size = new Vector2(BAR_WIDTH, BAR_HEIGHT);
		AddChild(_batteryBarFill);

		_batteryBarBorder = new Sprite2D();
		_batteryBarBorder.Texture = _progressBarTemplate.Texture;
		_batteryBarBorder.Centered = false;
		_batteryBarBorder.Scale = new Vector2(barScaleX, barScaleY);
		_batteryBarBorder.Position = new Vector2(MARGIN_X, currentY);
		AddChild(_batteryBarBorder);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		// Rebuild hearts if max health changed
		if (_player.MaxHealth != _lastMaxHealth)
		{
			RebuildHearts();
			_lastMaxHealth = _player.MaxHealth;
		}

		// Update filled vs empty hearts
		for (int i = 0; i < _heartSprites.Count; i++)
		{
			_heartSprites[i].Texture = i < _player.Health
				? _filledHeartTemplate.Texture
				: _emptyHeartTemplate.Texture;
		}

		// Update score (centered at top)
		_scoreLabel.Text = _player.Score.ToString();
		var viewportSize = GetViewport().GetVisibleRect().Size;
		var labelSize = _scoreLabel.GetMinimumSize();
		_scoreLabel.Position = new Vector2((viewportSize.X - labelSize.X) / 2f, MARGIN_Y);

		// Update air bar fill
		float airRatio = Mathf.Clamp(_player.AirLevel / _player.MaxAir, 0f, 1f);
		_airBarFill.Size = new Vector2(BAR_WIDTH * airRatio, BAR_HEIGHT);

		// Update battery bar fill
		float batteryRatio = Mathf.Clamp(_player.SpeakerCharge / _player.MaxSpeakerCharge, 0f, 1f);
		_batteryBarFill.Size = new Vector2(BAR_WIDTH * batteryRatio, BAR_HEIGHT);
	}

	private void RebuildHearts()
	{
		// Remove old hearts
		foreach (var heart in _heartSprites)
		{
			heart.QueueFree();
		}
		_heartSprites.Clear();

		float heartW = _filledHeartTemplate.Texture.GetWidth();
		float heartH = _filledHeartTemplate.Texture.GetHeight();
		float heartsY = MARGIN_Y + LABEL_HEIGHT + heartH / 2f;

		for (int i = 0; i < _player.MaxHealth; i++)
		{
			var heart = new Sprite2D();
			heart.Texture = _emptyHeartTemplate.Texture;
			heart.Position = new Vector2(
				MARGIN_X + heartW / 2f + i * (heartW + HEART_SPACING),
				heartsY
			);
			AddChild(heart);
			_heartSprites.Add(heart);
		}
	}

	private Label CreateLabel(string text, Vector2 position)
	{
		var label = new Label();
		label.Text = text;
		label.Position = position;
		label.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1)); // black
		label.AddThemeFontSizeOverride("font_size", 14);
		if (_font != null)
			label.AddThemeFontOverride("font", _font);
		return label;
	}
}
