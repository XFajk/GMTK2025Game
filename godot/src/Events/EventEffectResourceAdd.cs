using Godot;

public class EventEffectResourceAdd : IEventEffect {
    // the target container that receives resources
    public IContainer Target;

    // to drain a resource, set this to negative
    public float AdditionPerSecond;

    // when draining a resource, set this to negative
    public float MaxResourcesToAdd = float.MaxValue;

    // when draining a resource, this is negative
    public float TotalResourcesAdded;

    public void Process(double deltaTime) {
        if (Finished) return;

        float addition = AdditionPerSecond * (float)deltaTime;

        if (TotalResourcesAdded + addition > MaxResourcesToAdd) {
            addition = MaxResourcesToAdd - TotalResourcesAdded;
        }

        float notAdded = Target.RemainderOfAdd(addition);
        TotalResourcesAdded += (addition - notAdded);
    }

    public bool Finished => Mathf.Abs(TotalResourcesAdded) >= Mathf.Abs(MaxResourcesToAdd);
}