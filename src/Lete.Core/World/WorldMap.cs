using System.Text.Json;

namespace Lete.Core.World;

/// <summary>Um lugar do mapa: a região, o centro da miniatura no pergaminho (em pixels da arte) e a imagem dela.</summary>
public sealed record MapPlace(string RegionId, int X, int Y, string Image);

/// <summary>
/// O mapa do menu (<c>content/rules/mapa.json</c>; decisão do João, 13/nov/2026, PENDENCIAS 115): os lugares e as
/// passagens entre eles. Não tem marcador de missão nem de objetivo: o Diário continua sendo o único guia.
/// </summary>
public sealed record WorldMap(IReadOnlyList<MapPlace> Places, IReadOnlyList<(string A, string B)> Links);

public enum MapPlaceState
{
    /// <summary>Já esteve lá: aparece com a miniatura e o nome.</summary>
    Visited,

    /// <summary>Vizinho de um lugar visitado: um vulto na névoa, sem nome.</summary>
    Fogged,
}

/// <summary>Um lugar como o mapa o mostra agora; <paramref name="Current"/> marca onde o jogador está.</summary>
public sealed record MapPlaceView(MapPlace Place, MapPlaceState State, bool Current);

/// <summary>Uma passagem desenhada; <paramref name="Known"/>: as duas pontas já foram visitadas.</summary>
public sealed record MapLinkView(MapPlace A, MapPlace B, bool Known);

public sealed record MapView(IReadOnlyList<MapPlaceView> Places, IReadOnlyList<MapLinkView> Links);

public static class WorldMaps
{
    /// <summary>
    /// O que o mapa mostra: os lugares visitados, os vizinhos deles na névoa e mais nada (o resto do mundo não aparece,
    /// para o mapa não contar quanto falta). Uma passagem aparece se pelo menos uma ponta foi visitada.
    /// </summary>
    public static MapView View(WorldMap map, IReadOnlyCollection<string> visited, string? current)
    {
        var known = map.Places.Where(p => visited.Contains(p.RegionId)).Select(p => p.RegionId).ToHashSet();
        var fogged = map.Links
            .SelectMany(l => new[] { (From: l.A, To: l.B), (From: l.B, To: l.A) })
            .Where(l => known.Contains(l.From) && !known.Contains(l.To))
            .Select(l => l.To)
            .ToHashSet();
        var byId = map.Places.ToDictionary(p => p.RegionId);
        var places = map.Places
            .Where(p => known.Contains(p.RegionId) || fogged.Contains(p.RegionId))
            .Select(p => new MapPlaceView(p, known.Contains(p.RegionId) ? MapPlaceState.Visited : MapPlaceState.Fogged,
                p.RegionId == current && known.Contains(p.RegionId)))
            .ToList();
        var links = map.Links
            .Where(l => known.Contains(l.A) || known.Contains(l.B))
            .Select(l => new MapLinkView(byId[l.A], byId[l.B], known.Contains(l.A) && known.Contains(l.B)))
            .ToList();
        return new MapView(places, links);
    }

    public static WorldMap FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var places = root.GetProperty("places").EnumerateArray()
            .Select(p => new MapPlace(
                p.GetProperty("region").GetString()!,
                p.GetProperty("x").GetInt32(),
                p.GetProperty("y").GetInt32(),
                p.GetProperty("image").GetString()!))
            .ToList();
        var ids = places.Select(p => p.RegionId).ToHashSet();
        if (ids.Count != places.Count)
        {
            throw new FormatException("O mapa tem uma região repetida.");
        }

        var links = root.GetProperty("links").EnumerateArray()
            .Select(l => (A: l[0].GetString()!, B: l[1].GetString()!))
            .ToList();
        foreach (var (a, b) in links)
        {
            if (!ids.Contains(a) || !ids.Contains(b))
            {
                throw new FormatException($"O mapa liga '{a}' a '{b}', mas um dos dois não é um lugar dele.");
            }
        }

        return new WorldMap(places, links);
    }
}
