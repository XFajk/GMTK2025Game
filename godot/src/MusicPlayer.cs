using Godot;
using System;

public partial class MusicPlayer : AudioStreamPlayer {
    public enum MusicTrack {
        Main,
        Alarm,
    }

    public void ChangeTrack(MusicTrack track) {
        var basicPlayback = GetStreamPlayback();
        if (basicPlayback is AudioStreamPlaybackInteractive playback) {
            playback.SwitchToClip((int)track);
        }
    }
}
