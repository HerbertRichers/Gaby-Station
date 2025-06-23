using System;
using System.Collections.Generic;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.PlantAnalyzer;

[Serializable, NetSerializable]
public enum PlantAnalyzerUiKey : byte
{
    Key
}

/// <summary>
/// Information about the seed itself.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlantSeedInfo
{
    public string Name = string.Empty;
    public string DisplayName = string.Empty;
    public string Noun = string.Empty;

    public float Potency;
    public int Yield;
    public float Maturation;
    public float Endurance;
    public float Lifespan;
    public float Production;
    public bool Seedless;
    public bool Viable;

    public float WaterConsumption;
    public float NutrientConsumption;
    public float IdealLight;
    public float LightTolerance;
    public float IdealHeat;
    public float HeatTolerance;
    public float LowPressureTolerance;
    public float HighPressureTolerance;
    public float ToxinsTolerance;
    public float PestTolerance;
    public float WeedTolerance;

    public List<PlantChemEntry> Chemicals = new();
    public List<string> Mutations = new();
}

[Serializable, NetSerializable]
public sealed class PlantChemEntry
{
    public string Id = string.Empty;
    public int Min;
    public int Max;
    public int PotDiv;
}

/// <summary>
/// Status information about a planted specimen.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlantHolderInfo
{
    public float Health;
    public int Age;
    public bool HarvestReady;
    public bool Sampled;
    public bool Dead;

    public float WaterLevel;
    public float NutritionLevel;
    public float Toxins;
    public float PestLevel;
    public float WeedLevel;

    public bool ImproperHeat;
    public bool ImproperPressure;
    public bool ImproperLight;
}

/// <summary>
/// Message sent from server to client after scanning a seed or plant.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlantAnalyzerScannedMessage : BoundUserInterfaceMessage
{
    public readonly bool IsPlant;
    public readonly PlantSeedInfo Seed;
    public readonly PlantHolderInfo? Holder;

    public PlantAnalyzerScannedMessage(bool isPlant, PlantSeedInfo seed, PlantHolderInfo? holder)
    {
        IsPlant = isPlant;
        Seed = seed;
        Holder = holder;
    }
}
