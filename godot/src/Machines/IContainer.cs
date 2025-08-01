

public interface IContainer {
    Resource GetResource();
    float GetQuantity();
    int GetMaxQuantity();
    void SetQuantity(float newValue);
    void AddQuantity(float addition) => SetQuantity(GetQuantity() + addition);
    void RemoveQuantity(float addition) => AddQuantity(-addition);

    float GetQuantityFree() => GetMaxQuantity() - GetQuantity();
    string GetName();

    float RemainderOfAdd(float addition) {
        float newQuantity = GetQuantity() + addition;
        if (newQuantity < 0) {
            SetQuantity(0);
            // addition was negative, so the remainder must be negative as well
            return newQuantity;
        }

        int max = GetMaxQuantity();
        if (newQuantity > max) {
            SetQuantity(max);
            return newQuantity - max;
        }

        SetQuantity(newQuantity);
        return 0;
    }

    float RemainderOfRemove(float removed) => -RemainderOfAdd(-removed);
}