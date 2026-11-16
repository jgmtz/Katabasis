using System;
using Godot;

namespace Lete.Game.UI;

/// <summary>
/// Um espaço do inventário (de equipamento ou da bolsa): o rebaixo com o ícone do item, a quantidade (se houver mais de
/// um) e a marca de equipado. Em foco ou sob o mouse, o filete acende e a tela mostra o que é.
/// </summary>
public partial class ItemCell : Button
{
    /// <summary>Lado da marca de equipado e margem da quantidade, em pixels da arte.</summary>
    private const int MarkSize = 4;
    private const int CountMargin = 2;

    public Texture2D? Art { get; set; }

    public int Count { get; set; } = 1;

    /// <summary>Marca de "está equipado" (um canto de ouro).</summary>
    public bool Marked { get; set; }

    /// <summary>O espaço entrou em foco (teclado, controle ou mouse): a tela descreve o que há nele.</summary>
    public event Action? Described;

    public ItemCell()
    {
        Flat = true;
        TextureFilter = TextureFilterEnum.Nearest;
        var side = MenuArt.Px(MenuArt.SlotSize);
        CustomMinimumSize = new Vector2(side, side);
        Size = CustomMinimumSize;
        foreach (var state in new[] { "normal", "hover", "pressed", "focus", "disabled" })
        {
            AddThemeStyleboxOverride(state, new StyleBoxEmpty());
        }

        FocusEntered += OnHighlighted;
        MouseEntered += OnHighlighted;
        FocusExited += QueueRedraw;
        MouseExited += QueueRedraw;
    }

    private void OnHighlighted()
    {
        QueueRedraw();
        Described?.Invoke();
    }

    public override void _Draw()
    {
        var lit = HasFocus() || IsHovered();
        DrawTextureRect(MenuArt.Texture(lit ? "espaco_foco" : "espaco"), new Rect2(Vector2.Zero, Size), false);
        if (Art is not null)
        {
            var inset = MenuArt.Px((MenuArt.SlotSize - MenuArt.IconSize) / 2f);
            var side = MenuArt.Px(MenuArt.IconSize);
            DrawTextureRect(Art, new Rect2(inset, inset, side, side), false);
        }

        if (Marked)
        {
            var mark = MenuArt.Px(MarkSize);
            DrawRect(new Rect2(Size.X - mark - MenuArt.Px(2), MenuArt.Px(2), mark, mark), UiStyle.Palette.Gold);
        }

        if (Count > 1)
        {
            var font = UiStyle.PixelFont;
            var fontSize = MenuArt.SmallPixelFontSize * UiStyle.ExplorationScale;
            var text = Count.ToString();
            var width = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize).X;
            var at = new Vector2(Size.X - width - MenuArt.Px(CountMargin), Size.Y - MenuArt.Px(CountMargin) - font.GetDescent(fontSize));
            DrawStringOutline(font, at, text, HorizontalAlignment.Left, -1, fontSize, 2 * UiStyle.ExplorationScale, UiStyle.Palette.Outline);
            DrawString(font, at, text, HorizontalAlignment.Left, -1, fontSize, UiStyle.Palette.Ivory);
        }
    }
}
