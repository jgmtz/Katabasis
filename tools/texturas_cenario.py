#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Texturas em pixel art do cenário (paisagem HD-2D, PENDENCIAS 105), geradas por conta: pedra, laje, terra, campo,
reboco, madeira, telha, mármore, casca, folhagem, pano, e as folhas de sprites da flora (asfódelo, capim) e o ruído da
névoa e da água. Todas repetem sem emenda. São provisórias: uma textura feita no Gemini com o mesmo nome de arquivo
substitui a daqui (não rode de novo por cima sem querer: use --so para refazer uma só).

Uso (de dentro de lete-projeto/): python tools/texturas_cenario.py [--so laje muro ...]
Saída: art/tiles/cenario/<nome>.png
"""
import argparse
import os

import numpy as np
from PIL import Image

OUT = os.path.join('art', 'tiles', 'cenario')


def rgb(hex_color):
    hex_color = hex_color.lstrip('#')
    return np.array([int(hex_color[i:i + 2], 16) for i in (0, 2, 4)], dtype=float)


def ramp(colors):
    """Paleta em degraus (do escuro ao claro): o valor 0..1 escolhe um degrau, sem tons intermediários."""
    table = np.array([rgb(c) for c in colors])

    def pick(values):
        idx = np.clip((values * len(table)).astype(int), 0, len(table) - 1)
        return table[idx]

    return pick


def noise(rng, size, cells):
    """Ruído de valor que repete sem emenda: uma grade de 'cells' pontos, interpolada com suavização."""
    grid = rng.random((cells, cells))
    t = np.arange(size) * cells / size
    i0 = np.floor(t).astype(int)
    f = t - i0
    f = f * f * (3 - 2 * f)
    i1 = (i0 + 1) % cells
    rows = grid[i0][:, i0] * (1 - f)[None, :] + grid[i0][:, i1] * f[None, :]
    rows1 = grid[i1][:, i0] * (1 - f)[None, :] + grid[i1][:, i1] * f[None, :]
    return rows * (1 - f)[:, None] + rows1 * f[:, None]


def fbm(rng, size, cells=4, octaves=3):
    total, amp, norm = np.zeros((size, size)), 1.0, 0.0
    for o in range(octaves):
        total += noise(rng, size, cells * 2 ** o) * amp
        norm += amp
        amp *= 0.5
    return total / norm


def voronoi(rng, size, count):
    """Células irregulares que repetem: devolve (índice da célula, distância à borda da célula)."""
    pts = rng.random((count, 2)) * size
    ys, xs = np.mgrid[0:size, 0:size]
    best = np.full((size, size), 1e9)
    second = np.full((size, size), 1e9)
    index = np.zeros((size, size), dtype=int)
    for i, (px, py) in enumerate(pts):
        dx = np.abs(xs - px)
        dx = np.minimum(dx, size - dx)
        dy = np.abs(ys - py)
        dy = np.minimum(dy, size - dy)
        d = np.hypot(dx, dy * 1.15)
        closer = d < best
        second = np.where(closer, best, np.minimum(second, d))
        index = np.where(closer, i, index)
        best = np.where(closer, d, best)
    return index, second - best


def save(name, array, alpha=None):
    os.makedirs(OUT, exist_ok=True)
    data = np.clip(array, 0, 255).astype(np.uint8)
    if alpha is not None:
        data = np.dstack([data, np.clip(alpha, 0, 255).astype(np.uint8)])
    Image.fromarray(data).save(os.path.join(OUT, f'{name}.png'))
    print(f'  {name}.png {data.shape[1]}x{data.shape[0]}')


# ---- superfícies ----

def laje(rng):
    """Piso de lajes irregulares, rejunte escuro, lajes com tons diferentes e um desgaste leve."""
    s = 64
    index, edge = voronoi(rng, s, 22)
    tone = rng.random(22)[index] * 0.45 + fbm(rng, s, 8, 2) * 0.3 + 0.18
    tone = np.where(edge < 1.6, 0.02, tone)
    tone = np.where((edge >= 1.6) & (edge < 2.6), tone * 0.72, tone)
    save('laje', ramp(['#17161b', '#2b2a30', '#3a393d', '#48474a', '#575552', '#66635c'])(tone))


def terra(rng):
    s = 64
    base = fbm(rng, s, 4, 4)
    stones = (rng.random((s, s)) > 0.985).astype(float)
    tone = base * 0.75 + 0.1 + stones * 0.4
    save('terra', ramp(['#1f1a18', '#2a2320', '#352c27', '#40352d', '#4c3f34', '#6a5f52'])(tone))


def campo(rng):
    """Chão do campo: terra escura com manchas de capim seco, em riscos verticais curtos."""
    s = 64
    base = fbm(rng, s, 4, 3)
    blades = np.zeros((s, s))
    for _ in range(420):
        x, y, h = rng.integers(0, s), rng.integers(0, s), rng.integers(1, 4)
        for k in range(h):
            blades[(y - k) % s, x] = 0.25 + 0.1 * k
    tone = base * 0.55 + 0.12 + blades
    save('campo', ramp(['#191c1a', '#222622', '#2b302a', '#353a31', '#414637', '#525642', '#676a50'])(tone))


def muro(rng):
    """Muro de blocos de pedra em fiadas desencontradas, com blocos de tamanhos variados."""
    s = 64
    tone = np.zeros((s, s))
    row_h = 8
    for r in range(s // row_h):
        x = int(rng.integers(0, 8))
        end = x + s
        while x < end:
            w = int(rng.integers(8, 17))
            w = min(w, end - x)
            shade = 0.28 + rng.random() * 0.4
            for yy in range(row_h):
                for xx in range(w):
                    border = yy == 0 or xx == 0
                    low = yy == row_h - 1 or xx == w - 1
                    v = 0.04 if border else shade * (0.8 if low else 1.0)
                    tone[r * row_h + yy, (x + xx) % s] = v
            x += w
    tone += (fbm(rng, s, 8, 3) - 0.5) * 0.22
    tone = np.where(tone < 0.08, 0.02, tone)
    save('muro', ramp(['#151419', '#26252b', '#33323a', '#414046', '#504e52', '#5f5c5b', '#6f6b66'])(tone))


def reboco(rng):
    """Reboco de barro: liso, manchado, com rachaduras e a base mais suja."""
    s = 64
    base = fbm(rng, s, 3, 4)
    tone = base * 0.5 + 0.32
    for _ in range(5):
        x, y = rng.integers(0, s), rng.integers(0, s)
        for _ in range(int(rng.integers(8, 22))):
            tone[y % s, x % s] = 0.05
            x += int(rng.integers(-1, 2))
            y += 1
    save('reboco', ramp(['#2a2220', '#4a3c34', '#5b4a3f', '#6a574a', '#786456', '#877262', '#94806e'])(tone))


def madeira(rng):
    s = 32
    tone = np.zeros((s, s))
    for p in range(4):
        shade = 0.3 + rng.random() * 0.3
        grain = noise(rng, s, 4)[:, :8] * 0.25
        for xx in range(8):
            tone[:, p * 8 + xx] = shade + grain[:, xx] + (rng.random(s) > 0.9) * -0.12
        tone[:, p * 8] = 0.04
    for _ in range(3):
        tone[int(rng.integers(0, s)), :] *= 0.55
    save('madeira', ramp(['#1b1512', '#2f241d', '#3d2f25', '#4a392c', '#584434', '#66503d'])(tone))


def telha(rng):
    """Telhas de barro em fiadas, cada telha com a ponta arredondada e a sombra da de cima."""
    s = 32
    tone = np.zeros((s, s))
    for r in range(4):
        off = 4 if r % 2 else 0
        for c in range(4):
            shade = 0.45 + rng.random() * 0.35
            for yy in range(8):
                for xx in range(8):
                    v = shade * (0.55 if yy < 2 else 1.0)
                    if xx in (0, 7) or (yy == 7 and xx in (1, 6)):
                        v = 0.06
                    tone[r * 8 + yy, (off + c * 8 + xx) % s] = v
    tone += (noise(rng, s, 8) - 0.5) * 0.15
    save('telha', ramp(['#231512', '#4a251c', '#6b3424', '#84422c', '#9a5034', '#ad5f3f'])(tone))


def pedra(rng):
    """Rocha bruta, para rochedos e paredões: facetas grandes e veios."""
    s = 64
    index, edge = voronoi(rng, s, 9)
    tone = rng.random(9)[index] * 0.3 + fbm(rng, s, 4, 4) * 0.5 + 0.08
    tone = np.where(edge < 1.2, tone * 0.5, tone)
    save('pedra', ramp(['#131217', '#1e1d23', '#2a292f', '#36353a', '#434146', '#514e50'])(tone))


def marmore(rng):
    s = 32
    base = fbm(rng, s, 4, 3)
    veins = np.abs(fbm(rng, s, 4, 2) - 0.5) < 0.035
    tone = base * 0.4 + 0.45 - veins * 0.3
    save('marmore', ramp(['#3c3a3c', '#5c5a58', '#77746f', '#8f8b83', '#a39e93', '#b5afa2'])(tone))


def casca(rng):
    s = 32
    tone = np.tile((noise(rng, s, 8)[0:1, :] * 0.5 + 0.2), (s, 1)) + (fbm(rng, s, 8, 2) - 0.5) * 0.35
    save('casca', ramp(['#141110', '#211b18', '#2d2520', '#3a3029', '#463a31'])(tone))


def folhagem(rng):
    """Folhagem escura do cipreste: escamas pequenas viradas para baixo."""
    s = 32
    tone = fbm(rng, s, 8, 2) * 0.45 + 0.1
    for y in range(0, s, 4):
        for x in range(0, s, 4):
            ox = (2 if (y // 4) % 2 else 0)
            tone[(y + 3) % s, (x + ox) % s] += 0.3
            tone[(y + 2) % s, (x + ox + 1) % s] += 0.18
    save('folhagem', ramp(['#0e1412', '#141d19', '#1b2721', '#22322a', '#2b3f33', '#365040'])(tone))


def pano(rng):
    """Pano de toldo: trama fina com listras; a cor vem da tinta da peça (a textura é clara)."""
    s = 32
    tone = np.full((s, s), 0.72)
    tone[:, (np.arange(s) // 4) % 4 == 0] = 0.45
    tone += (rng.random((s, s)) - 0.5) * 0.12
    tone[::2, :] -= 0.05
    save('pano', ramp(['#3a3632', '#5d5750', '#7d766c', '#9a9286', '#b4ab9d', '#cbc2b2'])(tone))


def palha(rng):
    s = 32
    tone = np.tile(rng.random((1, s)) * 0.5 + 0.25, (s, 1)) + (rng.random((s, s)) - 0.5) * 0.25
    tone[::8, :] *= 0.5
    save('palha', ramp(['#2a2418', '#453b25', '#5c4f30', '#73633b', '#8a7746', '#a08b53'])(tone))


def ruido(rng):
    """Ruído macio em tons de cinza (névoa, água, chama): não é pixel art, é só o desenho do movimento."""
    s = 64
    n = fbm(rng, s, 4, 4)
    n = (n - n.min()) / (n.max() - n.min())
    save('ruido', np.dstack([n * 255] * 3))


# ---- folhas de sprites (fundo transparente) ----

def asfodelo(rng):
    """Asfódelos: 4 variantes de 16x40 lado a lado; haste fina, folhas na base e a espiga de flores pálidas."""
    w, h, n = 16, 40, 4
    img = np.zeros((h, w * n, 3))
    alpha = np.zeros((h, w * n))
    stem, leaf, leaf2 = rgb('#3a4a3c'), rgb('#2c3a30'), rgb('#44574a')
    petal, petal2, core = rgb('#d9cbb0'), rgb('#b7a98e'), rgb('#c8a24a')

    def put(v, x, y, c):
        if 0 <= x < w and 0 <= y < h:
            img[y, v * w + x] = c
            alpha[y, v * w + x] = 255

    for v in range(n):
        height = int(rng.integers(26, 37))
        cx = 8 + int(rng.integers(-1, 2))
        lean = rng.choice([-1, 0, 0, 1])
        top = h - height
        for y in range(h - 1, top - 1, -1):
            x = cx + (lean if y < h - height * 0.55 else 0)
            put(v, x, y, stem)
        for side in (-1, 1):
            for k in range(int(rng.integers(5, 9))):
                put(v, cx + side * (1 + k // 2), h - 1 - k, leaf if k % 2 else leaf2)
        flowers = int(rng.integers(4, 8))
        for k in range(flowers):
            y = top + 1 + k * 2 + int(rng.integers(0, 2))
            side = -1 if k % 2 else 1
            x = cx + lean + side * int(rng.integers(1, 3))
            for dx, dy, c in ((0, 0, core), (-1, 0, petal), (1, 0, petal), (0, -1, petal), (0, 1, petal2)):
                put(v, x + dx, y + dy, c)
        put(v, cx + lean, top, petal)
        put(v, cx + lean, top - 1, petal2)
    save('asfodelo', img, alpha)


def capim(rng):
    """Tufos de capim seco: 4 variantes de 16x16."""
    w, h, n = 16, 16, 4
    img = np.zeros((h, w * n, 3))
    alpha = np.zeros((h, w * n))
    tones = [rgb(c) for c in ('#3a3f31', '#4c5040', '#5f6249', '#77765a')]
    for v in range(n):
        for _ in range(int(rng.integers(7, 12))):
            x = int(rng.integers(3, 13))
            height = int(rng.integers(4, 13))
            lean = rng.choice([-1, 0, 1])
            c = tones[int(rng.integers(0, len(tones)))]
            for k in range(height):
                xx = x + (lean * k) // 5
                if 0 <= xx < w:
                    img[h - 1 - k, v * w + xx] = c if k < height - 1 else tones[-1]
                    alpha[h - 1 - k, v * w + xx] = 255
    save('capim', img, alpha)


def papoula(rng):
    """Papoulas do sono: 4 variantes de 16x24; haste curva, folhas baixas e a flor pálida (aberta ou em botão)."""
    w, h, n = 16, 24, 4
    img = np.zeros((h, w * n, 3))
    alpha = np.zeros((h, w * n))
    stem, leaf = rgb('#3c4a44'), rgb('#2e3a36')
    petal, petal2, core = rgb('#e2d8ea'), rgb('#b3a3c6'), rgb('#2a2230')

    def put(v, x, y, c):
        if 0 <= x < w and 0 <= y < h:
            img[y, v * w + x] = c
            alpha[y, v * w + x] = 255

    for v in range(n):
        height = int(rng.integers(13, 20))
        cx = 8 + int(rng.integers(-1, 2))
        lean = int(rng.choice([-1, 1]))
        top = h - height
        for y in range(h - 1, top - 1, -1):
            put(v, cx + (lean if y < h - height * 0.6 else 0), y, stem)
        for side in (-1, 1):
            for k in range(int(rng.integers(3, 6))):
                put(v, cx + side * (1 + k // 2), h - 1 - k, leaf)
        fx = cx + lean
        if v % 2 == 0:
            # Aberta: uma taça de pétalas de 7 px, com o miolo escuro.
            for dy, half in ((-3, 1), (-2, 2), (-1, 3), (0, 3), (1, 2)):
                for dx in range(-half, half + 1):
                    put(v, fx + dx, top + dy, petal2 if dy == 1 or abs(dx) == half else petal)
            put(v, fx, top - 1, core)
            put(v, fx, top, core)
        else:
            # Em botão: pendente, virado para o lado.
            for dx, dy, c in ((0, 0, petal2), (lean, 0, petal2), (lean, 1, petal), (lean * 2, 1, petal), (lean, 2, petal),
                              (lean * 2, 2, petal), (lean, 3, petal2), (lean * 2, 3, petal2)):
                put(v, fx + dx, top + dy, c)
    save('papoula', img, alpha)


TEXTURES = {f.__name__: f for f in (laje, terra, campo, muro, reboco, madeira, telha, pedra, marmore, casca, folhagem,
                                    pano, palha, ruido, asfodelo, capim, papoula)}

if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    p.add_argument('--so', nargs='+', choices=sorted(TEXTURES), help='refaz só estas')
    args = p.parse_args()
    for i, (name, make) in enumerate(TEXTURES.items()):
        if args.so and name not in args.so:
            continue
        make(np.random.default_rng(1000 + i))
