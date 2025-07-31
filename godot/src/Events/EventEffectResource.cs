public class EventEffectResource : IEventEffect {
    // the target container that receives resources
    public IContainer Target;

    // to drain a resource, set this to negative
    public float AdditionPerSecond;

    public float TotalResourcesAdded;

    public void Process(double deltaTime) {
        float addition = AdditionPerSecond * (float)deltaTime;
        Target.AddQuantity(addition);
        TotalResourcesAdded += addition;
    }
}