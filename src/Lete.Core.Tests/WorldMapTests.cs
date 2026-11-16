using Lete.Core.World;

namespace Lete.Core.Tests;

public class WorldMapTests
{
    private static readonly WorldMap Map = WorldMaps.FromJson("""
        {
          "places": [
            { "region": "a", "x": 10, "y": 10, "image": "a.png" },
            { "region": "b", "x": 20, "y": 10, "image": "b.png" },
            { "region": "c", "x": 30, "y": 10, "image": "c.png" },
            { "region": "d", "x": 40, "y": 10, "image": "d.png" }
          ],
          "links": [["a", "b"], ["b", "c"], ["c", "d"]]
        }
        """);

    [Fact]
    public void View_ShowsVisited_AndOnlyTheirNeighborsInTheFog()
    {
        var view = WorldMaps.View(Map, new[] { "a", "b" }, "b");
        Assert.Equal(new[] { "a", "b", "c" }, view.Places.Select(p => p.Place.RegionId));
        Assert.Equal(new[] { MapPlaceState.Visited, MapPlaceState.Visited, MapPlaceState.Fogged }, view.Places.Select(p => p.State));
    }

    [Fact]
    public void View_MarksOnlyTheCurrentRegion()
    {
        var view = WorldMaps.View(Map, new[] { "a", "b" }, "b");
        Assert.Equal(new[] { false, true, false }, view.Places.Select(p => p.Current));
    }

    [Fact]
    public void View_DrawsALink_WhenAtLeastOneEndWasVisited()
    {
        var view = WorldMaps.View(Map, new[] { "a", "b" }, "a");
        Assert.Equal(2, view.Links.Count);
        Assert.True(view.Links[0].Known);
        Assert.False(view.Links[1].Known);
    }

    [Fact]
    public void View_IsEmpty_BeforeAnyVisit()
    {
        var view = WorldMaps.View(Map, Array.Empty<string>(), null);
        Assert.Empty(view.Places);
        Assert.Empty(view.Links);
    }

    [Fact]
    public void View_IgnoresVisitedRegionsThatAreNotOnTheMap()
    {
        var view = WorldMaps.View(Map, new[] { "sala_teste" }, "sala_teste");
        Assert.Empty(view.Places);
    }

    [Fact]
    public void FromJson_RejectsALinkToAnUnknownPlace()
    {
        Assert.Throws<FormatException>(() => WorldMaps.FromJson(
            """{ "places": [{ "region": "a", "x": 0, "y": 0, "image": "a.png" }], "links": [["a", "z"]] }"""));
    }
}
