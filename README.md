# Katábasis: O Rio do Esquecimento

RPG de exploração e escolhas na mitologia greco-romana. Um vivo desce ao mundo dos mortos e não lembra por quê. Explorar o Além é recuperar memórias, e as escolhas decidem o que ele leva consigo e se consegue sair.

> Em desenvolvimento. Título provisório; o nome interno do projeto é **Lete**.

## O jogo

- **Combate por turnos feito de gestos.** Toda ação é um pequeno gesto de atenção (Anel, Esquiva, Segurar, Arrastar e Traço). Uma luta não termina só pela força: dá para escutar a sombra até saber o nome dela e libertá-la, absorvê-la, presenciar, poupar ou fugir.
- **Memórias no lugar de XP.** As Lembranças formam um Mosaico romano que cresce conforme o protagonista lembra. Elas viram habilidades na luta e abrem caminhos na exploração.
- **Escolhas irreversíveis.** A Marca dos Mortos, as Memórias Pesadas (encarar ou deixar), os juramentos e a escolha do companheiro não voltam atrás.
- **O mundo muda com você.** A Maré mede o tempo pelo progresso da história, e o Perfil (como você joga) muda mercadores, companheiros, NPCs e o final.
- **Sem marcadores de missão.** O Diário é o único guia.

O protagonista é uma sombra sem rosto, com dois corpos e quatro origens (Pescador, Sacerdote, Veterano e Artesão), e seis companheiros possíveis. Visual em 2.5D: cenário 3D low-poly com texturas em pixel art, sprites em pé e câmera diagonal.

Plataforma: Windows, com teclado, mouse e controle. Publicação prevista no itch.io.

## Estado

| Marco | Situação |
|---|---|
| M0, protótipo técnico | Concluído |
| M1, combate | Concluído |
| M2, sistemas e dados | Concluído |
| M3, fatia vertical | Em andamento: Cabo Tênaro, o Aqueronte e Asfódelos jogáveis em blocos |
| M4, expansão | Em andamento: o Vestíbulo e A Fome, os Atos II e III em blocos, os chefes Cérbero, O Campeão, A Sem-Nome e O Sonho |
| M5, acabamento | A fazer |

Os detalhes estão em [`docs/BACKLOG.md`](docs/BACKLOG.md), e o que falta, em [`docs/FALTANDO.md`](docs/FALTANDO.md).

## Como rodar

Precisa de:

- **Godot 4.7.2, versão .NET** (C#);
- **.NET SDK** 8 ou mais novo (o projeto compila para `net8.0`).

```
dotnet build Lete.Game.csproj
Godot_v4.7.2-stable_mono_win64.exe --path .
```

Ou abra a pasta no editor do Godot e aperte F5. O jogo começa pela abertura; para ir direto à exploração, passe `-- --sem-abertura`.

Testes das regras (o núcleo `Lete.Core` não depende do Godot):

```
dotnet test src/Lete.Core.Tests
```

## Estrutura

| Pasta | O que tem |
|---|---|
| `src/Lete.Core` | Regras do jogo em C# puro: combate, condições, diálogo, Mosaico, save, sistemas |
| `src/Lete.Core.Tests` | Testes do núcleo (xUnit) |
| `src/Lete.Game` | Cenas e scripts do Godot: mapa, batalha, telas, regiões |
| `content` | Todo o conteúdo em JSON: sombras, falas, itens, Lembranças, regiões, missões e textos (`locale`) |
| `art` | Sprites, texturas, interface e conceitos |
| `docs` | Documento de design (GDD), história, arquitetura, formato dos dados, arte e pendências |
| `tools` | Scripts de apoio em Python: pixelização de sprites, ciclos de andar, blockout das regiões |

Conteúdo é dado, não código: as sombras, as falas e as regiões ficam em `content/`, e nenhum texto fica fixo no código (tudo passa por chaves de localização, com português do Brasil primeiro).

## Documentos

- [`docs/GDD.md`](docs/GDD.md): design do jogo (fonte de verdade)
- [`docs/HISTORIA.md`](docs/HISTORIA.md): enredo, atos, origens, companheiros e chefes
- [`docs/ARQUITETURA.md`](docs/ARQUITETURA.md): organização do código
- [`docs/DADOS.md`](docs/DADOS.md): formato dos arquivos de conteúdo
- [`docs/ARTE.md`](docs/ARTE.md): direção de arte e pipeline de sprites
- [`docs/PENDENCIAS.md`](docs/PENDENCIAS.md): decisões e o que está em aberto

## Autor

João Guilherme Matzenbacher.
