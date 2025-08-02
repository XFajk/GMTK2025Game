using System.Collections.Generic;
using Godot;


public partial class StatusInterface : Sprite3D {
    public SubViewport VisualBarsViewPort = null;
    public Label MachineNameLabel = null;

    public List<StatusBar> StatusBars = [];

    public static PackedScene MachineStatusBarScene = GD.Load<PackedScene>("res://scenes/ui/machine_status_bar.tscn");

    public override void _Ready() {
        VisualBarsViewPort = GetNode<SubViewport>("ViewPort");
        MachineNameLabel = GetNode<Label>("ViewPort/MachineName");
        Visible = false;
    }

    public void SetRecepiePartsIntoInterface(List<MachineBuffer> recipeParts) {
        // clear old status bars
        foreach (StatusBar bar in StatusBars) {
            bar.QueueFree();
        }
        StatusBars.Clear();

        float inputOffset = -70;
        float outputOffset = 70;
        // create new status bars
        foreach (MachineBuffer buffer in recipeParts) {
            if (Resources.IsFloating(buffer.Resource)) continue;

            StatusBar statusBar = MachineStatusBarScene.Instantiate<StatusBar>();
            VisualBarsViewPort.AddChild(statusBar);

            statusBar._Ready();
            statusBar.SetStatusFromMachineBuffer(buffer);

            if (buffer.QuantityChangeInReceipe < 0) {
                statusBar.Position += new Vector2(inputOffset, 0);
                inputOffset -= 70;
            } else {
                statusBar.Position += new Vector2(outputOffset, 0);
                outputOffset += 70; // adjust for next output
            }
            StatusBars.Add(statusBar);
        }
    }

    public void SetStorageContainerIntoInterface(StorageContainer storageContainer) {
        // clear old status bars
        foreach (StatusBar bar in StatusBars) {
            bar.QueueFree();
        }
        StatusBars.Clear();

        VisualBarsViewPort.RemoveChild(VisualBarsViewPort.GetNode("Arrow"));

        StatusBar statusBar = MachineStatusBarScene.Instantiate<StatusBar>();
        VisualBarsViewPort.AddChild(statusBar);

        statusBar._Ready();
        statusBar.SetStatusFromStorageContainer(storageContainer);

        statusBar.Position += new Vector2(0, 0); // adjust position if needed
        StatusBars.Add(statusBar);
    }
    
    public void AddPlantHealthBar(ProgressBar plantHealthBar) {
        VisualBarsViewPort.AddChild(plantHealthBar);
    }
}
