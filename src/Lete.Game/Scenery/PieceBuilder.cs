using System;
using System.Collections.Generic;
using Godot;

namespace Lete.Game.Scenery;

/// <summary>
/// Monta a malha de uma peça do cenário por código (paisagem HD-2D, PENDÊNCIA 105): caixas, cilindros, tornos e
/// telhados em faces chapadas (low-poly), uma superfície por textura. A cor do vértice tinge cada face (variação de
/// tom e desgaste). Guarda também as caixas de colisão da peça. As medidas são em metros.
/// </summary>
public sealed class PieceBuilder
{
    private readonly Dictionary<string, SurfaceTool> _surfaces = new();
    private readonly List<(Transform3D Xf, Vector3 Size)> _colliders = new();
    private readonly List<Node3D> _extras = new();

    public PieceBuilder(int seed)
    {
        Rng = new Random(seed);
    }

    /// <summary>O acaso da peça: a mesma semente monta sempre a mesma peça.</summary>
    public Random Rng { get; }

    /// <summary>Transformação aplicada ao que for montado a seguir (para deitar um tambor, inclinar um galho).</summary>
    public Transform3D Xf { get; set; } = Transform3D.Identity;

    /// <summary>Quanto o tom de cada face varia para mais ou para menos (0 = todas iguais).</summary>
    public float Wear { get; set; } = 0.08f;

    public float Range(float min, float max) => min + (float)Rng.NextDouble() * (max - min);

    public bool Chance(float probability) => Rng.NextDouble() < probability;

    public int Pick(int count) => Rng.Next(count);

    /// <summary>Monta o que vier a seguir com outra transformação e volta à anterior ao sair do bloco.</summary>
    public IDisposable At(Transform3D local)
    {
        var previous = Xf;
        Xf = previous * local;
        return new Restore(() => Xf = previous);
    }

    public IDisposable At(Vector3 origin, float yawDegrees = 0f, float pitchDegrees = 0f, float rollDegrees = 0f) =>
        At(new Transform3D(Basis.FromEuler(new Vector3(
            Mathf.DegToRad(pitchDegrees), Mathf.DegToRad(yawDegrees), Mathf.DegToRad(rollDegrees))), origin));

    private SurfaceTool Surface(string texture)
    {
        if (!_surfaces.TryGetValue(texture, out var tool))
        {
            tool = new SurfaceTool();
            tool.Begin(Mesh.PrimitiveType.Triangles);
            _surfaces[texture] = tool;
        }

        return tool;
    }

    private Color Worn(Color color)
    {
        var k = 1f + Range(-Wear, Wear);
        return new Color(color.R * k, color.G * k, color.B * k);
    }

    /// <summary>Triângulo com os vértices no sentido anti-horário vistos de fora.</summary>
    public void Tri(string texture, Vector3 a, Vector3 b, Vector3 c, Color color)
    {
        a = Xf * a;
        b = Xf * b;
        c = Xf * c;
        var normal = (b - a).Cross(c - a).Normalized();
        var tool = Surface(texture);
        // O Godot trata o sentido horário como a frente da face.
        foreach (var v in new[] { a, c, b })
        {
            tool.SetColor(color);
            tool.SetNormal(normal);
            tool.SetUV(Vector2.Zero);
            tool.AddVertex(v);
        }
    }

    /// <summary>Quadrilátero com os cantos no sentido anti-horário vistos de fora; UV de 0 a 1 (a = 0,1; c = 1,0).</summary>
    public void Quad(string texture, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color, bool worn = true)
    {
        var tint = worn ? Worn(color) : color;
        var ta = Xf * a;
        var tb = Xf * b;
        var tc = Xf * c;
        var td = Xf * d;
        var normal = (tb - ta).Cross(tc - ta).Normalized();
        var tool = Surface(texture);
        void Vertex(Vector3 v, Vector2 uv)
        {
            tool.SetColor(tint);
            tool.SetNormal(normal);
            tool.SetUV(uv);
            tool.AddVertex(v);
        }

        Vertex(ta, new Vector2(0, 1));
        Vertex(tc, new Vector2(1, 0));
        Vertex(tb, new Vector2(1, 1));
        Vertex(ta, new Vector2(0, 1));
        Vertex(td, new Vector2(0, 0));
        Vertex(tc, new Vector2(1, 0));
    }

    /// <summary>Caixa centrada em <paramref name="center"/>; sem a face de baixo (ninguém vê).</summary>
    public void Box(string texture, Vector3 center, Vector3 size, Color color, bool top = true)
    {
        var h = size / 2f;
        Vector3 P(float x, float y, float z) => center + new Vector3(x * h.X, y * h.Y, z * h.Z);
        Quad(texture, P(-1, -1, 1), P(1, -1, 1), P(1, 1, 1), P(-1, 1, 1), color);
        Quad(texture, P(1, -1, -1), P(-1, -1, -1), P(-1, 1, -1), P(1, 1, -1), color);
        Quad(texture, P(1, -1, 1), P(1, -1, -1), P(1, 1, -1), P(1, 1, 1), color);
        Quad(texture, P(-1, -1, -1), P(-1, -1, 1), P(-1, 1, 1), P(-1, 1, -1), color);
        if (top)
        {
            Quad(texture, P(-1, 1, 1), P(1, 1, 1), P(1, 1, -1), P(-1, 1, -1), color);
        }
    }

    /// <summary>Caixa apoiada no chão da peça: <paramref name="foot"/> é o centro da base.</summary>
    public void Block(string texture, Vector3 foot, Vector3 size, Color color, bool top = true) =>
        Box(texture, foot + new Vector3(0, size.Y / 2f, 0), size, color, top);

    /// <summary>Cilindro ou tronco de cone em pé, a partir do centro da base; raio de cima zero dá um cone.</summary>
    public void Cyl(string texture, Vector3 foot, float radiusBottom, float radiusTop, float height, int sides, Color color,
        bool cap = true)
    {
        for (var i = 0; i < sides; i++)
        {
            var a0 = Mathf.Tau * i / sides;
            var a1 = Mathf.Tau * (i + 1) / sides;
            Vector3 Ring(float angle, float radius, float y) =>
                foot + new Vector3(Mathf.Sin(angle) * radius, y, Mathf.Cos(angle) * radius);
            var b0 = Ring(a0, radiusBottom, 0);
            var b1 = Ring(a1, radiusBottom, 0);
            var t0 = Ring(a0, radiusTop, height);
            var t1 = Ring(a1, radiusTop, height);
            if (radiusTop <= 0.001f)
            {
                Tri(texture, b0, b1, t0, Worn(color));
            }
            else
            {
                Quad(texture, b0, b1, t1, t0, color);
                if (cap)
                {
                    Tri(texture, t0, t1, foot + new Vector3(0, height, 0), Worn(color));
                }
            }
        }
    }

    /// <summary>Peça de torno (ânfora, coluna, tigela): o perfil são pares (raio, altura) de baixo para cima.</summary>
    public void Lathe(string texture, Vector3 foot, IReadOnlyList<(float Radius, float Y)> profile, int sides, Color color,
        bool cap = true)
    {
        for (var p = 0; p < profile.Count - 1; p++)
        {
            var (r0, y0) = profile[p];
            var (r1, y1) = profile[p + 1];
            Cyl(texture, foot + new Vector3(0, y0, 0), r0, Mathf.Max(r1, 0.002f), y1 - y0, sides, color,
                cap && p == profile.Count - 2);
        }
    }

    /// <summary>
    /// Pedra bruta: uma esfera de poucas faces com os vértices empurrados ao acaso, achatada em <paramref name="size"/>
    /// e cortada no chão. Serve para rochas, entulho, montes de terra e sacos.
    /// </summary>
    public void Rock(string texture, Vector3 foot, Vector3 size, Color color, int rings = 4, int segments = 7,
        float roughness = 0.28f)
    {
        var points = new Vector3[rings + 1, segments];
        for (var r = 0; r <= rings; r++)
        {
            var lat = Mathf.Pi * r / rings;
            for (var s = 0; s < segments; s++)
            {
                var lon = Mathf.Tau * s / segments;
                var k = r == 0 || r == rings ? 1f : 1f + Range(-roughness, roughness);
                var unit = new Vector3(Mathf.Sin(lat) * Mathf.Sin(lon), Mathf.Cos(lat), Mathf.Sin(lat) * Mathf.Cos(lon)) * k;
                var p = new Vector3(unit.X * size.X / 2f, unit.Y * size.Y * 0.62f + size.Y * 0.38f, unit.Z * size.Z / 2f);
                points[r, s] = foot + new Vector3(p.X, Mathf.Max(p.Y, 0f), p.Z);
            }
        }

        for (var r = 0; r < rings; r++)
        {
            for (var s = 0; s < segments; s++)
            {
                var n = (s + 1) % segments;
                var a = points[r + 1, s];
                var b = points[r + 1, n];
                var c = points[r, n];
                var d = points[r, s];
                if (r == 0)
                {
                    Tri(texture, a, b, d, Worn(color));
                }
                else if (r == rings - 1)
                {
                    Tri(texture, a, c, d, Worn(color));
                }
                else
                {
                    var tint = Worn(color);
                    Tri(texture, a, b, c, tint);
                    Tri(texture, a, c, d, Worn(color));
                }
            }
        }
    }

    /// <summary>Caixa de colisão da peça (no espaço da peça, com a transformação atual).</summary>
    public void Collide(Vector3 center, Vector3 size) => _colliders.Add((Xf * new Transform3D(Basis.Identity, center), size));

    /// <summary>Um nó a mais que acompanha a peça (luz, chama), já na transformação atual.</summary>
    public void Attach(Node3D node, Vector3 position)
    {
        node.Position = Xf * position;
        _extras.Add(node);
    }

    /// <summary>Põe a malha, a colisão e os nós a mais dentro de <paramref name="parent"/>.</summary>
    public void Commit(Node3D parent, bool castShadow = true)
    {
        if (_surfaces.Count > 0)
        {
            var mesh = new ArrayMesh();
            foreach (var (texture, tool) in _surfaces)
            {
                tool.Commit(mesh);
                mesh.SurfaceSetMaterial(mesh.GetSurfaceCount() - 1, SceneryMaterials.For(texture));
            }

            parent.AddChild(new MeshInstance3D
            {
                Name = "Malha",
                Mesh = mesh,
                CastShadow = castShadow ? GeometryInstance3D.ShadowCastingSetting.On : GeometryInstance3D.ShadowCastingSetting.Off,
            });
        }

        if (_colliders.Count > 0)
        {
            var body = new StaticBody3D { Name = "Colisao" };
            foreach (var (xf, size) in _colliders)
            {
                body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size }, Transform = xf });
            }

            parent.AddChild(body);
        }

        foreach (var extra in _extras)
        {
            parent.AddChild(extra);
        }
    }

    private sealed class Restore : IDisposable
    {
        private readonly Action _undo;

        public Restore(Action undo) => _undo = undo;

        public void Dispose() => _undo();
    }
}
