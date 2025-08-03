using Godot;
using System;

public partial class MainMenuUi : Control {
    public Button StartButton;
    public Button SettingsButton;
    public Button ExitButton;

    public Button SettingExitButton;

    public Control SettingsPanel;

    public PackedScene GameScene = GD.Load<PackedScene>("res://scenes/game.tscn");

    public HSlider SoundVolumeSlider;
    public HSlider MusicVolumeSlider;
    public HSlider MouseDragSensitivitySlider;

    private Node _global;

    public override void _Ready() {
        StartButton = GetNode<Button>("Start");
        SettingsButton = GetNode<Button>("Settings");
        ExitButton = GetNode<Button>("Exit");
        SettingsPanel = GetNode<Control>("SettingsPanel");
        SettingExitButton = GetNode<Button>("SettingsPanel/Exit");

        // Get sliders from the SettingsPanel
        SoundVolumeSlider = GetNode<HSlider>("SettingsPanel/SoundsVolume");
        MusicVolumeSlider = GetNode<HSlider>("SettingsPanel/MusicVolume");
        MouseDragSensitivitySlider = GetNode<HSlider>("SettingsPanel/MouseDragSensitivity");

        // Cache the Global singleton
        _global = GetNode("/root/Global");

        // Set initial slider values from global
        if (_global != null) {
            SoundVolumeSlider.Value = (float)(double)_global.Get("sound_volume");
            MusicVolumeSlider.Value = (float)(double)_global.Get("music_volume");
            MouseDragSensitivitySlider.Value = (float)(double)_global.Get("mouse_drag_sensitivity");
        }

        // Connect slider value changed signals
        SoundVolumeSlider.ValueChanged += (value) => {
            if (_global != null) _global.Set("sound_volume", value);
        };
        MusicVolumeSlider.ValueChanged += (value) => {
            if (_global != null) _global.Set("music_volume", value);
        };
        MouseDragSensitivitySlider.ValueChanged += (value) => {
            if (_global != null) _global.Set("mouse_drag_sensitivity", value);
        };

        StartButton.Pressed += OnStartButtonPressed;
        SettingsButton.Pressed += OnSettingButtonPressed;
        ExitButton.Pressed += OnExitButtonPressed;
        SettingExitButton.Pressed += () => {
            SettingsPanel.Visible = false;
            StartButton.Visible = true;
            SettingsButton.Visible = true;
            ExitButton.Visible = true;
        };
    }

    private void OnStartButtonPressed() {
        // Change to the game scene and remove the main menu
        GetTree().ChangeSceneToPacked(GameScene);
    }

    private void OnSettingButtonPressed() {
        StartButton.Visible = false;
        SettingsButton.Visible = false;
        ExitButton.Visible = false;
        SettingsPanel.Visible = true;

    }

    private void OnExitButtonPressed() {
        // Logic to exit the game
        GetTree().Quit();
    }
}
