using Godot;
using System;

/// input or output of a process
public partial class InputOutput : Node {
    [Export]
    public Node Source;

    [Export]
    /// change caused by a single execution cycle of the machine
    public int QuantityChangeInReceipe;

    public IContainer Container = null;

    public override void _Ready() {
        if (Source == null) {
            throw new ArgumentNullException(nameof(Source), Name);
        }
        if (Source is not IContainer c) {
            throw new ArgumentOutOfRangeException(nameof(Source), Source.GetType(), Name);
        }

        Container = c;

        if (GetParent().Name == "Inputs") {
            Name = Container.GetName() + "Input";
        } else if (GetParent().Name == "Outputs") {
            Name = Container.GetName() + "Output";
        }
    }
}
