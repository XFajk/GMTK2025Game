public class EventEffectResource : IEventEffect {
    // the target container that receives resources
    public IContainer Target;

    // to drain a resource, set this to negative
    public float AdditionPerSecond;

    public float MaxResourcesToAdd = float.MaxValue;

    public float TotalResourcesAdded;

    public void Process(double deltaTime) {
        if (Finished) return;

        float addition = AdditionPerSecond * (float)deltaTime;

        if (TotalResourcesAdded + addition > MaxResourcesToAdd) {
            addition = MaxResourcesToAdd - TotalResourcesAdded;
        }

        Target.AddQuantity(addition);
        TotalResourcesAdded += addition;
    }

    public bool Finished => TotalResourcesAdded >= MaxResourcesToAdd;
}