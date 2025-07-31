using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Connectable : Node3D {
    [Signal]
    // Handled by Game.cs
    public delegate void OnConnectionClickEventHandler(Machine machine, ConnectionNode point);

    public void HandleConnectionClicked(ConnectionNode connection) {
        EmitSignal(SignalName.OnConnectionClick, this, connection);
    }

    public abstract IEnumerable<InputOutput> Inputs();
    public abstract IEnumerable<InputOutput> Outputs();
}
