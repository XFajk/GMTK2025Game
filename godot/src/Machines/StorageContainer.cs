using Godot;
using System;
using System.Collections.Generic;

public partial class StorageContainer : Connectable {
    [Export]
    public Resource Resource;

    [Export]
    public int MaxQuantity;

    public InputOutput Contents { get; private set; }

    public override IEnumerable<InputOutput> Inputs() {
        yield return Contents;
    }

    public override IEnumerable<InputOutput> Outputs() {
        yield return Contents;
    }

    public override void _Ready() {
        Contents = new() {
            Resource = Resource,
            MaxQuantity = MaxQuantity
        };
        AddChild(Contents);
    }
}
