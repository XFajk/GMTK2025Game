
using System.Collections.Generic;
using Godot;

public partial class MissionFireRepair : Node, IMission {
    public Resource Resource = Resource.Disposables;
    public int Quantity;
    public Vector3 Location;
    public float SecondsToRepair;

    private Ship _ship;
    private bool _repairCompleted;

    public void ApplyEffect(Ship ship) {
        _ship = ship;
        ship.ScheduleCrewTask(new CrewTask() {
            Location = Location,
            Duration = SecondsToRepair,
            OnTaskComplete = (p => _repairCompleted = true)
        });
    }

    IList<KeyValuePair<Resource, int>> IMission.GetMaterialRequirements() => [
        new(Resource, Quantity)
    ];

    public string[] Briefing() => [
        $"We will need to repair the fire. "
        + $"Prepare materials by collecting {Quantity} {Resource}"
    ];

    public string[] Debrief() => [
        "The fire has been repaired."
    ];

    public bool IsPreparationFinished() => _ship.HasResource(Resource, Quantity);

    public bool IsMissionFinised() => _repairCompleted;

    public void OnCompletion(Ship ship) {}
}