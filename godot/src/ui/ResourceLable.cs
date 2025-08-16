using Godot;
using System;

public partial class ResourceLable : Control {
    [Export]
    public Resource Resource { get; private set; }

    private Label _amountLabel;
    private int _amount;
    public int Amount {
        get => _amount;
        set {
            _amount = value < 0 ? 0 : value;
            if (_amountLabel != null)
                _amountLabel.Text = Resources.ToUnit(Resource, _amount);
        }
    }

    private TextureRect _exclamation;
    private Tween _exclamationTween;

    public override void _Ready() {
        _amountLabel = GetNode<Label>("Background/ResourceName/Amount");

        _exclamation = GetNode<TextureRect>("Background/TextureRect/Exclamation");
        _exclamation.Modulate = new Color(1, 1, 1, 0); // Fully transparent
    }

    public override void _Process(double delta) {
        if (Resources.IsCritical(Resource) && Resources.IsAtCriticalLevel(Resource, _amount)) {
            BlinkExclamation();
        }
    }

    public void SetPrimaryPlusOtherSources(int primary, int other_sources) {
        _amountLabel.Text = $"{Resources.ToUnit(Resource, primary)}({Resources.ToUnit(Resource, other_sources)})";
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
