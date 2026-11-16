using System;
using System.Collections.Generic;
using Godot;

namespace Lete.Game.UI;

/// <summary>
/// Menu do jogo com abas (Diário, Mosaico, Inventário e Mapa). Cada aba tem a sua tecla, que é só um atalho: abre o
/// menu direto nela, ou troca para ela se o menu já estiver aberto, ou fecha se ela já estiver na frente. Dentro do
/// menu, <c>menu_tab_prev</c>/<c>menu_tab_next</c> (ou o clique no nome) passam de uma aba para outra, e
/// <c>cancel</c> fecha.
/// Tela cheia (decisão do João, 13/nov/2026, PENDENCIAS 115): cada aba é um objeto do mundo sobre a mesa, desenhado
/// numa tela de 640×360 pixels da arte e mostrado a 3×; em telas mais estreitas que 16:9, a tela inteira encolhe.
/// </summary>
public partial class GameMenu : CanvasLayer
{
    /// <summary>A cor do tampo da mesa, que sobra em volta em telas mais altas ou mais largas que 16:9.</summary>
    private static readonly Color TableColor = new("#171210");

    /// <summary>Distância entre os nomes das abas e margem da faixa de cima, em pixels da arte.</summary>
    private const int HeaderGap = 14;
    private const int HeaderMargin = 20;

    private readonly List<IMenuTab> _tabs = new();
    private readonly List<Button> _headers = new();
    private Control _root = null!;
    private Control _stage = null!;
    private TextureRect _background = null!;
    private Label _hint = null!;

    public bool IsOpen => _root.Visible;

    public int Current { get; private set; }

    public event Action? OpenChanged;

    public override void _Ready()
    {
        Layer = 6;
        _tabs.Add(new JournalTab());
        _tabs.Add(new MosaicTab());
        _tabs.Add(new InventoryTab());
        _tabs.Add(new MapTab());

        _root = new Control { Theme = UiStyle.BuildTheme(UiStyle.ExplorationScale) };
        _root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_root);
        var table = new ColorRect { Color = TableColor };
        table.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _root.AddChild(table);

        var size = MenuArt.Px((Vector2)MenuArt.ArtSize);
        _stage = new Control { Size = size, CustomMinimumSize = size };
        _root.AddChild(_stage);
        _background = new TextureRect
        {
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            Size = size,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        _stage.AddChild(_background);

        foreach (var tab in _tabs)
        {
            _stage.AddChild(tab.View);
            tab.View.Position = MenuArt.Px((Vector2)tab.ContentArea.Position);
            tab.View.Size = MenuArt.Px((Vector2)tab.ContentArea.Size);
        }

        var headers = new HBoxContainer
        {
            Position = MenuArt.Px(new Vector2(HeaderMargin, 0)),
            Size = MenuArt.Px(new Vector2(MenuArt.ArtSize.X - 2 * HeaderMargin, MenuArt.HeaderHeight)),
            Alignment = BoxContainer.AlignmentMode.Begin,
        };
        headers.AddThemeConstantOverride("separation", MenuArt.Px(HeaderGap));
        _stage.AddChild(headers);
        for (var i = 0; i < _tabs.Count; i++)
        {
            var index = i;
            var header = new Button { Flat = true, FocusMode = Control.FocusModeEnum.None, SizeFlagsVertical = Control.SizeFlags.ShrinkCenter };
            header.AddThemeFontSizeOverride("font_size", UiStyle.TitleFontSize);
            header.Pressed += () => Show(index);
            headers.AddChild(header);
            _headers.Add(header);
        }

        _hint = UiStyle.MakeLabel("");
        _hint.HorizontalAlignment = HorizontalAlignment.Right;
        _hint.VerticalAlignment = VerticalAlignment.Center;
        _hint.AddThemeColorOverride("font_color", UiStyle.Palette.IvoryDim);
        _hint.Position = headers.Position;
        _hint.Size = headers.Size;
        _hint.MouseFilter = Control.MouseFilterEnum.Ignore;
        _stage.AddChild(_hint);

        _root.Visible = false;
        _root.Resized += FitStage;
        GameRoot.Instance.StateReplaced += RefreshTexts;
    }

    public override void _ExitTree()
    {
        GameRoot.Instance.StateReplaced -= RefreshTexts;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        for (var i = 0; i < _tabs.Count; i++)
        {
            if (@event.IsActionPressed(_tabs[i].ShortcutAction))
            {
                if (IsOpen && Current == i)
                {
                    SetOpen(false);
                }
                else
                {
                    Open(i);
                }

                GetViewport().SetInputAsHandled();
                return;
            }
        }

        if (!IsOpen)
        {
            return;
        }

        if (@event.IsActionPressed("cancel"))
        {
            SetOpen(false);
        }
        else if (@event.IsActionPressed("menu_tab_prev"))
        {
            Show(Current - 1);
        }
        else if (@event.IsActionPressed("menu_tab_next"))
        {
            Show(Current + 1);
        }
        else
        {
            return;
        }

        GetViewport().SetInputAsHandled();
    }

    /// <summary>Abre o menu na aba do atalho pedido (<c>journal</c>, <c>inventory</c>, <c>map</c>).</summary>
    public void Open(string shortcutAction) => Open(_tabs.FindIndex(t => t.ShortcutAction == shortcutAction));

    public void Open(int index)
    {
        Show(index);
        if (!IsOpen)
        {
            SetOpen(true);
        }
    }

    public void SetOpen(bool open)
    {
        if (open)
        {
            FitStage();
            RefreshTexts();
        }

        _root.Visible = open;
        OpenChanged?.Invoke();
    }

    /// <summary>Redesenha os textos (depois de trocar o gênero no debug ou de carregar um save).</summary>
    public void RefreshTexts()
    {
        _hint.Text = GameRoot.Instance.Text("ui.menu.dica");
        for (var i = 0; i < _tabs.Count; i++)
        {
            _headers[i].Text = GameRoot.Instance.Text(_tabs[i].TitleKey);
        }

        _tabs[Current].Refresh();
    }

    private void Show(int index)
    {
        Current = (index % _tabs.Count + _tabs.Count) % _tabs.Count;
        _background.Texture = GD.Load<Texture2D>(_tabs[Current].BackgroundPath);
        for (var i = 0; i < _tabs.Count; i++)
        {
            _tabs[i].View.Visible = i == Current;
            var color = i == Current ? UiStyle.Palette.Gold : UiStyle.Palette.IvoryDim;
            foreach (var state in new[] { "font_color", "font_hover_color", "font_pressed_color" })
            {
                _headers[i].AddThemeColorOverride(state, color);
            }
        }

        _tabs[Current].Refresh();
    }

    /// <summary>Centraliza a tela do menu e, se a janela for menor que ela em algum lado, encolhe a tela inteira.</summary>
    private void FitStage()
    {
        var available = _root.Size;
        if (available.X <= 0 || available.Y <= 0)
        {
            return;
        }

        var scale = Mathf.Min(1f, Mathf.Min(available.X / _stage.Size.X, available.Y / _stage.Size.Y));
        _stage.Scale = new Vector2(scale, scale);
        _stage.Position = ((available - _stage.Size * scale) / 2f).Floor();
    }
}
