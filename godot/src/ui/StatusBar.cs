using Godot;
using System;

public partial class StatusBar : ProgressBar {

    private Sprite2D _icon;
    private StyleBoxFlat _fillStyleBox;
    private MachineBuffer _buffer;
    private StorageContainer _storageContainer;

    public override void _Ready() {
        _icon = GetNode<Sprite2D>("Icon");
        _fillStyleBox = (StyleBoxFlat)GetThemeStylebox("fill").Duplicate();
        AddThemeStyleboxOverride("fill", _fillStyleBox);
    }

    public override void _Process(double delta) {
        if (_buffer != null) {
            double newValue = 0;
            if (_buffer.Quantity == 0) {
                newValue = 0;
            } else {
                newValue = (_buffer.Quantity / _buffer.MaxQuantity * 100.0);
            }
            if (Value != newValue) {
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(this, "value", newValue, 0.2f);
            }
        }
        if (_storageContainer != null) {
            double newValue = 0;
            if (_storageContainer.Quantity == 0) {
                newValue = 0;
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
}
