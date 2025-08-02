using Godot;
using System;
using System.Collections.Generic;

public abstract partial class TimedMission : Node, IMission {
    [Export(PropertyHint.Range, "0,60")]
    public int PreparationTime = 10;

    [Export(PropertyHint.Range, "0,60")]
    public int Duration;
    
    protected ulong _preparationStartTime;
    protected ulong _startTime;

    public virtual void MissionReady(Ship ship) {
        _preparationStartTime = Time.GetTicksUsec();
    }

    public virtual void OnStart(Ship ship) {
        _startTime = Time.GetTicksUsec();
    }

    public virtual bool IsPreparationFinished() {
        double timePassed = (double)(Time.GetTicksUsec() - _preparationStartTime) / 1E6;
        return _preparationStartTime != 0 && timePassed > PreparationTime;
    }

    public virtual bool IsMissionFinised() {
        double timePassed = (double)(Time.GetTicksUsec() - _startTime) / 1E6;
        return _startTime != 0 && timePassed > Duration;
    }

    public abstract IMission.Properties GetMissionProperties();
    public abstract void OnCompletion(Ship ship);
}
