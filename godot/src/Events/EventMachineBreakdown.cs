using Godot;

public partial class EventMachineBreakdown : Node, IEvent {
    [Export]
    public float SecondsToRepair = 5;

    [Export]
    public Machine Target;

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"{Target.Name} has broken down. A crewmember is on its way to repair it.",
        IconPosition = Target.Position,
    };

    public void ApplyEffect(Ship ship) {
        Target.IsWorking = false;
        ScheduleRepair(ship);
    }

    private void ScheduleRepair(Ship ship) {
        ship.ScheduleCrewTask(new CrewTask() {
            Location = Target.Position,
            Duration = SecondsToRepair,
            OnTaskComplete = (p => Target.IsWorking = true),
            OnTaskAbort = (p => ScheduleRepair(ship))
        });
    }

}