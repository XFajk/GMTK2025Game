using Godot;
using System;

public partial class MusicPlayer : Node {
    [Export]
    public AudioStream MusicStream { get; set; }

    [Export]
    public float FadeTime { get; set; } = 1.5f; // seconds

    private Timer _fadeTimerNode = new();

    private AudioStreamPlayer _currentPlayer;
    private AudioStreamPlayer _nextPlayer;

    public override void _Ready() {
        AddChild(_fadeTimerNode);
        _fadeTimerNode.Timeout += () => {
            _nextPlayer = CreatePlayer();
            _nextPlayer.VolumeDb = -80.0f; 
            _nextPlayer.Play();
            _fadeTimerNode.Start(MusicStream.GetLength() - FadeTime*2);

            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(_nextPlayer, "volume_db", 10.0f, FadeTime); 
            tween.TweenProperty(_currentPlayer, "volume_db", -80.0f, FadeTime / 2);

            tween.Finished += () => {
                _currentPlayer.Stop();
                _currentPlayer.QueueFree();
                _currentPlayer = _nextPlayer;
                _nextPlayer = null;
            };
        };

        if (MusicStream == null) {
            GD.PrintErr("MusicStream is not set for MusicPlayer.");
            return;
        }

        _currentPlayer = CreatePlayer();
        _currentPlayer.Play();
        _fadeTimerNode.Start(MusicStream.GetLength() - FadeTime*2);


        // Preload the next track
        _nextPlayer = CreatePlayer();
        _nextPlayer.Stream = MusicStream; // Set to the same stream for now
        _nextPlayer.VolumeDb = 0f; // Start silent
    }

    private AudioStreamPlayer CreatePlayer() {
        var player = new AudioStreamPlayer();
        AddChild(player);
        player.Stream = MusicStream;
        player.VolumeDb = 10.0f;
        player.Bus = "Music";
        return player;
    }

    public void SetMusic(AudioStream newStream, float newFadeTime = -1f)
    {
        if (newStream == null)
            return;

        // Optionally update FadeTime
        if (newFadeTime >= 0f)
            FadeTime = newFadeTime;

        // If the new stream is the same as the current, do nothing
        if (_currentPlayer != null && _currentPlayer.Stream == newStream)
            return;

        // Prepare next player with new stream
        if (_nextPlayer != null)
        {
            _nextPlayer.Stop();
            _nextPlayer.QueueFree();
            _nextPlayer = null;
        }

        _nextPlayer = CreatePlayer();
        _nextPlayer.Stream = newStream;
        _nextPlayer.VolumeDb = -80.0f;
        _nextPlayer.Play();
        _fadeTimerNode.Start(MusicStream.GetLength() - FadeTime*2);

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(_nextPlayer, "volume_db", 10.0f, FadeTime);
        if (_currentPlayer != null)
            tween.TweenProperty(_currentPlayer, "volume_db", -80.0f, FadeTime/2);

        tween.Finished += () => {
            if (_currentPlayer != null)
            {
                _currentPlayer.Stop();
                _currentPlayer.QueueFree();
            }
            _currentPlayer = _nextPlayer;
            _nextPlayer = null;
            MusicStream = newStream;
        };
    }
}

