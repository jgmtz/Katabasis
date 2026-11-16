using System.Linq;
using Godot;
using Lete.Core.World;

namespace Lete.Game.UI;

/// <summary>
/// Aba do Mapa (decisão do João, 13/nov/2026, PENDENCIAS 115): um pergaminho com a miniatura e o nome de cada região
/// visitada; as vizinhas ainda não visitadas são um vulto na névoa, sem nome, e o resto do mundo não aparece. Uma
/// chama marca a região onde o jogador está. Não há marcador de missão nem de objetivo: o Diário é o único guia.
/// </summary>
public partial class MapTab : Control, IMenuTab
{
    private const string MapPath = "res://content/rules/mapa.json";
    private const string FlamePath = "res://art/ui/tocha.png";

    /// <summary>Tamanho da miniatura e da mancha de névoa, em pixels da arte.</summary>
    private static readonly Vector2 MiniatureSize = new(64, 40);
    private static readonly Vector2 FogSize = new(84, 60);

    /// <summary>Largura reservada ao nome de um lugar e distância dele à miniatura, em pixels da arte.</summary>
    private const int NameWidth = 100;
    private const int NameGap = 2;

    /// <summary>Quantas vezes a chama da tocha é ampliada sobre a miniatura da região atual.</summary>
    private const int FlameScale = 2;

    /// <summary>Traço e vão da passagem ainda não percorrida (linha tracejada), em pixels da arte.</summary>
    private const float DashLength = 3f;

    private WorldMap _map = null!;
    private MapView? _view;
    private Label _empty = null!;

    public string TitleKey => "ui.mapa.titulo";

    public string ShortcutAction => "map";

    public string BackgroundPath => $"{MenuArt.Folder}/fundo_mapa.png";

    public Rect2I ContentArea => new(Vector2I.Zero, MenuArt.ArtSize);

    public Control View => this;

    public override void _Ready()
    {
        _map = WorldMaps.FromJson(FileAccess.GetFileAsString(MapPath));
        TextureFilter = TextureFilterEnum.Nearest;
        _empty = MenuArt.MakeInkLabel("");
        _empty.HorizontalAlignment = HorizontalAlignment.Center;
        _empty.VerticalAlignment = VerticalAlignment.Center;
        _empty.Size = MenuArt.Px((Vector2)MenuArt.ArtSize);
        AddChild(_empty);
    }

    public void Refresh()
    {
        var root = GameRoot.Instance;
        _view = WorldMaps.View(_map, root.State.VisitedRegions.ToList(), root.Regions.Current);
        _empty.Text = _view.Places.Count == 0 ? root.Text("ui.mapa.vazio") : "";
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_view is null)
        {
            return;
        }

        var stroke = MenuArt.Px(1);
        foreach (var link in _view.Links)
        {
            var a = MenuArt.Px(new Vector2(link.A.X, link.A.Y));
            var b = MenuArt.Px(new Vector2(link.B.X, link.B.Y));
            if (link.Known)
            {
                DrawLine(a, b, MenuArt.Ink, stroke);
            }
            else
            {
                DrawDashedLine(a, b, MenuArt.InkFaded, stroke, MenuArt.Px(DashLength));
            }
        }

        var font = UiStyle.PixelFont;
        var fontSize = MenuArt.SmallPixelFontSize * UiStyle.ExplorationScale;
        foreach (var place in _view.Places)
        {
            var center = MenuArt.Px(new Vector2(place.Place.X, place.Place.Y));
            if (place.State == MapPlaceState.Fogged)
            {
                var fog = MenuArt.Px(FogSize);
                DrawTextureRect(MenuArt.Texture("mapa_nevoa"), new Rect2(center - fog / 2f, fog), false);
                continue;
            }

            var size = MenuArt.Px(MiniatureSize);
            var rect = new Rect2(center - size / 2f, size);
            if (ResourceLoader.Exists(place.Place.Image))
            {
                DrawTextureRect(GD.Load<Texture2D>(place.Place.Image), rect, false);
            }
            else
            {
                DrawRect(rect, MenuArt.InkFaded);
            }

            DrawRect(rect.Grow(stroke / 2f), place.Current ? UiStyle.Palette.Gold : MenuArt.Ink, false, stroke);
            var name = GameRoot.Instance.Text(GameRoot.Instance.RegionDefinitions[place.Place.RegionId].NameKey);
            var nameWidth = MenuArt.Px(NameWidth);
            DrawString(font, new Vector2(center.X - nameWidth / 2f, rect.End.Y + MenuArt.Px(NameGap) + font.GetAscent(fontSize)), name,
                HorizontalAlignment.Center, nameWidth, fontSize, MenuArt.Ink);
            if (place.Current)
            {
                var flame = GD.Load<Texture2D>(FlamePath);
                var flameSize = MenuArt.Px(flame.GetSize()) * FlameScale;
                DrawTextureRect(flame, new Rect2(new Vector2(center.X - flameSize.X / 2f, rect.Position.Y - flameSize.Y * 0.7f), flameSize), false);
            }
        }
    }
}
