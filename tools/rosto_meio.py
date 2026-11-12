#!/usr/bin/env python3
"""Monta o rosto meio coberto da regra C (decisão do João, 11/nov/2026: Pesadas encaradas e deixadas empatadas) a partir
das duas imagens que já existem no inventário: a metade esquerda da cabeça vem do rosto devolvido e a direita, da romã
do rosto coberto. O corpo é o do rosto devolvido.

Uso (de dentro de lete-projeto/): python tools/rosto_meio.py art/ui/inventario/sombra_f [--cabeca 0.24]
"""
import argparse
import os

from PIL import Image


def montar(pasta, fracao_cabeca):
    devolvido = Image.open(os.path.join(pasta, 'rosto_devolvido.png')).convert('RGBA')
    coberto = Image.open(os.path.join(pasta, 'rosto_coberto.png')).convert('RGBA')
    largura = max(devolvido.width, coberto.width)
    altura = max(devolvido.height, coberto.height)

    def centrada(img):
        tela = Image.new('RGBA', (largura, altura), (0, 0, 0, 0))
        tela.paste(img, ((largura - img.width) // 2, altura - img.height))
        return tela

    a, b = centrada(devolvido), centrada(coberto)
    # O meio da cabeça: o centro das colunas com pixel nas linhas da cabeça do rosto devolvido.
    fim_cabeca = int(altura * fracao_cabeca)
    colunas = [x for x in range(largura) for y in range(fim_cabeca) if a.getpixel((x, y))[3] > 0]
    meio = (min(colunas) + max(colunas) + 1) // 2 if colunas else largura // 2
    saida = a.copy()
    for y in range(fim_cabeca):
        for x in range(largura):
            if x >= meio:
                saida.putpixel((x, y), b.getpixel((x, y)))
    destino = os.path.join(pasta, 'rosto_meio.png')
    saida.save(destino)
    print(f'salvo: {destino} (cabeça até a linha {fim_cabeca}, corte na coluna {meio})')


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    p.add_argument('pastas', nargs='+', help='pastas do inventário (com rosto_devolvido.png e rosto_coberto.png)')
    p.add_argument('--cabeca', type=float, default=0.24, help='fração da altura que é a cabeça (padrão: 0.24)')
    args = p.parse_args()
    for pasta in args.pastas:
        montar(pasta, args.cabeca)
