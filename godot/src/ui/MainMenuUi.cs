using Godot;
using System;

public partial class MainMenuUi : Control {

    public Label Title;
    public Button StartButton;
    public Button SettingsButton;
    public Button ExitButton;

    public Button SettingExitButton;

    public Control SettingsPanel;

    public PackedScene GameScene = GD.Load<PackedScene>("res://scenes/game.tscn");

    public HSlider SoundVolumeSlider;
    public HSlider MusicVolumeSlider;
    public HSlider MouseDragSensitivitySlider;
    public CheckBox VSyncCheckButton;

    private Node _global;
    private Node _saveSystem;

    public override void _Ready() {
        Title = GetNode<Label>("Title");
        StartButton = GetNode<Button>("MainMenu/Start");
        SettingsButton = GetNode<Button>("MainMenu/Settings");
        ExitButton = GetNode<Button>("MainMenu/Exit");
        SettingsPanel = GetNode<Control>("SettingsPanel");

        // Get sliders from the SettingsPanel
        SoundVolumeSlider = GetNode<HSlider>("SettingsPanel/Sliders/SoundsVolume");
        MusicVolumeSlider = GetNode<HSlider>("SettingsPanel/Sliders/MusicVolume");
        MouseDragSensitivitySlider = GetNode<HSlider>("SettingsPanel/Sliders/MouseDragSensitivity");

        VSyncCheckButton = GetNode<CheckBox>("SettingsPanel/VSync");

        // Cache the Global singleton
        _global = GetNode("/root/Global");
        _saveSystem = GetNode("/root/SaveSystem");

        // Set initial slider values from global
        if (_global != null) {
            SoundVolumeSlider.Value = (float)(double)_global.Get("sound_volume");
            MusicVolumeSlider.Value = (float)(double)_global.Get("music_volume");
            MouseDragSensitivitySlider.Value = (float)(double)_global.Get("mouse_drag_sensitivity");
            VSyncCheckButton.ButtonPressed = (bool)_global.Get("v_sync");
        }

        // Connect slider value changed signals
        SoundVolumeSlider.ValueChanged += (value) => {
            if (_global != null) _global.Set("sound_volume", value);
            _saveSystem.Call("save_game");
        };
        MusicVolumeSlider.ValueChanged += (value) => {
            if (_global != null) _global.Set("music_volume", value);
            _saveSystem.Call("save_game");
        };
        MouseDragSensitivitySlider.ValueChanged += (value) => {
            if (_global != null) _global.Set("mouse_drag_sensitivity", value);
            _saveSystem.Call("save_game");
        };
        VSyncCheckButton.Toggled += (value) => {
            if (_global != null) _global.Set("v_sync", value);
            _saveSystem.Call("save_game");
        };

        StartButton.Pressed += OnStartButtonPressed;
        SettingsButton.Pressed += OnSettingButtonPressed;
        ExitButton.Pressed += OnExitButtonPressed;
    }

    private void OnStartButtonPressed() {
        // Change to the game scene and remove the main menu
        GetTree().ChangeSceneToPacked(GameScene);
    }

    private void OnSettingButtonPressed() {
        if (SettingsPanel.Visible == false) {
            SettingsPanel.Visible = true;
        } else {
            SettingsPanel.Visible = false;
        }
    }

    private void OnExitButtonPressed() {
        // Logic to exit the game
        GetTree().Quit();
    }
}
