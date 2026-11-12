using System;
using System.Collections.Generic;
using Godot;

namespace Lete.Game.Scenery;

/// <summary>
/// O catálogo de peças do cenário (paisagem HD-2D, PENDÊNCIA 105): cada peça é montada por código a partir do tipo
/// (<see cref="SceneryPiece.Kind"/>), do tamanho, da variante e da semente. A câmera olha do sudeste para o noroeste:
/// a frente das construções (porta, balcão) fica no +Z da peça, e o lado leste (+X) também aparece.
/// </summary>
public static partial class Pieces
{
    private static readonly Dictionary<string, Action<PieceBuilder, SceneryPiece>> Catalog = new()
    {
        ["casa"] = House,
        ["oficina"] = Workshop,
        ["barraca"] = Stall,
        ["muro"] = Wall,
        ["arco"] = Arch,
        ["coluna"] = Column,
        ["altar"] = Altar,
        ["hecateia"] = Hekataion,
        ["cipreste"] = Cypress,
        ["arvore_morta"] = DeadTree,
        ["rocha"] = Boulder,
        ["penhasco"] = Cliff,
        ["entulho"] = Rubble,
        ["estalagmite"] = Stalagmites,
        ["olmo"] = DreamElm,
        ["murta"] = Myrtle,
        ["romazeira"] = Pomegranate,
        ["arma"] = PlantedWeapon,
        ["estrado"] = Dais,
        ["trono"] = Throne,
        ["tocha"] = Torch,
        ["braseiro"] = Brazier,
        ["poco"] = Well,
        ["fosso"] = Pit,
        ["estela"] = Stele,
        ["cerca"] = Fence,
        ["cais"] = Pier,
        ["barca"] = Boat,
        ["varal"] = Clothesline,
        ["carroca"] = Cart,
        ["tumulo"] = Grave,
        ["mesa"] = Table,
        ["forno"] = Oven,
        ["roda"] = PotterWheel,
        ["tear"] = Loom,
        ["tralha"] = Clutter,
        ["anfora"] = (b, p) => Amphora(b, Vector3.Zero, p.Size.Y > 0 ? p.Size.Y : 0.9f, Clay(b)),
        ["caixote"] = (b, p) => Crate(b, Vector3.Zero, p.Size.X > 0 ? p.Size.X : 0.6f),
        ["cesto"] = (b, p) => Basket(b, Vector3.Zero, p.Size.X > 0 ? p.Size.X : 0.5f, true),
    };

    public static bool Knows(string kind) => Catalog.ContainsKey(kind);

    public static IEnumerable<string> Kinds => Catalog.Keys;

    public static void Build(PieceBuilder builder, SceneryPiece piece) => Catalog[piece.Kind](builder, piece);

    // ---- cores (multiplicam a textura) ----

    private static readonly Color White = new(1f, 1f, 1f);
    private static readonly Color Shade = new(0.72f, 0.72f, 0.76f);
    private static readonly Color DoorDark = new(0.035f, 0.03f, 0.04f);
    private static readonly Color WindowLit = new(0.95f, 0.6f, 0.28f);
    private static readonly Color Ember = new(1f, 0.45f, 0.15f);

    private static Color Mul(Color c, float k) => new(c.R * k, c.G * k, c.B * k);

    private static Color Tinted(Color a, Color b) => new(a.R * b.R, a.G * b.G, a.B * b.B);

    /// <summary>Barro cozido das ânforas e tigelas (a textura da telha, mais clara ou mais queimada).</summary>
    private static Color Clay(PieceBuilder b) => Mul(new Color(1f, 0.92f, 0.85f), b.Range(0.75f, 1.15f));

    /// <summary>O reboco de cada casa puxa um pouco para um tom (osso, barro, cinza).</summary>
    private static Color Plaster(PieceBuilder b, Color tint)
    {
        var tones = new[] { new Color(1f, 0.97f, 0.9f), new Color(0.95f, 0.85f, 0.78f), new Color(0.82f, 0.84f, 0.86f), new Color(1.05f, 0.9f, 0.75f) };
        return Tinted(Mul(tones[b.Pick(tones.Length)], b.Range(0.85f, 1.05f)), tint);
    }

    private static float Or(float value, float fallback) => value > 0f ? value : fallback;

    // ---- construções ----

    /// <summary>
    /// Casa de barro sobre base de pedra, com porta na frente e janelas. Variante 0: telhado de duas águas; 1: laje com
    /// mureta e jarros; 2: ruína sem telhado, com entulho.
    /// </summary>
    private static void House(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 4f);
        var h = Or(p.Size.Y, 2.8f);
        var d = Or(p.Size.Z, 3.6f);
        var wall = Plaster(b, p.Tint);
        const float baseH = 0.45f;
        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(w, h, d));

        if (p.Variant == 2)
        {
            RuinedWalls(b, w, h, d, wall);
            return;
        }

        b.Block("muro", Vector3.Zero, new Vector3(w + 0.14f, baseH, d + 0.14f), White);
        b.Box("reboco", new Vector3(0, baseH + (h - baseH) / 2f, 0), new Vector3(w, h - baseH, d), wall, top: p.Variant == 1);

        // A porta (às vezes aberta, só o vão escuro) e o degrau.
        var doorX = b.Range(-w * 0.22f, w * 0.22f);
        var front = d / 2f;
        b.Box(SceneryMaterials.Dark, new Vector3(doorX, 0.95f, front + 0.006f), new Vector3(0.9f, 1.9f, 0.012f), DoorDark);
        if (b.Chance(0.65f))
        {
            using (b.At(new Vector3(doorX - 0.45f, 0, front + 0.02f), b.Chance(0.3f) ? b.Range(15f, 70f) : 0f))
            {
                b.Block("madeira", new Vector3(0.45f, 0.02f, 0.03f), new Vector3(0.88f, 1.86f, 0.06f), Shade);
            }
        }

        b.Box("madeira", new Vector3(doorX, 1.98f, front + 0.06f), new Vector3(1.2f, 0.14f, 0.16f), White);
        b.Block("muro", new Vector3(doorX, 0, front + 0.22f), new Vector3(1.15f, 0.1f, 0.42f), White);

        // Janelas: uma na frente, ao lado da porta, e uma no lado leste. Algumas têm luz.
        var windowX = doorX > 0 ? doorX - 1.25f : doorX + 1.25f;
        if (Mathf.Abs(windowX) < w / 2f - 0.5f)
        {
            Window(b, new Vector3(windowX, 1.45f, front), facingX: false);
        }

        Window(b, new Vector3(w / 2f, 1.45f, b.Range(-d * 0.2f, d * 0.2f)), facingX: true);

        if (p.Variant == 1)
        {
            FlatRoof(b, w, h, d, wall);
        }
        else
        {
            GableRoof(b, w, h, d, wall, "telha");
        }
    }

    private static void Window(PieceBuilder b, Vector3 at, bool facingX)
    {
        var lit = b.Chance(0.3f);
        var color = lit ? Mul(WindowLit, b.Range(0.5f, 0.9f)) : DoorDark;
        var surface = lit ? SceneryMaterials.Glow : SceneryMaterials.Dark;
        var size = facingX ? new Vector3(0.012f, 0.5f, 0.5f) : new Vector3(0.5f, 0.5f, 0.012f);
        var outward = facingX ? new Vector3(0.006f, 0, 0) : new Vector3(0, 0, 0.006f);
        b.Box(surface, at + outward, size, color);
        // Peitoril e uma travessa de madeira.
        var sill = facingX ? new Vector3(0.1f, 0.06f, 0.66f) : new Vector3(0.66f, 0.06f, 0.1f);
        b.Box("madeira", at + outward * 6f + new Vector3(0, -0.29f, 0), sill, Shade);
        var bar = facingX ? new Vector3(0.03f, 0.5f, 0.05f) : new Vector3(0.05f, 0.5f, 0.03f);
        b.Box("madeira", at + outward * 3f, bar, Shade);
    }

    private static void GableRoof(PieceBuilder b, float w, float h, float d, Color wall, string cover)
    {
        const float over = 0.3f;
        var rise = d * 0.34f;
        var hw = w / 2f + over;
        var hd = d / 2f + over;
        var eave = h - 0.05f;
        var a = new Vector3(-hw, eave, hd);
        var c = new Vector3(hw, eave, hd);
        var r1 = new Vector3(hw, h + rise, 0);
        var r0 = new Vector3(-hw, h + rise, 0);
        b.Quad(cover, a, c, r1, r0, White);
        b.Quad(cover, new Vector3(hw, eave, -hd), new Vector3(-hw, eave, -hd), r0, r1, Shade);
        var apex = h + rise * (d / 2f) / hd;
        b.Tri("reboco", new Vector3(w / 2f, h, d / 2f), new Vector3(w / 2f, h, -d / 2f), new Vector3(w / 2f, apex, 0), wall);
        b.Tri("reboco", new Vector3(-w / 2f, h, -d / 2f), new Vector3(-w / 2f, h, d / 2f), new Vector3(-w / 2f, apex, 0), wall);
        // A cumeeira e a testeira do beiral, para o telhado ter espessura.
        b.Box(cover, new Vector3(0, h + rise + 0.03f, 0), new Vector3(2f * hw, 0.1f, 0.22f), Shade);
        b.Box("madeira", new Vector3(0, eave - 0.04f, hd - 0.03f), new Vector3(2f * hw, 0.1f, 0.06f), Shade);
        b.Quad("madeira", new Vector3(hw, eave - 0.09f, hd), new Vector3(hw, eave - 0.09f, -hd),
            new Vector3(hw, eave + 0.01f, -hd), new Vector3(hw, eave + 0.01f, hd), Shade);
    }

    private static void FlatRoof(PieceBuilder b, float w, float h, float d, Color wall)
    {
        b.Box("reboco", new Vector3(0, h + 0.07f, 0), new Vector3(w + 0.24f, 0.14f, d + 0.24f), Mul(wall, 0.9f));
        // Mureta baixa, com um lado aberto.
        var open = b.Pick(4);
        var sides = new (Vector3 At, Vector3 Size)[]
        {
            (new Vector3(0, h + 0.14f, d / 2f), new Vector3(w + 0.2f, 0.3f, 0.2f)),
            (new Vector3(0, h + 0.14f, -d / 2f), new Vector3(w + 0.2f, 0.3f, 0.2f)),
            (new Vector3(w / 2f, h + 0.14f, 0), new Vector3(0.2f, 0.3f, d + 0.2f)),
            (new Vector3(-w / 2f, h + 0.14f, 0), new Vector3(0.2f, 0.3f, d + 0.2f)),
        };
        for (var i = 0; i < sides.Length; i++)
        {
            if (i != open)
            {
                b.Block("reboco", sides[i].At, sides[i].Size, wall);
            }
        }

        // O que se guarda na laje: jarros, um cesto.
        for (var i = 0; i < 1 + b.Pick(3); i++)
        {
            var at = new Vector3(b.Range(-w * 0.3f, w * 0.3f), h + 0.14f, b.Range(-d * 0.3f, d * 0.3f));
            if (b.Chance(0.6f))
            {
                Amphora(b, at, b.Range(0.5f, 0.8f), Clay(b));
            }
            else
            {
                Basket(b, at, 0.45f, b.Chance(0.5f));
            }
        }

        // Uma escada de mão encostada no lado leste.
        if (b.Chance(0.6f))
        {
            using (b.At(new Vector3(w / 2f + 0.45f, 0, b.Range(-d * 0.2f, d * 0.2f)), 0f, 0f, 14f))
            {
                Ladder(b, h + 0.5f);
            }
        }
    }

    private static void Ladder(PieceBuilder b, float height)
    {
        b.Block("madeira", new Vector3(0, 0, -0.22f), new Vector3(0.06f, height, 0.06f), White);
        b.Block("madeira", new Vector3(0, 0, 0.22f), new Vector3(0.06f, height, 0.06f), White);
        for (var y = 0.3f; y < height - 0.1f; y += 0.34f)
        {
            b.Box("madeira", new Vector3(0, y, 0), new Vector3(0.05f, 0.05f, 0.44f), Shade);
        }
    }

    private static void RuinedWalls(PieceBuilder b, float w, float h, float d, Color wall)
    {
        const float t = 0.34f;
        void Run(Vector3 from, Vector3 to)
        {
            var length = from.DistanceTo(to);
            var steps = Mathf.Max(2, Mathf.RoundToInt(length / 0.8f));
            var gap = b.Pick(steps);
            for (var i = 0; i < steps; i++)
            {
                if (i == gap && b.Chance(0.7f))
                {
                    continue;
                }

                var at = from.Lerp(to, (i + 0.5f) / steps);
                var height = h * b.Range(0.25f, 1f);
                var size = Mathf.Abs(to.X - from.X) > 0.01f
                    ? new Vector3(length / steps + 0.02f, height, t)
                    : new Vector3(t, height, length / steps + 0.02f);
                b.Block("muro", at, new Vector3(size.X + 0.06f, Mathf.Min(0.45f, height), size.Z + 0.06f), White);
                b.Block("reboco", at, size, wall);
            }
        }

        var hw = w / 2f - t / 2f;
        var hd = d / 2f - t / 2f;
        Run(new Vector3(-hw, 0, -hd), new Vector3(hw, 0, -hd));
        Run(new Vector3(-hw, 0, hd), new Vector3(hw, 0, hd));
        Run(new Vector3(-hw, 0, -hd), new Vector3(-hw, 0, hd));
        Run(new Vector3(hw, 0, -hd), new Vector3(hw, 0, hd));
        for (var i = 0; i < 4 + b.Pick(4); i++)
        {
            var size = b.Range(0.25f, 0.7f);
            b.Rock("muro", new Vector3(b.Range(-w * 0.55f, w * 0.55f), 0, b.Range(-d * 0.55f, d * 0.55f)),
                new Vector3(size, size * 0.6f, size * b.Range(0.7f, 1.2f)), Shade, 3, 6);
        }
    }

    /// <summary>
    /// Oficina aberta na frente: três paredes, dois esteios e uma água de telhado. A variante escolhe o ofício: 0 só a
    /// bancada, 1 a roda do oleiro, 2 o tear, 3 a bancada de ferramentas com jarros, 4 a padaria (o forno aceso).
    /// </summary>
    private static void Workshop(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 4.2f);
        var h = Or(p.Size.Y, 2.6f);
        var d = Or(p.Size.Z, 3f);
        var wall = Plaster(b, p.Tint);
        const float t = 0.3f;
        var back = -d / 2f + t / 2f;
        b.Block("muro", new Vector3(0, 0, back), new Vector3(w + 0.1f, 0.45f, t + 0.1f), White);
        b.Block("reboco", new Vector3(0, 0, back), new Vector3(w, h, t), wall);
        foreach (var side in new[] { -1f, 1f })
        {
            var x = side * (w / 2f - t / 2f);
            b.Block("muro", new Vector3(x, 0, 0), new Vector3(t + 0.1f, 0.45f, d + 0.1f), White);
            b.Block("reboco", new Vector3(x, 0, -d * 0.15f), new Vector3(t, h * 0.92f, d * 0.7f), wall);
            b.Block("madeira", new Vector3(x, 0, d / 2f - 0.1f), new Vector3(0.16f, h * 0.8f, 0.16f), White);
            b.Collide(new Vector3(x, h / 2f, -d * 0.15f), new Vector3(t, h, d * 0.7f));
        }

        b.Collide(new Vector3(0, h / 2f, back), new Vector3(w, h, t));

        // Uma água de telhado, da parede do fundo (alta) até a frente (baixa), com beiral.
        const float over = 0.35f;
        var hw = w / 2f + over;
        var high = new Vector3(0, h + 0.12f, -d / 2f - 0.1f);
        var low = new Vector3(0, h * 0.8f, d / 2f + over);
        b.Quad("telha", new Vector3(-hw, low.Y, low.Z), new Vector3(hw, low.Y, low.Z), new Vector3(hw, high.Y, high.Z),
            new Vector3(-hw, high.Y, high.Z), White);
        b.Box("madeira", new Vector3(0, low.Y - 0.06f, low.Z - 0.04f), new Vector3(2f * hw, 0.1f, 0.08f), Shade);
        b.Box("madeira", new Vector3(0, h * 0.8f - 0.02f, d / 2f - 0.1f), new Vector3(w, 0.12f, 0.12f), White);

        switch (p.Variant)
        {
            case 1:
                using (b.At(new Vector3(-w * 0.18f, 0, 0.1f)))
                {
                    PotterWheel(b, p);
                }

                Shelf(b, new Vector3(w * 0.2f, 0, back + 0.35f), 1.4f);
                break;
            case 2:
                using (b.At(new Vector3(0, 0, -0.1f)))
                {
                    Loom(b, p);
                }

                break;
            case 3:
                Bench(b, new Vector3(0, 0, 0.2f), w * 0.6f, tools: true);
                Shelf(b, new Vector3(-w * 0.22f, 0, back + 0.35f), 1.2f);
                break;
            case 4:
                using (b.At(new Vector3(-w * 0.12f, 0, -0.15f)))
                {
                    OvenAt(b, 1.7f, p.Lit);
                }

                Basket(b, new Vector3(w * 0.3f, 0, 0.5f), 0.55f, full: true);
                Basket(b, new Vector3(w * 0.32f, 0, -0.2f), 0.45f, full: true);
                Sack(b, new Vector3(-w * 0.36f, 0, 0.8f), 0.55f);
                break;
            default:
                Bench(b, new Vector3(0, 0, 0.2f), w * 0.6f, tools: false);
                break;
        }
    }

    private static void Bench(PieceBuilder b, Vector3 at, float width, bool tools)
    {
        b.Box("madeira", at + new Vector3(0, 0.82f, 0), new Vector3(width, 0.08f, 0.7f), White);
        foreach (var sx in new[] { -1f, 1f })
        {
            b.Block("madeira", at + new Vector3(sx * (width / 2f - 0.1f), 0, 0), new Vector3(0.1f, 0.8f, 0.6f), Shade);
        }

        b.Collide(at + new Vector3(0, 0.45f, 0), new Vector3(width, 0.9f, 0.7f));
        var count = tools ? 4 : 2;
        for (var i = 0; i < count; i++)
        {
            var spot = at + new Vector3(b.Range(-width * 0.4f, width * 0.4f), 0.86f, b.Range(-0.2f, 0.2f));
            if (tools && b.Chance(0.5f))
            {
                b.Box("madeira", spot + new Vector3(0, 0.03f, 0), new Vector3(0.36f, 0.05f, 0.06f), Shade);
            }
            else
            {
                Bowl(b, spot, b.Range(0.12f, 0.2f), Clay(b));
            }
        }
    }

    private static void Shelf(PieceBuilder b, Vector3 at, float width)
    {
        for (var level = 0; level < 2; level++)
        {
            var y = 0.7f + level * 0.6f;
            b.Box("madeira", at + new Vector3(0, y, 0), new Vector3(width, 0.05f, 0.34f), White);
            for (var i = 0; i < 3; i++)
            {
                var spot = at + new Vector3(-width * 0.35f + i * width * 0.35f, y + 0.03f, 0);
                if (b.Chance(0.7f))
                {
                    Amphora(b, spot, b.Range(0.3f, 0.45f), Clay(b));
                }
            }
        }

        b.Block("madeira", at + new Vector3(-width / 2f, 0, 0), new Vector3(0.06f, 1.4f, 0.3f), Shade);
        b.Block("madeira", at + new Vector3(width / 2f, 0, 0), new Vector3(0.06f, 1.4f, 0.3f), Shade);
    }

    /// <summary>
    /// Barraca de mercado: quatro esteios, toldo de pano inclinado com a barra pendurada, balcão e mercadoria. A tinta
    /// da peça é a cor do toldo. Variante: 0 ânforas e jarros, 1 cestos de pão e frutos, 2 rolos de pano, 3 tigelas e
    /// miudezas.
    /// </summary>
    private static void Stall(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 2.8f);
        var h = Or(p.Size.Y, 2.4f);
        var d = Or(p.Size.Z, 1.9f);
        var cloth = p.Tint;
        var hw = w / 2f;
        var hd = d / 2f;
        var frontH = h * 0.78f;
        foreach (var (x, z, height) in new[] { (-hw, -hd, h), (hw, -hd, h), (-hw, hd, frontH), (hw, hd, frontH) })
        {
            b.Block("madeira", new Vector3(x, 0, z), new Vector3(0.1f, height, 0.1f), White);
        }

        // O toldo: do fundo (alto) para a frente (baixo), com uma barriga no meio, e a barra recortada na frente.
        const float over = 0.25f;
        var sag = 0.12f;
        var midY = (h + frontH) / 2f - sag;
        Vector3 Back(float x) => new(x, h + 0.02f, -hd - over);
        Vector3 Mid(float x) => new(x, midY, 0);
        Vector3 Front(float x) => new(x, frontH + 0.02f, hd + over);
        b.Quad("pano", Mid(-hw - over), Mid(hw + over), Back(hw + over), Back(-hw - over), cloth);
        b.Quad("pano", Front(-hw - over), Front(hw + over), Mid(hw + over), Mid(-hw - over), cloth);
        var flaps = Mathf.RoundToInt((w + 2 * over) / 0.4f);
        for (var i = 0; i < flaps; i++)
        {
            var x0 = -hw - over + i * (w + 2 * over) / flaps;
            var x1 = x0 + (w + 2 * over) / flaps;
            var drop = i % 2 == 0 ? 0.3f : 0.2f;
            b.Quad("pano", new Vector3(x0, frontH + 0.02f - drop, hd + over), new Vector3(x1, frontH + 0.02f - drop, hd + over),
                Front(x1), Front(x0), Mul(cloth, i % 2 == 0 ? 0.85f : 1f));
        }

        // Um pano de fundo fecha as costas da barraca.
        b.Quad("pano", new Vector3(-hw, 0.3f, -hd), new Vector3(hw, 0.3f, -hd), new Vector3(hw, h, -hd), new Vector3(-hw, h, -hd),
            Mul(cloth, 0.7f));

        // O balcão, na frente, e a mercadoria em cima.
        var counter = new Vector3(0, 0, hd - 0.4f);
        b.Box("madeira", counter + new Vector3(0, 0.84f, 0), new Vector3(w - 0.1f, 0.08f, 0.75f), White);
        b.Block("madeira", counter, new Vector3(w - 0.3f, 0.8f, 0.55f), Shade);
        b.Collide(new Vector3(0, 0.6f, hd - 0.4f), new Vector3(w, 1.2f, 0.8f));
        b.Collide(new Vector3(0, 1f, -hd), new Vector3(w, 2f, 0.2f));
        for (var i = 0; i < 4; i++)
        {
            var spot = counter + new Vector3(-w * 0.36f + i * w * 0.24f + b.Range(-0.05f, 0.05f), 0.88f, b.Range(-0.15f, 0.15f));
            switch (p.Variant)
            {
                case 1:
                    Basket(b, spot, b.Range(0.32f, 0.42f), full: true);
                    break;
                case 2:
                    using (b.At(spot, b.Range(-20f, 20f)))
                    {
                        var roll = new[] { new Color(0.75f, 0.35f, 0.25f), new Color(0.35f, 0.55f, 0.58f), new Color(0.85f, 0.8f, 0.68f), new Color(0.78f, 0.63f, 0.3f) }[b.Pick(4)];
                        b.Box("pano", new Vector3(0, 0.09f, 0), new Vector3(0.22f, 0.18f, 0.6f), roll);
                    }

                    break;
                case 3:
                    Bowl(b, spot, b.Range(0.12f, 0.22f), Clay(b));
                    break;
                default:
                    Amphora(b, spot, b.Range(0.35f, 0.6f), Clay(b));
                    break;
            }
        }

        // Ao lado, no chão, o que não coube no balcão.
        Amphora(b, new Vector3(hw + 0.35f, 0, hd - 0.2f), b.Range(0.7f, 1f), Clay(b));
        if (b.Chance(0.6f))
        {
            Crate(b, new Vector3(-hw - 0.4f, 0, hd - 0.1f), b.Range(0.45f, 0.65f));
        }
    }

    /// <summary>
    /// Muro de pedra ao longo do X da peça: tramos de alturas desiguais, capeamento e contrafortes. Variante 1: em
    /// ruína (as pontas e alguns tramos desabaram).
    /// </summary>
    private static void Wall(PieceBuilder b, SceneryPiece p)
    {
        var length = Or(p.Size.X, 6f);
        var h = Or(p.Size.Y, 2.6f);
        var t = Or(p.Size.Z, 0.6f);
        var steps = Mathf.Max(1, Mathf.RoundToInt(length / 1.15f));
        var step = length / steps;
        for (var i = 0; i < steps; i++)
        {
            var x = -length / 2f + (i + 0.5f) * step;
            var height = h + b.Range(-0.1f, 0.1f);
            if (p.Variant == 1)
            {
                var edge = Mathf.Min(i, steps - 1 - i);
                height *= edge == 0 ? b.Range(0.25f, 0.5f) : b.Chance(0.25f) ? b.Range(0.4f, 0.75f) : 1f;
            }

            b.Block("muro", new Vector3(x, 0, 0), new Vector3(step + 0.01f, height, t), White);
            if (p.Variant != 1 || height > h * 0.9f)
            {
                b.Block("muro", new Vector3(x, height, 0), new Vector3(step * 0.92f, 0.14f, t + 0.12f), Shade);
            }
        }

        for (var x = -length / 2f + 2.5f; x < length / 2f - 2f; x += 5f)
        {
            b.Block("muro", new Vector3(x, 0, t / 2f + 0.15f), new Vector3(0.6f, h * 0.7f, 0.4f), Shade);
        }

        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(length, h, t));
    }

    /// <summary>
    /// Portal: dois pilares e a verga com cornija; a passagem (largura X) fica livre. Variante 1: os pilares em mármore.
    /// </summary>
    private static void Arch(PieceBuilder b, SceneryPiece p)
    {
        var opening = Or(p.Size.X, 2.8f);
        var h = Or(p.Size.Y, 3.6f);
        var t = Or(p.Size.Z, 0.9f);
        var stone = p.Variant == 1 ? "marmore" : "muro";
        var pillar = Mathf.Max(0.8f, opening * 0.3f);
        var lintel = Mathf.Max(0.6f, h * 0.18f);
        foreach (var side in new[] { -1f, 1f })
        {
            var x = side * (opening / 2f + pillar / 2f);
            b.Block(stone, new Vector3(x, 0, 0), new Vector3(pillar + 0.16f, 0.4f, t + 0.16f), Shade);
            b.Block(stone, new Vector3(x, 0, 0), new Vector3(pillar, h - lintel, t), White);
            b.Collide(new Vector3(x, h / 2f, 0), new Vector3(pillar, h, t));
        }

        var span = opening + 2f * pillar;
        b.Block(stone, new Vector3(0, h - lintel, 0), new Vector3(span + 0.1f, lintel, t + 0.06f), White);
        b.Block(stone, new Vector3(0, h, 0), new Vector3(span + 0.4f, 0.16f, t + 0.3f), Shade);
        b.Block(stone, new Vector3(0, h - lintel - 0.12f, 0), new Vector3(opening * 0.34f, 0.12f, t + 0.1f), Shade);
    }

    /// <summary>Coluna dórica de mármore gasto. Variante 1: quebrada, com tambores caídos ao pé.</summary>
    private static void Column(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 3.4f);
        var r = Or(p.Size.X, 0.7f) / 2f;
        var broken = p.Variant == 1;
        var shaft = broken ? h * b.Range(0.3f, 0.65f) : h - 0.5f;
        b.Block("marmore", Vector3.Zero, new Vector3(r * 2.6f, 0.14f, r * 2.6f), Shade);
        b.Lathe("marmore", new Vector3(0, 0.14f, 0), new[] { (r * 1.15f, 0f), (r, 0.12f), (r * 0.86f, shaft) }, 10, White, cap: broken);
        if (broken)
        {
            for (var i = 0; i < 2; i++)
            {
                using (b.At(new Vector3(b.Range(0.7f, 1.5f) * (i == 0 ? 1 : -0.8f), r * 0.9f, b.Range(-0.9f, 0.9f)), b.Range(0, 180f), 90f))
                {
                    b.Cyl("marmore", new Vector3(0, -0.3f, 0), r * 0.9f, r * 0.9f, b.Range(0.5f, 0.9f), 10, Shade);
                }
            }
        }
        else
        {
            b.Lathe("marmore", new Vector3(0, 0.14f + shaft, 0), new[] { (r * 0.86f, 0f), (r * 1.25f, 0.16f) }, 10, White);
            b.Block("marmore", new Vector3(0, 0.3f + shaft, 0), new Vector3(r * 2.7f, 0.2f, r * 2.7f), White);
        }

        b.Collide(new Vector3(0, shaft / 2f, 0), new Vector3(r * 2f, shaft, r * 2f));
    }

    /// <summary>O Altar das Oferendas: três degraus, a mesa de mármore com cornija, tigelas e a chama.</summary>
    private static void Altar(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 3f);
        var d = Or(p.Size.Z, 2f);
        for (var i = 0; i < 3; i++)
        {
            var grow = (2 - i) * 0.5f;
            b.Block("marmore", new Vector3(0, i * 0.16f, 0), new Vector3(w + grow * 2f, 0.16f, d + grow * 2f), i % 2 == 0 ? Shade : White);
        }

        b.Block("marmore", new Vector3(0, 0.48f, 0), new Vector3(w * 0.72f, 0.7f, d * 0.6f), White);
        b.Block("marmore", new Vector3(0, 1.18f, 0), new Vector3(w * 0.8f, 0.14f, d * 0.7f), Shade);
        b.Collide(new Vector3(0, 0.6f, 0), new Vector3(w, 1.3f, d));
        Bowl(b, new Vector3(-w * 0.25f, 1.32f, 0), 0.24f, Clay(b));
        Bowl(b, new Vector3(w * 0.25f, 1.32f, 0.05f), 0.2f, Clay(b));
        Amphora(b, new Vector3(-w * 0.5f - 0.2f, 0.16f, d * 0.5f + 0.3f), 0.8f, Clay(b));
        Flame(b, new Vector3(0, 1.36f, 0), 0.9f, p.Lit ? 1.2f : 0f, 7f);
    }

    /// <summary>
    /// O pilar da Trívia de Hécate: três faces, três braços com tochas, sobre uma base de três degraus (as três
    /// estradas). A luz da Trívia é da cena; aqui só as chamas.
    /// </summary>
    private static void Hekataion(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 3f);
        // Três degraus triangulares (as três estradas) e o pilar de mármore, mais fino em cima, com a cabeça tripla.
        b.Cyl("marmore", Vector3.Zero, 1.5f, 1.5f, 0.14f, 3, Shade);
        b.Cyl("marmore", new Vector3(0, 0.14f, 0), 1.1f, 1.1f, 0.14f, 3, White);
        b.Cyl("marmore", new Vector3(0, 0.28f, 0), 0.75f, 0.75f, 0.14f, 3, Shade);
        b.Cyl("marmore", new Vector3(0, 0.42f, 0), 0.46f, 0.36f, h, 3, White);
        b.Cyl("marmore", new Vector3(0, 0.42f + h, 0), 0.52f, 0.52f, 0.1f, 3, Shade);
        for (var face = 0; face < 3; face++)
        {
            // Um rosto por face: a cabeça, com o véu, olhando para uma das estradas.
            using (b.At(new Vector3(0, 0.52f + h, 0), 120f * face + 60f))
            {
                b.Block("marmore", new Vector3(0, 0, 0.14f), new Vector3(0.3f, 0.4f, 0.26f), White);
                b.Block("marmore", new Vector3(0, 0.36f, 0.1f), new Vector3(0.36f, 0.12f, 0.34f), Shade);
            }
        }

        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(0.9f, h, 0.9f));
        for (var i = 0; i < 3; i++)
        {
            var angle = 360f * i / 3f + 60f;
            using (b.At(new Vector3(0, 0.42f + h * 0.7f, 0), angle))
            {
                b.Box("madeira", new Vector3(0, 0, 0.6f), new Vector3(0.07f, 0.07f, 0.8f), Shade);
                b.Cyl("madeira", new Vector3(0, 0, 1f), 0.05f, 0.12f, 0.3f, 6, White);
                Flame(b, new Vector3(0, 0.3f, 1f), 0.8f, 0f, 0f);
            }
        }
    }
}
