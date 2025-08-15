using Godot;
using System;
using System.Collections.Generic;

// ඞ
public partial class MissionSaboteur : Node, IMission {
    public IMission.Properties Properties;
    [Export(PropertyHint.Range, "0,60")]
    public int TimeUntilEmergencyMeeting = 10;
    [Export]
    private Node3D GatherLocation;
    [Export]
    private int GatherTimeSeconds;
    [Export]
    private Node3D Airlock;
    [Export]
    private Node3D OutOfTheAirlock;

    private bool _isFinished = false;
    private MissionManager.Clock _missionClock;
    private double _startTime;

    void IMission.MissionReady(Ship ship, MissionManager.Clock missionClock) {
        Properties = new() {
            Title = "Mission: Saboteur",
            Briefing = [
                "I suspect that one of our crew is sabotaging our mission. ",
                "Don't worry, we will try to find the impostor ourselves.",
                "End of Brief"
            ],
            Debrief = [
                "Well, It's been a pleasure"
            ],
        };

        _missionClock = missionClock;
        _startTime = _missionClock.time;
    }

    public IMission.Properties GetMissionProperties() => Properties;

    public void OnStart(Ship ship) {
        EverybodyGoTo(GatherLocation.GlobalPosition, ship);

        Tween tween = GetTree().CreateTween();
        tween.TweenCallback(Callable.From(() => EverybodyGoTo(Airlock.GlobalPosition, ship))).SetDelay(5);
        tween.TweenCallback(Callable.From(() => _isFinished = true)).SetDelay(2);
    }

    private static void EverybodyGoTo(Vector3 location, Ship ship) {
        foreach (Person crewMember in ship.Crew) {
            ship.ScheduleCrewTask(
                new CrewTask() {
                    Location = location,
                    ActionType = CrewTask.Type.JustStandThere,
                    Duration = float.MaxValue
                },
                crewMember
            );
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
        captain.Position -= new Vector3(0, 0, 20);
        captain.state = Person.State.Floating;
        Tween captaintween = GetTree().CreateTween();
        captaintween.TweenProperty(captain, "position", OutOfTheAirlock.Position, 5);
        captaintween.TweenCallback(Callable.From(() => captain.QueueFree()));
    }

    public virtual bool IsPreparationFinished() {
        double timePassed = (double)(_missionClock.time - _startTime) / 1E6;
        return _startTime != 0 && timePassed > TimeUntilEmergencyMeeting;
    }

    public bool IsMissionFinised() => _isFinished;

    public bool IsDelayed() => false;
}
