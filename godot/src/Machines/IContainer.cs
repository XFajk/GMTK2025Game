

public interface IContainer {
    Resource GetResource();
    float GetQuantity();
    int GetMaxQuantity();
    void SetQuantity(float newValue);
    void AddQuantity(float addition) => SetQuantity(GetQuantity() + addition);
    string GetName();
}