"""Arte do menu do jogo (Diário, Mosaico, Inventário e Mapa): cada aba é um objeto do mundo.

Decisão do João (13/nov/2026, PENDENCIAS 115): tela cheia; o Diário é a tabuinha de cera, o Mapa um pergaminho, o
Mosaico um piso de pedrinhas e o Inventário um pano estendido com as coisas em cima. Tudo provisório, na paleta de
barro e ouro, em pixels da arte (a tela mostra a 3x): fundos de 640x360, ícones de 24x24, espaço de 30x30.

Uso (de dentro de lete-projeto/):
  python tools/menu_arte.py                       gera fundos, espaços, ícones e a névoa do mapa em art/ui/menu/
  python tools/menu_arte.py --miniaturas PASTA    faz as miniaturas do mapa a partir das capturas PASTA/<regiao>.png
"""
import argparse
import os

import numpy as np
from PIL import Image, ImageDraw

SAIDA = os.path.join('art', 'ui', 'menu')
L, A = 640, 360
TOPO = 30  # faixa de cima, onde ficam os nomes das abas

MESA = (23, 18, 16)
BARRO, TERRACOTA, OURO, OURO_ESCURO = (122, 62, 43), (184, 102, 74), (201, 162, 75), (140, 107, 42)
MARFIM, CONTORNO = (233, 223, 199), (18, 14, 16)


def ruido(rng, largura, altura, celulas):
    """Ruído de valor suave (0 a 1), interpolado de uma grade de `celulas` pontos."""
    grade = rng.random((celulas + 2, celulas + 2))
    ys = np.linspace(0, celulas, altura, endpoint=False)
    xs = np.linspace(0, celulas, largura, endpoint=False)
    y0, x0 = ys.astype(int), xs.astype(int)
    fy, fx = (ys - y0)[:, None], (xs - x0)[None, :]
    fy, fx = fy * fy * (3 - 2 * fy), fx * fx * (3 - 2 * fx)
    a, b = grade[y0][:, x0], grade[y0][:, x0 + 1]
    c, d = grade[y0 + 1][:, x0], grade[y0 + 1][:, x0 + 1]
    return (a * (1 - fx) + b * fx) * (1 - fy) + (c * (1 - fx) + d * fx) * fy


def fbm(rng, largura, altura, celulas=4, oitavas=4):
    total, peso, soma = np.zeros((altura, largura)), 1.0, 0.0
    for i in range(oitavas):
        total += ruido(rng, largura, altura, celulas * 2 ** i) * peso
        soma += peso
        peso *= 0.5
    return total / soma


def rampa(cores, valores):
    """Pinta um mapa de valores (0 a 1) com os degraus de `cores` (sem meio-tom: pixel art)."""
    cores = np.array(cores, dtype=float)
    indices = np.clip((valores * len(cores)).astype(int), 0, len(cores) - 1)
    return cores[indices]


def salvar(nome, matriz):
    os.makedirs(SAIDA, exist_ok=True)
    Image.fromarray(np.clip(matriz, 0, 255).astype(np.uint8)).save(os.path.join(SAIDA, nome))
    print(' ', nome)


def mesa(rng):
    """O tampo escuro onde os objetos ficam: tábuas com veio."""
    veio = fbm(rng, L, A, 3, 3) * 0.5 + ruido(rng, L, 1, 200).repeat(A, axis=0) * 0.0
    riscos = ruido(rng, 1, A, 90).repeat(L, axis=1)
    tom = veio * 0.6 + riscos * 0.4
    img = rampa([(17, 13, 12), (21, 16, 14), (25, 19, 16), (29, 22, 18)], tom)
    for y in range(52, A, 62):
        img[y:y + 1] = (12, 9, 9)
    return img


def sombra(img, x0, y0, x1, y1, forca=0.55):
    """Sombra dura, 3 px para baixo e para a direita, do retângulo dado."""
    img[y0 + 3:y1 + 3, x0 + 3:x1 + 3] = img[y0 + 3:y1 + 3, x0 + 3:x1 + 3] * (1 - forca)


def fundo_diario(rng):
    """A tabuinha de cera: dois quadros de madeira com a cera escura no meio, presos por argolas de bronze."""
    img = mesa(rng)
    paineis = ((22, TOPO + 10, 312, A - 14), (328, TOPO + 10, 618, A - 14))
    for x0, y0, x1, y1 in paineis:
        sombra(img, x0, y0, x1, y1)
        veio = ruido(rng, 1, y1 - y0, 60).repeat(x1 - x0, axis=1) * 0.6 + fbm(rng, x1 - x0, y1 - y0, 5, 2) * 0.4
        img[y0:y1, x0:x1] = rampa([(52, 33, 22), (64, 41, 27), (76, 49, 31), (88, 58, 36)], veio)
        img[y0:y0 + 1, x0:x1] = (104, 72, 46)
        img[y1 - 1:y1, x0:x1] = (38, 24, 17)
        cx0, cy0, cx1, cy1 = x0 + 12, y0 + 12, x1 - 12, y1 - 12
        cera = fbm(rng, cx1 - cx0, cy1 - cy0, 6, 3)
        img[cy0:cy1, cx0:cx1] = rampa([(27, 28, 24), (31, 32, 27), (35, 36, 30), (40, 40, 33)], cera)
        img[cy0:cy0 + 2, cx0:cx1] = (16, 15, 14)
        img[cy0:cy1, cx0:cx0 + 2] = (16, 15, 14)
        img[cy1 - 1:cy1, cx0:cx1] = (58, 56, 46)
        # Riscos antigos, apagados, por baixo do que se escreve agora.
        for _ in range(26):
            y = int(rng.integers(cy0 + 8, cy1 - 8))
            x = int(rng.integers(cx0 + 6, cx1 - 60))
            img[y, x:x + int(rng.integers(14, 60))] = (44, 44, 37)
    for y in (TOPO + 60, A // 2 + 14, A - 64):
        img[y:y + 8, 310:330] = OURO_ESCURO
        img[y + 1:y + 7, 312:328] = OURO
        img[y + 3:y + 5, 314:326] = (90, 68, 28)
    return img


def fundo_mapa(rng):
    """O pergaminho: couro claro manchado, com a borda roída e as marcas das dobras."""
    img = mesa(rng)
    x0, y0, x1, y1 = 14, TOPO + 6, L - 14, A - 8
    yy, xx = np.mgrid[0:A, 0:L]
    borda = np.minimum.reduce([xx - x0, x1 - xx, yy - y0, y1 - yy]).astype(float)
    roido = fbm(rng, L, A, 14, 3) * 9
    dentro = borda > roido
    sombra_mask = np.roll(np.roll(dentro, 3, axis=0), 3, axis=1) & ~dentro
    img[sombra_mask] = img[sombra_mask] * 0.45
    mancha = fbm(rng, L, A, 5, 4)
    tom = np.clip(0.62 + (mancha - 0.5) * 0.9 - np.clip(1 - borda / 26, 0, 1) * 0.45, 0, 0.999)
    couro = rampa([(120, 92, 58), (142, 112, 72), (162, 132, 88), (180, 150, 102), (196, 168, 118), (208, 182, 132)], tom)
    img[dentro] = couro[dentro]
    for x in (L // 3, 2 * L // 3):
        img[:, x][dentro[:, x]] = img[:, x][dentro[:, x]] * 0.86
    meio = A // 2 + 12
    img[meio][dentro[meio]] = img[meio][dentro[meio]] * 0.86
    return img


def fundo_mosaico(rng):
    """O piso: pedrinhas miúdas e apagadas, com uma cercadura de meandro; as Lembranças acendem por cima."""
    img = mesa(rng)
    x0, y0, x1, y1 = 16, TOPO + 8, L - 16, A - 10
    passo = 6
    tons = [(34, 36, 40), (40, 42, 46), (46, 47, 50), (52, 51, 52), (44, 40, 38)]
    img[y0:y1, x0:x1] = (20, 20, 22)
    for y in range(y0 + 1, y1 - passo + 1, passo):
        for x in range(x0 + 1, x1 - passo + 1, passo):
            img[y:y + passo - 1, x:x + passo - 1] = tons[int(rng.integers(0, len(tons)))]
    # Cercadura: duas fileiras de pedras de barro com um filete de ouro apagado.
    for margem, cor in ((0, (74, 42, 32)), (passo, (96, 76, 40)), (2 * passo, (74, 42, 32))):
        for y in range(y0 + 1 + margem, y1 - passo - margem + 1, passo):
            for x in range(x0 + 1 + margem, x1 - passo - margem + 1, passo):
                if y in (y0 + 1 + margem, ((y1 - passo - margem - y0 - 1) // passo) * passo + y0 + 1) \
                        or x in (x0 + 1 + margem, ((x1 - passo - margem - x0 - 1) // passo) * passo + x0 + 1):
                    k = 0.8 + 0.4 * rng.random()
                    img[y:y + passo - 1, x:x + passo - 1] = [c * k for c in cor]
    return img


def fundo_inventario(rng):
    """O pano estendido: linho cru sobre a mesa, com a trama, as dobras e a barra desfiada."""
    img = mesa(rng)
    x0, y0, x1, y1 = 18, TOPO + 8, L - 18, A - 10
    sombra(img, x0, y0, x1, y1)
    yy, xx = np.mgrid[y0:y1, x0:x1]
    trama = ((xx + yy) % 2) * 0.06 + ((xx % 4 == 0) | (yy % 4 == 0)) * -0.05
    tom = np.clip(0.5 + (fbm(rng, x1 - x0, y1 - y0, 4, 3) - 0.5) * 0.7 + trama, 0, 0.999)
    img[y0:y1, x0:x1] = rampa([(128, 116, 96), (142, 129, 107), (156, 142, 118), (170, 155, 129), (184, 168, 140)], tom)
    for x in (x0 + (x1 - x0) // 3, x0 + 2 * (x1 - x0) // 3):
        img[y0:y1, x:x + 1] = img[y0:y1, x:x + 1] * 0.8
        img[y0:y1, x + 1:x + 2] = np.clip(img[y0:y1, x + 1:x + 2] * 1.12, 0, 255)
    for x in range(x0, x1, 2):
        fio = int(rng.integers(1, 5))
        img[y1:y1 + fio, x] = (156, 142, 118)
    img[y0:y0 + 1, x0:x1] = (198, 182, 154)
    return img


def espaco(foco):
    """Um espaço de equipamento ou da bolsa: rebaixo escuro com filete de ouro (claro quando em foco)."""
    t = 30
    img = np.zeros((t, t, 4))
    img[1:t - 1, 1:t - 1] = (*CONTORNO, 150)
    filete = OURO if foco else OURO_ESCURO
    img[0, 2:t - 2] = img[t - 1, 2:t - 2] = (*filete, 255)
    img[2:t - 2, 0] = img[2:t - 2, t - 1] = (*filete, 255)
    for y, x in ((1, 1), (1, t - 2), (t - 2, 1), (t - 2, t - 2)):
        img[y, x] = (*filete, 255)
    if foco:
        img[2:t - 2, 2:t - 2, :3] = (44, 36, 30)
        img[2:t - 2, 2:t - 2, 3] = 190
    return img


def nevoa(rng):
    """A névoa que cobre no mapa um lugar vizinho ainda não visitado: uma mancha clara de borda rala."""
    largura, altura = 84, 60
    yy, xx = np.mgrid[0:altura, 0:largura]
    d = ((xx - largura / 2) / (largura / 2)) ** 2 + ((yy - altura / 2) / (altura / 2)) ** 2
    n = fbm(rng, largura, altura, 5, 3)
    alfa = np.clip((1.0 - d) * 1.5 + (n - 0.5) * 1.1, 0, 1)
    alfa = np.floor(alfa * 4) / 4
    tom = rampa([(128, 122, 112), (150, 144, 132), (172, 166, 152), (190, 185, 170)], n)
    return np.dstack([tom, alfa * 235])


# ---- ícones: 24x24, desenhados com as formas simples do Pillow (sem suavização) ----

def tela():
    img = Image.new('RGBA', (24, 24), (0, 0, 0, 0))
    return img, ImageDraw.Draw(img)


def contornar(img):
    """Contorno escuro de 1 px em volta do desenho, para o ícone ler sobre o pano."""
    a = np.array(img)
    cheio = a[..., 3] > 0
    viz = np.zeros_like(cheio)
    for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        viz |= np.roll(np.roll(cheio, dy, axis=0), dx, axis=1)
    a[viz & ~cheio] = (*CONTORNO, 255)
    return a


def icone_remo():
    img, d = tela()
    d.line((5, 19, 16, 8), fill=(120, 84, 52), width=2)
    d.polygon(((15, 9), (20, 2), (22, 4), (17, 11)), fill=(150, 108, 66))
    d.line((17, 8, 20, 4), fill=(96, 66, 40))
    d.rectangle((3, 19, 6, 21), fill=(96, 66, 40))
    return img


def icone_manto():
    img, d = tela()
    d.polygon(((8, 3), (16, 3), (20, 20), (4, 20)), fill=(62, 78, 92))
    d.polygon(((8, 3), (12, 7), (16, 3)), fill=(40, 50, 60))
    for x in (8, 12, 16):
        d.line((x, 9, x - 1 if x < 12 else x + 1 if x > 12 else x, 19), fill=(44, 58, 70))
    d.point((12, 6), fill=OURO)
    return img


def icone_moeda(cor=(126, 150, 120), claro=(168, 190, 158)):
    img, d = tela()
    d.ellipse((5, 5, 18, 18), fill=cor)
    d.ellipse((7, 7, 16, 16), outline=claro)
    d.line((10, 12, 13, 12), fill=claro)
    d.line((12, 10, 12, 14), fill=claro)
    return img


def icone_ramo():
    img, d = tela()
    d.line((6, 20, 16, 5), fill=(96, 72, 44), width=1)
    for (x, y), lado in (((8, 16), -1), ((10, 13), 1), ((12, 10), -1), ((14, 7), 1), ((16, 4), -1)):
        d.ellipse((x - 2 + lado * 2, y - 1, x + 2 + lado * 2, y + 2), fill=OURO)
        d.point((x + lado * 2, y), fill=(236, 204, 120))
    return img


def icone_roma():
    img, d = tela()
    d.ellipse((5, 7, 18, 20), fill=(150, 26, 30))
    d.ellipse((7, 9, 12, 14), fill=(188, 44, 44))
    d.polygon(((9, 7), (10, 3), (12, 6), (14, 3), (15, 7)), fill=(110, 20, 24))
    return img


def icone_tabuinha():
    img, d = tela()
    d.rectangle((5, 3, 18, 20), fill=(88, 58, 36))
    d.rectangle((7, 5, 16, 18), fill=(38, 38, 32))
    for y in (7, 10, 13, 16):
        d.line((8, y, 15, y), fill=(70, 68, 56))
    d.line((8, 8, 15, 15), fill=(96, 92, 76))
    return img


def icone_exvoto():
    img, d = tela()
    d.polygon(((6, 4), (17, 4), (17, 14), (12, 20), (6, 14)), fill=(150, 112, 56))
    d.polygon(((8, 6), (15, 6), (15, 13), (12, 17), (8, 13)), fill=OURO)
    d.line((12, 7, 12, 14), fill=(110, 80, 36))
    d.line((9, 10, 14, 10), fill=(110, 80, 36))
    return img


def icone_amuleto():
    img, d = tela()
    d.arc((6, 1, 17, 14), 200, 340, fill=(120, 96, 60))
    d.ellipse((8, 11, 15, 19), fill=(78, 127, 134))
    d.ellipse((10, 13, 12, 15), fill=(150, 196, 200))
    return img


def icone_instrumento():
    img, d = tela()
    d.line((6, 19, 17, 5), fill=(120, 84, 52), width=2)
    d.ellipse((14, 2, 20, 8), fill=(150, 150, 156))
    return img


def icone_veste():
    img, d = tela()
    d.polygon(((7, 4), (16, 4), (20, 9), (17, 11), (17, 20), (6, 20), (6, 11), (3, 9)), fill=(150, 132, 104))
    d.line((11, 4, 11, 20), fill=(118, 102, 78))
    return img


def icone_utensilio():
    img, d = tela()
    d.rectangle((6, 9, 17, 19), fill=(112, 78, 50))
    d.arc((7, 3, 16, 14), 180, 360, fill=(84, 58, 38))
    d.rectangle((6, 12, 17, 13), fill=(84, 58, 38))
    return img


def icone_reliquia():
    img, d = tela()
    d.rectangle((5, 9, 18, 19), fill=(96, 70, 44))
    d.polygon(((5, 9), (11, 4), (12, 4), (18, 9)), fill=(122, 92, 58))
    d.rectangle((10, 12, 13, 15), fill=OURO)
    return img


def tigela(cor_dentro, brilho):
    img, d = tela()
    d.pieslice((3, 4, 20, 22), 0, 180, fill=(150, 96, 66))
    d.ellipse((3, 10, 20, 15), fill=(184, 122, 86))
    d.ellipse((5, 11, 18, 14), fill=cor_dentro)
    d.line((8, 12, 11, 12), fill=brilho)
    d.rectangle((9, 21, 14, 22), fill=(122, 74, 52))
    return img


def icone_mel():
    img, d = tela()
    d.rectangle((7, 8, 16, 20), fill=(196, 140, 44))
    d.rectangle((6, 5, 17, 8), fill=(150, 96, 66))
    d.rectangle((9, 3, 14, 5), fill=(122, 74, 52))
    d.line((9, 10, 9, 17), fill=(236, 190, 90))
    return img


def icone_farinha():
    img, d = tela()
    d.polygon(((6, 8), (17, 8), (19, 20), (4, 20)), fill=(196, 186, 160))
    d.polygon(((9, 8), (11, 3), (13, 3), (14, 8)), fill=(170, 158, 132))
    d.line((8, 8, 15, 8), fill=(120, 96, 60))
    d.line((8, 12, 10, 18), fill=(222, 214, 190))
    return img


def icone_pao():
    img, d = tela()
    d.ellipse((3, 8, 20, 20), fill=(162, 112, 60))
    d.ellipse((5, 9, 18, 15), fill=(196, 146, 84))
    for x in (8, 12, 16):
        d.line((x, 10, x - 2, 14), fill=(132, 88, 46))
    return img


def icone_sementes():
    img, d = tela()
    for x, y in ((8, 9), (13, 8), (10, 14), (15, 13), (7, 17), (13, 18)):
        d.ellipse((x - 2, y - 2, x + 2, y + 2), fill=(158, 30, 34))
        d.point((x - 1, y - 1), fill=(210, 80, 76))
    return img


ICONES = {
    'remo_do_afogado': icone_remo, 'manto_do_afogado': icone_manto, 'moeda_do_afogado': icone_moeda,
    'ramo_dourado': icone_ramo, 'roma_de_persefone': icone_roma, 'tabuinha_rasurada': icone_tabuinha,
    'exvoto_do_campeao': icone_exvoto,
    # O que não tem desenho próprio usa o da categoria.
    '_instrument': icone_instrumento, '_garment': icone_veste, '_amulet': icone_amuleto, '_votive': icone_exvoto,
    '_utensil': icone_utensilio, '_relic': icone_reliquia,
    'oferenda_mel': icone_mel, 'oferenda_farinha': icone_farinha,
    'oferenda_agua': lambda: tigela((78, 127, 134), (150, 196, 200)),
    'oferenda_leite': lambda: tigela((226, 220, 204), (255, 255, 255)),
    'oferenda_vinho': lambda: tigela((84, 44, 70), (130, 80, 110)),
    'oferenda_sangue': lambda: tigela((70, 22, 26), (110, 40, 44)),
    '_oferenda': lambda: tigela((120, 110, 96), (170, 160, 140)),
    'provisao_pao': icone_pao, 'provisao_sementes_roma': icone_sementes, '_provisao': icone_pao,
    'obolo': lambda: icone_moeda(OURO_ESCURO, OURO),
}


def miniaturas(pasta):
    """Recorta o meio de cada captura (sem o texto de baixo), reduz para 64x40 e limita as cores."""
    destino = os.path.join('art', 'ui', 'menu', 'mapa')
    os.makedirs(destino, exist_ok=True)
    for arquivo in sorted(os.listdir(pasta)):
        if not arquivo.endswith('.png'):
            continue
        img = Image.open(os.path.join(pasta, arquivo)).convert('RGB')
        largura, altura = img.size
        corte = img.crop((int(largura * 0.19), int(altura * 0.1), int(largura * 0.81), int(altura * 0.79)))
        # As regiões são escuras; no pergaminho claro a miniatura precisa de mais luz para se ler.
        claro = Image.eval(corte.resize((64, 40), Image.BOX), lambda v: min(255, int(v * 1.7 + 10)))
        mini = claro.quantize(28, dither=Image.Dither.NONE).convert('RGB')
        mini.save(os.path.join(destino, arquivo))
        print(' ', os.path.join('mapa', arquivo))


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    p.add_argument('--miniaturas', metavar='PASTA', help='faz só as miniaturas do mapa, das capturas desta pasta')
    args = p.parse_args()
    if args.miniaturas:
        miniaturas(args.miniaturas)
    else:
        for i, (nome, faz) in enumerate((('fundo_diario.png', fundo_diario), ('fundo_mapa.png', fundo_mapa),
                                         ('fundo_mosaico.png', fundo_mosaico), ('fundo_inventario.png', fundo_inventario))):
            salvar(nome, faz(np.random.default_rng(700 + i)))
        salvar('espaco.png', espaco(False))
        salvar('espaco_foco.png', espaco(True))
        salvar('mapa_nevoa.png', nevoa(np.random.default_rng(710)))
        os.makedirs(os.path.join(SAIDA, 'itens'), exist_ok=True)
        for nome, faz in ICONES.items():
            salvar(os.path.join('itens', f'{nome}.png'), contornar(faz()))
