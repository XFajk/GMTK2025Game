using Godot;
using System;

/// input or output of a process
public partial class InputOutput : Node {
    [Export]
    public Node Source;

    [Export]
    /// change caused by a single execution cycle of the machine
    public int QuantityChangeInReceipe;

    public IContainer Container;

    public override void _Ready() {
        Container = (IContainer)Source;
    }
}
