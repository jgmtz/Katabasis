using System.Collections.Generic;
using Godot;

namespace Lete.Game.UI;

/// <summary>
/// Aba do Diário: as frases do que o jogador viu e ouviu, em ordem. Sem marcadores nem contagens.
/// É a tabuinha de cera (decisão do João, 13/nov/2026, PENDENCIAS 115): duas placas, as frases riscadas na cera; as
/// frases enchem a placa da esquerda e depois a da direita, e <c>ui_left</c>/<c>ui_right</c> viram a tabuinha. Abre nas
/// frases mais novas.
/// </summary>
public partial class JournalTab : Control, IMenuTab
{
    /// <summary>As duas placas de cera sobre o fundo, em pixels da arte (a área de escrever, já dentro da moldura).</summary>
    private static readonly Rect2I[] Pages = { new(42, 60, 250, 268), new(348, 60, 250, 268) };

    /// <summary>Espaço entre uma frase e a seguinte, em pixels da arte.</summary>
    private const int EntryGap = 5;

    private readonly VBoxContainer[] _columns = new VBoxContainer[2];
    private readonly List<List<string>> _spreads = new();
    private Label _turnHint = null!;
    private int _spread;

    public string TitleKey => "ui.diario.titulo";

    public string ShortcutAction => "journal";

    public string BackgroundPath => $"{MenuArt.Folder}/fundo_diario.png";

    public Rect2I ContentArea => new(Vector2I.Zero, MenuArt.ArtSize);

    public Control View => this;

    public override void _Ready()
    {
        for (var i = 0; i < Pages.Length; i++)
        {
            _columns[i] = new VBoxContainer
            {
                Position = MenuArt.Px((Vector2)Pages[i].Position),
                Size = MenuArt.Px((Vector2)Pages[i].Size),
                ClipContents = true,
            };
            _columns[i].AddThemeConstantOverride("separation", MenuArt.Px(EntryGap));
            AddChild(_columns[i]);
        }

        _turnHint = UiStyle.MakeLabel("");
        _turnHint.AddThemeColorOverride("font_color", UiStyle.Palette.IvoryDim);
        _turnHint.HorizontalAlignment = HorizontalAlignment.Center;
        _turnHint.Position = MenuArt.Px(new Vector2(0, MenuArt.ArtSize.Y - 13));
        _turnHint.Size = MenuArt.Px(new Vector2(MenuArt.ArtSize.X, 12));
        AddChild(_turnHint);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsVisibleInTree() || _spreads.Count < 2)
        {
            return;
        }

        var step = @event.IsActionPressed("ui_left") ? -1 : @event.IsActionPressed("ui_right") ? 1 : 0;
        if (step != 0)
        {
            _spread = Mathf.Clamp(_spread + step, 0, _spreads.Count - 1);
            ShowSpread();
            GetViewport().SetInputAsHandled();
        }
    }

    public void Refresh()
    {
        var root = GameRoot.Instance;
        var texts = new List<string>();
        foreach (var key in root.Journal.Entries)
        {
            texts.Add(root.Text(key));
        }

        if (texts.Count == 0)
        {
            texts.Add(root.Text("ui.diario.vazio"));
        }

        Paginate(texts);
        _spread = _spreads.Count - 1;
        ShowSpread();
    }

    /// <summary>
    /// Reparte as frases pelas placas medindo a altura de cada uma com a letra da tela (quebra de linha na largura da
    /// placa); cada par de placas é uma abertura da tabuinha.
    /// </summary>
    private void Paginate(IReadOnlyList<string> texts)
    {
        _spreads.Clear();
        var font = UiStyle.PixelFont;
        var width = MenuArt.Px(Pages[0].Size.X);
        var limit = MenuArt.Px(Pages[0].Size.Y);
        // Enche das frases mais novas para as mais velhas: a última abertura fica cheia, e a placa que sobra pela
        // metade é a primeira de todas.
        var pages = new List<List<string>> { new() };
        var used = 0f;
        for (var i = texts.Count - 1; i >= 0; i--)
        {
            var height = font.GetMultilineStringSize(texts[i], HorizontalAlignment.Left, width, UiStyle.BodyFontSize).Y + MenuArt.Px(EntryGap);
            if (used + height > limit && pages[^1].Count > 0)
            {
                pages.Add(new List<string>());
                used = 0f;
            }

            pages[^1].Insert(0, texts[i]);
            used += height;
        }

        pages.Reverse();
        if (pages.Count > 1 && pages.Count % Pages.Length != 0)
        {
            pages.Insert(0, new List<string>());
        }

        for (var i = 0; i < pages.Count; i += Pages.Length)
        {
            var spread = new List<string>();
            for (var p = 0; p < Pages.Length; p++)
            {
                // O separador marca onde a placa da direita começa.
                spread.AddRange(i + p < pages.Count ? pages[i + p] : new List<string>());
                spread.Add(PageBreak);
            }

            _spreads.Add(spread);
        }
    }

    private const string PageBreak = "\f";

    private void ShowSpread()
    {
        foreach (var column in _columns)
        {
            foreach (var child in column.GetChildren())
            {
                column.RemoveChild(child);
                child.QueueFree();
            }
        }

        var page = 0;
        foreach (var text in _spreads[_spread])
        {
            if (text == PageBreak)
            {
                page++;
                continue;
            }

            var line = UiStyle.MakeLabel(text);
            line.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            line.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _columns[page].AddChild(line);
        }

        _turnHint.Text = _spreads.Count > 1 ? GameRoot.Instance.Text("ui.diario.virar") : "";
    }
}
