using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Ship : Node {
    public List<Machine> Machines;

    public override void _Ready() {
        foreach (Node node in GetChildren()) {
            if (node is Machine machine) {
                Machines.Add(machine);
            }
        }
    }

    public override void _Process(double deltaTime) {
    }
}
