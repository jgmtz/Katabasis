using System.Linq;
using Godot;
using Lete.Game.Debug;
using Lete.Game.Scenery;
using Lete.Game.UI;

namespace Lete.Game.Title;

/// <summary>
/// Tela de título, antes da abertura (pedido do João, 10/nov/2026; o desenho é proposta, PENDENCIAS 93): o nome do
/// jogo, o subtítulo e o menu. <b>Continuar</b> (só com save) carrega o save e vai direto à exploração; <b>Novo jogo</b>
/// abre a cutscene de abertura (com save, pede confirmação: o novo jogo grava por cima ao chegar à primeira região);
/// <b>Sair</b> fecha. Com qualquer argumento de conferência, passa direto para a abertura (que decide o resto).
/// O fundo é a margem do rio diante da gruta de Tênaro (<see cref="ShoreSet"/>, o mesmo cenário da abertura), vista
/// pela câmera da exploração e desenhada em pixel art, com a câmera passeando devagar pela beira (pedido do João,
/// 13/nov/2026, PENDENCIAS 116).
/// </summary>
public partial class TitleScreen : Control
{
    private const string OpeningScenePath = "res://src/Lete.Game/Opening/Opening.tscn";
    private const string MainScenePath = "res://src/Lete.Game/Main.tscn";

    /// <summary>Argumento que mostra a tela de título mesmo com outros argumentos (para conferir com --shot).</summary>
    private const string ForceArg = "--titulo";

    /// <summary>Tamanho do nome do jogo, em vezes a fonte de título da exploração.</summary>
    [Export] public int NameScale { get; set; } = 3;

    /// <summary>Espaço entre o subtítulo e o menu, em pixels de tela.</summary>
    [Export] public int MenuGap { get; set; } = 90;

    /// <summary>Largura mínima dos botões, em pixels de tela.</summary>
    [Export] public int ButtonWidth { get; set; } = 420;

    [Export] public float FadeInSeconds { get; set; } = 1.2f;
    [Export] public float FadeOutSeconds { get; set; } = 0.6f;

    [ExportGroup("Fundo (a margem do rio)")]
    /// <summary>A linha da margem e a altura da água do cenário, como na abertura.</summary>
    [Export] public float BankX { get; set; } = -2.4f;
    [Export] public float WaterY { get; set; } = -0.2f;

    /// <summary>A câmera da exploração: giro, inclinação e distância.</summary>
    [Export] public float CameraYawDegrees { get; set; } = 45f;
    [Export] public float CameraPitchDegrees { get; set; } = -35f;
    [Export] public float CameraDistance { get; set; } = 30f;

    /// <summary>Para onde a câmera olha, e quanto e em quanto tempo ela vai e volta pela beira.</summary>
    [Export] public Vector3 CameraTarget { get; set; } = new(-1.5f, 0f, -4.5f);
    [Export] public Vector3 CameraDrift { get; set; } = new(0f, 0f, 3f);
    [Export] public float CameraDriftSeconds { get; set; } = 50f;

    [Export] public float AmbientEnergy { get; set; } = 0.85f;
    [Export] public float MoonEnergy { get; set; } = 1.2f;
    [Export] public float FogDensity { get; set; } = 0.004f;

    /// <summary>Margem da coluna do título e largura da faixa escura atrás dela, em frações da tela.</summary>
    [Export] public float ColumnMargin { get; set; } = 0.07f;
    [Export] public float ShadeWidth { get; set; } = 0.55f;
    [Export] public float ShadeAlpha { get; set; } = 0.88f;

    private VBoxContainer _column = null!;
    private Node3D? _rig;
    private float _time;
    private VBoxContainer _menu = null!;
    private VBoxContainer _confirm = null!;
    private bool _leaving;

    private static string T(string key) => GameRoot.Instance.Text(key);

    public override void _Ready()
    {
        var args = OS.GetCmdlineUserArgs();
        if (args.Length > 0 && !args.Contains(ForceArg))
        {
            CallDeferred(MethodName.Go, OpeningScenePath);
            return;
        }

        SetAnchorsPreset(LayoutPreset.FullRect);
        Theme = UiStyle.BuildTheme(UiStyle.ExplorationScale);
        var p = UiStyle.Palette;

        BuildBackdrop();

        // Uma faixa escura que some para a direita, para o nome e o menu lerem sobre o cenário.
        var shade = new TextureRect
        {
            Texture = new GradientTexture2D
            {
                Gradient = new Gradient
                {
                    Colors = new[] { new Color(p.Outline, ShadeAlpha), new Color(p.Outline, 0f) },
                    Offsets = new[] { 0.35f, 1f },
                },
            },
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        shade.SetAnchorsPreset(LayoutPreset.FullRect);
        shade.AnchorRight = ShadeWidth;
        AddChild(shade);

        var side = new MarginContainer { MouseFilter = MouseFilterEnum.Ignore };
        side.SetAnchorsPreset(LayoutPreset.FullRect);
        side.AnchorLeft = ColumnMargin;
        AddChild(side);
        var center = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore, Alignment = BoxContainer.AlignmentMode.Center };
        center.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        side.AddChild(center);

        var column = _column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        center.AddChild(column);

        var name = UiStyle.MakeLabel(T("titulo.nome"), UiStyle.TitleFontSize * NameScale);
        column.AddChild(name);

        var subtitle = UiStyle.MakeLabel(T("titulo.subtitulo"));
        subtitle.AddThemeColorOverride("font_color", p.IvoryDim);
        column.AddChild(subtitle);

        column.AddChild(new Control { CustomMinimumSize = new Vector2(0, MenuGap) });

        _menu = new VBoxContainer();
        _menu.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        column.AddChild(_menu);
        if (GameRoot.Instance.HasSave)
        {
            AddButton(_menu, "titulo.continuar", Continue);
        }

        AddButton(_menu, "titulo.novo", NewGame);
        AddButton(_menu, "titulo.sair", () => GetTree().Quit());

        _confirm = new VBoxContainer { Visible = false };
        _confirm.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        column.AddChild(_confirm);
        var warning = UiStyle.MakeLabel(T("titulo.novo.aviso"));
        _confirm.AddChild(warning);
        AddButton(_confirm, "titulo.novo.confirmar", () => Go(OpeningScenePath));
        AddButton(_confirm, "titulo.novo.voltar", () => ShowMenu(true));

        var fade = CreateTween();
        fade.TweenProperty(column, "modulate:a", 1f, FadeInSeconds).From(0f);
        _menu.GetChild<Button>(0).CallDeferred(Control.MethodName.GrabFocus);
        SceneShot.Schedule(this);
    }

    public override void _Process(double delta)
    {
        if (_rig is null)
        {
            return;
        }

        // A câmera vai e volta pela beira, devagar.
        _time += (float)delta;
        _rig.Position = CameraTarget + CameraDrift * Mathf.Sin(_time / CameraDriftSeconds * Mathf.Tau);
    }

    /// <summary>O cenário do fundo: a margem do rio, em baixa resolução ampliada sem filtro, pela câmera da exploração.</summary>
    private void BuildBackdrop()
    {
        RenderingServer.GlobalShaderParameterSet("texel_density", FloraField.TexelDensity);
        var container = new SubViewportContainer
        {
            Stretch = true,
            StretchShrink = UiStyle.ExplorationScale,
            TextureFilter = TextureFilterEnum.Nearest,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        container.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(container);
        var viewport = new SubViewport { HandleInputLocally = false };
        container.AddChild(viewport);
        var world = new Node3D { Name = "Margem" };
        viewport.AddChild(world);
        world.AddChild(new WorldEnvironment { Environment = ShoreSet.MakeEnvironment(AmbientEnergy, FogDensity) });
        world.AddChild(new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-35f, 60f, 0f),
            LightColor = new Color(0.6f, 0.7f, 0.85f),
            LightEnergy = MoonEnergy,
            ShadowEnabled = true,
        });
        ShoreSet.Build(world, BankX, WaterY, new Vector2(BankX, 1.2f), 0f, eastWall: false);

        _rig = new Node3D { RotationDegrees = new Vector3(0f, CameraYawDegrees, 0f), Position = CameraTarget };
        var pitch = new Node3D { RotationDegrees = new Vector3(CameraPitchDegrees, 0f, 0f) };
        _rig.AddChild(pitch);
        pitch.AddChild(new Camera3D
        {
            Projection = Camera3D.ProjectionType.Orthogonal,
            // A mesma escala da exploração: a altura da tela em pixels da arte sobre os pixels por metro.
            Size = MenuArt.ArtSize.Y / FloraField.TexelDensity,
            Position = new Vector3(0f, 0f, CameraDistance),
            Far = 100f,
            Current = true,
        });
        world.AddChild(_rig);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Esc na confirmação volta ao menu.
        if (_confirm is { Visible: true } && @event.IsActionPressed("ui_cancel"))
        {
            ShowMenu(true);
            GetViewport().SetInputAsHandled();
        }
    }

    private void AddButton(VBoxContainer box, string key, System.Action pressed)
    {
        var button = new Button { Text = T(key), CustomMinimumSize = new Vector2(ButtonWidth, 0) };
        button.Pressed += pressed;
        box.AddChild(button);
    }

    private void ShowMenu(bool show)
    {
        _menu.Visible = show;
        _confirm.Visible = !show;
        var first = (show ? _menu : _confirm).GetChildren().OfType<Button>().First();
        first.GrabFocus();
    }

    private void Continue()
    {
        var error = GameRoot.Instance.Load();
        if (error != Error.Ok)
        {
            GD.PushError($"Título: não foi possível carregar o save ({error}).");
            return;
        }

        Go(MainScenePath);
    }

    private void NewGame()
    {
        if (GameRoot.Instance.HasSave)
        {
            ShowMenu(false);
            return;
        }

        Go(OpeningScenePath);
    }

    private async void Go(string scenePath)
    {
        if (_leaving)
        {
            return;
        }

        _leaving = true;
        if (_column is not null)
        {
            var fade = CreateTween();
            fade.TweenProperty(_column, "modulate:a", 0f, FadeOutSeconds);
            await ToSignal(fade, Tween.SignalName.Finished);
        }

        GetTree().ChangeSceneToFile(scenePath);
    }
}
