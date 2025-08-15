using Godot;
using System;

public partial class Satisfaction : Node {
    [Export]
    public int MaximumSatisfaction = 10000;
    [Export]
    public int CrewFoodOxygenPenalty = 1;
    [Export]
    public int CrewGarbagePenalty = 1;
    [Export]
    public int EnginePenalty = 1;
    [Export]
    public int MissionDelayPenalty = 1;
    [Export]
    public int MissionCompleteReward = 1000;

    private double _value;

    public override void _Ready() {
        _value = MaximumSatisfaction;
    }

    public float GetSatisfactionLevel() {
        if (_value < 0) return 0;
        return (float)_value / MaximumSatisfaction;
    }

    public void CheckProcessFailure(Process failedProcess, double deltaTime) {
        if (failedProcess.Name == "CrewGarbageProcess") {
            TriggerCrewGarbageProcessFailure(deltaTime);
        } else if (failedProcess.Name == "CrewFoodProcess") {
            TriggerCrewFoodProcessFailure(deltaTime);
        }
    }

    public void CheckMachineFailure(Machine failedMachine, double deltaTime) {
        if (failedMachine is Engine) {
            TriggerEngineProcessFailure(deltaTime);
        }
    }

    public void CheckMissionComplete(IMission mission) {
        if (mission is not MissionFireRepair) TriggerMissionComplete();
    }

    public void TriggerCrewFoodProcessFailure(double deltaTime) {
        _value -= CrewFoodOxygenPenalty * deltaTime;
    }

    public void TriggerCrewGarbageProcessFailure(double deltaTime) {
        _value -= CrewGarbagePenalty * deltaTime;
    }

    public void TriggerEngineProcessFailure(double deltaTime) {
        _value -= EnginePenalty * deltaTime;
    }

    public void TriggerMissionDelay(double deltaTime) {
        _value -= MissionDelayPenalty * deltaTime;
    }

    public void TriggerMissionComplete() {
        _value += MissionCompleteReward;
    }

}
