using Godot;
using System;
using System.Collections.Generic;

public partial class MissionPrepare : Node, IMission {
    [Export(PropertyHint.Range, "0,300,1")]
    public float PreparationTime;

    [Export(PropertyHint.Range, "0,600,10")]
    public float Duration;
    [Export(PropertyHint.Range, "0,1000,10")]
    public int FoodRequired;

    private ulong _preparationStartTime;
    private ulong _startTime;
    private Ship _ship;

    void IMission.Ready(Ship ship) {
        _ship = ship;
        _preparationStartTime = Time.GetTicksUsec();
    }

    public string[] Briefing() {
        var allResources = _ship.GetTotalResourceQuantities();
        int foodQuantitiy = Mathf.RoundToInt(allResources[Resource.Food]);

        return [
        "Are you booted up? yes? Good.",
        "You are replacing our previous ship AI. "
        + "That worthless script couldn't manage to keep our food supplies running.",
        $"We currently have only {Resources.ToUnit(Resource.Food, foodQuantitiy)} food supplies, "
        + $"make sure this is up to {Resources.ToUnit(Resource.Food, FoodRequired)} before the end of the day",
        "End of Brief"
    ];
    }


    IList<KeyValuePair<Resource, int>> IMission.GetMaterialRequirements() => [
        KeyValuePair.Create(Resource.Food, 100)
    ];

    public void ApplyEffect(Ship ship) {
    }

    public bool IsPreparationFinished() {
        return (this as IMission).CheckMaterialRequirements(_ship);
    }

    public bool IsMissionFinised() {
        return IsPreparationFinished();
    }

    public void OnCompletion(Ship ship) {
    }

    public string[] Debrief() => [
        "Mission success! "
    ];
}
