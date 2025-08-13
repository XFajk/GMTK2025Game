using Godot;
using System;
using System.Collections.Generic;

public partial class ResourceExpandButton : Button {

    private VBoxContainer _resources;
    private Sprite2D _arrowIcon;

    [Export]
    public MissionDialog MissionDialog;

    public override void _Ready() {
        _arrowIcon = GetNode<Sprite2D>("ArrowIcon");
        _resources = GetNode<VBoxContainer>("Resources");
        _resources.Modulate = new Color(1, 1, 1, 0);

        Toggled += OnToggled; 
    }

    private void OnToggled(bool toggledOn) {
        Tween resourceTween = GetTree().CreateTween().SetParallel();
        Tween arrowTween = GetTree().CreateTween();

        if (toggledOn) {
            resourceTween.TweenProperty(_resources, "theme_override_constants/separation", 5, 0.3).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            resourceTween.TweenProperty(_resources, "modulate", new Color(1, 1, 1, 1), 0.3);
            arrowTween.TweenProperty(_arrowIcon, "rotation", -Mathf.Pi / 2, 0.1);
        } else {
            resourceTween.TweenProperty(_resources, "theme_override_constants/separation", -40, 0.3).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            resourceTween.TweenProperty(_resources, "modulate", new Color(1, 1, 1, 0), 0.3);
            arrowTween.TweenProperty(_arrowIcon, "rotation", -Mathf.Pi, 0.1);
        }
    }
}
