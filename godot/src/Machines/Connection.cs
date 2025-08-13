using Godot;
using System;

/// connections are bi-directional; we transfer from machine a to b and from b to a at the same time
public partial class Connection {
    public Connectable aMachine;
    public Connectable bMachine;

    public Connection(Connectable aMachine, Connectable bMachine) {
        this.aMachine = aMachine;
        this.bMachine = bMachine;
    }
}
