using Godot;
using System;
using System.Collections.Generic;

public partial class StorageContainer : Connectable, IContainer {
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
        base._Ready();
        Contents = new() {
            Resource = Resource,
            MaxQuantity = MaxQuantity
        };
        AddChild(Contents);
    }

    public int GetMaxQuantity() => MaxQuantity;

    public float GetQuantity() => Contents.GetQuantity();

    public Resource GetResource() => Resource;

    public void SetQuantity(float newValue) => Contents.SetQuantity(newValue);

    string IContainer.GetName() => GetName();

}
