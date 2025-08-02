using Godot;
using System;
using System.Collections.Generic;

public partial class MissionIceAstroid : Node, IMission {
    // measured in 0.1% per second
    private const float CarbonDumpPerSecond = 10f;

    [Export(PropertyHint.Range, "0,300,1")]
    public int PreparationTime = 10;

    [Export(PropertyHint.Range, "0,600,10")]
    public int Duration;

    [Export]
    public int RequiredDisposables;
    [Export(PropertyHint.Range, "0,6")]
    public int CrewCount;
    [Export]
    public Node3D AirLock;

    public float OxygenDrainPerSecond;
    public float OxygenDrainTotal;
    public float CarbonDioxideReturnQuantity;

    private ulong _preparationStartTime;
    private ulong _startTime;

    public IMission.Properties Properties;
    private Ship _ship;
    private List<Engine> _engines = new();

    void IMission.MissionReady(Ship ship) {
        _ship = ship;
        var carbonToOxygen = Resources.GetRatio(Resource.CarbonDioxide, Resource.Oxygen);
        var disposablesToCarbon = Resources.GetRatio(Resource.Disposables, Resource.CarbonDioxide);

        CarbonDioxideReturnQuantity = RequiredDisposables * disposablesToCarbon.Key / disposablesToCarbon.Value;
        OxygenDrainTotal = (CarbonDioxideReturnQuantity * carbonToOxygen.Key / carbonToOxygen.Value);
        OxygenDrainPerSecond = OxygenDrainTotal / Duration;

        _preparationStartTime = Time.GetTicksUsec();

        Properties = new() {
            Title = "Mission: Astroid mining",
            Briefing = [
                "Before its... interruption, the scan picked up a nearby astroid. We will be scanning the astroid for its material composition."
                + $"For this mission we will need {Resources.ToUnit(Resource.Disposables, RequiredDisposables)} materials from the carbon scrubber, "
                + $"and during the operation we will channel an additional {Resources.ToUnit(Resource.Oxygen, Mathf.CeilToInt(OxygenDrainPerSecond * 60))} oxygen per minute to the crew outside the ship."
                + $"The operation begins in {PreparationTime} seconds, make sure the supplies are available then",
                "Naturally, the engine will be turned off for the duration",
                "End of Brief"
            ],
            ResourceMinimumRequirements = [KeyValuePair.Create(Resource.Disposables, RequiredDisposables)],
            Debrief = [
                "Mission completed! "
                + $"We will be emptying our cylinders of carbon dioxide for the next {(int) CarbonDioxideReturnQuantity / CarbonDumpPerSecond} seconds or so."
            ]
        };

        foreach (Machine m in ship.Machines) {
            if (m is Engine e) {
                _engines.Add(e);
            }
        }
    }
    
    public IMission.Properties GetMissionProperties() => Properties;

    public void OnStart(Ship ship) {
        _startTime = Time.GetTicksUsec();
        foreach (var pair in Properties.ResourceMinimumRequirements) {
            ship.RemoveResource(pair.Key, pair.Value);
        }

        for (int i = 0; i < CrewCount; i++) {
            ship.ScheduleCrewTask(new CrewTask() {
                Duration = Duration,
                Location = AirLock.Position
            });
        }

        ship.ActiveEffects.Add(new EventEffectResourceAdd() {
            AdditionPerSecond = -OxygenDrainPerSecond,
            MaxResourcesToAdd = OxygenDrainTotal,
            Target = ship.GetFloatingResource(Resource.Oxygen)
        });

        foreach (Engine e in _engines) {
            e.EnginePower = 0;
        }
    }

    public bool IsPreparationFinished() {
        double timePassed = (double)(Time.GetTicksUsec() - _preparationStartTime) / 1E6;
        // delay finished until we have the materials
        return _preparationStartTime != 0 && timePassed > PreparationTime && (this as IMission).CheckMaterialRequirements(_ship);
    }

    public bool IsMissionFinised() {
        double timePassed = (double)(Time.GetTicksUsec() - _startTime) / 1E6;
        return _startTime != 0 && timePassed > Duration;
    }

    public void OnCompletion(Ship ship) {
        ship.ActiveEffects.Add(new EventEffectResourceAdd() {
            AdditionPerSecond = CarbonDumpPerSecond,
            MaxResourcesToAdd = CarbonDioxideReturnQuantity,
            Target = ship.GetFloatingResource(Resource.CarbonDioxide)
        });

        foreach (Engine e in _engines) {
            e.EnginePower = Engine.DefaultEnginePower;
        }
    }
}
