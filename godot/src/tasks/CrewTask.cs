using System;
using Godot;

public class CrewTask {
    public Vector3 Location;
    public float Duration;
    public Type ActionType;

    public Action<Person> OnTaskComplete = p => { };
    public Action<Person> OnTaskAbort = p => { };

    public enum Type {
        Repair,
        JustStandThere, // menancingly
        Panic,
        SuitUp,
        SuitDown,
        Working,
    }
}