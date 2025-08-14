using Godot;
using System;

/// input or output resource container of a machine
public partial class MachineBuffer : Node, IContainer {
    [Export]
    public Resource Resource;

    [Export]
    public int MaxQuantity;

    [Export]
    /// change caused by a single execution cycle of the machine
    public int QuantityChangeInReceipe;

    [Export]
    public bool Critical = false;

    // floating-point to avoid rounding errors
    public float Quantity;

    public override void _Ready() {
        if (Resource != Resource.Unset && GetParent() != null) {
            if (GetParent().Name == "Inputs") {
                Name = Resource.ToString() + "Input";
            } else if (GetParent().Name == "Outputs") {
                Name = Resource.ToString() + "Output";
            }
        }

        if (Resources.IsFloating(Resource) && MaxQuantity == 0) {
            MaxQuantity = QuantityChangeInReceipe + 1;
        }
    }


    public int GetMaxQuantity() => MaxQuantity;

    public float GetQuantity() => Quantity;

    public Resource GetResource() => Resource;

    public void SetQuantity(float newValue) => Quantity = newValue;

    string IContainer.GetName() => Name;
}
