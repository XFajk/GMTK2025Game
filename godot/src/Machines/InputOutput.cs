using Godot;
using System;

/// input or output resource container of a machine
public partial class InputOutput : Node {
    [Export]
    public Resource Resource;

    [Export]
    public int MaxQuantity;

    [Export]
    /// change caused by a single execution cycle of the machine
    public int QuantityChangeInReceipe;

    public int Quantity;
}
