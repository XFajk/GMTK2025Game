
using System.Collections.Generic;
using Godot;

// This mission is supposed to be combined with an EventFire
public partial class MissionHighPowerScan : TimedMission {
    [Export]
    public string TargetSolarSystemName = "\b";
    public IMission.Properties Properties;

    private List<Machine> _disabledMachines = new();

    public override void MissionReady(Ship ship) {
        base.MissionReady(ship);

        Properties = new() {
            Title = "Mission: High power scan",
            Briefing = [
                $"We have arrived in the {TargetSolarSystemName} solar system. "
                + $"in {PreparationTime} seconds we will run a high-power scan, during which all machines will be disabled. "
                + $"This will only take {Duration * 3} seconds. ",
                "End of Brief"
            ],
            Debrief = [
                "So the scan was... Prematurely ended... "
                + "We will have to explore this solar system the old-fashioned way. "
            ],
        };
    }

    public override void OnStart(Ship ship) {
        base.OnStart(ship);
        foreach (Machine m in ship.Machines) {
            if (m is not Plants) {
                m.IsWorking = false;
                _disabledMachines.Add(m);
            }
        }
    }


    public override IMission.Properties GetMissionProperties() => Properties;

    public override void OnCompletion(Ship ship) {
        _disabledMachines.ForEach(m => m.IsWorking = true);
    }
}
