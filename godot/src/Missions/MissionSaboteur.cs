using Godot;
using System;
using System.Collections.Generic;

// ඞ
public partial class MissionSaboteur : Node, IMission {
    private const float CrewGatherOffset = 0.8f;

    public IMission.Properties Properties;
    [Export]
    private Node3D GatherLocation;
    [Export]
    private int GatherTimeSeconds;
    [Export]
    private Node3D Airlock;

    private bool _isFinished = false;
    private MissionManager.Clock _missionClock;
    private double _startTime;


    void IMission.MissionReady(Ship ship, MissionManager.Clock missionClock) {
        Properties = new() {
            Title = "Mission: Saboteur",
            Briefing = [
                "I suspect that one of our crew is sabotaging our mission.",
                "Don't worry, we will try to find the impostor ourselves.",
                "EMERGENCY MEETING!"
            ],
            Debrief = [
                "It looks like they've thrown me out.",
                "Oh well.",
                "Good luck."
            ],
        };

        _missionClock = missionClock;
        _startTime = _missionClock.time;
    }

    public IMission.Properties GetMissionProperties() => Properties;

    public void OnStart(Ship ship) {
        EverybodyGoTo(GatherLocation.GlobalPosition, ship);

        Tween tween = GetTree().CreateTween();
        tween.TweenCallback(Callable.From(() => EverybodyGoTo(Airlock.GlobalPosition, ship))).SetDelay(20);
        tween.TweenCallback(Callable.From(() => _isFinished = true)).SetDelay(13);
    }

    private static void EverybodyGoTo(Vector3 location, Ship ship) {
        location.X -= (ship.Crew.Count / 2f) * CrewGatherOffset;

        foreach (Person crewMember in ship.Crew) {
            ship.ScheduleCrewTask(
                new CrewTask() {
                    Location = location,
                    ActionType = CrewTask.Type.JustStandThere,
                    Duration = float.MaxValue
                },
                crewMember
            );
            // make sure they do not all clump up
            location.X += CrewGatherOffset;
        }
    }

    public void OnCompletion(Ship ship) {
        Person captain = null;
        foreach (Person crewMember in ship.Crew) {
            crewMember.SetCurrentTask(null);
            if (crewMember.IsCaptain) {
                captain = crewMember;
            }
        }

        // toss the captain out of the airlock
        captain.Position = Airlock.Position + new Vector3(0, 0, -5);
        captain.state = Person.State.Floating;
        Tween captaintween = GetTree().CreateTween();
        captaintween.TweenProperty(captain, "position", new Vector3(-50, 30, -10), 60);
        captaintween.TweenCallback(Callable.From(() => captain.QueueFree()));
    }

    public virtual bool IsPreparationFinished() => true;

    public bool IsMissionFinised() => _isFinished;

    public bool IsDelayed() => false;
}
