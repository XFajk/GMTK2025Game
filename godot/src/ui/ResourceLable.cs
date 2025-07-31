using Godot;
using System;

public partial class ResourceLable : Control {
    [Export]
    public Resource Resource { get; private set; }

    private Label _amountLabel;

    public override void _Ready() {
        _amountLabel = GetNode<Label>("ResourceName/Amount");
    }

    public void SetAmount(int amount) {
        _amountLabel.Text = Resources.ToUnit(Resource, amount);
    }
}
