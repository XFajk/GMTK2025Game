using Godot;
using System;
using System.Collections.Generic;

public partial class MissionAstroidMining : Node, IMission {
    // measured in 0.1% per second
    private const float CarbonDumpPerSecond = 10f;

    [Export(PropertyHint.Range, "0,300,1")]
    public float PreparationTime = 10.0f;

    [Export(PropertyHint.Range, "0,600,10")]
    public float Duration;

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

    void IMission.Ready(Ship ship) {
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
                "Today we will be running an astroid mining operation. "
                + $"For this mission we will need {Resources.ToUnit(Resource.Disposables, RequiredDisposables)} plastic materials, "
                + $"and the operation will consume up to {Resources.ToUnit(Resource.Oxygen, Mathf.CeilToInt(OxygenDrainPerSecond * 60))} oxygen per minute"
                + $"The operation begins in {PreparationTime} seconds, make sure the supplies are available then",
                "End of Brief"
            ],
            ResourceMinimumRequirements = [KeyValuePair.Create(Resource.Disposables, RequiredDisposables)],
            Debrief = [
                "Mission completed! "
                + $"We will be emptying our cylinders of carbon dioxide for the next {(int) CarbonDioxideReturnQuantity / CarbonDumpPerSecond} seconds or so."
            ]
        };
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
    }
}
