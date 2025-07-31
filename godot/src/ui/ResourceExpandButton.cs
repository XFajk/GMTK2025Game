using Godot;
using System;

public partial class ResourceExpandButton : Button {
    private AnimationPlayer _resourcesContainerAnimationPlayer;
    private AnimationPlayer _arrowIconAnimationPlayer;

    [Export]
    public MissionDialog MissionDialog;

    public override void _Ready() {
        _resourcesContainerAnimationPlayer = GetNode<AnimationPlayer>("Resources/AnimationPlayer");
        _arrowIconAnimationPlayer = GetNode<AnimationPlayer>("ArrowIcon/AnimationPlayer");
        Toggled += OnToggled;
    }

    private void OnToggled(bool toggledOn) {
        if (toggledOn) {
            _resourcesContainerAnimationPlayer.Stop();
            _resourcesContainerAnimationPlayer.Play("Open");
            _arrowIconAnimationPlayer.Stop();
            _arrowIconAnimationPlayer.Play("Open");
        } else {
            _resourcesContainerAnimationPlayer.Play("Close");
            _arrowIconAnimationPlayer.Play("Close");
        }
    }
}
