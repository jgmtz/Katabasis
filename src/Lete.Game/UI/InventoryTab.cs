using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Lete.Core.Combat;
using Lete.Core.Conditions;
using Lete.Core.Data;
using Lete.Core.Items;
using Lete.Core.Presentation;
using Lete.Core.State;
using Lete.Game.Combat;
using Lete.Game.Opening;

namespace Lete.Game.UI;

/// <summary>
/// Aba do inventário (<c>docs/PROTAGONISTA_SOMBRA.md</c> 6): o corpo escolhido no centro (a imagem troca pela etapa da
/// Marca e, no fim, pelo rosto da regra C; a Marca nunca aparece como número), os cinco espaços de equipamento em
/// volta dele, o selo da origem e, à direita, a bolsa em ícones.
/// É um pano estendido com as coisas em cima (decisão do João, 13/nov/2026, PENDENCIAS 115). Escolher um item da bolsa
/// equipa ou tira (trocar é livre fora da luta, decisão do João, 30/out/2026); escolher um espaço ocupado tira.
/// </summary>
public partial class InventoryTab : Control, IMenuTab
{
    /// <summary>Ampliação do símbolo da origem (pixel art de 64 px), sobre a escala da tela.</summary>
    private const float SymbolScale = 2f / 3f;

    // Medidas em pixels da arte, sobre o fundo de 640×360.
    private static readonly Vector2 BodyTopCenter = new(205, 58);
    private static readonly Vector2 BodyNameAt = new(145, 224);
    private static readonly Vector2 SealAt = new(36, 262);
    private static readonly Vector2 BagTitleAt = new(392, 44);
    private static readonly Vector2 BagAt = new(392, 62);
    private static readonly Vector2 ObolsAt = new(392, 204);
    private static readonly Rect2 DescriptionArea = new(392, 240, 214, 100);
    private const int BagColumns = 6;
    private const int BagGap = 4;
    private const int SlotNameWidth = 70;

    /// <summary>Cada espaço: o centro dele e o ponto do corpo a que o fio o liga.</summary>
    private static readonly Dictionary<EquipSlot, (Vector2 Center, Vector2 Body)> SlotPlaces = new()
    {
        [EquipSlot.Instrument] = (new Vector2(128, 88), new Vector2(190, 132)),
        [EquipSlot.Garment] = (new Vector2(128, 156), new Vector2(196, 156)),
        [EquipSlot.Amulet] = (new Vector2(282, 78), new Vector2(208, 92)),
        [EquipSlot.Votive1] = (new Vector2(282, 134), new Vector2(216, 138)),
        [EquipSlot.Votive2] = (new Vector2(282, 190), new Vector2(214, 176)),
    };

    private readonly Dictionary<EquipSlot, ItemCell> _slots = new();
    private IReadOnlyDictionary<string, CastMember> _cast = null!;
    private IReadOnlyList<Origin> _origins = null!;
    private TextureRect _body = null!;
    private TextureRect _symbol = null!;
    private Label _bodyName = null!;
    private Label _originName = null!;
    private Label _craft = null!;
    private GridContainer _bag = null!;
    private Label _obols = null!;
    private Label _itemName = null!;
    private Label _itemDetail = null!;

    public string TitleKey => "ui.inventario.titulo";

    public string ShortcutAction => "inventory";

    public string BackgroundPath => $"{MenuArt.Folder}/fundo_inventario.png";

    public Rect2I ContentArea => new(Vector2I.Zero, MenuArt.ArtSize);

    public Control View => this;

    public override void _Ready()
    {
        _cast = CastData.FromJson(FileAccess.GetFileAsString(BattleStage.CastPath));
        _origins = OriginData.FromJson(FileAccess.GetFileAsString(OpeningDirector.OriginsPath));
        var root = GameRoot.Instance;

        _body = new TextureRect { TextureFilter = TextureFilterEnum.Nearest, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_body);
        _bodyName = Place(MenuArt.MakeInkLabel("", MenuArt.SmallPixelFontSize, faded: true), BodyNameAt, new Vector2(120, 12));
        _bodyName.HorizontalAlignment = HorizontalAlignment.Center;

        foreach (var (slot, place) in SlotPlaces)
        {
            var cell = new ItemCell { Position = MenuArt.Px(place.Center - Vector2.One * MenuArt.SlotSize / 2f) };
            var captured = slot;
            cell.Pressed += () => PressSlot(captured);
            cell.Described += () => DescribeSlot(captured);
            AddChild(cell);
            _slots[slot] = cell;
            var name = Place(MenuArt.MakeInkLabel(root.Text($"item.espaco.{ConditionParser.SnakeCase(slot)}"), MenuArt.SmallPixelFontSize, faded: true),
                place.Center + new Vector2(-SlotNameWidth / 2f, MenuArt.SlotSize / 2f + 1), new Vector2(SlotNameWidth, 12));
            name.HorizontalAlignment = HorizontalAlignment.Center;
        }

        _symbol = new TextureRect
        {
            TextureFilter = TextureFilterEnum.Nearest,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            Position = MenuArt.Px(SealAt),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(_symbol);
        _originName = Place(MenuArt.MakeInkLabel("", UiStyle.TitlePixelFontSize), SealAt + new Vector2(50, 4), new Vector2(280, 18));
        _craft = Place(MenuArt.MakeInkLabel("", faded: true), SealAt + new Vector2(50, 24), new Vector2(280, 14));

        Place(MenuArt.MakeInkLabel(root.Text("ui.inventario.bolsa"), UiStyle.TitlePixelFontSize), BagTitleAt, new Vector2(200, 16));
        _bag = new GridContainer { Columns = BagColumns, Position = MenuArt.Px(BagAt) };
        _bag.AddThemeConstantOverride("h_separation", MenuArt.Px(BagGap));
        _bag.AddThemeConstantOverride("v_separation", MenuArt.Px(BagGap));
        AddChild(_bag);
        var coin = new TextureRect
        {
            Texture = MenuArt.ItemIcon("obolo", "obolo"),
            TextureFilter = TextureFilterEnum.Nearest,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            Position = MenuArt.Px(ObolsAt),
            Size = MenuArt.Px(Vector2.One * MenuArt.IconSize),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(coin);
        _obols = Place(MenuArt.MakeInkLabel(""), ObolsAt + new Vector2(MenuArt.IconSize + 4, 6), new Vector2(160, 14));

        _itemName = Place(MenuArt.MakeInkLabel("", UiStyle.TitlePixelFontSize), DescriptionArea.Position, new Vector2(DescriptionArea.Size.X, 18));
        _itemName.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _itemDetail = Place(MenuArt.MakeInkLabel(""), DescriptionArea.Position + new Vector2(0, 36), DescriptionArea.Size - new Vector2(0, 36));
        _itemDetail.AutowrapMode = TextServer.AutowrapMode.WordSmart;
    }

    /// <summary>Põe um rótulo na tela, em pixels da arte.</summary>
    private Label Place(Label label, Vector2 at, Vector2 size)
    {
        label.Position = MenuArt.Px(at);
        label.Size = MenuArt.Px(size);
        label.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(label);
        return label;
    }

    public override void _Draw()
    {
        // Os fios que ligam cada espaço ao corpo.
        foreach (var (_, place) in SlotPlaces)
        {
            var toBody = (place.Body - place.Center).Normalized();
            DrawLine(MenuArt.Px(place.Center + toBody * (MenuArt.SlotSize / 2f)), MenuArt.Px(place.Body), MenuArt.InkFaded, MenuArt.Px(1));
        }
    }

    public void Refresh()
    {
        var root = GameRoot.Instance;
        var state = root.State;
        var body = state.ProtagonistId is { } id && _cast.TryGetValue(id, out var member) ? member : null;
        _body.Texture = body?.InventoryFolder is { } folder
            ? GD.Load<Texture2D>(MarkArt.ImageFor(folder, state.MarkStage, state.FinalFace))
            : null;
        if (_body.Texture is { } texture)
        {
            _body.Size = MenuArt.Px(texture.GetSize());
            _body.Position = MenuArt.Px(BodyTopCenter - new Vector2(texture.GetWidth() / 2f, 0));
        }

        _bodyName.Text = body is null ? "" : root.Text(body.NameKey);

        var origin = _origins.FirstOrDefault(o => o.Id == state.OriginId);
        _symbol.Texture = origin is null ? null : GD.Load<Texture2D>(origin.Symbol);
        _symbol.Size = _symbol.Texture is { } symbol ? MenuArt.Px(symbol.GetSize()) * SymbolScale : Vector2.Zero;
        _originName.Text = origin is null ? root.Text("ui.inventario.sem_origem") : root.Text(origin.NameKey);
        _craft.Text = origin is null ? "" : $"{root.Text("origem.oficio")}: {root.Text(origin.CraftKey)}";
        _obols.Text = string.Format(root.Text("ui.inventario.obolos"), state.Inventory.Obols);
        RefreshItems();
        Describe("", "");
    }

    private string NameOf(Item item) => string.Format(GameRoot.Instance.Text("ui.inventario.item_estagio"), GameRoot.Instance.Text(item.NameKey),
        GameRoot.Instance.Text($"item.estagio.{ConditionParser.SnakeCase(GameRoot.Instance.Items.StageOf(item.Id)!.Value)}"));

    private static Texture2D IconOf(Item item) => MenuArt.ItemIcon(item.Id, ConditionParser.SnakeCase(item.Category));

    /// <summary>Os espaços de equipamento e a bolsa: itens que se equipam, relíquias do alforje, oferendas e provisões.</summary>
    private void RefreshItems()
    {
        var root = GameRoot.Instance;
        var items = root.Items;
        foreach (var (slot, cell) in _slots)
        {
            cell.Art = items.EquippedIn(slot) is { } equipped ? IconOf(items.Definitions[equipped]) : null;
            cell.QueueRedraw();
        }

        foreach (var child in _bag.GetChildren())
        {
            _bag.RemoveChild(child);
            child.QueueFree();
        }

        foreach (var item in items.Owned)
        {
            var equipable = item.Category is not (ItemCategory.Relic or ItemCategory.Utensil);
            var cell = new ItemCell { Art = IconOf(item), Marked = items.SlotOf(item.Id) is not null };
            var captured = item;
            cell.Described += () => Describe(NameOf(captured), root.Text(!equipable ? "ui.inventario.alforje"
                : items.SlotOf(captured.Id) is not null ? "ui.inventario.equipado" : "ui.inventario.equipar"));
            if (equipable)
            {
                cell.Pressed += () => ToggleEquip(captured.Id);
            }

            _bag.AddChild(cell);
        }

        foreach (var kind in root.Offerings.Carried)
        {
            var name = root.Text($"oferenda.{kind}");
            var cell = new ItemCell { Art = MenuArt.ItemIcon($"oferenda_{kind}", "oferenda"), Count = root.Offerings.Count(kind) };
            cell.Described += () => Describe(name, root.Text("ui.inventario.oferenda"));
            _bag.AddChild(cell);
        }

        foreach (var provision in root.Provisions)
        {
            var count = root.State.Inventory.Provisions.GetValueOrDefault(provision.Id);
            if (count <= 0)
            {
                continue;
            }

            var captured = provision;
            var cell = new ItemCell { Art = MenuArt.ItemIcon($"provisao_{provision.Id}", "provisao"), Count = count };
            cell.Described += () => Describe(root.Text(captured.NameKey), root.Text(captured.DescriptionKey));
            _bag.AddChild(cell);
        }
    }

    private void ToggleEquip(string itemId)
    {
        var items = GameRoot.Instance.Items;
        if (items.SlotOf(itemId) is not null)
        {
            items.Unequip(itemId);
        }
        else
        {
            items.Equip(itemId);
        }

        RefreshItems();
    }

    private void PressSlot(EquipSlot slot)
    {
        if (GameRoot.Instance.Items.EquippedIn(slot) is { } equipped)
        {
            GameRoot.Instance.Items.Unequip(equipped);
            RefreshItems();
            DescribeSlot(slot);
        }
    }

    private void DescribeSlot(EquipSlot slot)
    {
        var root = GameRoot.Instance;
        var slotName = root.Text($"item.espaco.{ConditionParser.SnakeCase(slot)}");
        if (root.Items.EquippedIn(slot) is { } equipped)
        {
            Describe(NameOf(root.Items.Definitions[equipped]), string.Format(root.Text("ui.inventario.no_espaco"), slotName));
        }
        else
        {
            Describe(slotName, root.Text("ui.inventario.espaco_vazio"));
        }
    }

    private void Describe(string name, string detail)
    {
        _itemName.Text = name;
        _itemDetail.Text = detail;
    }
}
