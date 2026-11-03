# Imagens de chefes (originais do Gemini, sem processamento)

Gerado no Gemini Flash (modo Imagens), uma conversa por chefe; fases 2 e 3 por edicao da fase anterior.
Padrao de nome: `<chefe>_<parte>_original.jpeg`; `f1/f2/f3` = fases de batalha, `mapa` = sprite parado para o mapa.
Os de batalha ficam virados para a direita e serao ESPELHADOS no processamento (o grupo fica a esquerda). Fundo magenta.
`_conferencia_geral.jpg` = miniaturas de todas as imagens (para achar rapido).

| Pasta | Chefe | Arquivos | Observacoes |
|---|---|---|---|
| fome/ | A Fome | f1, f2, f3, mapa (PNG) | feitos pelo Joao |
| cerbero/ | Cerbero | f1, f2, f3, mapa (16:9) | ok. f3 sentado, as 3 cabecas na frente. mapa = de frente |
| campeao/ | O Campeao | f1, f2, f3, mapa (3:4) | f2 sem escudo/elmo, f3 espada cravada. `mapa` refeito de FRENTE (1792x2400); o antigo, em guarda 3/4, ficou como `campeao_mapa_guarda_descartado.jpeg` (nao usar). O mapa foi gerado so por texto, entao a armadura e parecida com as fases, nao identica |
| sonho/ | O Sonho (Hipno) | f1, f2 (reflexo identico), f3 (real solido + reflexo translucido), mapa (4:3) | fundo magenta mais rosado/gradiente: limpar com cuidado. mapa = Hipno sozinho de frente |
| furias/ | As Furias | alecto, megera, tisifone (batalha, uma imagem cada), mapa_folha (3 de frente, 16:9) | 3 personagens separados |
| sem_nome/ | O Sem-Nome (opcional) | f1, f2 (muitas maos), f3 (parte de baixo do rosto), mapa | fundo levemente gradiente. mapa nao totalmente de frente. f3: conferir se o queixo/boca aparece |

Todos estao em JPEG (formato do download original do Gemini); converter ao processar. Nao renomear.
O processamento (espelhar, limpar magenta, pixelizar 128/160 px, mapa em 3-4 quadros) e feito à parte.

---

## Mapas em 3/4 (decisão de 03/11/2026)

Decisão: os chefes ficam parados no mapa em vista 3/4 (mais presença de chefe). Os mapas de frente
(`*_mapa_original.jpeg`) ficam como reserva. As fases de batalha não mudam.

Todos virados para baixo e para a esquerda (olhando para quem chega). Para a direção oposta, espelhar na hora.
Fundo magenta, JPEG, sem processar.

| Arquivo | Proporção | Observação |
|---|---|---|
| `cerbero/cerbero_mapa_3q_original.jpeg` | 3:4 | Ficou ótimo: três cabeças olhando para baixo-esquerda, cauda atrás. |
| `campeao/campeao_mapa_3q_original.jpeg` | 3:4 | Pose parada, espada baixa, escudo ao lado. Sem névoa. Armadura igual à do mapa de frente. |
| `sonho/sonho_mapa_3q_original.jpeg` | 3:4 | Fundo limpo, sem névoa rosada. Bainha esfiapada em vez de névoa. Papoulas na mão. |
| `furias/furias_mapa_folha_3q_original.jpeg` | 16:9 | Alecto, Megera e Tisífone lado a lado (cortar em 3 no processamento). A tocha da Tisífone saiu na horizontal, com a chama na altura da cintura: conferir/refazer. |
| `sem_nome/sem_nome_mapa_3q_original.jpeg` | 3:4 | Rosto liso mas com traços muito leves de olhos/nariz/boca (não totalmente em branco). Pedras flutuando. Resolver no processamento ou refazer. |

A Fome (feita pelo João) ainda não tem versão 3/4: conferir antes de processar.

### A Fome (`fome/`)

A Fome é o esqueleto do João (caveira, costelas, barriga escura oca, braços longos, trapo marrom, parte de baixo em fumaça cinza).
As imagens originais dele (PNG) foram recolocadas nesta pasta: `fome_f1_original.png`, `fome_f2_original.png`
(`fome_f2_v1.png` é uma variação), `fome_f3_original.png` e `fome_mapa_original.png` (de frente).

| Arquivo | Observação |
|---|---|
| `fome_mapa_3q_original.jpeg` | 3/4 v1: igual ao design do João, mas só a caveira virou; o corpo ficou quase de frente. |
| `fome_mapa_3q_v2_original.jpeg` | 3/4 v2 (**usar esta**): tronco e costelas girados, mesma fumaça e trapo. A caveira olha para a **direita**: espelhar para olhar para baixo-esquerda como os outros. |
| `fome_f1_gaunta_descartado.jpeg` | Tentativa minha com uma figura magra de pele (não esqueleto), com um fantasma rosa e cortada nos joelhos. **Descartada, não usar.** |
