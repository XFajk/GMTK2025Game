using Godot;
using System;

public partial class ResourceLable : Control {
    [Export]
    public Resource Resource { get; private set; }

    private Label _amountLabel;
    private int _amount;

    private TextureRect _exclamation;
    private Tween _exclamationTween;

    public override void _Ready() {
        _amountLabel = GetNode<Label>("Background/Amount");

        _exclamation = GetNode<TextureRect>("Background/TextureRect/Exclamation");
        _exclamation.Modulate = new Color(1, 1, 1, 0); // Fully transparent
    }

    public override void _Process(double delta) {
        if (Resources.IsAtCriticalLevel(Resource, _amount)) {
            BlinkExclamation();
        }
    }

    public void SetAmount(int available, int total) {
        _amount = Mathf.Clamp(available, 0, total);
        int extra = total - available;
        if (_amountLabel != null)
            _amountLabel.Text = $"{Resources.ToUnit(Resource, _amount)} (+{Resources.ToUnit(Resource, extra)})";
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
