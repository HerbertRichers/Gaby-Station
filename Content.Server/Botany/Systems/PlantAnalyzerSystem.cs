// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Server.Botany.Components;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.PlantAnalyzer;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Botany.Systems;

public sealed class PlantAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

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
            Console.WriteLine("=======================================");
            Console.WriteLine("SeedData não encontrado! Alvo não é planta nem semente.");
            Console.WriteLine("=======================================");
            return;
        }

        Console.WriteLine("=======================================");
        Console.WriteLine("========= SCANNER DE PLANTAS =========");

        if (isPlant)
        {
            Console.WriteLine("\n[INFORMAÇÕES DA PLANTA]");
        }
        else
        {
            Console.WriteLine("\n[INFORMAÇÕES DA SEMENTE]");
        }

        Console.WriteLine($"Nome: {seedData.Name}");
        Console.WriteLine($"Nome de Exibição: {seedData.DisplayName}");
        Console.WriteLine($"Tipo: {seedData.Noun}");

        // Informações Genéticas
        Console.WriteLine("\n[GENÉTICA]");
        Console.WriteLine($"- Potência: {seedData.Potency}");
        Console.WriteLine($"- Rendimento (Yield): {seedData.Yield}");
        Console.WriteLine($"- Maturação: {seedData.Maturation} ciclos");
        Console.WriteLine($"- Vida Máxima (Endurance): {seedData.Endurance}");
        Console.WriteLine($"- Tempo de Vida (Lifespan): {seedData.Lifespan} ciclos");
        Console.WriteLine($"- Produção: {seedData.Production}");
        Console.WriteLine($"- Pode gerar sementes: {(seedData.Seedless ? "❌" : "✔️")}");

        // Se for planta plantada, mostra status dinâmico
        if (isPlant && plant != null)
        {
            Console.WriteLine("\n[STATUS DA PLANTA]");
            Console.WriteLine($"- Saúde: {plant.Health}/{seedData.Endurance}");
            Console.WriteLine($"- Idade: {plant.Age} ciclos");
            Console.WriteLine($"- Pronta para colheita: {(plant.Harvest ? "✔️" : "❌")}");
            Console.WriteLine($"- Amostrada: {(plant.Sampled ? "✔️" : "❌")}");
            Console.WriteLine($"- Estado: {(plant.Dead ? "☠️ Morta" : "🌿 Saudável")}");
            Console.WriteLine($"- Viabilidade Genética: {(seedData.Viable ? "✔️ Saudável" : "❌ Defeituosa (Irá morrer)")}");

            Console.WriteLine("\n[CONDIÇÕES DO VASO]");
            Console.WriteLine($"- Água no vaso: {plant.WaterLevel}/100");
            Console.WriteLine($"- Nutrientes no vaso: {plant.NutritionLevel}/100");
            Console.WriteLine($"- Toxinas acumuladas: {plant.Toxins}");
            Console.WriteLine($"- Infestação de pragas: {plant.PestLevel}");
            Console.WriteLine($"- Ervas daninhas: {plant.WeedLevel}");

            Console.WriteLine("\n[AVISOS AMBIENTAIS]");
            Console.WriteLine($"- Temperatura: {(plant.ImproperHeat ? "⚠️ Incorreta" : "✔️ OK")}");
            Console.WriteLine($"- Pressão: {(plant.ImproperPressure ? "⚠️ Incorreta" : "✔️ OK")}");
            Console.WriteLine($"- Luz: {(plant.ImproperLight ? "⚠️ Incorreta" : "✔️ OK")}");
        }

        // Consumo
        Console.WriteLine("\n[CONSUMO]");
        Console.WriteLine($"- Água: {seedData.WaterConsumption} por ciclo");
        Console.WriteLine($"- Nutrientes: {seedData.NutrientConsumption} por ciclo");

        // Tolerâncias
        Console.WriteLine("\n[TOLERÂNCIAS]");
        Console.WriteLine($"- Luz: Ideal {seedData.IdealLight} ± {seedData.LightTolerance}");
        Console.WriteLine($"- Temperatura: Ideal {seedData.IdealHeat} ± {seedData.HeatTolerance}");
        Console.WriteLine($"- Pressão: {seedData.LowPressureTolerance} - {seedData.HighPressureTolerance} kPa");
        Console.WriteLine($"- Toxinas: até {seedData.ToxinsTolerance}");
        Console.WriteLine($"- Pragas: até {seedData.PestTolerance}");
        Console.WriteLine($"- Ervas daninhas: até {seedData.WeedTolerance}");

        // Químicos
        Console.WriteLine("\n[QUÍMICOS]");
        if (seedData.Chemicals.Count > 0)
        {
            foreach (var chem in seedData.Chemicals)
            {
                Console.WriteLine($"- {chem.Key} (Min: {chem.Value.Min}, Max: {chem.Value.Max}, PotDiv: {chem.Value.PotencyDivisor})");
            }
        }
        else
        {
            Console.WriteLine("Nenhum químico presente.");
        }

        // Mutações
        Console.WriteLine("\n[MUTAÇÕES ATIVAS]");
        if (seedData.Mutations.Count > 0)
        {
            foreach (var mutation in seedData.Mutations)
            {
                Console.WriteLine($"- {mutation.Name}");
            }
        }
        else
        {
            Console.WriteLine("Nenhuma mutação.");
        }

        Console.WriteLine("\n=======================================");
        args.Handled = true;
    }
}
