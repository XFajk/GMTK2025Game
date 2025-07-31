using Godot;
using System;

public partial class Connection {

    public Machine MachineA;
    public Machine MachineB;
    
    public Connection(Machine a, Machine b) {
        MachineA = a;
        MachineB = b;
    }

}
