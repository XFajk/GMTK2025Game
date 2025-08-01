using Godot;

public partial class EventMachineBreakdown : Node3D, IEvent {
    [Export]
    public float SecondsToRepair = 5;

    [Export]
    public Machine Target;

    public void ApplyEffect(Ship ship) {
        Target.IsWorking = false;

        // ship.AddCrewTask(new CrewTaskRepair() {
        //     Target = this,
        //     Duration = SecondsToRepair
        // });
    }
}