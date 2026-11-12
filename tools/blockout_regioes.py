"""Gera as cenas de blockout das regiões (Tênaro e Aqueronte) com os materiais da sala de teste.

Uso (da raiz do projeto): ../ferramentas/python/Scripts/python.exe tools/blockout_regioes.py .
Escreve src/Lete.Game/Regions/*.tscn do zero: depois de ajustar uma cena no editor do Godot, não rode de novo
(ou passe o ajuste para cá). Blocos agora, arte depois (decisão do João, 30/out/2026).
"""
import sys, os, json, math, zlib

root = sys.argv[1]
out_dir = os.path.join(root, 'src', 'Lete.Game', 'Regions')
os.makedirs(out_dir, exist_ok=True)

HEADER_EXT = [
    ('Script', 'res://src/Lete.Game/World/CameraRig.cs', '1_cam'),
    ('PackedScene', 'res://src/Lete.Game/World/Player.tscn', '2_player'),
    ('Script', 'res://src/Lete.Game/World/Interactable.cs', '3_inter'),
    ('Texture2D', 'res://art/tiles/piso_pedra.png', '4_piso'),
    ('Texture2D', 'res://art/tiles/parede_pedra.png', '5_parede'),
    ('Texture2D', 'res://art/tiles/terracota.png', '6_terracota'),
    ('Texture2D', 'res://art/tiles/agua_lete.png', '7_agua'),
    ('Script', 'res://src/Lete.Game/World/RegionExit.cs', '8_saida'),
    ('Script', 'res://src/Lete.Game/World/TriviaRest.cs', '9_trivia'),
    ('Script', 'res://src/Lete.Game/World/ShadeEncounter.cs', '10_encontro'),
    ('Script', 'res://src/Lete.Game/World/MapSprite.cs', '11_sprite'),
    ('Texture2D', 'res://art/sprites/sombras/remador/parado/remador_parado_frente.png', '12_remador'),
    ('Script', 'res://src/Lete.Game/World/OfferingAltar.cs', '13_altar'),
    ('Script', 'res://src/Lete.Game/World/Talkable.cs', '14_conversa'),
    ('Script', 'res://src/Lete.Game/World/MerchantStall.cs', '15_mercador'),
    ('Script', 'res://src/Lete.Game/World/HeavyMemoryPlace.cs', '18_pesada'),
    ('Script', 'res://src/Lete.Game/World/CompanionSpot.cs', '19_companheiro'),
    ('Texture2D', 'res://art/sprites/companheiros/dimas_sim/parado/dimas_parado_frente.png', '20_dimas'),
    ('Texture2D', 'res://art/sprites/companheiros/calias_sim/parado/calias_parado_frente.png', '21_calias'),
    ('Texture2D', 'res://art/sprites/sombras/padeiro/parado/padeiro_parado_frente.png', '16_padeiro'),
    ('Texture2D', 'res://art/sprites/sombras/mercador/parado/mercador_parado_frente.png', '17_coveiro'),
    ('Script', 'res://src/Lete.Game/World/WanderingShade.cs', '22_vagante'),
    ('Texture2D', 'res://art/sprites/companheiros/lia_sim/parado/lia_parado_frente.png', '23_lia'),
    ('Texture2D', 'res://art/sprites/sombras/camponesa/parado/camponesa_parado_frente.png', '24_camponesa'),
    ('Texture2D', 'res://art/sprites/sombras/cerbero/parado/cerbero_parado_frente.png', '25_cerbero'),
    ('Script', 'res://src/Lete.Game/World/PhysicalShortcut.cs', '26_atalho'),
    # Figuras da fatia em 3/4 (entrega de 05/nov/2026): a imagem parada de 64 px (a escala do mapa); a animação parada vem
    # dos quadros de idle_3q (MapSprite.IdleFramesPrefix).
    ('Texture2D', 'res://art/sprites/sombras/caronte/parado_3q/64px/caronte_parado_3q_esq.png', '27_caronte'),
    ('Texture2D', 'res://art/sprites/sombras/insepultos/parado_3q/64px/insepultos_parado_3q_esq.png', '28_insepultos'),
    ('Texture2D', 'res://art/sprites/sombras/afogado/parado_3q/64px/afogado_parado_3q_esq.png', '29_afogado'),
    ('Texture2D', 'res://art/sprites/sombras/oleiro/parado_3q/64px/oleiro_parado_3q_esq.png', '30_oleiro'),
    # Aprovadas pelo João em 05/nov/2026: a esposa do Padeiro e o Mercador da Planície com corpo próprio.
    ('Texture2D', 'res://art/sprites/sombras/esposa_padeiro/parado_3q/64px/esposa_padeiro_parado_3q_esq.png', '31_esposa'),
    ('Texture2D', 'res://art/sprites/sombras/mercador_planicie/parado_3q/64px/mercador_planicie_parado_3q_esq.png', '32_mercador_planicie'),
    ('Texture2D', 'res://art/sprites/sombras/fome/parado_3q/64px/fome_parado_3q_esq.png', '33_fome'),
    # Ato II (06/nov/2026): o Tribunal, os companheiros que se conhecem nas regiões novas e os chefes.
    ('Script', 'res://src/Lete.Game/World/TribunalSeat.cs', '34_tribunal'),
    ('Texture2D', 'res://art/sprites/companheiros/aulo_sim/parado/aulo_parado_frente.png', '35_aulo'),
    ('Texture2D', 'res://art/sprites/companheiros/ismena_sim/parado/ismena_parado_frente.png', '36_ismena'),
    ('Texture2D', 'res://art/sprites/companheiros/septimia_sim/parado/septimia_parado_frente.png', '37_septimia'),
    ('Texture2D', 'res://art/sprites/sombras/campeao/parado_3q/64px/campeao_parado_3q_esq.png', '38_campeao'),
    ('Texture2D', 'res://art/sprites/sombras/sem_nome/parado_3q/64px/sem_nome_parado_3q_esq.png', '39_sem_nome'),
    # Ato III (10/nov/2026): o Sonho.
    ('Texture2D', 'res://art/sprites/sombras/sonho/parado_3q/64px/sonho_parado_3q_esq.png', '40_sonho'),
    # Ato IV (11/nov/2026): as Fúrias e a Sombra do Esquecido (provisória: o protagonista de costas, indo embora).
    ('Texture2D', 'res://art/sprites/sombras/furias_alecto/parado_3q/64px/furias_alecto_parado_3q_esq.png', '41_furias'),
    ('Texture2D', 'res://art/sprites/sombras/esquecido_m/parado_3q/64px/esquecido_m_parado_3q_esq.png', '42_esquecido'),
    # Entrega de 11/nov/2026 (KIT_GEMINI_FALTANTE): a Sombra do Esquecido nos dois corpos e as figuras da seção B.
    ('Texture2D', 'res://art/sprites/sombras/esquecido_f/parado_3q/64px/esquecido_f_parado_3q_esq.png', '43_esquecido_f'),
    ('Texture2D', 'res://art/sprites/sombras/tanato/parado_3q/64px/tanato_parado_3q_esq.png', '44_tanato'),
    ('Texture2D', 'res://art/sprites/sombras/ascalafo/parado_3q/64px/ascalafo_parado_3q_esq.png', '45_ascalafo'),
    ('Texture2D', 'res://art/sprites/sombras/medo/parado_3q/64px/medo_parado_3q_esq.png', '46_medo'),
    # Paisagem HD-2D (12/nov/2026, PENDENCIAS 105): as peças montadas por código e as texturas do chão.
    ('Script', 'res://src/Lete.Game/Scenery/SceneryPiece.cs', '47_peca'),
    ('Script', 'res://src/Lete.Game/Scenery/FloraField.cs', '48_flora'),
    ('Texture2D', 'res://art/tiles/cenario/terra.png', '49_terra'),
    ('Texture2D', 'res://art/tiles/cenario/campo.png', '50_campo'),
    ('Texture2D', 'res://art/tiles/cenario/laje.png', '51_laje'),
    ('Texture2D', 'res://art/tiles/cenario/pedra.png', '59_pedra'),
    # Segunda entrega do kit (12/nov/2026): as figuras da seção B que faltavam.
    ('Texture2D', 'res://art/sprites/sombras/doenca/parado_3q/64px/doenca_parado_3q_esq.png', '52_doenca'),
    ('Texture2D', 'res://art/sprites/sombras/guerra/parado_3q/64px/guerra_parado_3q_esq.png', '53_guerra'),
    ('Texture2D', 'res://art/sprites/sombras/sisifo/parado_3q/64px/sisifo_parado_3q_esq.png', '54_sisifo'),
    ('Texture2D', 'res://art/sprites/sombras/tantalo/parado_3q/64px/tantalo_parado_3q_esq.png', '55_tantalo'),
    ('Texture2D', 'res://art/sprites/sombras/danaides/parado_3q/64px/danaides_parado_3q_esq.png', '56_danaides'),
    ('Texture2D', 'res://art/sprites/sombras/guardioes_mnemosine/parado_3q/64px/guardioes_mnemosine_parado_3q_esq.png', '57_guardioes'),
    ('Texture2D', 'res://art/sprites/sombras/acerto_pescador/parado_3q/64px/acerto_pescador_parado_3q_esq.png', '58_acerto'),
]


def fmt(v):
    return ('%.3f' % v).rstrip('0').rstrip('.') if isinstance(v, float) else str(v)


class Scene:
    def __init__(self, name, ambient, fog_color, moon_energy, hd=False):
        self.name = name
        self.nodes = []
        self.ambient = ambient
        self.fog_color = fog_color
        self.moon_energy = moon_energy
        # hd: a região já tem o visual novo (paisagem HD-2D, PENDENCIAS 105): brilho, oclusão e contraste no ambiente.
        self.hd = hd
        self._sheets = 0

    def piece(self, name, kind, pos, rot=0, size=(0, 0, 0), variant=0, seed=None, tint=None, lit=True, texture='', edge=None):
        """Peça do cenário montada por código (SceneryPiece): kind é o tipo do catálogo (casa, barraca, muro...); rot,
        o giro em graus (a frente da peça é o +Z; 90 vira a frente para o leste); size, as medidas (0 = padrão)."""
        if seed is None:
            seed = zlib.crc32(f'{self.name}/{name}'.encode()) % 100000
        a = math.radians(rot)
        c, sn = math.cos(a), math.sin(a)
        extra = ''
        if variant:
            extra += f'Variant = {variant}\n'
        if tint:
            extra += f'Tint = Color({tint})\n'
        if not lit:
            extra += 'Lit = false\n'
        if texture:
            extra += f'Texture = "{texture}"\n'
        if edge is not None:
            extra += f'Edge = {fmt(float(edge))}\n'
        self.node(f'''[node name="{name}" type="Node3D" parent="."]
transform = Transform3D({fmt(c)}, 0, {fmt(sn)}, 0, 1, 0, {fmt(-sn)}, 0, {fmt(c)}, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("47_peca")
Kind = "{kind}"
Seed = {seed}
Size = Vector3({fmt(float(size[0]))}, {fmt(float(size[1]))}, {fmt(float(size[2]))})
{extra}''')

    def ground(self, name, texture, center, size, edge=0.8, tint=None):
        """Mancha de chão (lajes de um caminho, terra batida) com a borda roída; cada uma fica um fio acima da anterior."""
        self._sheets += 1
        self.piece(name, 'chao', (center[0], 0.01 + 0.004 * self._sheets, center[1]), size=(size[0], 0, size[1]),
                   texture=texture, edge=edge, tint=tint)

    def mist(self, name, center, size, height=0.4, density=0.5, tint=None):
        """Névoa rasteira: uma lâmina de ruído que se arrasta, na altura dada."""
        self.piece(name, 'nevoa', (center[0], height, center[1]), size=(size[0], density, size[1]), tint=tint)

    def river(self, name, center, size, level=-0.25, still=False, tint=None, fire=False):
        """Água: um rio (com espuma na margem) ou, com still, água parada e rasa (poça, margem alagada); com fire, um
        rio de fogo (o Flegetonte)."""
        self.piece(name, 'agua', (center[0], level, center[1]), size=(size[0], 0, size[1]),
                   variant=2 if fire else (1 if still else 0), tint=tint)

    def flora(self, name, kind, center, area, count, avoid=(), clump=0.6, seed=None):
        """Campo de flora em cartões (FloraField): kind é a folha (asfodelo, capim); avoid, retângulos do mundo
        (x, z, largura, profundidade) onde não nasce nada."""
        if seed is None:
            seed = zlib.crc32(f'{self.name}/{name}'.encode()) % 100000
        rects = ', '.join(f'Rect2({fmt(float(x))}, {fmt(float(z))}, {fmt(float(w))}, {fmt(float(d))})' for x, z, w, d in avoid)
        self.node(f'''[node name="{name}" type="Node3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(center[0])}, 0, {fmt(center[1])})
script = ExtResource("48_flora")
Kind = "{kind}"
Area = Vector2({fmt(float(area[0]))}, {fmt(float(area[1]))})
Count = {count}
Seed = {seed}
Clump = {fmt(float(clump))}
Avoid = Array[Rect2]([{rects}])
''')

    def node(self, text):
        self.nodes.append(text)

    def box(self, name, size, pos, mat='Mat_parede', collide=True, parent='.', tide_min=None):
        # tide_min: o nó só aparece com a Maré nesse degrau ou acima (grupo "mare", lido pelo Main ao carregar a região).
        groups = '["pixel_surface"]' if tide_min is None else '["pixel_surface", "mare"]'
        tide = '' if tide_min is None else f'metadata/mare_min = {tide_min}\n'
        self.node(f'''[node name="{name}" type="CSGBox3D" parent="{parent}" groups={groups}]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
use_collision = {'true' if collide else 'false'}
size = Vector3({fmt(size[0])}, {fmt(size[1])}, {fmt(size[2])})
material = SubResource("{mat}")
{tide}''')

    def blocker(self, name, size, pos):
        """Parede invisível (a água não se atravessa)."""
        self.node(f'''[node name="{name}" type="StaticBody3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})

[node name="Shape" type="CollisionShape3D" parent="{name}"]
shape = SubResource("{self.box_shape(size)}")
''')

    _shapes = None

    def box_shape(self, size):
        if self._shapes is None:
            self._shapes = {}
        key = tuple(size)
        if key not in self._shapes:
            self._shapes[key] = f'Shape_box{len(self._shapes) + 1}'
        return self._shapes[key]

    def column(self, name, pos, height=2.4, radius=0.35, parent='.'):
        self.node(f'''[node name="{name}" type="CSGCylinder3D" parent="{parent}" groups=["pixel_surface"]]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(height / 2)}, {fmt(pos[2])})
use_collision = true
radius = {fmt(radius)}
height = {fmt(height)}
sides = 8
material = SubResource("Mat_parede")
''')

    def light(self, name, pos, color, energy, rng):
        self.node(f'''[node name="{name}" type="OmniLight3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
light_color = Color({color})
light_energy = {fmt(energy)}
omni_range = {fmt(rng)}
''')

    def water(self, name, size, pos, tide_min=None):
        self.box(name, size, pos, mat='Mat_agua', collide=False, tide_min=tide_min)

    def tide(self, tide_min=None, tide_max=None):
        """O último nó só aparece (e só funciona, se for uma área) entre esses degraus da Maré (grupo "mare")."""
        header, rest = self.nodes[-1].split('\n', 1)
        if 'groups=[' in header:
            header = header.replace('groups=[', 'groups=["mare", ', 1)
        else:
            header = header[:-1] + ' groups=["mare"]]'
        meta = ''.join(f'metadata/{k} = {v}\n' for k, v in (('mare_min', tide_min), ('mare_max', tide_max)) if v is not None)
        self.nodes[-1] = header + '\n' + meta + rest

    def tribunal(self, name, pos, trial_id):
        """A pedra do Tribunal (TribunalSeat): abre o julgamento (content/trials)."""
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("34_tribunal")
TrialId = "{trial_id}"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")
''')

    def examinable(self, name, pos, key, flag=None, texture=None):
        """Coisa para examinar (Interactable); texture: a figura parada em 3/4 (as personificações, 11/nov/2026)."""
        flag_line = f'Flag = "{flag}"\n' if flag else ''
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("3_inter")
JournalKey = "{key}"
{flag_line}
[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")
''' + (f'''
[node name="Sprite" type="Sprite3D" parent="{name}"]
texture = ExtResource("{texture}")
script = ExtResource("11_sprite")
''' if texture else ''))

    def figure(self, name, pos, texture):
        """Uma figura parada do mapa, sem interação própria (quem conversa ou examina é outro nó)."""
        self.node(f'''[node name="{name}" type="Sprite3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
texture = ExtResource("{texture}")
script = ExtResource("11_sprite")
''')

    def spring(self, name, pos, kind, amount, key, taken_key):
        """Fonte de oferenda (OfferingAltar): recolher dá o tipo, a frase vai para o Diário."""
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("13_altar")
JournalKey = "{key}"
Kind = "{kind}"
Amount = {amount}
TakenKey = "{taken_key}"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")
''')

    def encounter(self, name, pos, encounter_id, texture='12_remador', idle=None, feminine=None):
        """Sombra de encontro (ShadeEncounter): chegar perto abre a luta (content/encounters). idle: a pasta da figura
        com a animação parada de 64 px (6 quadros); feminine: (textura, pasta) da versão do corpo feminino."""
        extra = ''
        if idle:
            extra += (f'IdleFramesPrefix = "res://art/sprites/sombras/{idle}/idle_3q/64px/esq/{idle}_idle_3q_esq_"\n'
                      'IdleFrameCount = 6\n')
        if feminine:
            tex, folder = feminine
            extra += (f'FeminineTexture = ExtResource("{tex}")\n'
                      f'FeminineIdleFramesPrefix = "res://art/sprites/sombras/{folder}/idle_3q/64px/esq/{folder}_idle_3q_esq_"\n')
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("10_encontro")
EncounterId = "{encounter_id}"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")

[node name="Sprite" type="Sprite3D" parent="{name}"]
texture = ExtResource("{texture}")
script = ExtResource("11_sprite")
{extra}''')

    def wanderer(self, name, pos, encounter_id, shade, offset, notice=4.0):
        """Sombra comum que vaga (WanderingShade): anda até pos+offset e volta; nota quem entra no raio, à frente."""
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("22_vagante")
EncounterId = "{encounter_id}"
SpriteFolder = "res://art/sprites/sombras/{shade}"
SpritePrefix = "{shade}"
WanderOffset = Vector3({fmt(offset[0])}, {fmt(offset[1])}, {fmt(offset[2])})
NoticeRadius = {fmt(notice)}

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")
''')

    def shortcut(self, name, pos, shortcut_id, destination, inside, keys):
        """Ponta de um atalho físico (PhysicalShortcut): a de dentro abre; aberto, leva ao marcador da outra ponta."""
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("26_atalho")
ShortcutId = "{shortcut_id}"
Destination = "{destination}"
Inside = {'true' if inside else 'false'}
ClosedKey = "{keys}.fechado"
OpenedKey = "{keys}.aberto"
UseKey = "{keys}.usar"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")
''')

    def talkable(self, name, pos, dialogue, texture=None, idle=None, by_origin=None):
        """Alguém com quem conversar (Talkable); sem textura, só a voz. idle: a figura em 3/4 (pasta da sombra) com a
        animação parada de 64 px, olhando para a esquerda. by_origin: o caminho da figura com {origem} no lugar do id
        da origem do protagonista (a pessoa do Acerto é uma para cada origem)."""
        idle_line = (f'IdleFramesPrefix = "res://art/sprites/sombras/{idle}/idle_3q/64px/esq/{idle}_idle_3q_esq_"\n'
                     if idle else '')
        if by_origin:
            idle_line += f'OriginTexturePattern = "{by_origin}"\n'
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("14_conversa")
DialogueId = "{dialogue}"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")
''' + (f'''
[node name="Sprite" type="Sprite3D" parent="{name}"]
texture = ExtResource("{texture}")
script = ExtResource("11_sprite")
{idle_line}''' if texture else ''))

    def heavy_place(self, name, pos, memory_id):
        """Lugar onde uma Memória Pesada aflora (HeavyMemoryPlace)."""
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("18_pesada")
MemoryId = "{memory_id}"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")
''')

    def companion(self, name, pos, companion_id, texture):
        """Onde se conhece um companheiro (CompanionSpot): a conversa encontro_<id>; some depois de escolhido."""
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("19_companheiro")
DialogueId = "encontro_{companion_id}"
CompanionId = "{companion_id}"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")

[node name="Sprite" type="Sprite3D" parent="{name}"]
texture = ExtResource("{texture}")
script = ExtResource("11_sprite")
''')

    def merchant(self, name, pos, merchant_id, texture, idle=None):
        idle_line = (f'IdleFramesPrefix = "res://art/sprites/sombras/{idle}/idle_3q/64px/esq/{idle}_idle_3q_esq_"\n'
                     if idle else '')
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("15_mercador")
MerchantId = "{merchant_id}"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")

[node name="Sprite" type="Sprite3D" parent="{name}"]
texture = ExtResource("{texture}")
script = ExtResource("11_sprite")
{idle_line}''')

    def exit(self, name, pos, size, region, entry, path_key='', blocked='', milestone='', condition=None):
        # condition: a condição em JSON (como nos dados), escrita como texto na cena.
        cond = json.dumps(condition, separators=(',', ':')).replace('"', '\\"') if condition else ''
        extra = ''.join(f'{k} = "{v}"\n' for k, v in
                        (('RequiredPathKey', path_key), ('RequiredCondition', cond), ('BlockedKey', blocked),
                         ('Milestone', milestone)) if v)
        self.node(f'''[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(pos[0])}, {fmt(pos[1])}, {fmt(pos[2])})
script = ExtResource("8_saida")
TargetRegion = "{region}"
TargetEntry = "{entry}"
{extra}
[node name="Shape" type="CollisionShape3D" parent="{name}"]
shape = SubResource("{self.box_shape(size)}")
''')

    def trivia(self, name, pos, trivia_id):
        x, _, z = pos
        if self.hd:
            # No visual novo, a Trívia é o pilar de Hécate: três faces, três tochas.
            self.piece(f'{name}Pilar', 'hecateia', (x, 0, z))
        else:
            self.column(f'{name}Coluna', (x, 0, z), height=2.2, radius=0.3)
        self.node(f'''[node name="{name}Chama" type="OmniLight3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(x)}, 2.6, {fmt(z)})
light_color = Color(1, 0.62, 0.3, 1)
light_energy = 1.6
omni_range = 7.0

[node name="{name}" type="Area3D" parent="."]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(x)}, 0, {fmt(z)})
script = ExtResource("9_trivia")
JournalKey = "obj.trivia.examinar"
TriviaId = "{trivia_id}"

[node name="Reach" type="CollisionShape3D" parent="{name}"]
shape = SubResource("Shape_alcance")
''')

    def markers(self, points, player):
        self.node(f'''[node name="Player" parent="." instance=ExtResource("2_player")]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(player[0])}, 0, {fmt(player[2])})

[node name="CameraRig" type="Node3D" parent="." node_paths=PackedStringArray("Target")]
script = ExtResource("1_cam")
Target = NodePath("../Player")

[node name="Pitch" type="Node3D" parent="CameraRig"]

[node name="Camera3D" type="Camera3D" parent="CameraRig/Pitch"]
projection = 1
current = true
size = 10.0
far = 100.0

[node name="TeleportPoints" type="Node3D" parent="."]
''')
        for name, (x, y, z) in points.items():
            self.node(f'''[node name="{name}" type="Marker3D" parent="TeleportPoints" groups=["teleport_points"]]
transform = Transform3D(1, 0, 0, 0, 1, 0, 0, 0, 1, {fmt(x)}, {fmt(y)}, {fmt(z)})
''')

    def write(self, file_name):
        ext = '\n'.join(f'[ext_resource type="{t}" path="{p}" id="{i}"]' for t, p, i in HEADER_EXT)
        shapes = ''.join(f'''[sub_resource type="BoxShape3D" id="{sid}"]
size = Vector3({fmt(k[0])}, {fmt(k[1])}, {fmt(k[2])})

''' for k, sid in (self._shapes or {}).items())
        mats = ''.join(f'''[sub_resource type="StandardMaterial3D" id="{mid}"]
albedo_texture = ExtResource("{tex}")
roughness = 1.0
uv1_triplanar = true
uv1_world_triplanar = true
texture_filter = 0

''' for mid, tex in [('Mat_piso', '4_piso'), ('Mat_parede', '5_parede'), ('Mat_terracota', '6_terracota'), ('Mat_agua', '7_agua'),
                     ('Mat_terra', '49_terra'), ('Mat_campo', '50_campo'), ('Mat_laje', '51_laje'),
                     ('Mat_pedra', '59_pedra')])
        # O visual novo: o brilho pega as chamas e as janelas acesas; a oclusão escurece os cantos; mais contraste.
        hd_env = '''glow_enabled = true
glow_intensity = 0.55
glow_strength = 0.9
glow_bloom = 0.04
glow_hdr_threshold = 1.05
ssao_enabled = true
ssao_radius = 0.8
ssao_intensity = 1.6
tonemap_mode = 2
adjustment_enabled = true
adjustment_contrast = 1.08
adjustment_saturation = 1.08
''' if self.hd else ''
        body = f'''[gd_scene format=3]

{ext}

[sub_resource type="Environment" id="Environment_regiao"]
background_mode = 1
background_color = Color(0.05, 0.047, 0.06, 1)
ambient_light_source = 2
ambient_light_color = Color({self.ambient})
ambient_light_energy = {0.75 if self.hd else 0.55}
fog_enabled = true
fog_light_color = Color({self.fog_color})
fog_light_energy = 0.35
fog_density = 0.004
fog_sky_affect = 0.0
fog_height = 0.4
fog_height_density = 0.3
{hd_env}
{mats}[sub_resource type="SphereShape3D" id="Shape_alcance"]
radius = 1.4

{shapes}[node name="{self.name}" type="Node3D"]

[node name="WorldEnvironment" type="WorldEnvironment" parent="."]
environment = SubResource("Environment_regiao")

[node name="Moon" type="DirectionalLight3D" parent="."]
transform = Transform3D(0.939693, 0, 0.34202, 0.280166, 0.573576, -0.769751, -0.196175, 0.819152, 0.538986, 0, 10, 0)
light_color = Color(0.72, 0.8, 0.88, 1)
light_energy = {fmt(self.moon_energy)}
shadow_enabled = true
directional_shadow_max_distance = 60.0

''' + '\n'.join(self.nodes)
        with open(os.path.join(out_dir, file_name), 'w', encoding='utf-8', newline='\n') as f:
            f.write(body)


def pedras(scene, prefixo, a, b, n, altura=0.9):
    """Uma fila de rochas baixas de a até b (bordas do lado da câmera: baixas, para não tapar o jogador)."""
    for i in range(n):
        k = (i + 0.5) / n
        j = zlib.crc32(f'{prefixo}{i}'.encode())
        dx, dz, s = ((j % 7) - 3) * 0.08, ((j // 7 % 7) - 3) * 0.08, 0.75 + (j // 49 % 6) * 0.1
        scene.piece(f'{prefixo}{i + 1}', 'rocha', (a[0] + (b[0] - a[0]) * k + dx, 0, a[1] + (b[1] - a[1]) * k + dz),
                    size=(1.5 * s, altura * s, 1.2 * s))


# ---- Cabo Tênaro ----
# Só os lados do fundo (norte e oeste) têm paredão alto; os de perto da câmera (sul e leste) têm borda baixa, para não
# esconder o jogador. Sul (z positivo) é o mar; a gruta desce para o norte (z negativo). O chão é plano: a descida se lê
# pelos paredões mais altos e pela luz que some.
# Visual novo (paisagem HD-2D, 12/nov/2026, PENDENCIAS 105 e 108): paredões de rocha, estalagmites, o mar lá embaixo.
t = Scene('Tenaro', '0.45, 0.5, 0.58, 1', '0.306, 0.498, 0.525, 1', 1.1, hd=True)
# A: a saliência fora da gruta, sobre o mar.
t.box('ChaoSaliencia', (14, 1, 12), (0, -0.5, 4), 'Mat_pedra')
t.river('Mar', (0, 24), (70, 30), level=-1.6)
t.ground('TerraSaliencia', 'terra', (0.5, 4.5), (11, 9), edge=1.6)
t.piece('PenhascoOeste', 'penhasco', (-8.7, 0, 4), rot=90, size=(13, 8, 2.6))
t.piece('PenhascoBoca', 'penhasco', (-5.2, 0, -3.4), size=(6, 7, 2.6))
pedras(t, 'BordaMar', (-6.5, 10.4), (6.5, 10.4), 6, 0.7)
pedras(t, 'BordaLeste', (7.4, -1.5), (7.4, 9.5), 5, 0.7)
pedras(t, 'BocaLeste', (2.8, -2.6), (6.8, -2.6), 3, 1.0)
t.blocker('SalienciaSul', (14, 2, 1), (0, 1, 10.6))
t.blocker('SalienciaLeste', (1, 2, 13), (7.6, 1, 4))
t.blocker('BocaLesteBarreira', (5, 2, 1), (5, 1, -2.6))
t.piece('ArvoreSaliencia', 'arvore_morta', (-5.4, 0, 7.6), size=(0, 3.2, 0))
t.flora('CapimSaliencia', 'capim', (0.5, 4.5), (11, 9), 150, clump=0.5)
t.piece('EstalagmiteBoca', 'estalagmite', (-2.6, 0, -2.6), size=(1, 1.9, 0))
t.examinable('OlharMar', (0, 0, 9), 'obj.tenaro.mar')
t.examinable('Boca', (0, 0, -1.2), 'obj.tenaro.boca')
# B: o corredor da descida.
t.box('ChaoCorredor', (6, 1, 24), (0, -0.5, -14.5), 'Mat_pedra')
t.piece('CorredorOeste', 'penhasco', (-4.7, 0, -14.5), rot=90, size=(24, 7.5, 2.6))
pedras(t, 'CorredorLeste', (3.5, -26), (3.5, -3.5), 9, 0.8)
t.blocker('CorredorLesteBarreira', (1, 2, 24), (3.7, 1, -14.5))
t.piece('PedraCaida', 'rocha', (-1.8, 0, -9), size=(1.7, 0.9, 1.3))
t.piece('Estreito', 'rocha', (2, 0, -19), size=(2, 1.1, 1.2))
for i, (x, z, h) in enumerate(((-2.4, -6, 1.3), (2.4, -11, 0.9), (-2.5, -15.5, 1.7), (2.3, -23, 1.2), (-2.4, -25, 1.0))):
    t.piece(f'Estalagmite{i + 1}', 'estalagmite', (x, 0, z), size=(0.9, h, 0))
# A poça dá a água que o Remador aceita (o encontro das marcas pede a oferenda).
t.spring('Poca', (1.6, 0, -12.5), 'agua', 3, 'obj.tenaro.poca', 'obj.tenaro.poca.recolheu')
t.river('PocaAgua', (1.6, -12.5), (1.7, 1.7), level=0.02, still=True)
t.ground('TerraCorredor', 'terra', (-0.2, -14.5), (4.6, 23), edge=1.4)
t.ground('TerraCamara', 'terra', (0.5, -32), (14, 9), edge=2)
t.examinable('Marcas', (-2.5, 0, -22), 'obj.tenaro.marcas')
t.light('LuzBoca', (0, 2.5, -5), '0.55, 0.65, 0.8, 1', 1.0, 9)
t.light('LuzCorredor', (0, 2.5, -17), '0.3, 0.5, 0.55, 1', 0.9, 11)
t.piece('TochaCorredor', 'tocha', (-2.3, 0, -20.5), size=(1.2, 0, 8))
t.piece('TochaBoca', 'tocha', (2.2, 0, -4.2), size=(1.2, 0, 8))
t.piece('BraseiroCamara', 'braseiro', (6.4, 0, -33), size=(1.3, 0, 9))
# C: a câmara do rio, com a primeira Trívia e a passagem para o Vestíbulo.
t.box('ChaoCamara', (18, 1, 16), (0, -0.5, -34.5), 'Mat_pedra')
t.piece('CamaraOeste', 'penhasco', (-10.7, 0, -34.5), rot=90, size=(17, 8.5, 2.6))
t.piece('CamaraNorte', 'penhasco', (-2.6, 0, -44.3), size=(15, 8.5, 2.6))
t.piece('CamaraNorteLeste', 'penhasco', (9.6, 0, -44.2), size=(2.4, 7, 2.2))
t.piece('PortalVestibulo', 'arco', (6.5, 0, -43.7), size=(2.6, 3.8, 1.1))
t.piece('PortalVestibuloTocha', 'tocha', (4.2, 0, -42.6), size=(1.0, 2.1, 6))
pedras(t, 'CamaraSulO', (-8.6, -26.6), (-3.6, -26.6), 3, 0.7)
pedras(t, 'CamaraSulL', (3.6, -26.6), (8.6, -26.6), 3, 0.7)
pedras(t, 'CamaraLeste', (9.5, -42), (9.5, -27.5), 6, 0.8)
t.blocker('CamaraLesteBarreira', (1, 2, 17), (9.7, 1, -34.5))
t.blocker('CamaraSulA', (5.5, 2, 1), (-6.25, 1, -26.6))
t.blocker('CamaraSulB', (5.5, 2, 1), (6.25, 1, -26.6))
t.river('RioSubterraneo', (-3, -38.5), (12, 3), level=0.02, still=True)
t.blocker('RioBarreira', (12, 2, 3), (-3, 1, -38.5))
t.trivia('Trivia', (-5, 0, -30.5), 'trivia_tenaro')
# A tabela da Maré de Tênaro (05/nov/2026, PENDENCIAS 85): com o degrau 1, o rio passa da margem e a poça cresce.
t.river('RioMare1', (-3, -39.25), (14, 4.5), level=0.03, still=True)
t.tide(tide_min=1)
t.river('PocaMare1', (1.6, -12.5), (2.7, 2.7), level=0.03, still=True)
t.tide(tide_min=1)
t.light('LuzCamara', (2, 3, -35), '0.35, 0.55, 0.6, 1', 1.2, 13)
for i, (x, z, h) in enumerate(((-8, -41.5, 2.2), (-7.6, -28.4, 1.4), (7.8, -31, 1.1), (0.5, -41.6, 1.8))):
    t.piece(f'EstalagmiteCamara{i + 1}', 'estalagmite', (x, 0, z), size=(1.2, h, 0))
t.mist('NevoaCamara', (0, -35), (19, 17), height=0.3, density=0.5)
t.mist('NevoaCorredor', (0, -15), (7, 24), height=0.3, density=0.3)
# O Remador-tutorial, uma coisa por luta (decisão do João, 30-31/out/2026): cada encontro aparece depois do anterior.
t.encounter('RemadorBoca', (0, 0, -3), 'remador_boca')
t.encounter('RemadorPoca', (-0.8, 0, -16), 'remador_poca')
t.encounter('RemadorMarcas', (0.5, 0, -24), 'remador_marcas')
t.encounter('RemadorCamara', (3, 0, -36), 'remador_camara')
# A camada das sombras da Maré: a carpideira vaga na descida a partir do degrau 1 (o encontro pede tide_min 1).
t.wanderer('SombraCarpideira', (1, 0, -7), 'tenaro_carpideira', 'carpideira', (-2, 0, -3))
t.exit('ParaVestibulo', (6.5, 1, -43.6), (3, 2, 1.2), 'vestibulo', 'ChegadaTenaro')
# Atalho físico (GDD 3.6; 03/nov/2026): uma corda velha enrolada na câmara do rio; jogada pela fenda (por dentro), liga a
# câmara à saliência da boca da gruta, nos dois sentidos, sem custar Maré.
t.shortcut('CordaCamara', (8, 0, -28), 'corda_tenaro', 'CordaBocaPe', True, 'obj.tenaro.corda.camara')
t.shortcut('CordaBoca', (6, 0, -1.2), 'corda_tenaro', 'CordaCamaraPe', False, 'obj.tenaro.corda.boca')
t.piece('Fenda', 'rocha', (6.6, 0, -1.9), size=(0.7, 0.6, 0.6))
t.piece('Argola', 'cesto', (8.5, 0, -28.6), size=(0.45, 0, 0))
t.markers({
    'Entrada': (0, 0, 6),
    'Gruta': (0, 0, -5),
    'Poca': (0, 0, -12.5),
    'Camara': (2, 0, -30),
    'Trivia': (-3.5, 0, -30.5),
    'SaidaVestibulo': (6.5, 0, -40.5),
    'CordaCamara': (7.5, 0, -29),
    'CordaCamaraPe': (6.8, 0, -29.2),
    'CordaBoca': (5.4, 0, -0.2),
    'CordaBocaPe': (5, 0, 0.6),
    # Dentro da passagem: teleportar para ele atravessa (conferência e debug).
    'AtravessarVestibulo': (6.5, 0, -43.6),
    # Em cima dos encontros: teleportar para eles começa a luta (se o encontro estiver disponível).
    'RemadorBoca': (0, 0, -3),
    'RemadorPoca': (-0.8, 0, -16),
    'RemadorMarcas': (0.5, 0, -24),
    'RemadorCamara': (3, 0, -36),
    'Carpideira': (1, 0, -7),
}, player=(0, 0, 6))
t.write('Tenaro.tscn')

# ---- O Vestíbulo ----
# Ato I (HISTORIA 2 e 7.1, 05/nov/2026, PENDENCIAS 86): entre a câmara do rio de Tênaro (sul, z positivo) e a margem
# do Aqueronte (norte). Um salão sem teto com o olmo dos sonhos vãos e as personificações paradas na névoa (o Medo, a
# Doença e a Guerra); no estreito do norte, a Fome, chefe do Ato I, no caminho para o rio.
# Visual novo (12/nov/2026, PENDENCIAS 108): um salão antigo em ruína, de lajes, com a muralha a oeste e ao norte, colunas
# quebradas, o olmo com os sonhos pendurados e a névoa espessa.
v = Scene('Vestibulo', '0.4, 0.42, 0.5, 1', '0.33, 0.35, 0.42, 1', 0.9, hd=True)
v.box('ChaoSalao', (16, 1, 31), (0, -0.5, 0), 'Mat_laje')
v.box('ChaoFora', (90, 1, 90), (0, -0.56, 0), 'Mat_terra', collide=False)
v.ground('TerraOlmo', 'terra', (-4, 4), (7, 7), edge=1.8)
v.ground('TerraSul', 'terra', (2.5, 12), (9, 5), edge=1.6)
v.piece('MuralhaOeste', 'muro', (-8.6, 0, 0), rot=90, size=(31, 4.6, 0.9))
v.piece('ParedaoOeste', 'penhasco', (-12, 0, 0), rot=90, size=(34, 11, 3))
v.piece('MuralhaNorteO', 'muro', (-5, 0, -15.6), size=(6.4, 4.6, 0.9))
v.piece('MuralhaNorteL', 'muro', (5, 0, -15.6), size=(6.4, 4.6, 0.9))
v.piece('PortalAqueronte', 'arco', (0, 0, -15.6), size=(2.8, 4.4, 1.1))
v.piece('ParedaoNorte', 'penhasco', (0, 0, -19.5), size=(22, 12, 3))
v.piece('MuroLeste', 'muro', (8.6, 0, 0), rot=90, size=(31, 0.7, 0.6), variant=1)
v.blocker('LesteBarreira', (1, 2, 31), (8.7, 1, 0))
v.piece('MuroSulO', 'muro', (-4.9, 0, 15.6), size=(6.2, 0.7, 0.6), variant=1)
v.piece('MuroSulL', 'muro', (4.9, 0, 15.6), size=(6.2, 0.7, 0.6), variant=1)
v.blocker('SulA', (6.5, 2, 1), (-4.75, 1, 15.6))
v.blocker('SulB', (6.5, 2, 1), (4.75, 1, 15.6))
# O estreito: a muralha deixa uma passagem de 3 m, onde a Fome espera, entre dois braseiros frios.
v.piece('EstreitoOeste', 'muro', (-4.9, 0, -9), size=(6.4, 3.2, 1.6))
v.piece('EstreitoLeste', 'muro', (4.9, 0, -9), size=(6.4, 1.1, 1.6), variant=1)
v.piece('EstreitoBraseiro', 'braseiro', (-2.4, 0, -7.4), size=(0.9, 0, 6))
for i, (x, z, quebrada) in enumerate(((-6.6, 12, 0), (-6.6, 8.4, 1), (-6.6, -2, 0), (-6.6, -5.6, 1), (6.4, 11, 1), (6.2, -13, 1))):
    v.piece(f'Coluna{i + 1}', 'coluna', (x, 0, z), size=(0.8, 4, 0), variant=quebrada)
v.piece('Olmo', 'olmo', (-4, 0, 4), size=(0, 7.5, 0))
v.examinable('OlmoOlhar', (-2.4, 0, 4.6), 'obj.vestibulo.olmo')
v.examinable('Entrada', (0, 0, 11.5), 'obj.vestibulo.entrada')
v.examinable('Medo', (4, 0, 7.2), 'obj.vestibulo.medo', texture='46_medo')
v.examinable('Doenca', (4.6, 0, 0.2), 'obj.vestibulo.doenca', texture='52_doenca')
v.blocker('DoencaLaje', (1.8, 1, 0.7), (4.6, 0.5, 0))
v.examinable('Guerra', (-4.6, 0, -3.8), 'obj.vestibulo.guerra', texture='53_guerra')
v.blocker('GuerraCorpo', (0.8, 2, 0.6), (-4.6, 1, -4))
v.encounter('Fome', (0, 0, -9), 'vestibulo_fome', texture='33_fome')
v.piece('EntulhoSalao1', 'entulho', (6.4, 0, 4.5), size=(1.6, 0, 2))
v.piece('EntulhoSalao2', 'entulho', (-6, 0, -11.5), size=(2, 0, 1.6))
# Tabela da Maré (06/nov/2026, PENDENCIAS 89): com o degrau 1, mais formas paradas na névoa e um soldado que foge.
v.piece('FormaMare1', 'estela', (-5.5, 0, 9.6), rot=30, size=(0, 1.5, 0), tint='0.6, 0.62, 0.7, 1')
v.tide(tide_min=1)
v.piece('FormaMare2', 'estela', (5.5, 0, -4.5), rot=-20, size=(0, 1.8, 0), tint='0.6, 0.62, 0.7, 1')
v.tide(tide_min=2)
v.wanderer('SombraFugitivo', (2, 0, 4), 'vestibulo_fugitivo', 'soldado', (-4, 0, 0))
v.light('LuzSalao', (0, 4, 2), '0.45, 0.47, 0.6, 1', 1.1, 17)
v.mist('NevoaSalao', (0, 0), (17, 32), height=0.4, density=0.75, tint='0.2, 0.22, 0.28, 1')
v.mist('NevoaSalaoAlta', (0, 2), (17, 26), height=1.2, density=0.4, tint='0.2, 0.22, 0.28, 1')
v.exit('ParaTenaro', (0, 1, 15.4), (3, 2, 1.2), 'tenaro', 'SaidaVestibulo')
v.exit('ParaAqueronte', (0, 1, -15.4), (3, 2, 1.2), 'aqueronte', 'ChegadaVestibulo')
v.markers({
    'ChegadaTenaro': (0, 0, 12.5),
    'SaidaAqueronte': (0, 0, -12.5),
    'Olmo': (-2, 0, 5.5),
    # Em cima do encontro: teleportar para ele começa a luta com a Fome.
    'Fome': (0, 0, -9),
    'Fugitivo': (2, 0, 4),
    'Medo': (3, 0, 8.4),
    'Guerra': (-3.4, 0, -2.6),
    'AtravessarTenaro': (0, 0, 15.4),
    'AtravessarAqueronte': (0, 0, -15.4),
}, player=(0, 0, 12.5))
v.write('Vestibulo.tscn')

# ---- Margem do Aqueronte ----
# Visual novo (12/nov/2026, PENDENCIAS 108): a margem de terra e juncos, o paredão a oeste, o cais de Caronte com a
# lanterna dele, a barca e o rio largo com névoa ao norte.
a = Scene('Aqueronte', '0.42, 0.47, 0.55, 1', '0.36, 0.42, 0.48, 1', 1.0, hd=True)
a.box('ChaoMargem', (20, 1, 14), (0, -0.5, 0), 'Mat_terra')
a.box('ChaoFora', (90, 1, 50), (0, -0.56, 18), 'Mat_terra', collide=False)
a.ground('TrilhaVestibulo', 'laje', (0, 4.5), (3.4, 6.5), edge=1.2)
a.ground('AreiaMargem', 'campo', (-1, -5.2), (19, 3.4), edge=1.2, tint='0.8, 0.8, 0.8, 1')
a.piece('ParedaoOeste', 'penhasco', (-11.8, 0, 0), rot=90, size=(16, 9, 2.8))
pedras(a, 'BordaLeste', (10.4, -6), (10.4, 7), 6, 0.8)
pedras(a, 'BordaSulO', (-9.6, 7.5), (-2.4, 7.5), 4, 0.8)
pedras(a, 'BordaSulL', (2.4, 7.5), (9.6, 7.5), 4, 0.8)
a.blocker('Leste', (1, 2, 15), (10.6, 1, 0))
a.blocker('SulA', (8.5, 2, 1), (-5.75, 1, 7.6))
a.blocker('SulB', (8.5, 2, 1), (5.75, 1, 7.6))
a.river('Rio', (0, -22), (70, 30), level=-0.3)
a.blocker('RioBarreira', (22, 2, 1), (0, 1, -7.5))
a.piece('Cais', 'cais', (4, 0, -9), size=(2.2, 0, 4.6))
a.piece('Barca', 'barca', (6.3, -0.32, -10), rot=90, size=(5, 0, 1.6), tint='0.8, 0.78, 0.75, 1')
a.piece('PosteCaronte', 'tocha', (2.6, 0, -6.7), size=(1.4, 2.6, 8))
a.piece('ArvoreMargem', 'arvore_morta', (-8.6, 0, -4.6), size=(0, 3.4, 0))
a.piece('TroncoMargem', 'entulho', (-4.5, 0, 3.5), size=(2, 0, 1.6))
a.flora('Juncos', 'capim', (-1, -5.6), (19, 2.6), 220, clump=0.2)
a.flora('CapimMargem', 'capim', (0, 1), (19, 12), 260, avoid=[(-1.8, 1, 3.6, 7)], clump=0.7)
a.examinable('Margem', (-1, 0, -6), 'obj.aqueronte.margem')
# Caronte e os insepultos: conversas (content/dialogue/caronte.json e insepultos.json).
a.talkable('Caronte', (3.2, 0, -5.6), 'caronte', texture='27_caronte', idle='caronte')
a.talkable('Insepultos', (-7, 0, -2), 'insepultos', texture='28_insepultos', idle='insepultos')
# O afogado (capítulo 1 do Pescador: "um afogado pede para ser achado"), na água rasa da margem.
a.talkable('Afogado', (-3.5, 0, -6.2), 'afogado', texture='29_afogado', idle='afogado')
# A esposa do Padeiro (a missão dele, 03/nov/2026): não atravessou; espera na margem, do lado de cá.
a.talkable('Esposa', (-8, 0, 4.5), 'esposa_padeiro', texture='31_esposa', idle='esposa_padeiro')
a.examinable('BarcaOlhar', (7.6, 0, -5.4), 'obj.aqueronte.barca')
# Tabela da Maré (06/nov/2026, PENDENCIAS 89): o rio sobe na margem e traz uma afogada.
a.river('MargemMare1', (0, -6.5), (20, 1.6), level=0.025, still=True)
a.tide(tide_min=1)
a.river('MargemMare2', (0, -5.2), (20, 1.4), level=0.03, still=True)
a.tide(tide_min=2)
a.wanderer('SombraAfogada', (-5, 0, 1), 'aqueronte_afogada', 'lavadeira', (4, 0, 0))
# O Convite (1ª Pesada) aflora diante da barca, depois de Caronte falar da "outra pessoa" (gate na Pesada).
a.heavy_place('PesadaConvite', (5.8, 0, -4.2), 'o_convite')
# A barca: com a chave do Aqueronte (óbolo, insepultos, origem Pescador...), leva a Asfódelos; é o 1º marco da Maré.
a.exit('SubirNaBarca', (5.8, 1, -6.9), (1.6, 2, 0.8), 'asfodelos', 'ChegadaAqueronte',
       path_key='aqueronte', blocked='obj.aqueronte.barca.fechada', milestone='aqueronte')
a.light('LuzMargem', (0, 4, -2), '0.5, 0.56, 0.66, 1', 1.1, 15)
a.mist('NevoaMargem', (0, 0), (21, 15), height=0.35, density=0.4)
a.mist('NevoaRio', (0, -16), (60, 18), height=0.05, density=0.8)
a.mist('NevoaRioAlta', (0, -18), (60, 14), height=1.0, density=0.5)
a.exit('ParaVestibulo', (0, 1, 7.9), (3, 2, 1.2), 'vestibulo', 'SaidaAqueronte')
a.markers({
    'ChegadaVestibulo': (0, 0, 5),
    'Caronte': (3.2, 0, -4.5),
    'Insepultos': (-6, 0, -1),
    'Afogado': (-3.5, 0, -5),
    'Esposa': (-7, 0, 3.5),
    'Afogada': (-5, 0, 1),
    'AtravessarVestibulo': (0, 0, 7.9),
    # A volta de Asfódelos: na margem, diante da barca.
    'ChegadaBarca': (5.8, 0, -2.6),
    'PesadaConvite': (5.8, 0, -4.2),
    # Em cima da barca: teleportar para ele tenta atravessar (com a chave, vai a Asfódelos).
    'AtravessarBarca': (5.8, 0, -6.9),
}, player=(0, 0, 5))
a.write('Aqueronte.tscn')

# ---- Asfódelos (parte da fatia: a margem, a grande Trívia, a Rua dos Ofícios, o Altar e a Planície) ----
# A barca chega pelo sul; a Trívia fica no centro; a Rua dos Ofícios a oeste (o Padeiro e o Coveiro); o Altar a leste e,
# depois dele, a Planície (campo aberto com as sombras comuns) e o mercado no fundo, onde está a Lia (03/nov/2026).
#
# Piloto da paisagem HD-2D (decisão do João, 12/nov/2026, PENDENCIAS 105): a região é montada com as peças do cenário
# (SceneryPiece) no lugar dos blocos. O que é de jogo (conversas, encontros, passagens, marcadores) ficou onde estava.
# A câmera olha do sudeste para o noroeste: o que é alto fica ao norte e a oeste (muralha, paredões, casas com a frente
# para o sul ou o leste); ao sul e a leste só coisas baixas (o rio, cercas, bancas), para não tapar o jogador.
TERRACOTA, AZUL, OSSO, OURO, ROXO = ('0.62, 0.3, 0.22, 1', '0.3, 0.48, 0.5, 1', '0.8, 0.75, 0.62, 1',
                                     '0.72, 0.56, 0.26, 1', '0.4, 0.32, 0.42, 1')
f = Scene('Asfodelos', '0.44, 0.52, 0.55, 1', '0.3, 0.38, 0.4, 1', 1.25, hd=True)
f.box('Chao', (32, 1, 26), (0, -0.5, 0), 'Mat_terra')
f.box('ChaoPlanicie', (34, 1, 26), (33, -0.5, 0), 'Mat_campo')
f.box('ChaoPoco', (16, 1, 26), (-25, -0.5, 0), 'Mat_terra')
# O chão continua para além de onde se anda (ao norte e a oeste), sob a muralha e os paredões.
f.box('ChaoFora', (117.5, 1, 60), (-8.25, -0.56, -17), 'Mat_terra', collide=False)
# O Aqueronte ao sul e, dobrando, a leste da Planície.
f.river('Aqueronte', (6.25, 24), (88.5, 22))
f.river('AqueronteLeste', (62.75, 2.5), (24.5, 65))
f.blocker('MargemBarreira', (66, 2, 1), (17, 1, 13.5))
f.blocker('LesteBarreira', (1, 2, 27), (50.5, 1, 0))
f.blocker('NorteBarreira', (84, 3, 1), (8.5, 1.5, -14.2))
f.blocker('PocoOeste', (1, 3, 27), (-33.5, 1.5, 0))
f.blocker('PocoSul', (17, 2, 1), (-25, 1, 13.5))

# -- os caminhos: lajes na cidade, terra batida na Planície --
f.ground('EstradaChegada', 'laje', (0, 2.5), (4.4, 22))
f.ground('PracaTrivia', 'laje', (0, -8.6), (15, 8.5), edge=1.2)
f.ground('EstradaLesteOeste', 'laje', (4, 4.5), (27, 3.4))
f.ground('RuaOficios', 'laje', (-10.2, 1.5), (4.6, 17))
f.ground('PracaAltar', 'laje', (11, -2.6), (8.5, 7), edge=1.2)
f.ground('TrilhaPoco', 'terra', (-18, -6.5), (15, 2.6), tint='0.62, 0.62, 0.62, 1')
f.ground('TrilhaPlanicie', 'terra', (29, 4.5), (23, 2.6), edge=1.0)
f.ground('TrilhaMercado', 'terra', (40.5, 0.5), (2.6, 9), edge=1.0)
f.ground('PracaMercado', 'laje', (45, -4.6), (10, 10.5), edge=1.3)
f.ground('ChaoAldeia', 'terra', (30, -9.5), (17, 6.5), edge=1.6)
f.ground('MargemSul', 'terra', (10, 12.2), (70, 2.4), edge=1.0, tint='0.7, 0.7, 0.7, 1')

# -- a muralha do norte, com os portais dos três caminhos e o do Estige; atrás dela, o paredão da caverna --
for nome, x in (('PortalLuto', -6), ('PortalTribunal', 0), ('PortalGuerreiros', 6), ('PortalEstige', 12)):
    f.piece(nome, 'arco', (x, 0, -13.7), size=(3, 4.6, 1.2), variant=1 if nome == 'PortalEstige' else 0)
    f.piece(f'{nome}TochaO', 'tocha', (x - 2.2, 0, -12.7), size=(1.1, 2.2, 6.5))
    f.piece(f'{nome}TochaL', 'tocha', (x + 2.2, 0, -12.7), size=(0, 2.2, 0), lit=False)
for nome, centro, comprimento in (('MuralhaA', -12.45, 8.1), ('MuralhaB', -3, 1.2), ('MuralhaC', 3, 1.2), ('MuralhaD', 9, 1.2),
                                  ('MuralhaE', 15.7, 2.6)):
    f.piece(nome, 'muro', (centro, 0, -13.7), size=(comprimento, 4.2, 1.0))
f.piece('ParedaoCidade', 'penhasco', (0, 0, -18.5), size=(36, 13, 3.5))
f.piece('ParedaoPlanicie', 'penhasco', (34, 0, -18.5), size=(36, 11, 3.5), tint='0.9, 0.95, 0.95, 1')
f.piece('MuroCampoNorte', 'muro', (34, 0, -13.7), size=(32.8, 1.3, 0.6), variant=1)
for i, x in enumerate((20, 24.5, 29, 38, 43, 47.5)):
    f.piece(f'CiprestePlanicie{i + 1}', 'cipreste', (x, 0, -15.6 + (i % 2) * 0.8), size=(1.4, 6 + (i % 3) * 0.8, 0))

# -- a muralha do oeste (entre a cidade e o Poço), com a brecha --
f.piece('MuralhaOesteA', 'muro', (-16.6, 0, -10.75), rot=90, size=(5.5, 3.2, 0.8))
f.piece('MuralhaOesteB', 'muro', (-16.6, 0, 4.25), rot=90, size=(18.5, 3.2, 0.8))
f.piece('BrechaEntulho', 'entulho', (-16.4, 0, -8.4), size=(1.6, 0, 1.2))
f.piece('BrechaColuna', 'coluna', (-15.6, 0, -4.6), variant=1)

# -- a estrada da chegada: o cais, a barca e uma colunata até a Trívia --
f.piece('Cais', 'cais', (0, 0, 14.2), size=(2.4, 0, 5.6))
f.piece('Barca', 'barca', (2.3, -0.28, 15.6), rot=84, size=(5, 0, 1.5), tint='0.8, 0.78, 0.75, 1')
for i, z in enumerate((9.5, 5.6, 1.6, -2.4)):
    f.piece(f'ColunaO{i + 1}', 'coluna', (-2.9, 0, z), variant=1 if i == 1 else 0)
    f.piece(f'ColunaL{i + 1}', 'coluna', (2.9, 0, z), variant=1 if i == 2 else 0)
f.piece('BraseiroChegadaO', 'braseiro', (-2.9, 0, 7.5), size=(1.2, 0, 7))
f.piece('BraseiroChegadaL', 'braseiro', (2.9, 0, -0.4), size=(1.2, 0, 7))
f.examinable('Planicie', (19, 0, 4), 'obj.asfodelos.planicie')
f.exit('ParaAqueronte', (0, 1, 13), (2, 2, 0.8), 'aqueronte', 'ChegadaBarca')

# -- a grande Trívia (liga a todas as acesas) e os três caminhos --
f.trivia('Trivia', (0, 0, -8), 'trivia_asfodelos')
# Os três caminhos: fechados pela névoa até o segundo degrau da Maré (escolher o companheiro; decisão do João, 06/nov/2026).
f.examinable('CaminhoLuto', (-6, 0, -12.2), 'obj.asfodelos.caminho_luto')
f.tide(tide_max=1)
f.examinable('CaminhoTribunal', (0, 0, -12.2), 'obj.asfodelos.caminho_tribunal')
f.tide(tide_max=1)
f.examinable('CaminhoGuerreiros', (6, 0, -12.2), 'obj.asfodelos.caminho_guerreiros')
f.tide(tide_max=1)
for nome, x in (('NevoaLuto', -6), ('NevoaTribunal', 0), ('NevoaGuerreiros', 6)):
    f.piece(nome, 'veu', (x, 1.9, -13.2), size=(3, 3.8, 0.9), tint='0.26, 0.36, 0.38, 1')
    f.tide(tide_max=1)
f.exit('ParaLuto', (-6, 1, -12.6), (2.6, 2, 1), 'luto', 'ChegadaAsfodelos')
f.tide(tide_min=2)
f.exit('ParaTribunal', (0, 1, -12.6), (2.6, 2, 1), 'tribunal', 'ChegadaAsfodelos')
f.tide(tide_min=2)
f.exit('ParaGuerreiros', (6, 1, -12.6), (2.6, 2, 1), 'guerreiros', 'ChegadaAsfodelos')
f.tide(tide_min=2)
# O caminho do Estige (Ato III; decisão do João, 10/nov/2026): ao norte, a leste dos outros três; abre quando os três
# caminhos do Ato II foram visitados.
f.examinable('CaminhoEstige', (12, 0, -12.2), 'obj.asfodelos.caminho_estige')
f.exit('ParaEstige', (12, 1, -12.6), (2.6, 2, 1), 'estige', 'ChegadaAsfodelos', blocked='obj.asfodelos.caminho_estige.fechado',
       condition={'all': [{'region_visited': r} for r in ('luto', 'tribunal', 'guerreiros')]})

# -- a Rua dos Ofícios: as oficinas encostadas na muralha do oeste, com a frente para a rua (leste) --
f.piece('CasaNoroeste', 'casa', (-14.1, 0, -11), rot=90, size=(4, 2.9, 3.6))
# O oleiro sem mãos (capítulo 1 do Artesão: "um oleiro sem mãos pede reparo"), diante da oficina dele.
f.piece('OficinaOleiro', 'oficina', (-14.6, 0, -2.6), rot=90, size=(4, 2.6, 2.7), variant=1)
f.talkable('Oleiro', (-12, 0, -2.6), 'oleiro', texture='30_oleiro', idle='oleiro')
# A padaria: o forno do Padeiro aceso dentro da oficina.
f.piece('Padaria', 'oficina', (-14.6, 0, 1.4), rot=90, size=(3.4, 2.7, 2.7), variant=4)
f.talkable('Padeiro', (-10.5, 0, 1.5), 'padeiro', texture='16_padeiro')
# O beco onde o Dimas espreita, entre a padaria e a banca do Coveiro.
f.piece('BecoTralha', 'tralha', (-15.2, 0, 4), rot=90, size=(1.6, 0, 1.2))
f.companion('Dimas', (-13.5, 0, 4), 'dimas', '20_dimas')
# O Coveiro vende o que os mortos levam: a banca de toldo escuro e estelas por gravar.
f.piece('BancaCoveiro', 'barraca', (-12.6, 0, 6.6), rot=90, size=(2.8, 2.4, 1.9), tint=ROXO)
f.piece('EstelaVenda1', 'estela', (-12.9, 0, 8.6), rot=75, size=(0, 1.1, 0))
f.piece('EstelaVenda2', 'estela', (-13.7, 0, 9.1), rot=100, size=(0, 1.4, 0))
f.merchant('Coveiro', (-10.5, 0, 6.5), 'coveiro', '17_coveiro')
f.piece('CasaSudoeste', 'casa', (-14.1, 0, 11), rot=90, size=(4, 2.5, 3.6), variant=1)
f.examinable('Oficios', (-9, 0, -4), 'obj.asfodelos.oficios')
f.piece('TochaRua', 'tocha', (-8.2, 0, -0.6), size=(1.2, 0, 7))
# A viúva chega com o segundo degrau da Maré e procura entre as bancas da Rua dos Ofícios.
f.wanderer('SombraViuva', (-7, 0, -1), 'asfodelos_viuva', 'carpideira', (0, 0, 6))
# O miolo entre a Rua e a estrada: um quintal em ruína, o poço da cidade e ciprestes (baixo, para não tapar a Rua).
f.piece('RuinaQuintal', 'casa', (-4.6, 0, 9.2), size=(3.6, 1.5, 3), variant=2)
f.piece('PocoCidade', 'poco', (-5.4, 0, -2.6))
f.piece('CipresteCidade1', 'cipreste', (-5.6, 0, 1.6), size=(1.2, 5, 0))
f.piece('CipresteCidade2', 'cipreste', (-4.4, 0, -11.4), size=(1.2, 6, 0))
f.piece('CarrocaRua', 'carroca', (-7, 0, 9.6), rot=20)

# -- o Altar das Oferendas: um pequeno santuário a céu aberto --
f.piece('Altar', 'altar', (11, 0, -3), size=(3, 0, 2))
f.spring('AltarFarinha', (10.2, 0, -2), 'farinha', 2, 'obj.asfodelos.altar', 'ui.altar.recolheu')
f.spring('AltarMel', (11.8, 0, -2), 'mel', 1, 'obj.asfodelos.altar', 'ui.altar.recolheu')
f.companion('Calias', (14, 0, -0.5), 'calias', '21_calias')
for nome, x, z, quebrada in (('AltarColunaNO', 8, -5.6, 0), ('AltarColunaNE', 14, -5.6, 1), ('AltarColunaSO', 8, 0.2, 1)):
    f.piece(nome, 'coluna', (x, 0, z), size=(0.6, 3, 0), variant=quebrada)
f.piece('AltarCipresteO', 'cipreste', (8.4, 0, -8.2), size=(1.3, 6.5, 0))
f.piece('AltarCipresteL', 'cipreste', (14.6, 0, -8.6), size=(1.3, 5.5, 0))
f.piece('AltarOferendas', 'tralha', (13.4, 0, -4.4), size=(1.2, 0, 0.8))
# Ao sul da estrada, perto do rio: duas casas baixas de pescadores e o varal.
f.piece('CasaRio1', 'casa', (7.2, 0, 10.6), size=(4, 2.2, 3), variant=1)
f.piece('CasaRio2', 'casa', (13, 0, 10.9), size=(3.4, 2.1, 2.8))
f.piece('VaralRio', 'varal', (10.2, 0, 8.2), size=(3, 0, 0))
f.piece('CercaRio', 'cerca', (-9.5, 0, 12.4), size=(9, 0.9, 0))

# -- a entrada da Planície: um muro baixo com o portal, e Cérbero nele (HISTORIA 6; 03/nov/2026). Acalmado, libertado,
# presenciado, absorvido ou desfeito, a passagem fica livre. --
f.piece('MuroPlanicieNorte', 'muro', (17, 0, -5.3), rot=90, size=(15.4, 1.6, 0.7), variant=1)
f.piece('MuroPlanicieSul', 'muro', (17, 0, 9.7), rot=90, size=(6.6, 1.6, 0.7), variant=1)
f.piece('PortalPlanicie', 'arco', (17, 0, 4.5), rot=90, size=(2.4, 3.3, 0.9))
f.encounter('Cerbero', (17, 0, 4.5), 'cerbero', texture='25_cerbero')

# -- a Planície: o campo de asfódelos, e cada sombra comum vaga perto do que era dela --
PLANICIE_LIVRE = [(17.5, 3.1, 24, 2.8), (39.1, -4.5, 2.8, 10), (39.5, -10.5, 11, 11.5), (21.5, -12.8, 17, 6.6)]
f.flora('Asfodelos', 'asfodelo', (33.5, 0), (32, 25), 1500, avoid=PLANICIE_LIVRE, clump=0.75)
f.flora('CapimPlanicie', 'capim', (33.5, 0), (32, 25), 1100, avoid=PLANICIE_LIVRE[:3], clump=0.3)
f.flora('CapimMargem', 'capim', (10, 12.2), (68, 1.6), 260, clump=0.2)
f.piece('PedraPlanicie1', 'rocha', (22, 0, 8), size=(1.6, 1.1, 1.3))
f.piece('PedraPlanicie2', 'rocha', (41.5, 0, 9), size=(2.2, 1.5, 1.8))
f.piece('PedraPlanicie3', 'rocha', (36, 0, 8.5), size=(1.2, 0.8, 1.1))
# O soldado: covas de campanha em fila.
for i in range(5):
    f.piece(f'CovaSoldado{i + 1}', 'tumulo', (19.6 + i * 1.5, 0, -9.4 + (i % 2) * 0.5), rot=8 * (i - 2), variant=i)
f.wanderer('SombraSoldado', (22, 0, -6), 'planicie_soldado', 'soldado', (6, 0, 0))
# A lavadeira: o varal dela.
f.piece('VaralLavadeira', 'varal', (24.6, 0, 8.4), rot=15, size=(3.6, 0, 0))
f.wanderer('SombraLavadeira', (27, 0, 6), 'planicie_lavadeira', 'lavadeira', (0, 0, -5))
# O pastor: o cercado vazio.
f.piece('CercadoN', 'cerca', (36, 0, -4.6), size=(5, 0.9, 0))
f.piece('CercadoS', 'cerca', (36.6, 0, -0.8), size=(3.8, 0.9, 0))
f.piece('CercadoO', 'cerca', (33.5, 0, -2.7), rot=90, size=(3.8, 0.9, 0))
f.wanderer('SombraPastor', (33, 0, -2), 'planicie_pastor', 'pastor', (4, 0, 4))
# Cérbero combatido até o fim (destruído ou absorvido): a Planície fica agitada, e uma camponesa vaga perto do muro (05/nov/2026).
f.wanderer('SombraCamponesa', (21, 0, 1), 'planicie_camponesa', 'camponesa', (3, 0, 4))
# A aldeia, no fundo norte da Planície: três casas, o poço e uma árvore morta.
f.piece('CasaAldeia1', 'casa', (24.5, 0, -11), size=(4, 2.7, 3.2))
f.piece('CasaAldeia2', 'casa', (30, 0, -11.2), size=(4.4, 2.6, 3.2), variant=1)
f.piece('CasaAldeia3', 'casa', (35.6, 0, -10.8), size=(3.8, 2.6, 3.2), variant=2)
f.piece('PocoAldeia', 'poco', (28.4, 0, -7.4))
f.piece('ArvoreAldeia', 'arvore_morta', (33, 0, -7.6), size=(0, 3.6, 0))
f.piece('MesaAldeia', 'mesa', (25.2, 0, -7.6), rot=12)

# -- o mercado, no fundo da Planície: as barracas ao norte e a oeste, o braseiro no meio; o Mercador (vinho e leite,
# que o Altar não tem) e a Lia, que desenha rostos --
f.piece('BarracaMercador', 'barraca', (42, 0, -8.4), size=(2.9, 2.4, 1.9), tint=TERRACOTA)
f.piece('BarracaPao', 'barraca', (45.4, 0, -8.6), size=(2.7, 2.3, 1.9), tint=AZUL, variant=1)
f.piece('BarracaPanos', 'barraca', (48.6, 0, -8.3), size=(2.5, 2.4, 1.9), tint=OURO, variant=2)
f.piece('BarracaLouca', 'barraca', (40.4, 0, -3.4), rot=90, size=(2.8, 2.3, 1.9), tint=OSSO, variant=3)
f.piece('BraseiroMercado', 'braseiro', (45, 0, -4.4), size=(1.5, 0, 9))
f.piece('MesaMercado', 'mesa', (48.4, 0, -2.6), rot=80)
f.piece('TralhaMercado', 'tralha', (48.8, 0, -5.4), size=(1.2, 0, 1.4), variant=1)
f.piece('CarrocaMercado', 'carroca', (43, 0, 0.6), rot=-25, variant=0)
f.piece('TochaMercado', 'tocha', (40.2, 0, -0.4), size=(1.1, 0, 6.5))
f.examinable('Mercado', (44, 0, -5.5), 'obj.asfodelos.mercado')
f.merchant('Mercador', (42, 0, -6.6), 'mercador', '32_mercador_planicie', idle='mercador_planicie')
f.companion('Lia', (46.5, 0, 2.5), 'lia', '23_lia')
f.piece('BancoLia', 'caixote', (47.6, 0, 2.2), size=(0.55, 0, 0))
f.piece('MargemLestePedras', 'entulho', (50.2, 0, 4), rot=90, size=(8, 0, 1))
f.piece('MargemLesteCerca', 'cerca', (50.3, 0, -7), rot=90, size=(11, 0.8, 0))

# -- o Poço das Sem-Nome, a oeste da Rua dos Ofícios por uma brecha na muralha (Ato II, 06/nov/2026): o fosso escuro,
# as inscrições com os pedaços do nome dela (decisão do João, 06/nov/2026: o nome está espalhado; só com os quatro a
# Escuta revela o nome) e a Sem-Nome, chefe opcional, à beira do fosso. --
f.piece('Fosso', 'fosso', (-26, 0, 1), size=(5.6, 0, 0))
f.blocker('PocoBarreira', (5, 2, 5), (-26, 1, 1))
f.examinable('PocoOlhar', (-22.5, 0, 1), 'obj.poco.entrada')
for i, (x, z, giro) in enumerate(((-30, -9, 20), (-31, 8, 60), (-19.5, 10.5, -30), (-21, -10.5, 5))):
    f.piece(f'Estela{i + 1}', 'estela', (x, 0, z - 0.7), rot=giro, size=(0, 1.3 + 0.1 * i, 0))
    f.examinable(f'Inscricao{i + 1}', (x, 0, z), f'obj.poco.nome.{i + 1}', flag=f'sem_nome_pedaco_{i + 1}')
f.encounter('SemNome', (-26, 0, -3.5), 'asfodelos_sem_nome', texture='39_sem_nome')
f.light('LuzPoco', (-26, 3, 0), '0.5, 0.6, 0.75, 1', 0.9, 13)
f.piece('ParedaoPocoNorte', 'penhasco', (-25, 0, -16), size=(19, 12, 3.5), tint='0.85, 0.9, 1, 1')
f.piece('ParedaoPocoOeste', 'penhasco', (-36, 0, -1), rot=90, size=(30, 10, 3.5), tint='0.85, 0.9, 1, 1')
for i, (x, z) in enumerate(((-30.5, -4), (-22, 6.5), (-29, 11), (-20, -9))):
    f.piece(f'ArvorePoco{i + 1}', 'arvore_morta', (x, 0, z), size=(0, 3 + (i % 2) * 0.8, 0))
f.piece('EntulhoPoco1', 'entulho', (-23, 0, -5), size=(2.4, 0, 2))
f.piece('EntulhoPoco2', 'entulho', (-29.5, 0, 4.5), size=(2, 0, 2.4))

# -- a névoa rasteira: rala na cidade, mais espessa no campo e densa (e mais fria) no Poço --
f.mist('NevoaCidade', (0, 0), (33, 27), height=0.35, density=0.28)
f.mist('NevoaPlanicie', (33.5, 0), (34, 27), height=0.45, density=0.6)
f.mist('NevoaPlanicieAlta', (33.5, -4), (34, 19), height=1.1, density=0.3)
f.mist('NevoaPoco', (-25, 0), (17, 27), height=0.4, density=0.85, tint='0.18, 0.23, 0.31, 1')
f.mist('NevoaRio', (6, 20), (90, 14), height=0.1, density=0.55)

f.light('LuzAltar', (11, 2.5, -3), '0.95, 0.8, 0.55, 1', 0.5, 8)
f.markers({
    'ChegadaAqueronte': (0, 0, 10),
    'Trivia': (1.5, 0, -8),
    'Padeiro': (-9.5, 0, 2.5),
    'Oleiro': (-11, 0, -1.6),
    'Coveiro': (-9.5, 0, 7.5),
    'Altar': (11, 0, -1),
    'Dimas': (-12.5, 0, 5),
    'Calias': (13, 0, 0.5),
    'Planicie': (18, 0, 0),
    'EntradaPlanicie': (14.5, 0, 4.5),
    'Cerbero': (17, 0, 4.5),
    'Soldado': (22, 0, -6),
    'Lavadeira': (27, 0, 6),
    'Pastor': (33, 0, -2),
    'Camponesa': (21, 0, 1),
    'Mercado': (44, 0, -4),
    'Mercador': (42, 0, -5.4),
    'Lia': (45.5, 0, 3.4),
    'AtravessarAqueronte': (0, 0, 13),
    'Viuva': (-7, 0, -1),
    'Poco': (-20, 0, -6.5),
    'SemNome': (-26, 0, -3.5),
    'Inscricao1': (-29.5, 0, -8),
    'SaidaLuto': (-6, 0, -10.5),
    'SaidaTribunal': (0, 0, -10.5),
    'SaidaGuerreiros': (6, 0, -10.5),
    'AtravessarLuto': (-6, 0, -12.6),
    'AtravessarTribunal': (0, 0, -12.6),
    'AtravessarGuerreiros': (6, 0, -12.6),
    'SaidaEstige': (12, 0, -10.5),
    'AtravessarEstige': (12, 0, -12.6),
    'Aldeia': (29, 0, -6),
}, player=(0, 0, 10))
f.write('Asfodelos.tscn')

# ---- Ato II: os três caminhos da Trívia de Asfódelos (06/nov/2026, PENDENCIAS 89; blocos para revisar) ----
# Cada região tem a sua Trívia, o companheiro que se conhece lá (HISTORIA 5.2) e a volta para Asfódelos ao sul.
def ato2(name, ambient, fog, moon, back, back_region='asfodelos'):
    r = Scene(name, ambient, fog, moon)
    r.box('Chao', (22, 1, 26), (0, -0.5, 0), 'Mat_piso')
    r.box('Oeste', (1, 3.5, 27), (-11.5, 1.75, 0))
    r.box('Leste', (1, 0.5, 27), (11.5, 0.25, 0))
    r.box('Norte', (23, 3.5, 1), (0, 1.75, -13.5))
    r.box('SulA', (9.5, 0.5, 1), (-6.75, 0.25, 13.5))
    r.box('SulB', (9.5, 0.5, 1), (6.75, 0.25, 13.5))
    r.exit('ParaAsfodelos' if back_region == 'asfodelos' else 'Voltar', (0, 1, 13.4), (3, 2, 1.2), back_region, back)
    return r

def regiao_hd(name, ambient, fog, moon, back, back_region='asfodelos', chao='Mat_terra', muros=False):
    """A mesma planta do ato2 (22 x 26 m, a volta ao sul) no visual novo (paisagem HD-2D, PENDENCIAS 105 e 109):
    paredões ao norte e a oeste (com muros, uma muralha na frente deles), pedras baixas ao sul e a leste."""
    r = Scene(name, ambient, fog, moon, hd=True)
    r.box('Chao', (22, 1, 26), (0, -0.5, 0), chao)
    r.box('ChaoFora', (90, 1, 90), (0, -0.56, 0), 'Mat_terra', collide=False)
    if muros:
        r.piece('MuralhaOeste', 'muro', (-11.5, 0, 0), rot=90, size=(27, 4.6, 0.9))
        r.piece('MuralhaNorte', 'muro', (0, 0, -13.5), size=(23, 4.6, 0.9))
    r.piece('ParedaoOeste', 'penhasco', (-14.6 if muros else -12.8, 0, 0), rot=90, size=(32, 10, 3))
    r.piece('ParedaoNorte', 'penhasco', (0, 0, -16.6 if muros else -14.8), size=(32, 11, 3))
    r.blocker('Oeste', (1, 3.5, 27), (-11.5, 1.75, 0))
    r.blocker('Norte', (23, 3.5, 1), (0, 1.75, -13.5))
    pedras(r, 'BordaLeste', (11.5, -12.5), (11.5, 12.5), 11, 0.8)
    pedras(r, 'BordaSulO', (-11, 13.5), (-2.4, 13.5), 5, 0.8)
    pedras(r, 'BordaSulL', (2.4, 13.5), (11, 13.5), 5, 0.8)
    r.blocker('Leste', (1, 2, 27), (11.5, 1, 0))
    r.blocker('SulA', (9.5, 2, 1), (-6.75, 1, 13.5))
    r.blocker('SulB', (9.5, 2, 1), (6.75, 1, 13.5))
    r.exit('ParaAsfodelos' if back_region == 'asfodelos' else 'Voltar', (0, 1, 13.4), (3, 2, 1.2), back_region, back)
    return r


# Campos do Luto: bosque de murtas, os que morreram de amor e de saudade; Ismena se conhece aqui.
# Visual novo (12/nov/2026, PENDENCIAS 109): murtas de copa escura com flores pálidas, estelas onde se chora, capim
# alto e névoa; a trilha de terra sobe do sul até a clareira da Trívia.
l = regiao_hd('Luto', '0.5, 0.56, 0.62, 1', '0.3, 0.36, 0.42, 1', 1.3, 'SaidaLuto', chao='Mat_campo')
l.ground('Trilha', 'terra', (0, 8.5), (3.6, 10), edge=1.6)
l.ground('TrilhaMeio', 'terra', (-2.5, -1.5), (4.4, 12), edge=2)
l.ground('Clareira', 'terra', (-5, -9.5), (7, 5.5), edge=2)
for i, (x, z, h) in enumerate(((-7, -6, 3), (-4, 4, 2.6), (6, -8, 2.9), (7, 3, 2.2), (-8, 8, 2.8), (2, -2, 2.5),
                                (-9.4, -10.6, 3.5), (-9.6, -2.4, 3.2), (-1.5, -11.6, 3.4), (3, -11.2, 3.2),
                                (8.6, -11, 3), (-9.2, 3.4, 3), (9.6, -3.4, 2.4), (-6.4, 11, 2.4))):
    l.piece(f'Murta{i + 1}', 'murta', (x, 0, z), size=(0, h, 0))
# Onde se chora: três estelas tortas no capim.
l.piece('EstelaChoro1', 'estela', (6.4, 0, -6), rot=-25, size=(0, 1.4, 0), tint='0.7, 0.72, 0.78, 1')
l.piece('EstelaChoro2', 'estela', (7.6, 0, -4.6), rot=-50, size=(0, 1.1, 0), tint='0.62, 0.64, 0.7, 1')
l.piece('EstelaChoro3', 'estela', (4.2, 0, -7.2), rot=10, size=(0, 1.2, 0), tint='0.66, 0.68, 0.74, 1')
l.piece('EstelaTrilha', 'estela', (-5.6, 0, 0.4), rot=60, size=(0, 1, 0), tint='0.62, 0.64, 0.7, 1')
l.piece('PedraMusgo1', 'rocha', (-8.6, 0, -7.6), size=(1.6, 0.9, 1.3))
l.piece('PedraMusgo2', 'rocha', (4.6, 0, 9.4), size=(1.3, 0.6, 1.1))
TRILHAS_LUTO = [(-1.8, 3.5, 3.6, 10), (-4.7, -7.5, 4.4, 12), (-8.5, -12.2, 7, 5.5)]
l.flora('Capim', 'capim', (0, 0), (21, 25), 520, avoid=TRILHAS_LUTO, clump=0.6)
l.flora('Asfodelos', 'asfodelo', (1, 1), (19, 22), 150, avoid=TRILHAS_LUTO, clump=0.5)
l.examinable('Murtas', (-2.5, 0, 5), 'obj.luto.murtas')
l.examinable('Choro', (5, 0, -5.5), 'obj.luto.choro')
l.companion('Ismena', (3, 0, 6), 'ismena', '36_ismena')
l.wanderer('SombraNoiva', (-3, 0, -4), 'luto_noiva', 'camponesa', (4, 0, 0))
l.trivia('Trivia', (-6, 0, -10), 'trivia_luto')
l.light('LuzBosque', (0, 4, 0), '0.5, 0.6, 0.7, 1', 1.6, 16)
l.light('LuzChoro', (5.5, 3, -5.5), '0.6, 0.7, 0.8, 1', 1.2, 9)
l.mist('Nevoa', (0, 0), (23, 27), height=0.35, density=0.6, tint='0.2, 0.25, 0.3, 1')
l.mist('NevoaAlta', (-2, -4), (22, 18), height=1.3, density=0.35, tint='0.2, 0.25, 0.3, 1')
l.markers({'ChegadaAsfodelos': (0, 0, 10.5), 'Trivia': (-4.5, 0, -10), 'Ismena': (3.5, 0, 7), 'Noiva': (-3, 0, -4),
           'Choro': (5, 0, -4.4), 'AtravessarAsfodelos': (0, 0, 13.4)}, player=(0, 0, 10.5))
l.write('Luto.tscn')

# O Tribunal: a pedra onde as sombras são julgadas; Septímia se conhece aqui.
# Visual novo (12/nov/2026, PENDENCIAS 109): pátio de lajes entre muralhas, o estrado de mármore com os três tronos
# dos juízes, colunas no fundo e dois braseiros; a pedra do julgamento fica ao pé do estrado.
tr = regiao_hd('Tribunal', '0.54, 0.53, 0.54, 1', '0.34, 0.33, 0.34, 1', 1.2, 'SaidaTribunal', chao='Mat_laje', muros=True)
tr.ground('TerraSul', 'terra', (4, 10), (12, 6), edge=2)
tr.ground('TerraOeste', 'terra', (-8, 3), (6, 9), edge=2)
tr.piece('Estrado', 'estrado', (0, 0, -9.8), size=(11, 0.6, 4.6))
tr.piece('TronoMinos', 'trono', (0, 0.6, -11), size=(1.5, 2.8, 0))
tr.piece('TronoRadamanto', 'trono', (-3.4, 0.6, -10.8), size=(1.3, 2.3, 0))
tr.piece('TronoEaco', 'trono', (3.4, 0.6, -10.8), size=(1.3, 2.3, 0))
tr.piece('PedraJulgamento', 'rocha', (0, 0, -6.9), size=(1.7, 0.6, 1.1), tint='0.85, 0.85, 0.9, 1')
for i, x in enumerate((-9.4, -6.6, 6.6, 9.4)):
    tr.piece(f'Coluna{i + 1}', 'coluna', (x, 0, -12.2), size=(0.9, 4.6, 0))
for i, (x, z, quebrada) in enumerate(((-9.6, -5, 0), (-9.6, 2, 1), (-9.6, 9, 0), (9.4, -6, 1))):
    tr.piece(f'ColunaPatio{i + 1}', 'coluna', (x, 0, z), size=(0.8, 4, 0), variant=quebrada)
tr.piece('BraseiroOeste', 'braseiro', (-6.6, 0, -7), size=(0.9, 0, 7))
tr.piece('BraseiroLeste', 'braseiro', (6.6, 0, -7), size=(0.9, 0, 7))
tr.piece('Entulho1', 'entulho', (8.6, 0, -3), size=(1.8, 0, 1.6))
tr.piece('Entulho2', 'entulho', (-8.4, 0, 11), size=(2, 0, 1.6))
tr.tribunal('Pedra', (0, 0, -6.5), 'julgamento_padeiro')
# O Plano (2ª Pesada) aflora junto da pedra, depois de conhecer Septímia (06/nov/2026).
tr.heavy_place('PesadaPlano', (2.5, 0, -5), 'o_plano')
tr.examinable('Juizes', (-3, 0, -7), 'obj.tribunal.juizes')
tr.companion('Septimia', (-6, 0, 4), 'septimia', '37_septimia')
tr.wanderer('SombraPerjuro', (5, 0, 0), 'tribunal_perjuro', 'mercador', (-3, 0, 3))
tr.trivia('Trivia', (7, 0, 6), 'trivia_tribunal')
tr.flora('CapimFrestas', 'capim', (2, 6), (18, 13), 110, clump=0.8)
tr.mist('Nevoa', (0, 0), (23, 27), height=0.3, density=0.45, tint='0.22, 0.22, 0.25, 1')
tr.markers({'ChegadaAsfodelos': (0, 0, 10.5), 'Pedra': (0, 0, -5.5), 'Trivia': (5.5, 0, 6), 'Septimia': (-5.5, 0, 5), 'Perjuro': (5, 0, 0), 'PesadaPlano': (2.5, 0, -4),
            'AtravessarAsfodelos': (0, 0, 13.4)}, player=(0, 0, 10.5))
tr.write('Tribunal.tscn')

# O Campo dos Guerreiros: armas fincadas no chão e o Campeão no fundo; Aulo se conhece aqui.
# Visual novo (12/nov/2026, PENDENCIAS 109): terra pisada, lanças, espadas e escudos fincados por todo o campo, a arena
# de lajes do Campeão cercada de varas, com dois troféus na entrada e braseiros no fundo.
g = regiao_hd('Guerreiros', '0.6, 0.57, 0.55, 1', '0.36, 0.33, 0.32, 1', 1.5, 'SaidaGuerreiros')
g.ground('Arena', 'laje', (0, -8), (9, 7), edge=1.2, tint='0.8, 0.76, 0.72, 1')
g.ground('CapimOeste', 'campo', (-6.5, 4), (8, 12), edge=2.6)
g.ground('CapimLeste', 'campo', (6.5, 5), (8, 11), edge=2.6)
for i, (x, z, tipo) in enumerate(((-8, 2, 0), (-5, -3, 1), (4, 5, 0), (8, -2, 2), (-2, 8, 0), (6, 9, 1),
                                   (-9.4, -4.4, 0), (-7.6, -1.2, 2), (-3.4, 2.6, 0), (-9, 6.4, 1), (-4.6, 10.6, 2),
                                   (2.6, 1.4, 2), (7.2, 1.6, 0), (9.6, 4.4, 0), (3.2, 10.8, 0), (8.8, -7.6, 1),
                                   (-8.4, 10, 0), (9.4, 10.6, 2))):
    g.piece(f'Arma{i + 1}', 'arma', (x, 0, z), size=(0, 1.9 if tipo != 2 else 1.6, 0), variant=tipo)
g.piece('TrofeuOeste', 'arma', (-4.2, 0, -4.2), size=(0, 2.5, 0), variant=3)
g.piece('TrofeuLeste', 'arma', (4.2, 0, -4.2), size=(0, 2.5, 0), variant=3)
g.piece('CercaNorte', 'cerca', (0, 0, -11.8), size=(9.4, 1.1, 0))
g.piece('CercaOeste', 'cerca', (-4.9, 0, -8.6), rot=90, size=(6, 1.1, 0))
g.piece('BraseiroOeste', 'braseiro', (-3.6, 0, -11), size=(0.9, 0, 7))
g.piece('BraseiroLeste', 'braseiro', (3.6, 0, -11), size=(0.9, 0, 7))
g.piece('ArvoreMorta1', 'arvore_morta', (-9.2, 0, -9.6), size=(0, 3.8, 0))
g.piece('ArvoreMorta2', 'arvore_morta', (-9.6, 0, 3.6), size=(0, 3.2, 0))
g.piece('Rocha1', 'rocha', (-7.4, 0, -11.4), size=(2.2, 1.3, 1.6))
g.piece('Rocha2', 'rocha', (8.4, 0, -11.6), size=(1.8, 1, 1.4))
g.flora('Capim', 'capim', (0, 4.5), (21, 16), 300, avoid=[(-1.8, 5, 3.6, 8.5)], clump=0.75)
g.light('LuzCampo', (-2, 4, 3), '0.75, 0.68, 0.6, 1', 1.6, 16)
g.light('LuzCampoLeste', (6, 4, 4), '0.75, 0.68, 0.6, 1', 1.2, 12)
g.examinable('Armas', (-4, 0, 1), 'obj.guerreiros.armas')
g.encounter('Campeao', (0, 0, -8), 'guerreiros_campeao', texture='38_campeao')
g.companion('Aulo', (-6, 0, 6), 'aulo', '35_aulo')
g.trivia('Trivia', (7, 0, 7), 'trivia_guerreiros')
g.mist('Nevoa', (0, 0), (23, 27), height=0.3, density=0.45, tint='0.24, 0.22, 0.22, 1')
g.markers({'ChegadaAsfodelos': (0, 0, 10.5), 'Campeao': (0, 0, -8), 'Trivia': (5.5, 0, 7), 'Aulo': (-5.5, 0, 7),
           'Armas': (-4, 0, 2), 'AtravessarAsfodelos': (0, 0, 13.4)}, player=(0, 0, 10.5))
g.write('Guerreiros.tscn')

def fonte(scene, nome, centro):
    """Uma fonte rasa: água parada com um anel de pedras baixas em volta."""
    x, z = centro
    scene.river(f'{nome}Agua', (x, z), (2.7, 2.7), level=0.03, still=True)
    for i in range(8):
        ang = math.radians(45 * i + 20)
        scene.piece(f'{nome}Pedra{i + 1}', 'rocha', (x + 1.75 * math.cos(ang), 0, z + 1.75 * math.sin(ang)),
                    size=(1.2, 0.45, 1))
    scene.blocker(f'{nome}Corpo', (3, 1, 3), (x, 0.5, z))


# ---- Ato III (10/nov/2026, PENDENCIAS 92) ----
# O Estige: chega-se de Asfódelos pelo sul; a deusa (só a voz) à beira do rio oferece os juramentos; jurar abre a
# travessia, ao norte, que é o 3º marco da Maré e leva ao Palácio.
# Visual novo (12/nov/2026, PENDENCIAS 110): a margem de terra escura acaba no rio largo e negro, com névoa grossa; a
# pedra dos juramentos junto do cais, entre duas tochas; árvores mortas e o paredão a oeste.
e = Scene('Estige', '0.42, 0.42, 0.52, 1', '0.24, 0.24, 0.32, 1', 1.0, hd=True)
e.box('Chao', (22, 1, 19), (0, -0.5, 4), 'Mat_terra')
e.box('ChaoFora', (90, 1, 60), (0, -0.56, 24.5), 'Mat_terra', collide=False)
e.ground('Trilha', 'laje', (0, 4), (3.2, 18), edge=1.2, tint='0.7, 0.7, 0.78, 1')
e.ground('Areia', 'campo', (0, -3.6), (21, 3.4), edge=1.2, tint='0.6, 0.6, 0.7, 1')
e.piece('ParedaoOeste', 'penhasco', (-12.8, 0, 2), rot=90, size=(30, 10, 3))
e.blocker('Oeste', (1, 3.5, 20), (-11.5, 1.75, 4))
pedras(e, 'BordaLeste', (11.5, -5), (11.5, 12.5), 8, 0.8)
pedras(e, 'BordaSulO', (-11, 13.5), (-2.4, 13.5), 5, 0.8)
pedras(e, 'BordaSulL', (2.4, 13.5), (11, 13.5), 5, 0.8)
e.blocker('Leste', (1, 2, 20), (11.5, 1, 4))
e.blocker('SulA', (9.5, 2, 1), (-6.75, 1, 13.5))
e.blocker('SulB', (9.5, 2, 1), (6.75, 1, 13.5))
e.exit('ParaAsfodelos', (0, 1, 13.4), (3, 2, 1.2), 'asfodelos', 'SaidaEstige')
e.river('Rio', (0, -22), (80, 33), level=-0.3)
e.blocker('RioBarreira', (22, 2, 1), (0, 1, -6))
e.piece('Cais', 'cais', (0, 0, -7.6), size=(2.4, 0, 4.6))
e.piece('TochaCaisO', 'tocha', (-2, 0, -4.9), size=(1.4, 2.6, 8))
e.piece('TochaCaisL', 'tocha', (2, 0, -4.9), size=(1.4, 2.6, 8))
e.piece('PedraJuramento', 'estela', (-1.7, 0, -3.9), rot=20, size=(0, 2.1, 0), tint='0.5, 0.5, 0.6, 1')
e.piece('Arvore1', 'arvore_morta', (-8.6, 0, -3), size=(0, 3.8, 0))
e.piece('Arvore2', 'arvore_morta', (-9.4, 0, 6), size=(0, 3.2, 0))
e.piece('Arvore3', 'arvore_morta', (8.4, 0, -3.6), size=(0, 2.6, 0))
e.piece('Rocha1', 'rocha', (-6, 0, -4.6), size=(2.2, 1.1, 1.6))
e.piece('Rocha2', 'rocha', (6, 0, 2.6), size=(1.6, 0.8, 1.3))
e.piece('Estalagmite1', 'estalagmite', (-9.6, 0, 1), size=(1.4, 2.2, 0))
e.flora('Juncos', 'capim', (0, -4), (21, 2.6), 200, avoid=[(-1.6, -5.4, 3.2, 3)], clump=0.2)
e.flora('Capim', 'capim', (0, 5), (21, 16), 200, avoid=[(-1.7, -4, 3.4, 18)], clump=0.75)
e.examinable('RioOlhar', (-5, 0, -3), 'obj.estige.rio')
e.talkable('Estige', (0, 0, -3), 'estige')
e.exit('ParaPalacio', (0, 1, -4.8), (3, 2, 0.8), 'palacio', 'ChegadaEstige', path_key='estige',
       blocked='obj.estige.travessia.fechada', milestone='cruzar_estige')
e.trivia('Trivia', (7, 0, 6), 'trivia_estige')
# Maré do Ato III (11/nov/2026, PENDENCIAS 96): o rio sobe até a margem; o desertor caminha por ela.
e.river('MargemMare4', (0, -4), (22, 3), level=0.03, still=True)
e.tide(tide_min=4)
e.wanderer('SombraDesertor', (-6, 0, 2), 'estige_desertor', 'desertor', (0, 0, 5))
e.light('LuzRio', (0, 3, -6), '0.55, 0.6, 0.75, 1', 1.0, 14)
e.mist('NevoaMargem', (0, 4), (23, 20), height=0.35, density=0.5, tint='0.18, 0.18, 0.26, 1')
e.mist('NevoaRio', (0, -18), (70, 24), height=0.05, density=0.85, tint='0.18, 0.18, 0.26, 1')
e.mist('NevoaRioAlta', (0, -20), (70, 18), height=1.1, density=0.55, tint='0.18, 0.18, 0.26, 1')
e.markers({'ChegadaAsfodelos': (0, 0, 10.5), 'ChegadaPalacio': (0, 0, -1), 'Estige': (0, 0, -1.6), 'Trivia': (5.5, 0, 6), 'Desertor': (-6, 0, 2),
           'AtravessarPalacio': (0, 0, -4.8), 'AtravessarAsfodelos': (0, 0, 13.4)}, player=(0, 0, 10.5))
e.write('Estige.tscn')

# O Palácio: os tronos ao norte (Hades, só a voz, e Perséfone), o jardim a oeste (a romãzeira e Ascálafo sob a pedra);
# a escada para Hipno a leste, que pede o caminho da descida (o favor de Hades, o jardim ou a escada dos jardineiros).
# Visual novo (12/nov/2026, PENDENCIAS 110): salão de lajes entre muralhas, o estrado com o trono escuro de Hades e o
# claro de Perséfone, colunas e braseiros; a oeste, o jardim de ciprestes com a romãzeira, o único vermelho do lugar.
pa = regiao_hd('Palacio', '0.52, 0.48, 0.5, 1', '0.3, 0.26, 0.28, 1', 1.1, 'ChegadaPalacio', 'estige', chao='Mat_laje', muros=True)
pa.ground('Jardim', 'campo', (-7.6, 4.5), (7, 17), edge=2.2)
pa.piece('Estrado', 'estrado', (2, 0, -11), size=(8.5, 0.6, 3.4))
pa.piece('TronoHades', 'trono', (0.5, 0.6, -11.7), size=(1.6, 3, 0), tint='0.36, 0.36, 0.42, 1')
pa.piece('TronoPersefone', 'trono', (3.5, 0.6, -11.7), size=(1.3, 2.4, 0), tint='0.95, 0.9, 0.84, 1')
pa.piece('BraseiroTronoO', 'braseiro', (-3, 0, -10.6), size=(0.9, 0, 7))
pa.piece('BraseiroTronoL', 'braseiro', (7, 0, -10.6), size=(0.9, 0, 7))
for i, x in enumerate((-9.6, -6.4, 9.6)):
    pa.piece(f'Coluna{i + 1}', 'coluna', (x, 0, -12.3), size=(0.9, 4.6, 0))
for i, (x, z, quebrada) in enumerate(((-4.2, -3.6, 0), (-4.2, 6.4, 0), (-4.2, 11.4, 1), (9.6, -7, 0), (9.6, 7.6, 1))):
    pa.piece(f'ColunaSalao{i + 1}', 'coluna', (x, 0, z), size=(0.8, 4, 0), variant=quebrada)
pa.examinable('Tronos', (2, 0, -8.8), 'obj.palacio.trono')
pa.talkable('Hades', (0.5, 0, -9.2), 'hades')
pa.talkable('Persefone', (3.5, 0, -9.2), 'persefone')
for i, (x, z, h) in enumerate(((-8, -2, 5.5), (-9, 4, 5), (-5.6, 8.6, 4.6), (-9.6, -7.6, 6), (-9.4, 10.6, 5))):
    pa.piece(f'Cipreste{i + 1}', 'cipreste', (x, 0, z), size=(0, h, 0))
pa.piece('Romanzeira', 'romazeira', (-6, 0, 2), size=(0, 2.6, 0))
pa.flora('CapimJardim', 'capim', (-7.6, 4.5), (6.4, 16), 170, clump=0.6)
pa.flora('AsfodelosJardim', 'asfodelo', (-7.6, 4.5), (6, 15), 40, clump=0.5)
pa.examinable('Roma', (-4.6, 0, 2.5), 'obj.palacio.roma')
# A pedra já vem desenhada na figura de Ascálafo (11/nov/2026).
pa.talkable('Ascalafo', (-7.4, 0, 9), 'ascalafo', texture='45_ascalafo')
# A Venda (3ª Pesada; decisão do João, 11/nov/2026): aflora diante do trono, depois da audiência com Hades.
pa.heavy_place('PesadaVenda', (2, 0, -7.4), 'a_venda')
# O capítulo 3 das origens (Prova, 11/nov/2026, PENDENCIAS 99): a mesa do pacto, ao lado do estrado.
pa.piece('MesaPacto', 'mesa', (-3.5, 0, -8.5), size=(1.8, 0, 0))
pa.talkable('Prova', (-3.5, 0, -7.4), 'prova')
pa.piece('ArcoDescida', 'arco', (11.5, 0, 0), rot=90, size=(2.8, 3.6, 1.1))
pa.exit('ParaHipno', (11, 1, 0), (0.8, 2, 3), 'hipno', 'ChegadaPalacio', path_key='palacio_descida',
        blocked='obj.palacio.descida.fechada')
pa.trivia('Trivia', (7, 0, 6), 'trivia_palacio')
# Maré do Ato III (11/nov/2026, PENDENCIAS 96): o sacristão vaga pelos corredores, acendendo lamparinas.
pa.wanderer('SombraSacristao', (6, 0, -3), 'palacio_sacristao', 'sacristao', (0, 0, 6))
pa.light('LuzTronos', (2, 3, -9), '0.85, 0.55, 0.45, 1', 1.0, 12)
pa.light('LuzJardim', (-6, 3, 5), '0.6, 0.7, 0.55, 1', 1.3, 12)
pa.mist('Nevoa', (0, 0), (23, 27), height=0.3, density=0.4, tint='0.22, 0.2, 0.22, 1')
pa.markers({'ChegadaEstige': (0, 0, 10.5), 'ChegadaHipno': (9, 0, 0), 'Hades': (0.5, 0, -8), 'Persefone': (3.5, 0, -8),
            'Ascalafo': (-5.5, 0, 9), 'PesadaVenda': (2, 0, -6.4), 'Sacristao': (6, 0, -3), 'Prova': (-3.5, 0, -6.4), 'Trivia': (5.5, 0, 6), 'AtravessarHipno': (11, 0, 0), 'AtravessarEstige': (0, 0, 13.4),
            'Jardim': (-4.6, 0, 3.6)},
           player=(0, 0, 10.5))
pa.write('Palacio.tscn')
# A volta do Palácio vai ao Estige, do lado de cá do rio (o marcador ChegadaPalacio de lá).

# A morada de Hipno: papoulas; Hipno (só a voz), o Sonho no fundo, Tânato (sem luta), O Olhar (4ª Pesada) e, no canto
# leste, o som de água que leva, escondido, a Mnemósine.
# Visual novo (12/nov/2026, PENDENCIAS 110): um campo de papoulas pálidas dentro da gruta, névoa lilás, o leito de
# pedra de Hipno, a clareira do Sonho no fundo e a água parada no canto.
h = regiao_hd('Hipno', '0.6, 0.56, 0.7, 1', '0.3, 0.26, 0.38, 1', 1.5, 'ChegadaHipno', 'palacio', chao='Mat_campo')
h.ground('Trilha', 'terra', (0, 7), (3.4, 12), edge=1.6)
h.ground('ClareiraSonho', 'terra', (0.5, -8.6), (9, 6.5), edge=2)
LIMPO_HIPNO = [(-1.7, 1, 3.4, 12), (-4, -11.8, 9, 6.5)]
h.flora('CampoPapoulas', 'papoula', (0, 0), (21, 25), 760, avoid=LIMPO_HIPNO, clump=0.55)
h.flora('Capim', 'capim', (0, 0), (21, 25), 220, avoid=LIMPO_HIPNO, clump=0.7)
h.piece('LeitoHipno', 'estrado', (-6.6, 0, -8.2), size=(3.4, 0.5, 2.2), tint='0.4, 0.38, 0.48, 1')
h.piece('TabuinhaPedra', 'estela', (-10, 0, 0), rot=90, size=(0, 1.2, 0), tint='0.62, 0.6, 0.7, 1')
h.piece('Estalagmite1', 'estalagmite', (-9.4, 0, -11), size=(1.6, 2.8, 0))
h.piece('Estalagmite2', 'estalagmite', (4.4, 0, -11.8), size=(1.4, 2.2, 0))
h.piece('Estalagmite3', 'estalagmite', (-9.8, 0, 6.4), size=(1.2, 1.8, 0))
h.piece('Rocha1', 'rocha', (8.6, 0, -5), size=(1.8, 0.9, 1.4))
h.piece('ArcoBifurcacao', 'arco', (9, 0, -13.5), size=(2.8, 3.8, 1.1))
h.river('AguaEscondida', (10, 10.8), (2.6, 4), level=0.03, still=True)
h.examinable('Papoulas', (-2, 0, 4), 'obj.hipno.papoulas')
h.examinable('Tabuinha', (-9, 0, 0), 'obj.hipno.tabuinha')
h.talkable('Hipno', (-5, 0, -6), 'hipno')
h.encounter('Sonho', (0, 0, -9), 'hipno_sonho', texture='40_sonho')
h.talkable('Tanato', (6, 0, -7), 'tanato', texture='44_tanato')
h.heavy_place('PesadaOlhar', (-6, 0, 6), 'o_olhar')
h.examinable('Agua', (9, 0, 9), 'obj.hipno.agua', flag='mnemosine_achada')
h.exit('ParaMnemosine', (11, 1, 9), (0.8, 2, 3), 'mnemosine', 'ChegadaHipno', blocked='obj.hipno.agua.fechada',
       condition={'flag': 'mnemosine_achada'})
h.trivia('Trivia', (-8, 0, -10), 'trivia_hipno')
# Ato IV (11/nov/2026, PENDENCIAS 97): depois do Sonho, o caminho segue ao norte, para a bifurcação (4º marco da Maré).
h.exit('ParaBifurcacao', (9, 1, -12.6), (3, 2, 1), 'bifurcacao', 'ChegadaHipno', blocked='obj.hipno.bifurcacao.fechada',
       milestone='bifurcacao',
       condition={'any': [{'shade_state': {'id': 'sonho', 'state': s}} for s in ('destroyed', 'seen', 'absorbed', 'spared')]})
# Maré do Ato III (11/nov/2026, PENDENCIAS 96): as papoulas sobem; o trombeteiro dorme em pé entre elas.
h.flora('PapoulasMare', 'papoula', (0, 0), (21, 25), 520, avoid=LIMPO_HIPNO, clump=0.3)
h.tide(tide_min=4)
h.wanderer('SombraTrombeteiro', (4, 0, 4), 'hipno_trombeteiro', 'trombeteiro', (-5, 0, 0))
h.light('LuzPapoulas', (0, 4, 2), '0.75, 0.62, 0.85, 1', 2.2, 18)
h.light('LuzSonho', (0, 3, -8), '0.6, 0.65, 0.85, 1', 1.0, 10)
h.mist('Nevoa', (0, 0), (23, 27), height=0.35, density=0.4, tint='0.24, 0.2, 0.32, 1')
h.mist('NevoaAlta', (0, -2), (22, 20), height=1.3, density=0.4, tint='0.24, 0.2, 0.32, 1')
h.markers({'ChegadaPalacio': (0, 0, 10.5), 'ChegadaMnemosine': (9, 0, 7), 'Hipno': (-5, 0, -4.8), 'Sonho': (0, 0, -9),
           'Tanato': (6, 0, -5.8), 'PesadaOlhar': (-6, 0, 7), 'Trombeteiro': (4, 0, 4), 'Agua': (8, 0, 8), 'Trivia': (-6.5, 0, -10),
           'AtravessarMnemosine': (11, 0, 9), 'AtravessarPalacio': (0, 0, 13.4), 'Papoulas': (-2, 0, 5),
           'ChegadaBifurcacao': (9, 0, -10.5), 'AtravessarBifurcacao': (9, 0, -12.6)}, player=(0, 0, 10.5))
h.write('Hipno.tscn')

# Mnemósine (secreta): duas fontes iguais; a do cipreste branco (a do Lete, à esquerda) e a fria, com os guardiões.
# Visual novo (12/nov/2026, PENDENCIAS 110): um terraço de lajes com as duas fontes iguais em anéis de pedra, o
# cipreste branco ao lado da esquerda, ciprestes comuns no fundo e névoa clara.
m = regiao_hd('Mnemosine', '0.5, 0.54, 0.6, 1', '0.34, 0.4, 0.46, 1', 1.1, 'ChegadaMnemosine', 'hipno', chao='Mat_campo')
m.ground('Trilha', 'laje', (0, 5.5), (3.2, 15), edge=1.2)
m.ground('Terraco', 'laje', (0, -5.4), (17, 7), edge=1.6)
fonte(m, 'FonteEsquerda', (-5, -6))
m.piece('CipresteBranco', 'cipreste', (-7.8, 0, -7.4), size=(0, 5.6, 0), variant=1)
m.talkable('FonteLete', (-5, 0, -4), 'fonte_lete')
fonte(m, 'FonteDireita', (5, -6))
m.figure('Guardioes', (5, 0, -4.2), '57_guardioes')
m.blocker('GuardioesCorpo', (1.6, 2, 0.4), (5, 1, -4.3))
m.talkable('FonteMnemosine', (5, 0, -3), 'fonte_mnemosine')
for i, (x, z, alt) in enumerate(((-9.8, -11.4, 6), (0, -12, 5.4), (9.4, -11.6, 5), (-9.8, 3, 5))):
    m.piece(f'Cipreste{i + 1}', 'cipreste', (x, 0, z), size=(0, alt, 0))
m.piece('TabuinhaPedra', 'estela', (1.2, 0, 1.2), rot=-20, size=(0, 1.1, 0), tint='0.7, 0.72, 0.78, 1')
m.flora('Capim', 'capim', (0, 2), (21, 22), 300, avoid=[(-1.6, -2, 3.2, 15), (-8.5, -8.9, 17, 7)], clump=0.6)
m.flora('Asfodelos', 'asfodelo', (0, 5), (20, 14), 90, avoid=[(-1.6, -2, 3.2, 15)], clump=0.5)
m.examinable('Tabuinha', (0, 0, 2), 'obj.mnemosine.tabuinha')
m.light('LuzFontes', (0, 3, -5), '0.6, 0.75, 0.85, 1', 1.4, 16)
m.mist('Nevoa', (0, 0), (23, 27), height=0.3, density=0.5, tint='0.26, 0.3, 0.34, 1')
m.markers({'ChegadaHipno': (0, 0, 10.5), 'FonteLete': (-5, 0, -2.6), 'FonteMnemosine': (5, 0, -1.6), 'Tabuinha': (0, 0, 3.5),
           'AtravessarHipno': (0, 0, 13.4)}, player=(0, 0, 10.5))
m.write('Mnemosine.tscn')
# ---- Ato IV (11/nov/2026, PENDENCIAS 97; decisões do João na PENDENCIAS 94) ----
# A bifurcação: à esquerda o Tártaro, à direita os Elísios; o caminho do meio desce ao Vale do Lete, aberto quando os dois
# lados foram vistos e com a Chave de Memória (5º marco da Maré).
# Visual novo (12/nov/2026, PENDENCIAS 111): uma encruzilhada de lajes; a oeste, o muro que o fogo do Tártaro clareia e
# um arco entre braseiros; a leste, a árvore clara e a luz dourada dos Elísios; ao norte, o arco na névoa do Lete.
bi = regiao_hd('Bifurcacao', '0.48, 0.46, 0.5, 1', '0.3, 0.28, 0.32, 1', 1.1, 'ChegadaBifurcacao', 'hipno')
bi.ground('CaminhoSul', 'laje', (0, 5), (3.2, 16), edge=1.2)
bi.ground('CaminhoOeste', 'laje', (-6, 0), (11, 3.2), edge=1.2, tint='0.9, 0.74, 0.66, 1')
bi.ground('CaminhoLeste', 'laje', (6, 0), (11, 3.2), edge=1.2, tint='0.95, 0.92, 0.78, 1')
bi.ground('CaminhoNorte', 'laje', (0, -7.5), (3.2, 12), edge=1.2, tint='0.76, 0.8, 0.88, 1')
bi.examinable('Caminhos', (0, 0, -3), 'obj.bifurcacao.caminhos')
bi.piece('PilarCaminhos', 'estela', (1.9, 0, -2.6), rot=-30, size=(0, 1.6, 0), tint='0.7, 0.7, 0.76, 1')
bi.piece('MuroFogo', 'muro', (-9.5, 0, -6), rot=90, size=(6, 2.6, 1))
bi.piece('ArcoTartaro', 'arco', (-11.6, 0, 0), rot=90, size=(2.8, 3.8, 1.1), tint='0.7, 0.5, 0.45, 1')
bi.piece('BraseiroTartaroN', 'braseiro', (-10, 0, -2.4), size=(0.9, 0, 8))
bi.piece('BraseiroTartaroS', 'braseiro', (-10, 0, 2.4), size=(0.9, 0, 8))
bi.piece('ArvoreMortaOeste', 'arvore_morta', (-8.4, 0, 6), size=(0, 3.4, 0))
bi.piece('EstalagmiteOeste', 'estalagmite', (-9.6, 0, -10.6), size=(1.6, 2.6, 0))
bi.piece('ArvoreClara', 'murta', (9, 0, -6), size=(0, 3.4, 0))
# A leste (lado da câmera) só coisa baixa: dois marcos de mármore no lugar de um arco.
bi.piece('MarcoElisiosN', 'coluna', (10.9, 0, -2), size=(0.7, 2.2, 0))
bi.piece('MarcoElisiosS', 'coluna', (10.9, 0, 2), size=(0.7, 2.2, 0))
bi.flora('AsfodelosLeste', 'asfodelo', (6.5, 2), (8, 20), 130, avoid=[(0.5, -1.6, 11, 3.2)], clump=0.5)
bi.flora('CapimLeste', 'capim', (6.5, 2), (8, 20), 150, avoid=[(0.5, -1.6, 11, 3.2)], clump=0.6)
bi.piece('ArcoLete', 'arco', (0, 0, -13.5), size=(2.8, 4, 1.1))
bi.light('LuzTartaro', (-9, 3, 0), '1, 0.5, 0.25, 1', 1.6, 10)
bi.light('LuzElisios', (9, 3, 0), '1, 0.9, 0.6, 1', 1.8, 11)
bi.light('LuzLete', (0, 3, -10), '0.6, 0.7, 0.9, 1', 1.2, 9)
bi.exit('ParaTartaro', (-11, 1, 0), (0.8, 2, 3), 'tartaro', 'ChegadaBifurcacao')
bi.exit('ParaElisios', (11, 1, 0), (0.8, 2, 3), 'elisios', 'ChegadaBifurcacao')
bi.exit('ParaLete', (0, 1, -12.6), (3, 2, 1), 'vale_lete', 'ChegadaBifurcacao', path_key='vale_do_lete',
        blocked='obj.bifurcacao.lete.fechado', milestone='vale_do_lete',
        condition={'all': [{'region_visited': 'tartaro'}, {'region_visited': 'elisios'}]})
bi.trivia('Trivia', (7, 0, 6), 'trivia_bifurcacao')
bi.mist('Nevoa', (0, 0), (23, 27), height=0.3, density=0.4, tint='0.22, 0.22, 0.26, 1')
bi.mist('NevoaLete', (0, -9), (10, 9), height=1.1, density=0.55, tint='0.24, 0.28, 0.36, 1')
bi.markers({'ChegadaHipno': (0, 0, 10.5), 'ChegadaTartaro': (-9, 0, 0), 'ChegadaElisios': (9, 0, 0), 'ChegadaLete': (0, 0, -10.5),
            'Caminhos': (0, 0, -1.8), 'Trivia': (5.5, 0, 6), 'AtravessarTartaro': (-11, 0, 0), 'AtravessarElisios': (11, 0, 0),
            'AtravessarLete': (0, 0, -12.6), 'AtravessarHipno': (0, 0, 13.4)}, player=(0, 0, 10.5))
bi.write('Bifurcacao.tscn')

# O Tártaro: o Flegetonte (fogo) atravessa o campo; os condenados; no portão de ferro, as Fúrias.
# Visual novo (12/nov/2026, PENDENCIAS 111): chão de terra queimada entre muralhas, o Flegetonte como um rio de fogo
# que clareia tudo por baixo, estalagmites, árvores mortas e o portão escuro ao norte, entre dois braseiros.
ta = regiao_hd('Tartaro', '0.5, 0.34, 0.3, 1', '0.36, 0.18, 0.14, 1', 0.7, 'ChegadaTartaro', 'bifurcacao', muros=True)
ta.ground('Trilha', 'laje', (0, 3), (3.2, 20), edge=1.2, tint='0.7, 0.6, 0.56, 1')
ta.ground('PatioFurias', 'laje', (0, -9.6), (9, 6), edge=1.4, tint='0.7, 0.6, 0.56, 1')
ta.river('Flegetonte', (4, 1), (38, 2.6), level=0.03, fire=True)
for i, (x, z) in enumerate(((-9.6, -0.6), (-6, 2.7), (-3.4, -0.7), (3.6, 2.7), (6.4, -0.7), (9.6, 2.6))):
    ta.piece(f'PedraFogo{i + 1}', 'rocha', (x, 0, z), size=(1.3, 0.5, 0.9), tint='0.5, 0.4, 0.38, 1')
ta.light('LuzFogo', (0, 2, 1), '1, 0.5, 0.2, 1', 2.2, 14)
ta.light('LuzFogoOeste', (-8, 2, 1), '1, 0.5, 0.2, 1', 1.8, 10)
ta.light('LuzFogoLeste', (8, 2, 1), '1, 0.5, 0.2, 1', 1.8, 10)
ta.piece('Portao', 'arco', (0, 0, -12.9), size=(4, 5, 1.3), tint='0.4, 0.38, 0.4, 1')
ta.piece('PortaoVeu', 'veu', (0, 2, -12.8), size=(4, 4, 0.95), tint='0.06, 0.05, 0.05, 1')
ta.blocker('PortaoBarreira', (6, 3.5, 0.6), (0, 1.75, -12.6))
ta.piece('BraseiroPortaoO', 'braseiro', (-3.6, 0, -11.6), size=(0.9, 0, 8))
ta.piece('BraseiroPortaoL', 'braseiro', (3.6, 0, -11.6), size=(0.9, 0, 8))
for i, (x, z, alt) in enumerate(((-9.8, -11, 3), (-4.6, -11.8, 2), (8.8, -11.4, 2.6), (-9.8, 9.6, 2.2), (9.6, -3.4, 1.6))):
    ta.piece(f'Estalagmite{i + 1}', 'estalagmite', (x, 0, z), size=(1.4, alt, 0))
ta.piece('ArvoreMorta1', 'arvore_morta', (-9.4, 0, -2.6), size=(0, 3.6, 0))
ta.piece('ArvoreMorta2', 'arvore_morta', (-4.4, 0, 9.6), size=(0, 2.8, 0))
ta.piece('Entulho1', 'entulho', (5, 0, -3.4), size=(2, 0, 1.6))
ta.encounter('Furias', (0, 0, -9.5), 'tartaro_furias', texture='41_furias')
ta.examinable('Sisifo', (-7.6, 0, -5.6), 'obj.tartaro.sisifo', texture='54_sisifo')
ta.blocker('SisifoPedra', (2.2, 2, 1), (-7.6, 1, -5.9))
ta.examinable('Tantalo', (7, 0, -5.4), 'obj.tartaro.tantalo', texture='55_tantalo')
ta.blocker('TantaloPoca', (1.6, 2, 0.8), (7, 1, -5.6))
ta.examinable('Danaides', (-7, 0, 5.6), 'obj.tartaro.danaides', texture='56_danaides')
ta.blocker('DanaidesCorpo', (1.2, 2, 0.6), (-7, 1, 5.4))
ta.wanderer('SombraAncora', (5, 0, 5), 'tartaro_ancora', 'ancora', (-6, 0, 0))
ta.trivia('Trivia', (7, 0, 8), 'trivia_tartaro')
ta.mist('Nevoa', (0, 0), (23, 27), height=0.3, density=0.4, tint='0.2, 0.1, 0.08, 1')
ta.markers({'ChegadaBifurcacao': (0, 0, 10.5), 'Furias': (0, 0, -8), 'Sisifo': (-7, 0, -4), 'Tantalo': (7, 0, -4),
            'Danaides': (-7, 0, 7), 'Ancora': (5, 0, 5), 'Trivia': (5.5, 0, 8), 'Flegetonte': (0, 0, 3.6),
            'AtravessarBifurcacao': (0, 0, 13.4)},
           player=(0, 0, 10.5))
ta.write('Tartaro.tscn')

# Os Campos Elísios: luz baixa e dourada; Anquises (só a voz) à beira do rio onde as almas bebem para voltar; A Mentira.
# Visual novo (12/nov/2026, PENDENCIAS 111): prado claro de asfódelos sob luz dourada, árvores de copa cheia, colunas
# brancas inteiras e o rio raso das almas entre pedras.
el = regiao_hd('Elisios', '0.74, 0.7, 0.56, 1', '0.5, 0.48, 0.38, 1', 1.9, 'ChegadaElisios', 'bifurcacao', chao='Mat_campo')
el.ground('Trilha', 'laje', (0, 6), (3.2, 14), edge=1.4, tint='1, 0.96, 0.84, 1')
el.ground('BeiraRio', 'laje', (1, -4.4), (13, 3.4), edge=1.6, tint='1, 0.96, 0.84, 1')
for i, (x, z, alt) in enumerate(((-8, -6, 3.6), (-4, 6, 3), (6, -10.6, 3.6), (8, 4, 2.4), (-8, 9, 3), (-9.4, 0.6, 3.4),
                                  (-3, -11.4, 3.6), (9.6, -3, 2.4))):
    el.piece(f'Loureiro{i + 1}', 'murta', (x, 0, z), size=(0, alt, 0))
for i, (x, z) in enumerate(((-6, -11.6), (-9.8, -9), (-9.8, 5))):
    el.piece(f'Coluna{i + 1}', 'coluna', (x, 0, z), size=(0.8, 4, 0))
el.river('RioAlmas', (5, -7), (13, 2.6), level=0.03, still=True)
for i, (x, z) in enumerate(((-1.6, -7), (0.6, -8.5), (4, -8.6), (8, -8.6), (1, -5.6), (5, -5.5), (9, -5.5))):
    el.piece(f'PedraRio{i + 1}', 'rocha', (x, 0, z), size=(1.2, 0.4, 0.9), tint='0.95, 0.92, 0.85, 1')
el.blocker('RioCorpo', (12, 1, 2.2), (5, 0.5, -7))
LIMPO_ELISIOS = [(-1.6, -1, 3.2, 14), (-5.5, -8.6, 17, 5)]
el.flora('Asfodelos', 'asfodelo', (0, 1), (21, 24), 520, avoid=LIMPO_ELISIOS, clump=0.45)
el.flora('Capim', 'capim', (0, 1), (21, 24), 320, avoid=LIMPO_ELISIOS, clump=0.6)
el.examinable('Almas', (4, 0, -5), 'obj.elisios.almas')
el.talkable('Anquises', (-2, 0, -5), 'anquises')
el.heavy_place('PesadaMentira', (-5, 0, 1), 'a_mentira')
el.wanderer('SombraOferente', (3, 0, 4), 'elisios_oferente', 'oferente', (-5, 0, 0))
el.trivia('Trivia', (7, 0, 8), 'trivia_elisios')
el.light('LuzDourada', (0, 5, 0), '1, 0.88, 0.6, 1', 2.0, 20)
el.mist('Nevoa', (0, 0), (23, 27), height=0.3, density=0.25, tint='0.5, 0.46, 0.34, 1')
el.markers({'ChegadaBifurcacao': (0, 0, 10.5), 'Anquises': (-2, 0, -3.8), 'Almas': (4, 0, -4), 'PesadaMentira': (-5, 0, 2),
            'Oferente': (3, 0, 4), 'Trivia': (5.5, 0, 8), 'AtravessarBifurcacao': (0, 0, 13.4)}, player=(0, 0, 10.5))
el.write('Elisios.tscn')

# O Vale do Lete: o rio do esquecimento; a Cadeira do Esquecimento (O Gole); no fundo, a Sombra do Esquecido; depois
# dela, os Portões.
# Visual novo (12/nov/2026, PENDENCIAS 111): vale de capim e juncos coberto de névoa clara, o Lete raso e parado
# cortando o caminho, a Cadeira de pedra a oeste, ciprestes e o arco dos Portões ao norte.
vl = regiao_hd('ValeLete', '0.46, 0.5, 0.58, 1', '0.3, 0.34, 0.4, 1', 1.0, 'ChegadaLete', 'bifurcacao', chao='Mat_campo')
vl.ground('Trilha', 'terra', (0, 5), (3.4, 15), edge=1.6)
vl.ground('ClareiraSombra', 'terra', (0, -9.4), (9, 6), edge=2)
vl.river('Lete', (4, -4), (38, 3.2), level=0.03, still=True)
vl.flora('Juncos', 'capim', (0, -4), (21, 5), 320, avoid=[(-1.6, -6.5, 3.2, 5)], clump=0.2)
vl.flora('Capim', 'capim', (0, 5), (21, 16), 340, avoid=[(-1.7, -2.5, 3.4, 15)], clump=0.6)
vl.flora('Asfodelos', 'asfodelo', (0, 5), (20, 15), 70, avoid=[(-1.7, -2.5, 3.4, 15)], clump=0.5)
vl.piece('Cadeira', 'trono', (-6, 0, 2), rot=90, size=(1.3, 2, 0), tint='0.66, 0.68, 0.74, 1')
for i, (x, z, alt) in enumerate(((-9.6, -10.6, 6), (-9.4, -1, 5), (8.6, -11.4, 5), (-9.6, 8, 5))):
    vl.piece(f'Cipreste{i + 1}', 'cipreste', (x, 0, z), size=(0, alt, 0))
vl.piece('Rocha1', 'rocha', (-4.6, 0, -6.4), size=(1.8, 0.8, 1.3))
vl.piece('Rocha2', 'rocha', (8.6, 0, -1.6), size=(1.5, 0.6, 1.2))
vl.piece('ArcoPortoes', 'arco', (0, 0, -13.5), size=(2.8, 4, 1.1), variant=1)
vl.talkable('CadeiraEsquecimento', (-4.6, 0, 2.5), 'cadeira')
vl.heavy_place('PesadaGole', (-6, 0, 4), 'o_gole')
vl.encounter('SombraEsquecido', (0, 0, -9), 'lete_sombra', texture='42_esquecido', idle='esquecido_m',
             feminine=('43_esquecido_f', 'esquecido_f'))
# O capítulo 4 das origens (Acerto, PENDENCIAS 99): na margem, entre as Sem-Nome, a pessoa que ele traiu.
vl.talkable('Acerto', (6, 0, -1.5), 'acerto', texture='58_acerto',
            by_origin='res://art/sprites/sombras/acerto_{origem}/parado_3q/64px/acerto_{origem}_parado_3q_esq.png')
vl.exit('ParaPortoes', (0, 1, -12.6), (3, 2, 1), 'portoes', 'ChegadaLete', blocked='obj.lete.portoes.fechados',
        condition={'any': [{'flag': 'trocou_lugar'}] + [{'shade_state': {'id': 'sombra_esquecido', 'state': s}}
                                                         for s in ('destroyed', 'freed', 'absorbed')]})
vl.trivia('Trivia', (7, 0, 8), 'trivia_lete')
vl.light('LuzVale', (0, 4, 0), '0.6, 0.7, 0.85, 1', 1.4, 18)
vl.mist('Nevoa', (0, 0), (23, 27), height=0.35, density=0.7, tint='0.3, 0.34, 0.4, 1')
vl.mist('NevoaAlta', (0, -5), (23, 14), height=1.2, density=0.5, tint='0.3, 0.34, 0.4, 1')
vl.markers({'ChegadaBifurcacao': (0, 0, 10.5), 'ChegadaPortoes': (0, 0, -10.5), 'Cadeira': (-4.6, 0, 3.6),
            'PesadaGole': (-6, 0, 5), 'Sombra': (0, 0, -9), 'Acerto': (6, 0, -0.4), 'Trivia': (5.5, 0, 8), 'AtravessarPortoes': (0, 0, -12.6),
            'Rio': (0, 0, -1.6), 'AtravessarBifurcacao': (0, 0, 13.4)}, player=(0, 0, 10.5))
vl.write('ValeLete.tscn')

# Os Portões: dois portões iguais na forma, um de chifre (fosco) e um de marfim (que brilha).
# Visual novo (12/nov/2026, PENDENCIAS 111): um adro de lajes entre muralhas; os dois arcos lado a lado, iguais na
# forma: o da esquerda fosco e escuro, o da direita claro, com luz própria; colunas pelo caminho.
po = regiao_hd('Portoes', '0.48, 0.48, 0.54, 1', '0.28, 0.28, 0.32, 1', 1.0, 'ChegadaPortoes', 'vale_lete', chao='Mat_laje', muros=True)
po.piece('PortaoChifre', 'arco', (-4, 0, -12.6), size=(2.8, 4.6, 1.2), tint='0.56, 0.47, 0.36, 1')
po.piece('VeuChifre', 'veu', (-4, 1.9, -12.5), size=(2.8, 3.8, 0.9), tint='0.14, 0.12, 0.1, 1')
po.piece('PortaoMarfim', 'arco', (4, 0, -12.6), size=(2.8, 4.6, 1.2), variant=1)
po.piece('VeuMarfim', 'veu', (4, 1.9, -12.5), size=(2.8, 3.8, 0.9), tint='0.8, 0.8, 0.74, 1')
po.light('LuzMarfim', (4, 2.4, -11), '1, 0.97, 0.88, 1', 2.4, 8)
po.blocker('PortoesCorpo', (11, 3.5, 1), (0, 1.75, -12.6))
po.piece('Degraus', 'estrado', (0, 0, -11.2), size=(13, 0.3, 2.4))
for i, (x, z, quebrada) in enumerate(((-3.4, 6, 0), (-3.4, 0, 0), (-3.4, -6, 0), (3.4, 6, 1), (3.4, 0, 1), (-9.6, -11, 0), (9.6, -11, 0))):
    po.piece(f'Coluna{i + 1}', 'coluna', (x, 0, z), size=(0.8, 4.2, 0), variant=quebrada)
po.piece('Entulho1', 'entulho', (6.4, 0, -5), size=(1.8, 0, 1.6))
po.talkable('Portoes', (0, 0, -8), 'portoes')
po.mist('Nevoa', (0, 0), (23, 27), height=0.3, density=0.5, tint='0.22, 0.22, 0.26, 1')
po.markers({'ChegadaLete': (0, 0, 10.5), 'Portoes': (0, 0, -6.8), 'AtravessarLete': (0, 0, 13.4)}, player=(0, 0, 10.5))
po.write('Portoes.tscn')
print('ok')
