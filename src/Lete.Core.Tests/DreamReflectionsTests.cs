using Lete.Core.Combat;
using Lete.Core.Data;

namespace Lete.Core.Tests;

/// <summary>O Sonho: distinguir o reflexo verdadeiro (decisão do João, 10/nov/2026).</summary>
public class DreamReflectionsTests
{
    private static readonly DreamRules Rules = new(3, 0.1f, 10f, new[] { "falso.coroa", "falso.navio", "falso.festa" }, "detalhe", "vazio");

    [Fact]
    public void Arrange_OneTrueWithAFullRecollection_FalsesWithInventedOnes()
    {
        var dream = new DreamReflections(Rules);
        dream.Arrange(new[] { "plena.colo" }, Array.Empty<string>(), new Random(1));

        Assert.Equal(3, dream.Current.Count);
        Assert.Single(dream.Current, r => r.IsTrue);
        Assert.Equal("plena.colo", dream.Current.Single(r => r.IsTrue).TitleKey);
        Assert.All(dream.Current.Where(r => !r.IsTrue), r => Assert.StartsWith("falso.", r.TitleKey));
        Assert.Equal(3, dream.Current.Select(r => r.TitleKey).Distinct().Count());
    }

    [Fact]
    public void Arrange_AnUncertainRecollectionConfusesTheClue_AndWithoutFullTheTrueHoldsNothing()
    {
        var dream = new DreamReflections(Rules);
        dream.Arrange(Array.Empty<string>(), new[] { "incerta.agua" }, new Random(2));

        Assert.Null(dream.Current.Single(r => r.IsTrue).TitleKey);
        Assert.Contains(dream.Current, r => !r.IsTrue && r.TitleKey == "incerta.agua");
    }

    [Fact]
    public void StrikingTheTrue_GoesJustBelowTheNextPhase_ThenToZero()
    {
        var dream = new DreamReflections(Rules);
        var resolve = new ShadeResolve(200f);
        var lucidity = new Gauge(50f);
        var phases = new[] { 0.66f, 0.33f };
        dream.Arrange(new[] { "plena" }, Array.Empty<string>(), new Random(3));
        var truth = dream.Current.ToList().FindIndex(r => r.IsTrue);

        Assert.True(dream.Strike(truth, resolve, lucidity, phases).HitTrue);
        Assert.InRange(resolve.Fraction, 0.65f, 0.66f - 1e-6f);
        dream.Strike(truth, resolve, lucidity, phases);
        Assert.InRange(resolve.Fraction, 0.32f, 0.33f - 1e-6f);
        dream.Strike(truth, resolve, lucidity, phases);
        Assert.True(resolve.IsBroken);
        Assert.Equal(50f, lucidity.Current);
    }

    [Fact]
    public void StrikingAFalse_RemakesTheDream_AndCostsLucidity()
    {
        var dream = new DreamReflections(Rules);
        var resolve = new ShadeResolve(200f);
        resolve.Reduce(100f);
        var lucidity = new Gauge(15f);
        dream.Arrange(new[] { "plena" }, Array.Empty<string>(), new Random(4));
        var lie = dream.Current.ToList().FindIndex(r => !r.IsTrue);

        var strike = dream.Strike(lie, resolve, lucidity, new[] { 0.66f });

        Assert.Equal((false, 20f, 10f), (strike.HitTrue, strike.Healed, strike.LucidityCost));
        Assert.Equal(120f, resolve.Current);
        Assert.Equal(5f, lucidity.Current);
        Assert.Throws<ArgumentOutOfRangeException>(() => dream.Strike(7, resolve, lucidity, new[] { 0.66f }));
    }

    [Fact]
    public void Rules_ReadFromTheShade_AndValidate()
    {
        var rules = ShadeData.ReflectionsFromJson("""
            { "reflections": { "count": { "_tbd": true, "value": 3 }, "false_heal": 0.1, "false_lucidity": 10,
                               "false_titles": [ "a", "b" ], "detail_key": "d", "empty_key": "e" } }
            """)!;
        Assert.Equal(3, rules.Count);
        Assert.Null(ShadeData.ReflectionsFromJson("{}"));
        Assert.Throws<FormatException>(() => ShadeData.ReflectionsFromJson("""
            { "reflections": { "count": 3, "false_heal": 0.1, "false_lucidity": 10, "false_titles": [ "a" ],
                               "detail_key": "d", "empty_key": "e" } }
            """));
    }
}
