using Godot;
using System;

public partial class MusicPlayer : AudioStreamPlayer {
    public enum MusicTrack {
        Main,
        Alarm,
    }

    private MusicTrack _current;

    public void SetTrack(MusicTrack track) {
        if (track == _current) return;

        _current = track;
        var basicPlayback = GetStreamPlayback();
        if (basicPlayback is AudioStreamPlaybackInteractive playback) {
            playback.SwitchToClip((int)track);
            GD.Print($"Now playing track {track}");
        }
    }
}
