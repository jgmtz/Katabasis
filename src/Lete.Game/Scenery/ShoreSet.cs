using Godot;

namespace Lete.Game.Scenery;

/// <summary>
/// O cenário da margem do rio diante da gruta de Cabo Tênaro, montado com as peças da paisagem (<see cref="SceneryPiece"/>
/// e <see cref="FloraField"/>): é onde a abertura acontece e o fundo da tela de título (pedido do João, 13/nov/2026,
/// PENDENCIAS 116). O rio corre ao longo do Z, a oeste da linha da margem; a boca da gruta fica ao norte, entre duas
/// tochas; os paredões fecham o norte e o leste.
/// </summary>
public static class ShoreSet
{
    /// <summary>Comprimento da margem (Z) e largura da terra firme e do rio (X), em metros.</summary>
    private const float Length = 44f;
    private const float LandWidth = 30f;
    private const float RiverWidth = 60f;

    /// <summary>Onde ficam a parede da gruta (Z) e o paredão do leste (X).</summary>
    private const float CaveWallZ = -11.5f;
    private const float EastWallX = 11f;

    /// <summary>A faixa de juncos da beira: largura e quantos.</summary>
    private const float ReedBand = 1.6f;
    private const int ReedCount = 520;

    private static readonly Color MistTint = new(0.16f, 0.2f, 0.24f);

    /// <summary>A terra da margem, molhada e escura (a lâmina de chão usa o brilho da tinta).</summary>
    private static readonly Color DarkEarth = new(0.55f, 0.55f, 0.55f);

    /// <summary>
    /// Monta o cenário dentro de <paramref name="root"/>. <paramref name="bankX"/> é a linha da margem,
    /// <paramref name="waterY"/> a altura da água; em volta de <paramref name="clearing"/> (X, Z) não nascem juncos, num
    /// raio de <paramref name="clearRadius"/>, para a vista do rio ficar livre. Sem <paramref name="eastWall"/>, o
    /// paredão do leste não entra (visto pela câmera da exploração, ele taparia a cena).
    /// </summary>
    public static void Build(Node3D root, float bankX, float waterY, Vector2 clearing, float clearRadius, bool eastWall = true)
    {
        var landCenterX = bankX + LandWidth / 2f;
        Piece(root, "chao", new Vector3(landCenterX, 0f, 0f), new Vector3(LandWidth, 0f, Length), texture: "terra", edge: 0f, tint: DarkEarth);
        Piece(root, "chao", new Vector3(bankX + 3.5f, 0.012f, 2f), new Vector3(7f, 0f, 22f), texture: "campo", edge: 2.2f);
        Piece(root, "chao", new Vector3(1.5f, 0.016f, -5.5f), new Vector3(5f, 0f, 10f), texture: "laje", edge: 1.6f, seed: 3);
        Piece(root, "chao", new Vector3(6.5f, 0.02f, 6f), new Vector3(8f, 0f, 12f), texture: "campo", edge: 2.6f, seed: 5);
        Piece(root, "agua", new Vector3(bankX - RiverWidth / 2f, waterY, 0f), new Vector3(RiverWidth, 0f, Length));

        // A gruta: o paredão do norte, a boca escura no meio dele e uma tocha de cada lado.
        Piece(root, "penhasco", new Vector3(2f, 0f, CaveWallZ - 1.6f), new Vector3(34f, 13f, 3f), seed: 11);
        Piece(root, "arco", new Vector3(1.5f, 0f, CaveWallZ + 0.4f), new Vector3(3.2f, 4.4f, 1.6f), seed: 12);
        Piece(root, "veu", new Vector3(1.5f, 2f, CaveWallZ + 0.5f), new Vector3(3.2f, 4f, 0.95f), tint: new Color(0.03f, 0.03f, 0.04f));
        Piece(root, "tocha", new Vector3(-1.4f, 0f, CaveWallZ + 2f), new Vector3(1.4f, 2.6f, 9f), seed: 13);
        Piece(root, "tocha", new Vector3(4.4f, 0f, CaveWallZ + 2f), new Vector3(1.4f, 2.6f, 9f), seed: 14);
        if (eastWall)
        {
            Piece(root, "penhasco", new Vector3(EastWallX + 1.6f, 0f, 0f), new Vector3(Length, 11f, 3f), rotation: -90f, seed: 15);
        }

        Piece(root, "arvore_morta", new Vector3(5f, 0f, 4.5f), new Vector3(0f, 3.8f, 0f), seed: 21);
        Piece(root, "arvore_morta", new Vector3(bankX + 1.4f, 0f, -6.5f), new Vector3(0f, 3.2f, 0f), seed: 22);
        Piece(root, "arvore_morta", new Vector3(7.5f, 0f, -4f), new Vector3(0f, 4.2f, 0f), seed: 23);
        Piece(root, "estela", new Vector3(2.6f, 0f, 3.2f), new Vector3(0f, 1.3f, 0f), rotation: -30f, seed: 24, tint: new Color(0.62f, 0.64f, 0.7f));
        Piece(root, "rocha", new Vector3(bankX + 0.8f, 0f, 6.5f), new Vector3(1.6f, 0.8f, 1.2f), seed: 31);
        Piece(root, "rocha", new Vector3(bankX + 0.5f, 0f, -3.4f), new Vector3(1.3f, 0.6f, 1.1f), seed: 32);
        Piece(root, "rocha", new Vector3(4.2f, 0f, -8.6f), new Vector3(2.2f, 1.2f, 1.6f), seed: 33);
        Piece(root, "rocha", new Vector3(8.6f, 0f, 9f), new Vector3(2f, 1.1f, 1.5f), seed: 34);
        Piece(root, "entulho", new Vector3(-0.6f, 0f, -8.4f), new Vector3(1.8f, 0f, 1.4f), seed: 35);

        var avoid = new Godot.Collections.Array<Rect2>
        {
            new(clearing.X - clearRadius, clearing.Y - clearRadius, clearRadius * 2f, clearRadius * 2f),
        };
        root.AddChild(new FloraField
        {
            Name = "Juncos",
            Kind = "capim",
            Position = new Vector3(bankX + ReedBand / 2f, 0f, 0f),
            Area = new Vector2(ReedBand, Length - 4f),
            Count = ReedCount,
            Seed = 41,
            Clump = 0.25f,
            Avoid = avoid,
        });
        root.AddChild(new FloraField
        {
            Name = "Capim",
            Kind = "capim",
            Position = new Vector3(landCenterX - 6f, 0f, 1f),
            Area = new Vector2(16f, Length - 8f),
            Count = 420,
            Seed = 42,
            Clump = 0.7f,
            Avoid = avoid,
        });
        root.AddChild(new FloraField
        {
            Name = "Asfodelos",
            Kind = "asfodelo",
            Position = new Vector3(5f, 0f, 3f),
            Area = new Vector2(10f, 20f),
            Count = 120,
            Seed = 43,
            Clump = 0.5f,
            Avoid = avoid,
        });

        Piece(root, "nevoa", new Vector3(landCenterX - 6f, 0.35f, 0f), new Vector3(20f, 0.45f, Length), tint: MistTint);
        Piece(root, "nevoa", new Vector3(bankX - 14f, waterY + 0.25f, 0f), new Vector3(30f, 0.8f, Length), tint: MistTint);
        Piece(root, "nevoa", new Vector3(bankX - 16f, waterY + 1.3f, 0f), new Vector3(26f, 0.5f, Length), tint: MistTint);
    }

    /// <summary>O ambiente do cenário: noite de lua com névoa, o brilho das tochas e o contraste das regiões.</summary>
    public static Environment MakeEnvironment(float ambientEnergy, float fogDensity) => new()
    {
        BackgroundMode = Environment.BGMode.Color,
        BackgroundColor = new Color(0.03f, 0.035f, 0.05f),
        AmbientLightSource = Environment.AmbientSource.Color,
        AmbientLightColor = new Color(0.42f, 0.5f, 0.6f),
        AmbientLightEnergy = ambientEnergy,
        FogEnabled = true,
        FogLightColor = new Color(0.14f, 0.18f, 0.22f),
        FogDensity = fogDensity,
        GlowEnabled = true,
        GlowIntensity = 0.55f,
        GlowStrength = 0.9f,
        GlowBloom = 0.04f,
        GlowHdrThreshold = 1.05f,
        TonemapMode = Environment.ToneMapper.Filmic,
        AdjustmentEnabled = true,
        AdjustmentContrast = 1.08f,
        AdjustmentSaturation = 1.08f,
    };

    private static void Piece(Node3D root, string kind, Vector3 position, Vector3 size, float rotation = 0f, int seed = 1,
        Color? tint = null, string texture = "", float? edge = null)
    {
        var piece = new SceneryPiece
        {
            Kind = kind,
            Seed = seed,
            Size = size,
            Position = position,
            RotationDegrees = new Vector3(0f, rotation, 0f),
            Texture = texture,
        };
        if (tint is { } color)
        {
            piece.Tint = color;
        }

        if (edge is { } eroded)
        {
            piece.Edge = eroded;
        }

        root.AddChild(piece);
    }
}
