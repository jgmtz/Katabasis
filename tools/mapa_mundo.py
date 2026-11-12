"""Desenha o mapa esquemático do mundo: as regiões, o que há em cada uma e por onde se passa de uma a outra.

Não é o mapa do jogo (o jogo não tem mapa: o Diário é o único guia); é uma folha de consulta para o desenvolvimento.
As ligações seguem as passagens de tools/blockout_regioes.py.

Uso (de dentro de lete-projeto/): python tools/mapa_mundo.py   (gera docs/MAPA_MUNDO.png)
"""
import os
import textwrap

from PIL import Image, ImageDraw, ImageFont

W, H = 1900, 2420
FUNDO, TINTA, FRACA = (24, 22, 26), (232, 224, 208), (150, 144, 136)
ATOS = {
    1: ((58, 74, 86), 'Ato I · a descida'),
    2: ((70, 84, 62), 'Ato II · os três caminhos'),
    3: ((84, 66, 96), 'Ato III · o Estige e o Palácio'),
    4: ((112, 62, 46), 'Ato IV · o fundo'),
}


def fonte(tamanho, negrito=False):
    for nome in (('arialbd.ttf', 'DejaVuSans-Bold.ttf') if negrito else ('arial.ttf', 'DejaVuSans.ttf')):
        try:
            return ImageFont.truetype(nome, tamanho)
        except OSError:
            continue
    return ImageFont.load_default()


F_TITULO, F_NOME, F_TEXTO, F_ROTULO = fonte(46, True), fonte(26, True), fonte(17), fonte(16)

# id: (nome, ato, centro x, centro y, largura, altura, o que há lá)
REGIOES = {
    'tenaro': ('Cabo Tênaro', 1, 520, 2290, 400, 130, 'Boca da gruta, corredor e câmara do rio. 1ª Trívia (Hécate), o Remador, a corda (atalho da câmara à boca).'),
    'vestibulo': ('Vestíbulo', 1, 520, 2090, 400, 120, 'O olmo dos sonhos vãos, o Medo, a Doença, a Guerra. No estreito, A Fome (chefe).'),
    'aqueronte': ('Margem do Aqueronte', 1, 520, 1890, 400, 120, 'Caronte e a barca, os insepultos, o afogado, a esposa do Padeiro. O Convite (1ª Pesada).'),
    'asfodelos': ('Asfódelos', 1, 760, 1650, 1080, 170, 'Poço das Sem-Nome (oeste) · Rua dos Ofícios: Padeiro, Coveiro, Dimas · a grande Trívia, no centro, diante da muralha norte · Altar das Oferendas: Cálias · Cérbero na passagem · Planície, aldeia e mercado: Mercador e Lia (leste).'),
    'luto': ('Campos do Luto', 2, 250, 1390, 300, 130, 'Bosque de murtas. Ismena. As lágrimas deixam a luta mais lenta.'),
    'tribunal': ('Tribunal', 2, 590, 1390, 300, 130, 'Os três juízes, a pedra do julgamento. Septímia. O Plano (2ª Pesada).'),
    'guerreiros': ('Campo dos Guerreiros', 2, 930, 1390, 300, 130, 'Armas fincadas, a arena do Campeão (duelo de honra). Aulo.'),
    'estige': ('Estige', 3, 1360, 1390, 340, 130, 'A deusa do rio (só a voz) oferece os juramentos. O cais da travessia.'),
    'palacio': ('Palácio', 3, 1360, 1170, 340, 130, 'Hades e Perséfone nos tronos, a mesa do pacto, o jardim com a romãzeira e Ascálafo. A Venda (3ª Pesada).'),
    'hipno': ('Morada de Hipno', 3, 1360, 950, 340, 130, 'Campo de papoulas. Hipno (voz), Tânato, o Sonho (chefe). O Olhar (4ª Pesada).'),
    'mnemosine': ('Mnemósine', 3, 1730, 950, 280, 130, 'Secreta. Duas fontes iguais: a do cipreste branco e a dos guardiões.'),
    'bifurcacao': ('Bifurcação', 4, 1360, 730, 300, 120, 'A encruzilhada: fogo a oeste, luz dourada a leste, névoa ao norte.'),
    'tartaro': ('Tártaro', 4, 990, 730, 300, 130, 'O Flegetonte (rio de fogo), Sísifo, Tântalo, as Danaides. As Fúrias (chefe).'),
    'elisios': ('Campos Elísios', 4, 1730, 730, 280, 130, 'Anquises e o rio das almas (ficar é um fim). A Mentira (5ª Pesada).'),
    'vale_lete': ('Vale do Lete', 4, 1360, 520, 360, 130, 'O rio do esquecimento, a Cadeira (O Gole, 6ª Pesada), a pessoa do Acerto. A Sombra do Esquecido (chefe final).'),
    'portoes': ('Os Portões', 4, 1360, 310, 360, 120, 'Dois portões iguais: o fosco e o que brilha. A Pergunta do companheiro. O final.'),
}

# (de, para, rótulo, lado do rótulo: deslocamento x, y em relação ao meio da linha, âncora)
LIGACOES = [
    ('tenaro', 'vestibulo', 'passagem no fim da gruta', 16, 0, 'lm'),
    ('vestibulo', 'aqueronte', 'pelo estreito, depois da Fome', 16, 0, 'lm'),
    ('aqueronte', 'asfodelos', 'a barca de Caronte: pede a chave do Aqueronte\n(óbolo, ajudar os insepultos ou origem Pescador)\n1º marco da Maré', 16, 0, 'lm'),
    ('asfodelos', 'luto', '', 0, 0, 'mm'),
    ('asfodelos', 'tribunal', 'três portais da muralha norte:\nabrem com a Maré no degrau 2\n(escolher o companheiro na Trívia)', -190, 6, 'mm'),
    ('asfodelos', 'guerreiros', '', 0, 0, 'mm'),
    ('asfodelos', 'estige', '4º portal: abre depois de\nvisitar os três caminhos', 14, 6, 'lm'),
    ('estige', 'palacio', 'a travessia: pede um juramento · 3º marco\n(a volta do Palácio cai na margem de cá,\ne cruzar de novo leva outra vez ao Palácio)', 16, 0, 'lm'),
    ('palacio', 'hipno', 'escada a leste: pede o caminho da descida\n(favor de Hades, o jardim ou a escada dos jardineiros)', 16, 0, 'lm'),
    ('hipno', 'mnemosine', 'som de\nágua', 0, 16, 'ma'),
    ('hipno', 'bifurcacao', 'arco ao norte: abre depois do Sonho · 4º marco', 16, 0, 'lm'),
    ('bifurcacao', 'tartaro', 'oeste', 0, -14, 'mb'),
    ('bifurcacao', 'elisios', 'leste', 0, -14, 'mb'),
    ('bifurcacao', 'vale_lete', 'norte: pede o Tártaro e os Elísios vistos\ne a Chave de Memória · 5º marco', 16, 0, 'lm'),
    ('vale_lete', 'portoes', 'norte: abre depois da Sombra do Esquecido', 16, 0, 'lm'),
]


def borda(regiao, alvo):
    """O ponto da borda da caixa de `regiao` voltado para `alvo`."""
    _, _, x, y, w, h, _ = REGIOES[regiao]
    _, _, ax, ay, aw, ah, _ = REGIOES[alvo]
    if abs(ay - y) > (h + ah) / 2 - 1:
        # Uma acima da outra: sai por cima ou por baixo, no x mais próximo do alvo que ainda cabe na caixa.
        px = min(max(ax, x - w / 2 + 30), x + w / 2 - 30)
        return px, (y - h / 2 if ay < y else y + h / 2)
    return (x - w / 2 if ax < x else x + w / 2), y


def main():
    img = Image.new('RGB', (W, H), FUNDO)
    d = ImageDraw.Draw(img)
    d.text((60, 50), 'Katábasis · mapa das regiões', font=F_TITULO, fill=TINTA)
    d.multiline_text((62, 116), 'Folha de consulta do desenvolvimento (o jogo não mostra mapa: o Diário é o único guia).\n'
                     'Lê-se de baixo para cima: a descida começa em Cabo Tênaro e termina nos Portões.\n'
                     'Toda passagem volta pelo mesmo caminho; em cada região há uma Trívia (descansar, salvar e viajar pela tocha).',
                     font=F_TEXTO, fill=FRACA, spacing=6)
    for i, (cor, nome) in enumerate(ATOS.values()):
        d.rounded_rectangle((62, 230 + i * 44, 98, 260 + i * 44), 6, fill=cor, outline=TINTA)
        d.text((112, 245 + i * 44), nome, font=F_TEXTO, fill=TINTA, anchor='lm')

    for de, para, rotulo, dx, dy, ancora in LIGACOES:
        a, b = borda(de, para), borda(para, de)
        d.line((a, b), fill=TINTA, width=3)
        for p in (a, b):
            d.ellipse((p[0] - 5, p[1] - 5, p[0] + 5, p[1] + 5), fill=TINTA)
        if rotulo:
            mx, my = (a[0] + b[0]) / 2 + dx, (a[1] + b[1]) / 2 + dy
            caixa = d.multiline_textbbox((mx, my), rotulo, font=F_ROTULO, anchor=ancora, spacing=3, align='left' if ancora[0] == 'l' else 'center')
            d.rectangle((caixa[0] - 4, caixa[1] - 3, caixa[2] + 4, caixa[3] + 3), fill=FUNDO)
            d.multiline_text((mx, my), rotulo, font=F_ROTULO, fill=FRACA, anchor=ancora, spacing=3,
                             align='left' if ancora[0] == 'l' else 'center')

    for nome, ato, x, y, w, h, texto in REGIOES.values():
        d.rounded_rectangle((x - w / 2, y - h / 2, x + w / 2, y + h / 2), 14, fill=ATOS[ato][0], outline=TINTA, width=2)
        d.text((x, y - h / 2 + 12), nome, font=F_NOME, fill=TINTA, anchor='mt')
        linhas = textwrap.wrap(texto, width=max(20, int(w / 8.6)))
        d.multiline_text((x, y - h / 2 + 48), '\n'.join(linhas), font=F_TEXTO, fill=TINTA, anchor='ma', align='center', spacing=3)

    saida = os.path.join('docs', 'MAPA_MUNDO.png')
    img.save(saida)
    print(saida)


if __name__ == '__main__':
    main()
