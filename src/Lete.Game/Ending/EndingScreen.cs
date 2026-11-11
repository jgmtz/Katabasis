using System.Linq;
using Godot;
using Lete.Core.Data;
using Lete.Core.Systems;
using Lete.Game.Combat;
using Lete.Game.UI;

namespace Lete.Game.Ending;

/// <summary>
/// O final composto (HISTORIA 3; decisões do João de 11/nov/2026, PENDENCIAS 94; a tela é proposta, PENDENCIAS 98):
/// lê o mundo pelas regras de <c>content/rules/final.json</c>, assenta o rosto (regra C) e mostra os cartões do epílogo,
/// um por vez, sobre a água escura, com o rosto do protagonista (devolvido ou coberto pela romã). Enter ou clique
/// avança; no fim, volta à tela de título. Para conferir: <c>-- --final</c> no Main, com as flags do mundo (<c>--flag=</c>).
/// </summary>
public partial class EndingScreen : Control
{
    public const string ScenePath = "res://src/Lete.Game/Ending/Ending.tscn";
    private const string RulesPath = "res://content/rules/final.json";
    private const string TitleScenePath = "res://src/Lete.Game/Title/Title.tscn";

    [Export] public float FadeSeconds { get; set; } = 0.8f;

    /// <summary>Ampliação do rosto (pixel art, sem filtro).</summary>
    [Export] public int FaceScale { get; set; } = 4;

    /// <summary>Largura máxima do texto, em fração da tela.</summary>
    [Export] public float TextWidthFraction { get; set; } = 0.6f;

    private Label _card = null!;
    private Label _hint = null!;
    private TextureRect _face = null!;
    private Lete.Core.Systems.Ending _ending = null!;
    private int _index = -1;
    private bool _busy;

    private static string T(string key) => GameRoot.Instance.Text(key);

    public override void _Ready()
    {
        var root = GameRoot.Instance;
        var rules = EndingRules.FromJson(FileAccess.GetFileAsString(RulesPath));
        var facts = root.Facts;
        var face = root.Mark.SettleFace();
        _ending = Endings.Compose(new EndingFacts(
            rules.DebtFrom(facts),
            rules.GateFrom(facts),
            rules.StayFrom(facts),
            root.Mark.Band,
            face,
            root.State.CompanionId,
            root.State.Oaths.Values.Any(o => o == Lete.Core.State.OathStatus.Broken),
            rules.PastCardsFrom(facts)));
        GD.Print($"Final: dívida {_ending.Debt?.ToString() ?? "nenhuma"}, saída {_ending.Exit}, rosto {_ending.Face}; " +
                 string.Join(", ", _ending.CardKeys));

        SetAnchorsPreset(LayoutPreset.FullRect);
        Theme = UiStyle.BuildTheme(UiStyle.ExplorationScale);
        var p = UiStyle.Palette;
        var background = new ColorRect { Color = p.Outline, MouseFilter = MouseFilterEnum.Ignore };
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(background);

        var center = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(center);
        var column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore, Alignment = BoxContainer.AlignmentMode.Center };
        center.AddChild(column);

        _face = new TextureRect
        {
            TextureFilter = TextureFilterEnum.Nearest,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
        };
        if (FaceTexture(face) is { } texture)
        {
            _face.Texture = texture;
            _face.CustomMinimumSize = texture.GetSize() * FaceScale;
        }

        column.AddChild(_face);

        _card = UiStyle.MakeLabel("");
        _card.HorizontalAlignment = HorizontalAlignment.Center;
        _card.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _card.CustomMinimumSize = new Vector2(Screen.LogicalSize(this).X * TextWidthFraction, 0);
        column.AddChild(_card);

        _hint = UiStyle.MakeLabel(T("final.dica"));
        _hint.HorizontalAlignment = HorizontalAlignment.Center;
        _hint.AddThemeColorOverride("font_color", p.IvoryDim);
        column.AddChild(_hint);

        Next();
    }

    /// <summary>O rosto do fim (regra C): a arte do inventário do corpo escolhido, devolvido ou coberto.</summary>
    private static Texture2D? FaceTexture(Lete.Core.State.FinalFace face)
    {
        var cast = CastData.FromJson(FileAccess.GetFileAsString(BattleStage.CastPath));
        if (GameRoot.Instance.State.ProtagonistId is not { } id || !cast.TryGetValue(id, out var body) || body.InventoryFolder is null)
        {
            return null;
        }

        var path = Lete.Core.Presentation.MarkArt.ImageFor(body.InventoryFolder, 0, face);
        return ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") || @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            GetViewport().SetInputAsHandled();
            Next();
        }
    }

    private async void Next()
    {
        if (_busy)
        {
            return;
        }

        _busy = true;
        if (_index >= 0)
        {
            var hide = CreateTween();
            hide.TweenProperty(_card, "modulate:a", 0f, FadeSeconds / 2f);
            await ToSignal(hide, Tween.SignalName.Finished);
        }

        _index++;
        if (_index >= _ending.CardKeys.Count)
        {
            GetTree().ChangeSceneToFile(TitleScenePath);
            return;
        }

        // O rosto só aparece no cartão dele.
        _face.Visible = _ending.CardKeys[_index].StartsWith("final.rosto.");
        _card.Text = T(_ending.CardKeys[_index]);
        var show = CreateTween();
        show.TweenProperty(_card, "modulate:a", 1f, FadeSeconds).From(0f);
        await ToSignal(show, Tween.SignalName.Finished);
        _busy = false;
    }
}
