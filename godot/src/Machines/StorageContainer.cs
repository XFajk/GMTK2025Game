using Godot;
using System;
using System.Collections.Generic;

public partial class StorageContainer : Connectable, IContainer {
    [Export]
    public Resource Resource;

    [Export]
    public int MaxQuantity;

    // floating-point to avoid rounding errors
    public float Quantity;

    public override IEnumerable<IContainer> Inputs() {
        yield return this;
    }

    public override IEnumerable<IContainer> Outputs() {
        yield return this;
    }

    public int GetMaxQuantity() => MaxQuantity;

    public float GetQuantity() => Quantity;

    public Resource GetResource() => Resource;

    public void SetQuantity(float newValue) => Quantity = newValue;

    string IContainer.GetName() => Name;

}
