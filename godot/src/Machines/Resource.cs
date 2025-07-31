using System;
using System.Collections.Generic;
using System.Linq;

public enum Resource {
    /// 1 coolant + 4 hydro
    CoolantHot,
    /// 1 coolant + 4 hydro
    CoolantCold,
    /// 1 hydro
    Humidity,
    /// 1 hydro
    Water,
    /// 1 hydro + 1 carbon
    Food,
    /// 1 hydro
    FluidWaste,
    /// 1 carbon
    SolidWaste,
    /// 1 oxygen
    Oxygen,
    /// 1 carbon + 2 oxygen
    CarbonDioxide,
    /// 8 carbon
    Disposables,
    /// 8 carbon
    Garbage
}

/// helper classes
public class Resources {
    public const float AirPercentageFactor = 0.1f;

    /// returns true if this resource can flow without a connection
    public static bool IsFloating(Resource resource) => resource switch {
        Resource.Humidity or Resource.Oxygen or Resource.CarbonDioxide => true,
        _ => false,
    };

    public static string ToUnit(Resource resource, int quantity) {
        return resource switch {
            Resource.CoolantHot or Resource.CoolantCold => $"{quantity * 4} L",
            Resource.Humidity => $"{quantity} %",
            Resource.Water => $"{quantity / 10} L",
            Resource.Food => $"{quantity / 5.0f} KG",
            Resource.SolidWaste => $"{quantity / 10.0f} KG",
            Resource.FluidWaste => $"{quantity / 10} L",
            Resource.Oxygen => $"{quantity * AirPercentageFactor} %",
            Resource.CarbonDioxide => $"{quantity * AirPercentageFactor} %",
            Resource.Disposables => $"{quantity}",
            Resource.Garbage => $"{quantity}",
            _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null),
        };
    }

    public static bool IsLossless(IList<InputOutput> receipe) {
        Dictionary<ResourcePart, int> totals = new();

        // if all resource changes add up to 0, then we lose nothing
        foreach (InputOutput io in receipe) {
            var parts = GetParts(io.Resource);
            foreach (ResourcePart p in parts) {
                int current = totals.GetValueOrDefault(p);
                totals[p] = current + io.QuantityChangeInReceipe;
            }
        }

        return totals.Values.All(v => v == 0);
    }

    // gests
    private static List<ResourcePart> GetParts(Resource resource) {
        return resource switch {
            Resource.CoolantHot or Resource.CoolantCold => [ResourcePart.Coolant, .. Enumerable.Repeat(ResourcePart.Hydro, 4)],
            Resource.Humidity or Resource.Water => [ResourcePart.Hydro],
            Resource.Food => [ResourcePart.Hydro, ResourcePart.Carbon],
            Resource.FluidWaste => [ResourcePart.Hydro],
            Resource.SolidWaste => [ResourcePart.Carbon],
            Resource.Oxygen => [ResourcePart.Oxygen],
            Resource.CarbonDioxide => [ResourcePart.Carbon, ResourcePart.Oxygen],
            Resource.Disposables or Resource.Garbage => [.. Enumerable.Repeat(ResourcePart.Carbon, 8)],
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