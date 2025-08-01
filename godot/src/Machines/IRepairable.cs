using Godot;

public interface IRepairable {
    bool IsWorking();

    Node3D AsNode();

    void SetRepaired();
}