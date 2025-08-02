
using System.Collections.Generic;
using Godot;

public partial class MissionFireRepair : Node, IMission {
    public Resource Resource = Resource.Disposables;
    public int Quantity;
    public Vector3 Location;
    public int SecondsToRepair;

    private Ship _ship;
    private bool _repairCompleted;

    public IMission.Properties Properties;

    void IMission.MissionReady(Ship ship) {
        _ship = ship;
        Properties = new() {
            Title = "Mission: Reparations",
            Briefing = [
                $"We will need to repair the fire damage. "
                + $"Prepare materials by collecting {Quantity} {Resource}"
            ],
            Debrief = [
                "The fire dmange has been repaired."
            ],
            ResourceMinimumRequirements = [KeyValuePair.Create(Resource, Quantity)],
        };
    }

    public IMission.Properties GetMissionProperties() => Properties;

    public void OnStart(Ship ship) {
        foreach (var pair in Properties.ResourceMinimumRequirements) {
            ship.RemoveResource(pair.Key, pair.Value);
        }
        ScheduleRepair(ship);
    }

    private void ScheduleRepair(Ship ship) {
        ship.ScheduleCrewTask(new CrewTask() {
            Location = Location,
            Duration = SecondsToRepair,
            OnTaskComplete = (p => _repairCompleted = true),
            OnTaskAbort = (p => ScheduleRepair(ship))
        });
    }


    public bool IsPreparationFinished() => (this as IMission).CheckMaterialRequirements(_ship);

    public bool IsMissionFinised() => _repairCompleted;

    public void OnCompletion(Ship ship) { }
}