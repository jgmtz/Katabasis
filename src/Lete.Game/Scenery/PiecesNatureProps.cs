using Godot;

namespace Lete.Game.Scenery;

/// <summary>A natureza (ciprestes, rochas, paredões) e os objetos pequenos do catálogo de peças.</summary>
public static partial class Pieces
{
    // ---- natureza ----

    /// <summary>Cipreste: tronco curto e copa estreita em cones empilhados, um pouco torta.</summary>
    private static void Cypress(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 5.5f) * b.Range(0.85f, 1.15f);
        var r = Or(p.Size.X, 1.3f) / 2f;
        // Variante 1: o cipreste branco de Mnemósine (casca e copa pálidas).
        var leaves = p.Variant == 1 ? "pano" : "folhagem";
        b.Cyl(p.Variant == 1 ? "marmore" : "casca", Vector3.Zero, 0.16f, 0.1f, h * 0.2f, 6, White);
        b.Collide(new Vector3(0, 1f, 0), new Vector3(0.5f, 2f, 0.5f));
        const int tiers = 4;
        var lean = new Vector3(b.Range(-0.12f, 0.12f), 0, b.Range(-0.12f, 0.12f));
        for (var i = 0; i < tiers; i++)
        {
            var t = i / (float)tiers;
            var y = h * (0.12f + t * 0.62f);
            var radius = r * (1f - t * 0.55f);
            var height = h * (1f - 0.12f - t * 0.62f) * (i == tiers - 1 ? 1f : 0.62f);
            b.Cyl(leaves, new Vector3(0, y, 0) + lean * (t * h * 0.5f), radius, 0f, height, 7, Mul(White, 1f - t * 0.12f));
        }
    }

    /// <summary>Árvore morta: tronco torto e galhos nus.</summary>
    private static void DeadTree(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 3.4f);
        b.Collide(new Vector3(0, 1f, 0), new Vector3(0.5f, 2f, 0.5f));
        using (b.At(Vector3.Zero, b.Range(0, 360f), b.Range(-8f, 8f)))
        {
            b.Cyl("casca", Vector3.Zero, 0.24f, 0.13f, h * 0.6f, 6, White);
            for (var i = 0; i < 4; i++)
            {
                using (b.At(new Vector3(0, h * b.Range(0.35f, 0.58f), 0), i * 90f + b.Range(-25f, 25f), b.Range(28f, 62f)))
                {
                    var length = h * b.Range(0.3f, 0.55f);
                    b.Cyl("casca", Vector3.Zero, 0.1f, 0.04f, length, 5, Shade);
                    using (b.At(new Vector3(0, length * 0.6f, 0), b.Range(0, 360f), b.Range(25f, 50f)))
                    {
                        b.Cyl("casca", Vector3.Zero, 0.05f, 0.02f, length * 0.5f, 4, Shade);
                    }
                }
            }
        }
    }

    /// <summary>Rocha solta. O tamanho é a caixa que ela ocupa.</summary>
    private static void Boulder(PieceBuilder b, SceneryPiece p)
    {
        var size = new Vector3(Or(p.Size.X, 1.4f), Or(p.Size.Y, 1f), Or(p.Size.Z, 1.2f));
        b.Rock("pedra@mundo", Vector3.Zero, size, p.Tint, 4, 8);
        if (b.Chance(0.6f))
        {
            var small = size * b.Range(0.3f, 0.5f);
            b.Rock("pedra@mundo", new Vector3(size.X * 0.55f, 0, size.Z * b.Range(-0.3f, 0.4f)), small, Mul(p.Tint, 0.9f), 3, 6);
        }

        b.Collide(new Vector3(0, size.Y / 2f, 0), size * 0.8f);
    }

    /// <summary>
    /// Paredão de rocha ao longo do X da peça, para as bordas da região: lascas altas e desiguais, umas à frente das
    /// outras, que somem na escuridão em cima.
    /// </summary>
    private static void Cliff(PieceBuilder b, SceneryPiece p)
    {
        var length = Or(p.Size.X, 12f);
        var h = Or(p.Size.Y, 9f);
        var depth = Or(p.Size.Z, 3f);
        // Um fundo contínuo atrás das lascas, para não sobrar vão entre elas.
        b.Block("pedra@mundo", new Vector3(0, 0, -depth * 0.3f), new Vector3(length, h * 0.6f, depth * 0.5f), Mul(p.Tint, 0.8f));
        var x = -length / 2f;
        while (x < length / 2f)
        {
            var w = b.Range(1.8f, 3.4f);
            var height = h * b.Range(0.55f, 1f);
            var z = b.Range(-depth * 0.25f, depth * 0.25f);
            var d = depth * b.Range(0.7f, 1.1f);
            // Enterrada um pouco no chão, para a base nunca deixar fresta.
            var foot = new Vector3(x + w / 2f, -0.4f, z);
            using (b.At(foot, b.Range(-12f, 12f)))
            {
                // Uma lasca: mais larga embaixo, com o topo quebrado em dois níveis.
                b.Block("pedra@mundo", Vector3.Zero, new Vector3(w * 1.15f, height * 0.45f, d * 1.1f), p.Tint);
                b.Block("pedra@mundo", new Vector3(0, height * 0.45f, -d * 0.05f), new Vector3(w, height * 0.35f, d * 0.9f), Mul(p.Tint, 0.92f));
                b.Block("pedra@mundo", new Vector3(b.Range(-0.2f, 0.2f) * w, height * 0.8f, -d * 0.12f),
                    new Vector3(w * b.Range(0.5f, 0.8f), height * 0.2f, d * 0.7f), Mul(p.Tint, 0.85f));
            }

            if (b.Chance(0.5f))
            {
                var s = b.Range(0.6f, 1.4f);
                b.Rock("pedra@mundo", new Vector3(x + b.Range(0, w), 0, depth * 0.6f + b.Range(0f, 0.8f)),
                    new Vector3(s, s * 0.7f, s), p.Tint, 3, 6);
            }

            x += w * 0.7f;
        }

        b.Collide(new Vector3(0, 2f, 0), new Vector3(length, 4f, depth));
    }

    /// <summary>Entulho: pedras e blocos caídos espalhados na área X por Z.</summary>
    private static void Rubble(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 2f);
        var d = Or(p.Size.Z, 2f);
        var count = 4 + b.Pick(5);
        for (var i = 0; i < count; i++)
        {
            var s = b.Range(0.2f, 0.65f);
            var at = new Vector3(b.Range(-w / 2f, w / 2f), 0, b.Range(-d / 2f, d / 2f));
            if (b.Chance(0.4f))
            {
                using (b.At(at, b.Range(0, 90f), 0f, b.Range(-12f, 12f)))
                {
                    b.Block("muro", Vector3.Zero, new Vector3(s * 1.4f, s * 0.7f, s), Shade);
                }
            }
            else
            {
                b.Rock("pedra@mundo", at, new Vector3(s, s * 0.7f, s * b.Range(0.8f, 1.3f)), Shade, 3, 6);
            }
        }
    }

    /// <summary>Estalagmites: um grupo de cones de rocha, o maior com a altura Y do tamanho.</summary>
    private static void Stalagmites(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 1.6f);
        var spread = Or(p.Size.X, 1.2f);
        var count = 2 + b.Pick(3);
        for (var i = 0; i < count; i++)
        {
            var height = i == 0 ? h : h * b.Range(0.3f, 0.75f);
            var at = i == 0 ? Vector3.Zero : new Vector3(b.Range(-spread, spread) * 0.5f, 0, b.Range(-spread, spread) * 0.5f);
            using (b.At(at, b.Range(0, 360f), b.Range(-6f, 6f)))
            {
                b.Cyl("pedra@mundo", Vector3.Zero, height * 0.24f, height * 0.11f, height * 0.55f, 6, p.Tint, cap: false);
                b.Cyl("pedra@mundo", new Vector3(0, height * 0.55f, 0), height * 0.11f, 0f, height * 0.45f, 6, Mul(p.Tint, 1.1f));
            }
        }

        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(spread * 0.6f, h, spread * 0.6f));
    }

    /// <summary>
    /// O olmo dos sonhos vãos (Vestíbulo): tronco enorme e torto, raízes à mostra, galhos nus abertos e, debaixo de
    /// cada galho, os sonhos pendurados como trapos pálidos.
    /// </summary>
    private static void DreamElm(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 7f);
        var trunk = h * 0.42f;
        var hang = new System.Collections.Generic.List<Vector3>();
        for (var i = 0; i < 6; i++)
        {
            using (b.At(Vector3.Zero, 60f * i + b.Range(-15f, 15f)))
            {
                b.Rock("casca", new Vector3(0, 0, 1.25f), new Vector3(0.7f, 0.55f, 1.5f), Shade, 3, 6, 0.2f);
            }
        }

        b.Cyl("casca", Vector3.Zero, 1.15f, 0.8f, trunk * 0.55f, 8, White);
        using (b.At(new Vector3(0, trunk * 0.55f, 0), b.Range(0, 360f), 7f))
        {
            b.Cyl("casca", Vector3.Zero, 0.8f, 0.62f, trunk * 0.5f, 8, White);
            for (var i = 0; i < 5; i++)
            {
                using (b.At(new Vector3(0, trunk * 0.42f, 0), 72f * i + b.Range(-18f, 18f), b.Range(48f, 68f)))
                {
                    var length = h * b.Range(0.42f, 0.6f);
                    b.Cyl("casca", Vector3.Zero, 0.36f, 0.14f, length, 6, Shade);
                    foreach (var t in new[] { 0.45f, 0.7f, 0.95f })
                    {
                        hang.Add(b.Xf * new Vector3(0, length * t, 0));
                    }

                    foreach (var side in new[] { -1f, 1f })
                    {
                        using (b.At(new Vector3(0, length * b.Range(0.4f, 0.6f), 0), 90f * side + b.Range(-20f, 20f), b.Range(35f, 55f)))
                        {
                            var twig = length * b.Range(0.35f, 0.5f);
                            b.Cyl("casca", Vector3.Zero, 0.14f, 0.05f, twig, 5, Shade);
                            hang.Add(b.Xf * new Vector3(0, twig * 0.8f, 0));
                        }
                    }
                }
            }
        }

        // Os sonhos: trapos pálidos pendurados, cada um de um tamanho.
        var pale = new Color(0.82f, 0.84f, 0.8f);
        foreach (var at in hang)
        {
            if (b.Chance(0.2f))
            {
                continue;
            }

            var w = b.Range(0.14f, 0.26f);
            var drop = b.Range(0.35f, 0.8f);
            var yaw = b.Range(0f, Mathf.Pi);
            var side = new Vector3(Mathf.Cos(yaw), 0, Mathf.Sin(yaw)) * w;
            var top = at + new Vector3(0, -0.05f, 0);
            var color = Mul(pale, b.Range(0.7f, 1f));
            b.Quad("pano", top - side + new Vector3(0, -drop, 0), top + side + new Vector3(0, -drop, 0), top + side, top - side, color);
            b.Quad("pano", top + side + new Vector3(0, -drop, 0), top - side + new Vector3(0, -drop, 0), top - side, top + side, Mul(color, 0.85f));
        }

        b.Collide(new Vector3(0, 1.5f, 0), new Vector3(2.2f, 3f, 2.2f));
    }

    /// <summary>Murta dos Campos do Luto: troncos baixos e tortos, copa escura em tufos e flores pálidas miúdas.</summary>
    private static void Myrtle(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 2.8f) * b.Range(0.9f, 1.1f);
        var trunks = 2 + b.Pick(2);
        for (var i = 0; i < trunks; i++)
        {
            using (b.At(Vector3.Zero, 360f / trunks * i + b.Range(-30f, 30f), b.Range(14f, 30f)))
            {
                var length = h * b.Range(0.5f, 0.62f);
                b.Cyl("casca", Vector3.Zero, 0.16f, 0.09f, length, 5, Shade);
                var tuft = h * b.Range(0.45f, 0.62f);
                b.Rock("copa", new Vector3(0, length - tuft * 0.3f, 0), new Vector3(tuft, tuft * 0.75f, tuft),
                    Mul(p.Tint, b.Range(0.78f, 0.95f)), 3, 6, 0.3f);
            }
        }

        var crown = h * 0.78f;
        b.Rock("copa", new Vector3(0, h * 0.48f, 0), new Vector3(crown, h * 0.52f, crown), p.Tint, 4, 7, 0.3f);
        var pale = new Color(0.86f, 0.86f, 0.8f);
        for (var i = 0; i < 9; i++)
        {
            var angle = b.Range(0f, Mathf.Tau);
            var up = b.Range(0.25f, 0.95f);
            var reach = crown * 0.5f * Mathf.Sqrt(1f - (up - 0.5f) * (up - 0.5f) * 3f);
            var at = new Vector3(Mathf.Sin(angle) * reach, h * (0.48f + 0.5f * up), Mathf.Cos(angle) * reach);
            b.Box("pano", at, new Vector3(0.09f, 0.09f, 0.09f), Mul(pale, b.Range(0.8f, 1f)));
        }

        b.Collide(new Vector3(0, 1f, 0), new Vector3(0.6f, 2f, 0.6f));
    }

    /// <summary>A romãzeira do jardim de Perséfone: tronco torto, copa baixa e as romãs, o único vermelho forte.</summary>
    private static void Pomegranate(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 2.4f);
        using (b.At(Vector3.Zero, b.Range(0, 360f), b.Range(6f, 12f)))
        {
            b.Cyl("casca", Vector3.Zero, 0.17f, 0.1f, h * 0.55f, 6, Shade);
        }

        var crown = h * 0.95f;
        b.Rock("copa", new Vector3(0, h * 0.4f, 0), new Vector3(crown, h * 0.6f, crown), p.Tint, 4, 7, 0.3f);
        var red = new Color(0.78f, 0.09f, 0.1f);
        for (var i = 0; i < 8; i++)
        {
            var angle = b.Range(0f, Mathf.Tau);
            var up = b.Range(0.15f, 0.8f);
            var reach = crown * 0.5f * Mathf.Sqrt(1f - (up - 0.5f) * (up - 0.5f) * 3f);
            var at = new Vector3(Mathf.Sin(angle) * reach, h * (0.42f + 0.55f * up), Mathf.Cos(angle) * reach);
            b.Box("pano", at, new Vector3(0.16f, 0.16f, 0.16f), Mul(red, b.Range(0.85f, 1f)));
        }

        b.Collide(new Vector3(0, 1f, 0), new Vector3(0.6f, 2f, 0.6f));
    }

    /// <summary>
    /// Arma fincada no chão (Campo dos Guerreiros). Variantes: 0 lança, 1 lança com escudo encostado, 2 espada,
    /// 3 troféu (lança com elmo e um trapo de manto).
    /// </summary>
    private static void PlantedWeapon(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 1.9f);
        var bronze = Mul(new Color(0.78f, 0.6f, 0.34f), b.Range(0.75f, 1f));
        var iron = new Color(0.55f, 0.56f, 0.6f);
        using (b.At(Vector3.Zero, b.Range(0, 360f), b.Range(-13f, 13f), b.Range(-9f, 9f)))
        {
            if (p.Variant == 2)
            {
                var blade = h * 0.42f;
                b.Block("marmore", Vector3.Zero, new Vector3(0.16f, blade, 0.05f), iron);
                b.Block("marmore", new Vector3(0, blade, 0), new Vector3(0.44f, 0.08f, 0.1f), bronze);
                b.Cyl("madeira", new Vector3(0, blade + 0.05f, 0), 0.03f, 0.03f, 0.2f, 5, Shade);
                b.Block("marmore", new Vector3(0, blade + 0.25f, 0), new Vector3(0.08f, 0.07f, 0.08f), bronze);
            }
            else
            {
                b.Cyl("madeira", Vector3.Zero, 0.06f, 0.05f, h, 5, White);
                b.Cyl("marmore", new Vector3(0, h, 0), 0.12f, 0f, 0.42f, 4, bronze);
                if (p.Variant == 1)
                {
                    using (b.At(new Vector3(0, 0.48f, 0.16f), 0f, 74f))
                    {
                        b.Cyl("marmore", Vector3.Zero, 0.55f, 0.55f, 0.07f, 10, bronze);
                        b.Cyl("marmore", new Vector3(0, 0.05f, 0), 0.14f, 0f, 0.09f, 6, Mul(bronze, 1.2f));
                    }
                }
                else if (p.Variant == 3)
                {
                    var top = new Vector3(0, h - 0.08f, 0);
                    b.Lathe("marmore", top, new[] { (0.17f, 0f), (0.2f, 0.12f), (0.14f, 0.27f), (0.02f, 0.32f) }, 8, bronze);
                    b.Block("pano", top + new Vector3(0, 0.3f, 0), new Vector3(0.05f, 0.16f, 0.36f), new Color(0.22f, 0.2f, 0.22f));
                    var y = h * 0.72f;
                    var drop = h * 0.5f;
                    var cloth = new Color(0.36f, 0.32f, 0.3f);
                    b.Box("madeira", new Vector3(0, y, 0), new Vector3(0.84f, 0.05f, 0.05f), Shade);
                    b.Quad("pano", new Vector3(-0.4f, y - drop, 0.03f), new Vector3(0.36f, y - drop * 0.8f, 0.03f),
                        new Vector3(0.4f, y, 0.03f), new Vector3(-0.4f, y, 0.03f), cloth);
                    b.Quad("pano", new Vector3(0.36f, y - drop * 0.8f, -0.03f), new Vector3(-0.4f, y - drop, -0.03f),
                        new Vector3(-0.4f, y, -0.03f), new Vector3(0.4f, y, -0.03f), Mul(cloth, 0.8f));
                }
            }
        }

        b.Collide(new Vector3(0, 0.6f, 0), new Vector3(0.35f, 1.2f, 0.35f));
    }

    /// <summary>Estrado de mármore em três degraus (a frente e os lados recuam; o fundo é reto).</summary>
    private static void Dais(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 8f);
        var h = Or(p.Size.Y, 0.6f);
        var d = Or(p.Size.Z, 4f);
        const int steps = 3;
        const float tread = 0.36f;
        for (var i = 0; i < steps; i++)
        {
            b.Block("marmore", new Vector3(0, h * i / steps, -tread * i / 2f),
                new Vector3(w - tread * 2f * i, h / steps, d - tread * i), Mul(p.Tint, 0.78f + 0.1f * i));
        }

        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(w, h, d));
    }

    /// <summary>Trono de pedra: assento, encosto alto com cornija, braços e escabelo.</summary>
    private static void Throne(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 2.2f);
        var w = Or(p.Size.X, 1.3f);
        const float seat = 0.55f;
        b.Block("marmore", Vector3.Zero, new Vector3(w, seat, w * 0.9f), Mul(p.Tint, 0.9f));
        b.Block("marmore", new Vector3(0, seat, -w * 0.37f), new Vector3(w, h - seat, w * 0.16f), p.Tint);
        b.Block("marmore", new Vector3(0, h, -w * 0.37f), new Vector3(w * 1.12f, 0.12f, w * 0.24f), Mul(p.Tint, 0.75f));
        foreach (var side in new[] { -1f, 1f })
        {
            b.Block("marmore", new Vector3(side * (w / 2f - 0.09f), seat, 0.02f), new Vector3(0.18f, 0.32f, w * 0.7f), Mul(p.Tint, 0.8f));
        }

        b.Block("marmore", new Vector3(0, 0, w * 0.62f), new Vector3(w * 0.7f, 0.16f, 0.4f), Mul(p.Tint, 0.8f));
        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(w, h, w));
    }

    // ---- fogo e luz ----

    /// <summary>
    /// Uma chama em pixel art (cartão virado para a câmera) e, com <paramref name="energy"/> acima de zero, a luz
    /// quente que treme.
    /// </summary>
    private static void Flame(PieceBuilder b, Vector3 at, float scale, float energy, float range)
    {
        var material = SceneryMaterials.Own("chama");
        material.SetShaderParameter("ruido", SceneryMaterials.Texture("ruido"));
        material.SetShaderParameter("tamanho", new Vector2(0.42f, 0.7f) * scale);
        material.SetShaderParameter("fase", b.Range(0f, 10f));
        b.Attach(new MeshInstance3D
        {
            Name = "Chama",
            Mesh = new QuadMesh { Size = Vector2.One },
            MaterialOverride = material,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            // O shader move os vértices: a caixa de corte precisa cobrir a chama inteira.
            CustomAabb = new Aabb(new Vector3(-1, -0.5f, -1), new Vector3(2, 2, 2)),
        }, at);
        if (energy > 0f)
        {
            b.Attach(new Flicker
            {
                Name = "Luz",
                LightColor = new Color(1f, 0.62f, 0.3f),
                BaseEnergy = energy,
                OmniRange = range,
                OmniAttenuation = 1.4f,
                Phase = b.Range(0f, 10f),
            }, at + new Vector3(0, 0.35f * scale, 0));
        }
    }

    /// <summary>Tocha de pé: uma haste com a cesta de ferro e a chama.</summary>
    private static void Torch(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 1.9f);
        b.Block("madeira", Vector3.Zero, new Vector3(0.09f, h, 0.09f), Shade);
        b.Block("muro", Vector3.Zero, new Vector3(0.34f, 0.12f, 0.34f), Shade);
        b.Cyl("madeira", new Vector3(0, h, 0), 0.07f, 0.15f, 0.22f, 6, Mul(White, 0.5f));
        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(0.3f, h, 0.3f));
        Flame(b, new Vector3(0, h + 0.2f, 0), 0.85f, p.Lit ? Or(p.Size.X, 1.3f) : 0f, Or(p.Size.Z, 7f));
    }

    /// <summary>Braseiro: uma bacia de bronze sobre três pés, com brasas e chama.</summary>
    private static void Brazier(PieceBuilder b, SceneryPiece p)
    {
        var bronze = new Color(0.75f, 0.55f, 0.3f);
        for (var i = 0; i < 3; i++)
        {
            using (b.At(Vector3.Zero, 120f * i))
            {
                using (b.At(new Vector3(0, 0, 0.26f), 0f, -14f))
                {
                    b.Block("madeira", Vector3.Zero, new Vector3(0.05f, 0.75f, 0.05f), Mul(bronze, 0.6f));
                }
            }
        }

        b.Lathe("telha", new Vector3(0, 0.68f, 0), new[] { (0.12f, 0f), (0.36f, 0.14f), (0.4f, 0.2f) }, 8, bronze, cap: false);
        b.Cyl(SceneryMaterials.Glow, new Vector3(0, 0.84f, 0), 0.34f, 0.34f, 0.02f, 8, Mul(Ember, 0.7f));
        b.Collide(new Vector3(0, 0.45f, 0), new Vector3(0.7f, 0.9f, 0.7f));
        Flame(b, new Vector3(0, 0.84f, 0), 1.1f, p.Lit ? Or(p.Size.X, 1.5f) : 0f, Or(p.Size.Z, 8f));
    }

    // ---- lugares ----

    /// <summary>Poço de aldeia: a boca de pedra, dois esteios, a trave e o balde.</summary>
    private static void Well(PieceBuilder b, SceneryPiece p)
    {
        const float r = 0.7f;
        StoneRing(b, r, 0.75f, 9);
        b.Cyl(SceneryMaterials.Dark, new Vector3(0, 0.4f, 0), r - 0.2f, r - 0.2f, 0.02f, 9, DoorDark);
        foreach (var side in new[] { -1f, 1f })
        {
            b.Block("madeira", new Vector3(side * (r + 0.05f), 0, 0), new Vector3(0.12f, 2f, 0.12f), White);
        }

        b.Box("madeira", new Vector3(0, 1.95f, 0), new Vector3(2f * r + 0.5f, 0.12f, 0.14f), White);
        b.Box("madeira", new Vector3(0.1f, 1.45f, 0), new Vector3(0.03f, 0.9f, 0.03f), Shade);
        b.Cyl("madeira", new Vector3(0.1f, 0.85f, 0), 0.13f, 0.16f, 0.24f, 7, Shade);
        b.Collide(new Vector3(0, 0.5f, 0), new Vector3(2f * r, 1f, 2f * r));
    }

    private static void StoneRing(PieceBuilder b, float radius, float height, int stones, float missing = 0f)
    {
        for (var i = 0; i < stones; i++)
        {
            if (b.Chance(missing))
            {
                continue;
            }

            using (b.At(Vector3.Zero, 360f * i / stones))
            {
                var width = Mathf.Tau * radius / stones;
                b.Block("muro", new Vector3(0, 0, radius), new Vector3(width * 1.05f, height * b.Range(0.8f, 1.1f), 0.32f), White);
            }
        }
    }

    /// <summary>
    /// O Poço das Sem-Nome: um fosso largo de borda de pedra roída, escuro por dentro. O diâmetro é o X do tamanho.
    /// </summary>
    private static void Pit(PieceBuilder b, SceneryPiece p)
    {
        var r = Or(p.Size.X, 5f) / 2f;
        StoneRing(b, r, 0.55f, Mathf.RoundToInt(r * 7f), missing: 0.18f);
        StoneRing(b, r + 0.45f, 0.2f, Mathf.RoundToInt(r * 6f), missing: 0.3f);
        b.Cyl(SceneryMaterials.Dark, new Vector3(0, 0.03f, 0), r - 0.1f, r - 0.1f, 0.02f, 16, DoorDark);
    }

    /// <summary>Estela: uma laje de pedra em pé com linhas gravadas, um pouco tombada.</summary>
    private static void Stele(PieceBuilder b, SceneryPiece p)
    {
        var h = Or(p.Size.Y, 1.3f);
        b.Block("muro", Vector3.Zero, new Vector3(0.8f, 0.14f, 0.4f), Shade);
        using (b.At(new Vector3(0, 0.14f, 0), 0f, b.Range(-5f, 5f), b.Range(-7f, 7f)))
        {
            b.Block("marmore", Vector3.Zero, new Vector3(0.6f, h, 0.16f), Mul(p.Tint, 0.85f));
            b.Block("marmore", new Vector3(0, h, 0), new Vector3(0.7f, 0.1f, 0.2f), Mul(p.Tint, 0.7f));
            for (var i = 0; i < 4; i++)
            {
                b.Box(SceneryMaterials.Dark, new Vector3(b.Range(-0.04f, 0.04f), h * (0.82f - i * 0.15f), 0.082f),
                    new Vector3(b.Range(0.25f, 0.44f), 0.03f, 0.006f), new Color(0.13f, 0.13f, 0.14f));
            }
        }

        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(0.7f, h, 0.3f));
    }

    /// <summary>Cerca de varas ao longo do X da peça.</summary>
    private static void Fence(PieceBuilder b, SceneryPiece p)
    {
        var length = Or(p.Size.X, 4f);
        var h = Or(p.Size.Y, 0.95f);
        var posts = Mathf.Max(2, Mathf.RoundToInt(length / 1.3f) + 1);
        for (var i = 0; i < posts; i++)
        {
            var x = -length / 2f + i * length / (posts - 1);
            using (b.At(new Vector3(x, 0, 0), 0f, b.Range(-4f, 4f), b.Range(-5f, 5f)))
            {
                b.Block("madeira", Vector3.Zero, new Vector3(0.1f, h * b.Range(0.95f, 1.15f), 0.1f), White);
            }
        }

        foreach (var y in new[] { h * 0.45f, h * 0.8f })
        {
            using (b.At(new Vector3(0, y, 0), 0f, 0f, b.Range(-1.5f, 1.5f)))
            {
                b.Box("madeira", Vector3.Zero, new Vector3(length, 0.07f, 0.05f), Shade);
            }
        }

        b.Collide(new Vector3(0, h / 2f, 0), new Vector3(length, h, 0.2f));
    }

    /// <summary>Cais de tábuas sobre estacas: o comprimento é o Z da peça (entra na água para o +Z).</summary>
    private static void Pier(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 2.2f);
        var length = Or(p.Size.Z, 4f);
        var planks = Mathf.RoundToInt(length / 0.32f);
        for (var i = 0; i < planks; i++)
        {
            var z = -length / 2f + (i + 0.5f) * length / planks;
            b.Box("madeira", new Vector3(b.Range(-0.04f, 0.04f), 0.03f, z), new Vector3(w * b.Range(0.94f, 1.02f), 0.07f, length / planks - 0.03f), White);
        }

        for (var z = -length / 2f + 0.3f; z <= length / 2f; z += 1.6f)
        {
            foreach (var side in new[] { -1f, 1f })
            {
                b.Box("madeira", new Vector3(side * (w / 2f - 0.05f), -0.1f, z), new Vector3(0.16f, b.Range(0.9f, 1.4f), 0.16f), Shade);
            }
        }
    }

    /// <summary>A barca: casco de tábuas afilado nas duas pontas, bancos e a vara de empurrar.</summary>
    private static void Boat(PieceBuilder b, SceneryPiece p)
    {
        var length = Or(p.Size.X, 5f);
        var beam = Or(p.Size.Z, 1.5f);
        var stations = new (float X, float Half, float Top)[]
        {
            (-0.5f, 0.02f, 0.7f), (-0.36f, 0.34f, 0.5f), (-0.12f, 0.5f, 0.42f), (0.14f, 0.5f, 0.42f), (0.38f, 0.32f, 0.52f), (0.5f, 0.02f, 0.78f),
        };
        var dark = Mul(p.Tint, 0.55f);
        for (var i = 0; i < stations.Length - 1; i++)
        {
            var (x0, w0, t0) = stations[i];
            var (x1, w1, t1) = stations[i + 1];
            Vector3 P(float x, float y, float halfBeam, float side) => new(x * length, y, side * halfBeam * beam);
            foreach (var side in new[] { 1f, -1f })
            {
                var a = P(x0, 0.05f, w0 * 0.55f, side);
                var c = P(x1, 0.05f, w1 * 0.55f, side);
                var top0 = P(x0, t0, w0, side);
                var top1 = P(x1, t1, w1, side);
                if (side > 0)
                {
                    b.Quad("madeira", a, c, top1, top0, dark);
                }
                else
                {
                    b.Quad("madeira", c, a, top0, top1, dark);
                }
            }

            // O fundo, por dentro.
            b.Quad("madeira", P(x0, 0.08f, w0 * 0.55f, 1), P(x1, 0.08f, w1 * 0.55f, 1), P(x1, 0.08f, w1 * 0.55f, -1),
                P(x0, 0.08f, w0 * 0.55f, -1), Mul(dark, 0.7f));
        }

        foreach (var x in new[] { -0.2f, 0.08f, 0.3f })
        {
            b.Box("madeira", new Vector3(x * length, 0.34f, 0), new Vector3(0.22f, 0.05f, beam * 0.88f), p.Tint);
        }

        using (b.At(new Vector3(length * 0.2f, 0.4f, beam * 0.2f), 20f, 0f, 68f))
        {
            b.Block("madeira", Vector3.Zero, new Vector3(0.06f, length * 0.9f, 0.06f), White);
        }

        b.Collide(new Vector3(0, 0.4f, 0), new Vector3(length, 0.8f, beam));
    }

    /// <summary>Varal: dois esteios, a corda e panos pendurados (a lavadeira).</summary>
    private static void Clothesline(PieceBuilder b, SceneryPiece p)
    {
        var length = Or(p.Size.X, 3.4f);
        foreach (var side in new[] { -1f, 1f })
        {
            b.Block("madeira", new Vector3(side * length / 2f, 0, 0), new Vector3(0.09f, 1.9f, 0.09f), White);
        }

        b.Box("madeira", new Vector3(0, 1.8f, 0), new Vector3(length, 0.025f, 0.025f), Shade);
        var colors = new[] { new Color(0.85f, 0.82f, 0.74f), new Color(0.6f, 0.36f, 0.3f), new Color(0.42f, 0.55f, 0.56f), new Color(0.7f, 0.68f, 0.62f) };
        var x = -length / 2f + 0.3f;
        while (x < length / 2f - 0.5f)
        {
            var w = b.Range(0.4f, 0.75f);
            var drop = b.Range(0.6f, 1.1f);
            var color = colors[b.Pick(colors.Length)];
            b.Quad("pano", new Vector3(x, 1.8f - drop, 0.01f), new Vector3(x + w, 1.8f - drop, 0.01f), new Vector3(x + w, 1.8f, 0.01f),
                new Vector3(x, 1.8f, 0.01f), color);
            b.Quad("pano", new Vector3(x + w, 1.8f - drop, -0.01f), new Vector3(x, 1.8f - drop, -0.01f), new Vector3(x, 1.8f, -0.01f),
                new Vector3(x + w, 1.8f, -0.01f), Mul(color, 0.8f));
            x += w + b.Range(0.1f, 0.4f);
        }
    }

    /// <summary>Carroça de mão: a caixa, duas rodas e os varais, às vezes com carga.</summary>
    private static void Cart(PieceBuilder b, SceneryPiece p)
    {
        b.Box("madeira", new Vector3(0, 0.62f, 0), new Vector3(1.7f, 0.08f, 1f), White);
        foreach (var z in new[] { -0.5f, 0.5f })
        {
            b.Box("madeira", new Vector3(0, 0.82f, z), new Vector3(1.7f, 0.34f, 0.05f), Shade);
            using (b.At(new Vector3(-0.15f, 0.42f, z * 1.16f), 0f, 90f))
            {
                b.Cyl("madeira", new Vector3(0, -0.04f, 0), 0.42f, 0.42f, 0.08f, 10, Mul(White, 0.7f));
            }
        }

        b.Box("madeira", new Vector3(-0.85f, 0.82f, 0), new Vector3(0.05f, 0.34f, 1f), Shade);
        foreach (var z in new[] { -0.4f, 0.4f })
        {
            using (b.At(new Vector3(0.85f, 0.6f, z), 0f, 0f, -12f))
            {
                b.Box("madeira", new Vector3(0.6f, 0, 0), new Vector3(1.2f, 0.06f, 0.06f), White);
            }
        }

        if (p.Variant != 1)
        {
            Amphora(b, new Vector3(-0.4f, 0.66f, 0.15f), 0.6f, Clay(b));
            Sack(b, new Vector3(0.25f, 0.66f, -0.1f), 0.5f);
        }

        b.Collide(new Vector3(0, 0.5f, 0), new Vector3(1.8f, 1f, 1.1f));
    }

    /// <summary>Cova: um monte de terra com um marco (uma ânfora quebrada, uma pedra ou uma estaca).</summary>
    private static void Grave(PieceBuilder b, SceneryPiece p)
    {
        b.Wear = 0.04f;
        b.Rock("terra", Vector3.Zero, new Vector3(0.95f, 0.34f, 1.9f), White, 3, 8, 0.12f);
        var head = new Vector3(0, 0, -1.05f);
        switch (p.Variant % 3)
        {
            case 0:
                b.Block("muro", head, new Vector3(0.42f, 0.6f, 0.14f), Shade);
                break;
            case 1:
                Amphora(b, head, 0.55f, Mul(Clay(b), 0.8f));
                break;
            default:
                b.Block("madeira", head, new Vector3(0.07f, 0.8f, 0.07f), Shade);
                b.Box("madeira", head + new Vector3(0, 0.6f, 0), new Vector3(0.4f, 0.06f, 0.06f), Shade);
                break;
        }
    }

    /// <summary>Mesa com bancos, e o que ficou em cima.</summary>
    private static void Table(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 1.8f);
        b.Box("madeira", new Vector3(0, 0.76f, 0), new Vector3(w, 0.07f, 0.8f), White);
        foreach (var sx in new[] { -1f, 1f })
        {
            b.Block("madeira", new Vector3(sx * (w / 2f - 0.12f), 0, 0), new Vector3(0.1f, 0.74f, 0.66f), Shade);
        }

        foreach (var z in new[] { -0.75f, 0.75f })
        {
            b.Box("madeira", new Vector3(0, 0.44f, z), new Vector3(w * 0.9f, 0.06f, 0.26f), White);
            b.Block("madeira", new Vector3(-w * 0.35f, 0, z), new Vector3(0.07f, 0.42f, 0.22f), Shade);
            b.Block("madeira", new Vector3(w * 0.35f, 0, z), new Vector3(0.07f, 0.42f, 0.22f), Shade);
        }

        for (var i = 0; i < 3; i++)
        {
            Bowl(b, new Vector3(b.Range(-w * 0.35f, w * 0.35f), 0.8f, b.Range(-0.22f, 0.22f)), b.Range(0.1f, 0.17f), Clay(b));
        }

        b.Collide(new Vector3(0, 0.4f, 0), new Vector3(w, 0.8f, 0.8f));
    }

    /// <summary>Forno de pão: a base de pedra, a cúpula de barro, a boca com brasa e a luz dela.</summary>
    private static void Oven(PieceBuilder b, SceneryPiece p) => OvenAt(b, Or(p.Size.X, 2f), p.Lit);

    private static void OvenAt(PieceBuilder b, float w, bool lit)
    {
        b.Block("muro", Vector3.Zero, new Vector3(w, 0.7f, w), White);
        b.Lathe("reboco", new Vector3(0, 0.7f, 0), new[] { (w * 0.48f, 0f), (w * 0.44f, 0.4f), (w * 0.3f, 0.75f), (w * 0.08f, 0.92f) }, 9,
            new Color(0.95f, 0.8f, 0.7f));
        b.Block("reboco", new Vector3(0, 1.55f, -w * 0.15f), new Vector3(0.3f, 0.5f, 0.3f), Shade);
        // A boca virada para a frente, com a brasa lá dentro.
        b.Box(SceneryMaterials.Dark, new Vector3(0, 1.02f, w * 0.46f), new Vector3(0.62f, 0.5f, 0.05f), DoorDark);
        b.Box(SceneryMaterials.Glow, new Vector3(0, 0.9f, w * 0.47f), new Vector3(0.5f, 0.2f, 0.04f), Mul(Ember, 0.8f));
        b.Block("muro", new Vector3(0, 0.7f, w * 0.55f), new Vector3(0.9f, 0.08f, 0.3f), Shade);
        b.Collide(new Vector3(0, 0.8f, 0), new Vector3(w, 1.6f, w));
        if (lit)
        {
            b.Attach(new Flicker
            {
                Name = "Brasa",
                LightColor = new Color(1f, 0.5f, 0.2f),
                BaseEnergy = 0.9f,
                OmniRange = 5f,
                OmniAttenuation = 1.5f,
                Phase = b.Range(0f, 10f),
            }, new Vector3(0, 1f, w * 0.75f));
        }
    }

    /// <summary>A roda do oleiro: o banco, o disco e um pote por terminar.</summary>
    private static void PotterWheel(PieceBuilder b, SceneryPiece p)
    {
        b.Cyl("madeira", Vector3.Zero, 0.34f, 0.34f, 0.1f, 9, Shade);
        b.Cyl("madeira", new Vector3(0, 0.1f, 0), 0.06f, 0.06f, 0.5f, 6, White);
        b.Cyl("madeira", new Vector3(0, 0.6f, 0), 0.3f, 0.3f, 0.06f, 9, White);
        b.Lathe("telha", new Vector3(0, 0.66f, 0), new[] { (0.1f, 0f), (0.17f, 0.14f), (0.13f, 0.28f) }, 8, Mul(Clay(b), 0.8f), cap: false);
        b.Block("madeira", new Vector3(0.6f, 0, 0.1f), new Vector3(0.36f, 0.4f, 0.36f), Shade);
        b.Collide(new Vector3(0, 0.4f, 0), new Vector3(0.7f, 0.8f, 0.7f));
    }

    /// <summary>O tear vertical: dois esteios, as traves, os fios e o pano começado.</summary>
    private static void Loom(PieceBuilder b, SceneryPiece p)
    {
        foreach (var side in new[] { -1f, 1f })
        {
            b.Block("madeira", new Vector3(side * 0.8f, 0, 0), new Vector3(0.1f, 1.95f, 0.1f), White);
        }

        b.Box("madeira", new Vector3(0, 1.9f, 0), new Vector3(1.8f, 0.1f, 0.1f), White);
        b.Box("madeira", new Vector3(0, 0.5f, 0), new Vector3(1.6f, 0.06f, 0.06f), Shade);
        b.Quad("pano", new Vector3(-0.7f, 1.2f, 0.02f), new Vector3(0.7f, 1.2f, 0.02f), new Vector3(0.7f, 1.85f, 0.02f),
            new Vector3(-0.7f, 1.85f, 0.02f), new Color(0.62f, 0.42f, 0.36f));
        for (var i = 0; i < 9; i++)
        {
            var x = -0.66f + i * 0.165f;
            b.Box("pano", new Vector3(x, 0.85f, 0.02f), new Vector3(0.02f, 0.7f, 0.01f), new Color(0.85f, 0.82f, 0.74f));
            b.Cyl("muro", new Vector3(x, 0.38f, 0.02f), 0.035f, 0.035f, 0.1f, 5, Shade);
        }

        b.Collide(new Vector3(0, 1f, 0), new Vector3(1.8f, 2f, 0.3f));
    }

    /// <summary>Tralha: um punhado de ânforas, caixotes, cestos e sacos amontoados na área X por Z.</summary>
    private static void Clutter(PieceBuilder b, SceneryPiece p)
    {
        var w = Or(p.Size.X, 1.6f);
        var d = Or(p.Size.Z, 1.2f);
        var count = 3 + b.Pick(4);
        for (var i = 0; i < count; i++)
        {
            var at = new Vector3(b.Range(-w / 2f, w / 2f), 0, b.Range(-d / 2f, d / 2f));
            switch ((p.Variant + b.Pick(4)) % 4)
            {
                case 0:
                    Amphora(b, at, b.Range(0.6f, 1f), Clay(b), fallen: b.Chance(0.2f));
                    break;
                case 1:
                    Crate(b, at, b.Range(0.4f, 0.7f));
                    break;
                case 2:
                    Basket(b, at, b.Range(0.4f, 0.55f), b.Chance(0.5f));
                    break;
                default:
                    Sack(b, at, b.Range(0.45f, 0.65f));
                    break;
            }
        }

        b.Collide(new Vector3(0, 0.4f, 0), new Vector3(w, 0.8f, d));
    }

    // ---- miudezas ----

    private static void Amphora(PieceBuilder b, Vector3 at, float height, Color clay, bool fallen = false)
    {
        var h = height;
        var profile = new[] { (0.05f * h, 0f), (0.24f * h, 0.3f * h), (0.27f * h, 0.52f * h), (0.16f * h, 0.78f * h), (0.09f * h, 0.9f * h), (0.13f * h, h) };
        if (fallen)
        {
            using (b.At(at + new Vector3(0, 0.26f * h, 0), b.Range(0, 360f), 88f))
            {
                b.Lathe("telha", Vector3.Zero, profile, 7, clay, cap: false);
            }
        }
        else
        {
            b.Lathe("telha", at, profile, 7, clay, cap: false);
        }
    }

    private static void Bowl(PieceBuilder b, Vector3 at, float radius, Color clay) =>
        b.Lathe("telha", at, new[] { (radius * 0.35f, 0f), (radius, radius * 0.5f), (radius * 0.92f, radius * 0.55f) }, 7, clay, cap: true);

    private static void Crate(PieceBuilder b, Vector3 at, float size)
    {
        using (b.At(at, b.Range(-25f, 25f)))
        {
            b.Block("madeira", Vector3.Zero, new Vector3(size, size * 0.8f, size), White);
            b.Box("madeira", new Vector3(0, size * 0.8f, 0), new Vector3(size * 1.04f, 0.04f, size * 1.04f), Shade);
        }
    }

    private static void Basket(PieceBuilder b, Vector3 at, float size, bool full)
    {
        b.Cyl("palha", at, size * 0.38f, size * 0.5f, size * 0.55f, 8, White, cap: !full);
        if (full)
        {
            // Pães ou frutos: um monte baixo de tom quente.
            b.Wear = 0.2f;
            b.Rock("palha", at + new Vector3(0, size * 0.45f, 0), new Vector3(size * 0.9f, size * 0.36f, size * 0.9f),
                new Color(1.1f, 0.8f, 0.55f), 3, 7, 0.15f);
            b.Wear = 0.08f;
        }
    }

    private static void Sack(PieceBuilder b, Vector3 at, float size) =>
        b.Rock("pano", at, new Vector3(size, size * 0.85f, size * 0.8f), new Color(0.7f, 0.62f, 0.5f), 3, 7, 0.14f);
}
