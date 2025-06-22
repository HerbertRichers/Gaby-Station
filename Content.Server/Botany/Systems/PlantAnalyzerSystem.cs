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

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[SCANNER DE PLANTAS]");

        if (isPlant)
            sb.AppendLine("\n[INFORMAÇÕES DA PLANTA]");
        else
            sb.AppendLine("\n[INFORMAÇÕES DA SEMENTE]");

        sb.AppendLine($"Nome: {seedData.Name}");
        sb.AppendLine($"Nome de Exibição: {seedData.DisplayName}");
        sb.AppendLine($"Tipo: {seedData.Noun}");

        sb.AppendLine("\n[GENÉTICA]");
        sb.AppendLine($"- Potência: {seedData.Potency}");
        sb.AppendLine($"- Rendimento: {seedData.Yield}");
        sb.AppendLine($"- Maturação: {seedData.Maturation} ciclos");
        sb.AppendLine($"- Vida Máxima: {seedData.Endurance}");
        sb.AppendLine($"- Tempo de Vida: {seedData.Lifespan} ciclos");
        sb.AppendLine($"- Produção: {seedData.Production}");
        sb.AppendLine($"- Pode gerar sementes: {(seedData.Seedless ? "Não" : "Sim")}");

        if (isPlant && plant != null)
        {
            sb.AppendLine("\n[STATUS DA PLANTA]");
            sb.AppendLine($"- Saúde: {plant.Health}/{seedData.Endurance}");
            sb.AppendLine($"- Idade: {plant.Age} ciclos");
            sb.AppendLine($"- Pronta para colheita: {(plant.Harvest ? "Sim" : "Não")}");
            sb.AppendLine($"- Amostrada: {(plant.Sampled ? "Sim" : "Não")}");
            sb.AppendLine($"- Estado: {(plant.Dead ? "Morta" : "Saudável")}");
            sb.AppendLine($"- Viabilidade Genética: {(seedData.Viable ? "Saudável" : "Defeituosa")}");

            sb.AppendLine("\n[CONDIÇÕES DO VASO]");
            sb.AppendLine($"- Água no vaso: {plant.WaterLevel}/100");
            sb.AppendLine($"- Nutrientes no vaso: {plant.NutritionLevel}/100");
            sb.AppendLine($"- Toxinas acumuladas: {plant.Toxins}");
            sb.AppendLine($"- Infestação de pragas: {plant.PestLevel}");
            sb.AppendLine($"- Ervas daninhas: {plant.WeedLevel}");

            sb.AppendLine("\n[AVISOS AMBIENTAIS]");
            sb.AppendLine($"- Temperatura: {(plant.ImproperHeat ? "Incorreta" : "OK")}");
            sb.AppendLine($"- Pressão: {(plant.ImproperPressure ? "Incorreta" : "OK")}");
            sb.AppendLine($"- Luz: {(plant.ImproperLight ? "Incorreta" : "OK")}");
        }

        sb.AppendLine("\n[CONSUMO]");
        sb.AppendLine($"- Água: {seedData.WaterConsumption} por ciclo");
        sb.AppendLine($"- Nutrientes: {seedData.NutrientConsumption} por ciclo");

        sb.AppendLine("\n[TOLERÂNCIAS]");
        sb.AppendLine($"- Luz: Ideal {seedData.IdealLight} ± {seedData.LightTolerance}");
        sb.AppendLine($"- Temperatura: Ideal {seedData.IdealHeat} ± {seedData.HeatTolerance}");
        sb.AppendLine($"- Pressão: {seedData.LowPressureTolerance} - {seedData.HighPressureTolerance} kPa");
        sb.AppendLine($"- Toxinas: até {seedData.ToxinsTolerance}");
        sb.AppendLine($"- Pragas: até {seedData.PestTolerance}");
        sb.AppendLine($"- Ervas daninhas: até {seedData.WeedTolerance}");

        sb.AppendLine("\n[QUÍMICOS]");
        if (seedData.Chemicals.Count > 0)
        {
            foreach (var chem in seedData.Chemicals)
            {
                sb.AppendLine($"- {chem.Key} (Min: {chem.Value.Min}, Max: {chem.Value.Max}, PotDiv: {chem.Value.PotencyDivisor})");
            }
        }
        else
        {
            sb.AppendLine("Nenhum químico presente.");
        }

        sb.AppendLine("\n[MUTAÇÕES ATIVAS]");
        if (seedData.Mutations.Count > 0)
        {
            foreach (var mutation in seedData.Mutations)
            {
                sb.AppendLine($"- {mutation.Name}");
            }
        }
        else
        {
            sb.AppendLine("Nenhuma mutação.");
        }

        var msg = new FormattedMessage();
        msg.AddText(sb.ToString());

        _ui.SetUiState(uid, PlantAnalyzerUiKey.Key, new PlantAnalyzerBoundUserInterfaceState(msg));
        _ui.OpenUi(uid, PlantAnalyzerUiKey.Key, args.Args.User);
        args.Handled = true;
    }
}
