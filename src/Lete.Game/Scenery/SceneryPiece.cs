using Godot;

namespace Lete.Game.Scenery;

/// <summary>
/// Uma peça do cenário na cena de uma região (paisagem HD-2D, PENDÊNCIA 105): o nó só guarda o tipo, o tamanho, a
/// variante, a semente e a tinta; a malha, a colisão e as luzes são montadas por código ao abrir a cena
/// (<see cref="Pieces"/>). Três tipos são lâminas com material próprio: <c>chao</c> (mancha de chão com a textura
/// <see cref="Texture"/> e a borda roída), <c>agua</c>, <c>nevoa</c> (rasteira) e <c>veu</c> (névoa em pé, num portal).
/// </summary>
public partial class SceneryPiece : Node3D
{
    /// <summary>Quanta luz própria tem o rio de fogo (variante 2 da água).</summary>
    private const float FireGlow = 1.6f;

    /// <summary>Quantas lâminas tem o véu de um portal, e a distância entre elas.</summary>
    private const int VeilLayers = 3;
    private const float VeilLayerGap = 0.28f;

    /// <summary>O tipo da peça (casa, barraca, muro, cipreste...; a lista está em <see cref="Pieces"/>).</summary>
    [Export] public string Kind { get; set; } = "";

    /// <summary>A mesma semente monta sempre a mesma peça.</summary>
    [Export] public int Seed { get; set; }

    /// <summary>Medidas em metros; zero num eixo usa o padrão da peça.</summary>
    [Export] public Vector3 Size { get; set; }

    [Export] public int Variant { get; set; }

    /// <summary>Tinta da peça (o toldo da barraca, o tom da pedra).</summary>
    [Export] public Color Tint { get; set; } = new(1f, 1f, 1f);

    /// <summary>Peças com fogo: se a chama também ilumina (uma luz a mais na cena).</summary>
    [Export] public bool Lit { get; set; } = true;

    /// <summary>Lâmina de chão: o nome da textura (em <see cref="SceneryMaterials.Folder"/>).</summary>
    [Export] public string Texture { get; set; } = "";

    /// <summary>Lâmina de chão: quanto a borda é roída, em metros (0 = borda reta).</summary>
    [Export] public float Edge { get; set; } = 0.8f;

    public override void _Ready()
    {
        switch (Kind)
        {
            case "chao":
                AddSheet(Ground(), castShadow: false);
                break;
            case "agua":
                var water = SceneryMaterials.Own("agua");
                water.SetShaderParameter("ruido", SceneryMaterials.Texture("ruido"));
                water.SetShaderParameter("tinta", Tint.V);
                if (Variant != 0)
                {
                    // Água rasa sobre o chão (ou o rio de fogo): a borda da lâmina é roída, sem linha reta.
                    water.SetShaderParameter("tamanho", new Vector2(Size.X, Size.Z));
                    water.SetShaderParameter("borda", Edge);
                }

                if (Variant == 1)
                {
                    // Água parada e rasa (uma poça, a margem alagada): sem a espuma de quem encosta na margem.
                    water.SetShaderParameter("largura_espuma", 0f);
                    water.SetShaderParameter("corrente", new Vector2(0.02f, 0.01f));
                }
                else if (Variant == 2)
                {
                    // Rio de fogo (o Flegetonte): as mesmas faixas, em brasa, com luz própria.
                    water.SetShaderParameter("fundo", new Color(0.3f, 0.07f, 0.02f));
                    water.SetShaderParameter("meio", new Color(0.78f, 0.3f, 0.06f));
                    water.SetShaderParameter("crista", new Color(1f, 0.72f, 0.25f));
                    water.SetShaderParameter("largura_espuma", 0f);
                    water.SetShaderParameter("corrente", new Vector2(0.09f, 0.02f));
                    water.SetShaderParameter("brasa", FireGlow);
                }

                AddSheet(water, castShadow: false);
                break;
            case "nevoa":
                var mist = SceneryMaterials.Own("nevoa");
                mist.SetShaderParameter("ruido", SceneryMaterials.Texture("ruido"));
                SetMistColor(mist);
                mist.SetShaderParameter("densidade", Size.Y > 0f ? Size.Y : 0.5f);
                AddSheet(mist, castShadow: false);
                break;
            case "veu":
                // Um véu de névoa em pé (a passagem fechada de um portal): largura X, altura Y.
                var veil = SceneryMaterials.Own("nevoa");
                veil.SetShaderParameter("ruido", SceneryMaterials.Texture("ruido"));
                SetMistColor(veil);
                veil.SetShaderParameter("densidade", Size.Z > 0f ? Size.Z : 0.9f);
                veil.SetShaderParameter("em_pe", true);
                for (var layer = 0; layer < VeilLayers; layer++)
                {
                    // Lâminas uma atrás da outra: o desenho do ruído muda com a posição, então elas não coincidem.
                    AddChild(new MeshInstance3D
                    {
                        Name = $"Veu{layer + 1}",
                        Mesh = new QuadMesh { Size = new Vector2(Size.X, Size.Y) },
                        MaterialOverride = veil,
                        CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                        Position = new Vector3(0, 0, (layer - (VeilLayers - 1) / 2f) * VeilLayerGap),
                    });
                }

                break;
            default:
                if (!Pieces.Knows(Kind))
                {
                    GD.PushError($"Peça de cenário '{Kind}' desconhecida ({GetPath()}).");
                    return;
                }

                var builder = new PieceBuilder(Seed);
                Pieces.Build(builder, this);
                builder.Commit(this);
                break;
        }
    }

    /// <summary>A névoa só muda de cor com uma tinta dada (branco = a cor padrão do shader).</summary>
    private void SetMistColor(ShaderMaterial material)
    {
        if (Tint != new Color(1f, 1f, 1f))
        {
            material.SetShaderParameter("cor", Tint);
        }
    }

    private ShaderMaterial Ground()
    {
        var material = SceneryMaterials.Own("chao");
        material.SetShaderParameter("albedo", SceneryMaterials.Texture(Texture));
        material.SetShaderParameter("ruido", SceneryMaterials.Texture("ruido"));
        material.SetShaderParameter("tamanho", new Vector2(Size.X, Size.Z));
        material.SetShaderParameter("borda", Edge);
        material.SetShaderParameter("brilho", Tint.V);
        return material;
    }

    private void AddSheet(Material material, bool castShadow) => AddChild(new MeshInstance3D
    {
        Name = "Lamina",
        Mesh = new PlaneMesh { Size = new Vector2(Size.X, Size.Z) },
        MaterialOverride = material,
        CastShadow = castShadow ? GeometryInstance3D.ShadowCastingSetting.On : GeometryInstance3D.ShadowCastingSetting.Off,
    });
}
