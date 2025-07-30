using Godot;
using System;

public partial class InputOutput : Node {
    [Export]
    public Resource Resource;

    [Export]
    public int Quantity;

    [Export]
    public int MaxQuantity;

    [Export]
    /// change caused by a single execution cycle of the machine
    public int QuantityChangeInReceipe;
}
