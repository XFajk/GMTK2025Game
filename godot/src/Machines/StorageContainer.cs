using Godot;
using System;
using System.Collections.Generic;

public partial class StorageContainer : Connectable {
    public InputOutput Contents { get; private set; }

    public override IEnumerable<InputOutput> Inputs() {
        yield return Contents;
    }

    public override IEnumerable<InputOutput> Outputs() {
        yield return Contents;
    }
}
