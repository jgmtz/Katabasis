using System.Linq;
using Godot;
using Lete.Game.Scenery;

namespace Lete.Game.Combat;

/// <summary>
/// O palco simples da batalha, feito com as peças da paisagem: uma mancha de chão do lugar do encontro, névoa rasteira
/// e um braseiro ao fundo, que ilumina o chão e dá a direção da luz e das sombras dos participantes. O fundo liso
/// (<c>BattleBackdrop</c>) continua atrás, onde a mancha de chão acaba. Na batalha a unidade do mundo é a altura de um
/// personagem; as peças, feitas em metros, entram reduzidas.
/// </summary>
public partial class BattleSet : Node3D
{
    /// <summary>Texels do chão por unidade do mundo: metade da densidade da arte de batalha (160 px por altura).</summary>
    private const float GroundTexelDensity = 80f;

    /// <summary>Metros por unidade do mundo da batalha (a altura de um personagem).</summary>
    private const float MetersPerUnit = 1.7f;

    /// <summary>Tamanho da mancha de chão (largura na tela, profundidade) e o roído da borda, em unidades.</summary>
    private static readonly Vector2 GroundSize = new(9f, 6.5f);
    private const float GroundEdge = 1.4f;

    /// <summary>Altura visível da câmera nas lutas normais (unidades) e a folga do chão quando ela abre mais.</summary>
    private const float NormalViewHeight = 1.6875f;
    private const float WideViewMargin = 1.4f;

    /// <summary>Onde fica o braseiro no chão do palco (direita da tela, fundo), em unidades: logo fora do quadro, à
    /// esquerda, para a chama não cobrir as barras do HUD; o que se vê é a luz dele no chão e nos corpos.</summary>
    private static readonly Vector2 FireGround = new(-2f, 0.15f);
    private const float FireHeight = 0.55f;
    private const float FireEnergy = 3.4f;
    private const float FireRange = 3.8f;
    private static readonly Color FireTint = new(1f, 0.62f, 0.3f);

    /// <summary>Névoa rasteira: altura e densidade.</summary>
    private const float MistHeight = 0.1f;
    private const float MistDensity = 0.45f;

    /// <summary>
    /// Névoa de profundidade do palco: começa um pouco atrás do centro (onde estão os pés das sombras), fecha até
    /// <see cref="FogMax"/> no fundo, na cor da névoa da região escurecida. Em unidades a partir da câmera.
    /// </summary>
    private const float DefaultCameraDistance = 30f;
    private const float FogBeginsBehind = 0.15f;
    private const float FogEndsBehind = 1.6f;
    private const float FogMax = 0.6f;
    private const float FogDarken = 0.45f;

    /// <summary>Escurecimento do fundo: o que fica em y = <see cref="SinkFrom"/> e o que fica em y = <see cref="SinkTo"/>.</summary>
    private const float NearSink = 0.6f;
    private const float FarSink = 0.38f;
    private const float SinkFrom = 0.3f;
    private const float SinkTo = 1.4f;

    /// <summary>Quanto a cor do ambiente da região pesa na luz ambiente do palco e na tinta dos participantes.</summary>
    private const float AmbientWeight = 0.7f;
    private const float ActorTintWeight = 0.35f;

    /// <summary>Onde está a chama (mundo) e a cor dela.</summary>
    public Vector3 FirePosition { get; private set; }
    public Color FireColor => FireTint;

    /// <summary>A tinta que os participantes recebem do lugar (perto do branco) e a cor da névoa nos pés deles.</summary>
    public Color ActorTint { get; private set; } = Colors.White;
    public Color MistColor { get; private set; }

    /// <summary>O fundo de cada região (<c>content/rules/palco_batalha.json</c>).</summary>
    public const string BackdropPath = "res://content/rules/palco_batalha.json";

    /// <summary>
    /// As peças do fundo da região (ou as do padrão, sem região ou sem entrada para ela): x e y no chão do palco, em
    /// unidades; as peças são feitas em metros e entram reduzidas.
    /// </summary>
    private void AddBackdrop(string regionId, Vector3 right, Vector3 forward, float yaw)
    {
        if (Json.ParseString(FileAccess.GetFileAsString(BackdropPath)).AsGodotDictionary() is not { } data)
        {
            return;
        }

        var regions = data["regioes"].AsGodotDictionary();
        var items = regionId != "" && regions.ContainsKey(regionId)
            ? regions[regionId].AsGodotArray()
            : data["padrao"].AsGodotArray();
        var backdrop = new Node3D { Name = "Fundo" };
        AddChild(backdrop);

        var index = 0;
        foreach (var entry in items)
        {
            var item = entry.AsGodotDictionary();
            float Num(string key, float fallback = 0f) => item.ContainsKey(key) ? (float)item[key].AsDouble() : fallback;
            var kind = item["peca"].AsString();
            var at = right * Num("x") + forward * Num("y");
            var turn = yaw + Mathf.DegToRad(Num("rot"));
            var footprint = new Vector3(Num("largura") * MetersPerUnit, 0f, Num("profundidade") * MetersPerUnit);
            index++;

            Node3D node = kind switch
            {
                "flora" => new FloraField
                {
                    Kind = item["folha"].AsString(),
                    Area = new Vector2(footprint.X, footprint.Z),
                    Count = (int)Num("quantidade", 120f),
                    Seed = index * 97,
                    CardScale = 1f / MetersPerUnit,
                },
                "agua" => new SceneryPiece
                {
                    Kind = "agua",
                    Variant = (int)Num("variante"),
                    Size = footprint,
                    Edge = 0.6f,
                },
                _ => new SceneryPiece
                {
                    Kind = kind,
                    Variant = (int)Num("variante"),
                    Seed = (int)Num("semente", index),
                    Size = item.ContainsKey("tamanho")
                        ? new Vector3((float)item["tamanho"].AsGodotArray()[0].AsDouble(), (float)item["tamanho"].AsGodotArray()[1].AsDouble(),
                            (float)item["tamanho"].AsGodotArray()[2].AsDouble())
                        : Vector3.Zero,
                    Lit = false,
                },
            };

            if (node is SceneryPiece piece && item.ContainsKey("tinta"))
            {
                piece.Tint = new Color(item["tinta"].AsString());
            }

            node.Name = $"{kind}{index}";
            // Água e flora ficam rentes ao chão do palco (a água um fio acima da mancha de chão).
            node.Position = at + (kind == "agua" ? Vector3.Up * 0.004f : Vector3.Zero);
            node.Rotation = new Vector3(0f, turn, 0f);
            node.Scale = Vector3.One / MetersPerUnit;
            backdrop.AddChild(node);
            SinkIntoBackground(node, Num("y"));
        }
    }

    /// <summary>
    /// O fundo afunda no escuro conforme se afasta: as peças mais ao fundo ficam mais escuras, para os participantes
    /// saltarem à frente (a câmera olha de cima, então a névoa de profundidade sozinha não basta para o alto delas).
    /// </summary>
    private static void SinkIntoBackground(Node3D node, float depth)
    {
        var sink = Mathf.Lerp(NearSink, FarSink, Mathf.Clamp((depth - SinkFrom) / (SinkTo - SinkFrom), 0f, 1f));
        foreach (var geometry in node.FindChildren("*", nameof(GeometryInstance3D), true, false).OfType<GeometryInstance3D>())
        {
            if (geometry.MaterialOverride is ShaderMaterial { Shader: { } shader } && !UsesSink(shader))
            {
                continue;
            }

            geometry.SetInstanceShaderParameter("recuo", sink);
        }
    }

    private static bool UsesSink(Shader shader) =>
        shader == SceneryMaterials.Shader("cenario") || shader == SceneryMaterials.Shader("flora");

    /// <summary>Monta o palco;<paramref name="right"/> e <paramref name="forward"/> são os eixos da tela no chão.</summary>
    public void Build(BattleAmbience ambience, Vector3 right, Vector3 forward, float yaw, float viewHeight)
    {
        // Nas lutas de chefe a câmera abre: o chão cresce junto, para a borda dele não entrar no quadro.
        var ground = GroundSize * Mathf.Max(1f, viewHeight / NormalViewHeight * WideViewMargin);
        RenderingServer.GlobalShaderParameterSet("texel_density", GroundTexelDensity);
        MistColor = ambience.Mist;
        var cast = ambience.Ambient / Mathf.Max(ambience.Ambient.V, 0.001f);
        ActorTint = Colors.White.Lerp(new Color(cast.R, cast.G, cast.B), ActorTintWeight);

        AddChild(new SceneryPiece
        {
            Name = "Chao",
            Kind = "chao",
            Texture = ambience.GroundTexture,
            Tint = Color.FromHsv(0f, 0f, ambience.GroundBrightness),
            Size = new Vector3(ground.X, 0f, ground.Y),
            Edge = GroundEdge,
            Position = new Vector3(0f, 0.001f, 0f),
            Rotation = new Vector3(0f, yaw, 0f),
        });
        AddChild(new SceneryPiece
        {
            Name = "Nevoa",
            Kind = "nevoa",
            Tint = ambience.Mist,
            Size = new Vector3(ground.X * 1.3f, MistDensity, ground.Y * 1.3f),
            Position = new Vector3(0f, MistHeight, 0f),
            Rotation = new Vector3(0f, yaw, 0f),
        });

        var fire = right * FireGround.X + forward * FireGround.Y;
        AddChild(new SceneryPiece
        {
            Name = "Braseiro",
            Kind = "braseiro",
            Lit = false,
            Position = fire,
            Scale = Vector3.One / MetersPerUnit,
        });
        FirePosition = fire + Vector3.Up * FireHeight;
        AddChild(new Flicker
        {
            Name = "LuzBraseiro",
            LightColor = FireTint,
            BaseEnergy = FireEnergy,
            OmniRange = FireRange,
            Position = FirePosition,
        });

        // A luz ambiente do fundo puxa para a cor da região (o Tártaro em brasa, o Luto em azul).
        AddBackdrop(ambience.RegionId, right, forward, yaw);

        // O ambiente é copiado antes: o recurso da cena é dividido entre as lutas.
        if (GetTree().CurrentScene?.FindChild("WorldEnvironment", true, false) is WorldEnvironment { Environment: { } shared } holder)
        {
            var environment = (Godot.Environment)shared.Duplicate();
            environment.AmbientLightColor = environment.AmbientLightColor.Lerp(ambience.Ambient, AmbientWeight);

            // O fundo se perde na névoa do lugar conforme se afasta dos participantes, que ficam à frente dela.
            var stageDepth = GetViewport().GetCamera3D()?.GlobalPosition.Length() ?? DefaultCameraDistance;
            environment.FogEnabled = true;
            environment.FogMode = Godot.Environment.FogModeEnum.Depth;
            environment.FogDepthBegin = stageDepth + FogBeginsBehind;
            environment.FogDepthEnd = stageDepth + FogEndsBehind;
            environment.FogDepthCurve = 1f;
            environment.FogDensity = FogMax;
            environment.FogLightColor = ambience.Mist.Darkened(FogDarken);
            environment.FogLightEnergy = 1f;
            environment.FogSkyAffect = 0f;
            holder.Environment = environment;
        }
    }
}
