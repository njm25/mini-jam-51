using Godot;
using System;
using System.Collections.Generic;

public partial class MenuManager : Node
{
    private GameManager _gameManager;
	public PauseMenu _pauseMenu;
	public StartMenu _startMenu;
	public RetryMenu _retryMenu;
	private CanvasLayer _canvasLayer;
    private Stack<Control> _menuStack = new Stack<Control>();
	private AudioStreamPlayer _menuMusicPlayer;
	const string START_MENU_PATH = "res://nodes/menus/StartMenu/StartMenu.tscn";
	const string RETRY_MENU_PATH = "res://nodes/menus/RetryMenu/RetryMenu.tscn";
	const string PAUSE_MENU_PATH = "res://nodes/menus/PauseMenu/PauseMenu.tscn";
    const string MENU_MUSIC_PATH = "res://assets/menu.wav";

    #region Lifecycle

    public override void _Ready()
    {
        base._Ready();
        _gameManager = GetParent<GameManager>();

        LoadCanvasLayer();
        LoadMenus();
        LoadMenuMusic();
    }

    #endregion

    #region Loading/Unloading

    private void LoadCanvasLayer()
    {
        CanvasLayer canvasLayer = new CanvasLayer();
        AddChild(canvasLayer);
        _canvasLayer = canvasLayer;
    }

    private void LoadMenus()
    {

        var startMenuScene = GD.Load<PackedScene>(START_MENU_PATH);
        _startMenu = startMenuScene.Instantiate<StartMenu>();
        _startMenu.Visible = false;
        _canvasLayer.AddChild(_startMenu);

        var retryMenuScene = GD.Load<PackedScene>(RETRY_MENU_PATH);
        _retryMenu = retryMenuScene.Instantiate<RetryMenu>();
        _retryMenu.Visible = false;
        _canvasLayer.AddChild(_retryMenu);

        var pauseMenuScene = GD.Load<PackedScene>(PAUSE_MENU_PATH);
        _pauseMenu = pauseMenuScene.Instantiate<PauseMenu>();
        _pauseMenu.Visible = false;
        _canvasLayer.AddChild(_pauseMenu);
    }

    private void LoadMenuMusic()
    {
        _menuMusicPlayer = new AudioStreamPlayer();
        var stream = GD.Load<AudioStream>(MENU_MUSIC_PATH);
        _menuMusicPlayer.Stream = stream;
        _menuMusicPlayer.Autoplay = false;
        AddChild(_menuMusicPlayer);
        _menuMusicPlayer.Finished += () => _menuMusicPlayer.Play();
    }

    #endregion

    #region Menu Management

    public void Navigate(Control menu)
    {
        if (_menuStack.Count > 0)
        {
            _menuStack.Peek().Visible = false;
        }
        menu.Visible = true;
        _menuStack.Push(menu);
        if (!_menuMusicPlayer.Playing)
            _menuMusicPlayer.Play();
    }

    public void Back()
    {
        if (_menuStack.Count == 0)
            return;
        _menuStack.Peek().Visible = false;
        _menuStack.Pop();
        if (_menuStack.Count > 0)
        {
            _menuStack.Peek().Visible = true;
        }
        else
        {
            _menuMusicPlayer.Stop();
        }
    }

    public void Close()
    {
        if (_menuStack.Count == 0)
            return;
        for (int i = 0; i < _menuStack.Count; i++) {
            _menuStack.Peek().Visible = false;
            _menuStack.Pop();
        }
        _menuMusicPlayer.Stop();
    }

    #endregion
}
