using Godot;
using System;

/// air in the ship
public partial class FloatingResource : Node {
    [Export]
    public Resource Resource;

    [Export]
    public int MaxQuantity;

    // floating-point to avoid rounding errors
    public float Quantity;
}
