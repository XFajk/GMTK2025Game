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

    private float _baseProcessingPerSecond;

    private ProgressBar _plantHealthBar = GD.Load<PackedScene>("res://scenes/ui/plant_health_status_bar.tscn").Instantiate<ProgressBar>();

    public override void _Ready() {
        base._Ready();
        _baseProcessingPerSecond = _processingPerSecond;

        _statusInterface.AddPlantHealthBar(_plantHealthBar);
    }

    public override void _Process(double deltaTime) {

        PlantHealth = Mathf.Clamp(PlantHealth, 0, 2.0f);
        _plantHealthBar.Value = PlantHealth / 2.0f * 100;
        _processingPerSecond = _baseProcessingPerSecond * PlantHealth;
        
        // always progess
        _processProgress += _processingPerSecond * (float)deltaTime;

        while (_processProgress > 1) {
            _processProgress -= 1;
            
            // check if all ingredients are present and enough output space available
            foreach (MachineBuffer container in _recipeParts) {
                if (!CanCycle(container)) {
                    _processProgress = 0;
                    PlantHealth -= DryingRate; 
                    return;
                }
            }

            PlantHealth += GrowthRate;

            foreach (MachineBuffer container in _recipeParts) {
                container.Quantity += container.QuantityChangeInReceipe;
            }
        }
    }
}