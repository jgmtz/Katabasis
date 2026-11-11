using System.Linq;
using Godot;
using Lete.Game.UI;

namespace Lete.Game.Title;

/// <summary>
/// Tela de título, antes da abertura (pedido do João, 10/nov/2026; o desenho é proposta, PENDENCIAS 93): o nome do
/// jogo, o subtítulo e o menu. <b>Continuar</b> (só com save) carrega o save e vai direto à exploração; <b>Novo jogo</b>
/// abre a cutscene de abertura (com save, pede confirmação: o novo jogo grava por cima ao chegar à primeira região);
/// <b>Sair</b> fecha. Com qualquer argumento de conferência, passa direto para a abertura (que decide o resto).
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

    private VBoxContainer _column = null!;
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

        var background = new ColorRect { Color = p.Outline, MouseFilter = MouseFilterEnum.Ignore };
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(background);

        var center = new CenterContainer { MouseFilter = MouseFilterEnum.Ignore };
        center.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var column = _column = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        center.AddChild(column);

        var name = UiStyle.MakeLabel(T("titulo.nome"), UiStyle.TitleFontSize * NameScale);
        name.HorizontalAlignment = HorizontalAlignment.Center;
        column.AddChild(name);

        var subtitle = UiStyle.MakeLabel(T("titulo.subtitulo"));
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        subtitle.AddThemeColorOverride("font_color", p.IvoryDim);
        column.AddChild(subtitle);

        column.AddChild(new Control { CustomMinimumSize = new Vector2(0, MenuGap) });

        _menu = new VBoxContainer();
        _menu.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        column.AddChild(_menu);
        if (GameRoot.Instance.HasSave)
        {
            AddButton(_menu, "titulo.continuar", Continue);
        }

        AddButton(_menu, "titulo.novo", NewGame);
        AddButton(_menu, "titulo.sair", () => GetTree().Quit());

        _confirm = new VBoxContainer { Visible = false };
        _confirm.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        column.AddChild(_confirm);
        var warning = UiStyle.MakeLabel(T("titulo.novo.aviso"));
        warning.HorizontalAlignment = HorizontalAlignment.Center;
        _confirm.AddChild(warning);
        AddButton(_confirm, "titulo.novo.confirmar", () => Go(OpeningScenePath));
        AddButton(_confirm, "titulo.novo.voltar", () => ShowMenu(true));

        var fade = CreateTween();
        fade.TweenProperty(column, "modulate:a", 1f, FadeInSeconds).From(0f);
        _menu.GetChild<Button>(0).CallDeferred(Control.MethodName.GrabFocus);
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
