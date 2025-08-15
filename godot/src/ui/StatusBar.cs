using Godot;
using System;

public partial class StatusBar : ProgressBar {

    private Sprite2D _icon;
    private Sprite2D _exclamation;
    private Tween _exclamationTween;

    private StyleBoxFlat _fillStyleBox;
    private MachineBuffer _buffer;
    private StorageContainer _storageContainer;

    public override void _Ready() {
        _icon = GetNode<Sprite2D>("Icon");

        _exclamation = GetNode<Sprite2D>("Exclamation");
        _exclamation.Modulate = new Color(1, 1, 1, 0); // Fully transparent

        _fillStyleBox = (StyleBoxFlat)GetThemeStylebox("fill").Duplicate();
        AddThemeStyleboxOverride("fill", _fillStyleBox);
    }

    public override void _Process(double delta) {
        if (_buffer != null) {
            double newValue;
            if (_buffer.Quantity == 0) {
                newValue = 0;
            } else if (Resources.IsFloating(_buffer.Resource)) {
                // floating is either empty or full
                newValue = (_buffer.Quantity >= _buffer.MaxQuantity) ? 100.0f : 0;
            } else {
                newValue = (_buffer.Quantity / _buffer.MaxQuantity * 100.0);
            }
            if (Value != newValue) {
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(this, "value", newValue, 0.2f);
            }

            if (Resources.IsCritical(_buffer.Resource) && Value <= MinValue + Step) {
                BlinkExclamation();
            }
        }
        if (_storageContainer != null) {
            double newValue;
            if (_storageContainer.Quantity == 0) {
                newValue = 0;
            } else if (Resources.IsFloating(_storageContainer.Resource)) {
                // floating is either empty or full
                newValue = (_storageContainer.Quantity >= _storageContainer.MaxQuantity) ? 100.0f : 0;
            } else {
                newValue = (_storageContainer.Quantity / _storageContainer.MaxQuantity * 100.0);
            }
            if (Value != newValue) {
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(this, "value", newValue, 0.2f);
            }
        }
    }

    public void SetStatusFromMachineBuffer(MachineBuffer buffer) {

        if (buffer.Resource == Resource.Unset) {
            _icon.Visible = false;
            Value = 0;
            return;
        }

        _icon.Visible = true;
        _icon.Texture = Resources.GetResourceIcon(buffer.Resource);
        _fillStyleBox.BgColor = Resources.GetResourceColor(buffer.Resource);

        double newValue = 0.0;
        if (buffer.Quantity == 0) {
            newValue = 0.0;
        } else {
            newValue = (buffer.Quantity / buffer.MaxQuantity * 100.0);
        }

        if (Value != newValue) {
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(this, "value", newValue, 0.2f);
        }

        _buffer = buffer;
    }

    public void SetStatusFromStorageContainer(StorageContainer container) {

        if (container.Resource == Resource.Unset) {
            _icon.Visible = false;
            Value = 0;
            return;
        }

        _icon.Visible = true;
        _icon.Texture = Resources.GetResourceIcon(container.Resource);
        _fillStyleBox.BgColor = Resources.GetResourceColor(container.Resource);

        double newValue = 0.0;
        if (container.Quantity == 0) {
            newValue = 0.0;
        } else {
            newValue = (container.Quantity / container.MaxQuantity * 100.0);
        }

        if (Value != newValue) {
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(this, "value", newValue, 0.2f);
        }

        _storageContainer = container;
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
