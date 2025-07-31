using Godot;
using System;
using System.Collections.Generic;

public partial class TemplateEvent : Node3D, IEvent {
    public void ApplyEffect(List<Machine> machines, List<CrewMember> crew) {
        // delete every unhandled output
        foreach (Machine machine in machines) {
            foreach (InputOutput output in machine.Outputs) {
                output.Quantity = 0;
            }
        }
    }
}
