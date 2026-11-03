using Lete.Core.World;

namespace Lete.Core.Tests;

public class TideTableTests
{
    private const string Region = """
        { "id": "tenaro", "name_key": "regiao.tenaro.nome", "scene": "res://x.tscn", "entry": "Entrada",
          "tide": [
            { "step": 1, "arrival_key": "mare.tenaro.1", "stubborn": 0.15 },
            { "step": 3, "arrival_key": "mare.tenaro.3", "stubborn": { "_tbd": true, "value": 0.25 } }
          ] }
        """;

    [Fact]
    public void LayerAt_IsTheHighestStepReached_AndStaysAtTheLast()
    {
        var region = WorldData.RegionFromJson(Region);

        Assert.Null(region.LayerAt(0));
        Assert.Equal("mare.tenaro.1", region.LayerAt(1)!.ArrivalKey);
        Assert.Equal("mare.tenaro.1", region.LayerAt(2)!.ArrivalKey);
        Assert.Equal(0.25f, region.LayerAt(3)!.Stubborn);
        // O piso: depois do último degrau da tabela, nada endurece mais.
        Assert.Equal(0.25f, region.LayerAt(9)!.Stubborn);
    }

    [Fact]
    public void RegionWithoutTable_HasNoLayer()
    {
        var region = WorldData.RegionFromJson("""
            { "id": "x", "name_key": "x", "scene": "res://x.tscn", "entry": "Entrada" }
            """);
        Assert.Empty(region.TideLayers);
        Assert.Null(region.LayerAt(5));
    }

    [Theory]
    [InlineData("""[ { "step": 0, "arrival_key": "a", "stubborn": 0 } ]""")]
    [InlineData("""[ { "step": 2, "arrival_key": "a", "stubborn": 0 }, { "step": 1, "arrival_key": "b", "stubborn": 0 } ]""")]
    [InlineData("""[ { "step": 1, "arrival_key": "a", "stubborn": 0.3 }, { "step": 2, "arrival_key": "b", "stubborn": 0.1 } ]""")]
    [InlineData("""[ { "step": 1, "arrival_key": "a", "stubborn": -0.1 } ]""")]
    public void InvalidTable_IsRejected(string tide)
    {
        Assert.Throws<FormatException>(() => WorldData.RegionFromJson(
            $$"""{ "id": "x", "name_key": "x", "scene": "res://x.tscn", "entry": "Entrada", "tide": {{tide}} }"""));
    }
}
