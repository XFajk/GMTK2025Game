using Godot;
using System;
using System.Collections.Generic;

public partial class ResourceExpandButton : Button {

    private List<ResourceLable> _resources = new List<ResourceLable>();
    private Sprite2D _arrowIcon;

    [Export]
    public MissionDialog MissionDialog;

    public override void _Ready() {
        _arrowIcon = GetNode<Sprite2D>("ArrowIcon");

        foreach (Node child in GetNode("Resources").GetChildren()) {
            if (child is ResourceLable resource) {
                _resources.Add(resource);
            }
        }

        Toggled += OnToggled; 
    }

    private void OnToggled(bool toggledOn) {
        Tween resourceTween = GetTree().CreateTween();
        Tween arrowTween = GetTree().CreateTween();

        if (toggledOn) {
            foreach (ResourceLable resource in _resources) {
                resourceTween.TweenProperty(resource, "visible", true, 0).SetDelay(0.07);
            }
            arrowTween.TweenProperty(_arrowIcon, "rotation", -Mathf.Pi / 2, 0.1);
        } else {
            foreach (ResourceLable resource in _resources) {
                resourceTween.TweenProperty(resource, "visible", false, 0).SetDelay(0.07);
            }
            arrowTween.TweenProperty(_arrowIcon, "rotation", -Mathf.Pi, 0.1);
        }
    }
}
