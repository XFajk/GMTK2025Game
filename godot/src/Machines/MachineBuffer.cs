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

    // floating-point to avoid rounding errors
    public float Quantity;

    public int GetMaxQuantity() => MaxQuantity;

    public float GetQuantity() => Quantity;

    public Resource GetResource() => Resource;

    public void SetQuantity(float newValue) => Quantity = newValue;
    
    string IContainer.GetName() => Name;
}
