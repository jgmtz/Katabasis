using Godot;

namespace Lete.Game.UI;

/// <summary>
/// A arte do menu do jogo (<c>art/ui/menu/</c>, gerada por <c>tools/menu_arte.py</c>) e as medidas em pixels da arte:
/// o menu é desenhado numa tela de 640×360 mostrada a 3× (1080p).
/// </summary>
public static class MenuArt
{
    public const string Folder = "res://art/ui/menu";

    /// <summary>A tela do menu em pixels da arte.</summary>
    public static readonly Vector2I ArtSize = new(640, 360);

    /// <summary>Altura da faixa de cima, onde ficam os nomes das abas.</summary>
    public const int HeaderHeight = 30;

    /// <summary>Lado de um espaço (de equipamento ou da bolsa) e de um ícone de item.</summary>
    public const int SlotSize = 30;
    public const int IconSize = 24;

    /// <summary>Tinta sobre o pergaminho e o pano (texto escuro, sem contorno).</summary>
    public static readonly Color Ink = new("#3A2618");
    public static readonly Color InkFaded = new("#6E5238");

    /// <summary>Letra miúda (nomes no mapa, rótulos dos espaços), em pixels da arte.</summary>
    public const int SmallPixelFontSize = 8;

    public static int Px(float art) => Mathf.RoundToInt(art * UiStyle.ExplorationScale);

    public static Vector2 Px(Vector2 art) => art * UiStyle.ExplorationScale;

    public static Texture2D Texture(string name) => GD.Load<Texture2D>($"{Folder}/{name}.png");

    /// <summary>O ícone de um item: o desenho próprio (<c>itens/&lt;id&gt;.png</c>) ou o da categoria (<c>itens/_&lt;categoria&gt;.png</c>).</summary>
    public static Texture2D ItemIcon(string id, string category)
    {
        var own = $"{Folder}/itens/{id}.png";
        return GD.Load<Texture2D>(ResourceLoader.Exists(own) ? own : $"{Folder}/itens/_{category}.png");
    }

    /// <summary>Rótulo em tinta: escuro, sem contorno nem sombra, para ler sobre o pergaminho e o pano claros.</summary>
    public static Label MakeInkLabel(string text, int pixelFontSize = UiStyle.PixelFontSize, bool faded = false)
    {
        var label = new Label { Text = text };
        label.AddThemeFontOverride("font", UiStyle.PixelFont);
        label.AddThemeFontSizeOverride("font_size", pixelFontSize * UiStyle.ExplorationScale);
        label.AddThemeColorOverride("font_color", faded ? InkFaded : Ink);
        label.AddThemeConstantOverride("outline_size", 0);
        label.AddThemeColorOverride("font_shadow_color", Colors.Transparent);
        return label;
    }
}
