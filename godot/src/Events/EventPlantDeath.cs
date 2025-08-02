using Godot;
using System;
using System.Collections.Generic;

public partial class EventPlantDeath : Node, IEvent {
    [Export(PropertyHint.Range, "0, 100, 5")]
    public int percentageLost = 50;
    [Export]
    public Plants Greenhouse;

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"A disease has ravaged the plants in the greenhouse! {percentageLost}% of our plants have died.",
        IconPosition = Greenhouse.Position,
    };

    public void ApplyEffect(Ship ship) {
        Greenhouse.PlantHealth *= (percentageLost / 100.0f);
    }
}
