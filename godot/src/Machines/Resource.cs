using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using Godot;

public enum Resource {
    Unset,
    CoolantHot,
    CoolantCold,
    Humidity,
    Water,
    Food,
    FluidWaste,
    SolidWaste,
    Oxygen,
    CarbonDioxide,
    Disposables,
    Garbage,
}

/// helper classes
public class Resources {

    private static Texture2D[] _resourceIcons = {
        GD.Load<Texture2D>("res://assets/icons/coolant_hot.png"),
        GD.Load<Texture2D>("res://assets/icons/coolant_cold.png"),
        GD.Load<Texture2D>("res://assets/icons/humidity.png"),
        GD.Load<Texture2D>("res://assets/icons/water.png"),
        GD.Load<Texture2D>("res://assets/icons/food.png"),
        GD.Load<Texture2D>("res://assets/icons/fluid_waste.png"),
        GD.Load<Texture2D>("res://assets/icons/solid_waste.png"),
        GD.Load<Texture2D>("res://assets/icons/oxygen.png"),
        GD.Load<Texture2D>("res://assets/icons/carbon_dioxide.png"),
        GD.Load<Texture2D>("res://assets/icons/disposables.png"),
        GD.Load<Texture2D>("res://assets/icons/garbage.png"),
    };

    public const float AirPercentageFactor = 0.1f;

    /// returns true if this resource can flow without a connection
    public static bool IsFloating(Resource resource) => resource switch {
        Resource.Humidity or Resource.Oxygen or Resource.CarbonDioxide => true,
        _ => false,
    };

    public static string ToUnit(Resource resource, int quantity) {
        return resource switch {
            Resource.CoolantHot or Resource.CoolantCold => $"{quantity * 4} L",
            Resource.Humidity or Resource.Oxygen or Resource.CarbonDioxide => string.Format("{0,3:F1} %", quantity * AirPercentageFactor),
            Resource.Water => $"{quantity / 10} L",
            Resource.Food => $"{quantity / 5.0f} KG",
            Resource.SolidWaste => $"{quantity / 10.0f} KG",
            Resource.FluidWaste => $"{quantity / 10} L",
            Resource.Disposables => $"{quantity}",
            Resource.Garbage => $"{quantity}",
            _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
        };
    }

    public static void VerifyLossless(IList<MachineBuffer> receipe, string reportingName) {
        Dictionary<ResourcePart, int> totals = new();

        // if all resource changes add up to 0, then we lose nothing
        foreach (MachineBuffer buffer in receipe) {
            if (buffer.Resource == Resource.Unset) {
                throw new Exception($"node {buffer.Name} of {reportingName} has no resource set");
            }

            var parts = GetParts(buffer.Resource);
            foreach (ResourcePart p in parts) {
                int current = totals.GetValueOrDefault(p);
                totals[p] = current + buffer.QuantityChangeInReceipe;
            }
        }

        Check(reportingName, totals);
    }

    public static void VerifyLossless(IList<InputOutput> receipe, string reportingName) {
        Dictionary<ResourcePart, int> totals = new();

        // if all resource changes add up to 0, then we lose nothing
        foreach (InputOutput io in receipe) {
            IContainer container = io.Container ?? throw new Exception($"IO {io.Name} of {reportingName} has no container set");

            Resource resource = container.GetResource();

            if (resource == Resource.Unset) {
                throw new Exception($"node {io.Name} of {reportingName} refers to container {container.GetName()} which has no resource set");
            }

            var parts = GetParts(resource);
            foreach (ResourcePart p in parts) {
                int current = totals.GetValueOrDefault(p);
                totals[p] = current + io.QuantityChangeInReceipe;
            }
        }

        Check(reportingName, totals);
    }

    private static void Check(string reportingName, Dictionary<ResourcePart, int> totals) {
        List<string> losses = totals.Where(pair => pair.Value != 0).Select(pair => pair.Key + ":" + pair.Value).ToList();
        if (losses.Count == 0) return;

        throw new Exception(reportingName + " has loss: " + string.Join(", ", losses));
    }

    /// returns how many of r2 you can make from r1
    /// if no parts are shared, this function returns float.MaxValue
    /// if multiple parts are shared, this function returns the lowest ratio
    public static KeyValuePair<int, int> GetRatio(Resource r1, Resource r2) {
        var parts1 = GetParts(r1);
        var parts2 = GetParts(r2);
        HashSet<ResourcePart> sharedParts = new();
        foreach (ResourcePart p1 in parts1) {
            if (sharedParts.Contains(p1)) continue;

            foreach (ResourcePart p2 in parts2) {
                if (p1 == p2) sharedParts.Add(p1);
            }
        }
        
        int countsOf1 = 0;
        int countsOf2 = 0;
        foreach (ResourcePart pShared in sharedParts) {
            countsOf1 += parts1.Where(p1 => p1 == pShared).Count();
            countsOf2 += parts2.Where(p2 => p2 == pShared).Count();
        }

        int divisor = GCD(countsOf1, countsOf2);

        return KeyValuePair.Create(countsOf1 / divisor, countsOf2 / divisor);
    }
    
    static int GCD(int a, int b)
    {
        return b == 0 ? a : GCD(b, a % b);
    }

    // gests
    private static List<ResourcePart> GetParts(Resource resource) {
        return resource switch {
            Resource.CoolantHot => [ResourcePart.Coolant],
            Resource.CoolantCold => [ResourcePart.Coolant, .. Enumerable.Repeat(ResourcePart.Hydro, 4)],
            Resource.Humidity or Resource.Water => [ResourcePart.Hydro],
            Resource.Food => [ResourcePart.Hydro, ResourcePart.Carbon],
            Resource.FluidWaste => [ResourcePart.Hydro],
            Resource.SolidWaste => [ResourcePart.Carbon, ResourcePart.Oxygen],
            Resource.Oxygen => [ResourcePart.Oxygen],
            Resource.CarbonDioxide => [ResourcePart.Carbon, ResourcePart.Oxygen, ResourcePart.Oxygen],
            Resource.Disposables or Resource.Garbage => [.. Enumerable.Repeat(ResourcePart.Carbon, 8)],
            _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
        };
    }

    public static Texture2D GetResourceIcon(Resource resource) {
        if (resource == Resource.Unset) return null;

        int index = (int)resource - 1; // Unset is 0, so we start at index 0
        if (index < 0 || index >= _resourceIcons.Length) {
            throw new ArgumentOutOfRangeException(nameof(resource), resource, "Invalid resource icon index");
        }

        return _resourceIcons[index];
    }

    public static Color GetResourceColor(Resource resource) {
        return resource switch {
            Resource.CoolantHot => new Color(0.90f, 0.14f, 0.27f), // vivid red
            Resource.CoolantCold => new Color(0.18f, 0.49f, 0.92f), // electric blue
            Resource.Humidity => new Color(0.00f, 0.71f, 0.82f), // clear cyan
            Resource.Water => new Color(0.00f, 0.48f, 0.75f), // deep ocean blue
            Resource.Food => new Color(0.12f, 0.68f, 0.38f), // leafy green
            Resource.FluidWaste => new Color(0.94f, 0.67f, 0.30f), // warm amber
            Resource.SolidWaste => new Color(0.42f, 0.41f, 0.27f), // muddy olive
            Resource.Oxygen => new Color(0.97f, 0.96f, 0.87f), // soft off-white
            Resource.CarbonDioxide => new Color(0.20f, 0.20f, 0.22f), // deepest charcoal
            Resource.Disposables => new Color(0.78f, 0.15f, 0.65f), // high‑contrast magenta
            Resource.Garbage => new Color(0.15f, 0.29f, 0.33f), // rich teal
            _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
        };
    }

    // every resource is composed of a combination of these atoms
    private enum ResourcePart {
        Carbon,
        Hydro,
        Oxygen,
        Coolant,
    }
}
