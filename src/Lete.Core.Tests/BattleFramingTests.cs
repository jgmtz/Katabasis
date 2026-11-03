using Lete.Core.Combat;
using Lete.Core.Data;

namespace Lete.Core.Tests;

public class BattleFramingTests
{
    private static readonly BattleFraming Boss = new("chefe", ScreenHeightPx: 320, Letterbox: 0.1f, FitMargin: 0.05f);

    [Fact]
    public void ZoomToFit_IsOne_WhenEveryoneIsInsideTheBand()
    {
        Assert.Equal(1f, Boss.ZoomToFit(new[] { new ScreenRect(0.1f, 0.2f, 0.8f, 0.6f) }));
        Assert.Equal(1f, Boss.ZoomToFit(Array.Empty<ScreenRect>()));
    }

    [Fact]
    public void ZoomToFit_OpensTheCamera_UntilTheTallestFitsBetweenTheBars()
    {
        // Limite de cima: 0,5 - 0,1 (faixa) - 0,05 (folga) = 0,35 do centro. A cabeça a 0,5 acima do centro (topo 0)
        // pede 0,5 / 0,35; a borda direita a 0,45 do centro cabe nos 0,45 da largura.
        var zoom = Boss.ZoomToFit(new[] { new ScreenRect(0.5f, 0f, 0.45f, 0.6f) });
        Assert.Equal(0.5f / 0.35f, zoom, 4);
    }

    [Fact]
    public void ZoomToFit_UsesTheWorstEdge()
    {
        var zoom = Boss.ZoomToFit(new[]
        {
            new ScreenRect(0.4f, 0.1f, 0.2f, 0.8f),
            new ScreenRect(-0.1f, 0.3f, 0.3f, 0.4f),
        });
        Assert.Equal(0.6f / 0.45f, zoom, 4);
    }

    [Fact]
    public void Framings_ParseAndValidate()
    {
        var framings = BattleData.FramingsFromJson("""
            { "framings": [
              { "id": "normal", "screen_height_px": 640, "letterbox": 0, "fit_margin": 0.01 },
              { "id": "chefe", "screen_height_px": { "_tbd": true, "value": 320 }, "letterbox": 0.1, "fit_margin": 0.03 }
            ] }
            """);
        Assert.Equal(320f, framings["chefe"].ScreenHeightPx);
        Assert.Equal(0.1f, framings["chefe"].Letterbox);

        Assert.Throws<FormatException>(() => BattleData.FramingsFromJson("""
            { "framings": [ { "id": "chefe", "screen_height_px": 320, "letterbox": 0.1, "fit_margin": 0.03 } ] }
            """));
        Assert.Throws<FormatException>(() => BattleData.FramingsFromJson("""
            { "framings": [ { "id": "normal", "screen_height_px": 640, "letterbox": 0.4, "fit_margin": 0.01 } ] }
            """));
        Assert.NotEmpty(new BattleFraming("x", 0f, 0f, 0f).Validate());
    }
}
