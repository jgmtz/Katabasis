using System.Text.RegularExpressions;
using Lete.Core.World;

namespace Lete.Core.Tests;

/// <summary>As regiões (<c>content/regions</c>) e as cenas delas: chegada padrão e passagens apontando para lugares que existem.</summary>
public class RegionContentTests
{
    private static readonly string Root = FindRoot();

    private static readonly Regex Marker = new(@"\[node name=""([^""]+)"" type=""Marker3D""[^\]]*teleport_points");

    private static readonly Regex Exit = new(@"TargetRegion = ""([^""]*)""\s+TargetEntry = ""([^""]*)""");

    private static IReadOnlyDictionary<string, RegionDefinition> Regions() =>
        Directory.GetFiles(Path.Combine(Root, "content", "regions"), "*.json")
            .Select(f => WorldData.RegionFromJson(File.ReadAllText(f)))
            .ToDictionary(r => r.Id);

    private static string SceneText(RegionDefinition region) =>
        File.ReadAllText(Path.Combine(Root, region.Scene["res://".Length..]));

    private static HashSet<string> Markers(RegionDefinition region) =>
        Marker.Matches(SceneText(region)).Select(m => m.Groups[1].Value).ToHashSet();

    [Fact]
    public void EveryRegion_HasItsSceneAndDefaultEntry_AndIdMatchesTheFile()
    {
        foreach (var file in Directory.GetFiles(Path.Combine(Root, "content", "regions"), "*.json"))
        {
            var region = WorldData.RegionFromJson(File.ReadAllText(file));
            Assert.Equal(Path.GetFileNameWithoutExtension(file), region.Id);
            Assert.True(File.Exists(Path.Combine(Root, region.Scene["res://".Length..])), region.Scene);
            Assert.Contains(region.Entry, Markers(region));
        }

        Assert.Contains("tenaro", Regions().Keys);
    }

    [Fact]
    public void EveryPassage_LeadsToAnExistingRegionAndMarker()
    {
        var regions = Regions();
        var passages = 0;
        foreach (var region in regions.Values)
        {
            foreach (Match exit in Exit.Matches(SceneText(region)))
            {
                passages++;
                var target = exit.Groups[1].Value;
                Assert.True(regions.ContainsKey(target), $"{region.Id} → região '{target}' não existe.");
                Assert.Contains(exit.Groups[2].Value, Markers(regions[target]));
            }
        }

        Assert.True(passages >= 2);
    }

    [Fact]
    public void TriviasOfARegion_PointToARegionThatExists()
    {
        var regions = Regions();
        foreach (var file in Directory.GetFiles(Path.Combine(Root, "content", "trivias"), "*.json"))
        {
            Assert.Contains(WorldData.TriviaFromJson(File.ReadAllText(file)).RegionId, regions.Keys);
        }
    }

    private static readonly Regex EncounterNode = new(@"EncounterId = ""([^""]*)""");

    [Fact]
    public void EveryEncounter_StandsOnceInTheSceneOfItsRegion_AndFightsItsOwnShade()
    {
        var regions = Regions();
        var placed = regions.Values
            .SelectMany(r => EncounterNode.Matches(SceneText(r)).Select(m => (Region: r.Id, Id: m.Groups[1].Value)))
            .ToList();
        var encounters = Directory.GetFiles(Path.Combine(Root, "content", "encounters"), "*.json")
            .Select(f => WorldData.EncounterFromJson(File.ReadAllText(f)))
            .ToDictionary(e => e.Id);

        foreach (var encounter in encounters.Values)
        {
            Assert.Equal(new[] { encounter.RegionId }, placed.Where(p => p.Id == encounter.Id).Select(p => p.Region));
            var battle = File.ReadAllText(Path.Combine(Root, "content", "battle", encounter.Battle + ".json"));
            Assert.Equal(encounter.ShadeId, Data.CombatData.ShadeIdFromJson(battle));
            Assert.True(encounter.After is null || encounters.ContainsKey(encounter.After), encounter.Id);
        }

        Assert.All(placed, p => Assert.Contains(p.Id, encounters.Keys));
        // A descida de Tênaro: quatro encontros com o Remador, cada um depois do anterior, e o último é a luta inteira.
        var chain = new List<Encounter>();
        for (var next = encounters.Values.Single(e => e.RegionId == "tenaro" && e.After is null); next is not null;
             next = encounters.Values.SingleOrDefault(e => e.After == next.Id))
        {
            chain.Add(next);
        }

        Assert.Equal(4, chain.Count);
        Assert.All(chain.SkipLast(1), e => Assert.True(e.IsLesson && e.FreeDefeat));
        Assert.False(chain[^1].IsLesson);

        // A regra da romã (GDD 2.3): a luta inteira (onde Absorver aparece) só depois da conversa de Hécate na Trívia de
        // Tênaro, que liga a flag que o encontro pede.
        var flag = Assert.IsType<Conditions.FlagSet>(chain[^1].Requires).Flag;
        var intro = Directory.GetFiles(Path.Combine(Root, "content", "trivias"), "*.json")
            .Select(f => WorldData.TriviaFromJson(File.ReadAllText(f)))
            .Single(t => t.RegionId == "tenaro").Intro;
        Assert.NotNull(intro);
        var dialogue = Dialogue.DialogueData.FromJson(File.ReadAllText(Path.Combine(Root, "content", "dialogue", intro.DialogueId + ".json")));
        Assert.Contains(new Dialogue.SetFlag(flag), dialogue.Nodes.Values.SelectMany(n => n.Effects));
    }

    private static readonly Regex GatedExit = new(
        @"TargetRegion = ""([^""]*)""\s+TargetEntry = ""[^""]*""\s+RequiredPathKey = ""([^""]*)""\s+BlockedKey = ""([^""]*)""(?:\s+Milestone = ""([^""]*)"")?");

    /// <summary>Formas de atravessar que dependem de algo fora da fatia (o favor de Hermes, a relíquia do ramo).</summary>
    private static readonly HashSet<string> WaysOutsideTheSlice = new() { "favor_de_hermes", "ramo_dourado" };

    [Fact]
    public void TheAcheronCrossing_TheBoatNeedsTheKey_AndEveryWayOfTheSliceCanBeEarned()
    {
        var aqueronte = Regions()["aqueronte"];
        var boat = GatedExit.Match(SceneText(aqueronte));
        Assert.True(boat.Success, "A barca do Aqueronte precisa pedir uma chave do Caminho.");
        Assert.Equal("asfodelos", boat.Groups[1].Value);
        Assert.Equal("aqueronte", boat.Groups[2].Value);
        Assert.Equal("aqueronte", boat.Groups[4].Value);

        var key = Data.PathData.FromJson(File.ReadAllText(Path.Combine(Root, "content", "path", "aqueronte.json")));
        var setFlags = Directory.GetFiles(Path.Combine(Root, "content", "dialogue"), "*.json")
            .Select(f => Dialogue.DialogueData.FromJson(File.ReadAllText(f)))
            .SelectMany(g => g.Nodes.Values.SelectMany(n => n.Effects))
            .OfType<Dialogue.SetFlag>()
            .Select(f => f.Flag)
            .ToHashSet();
        foreach (var way in key.Ways.Where(w => !WaysOutsideTheSlice.Contains(w.Id)))
        {
            // Cada forma é uma origem (vale sozinha) ou uma flag que alguma conversa liga.
            Assert.True(way.Condition is Conditions.OriginIs || way.Condition is Conditions.FlagSet flag && setFlags.Contains(flag.Flag), way.Id);
        }
    }

    private static readonly Regex TopNode = new(@"\[node name=""([^""]+)""[^\]]*parent=""\.""");

    [Fact]
    public void RegionScenes_HaveNoRepeatedNodeNames()
    {
        // Dois nós com o mesmo nome no mesmo pai: o Godot renomeia um e o recurso dele vaza ao fechar (a barca, 31/out).
        foreach (var region in Regions().Values)
        {
            var names = TopNode.Matches(SceneText(region)).Select(m => m.Groups[1].Value).ToList();
            Assert.True(names.Count == names.Distinct().Count(),
                $"{region.Id}: {string.Join(", ", names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key))}");
        }
    }

    private static IEnumerable<string> FlagsRead(Conditions.Condition? condition) => condition switch
    {
        Conditions.AllOf all => all.Items.SelectMany(FlagsRead),
        Conditions.AnyOf any => any.Items.SelectMany(FlagsRead),
        Conditions.NotOf not => FlagsRead(not.Inner),
        Conditions.FlagSet flag => new[] { flag.Flag },
        _ => Enumerable.Empty<string>(),
    };

    private static IEnumerable<T> Load<T>(string folder, Func<string, T> read) =>
        Directory.GetFiles(Path.Combine(Root, "content", folder), "*.json").Select(f => read(File.ReadAllText(f)));

    [Fact]
    public void EveryFlagThatTheStoryWaitsFor_IsSetSomewhere()
    {
        // Uma Pesada, um encontro, a introdução de uma Trívia ou uma missão que esperam uma flag que ninguém liga
        // nunca acontecem. Quem liga: um efeito de conversa ou a consequência de uma missão.
        var missions = Load("missions", Data.MissionData.FromJson).ToList();
        var set = Load("dialogue", Dialogue.DialogueData.FromJson)
            .SelectMany(g => g.Nodes.Values.SelectMany(n => n.Effects))
            .Concat(missions.SelectMany(m => m.Consequences))
            .OfType<Dialogue.SetFlag>()
            .Select(f => f.Flag)
            .ToHashSet();
        var read = Load("recollections", Data.RecollectionData.FromJson).Select(r => r.Heavy?.Gate)
            .Concat(Load("encounters", WorldData.EncounterFromJson).Select(e => e.Requires))
            .Concat(Load("trivias", WorldData.TriviaFromJson).Select(t => t.Intro?.When))
            .Concat(missions.SelectMany(m => m.Stages.Select(s => s.When).Prepend(m.Hook)))
            .SelectMany(FlagsRead)
            .Distinct();

        Assert.All(read, flag => Assert.Contains(flag, set));
    }

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "project.godot")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("project.godot não encontrado.");
    }
}
