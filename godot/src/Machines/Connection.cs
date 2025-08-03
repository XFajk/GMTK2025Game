using Godot;
using System;

/// connections are bi-directional; we transfer from machine a to b and from b to a at the same time
public partial class Connection {
    public Connectable aMachine;
    public ConnectionNode aNode;
    public Connectable bMachine;
    public ConnectionNode bNode;

    public Connection(Connectable aMachine, ConnectionNode aNode, Connectable bMachine, ConnectionNode bNode) {
        this.aMachine = aMachine;
        this.aNode = aNode;
        this.bMachine = bMachine;
        this.bNode = bNode;
    }
}
