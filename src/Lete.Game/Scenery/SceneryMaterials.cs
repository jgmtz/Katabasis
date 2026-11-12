using System.Collections.Generic;
using Godot;

namespace Lete.Game.Scenery;

/// <summary>
/// Os materiais do cenário, um por textura (guardados para as peças dividirem). O nome é o do arquivo em
/// <see cref="Folder"/>; com <c>@mundo</c> no fim, a textura é projetada no espaço do mundo (rochas, paredões), e não no
/// da peça. Dois nomes especiais não têm textura: <see cref="Glow"/> (aceso, sem sombra) e <see cref="Dark"/> (um vão).
/// </summary>
public static class SceneryMaterials
{
    public const string Folder = "res://art/tiles/cenario";
    public const string ShaderFolder = "res://src/Lete.Game/Scenery";

    /// <summary>Superfície acesa (janela com luz, brasa): a cor do vértice, sem sombra, forte o bastante para brilhar.</summary>
    public const string Glow = "luz";

    /// <summary>Vão escuro (porta aberta, boca do forno, fundo do poço): a cor do vértice, sem luz.</summary>
    public const string Dark = "escuro";

    private const string WorldSuffix = "@mundo";

    /// <summary>Quanto as paredes escurecem junto ao chão (0 a 1).</summary>
    private const float WallBaseDarkening = 0.4f;

    /// <summary>Quanto uma superfície acesa passa do branco (para o brilho da cena pegar).</summary>
    private const float GlowEnergy = 1.7f;

    /// <summary>A rocha bruta é muito escura para se ler de longe: sobe um pouco (a cor do vértice não passa de 1).</summary>
    private const float RockBrightness = 1.5f;

    private static readonly HashSet<string> Walls = new() { "reboco", "muro", "marmore" };
    private static readonly Dictionary<string, Material> Cache = new();

    public static Texture2D Texture(string name) => GD.Load<Texture2D>($"{Folder}/{name}.png");

    public static Shader Shader(string name) => GD.Load<Shader>($"{ShaderFolder}/{name}.gdshader");

    public static Material For(string key)
    {
        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        Material material;
        if (key is Glow or Dark)
        {
            material = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                VertexColorUseAsAlbedo = true,
                // As cores das peças são escritas como cores de tela (sRGB), não lineares.
                VertexColorIsSrgb = true,
                DisableFog = key == Dark,
                AlbedoColor = key == Glow ? new Color(GlowEnergy, GlowEnergy, GlowEnergy) : Colors.White,
            };
        }
        else
        {
            var world = key.EndsWith(WorldSuffix);
            var name = world ? key[..^WorldSuffix.Length] : key;
            var shader = new ShaderMaterial { Shader = Shader("cenario") };
            shader.SetShaderParameter("albedo", Texture(name));
            shader.SetShaderParameter("mundo", world);
            shader.SetShaderParameter("escurecer_base", Walls.Contains(name) ? WallBaseDarkening : 0f);
            shader.SetShaderParameter("brilho", name == "pedra" ? RockBrightness : 1f);
            material = shader;
        }

        Cache[key] = material;
        return material;
    }

    /// <summary>Material novo (não dividido) de um dos shaders do cenário, para quem ajusta os parâmetros por peça.</summary>
    public static ShaderMaterial Own(string shader) => new() { Shader = Shader(shader) };
}
