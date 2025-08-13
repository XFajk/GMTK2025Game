using Godot;
using System;

public partial class DiagramButton : Button {

    private Panel _resourceDiagram = null;
    public override void _Ready() {
        _resourceDiagram = GetNode<Panel>("ResourceDiagram");
        _resourceDiagram.Size = new Vector2(0.0f, 0.0f);
        _resourceDiagram.Position = new Vector2(-9.0f, -1.0f);

        Toggled += OnToggled;
    }

    private void OnToggled(bool pressed) {
        Tween tween = GetTree().CreateTween().SetParallel();

        if (pressed) {
            _resourceDiagram.Visible = true;
            tween.TweenProperty(_resourceDiagram, "size", new Vector2(765.0f, 502.0f), 0.2f);
            tween.TweenProperty(_resourceDiagram, "position", new Vector2(-774.0f, -503.0f), 0.2f);
        } else {
            tween.TweenProperty(_resourceDiagram, "size", new Vector2(0.0f, 0.0f), 0.2f);
            tween.TweenProperty(_resourceDiagram, "position", new Vector2(-9.0f, -1.0f), 0.2f);
            tween.TweenCallback(Callable.From(() => {
                _resourceDiagram.Visible = false;
            })).SetDelay(0.2f);
        }
    }
}
