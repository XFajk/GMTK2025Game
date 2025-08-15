using Godot;
using System;

public partial class StatusBar : ProgressBar {

    private Sprite2D _icon;
    private Sprite2D _exclamation;
    private Tween _exclamationTween;

    private StyleBoxFlat _fillStyleBox;
    private IContainer _target;

    public override void _Ready() {
        _icon = GetNode<Sprite2D>("Icon");

        _exclamation = GetNode<Sprite2D>("Exclamation");
        _exclamation.Modulate = new Color(1, 1, 1, 0); // Fully transparent

        _fillStyleBox = (StyleBoxFlat)GetThemeStylebox("fill").Duplicate();
        AddThemeStyleboxOverride("fill", _fillStyleBox);
    }

    public override void _Process(double delta) {
        double newValue;
        if (_target.GetQuantity() == 0) {
            newValue = 0;
        } else if (Resources.IsFloating(_target.GetResource())) {
            // floating is either empty or full
            newValue = (_target.GetQuantity() >= _target.GetMaxQuantity()) ? 100.0f : 0;
        } else {
            newValue = (_target.GetQuantity() / _target.GetMaxQuantity() * 100.0);
        }
        if (Value != newValue) {
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(this, "value", newValue, 0.2f);
        }

        if (Resources.IsCritical(_target.GetResource())) {
            if (_target is StorageContainer && Value <= MinValue + Step) {
                BlinkExclamation();
            }
        }
    }

    public void SetStatusFrom(IContainer buffer) {
        if (buffer.GetResource() == Resource.Unset) {
            _icon.Visible = false;
            Value = 0;
            return;
        }

        _icon.Visible = true;
        _icon.Texture = Resources.GetResourceIcon(buffer.GetResource());
        _fillStyleBox.BgColor = Resources.GetResourceColor(buffer.GetResource());

        double newValue = 0.0;
        if (buffer.GetQuantity() == 0) {
            newValue = 0.0;
        } else {
            newValue = (buffer.GetQuantity() / buffer.GetMaxQuantity() * 100.0);
        }

        if (Value != newValue) {
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(this, "value", newValue, 0.2f);
        }
        _target = buffer;
    }

    private void BlinkExclamation() {
        if (_exclamation != null && _exclamationTween == null) {
            _exclamationTween = GetTree().CreateTween();
            _exclamationTween.TweenProperty(_exclamation, "modulate", new Color(1, 1, 1, 1), 0.2f);
            _exclamationTween.TweenProperty(_exclamation, "modulate", new Color(1, 1, 1, 0), 0.2f);

            _exclamationTween.TweenCallback(Callable.From(() => {
                _exclamationTween = null;
            }));
        }
    }
}
