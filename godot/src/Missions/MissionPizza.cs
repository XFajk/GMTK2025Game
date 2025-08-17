using Godot;
using System;
using System.Collections.Generic;

public partial class MissionPizza : Node, IMission {
    [Export]
    public string ActualFoodName = "pizza";

    [Export(PropertyHint.Range, "0,1000,10")]
    public int TargetFoodQuantity;

    public IMission.Properties Properties;

    private Ship _ship;

    void IMission.MissionReady(Ship ship, MissionManager.Clock missionClock) {
        _ship = ship;
        Properties = new() {
            Title = $"Mission: {ActualFoodName} time!",
            Briefing = [
                $"Hey Ship, new captain here. "
                + $"Starting today, we will no longer eat that dry, tasteless junk anymore. "
                + $"From now on, we're eating {ActualFoodName}. ",
                $"We've throw away whatever was left over, and reconfigured the plants to make {ActualFoodName}",
                $"Make sure we have about {TargetFoodQuantity} of {ActualFoodName} before tomorrow. "
            ],
            Debrief = [
                $"It's {ActualFoodName} time!"
            ],
            ResourceMinimumRequirements = [KeyValuePair.Create(Resource.Food, TargetFoodQuantity)]
        };
    }

    public IMission.Properties GetMissionProperties() => Properties;

    public void OnStart(Ship ship) {
        ship.RemoveResource(Resource.Food, float.MaxValue);

        // Plants only produces half the quantity of food
        foreach (Machine machine in _ship.Machines) {
            if (machine is not Plants) continue;
            foreach (Node child in machine.GetNode("Outputs").GetChildren()) {
                if (child is MachineBuffer output && output.Resource == Resource.Food) {
                    output.QuantityChangeInReceipe = 1;
                }
            }
        }

        // because it took twice as much resources to make the same unit of food, 
        // we now need to balance out the loop again. Refer to the resource flow diagram.
        // Also the Resource.GetRatio no longer gives the correct values.
        Process foodProcess = ship.GetNode<Process>("CrewFoodProcess");
        foreach (Node child in foodProcess.GetNode("Inputs").GetChildren()) {
            if (child is InputOutput input && input.Container.GetResource() == Resource.Oxygen) {
                input.QuantityChangeInReceipe = 4;
            }
        }

        foreach (Node child in foodProcess.GetNode("Outputs").GetChildren()) {
            if (child is not InputOutput output) continue;

            Resource res = output.Container.GetResource();
            if (res == Resource.SolidWaste) {
                output.QuantityChangeInReceipe = 4;
            } else if (res == Resource.FluidWaste) {
                output.QuantityChangeInReceipe = 5;
            }
        }
    }

    public bool IsPreparationFinished() => true;

    public bool IsMissionFinised() => (this as IMission).CheckMaterialRequirements(_ship);

    public void OnCompletion(Ship ship) {
    }

    public bool IsDelayed() => false;
}
