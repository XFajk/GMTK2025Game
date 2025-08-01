using Godot;
using Godot.Collections;

public partial class Plants : Machine {
    [Export]
    public Array<MeshInstance3D> PlantMeshes = new();

    [Export]
    public float GrowthRate = 0.1f;

    [Export]
    public float DryingRate = 0.05f;

    public float _plantHealth = 1.0f;

    public float _baseProcessingPerSecond;

    public override void _Ready() {
        base._Ready();
        _baseProcessingPerSecond = _processingPerSecond;
    }

    public override void _Process(double deltaTime) {

        _plantHealth = Mathf.Clamp(_plantHealth, 0, 2.0f);
        _processingPerSecond = _baseProcessingPerSecond * _plantHealth;
        
        GD.Print($"Plant {Name} health: {_plantHealth} -> running {_processingPerSecond} times per second");
        
        // always progess
        _processProgress += _processingPerSecond * (float)deltaTime;

        while (_processProgress > 1) {
            _processProgress -= 1;
            
            // check if all ingredients are present and enough output space available
            foreach (MachineBuffer container in _recipeParts) {
                if (!CanCycle(container)) {
                    GD.Print($"Cannot cycle {Name} due to insufficient resources or space. of {container.GetName()}");
                    _processProgress = 0;
                    _plantHealth -= DryingRate * (float)deltaTime;
                    return;
                }
            }

            _plantHealth += GrowthRate * (float)deltaTime;

            foreach (MachineBuffer container in _recipeParts) {
                container.Quantity += container.QuantityChangeInReceipe;
            }
        }
    }
}