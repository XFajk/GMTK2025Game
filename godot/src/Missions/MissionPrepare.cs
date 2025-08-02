using Godot;
using System;
using System.Collections.Generic;

/// Essentially the tutorial.
/// Garbage must be all over the floor, Plants must have water, and CO2 must be 0%.
/// Putting even one garbage in the GarbageBurner should add some CO2 into the ship, causing the plants to unblock, and make food.
/// This should be sufficient to get an 'aha' from the player before we explain the systems in the Debrief
public partial class MissionPrepare : Node, IMission {
    [Export(PropertyHint.Range, "0,1000,10")]
    public int FoodRequired;
    [Export(PropertyHint.Range, "0,100")]
    public int GarbageMax;
    public IMission.Properties Properties;

    private Ship _ship;

    void IMission.MissionReady(Ship ship) {
        _ship = ship;
        var allResources = _ship.GetTotalResourceQuantities();
        int foodQuantitiy = Mathf.RoundToInt(allResources[Resource.Food]);
        Properties = new() {
            Title = "Mission: System rebalance",
            Briefing = [
                "Are you booted up? yes? Good.",
                "We are having difficulties with our food situation. "
                + $"Our plants are dying, even though they have enough water, and now we have only {Resources.ToUnit(Resource.Food, foodQuantitiy)} of food supplies left.",
                $"Figure out why the plants are dying and make sure we have {Resources.ToUnit(Resource.Food, FoodRequired)} of food ASAP. ",
                "While you're at it, clean up the floors as well. "
                + "Just toss the garbage in the Incinterator.",
            ],
            Debrief = [
                "Mission success! The plants are growing again and we have food!",
                "You see, this spaceship is a closed cycle, and any resources lost are lost forever. "
                + "The machines on this ship are lossless, so everything spent will eventually loop back to the inputs",
                "With you in control of the ship's resource cycles I am confident that we can continue our mission!",
                "You'll hear back from me shortly"
            ],
            ResourceMinimumRequirements = [KeyValuePair.Create(Resource.Food, FoodRequired)],
            ResourceMaximumRequirements = [KeyValuePair.Create(Resource.Garbage, GarbageMax)]
        };
    }
    
    public IMission.Properties GetMissionProperties() => Properties;

    public void OnStart(Ship ship) {
    }

    public bool IsPreparationFinished() {
        return (this as IMission).CheckMaterialRequirements(_ship);
    }

    public bool IsMissionFinised() {
        return IsPreparationFinished();
    }

    public void OnCompletion(Ship ship) {
    }
}
