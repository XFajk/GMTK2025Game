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

    public float OxygenDrainPerSecond;
    public float OxygenDrainTotal;
    public float CarbonDioxideReturnQuantity;

    private ulong _preparationStartTime;
    private ulong _startTime;

    void IMission.Ready() {
        float carbonToOxygen = Resources.GetRatio(Resource.CarbonDioxide, Resource.Oxygen);
        float disposablesToCarbon = Resources.GetRatio(Resource.Disposables, Resource.CarbonDioxide);

        CarbonDioxideReturnQuantity = RequiredDisposables * disposablesToCarbon;
        OxygenDrainTotal = (CarbonDioxideReturnQuantity * carbonToOxygen);
        OxygenDrainPerSecond = OxygenDrainTotal / Duration;

        _preparationStartTime = Time.GetTicksUsec();
    }

    public string[] Briefing() => [
        "Today we will be running an astroid minig operation. "
        + $"For this mission we will need {Resources.ToUnit(Resource.Disposables, RequiredDisposables)} plastic materials, "
        + $"and the operation will consume up to {Resources.ToUnit(Resource.Oxygen, Mathf.CeilToInt(OxygenDrainPerSecond * 60))} oxygen per minute"
        + $"The operation begins in {PreparationTime} seconds",
        "End of Brief"
    ];

    IList<KeyValuePair<Resource, int>> IMission.GetMaterialRequirements() => [KeyValuePair.Create(Resource.Disposables, RequiredDisposables)];

    public void ApplyEffect(Ship ship) {
        foreach (var pair in (this as IMission).GetMaterialRequirements()) {
            ship.RemoveResource(pair.Key, pair.Value);
        }
        _startTime = Time.GetTicksUsec();
    
        ship.ActiveEffects.Add(new EventEffectResource() {
            AdditionPerSecond = -OxygenDrainPerSecond,
            MaxResourcesToAdd = OxygenDrainTotal,
            Target = ship.GetFloatingResource(Resource.Oxygen)
        });
    }

    public bool IsPreparationFinished() {
        double timePassed = (double)(Time.GetTicksUsec() - _preparationStartTime) / 1E6;
        return _preparationStartTime != 0 && timePassed > PreparationTime;
    }

    public bool IsMissionFinised() {
        double timePassed = (double)(Time.GetTicksUsec() - _startTime) / 1E6;
        return _startTime != 0 && timePassed > Duration;
    }

    public void OnCompletion(Ship ship) {
        ship.ActiveEffects.Add(new EventEffectResource() {
            AdditionPerSecond = CarbonDumpPerSecond,
            MaxResourcesToAdd = CarbonDioxideReturnQuantity,
            Target = ship.GetFloatingResource(Resource.CarbonDioxide)
        });
    }

    public string[] Debrief() => [
        "Mission success! "
        + $"We will be emptying our cylinders of carbon dioxide for the next {(int) CarbonDioxideReturnQuantity / CarbonDumpPerSecond} seconds or so."
    ];
}
