using Godot;
using System;

/// connections are bi-directional; we transfer from machine a to b and from b to a at the same time
public partial class Connection {
    public Machine MachineA;
    public Machine MachineB;

    public Connection(Machine a, Machine b) {
        MachineA = a;
        MachineB = b;
    }
}
