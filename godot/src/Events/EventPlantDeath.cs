using Godot;
using System;
using System.Collections.Generic;

public partial class EventPlantDeath : Node3D, IEvent {
    [Export(PropertyHint.Range, "0, 100, 1")]
    public int percentageOfMaxLost = 50;

    public void ApplyEffect(Ship ship) {
        foreach (Machine m in ship.Machines) {
            // if (m is Plant p) {
            //     p.plants *= (percentageOfMaxLost / 100.0f);
            // }
        }
    }
}
