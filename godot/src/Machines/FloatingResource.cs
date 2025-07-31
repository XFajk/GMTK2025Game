using Godot;
using System;

/// air in the ship
public partial class FloatingResource : Node, IContainer {
    [Export]
    public Resource Resource;

    [Export]
    public int MaxQuantity = 1000;

    // floating-point to avoid rounding errors
    public float Quantity;

    public int GetMaxQuantity() => MaxQuantity;

    public float GetQuantity() => Quantity;

    public Resource GetResource() => Resource;

    public void SetQuantity(float newValue) => Quantity = newValue;
    
    string IContainer.GetName() => GetName();
}
