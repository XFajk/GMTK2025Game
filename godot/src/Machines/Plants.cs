using Godot;
using Godot.Collections;

public partial class Plants : Machine {
    [Export]
    public Array<MeshInstance3D> PlantMeshes = new();

    [Export]
    public float GrowthRate = 0.1f;

    [Export]
    public float DryingRate = 0.05f;

    public float PlantHealth = 1.0f;

    public override void _Process(double deltaTime) {
        if (!IsWorking) return;
        PlantHealth = Mathf.Clamp(PlantHealth, 0, 2.0f);
        GD.Print($"Plant {Name} health: {PlantHealth}");

        // check if all ingredients are present and enough output space available
        foreach (MachineBuffer container in _recipeParts) {
            if (!CanCycle(container, PlantHealth)) {
                GD.Print($"Cannot cycle {Name} due to insufficient resources or space. of {container.GetName()}");
                _processProgress = 0;
                PlantHealth -= DryingRate * (float)deltaTime;
                return;
            }
        }

        // now run the machine
        _processProgress += _processingPerSecond * (float)deltaTime;

        while (_processProgress > 1) {
            GD.Print($"Machine {Name} execution!");
            _processProgress -= 1;

            PlantHealth += GrowthRate * (float)deltaTime;

            foreach (MachineBuffer container in _recipeParts) {
                container.Quantity += container.QuantityChangeInReceipe * PlantHealth;
            }

            // check again if we can continue to cycle
            foreach (MachineBuffer container in _recipeParts) {
                if (!CanCycle(container, PlantHealth)) {
                    _processProgress = 0;
                    return;
                }
            }
        }
    }

    // Updated CanCycle to include PlantHealth
    private static bool CanCycle(MachineBuffer container, float plantHealth) {
        float delta = container.QuantityChangeInReceipe * plantHealth;
        float quantityAfterCycle = container.Quantity + delta;
        return quantityAfterCycle >= 0 && quantityAfterCycle <= container.MaxQuantity;
    }
}