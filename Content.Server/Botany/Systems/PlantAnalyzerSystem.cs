// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Server.Botany.Components;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.PlantAnalyzer;
using Robust.Server.GameObjects;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Botany.Systems;

public sealed class PlantAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlantAnalyzerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PlantAnalyzerComponent, PlantAnalyzerDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(EntityUid uid, PlantAnalyzerComponent component, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach ||
            (!HasComp<PlantHolderComponent>(args.Target.Value) && !HasComp<SeedComponent>(args.Target.Value)))
            return;

        var doAfter = new DoAfterArgs(EntityManager, args.User, component.ScanDelay, new PlantAnalyzerDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            BreakOnMove = true,
            NeedHand = true,
            Broadcast = true
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(EntityUid uid, PlantAnalyzerComponent component, DoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (component.ScanSound != null)
            _audio.PlayPvs(component.ScanSound, uid);

        var target = args.Args.Target;
        if (target == null)
            return;

        SeedData? seedData = null;
        bool isPlant = false;
        PlantHolderComponent? plant = null;

        // Verifica se é uma planta
        if (TryComp<PlantHolderComponent>(target.Value, out var holder) && holder.Seed != null)
        {
            seedData = holder.Seed;
            isPlant = true;
            plant = holder;
        }
        // Verifica se é uma semente
        else if (TryComp<SeedComponent>(target.Value, out var seed))
        {
            if (seed.Seed != null)
            {
                seedData = seed.Seed;
            }
            else if (seed.SeedId != null)
            {
                seedData = _prototypeManager.Index<SeedPrototype>(seed.SeedId);
            }
        }

        if (seedData == null)
        {
            return;
        }

        var seedInfo = new PlantSeedInfo
        {
            Name = seedData.Name,
            DisplayName = seedData.DisplayName,
            Noun = seedData.Noun,
            Potency = seedData.Potency,
            Yield = seedData.Yield,
            Maturation = seedData.Maturation,
            Endurance = seedData.Endurance,
            Lifespan = seedData.Lifespan,
            Production = seedData.Production,
            Seedless = seedData.Seedless,
            Viable = seedData.Viable,
            WaterConsumption = seedData.WaterConsumption,
            NutrientConsumption = seedData.NutrientConsumption,
            IdealLight = seedData.IdealLight,
            LightTolerance = seedData.LightTolerance,
            IdealHeat = seedData.IdealHeat,
            HeatTolerance = seedData.HeatTolerance,
            LowPressureTolerance = seedData.LowPressureTolerance,
            HighPressureTolerance = seedData.HighPressureTolerance,
            ToxinsTolerance = seedData.ToxinsTolerance,
            PestTolerance = seedData.PestTolerance,
            WeedTolerance = seedData.WeedTolerance,
        };

        foreach (var chem in seedData.Chemicals)
        {
            seedInfo.Chemicals.Add(new PlantChemEntry
            {
                Id = chem.Key,
                Min = chem.Value.Min,
                Max = chem.Value.Max,
                PotDiv = chem.Value.PotencyDivisor,
            });
        }

        foreach (var mutation in seedData.Mutations)
        {
            seedInfo.Mutations.Add(mutation.Name);
        }

        PlantHolderInfo? holderInfo = null;

        if (isPlant && plant != null)
        {
            holderInfo = new PlantHolderInfo
            {
                Health = plant.Health,
                Age = plant.Age,
                HarvestReady = plant.Harvest,
                Sampled = plant.Sampled,
                Dead = plant.Dead,
                WaterLevel = plant.WaterLevel,
                NutritionLevel = plant.NutritionLevel,
                Toxins = plant.Toxins,
                PestLevel = plant.PestLevel,
                WeedLevel = plant.WeedLevel,
                ImproperHeat = plant.ImproperHeat,
                ImproperPressure = plant.ImproperPressure,
                ImproperLight = plant.ImproperLight,
            };
        }

        _ui.ServerSendUiMessage(uid, PlantAnalyzerUiKey.Key,
            new PlantAnalyzerScannedMessage(isPlant, seedInfo, holderInfo));
        _ui.OpenUi(uid, PlantAnalyzerUiKey.Key, args.Args.User);
        args.Handled = true;
    }
}
