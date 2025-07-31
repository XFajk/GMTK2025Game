using Godot;
using System;

public partial class EventScheduleEntry : Node {
    [Export]
    public float EventTriggerTime;
    [Export]
    public Node EventToFire;

    public IEvent Event => (IEvent)EventToFire;
}
