using Godot;
using System;
using System.Collections.Generic;

public partial class MissionEngineRev : TimedMission, IMission {

    [Export(PropertyHint.Range, "0,1,0.05")]
    public float TargetEnginePower = Engine.DefaultEnginePower * 4;
    public float FinalEnginePower;
    public IMission.Properties Properties;

    private List<Engine> _engines = new();

    void IMission.MissionReady(Ship ship) {
        base.MissionReady(ship);
        FinalEnginePower = Mathf.Lerp(Engine.DefaultEnginePower, TargetEnginePower, 0.5f);

        Properties = new() {
            Title = "Mission: Getting around Quicker",
            Briefing = [
                $"We're done crawling around the solar system, we'll be running the engine at {TargetEnginePower * 100} from now on"
            ],
            Debrief = [
                $"So as it turns out, running the engine requires water. We decide to run the engine on {TargetEnginePower * 100} instead, how's that sound "
            ],
        };

        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                _engines.Add(e);
            }
        }
    }

    public override IMission.Properties GetMissionProperties() => Properties;

    public override void OnStart(Ship ship) {
        base.OnStart(ship);
        foreach (Engine e in _engines) {
            e.EnginePower = TargetEnginePower;
        }
    }

    public override void OnCompletion(Ship ship) {
        foreach (Engine e in _engines) {
            e.EnginePower = FinalEnginePower;
        }
    }
}
