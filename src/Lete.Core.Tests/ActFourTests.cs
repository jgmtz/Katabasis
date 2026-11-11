using Lete.Core.Combat;
using Lete.Core.Data;

namespace Lete.Core.Tests;

/// <summary>As Fúrias, Trocar e o espelho do protagonista (decisões do João, 11/nov/2026).</summary>
public class ActFourTests
{
    [Fact]
    public void Reading_GrowsWithLeftMemoriesAndBrokenOaths_UpToTheCap()
    {
        var reading = new ShadeReading(0.1f, 0.2f, 1.5f, null);
        Assert.Equal(1f, reading.Multiplier(0, 0));
        Assert.Equal(1.4f, reading.Multiplier(2, 1), 3);
        Assert.Equal(1.5f, reading.Multiplier(6, 3));
        Assert.Equal(1f, reading.Multiplier(-2, 0));
    }

    [Fact]
    public void Reading_Validates()
    {
        Assert.NotEmpty(new ShadeReading(-0.1f, 0f, 1f, null).Validate());
        Assert.NotEmpty(new ShadeReading(0f, 0f, 0.5f, null).Validate());
        Assert.Empty(new ShadeReading(0.1f, 0.1f, 2f, "k").Validate());
    }

    [Fact]
    public void ShadeJson_ReadsReadingAndExchange()
    {
        const string json = """
            {
              "reading": { "per_heavy_left": { "_tbd": true, "value": 0.1 }, "per_oath_broken": 0.2, "max_multiplier": 2, "read_key": "k" },
              "exchange": { "requires": { "heavy_memory": { "id": "a_venda", "state": "face" } }, "flag": "trocou",
                            "description_key": "d", "closed_key": "c", "text_keys": ["t1", "t2"] }
            }
            """;
        var reading = ShadeData.ReadingFromJson(json)!;
        Assert.Equal(0.2f, reading.PerOathBroken);
        Assert.Equal("k", reading.ReadKey);
        var exchange = ShadeData.ExchangeFromJson(json)!;
        Assert.Equal("trocou", exchange.Flag);
        Assert.Equal(new[] { "t1", "t2" }, exchange.TextKeys);
        Assert.Null(ShadeData.ReadingFromJson("{}"));
        Assert.Null(ShadeData.ExchangeFromJson("{}"));
    }

    [Fact]
    public void Cast_MirrorMustPointToSomeoneInTheCast()
    {
        const string json = """
            { "id": "elenco", "cast": [
              { "id": "sombra_f", "name_key": "n", "kind": "protagonist", "height": 1, "battle_back": "b" },
              { "id": "esquecido_f", "name_key": "n", "kind": "shade", "height": 1, "battle_front": "b", "mirror_of": "sombra_f" },
              { "id": "esquecido_x", "name_key": "n", "kind": "shade", "height": 1, "battle_front": "b", "mirror_of": "ninguem" }
            ] }
            """;
        var error = Assert.ThrowsAny<Exception>(() => CastData.FromJson(json));
        Assert.Contains("ninguem", error.Message);
    }
}
