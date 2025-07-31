using Godot;
using System;

/// connections are bi-directional; we transfer from machine a to b and from b to a at the same time
public partial class Connection {
    public Connectable A;
    public Connectable B;

    public Connection(Connectable a, Connectable b) {
        A = a;
        B = b;
    }
}
