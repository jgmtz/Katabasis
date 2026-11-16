using System.Linq;
using Godot;
using Lete.Game.Scenery;

namespace Lete.Game.Combat;

/// <summary>
/// O ar do lugar onde a luta começou, levado do mapa para o palco da batalha: o chão que estava sob os pés e as cores
/// do ambiente da região. Sem encontro do mapa (as cenas de teste), vale o padrão.
/// </summary>
public sealed record BattleAmbience(string GroundTexture, float GroundBrightness, Color Ambient, Color Mist, string RegionId = "")
{
    /// <summary>O palco das cenas de teste: laje sob a luz fria do Além.</summary>
    public static readonly BattleAmbience Default = new("laje", 1f, new Color(0.44f, 0.52f, 0.55f), new Color(0.3f, 0.38f, 0.4f));

    /// <summary>Chão de terra batida, quando o jogador não está sobre nenhuma mancha de chão.</summary>
    private const string BareGround = "terra";

    public static BattleAmbience Current { get; private set; } = Default;

    /// <summary>Lê o lugar: a mancha de chão mais alta sob <paramref name="at"/> e o ambiente da região.</summary>
    public static void Capture(Node world, Vector3 at, string regionId)
    {
        var sheet = world.FindChildren("*", nameof(Node3D), true, false).OfType<SceneryPiece>()
            .Where(p => p.Kind == "chao" && p.Texture != "" && p.IsVisibleInTree()
                && Mathf.Abs(at.X - p.GlobalPosition.X) <= p.Size.X / 2f
                && Mathf.Abs(at.Z - p.GlobalPosition.Z) <= p.Size.Z / 2f)
            .OrderByDescending(p => p.GlobalPosition.Y)
            .FirstOrDefault();
        var environment = world.FindChildren("*", nameof(WorldEnvironment), true, false).OfType<WorldEnvironment>()
            .FirstOrDefault()?.Environment;
        Current = new BattleAmbience(
            sheet?.Texture ?? BareGround,
            sheet?.Tint.V ?? 1f,
            environment?.AmbientLightColor ?? Default.Ambient,
            environment is { FogEnabled: true } ? environment.FogLightColor : Default.Mist,
            regionId);
    }
}
