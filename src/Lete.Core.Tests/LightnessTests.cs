using Lete.Core.Data;
using Lete.Core.Recollections;

namespace Lete.Core.Tests;

public class LightnessTests
{
    [Fact]
    public void Plus_SumsEveryKindOfLightness()
    {
        var sum = new Lightness(10f, 1).Plus(new Lightness(0f, 2, BreathMax: 15f, ParryWindow: 0.1f, DodgeBox: 0.2f));
        Assert.Equal(new Lightness(10f, 3, 15f, 0.1f, 0.2f), sum);
    }

    [Fact]
    public void Leave_ReadsOnlyTheLightnessItGives()
    {
        var recollection = RecollectionData.FromJson("""
            { "id": "a_venda", "kind": "feast", "scope": "heavy", "order": 3, "title_key": "t", "skill": "s",
              "warning_key": "w", "surface_key": "a", "stages": { "full": { "text_key": "v" } },
              "leave": { "text_key": "d", "_leveza": "nota", "breath_max": { "_tbd": true, "value": 15 } } }
            """);
        Assert.Equal(new Lightness(0f, 0, BreathMax: 15f), recollection.Heavy!.Lightness);
    }

    [Fact]
    public void Leave_RejectsNegativeLightness() =>
        Assert.ThrowsAny<FormatException>(() => RecollectionData.FromJson("""
            { "id": "x", "kind": "feast", "scope": "heavy", "order": 1, "title_key": "t", "skill": "s",
              "warning_key": "w", "surface_key": "a", "stages": { "full": { "text_key": "v" } },
              "leave": { "text_key": "d", "dodge_box": -0.1 } }
            """));
}
