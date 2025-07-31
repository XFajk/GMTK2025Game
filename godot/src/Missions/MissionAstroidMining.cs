using Godot;
using System;
using System.Collections.Generic;

public partial class MissionAstroidMining : Node3D, IMission {
    // measured in 0.1% per second
    private const float CarbonDumpPerSecond = 10f;

    [Export(PropertyHint.Range, "0,300,1")]
    public float PreparationTime = 10.0f;

    [Export(PropertyHint.Range, "0,600,10")]
    public float Duration;

    [Export]
    public int RequiredDisposables;

    public float OxygenDrainPerSecond;
    public float CarbonDioxideReturnQuantity;

    void IMission.Ready() {
        float carbonToOxygen = Resources.GetRatio(Resource.CarbonDioxide, Resource.Oxygen);
        float disposablesToCarbon = Resources.GetRatio(Resource.Disposables, Resource.CarbonDioxide);

        CarbonDioxideReturnQuantity = RequiredDisposables * disposablesToCarbon;
        OxygenDrainPerSecond = (CarbonDioxideReturnQuantity * carbonToOxygen) / Duration;
    }

    public void ApplyEffect(Ship ship) {
    }

    public string[] Briefing() => [
        "Today we will be running an astroid minig operation. "
        + $"For this mission we will need {Resources.ToUnit(Resource.Disposables, RequiredDisposables)} plastic materials, "
        + $"and the operation will consume up to {Resources.ToUnit(Resource.Oxygen, Mathf.CeilToInt(OxygenDrainPerSecond * 60))} oxygen per minute"
        + $"The operation begins in {PreparationTime} seconds",
        "End of Brief"
    ];

    public float GetDuration() => Duration;

    public float GetPreparationTime() => PreparationTime;

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
