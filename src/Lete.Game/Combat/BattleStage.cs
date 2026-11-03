using System.Collections.Generic;
using System.Linq;
using Godot;
using Lete.Core.Combat;
using Lete.Core.Data;
using Lete.Game.World;

namespace Lete.Game.Combat;

/// <summary>
/// Palco da batalha: lê a configuração (<c>content/battle</c>) e o elenco (<c>content/characters/elenco.json</c>),
/// põe grupo e sombras nas posições do layout, ajusta a câmera para a altura na tela e aplica a arte.
/// Precisa de um filho "CameraRig".
/// </summary>
public partial class BattleStage : Node3D
{
    public const string CastPath = "res://content/characters/elenco.json";
    public const string FramingPath = "res://content/rules/enquadramento_batalha.json";

    [Export(PropertyHint.File, "*.json")] public string SetupPath { get; set; } = "res://content/battle/teste_elenco.json";
    [Export] public string ArtRoot { get; set; } = "res://art/sprites/batalha";

    /// <summary>Faixas de cinema das lutas de chefe (desenhadas embaixo do HUD).</summary>
    [Export] public Color LetterboxColor { get; set; } = Colors.Black;

    /// <summary>Segundos para as faixas de cinema entrarem no começo da luta.</summary>
    [Export] public float LetterboxInSeconds { get; set; } = 0.6f;

    /// <summary>O enquadramento da luta (<see cref="BattleSetup.Framing"/>): câmera, faixas de cinema e folga.</summary>
    public BattleFraming Framing { get; private set; } = null!;

    /// <summary>
    /// Altura na tela (1080p) de um participante de altura 1,0, pelo enquadramento. 640 = 160 × 4 nas lutas normais
    /// (a arte de 160 px com o mesmo 4× do mapa); 320 = 160 × 2 nas de chefe (câmera aberta).
    /// </summary>
    public float ScreenHeightPx => Framing.ScreenHeightPx;

    /// <summary>A altura de referência do HUD: a das lutas normais (o HUD foi desenhado para o encolhimento delas).</summary>
    public float HudScreenHeightPx { get; private set; }

    [Export] public BattleAnimTuning Tuning { get; set; } = null!;

    /// <summary>
    /// O grupo segue o save: o corpo do protagonista-sombra escolhido e o companheiro (ou o de <c>-- --companheiro=id</c>);
    /// sem escolha, fica quem está no arquivo da batalha.
    /// </summary>
    [Export] public bool FollowWorldCompanion { get; set; }

    public BattleSetup Setup { get; private set; } = null!;
    public IReadOnlyDictionary<string, CastMember> Cast { get; private set; } = null!;
    public List<(BattleActor Node, BattleActorArt Art)> Party { get; } = new();
    public List<(BattleActor Node, BattleActorArt Art)> Enemies { get; } = new();
    public Camera3D Camera { get; private set; } = null!;
    public int Style { get; private set; }

    /// <summary>O tamanho da câmera pelo enquadramento, antes do ajuste para todos caberem.</summary>
    private float _baseCameraSize;
    private float _baseWorldPerScreenPixel;

    private BattleSetup WithWorldCompanion(BattleSetup setup)
    {
        var state = GameRoot.Instance.State;
        if (System.Array.Find(OS.GetCmdlineUserArgs(), x => x.StartsWith("--companheiro=")) is { } arg)
        {
            // Teste: troca direto no estado (no jogo a escolha é uma vez só, por Companions.Choose).
            state.CompanionId = arg["--companheiro=".Length..];
        }

        if (System.Array.Find(OS.GetCmdlineUserArgs(), x => x.StartsWith("--corpo=")) is { } bodyArg)
        {
            // Teste: -- --corpo=sombra_m luta com o outro corpo (e o gênero dos textos acompanha).
            state.ProtagonistId = bodyArg["--corpo=".Length..];
            state.Gender = Cast.TryGetValue(state.ProtagonistId, out var body) && body.Gender == "f"
                ? Lete.Core.State.Gender.Feminine
                : Lete.Core.State.Gender.Masculine;
        }

        BattleActorArt? Replacement(string? id, CastKind kind) =>
            id is not null && Cast.TryGetValue(id, out var member) && member.Kind == kind
                // Sem arte de costas (Dimas, por enquanto), usa a de frente.
                ? member.AsActor(back: member.BackArt is not null, partySide: true)
                : null;

        var companion = Replacement(state.CompanionId, CastKind.Companion);
        var protagonist = Replacement(state.ProtagonistId, CastKind.Protagonist);
        if (companion is null && EncounterTransit.Pending is not null)
        {
            // Uma luta do mapa antes de escolher o companheiro: o lugar dele sai (e a posição dele no layout também).
            var kept = setup.Party.Select((a, i) => (Actor: a, Index: i))
                .Where(p => !(Cast.TryGetValue(p.Actor.Id, out var m) && m.Kind == CastKind.Companion))
                .ToList();
            setup = setup with
            {
                Party = kept.Select(p => p.Actor).ToList(),
                Layout = setup.Layout with { Party = kept.Select(p => setup.Layout.Party[p.Index]).ToList() },
            };
        }

        return setup with
        {
            Party = setup.Party.Select(a => Cast.TryGetValue(a.Id, out var m) ? m.Kind switch
            {
                CastKind.Companion => companion ?? a,
                CastKind.Protagonist => protagonist ?? a,
                _ => a,
            } : a).ToList(),
        };
    }

    public override void _Ready()
    {
        // Vindo de um encontro do mapa, a configuração é a dele.
        if (EncounterTransit.Pending is { } encounter)
        {
            SetupPath = $"{SetupPath.GetBaseDir()}/{encounter.Battle}.json";
        }

        // -- --setup=<id> troca a configuração (ex.: teste_ataque), sem mexer na cena.
        var setupArg = System.Array.Find(OS.GetCmdlineUserArgs(), x => x.StartsWith("--setup="));
        if (setupArg is not null)
        {
            SetupPath = $"{SetupPath.GetBaseDir()}/{setupArg["--setup=".Length..]}.json";
        }

        Cast = CastData.FromJson(FileAccess.GetFileAsString(CastPath));
        Setup = BattleData.FromJson(FileAccess.GetFileAsString(SetupPath), Cast);
        if (FollowWorldCompanion)
        {
            Setup = WithWorldCompanion(Setup);
        }

        var framings = BattleData.FramingsFromJson(FileAccess.GetFileAsString(FramingPath));
        Framing = framings[Setup.Framing];
        HudScreenHeightPx = framings[BattleFraming.Normal].ScreenHeightPx;

        var rig = GetNode<CameraRig>("CameraRig");
        Camera = rig.Camera;
        var viewportHeight = UI.Screen.LogicalHeight(this);
        Camera.Size = _baseCameraSize = viewportHeight / ScreenHeightPx;
        var worldPerScreenPixel = _baseWorldPerScreenPixel = Camera.Size / viewportHeight;

        var basis = Camera.GlobalBasis;
        var right = new Vector3(basis.X.X, 0, basis.X.Z).Normalized();
        var forward = new Vector3(-basis.Z.X, 0, -basis.Z.Z).Normalized();

        BattleActor Spawn(BattleActorArt art, System.Numerics.Vector2 ground, bool spirit)
        {
            var actor = new BattleActor { Name = art.Id, Tuning = Tuning, WorldPerScreenPixel = worldPerScreenPixel, IsSpirit = spirit };
            AddChild(actor);
            actor.Position = right * ground.X + forward * ground.Y;
            actor.FaceCamera(rig.Rotation.Y, Mathf.DegToRad(rig.PitchDegrees));
            actor.SetDepthScale(Setup.Layout.ScaleAt(ground));
            return actor;
        }

        for (var i = 0; i < Setup.Party.Count; i++)
        {
            Party.Add((Spawn(Setup.Party[i], Setup.Layout.Party[i], spirit: false), Setup.Party[i]));
        }

        for (var i = 0; i < Setup.Enemies.Count; i++)
        {
            Enemies.Add((Spawn(Setup.Enemies[i], Setup.Layout.Enemies[i], spirit: true), Setup.Enemies[i]));
        }

        // Para conferir sem jogar: -- --art=<id do estilo> escolhe a arte inicial.
        var artArg = System.Array.Find(OS.GetCmdlineUserArgs(), x => x.StartsWith("--art="));
        var initial = artArg is null ? 0 : Setup.Styles.ToList().FindIndex(st => st.Id == artArg["--art=".Length..]);
        ApplyStyle(System.Math.Max(0, initial));
        // O Sprite3D só calcula a caixa dele no fim do quadro: o ajuste mede depois disso.
        Callable.From(FitToFrame).CallDeferred();
        AddLetterbox();

        foreach (var (node, _) in Party.Concat(Enemies))
        {
            node.StartIdle();
        }
    }

    /// <summary>
    /// Ninguém fica fora do quadro: se a arte de alguém passar da borda (ou entrar nas faixas de cinema), a câmera
    /// se afasta até caber (<see cref="BattleFraming.ZoomToFit"/>). Parte sempre do tamanho do enquadramento, então
    /// volta a fechar quando a arte troca por uma menor.
    /// </summary>
    private void FitToFrame()
    {
        Camera.Size = _baseCameraSize;
        var size = GetViewport().GetVisibleRect().Size;
        var zoom = Framing.ZoomToFit(Party.Concat(Enemies).Select(p => p.Node.ScreenBounds(Camera, size)));
        Camera.Size = _baseCameraSize * zoom;
        foreach (var (node, _) in Party.Concat(Enemies))
        {
            node.WorldPerScreenPixel = _baseWorldPerScreenPixel * zoom;
        }

        if (zoom > 1f)
        {
            GD.Print($"Enquadramento '{Framing.Id}': a câmera abriu {zoom:0.##}× para todos caberem no quadro.");
        }
    }

    /// <summary>As faixas de cinema (lutas de chefe): entram de fora da tela, em cima e embaixo, embaixo do HUD.</summary>
    private void AddLetterbox()
    {
        if (Framing.Letterbox <= 0f)
        {
            return;
        }

        // A camada 0 fica acima do palco 3D e abaixo do HUD (camada 1).
        var layer = new CanvasLayer { Name = "Letterbox", Layer = 0 };
        AddChild(layer);
        var tween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        foreach (var top in new[] { true, false })
        {
            var bar = new ColorRect { Color = LetterboxColor, MouseFilter = Control.MouseFilterEnum.Ignore };
            layer.AddChild(bar);
            bar.AnchorLeft = 0f;
            bar.AnchorRight = 1f;
            bar.AnchorTop = top ? 0f : 1f;
            bar.AnchorBottom = top ? 0f : 1f;
            tween.TweenProperty(bar, top ? "anchor_bottom" : "anchor_top", top ? Framing.Letterbox : 1f - Framing.Letterbox,
                LetterboxInSeconds);
        }
    }

    public void ApplyStyle(int index)
    {
        if (index >= Setup.Styles.Count)
        {
            return;
        }

        Style = index;
        var style = Setup.Styles[index];
        foreach (var (node, art) in Party.Concat(Enemies))
        {
            ApplyArt(node, art);
        }

        // Quantos pixels de tela cada pixel da arte ocupa (inteiro = pixels todos iguais).
        var reference = GD.Load<Texture2D>($"{ArtRoot}/{style.Folder}/{Party[0].Art.Art}.png");
        var scale = ScreenHeightPx * Party[0].Art.Height / reference.GetHeight();
        GD.Print($"Arte '{style.Id}': {reference.GetHeight()} px de altura, {scale:0.##} px de tela por px de arte");
    }

    /// <summary>Troca quem ocupa uma posição do grupo (ou da sombra), mantendo o nó e o lugar no palco.</summary>
    public void Replace(bool enemy, int index, BattleActorArt art)
    {
        var list = enemy ? Enemies : Party;
        list[index] = (list[index].Node, art);
        ApplyArt(list[index].Node, art);
        Callable.From(FitToFrame).CallDeferred();
    }

    /// <summary>Arte, tinta e animação parada desenhada (o flutuar das sombras) de um participante, na versão atual.</summary>
    private void ApplyArt(BattleActor node, BattleActorArt art)
    {
        var style = Setup.Styles[Style];
        var texture = GD.Load<Texture2D>($"{ArtRoot}/{style.Folder}/{art.Art}.png");
        var tint = art.Tint is { } hex ? new Color(hex) : Colors.White;
        node.SetArt(texture, art.Height, style.Smooth, art.FlipH, tint);

        var idle = art.Idle is { } animation && animation.PrefixByStyle.ContainsKey(style.Id)
            ? Enumerable.Range(0, animation.Frames).Select(f => GD.Load<Texture2D>(animation.FramePath(style.Id, f))).ToArray()
            : null;
        node.SetIdleFrames(idle, art.Idle?.Fps ?? 0f);
        if (node.IdleRunning)
        {
            node.StartIdle();
        }
    }
}
