"""Gera as cenas de blockout das regiões (Tênaro e Aqueronte) com os materiais da sala de teste.

Uso (da raiz do projeto): ../ferramentas/python/Scripts/python.exe tools/blockout_regioes.py .
Escreve src/Lete.Game/Regions/*.tscn do zero: depois de ajustar uma cena no editor do Godot, não rode de novo
(ou passe o ajuste para cá). Blocos agora, arte depois (decisão do João, 30/out/2026).
"""
import sys, os, json

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
]


def fmt(v):
    return ('%.3f' % v).rstrip('0').rstrip('.') if isinstance(v, float) else str(v)


class Scene:
    def __init__(self, name, ambient, fog_color, moon_energy):
        self.name = name
        self.nodes = []
        self.ambient = ambient
        self.fog_color = fog_color
        self.moon_energy = moon_energy

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

    def talkable(self, name, pos, dialogue, texture=None, idle=None):
        """Alguém com quem conversar (Talkable); sem textura, só a voz. idle: a figura em 3/4 (pasta da sombra) com a
        animação parada de 64 px, olhando para a esquerda."""
        idle_line = (f'IdleFramesPrefix = "res://art/sprites/sombras/{idle}/idle_3q/64px/esq/{idle}_idle_3q_esq_"\n'
                     if idle else '')
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

''' for mid, tex in [('Mat_piso', '4_piso'), ('Mat_parede', '5_parede'), ('Mat_terracota', '6_terracota'), ('Mat_agua', '7_agua')])
        body = f'''[gd_scene format=3]

{ext}

[sub_resource type="Environment" id="Environment_regiao"]
background_mode = 1
background_color = Color(0.05, 0.047, 0.06, 1)
ambient_light_source = 2
ambient_light_color = Color({self.ambient})
ambient_light_energy = 0.55
fog_enabled = true
fog_light_color = Color({self.fog_color})
fog_light_energy = 0.35
fog_density = 0.004
fog_sky_affect = 0.0
fog_height = 0.4
fog_height_density = 0.3

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


# ---- Cabo Tênaro ----
# Como na sala de teste, só os lados do fundo (norte e oeste) têm parede alta; os de perto da câmera (sul e leste)
# têm borda baixa, para não esconder o jogador. Sul (z positivo) é o mar; a gruta desce para o norte (z negativo). O chão é plano (o blockout não tem rampas):
# a descida se lê pelas paredes mais altas e pela luz que some.
t = Scene('Tenaro', '0.45, 0.5, 0.58, 1', '0.306, 0.498, 0.525, 1', 0.8)
# A: a saliência fora da gruta, sobre o mar.
t.box('ChaoSaliencia', (14, 1, 12), (0, -0.5, 4), 'Mat_piso')
t.water('Mar', (40, 0.2, 24), (0, -2.5, 22))
t.box('BordaMar', (14, 0.5, 1), (0, 0.25, 10.5))
t.box('PenhascoOeste', (1, 5, 13), (-7.5, 2.5, 4))
t.box('PenhascoLeste', (1, 0.5, 13), (7.5, 0.25, 4))
t.box('PenhascoNorteA', (5, 2.5, 1), (-4.5, 1.25, -2.5))
t.box('PenhascoNorteB', (5, 1, 1), (4.5, 0.5, -2.5))
t.examinable('OlharMar', (0, 0, 9), 'obj.tenaro.mar')
t.examinable('Boca', (0, 0, -1.2), 'obj.tenaro.boca')
# B: o corredor da descida.
t.box('ChaoCorredor', (6, 1, 24), (0, -0.5, -14.5), 'Mat_piso')
t.box('CorredorOeste', (1, 4, 24), (-3.5, 2, -14.5))
t.box('CorredorLeste', (1, 0.5, 24), (3.5, 0.25, -14.5))
t.box('PedraCaida', (1.6, 0.8, 1.2), (-1.8, 0.4, -9))
t.box('Estreito', (2, 1, 1), (2, 0.5, -19))
# A poça dá a água que o Remador aceita (o encontro das marcas pede a oferenda).
t.spring('Poca', (1.6, 0, -12.5), 'agua', 3, 'obj.tenaro.poca', 'obj.tenaro.poca.recolheu')
t.water('PocaAgua', (1.6, 0.04, 1.6), (1.6, 0.02, -12.5))
t.examinable('Marcas', (-2.5, 0, -22), 'obj.tenaro.marcas')
t.light('LuzBoca', (0, 2.5, -5), '0.55, 0.65, 0.8, 1', 0.8, 8)
# C: a câmara do rio, com a primeira Trívia e a passagem para o Aqueronte.
t.box('ChaoCamara', (18, 1, 16), (0, -0.5, -34.5), 'Mat_piso')
t.box('CamaraOeste', (1, 4, 17), (-9.5, 2, -34.5))
t.box('CamaraLeste', (1, 0.5, 17), (9.5, 0.25, -34.5))
t.box('CamaraSulA', (5.5, 0.5, 1), (-6.25, 0.25, -26.5))
t.box('CamaraSulB', (5.5, 0.5, 1), (6.25, 0.25, -26.5))
t.box('CamaraNorteA', (14, 4, 1), (-2, 2, -43))
t.box('CamaraNorteB', (1, 4, 1), (8.5, 2, -43))
t.water('RioSubterraneo', (12, 0.04, 3), (-3, 0.02, -38.5))
t.blocker('RioBarreira', (12, 2, 3), (-3, 1, -38.5))
t.trivia('Trivia', (-5, 0, -30.5), 'trivia_tenaro')
# A tabela da Maré de Tênaro (05/nov/2026, PENDENCIAS 85): com o degrau 1, o rio passa da margem e a poça cresce.
t.water('RioMare1', (14, 0.05, 4.5), (-3, 0.025, -39.25), tide_min=1)
t.water('PocaMare1', (2.6, 0.05, 2.6), (1.6, 0.025, -12.5), tide_min=1)
t.light('LuzCamara', (2, 3, -35), '0.35, 0.55, 0.6, 1', 1.0, 12)
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
t.column('Fenda', (6.6, 0, -1.9), height=0.6, radius=0.25)
t.column('Argola', (8.5, 0, -28.6), height=0.5, radius=0.12)
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
# do Aqueronte (norte). Um salão sem teto com o olmo dos sonhos vãos e as personificações paradas na névoa (Medo, Doença,
# Guerra, só para examinar até terem arte); no estreito do norte, a Fome, chefe do Ato I, no caminho para o rio.
v = Scene('Vestibulo', '0.4, 0.42, 0.48, 1', '0.33, 0.35, 0.4, 1', 0.5)
v.box('ChaoSalao', (16, 1, 31), (0, -0.5, 0), 'Mat_piso')
v.box('Oeste', (1, 4.5, 31), (-8.5, 2.25, 0))
v.box('Leste', (1, 0.5, 31), (8.5, 0.25, 0))
v.box('SulA', (6.5, 0.5, 1), (-4.75, 0.25, 15.5))
v.box('SulB', (6.5, 0.5, 1), (4.75, 0.25, 15.5))
v.box('NorteA', (6.5, 4.5, 1), (-4.75, 2.25, -15.5))
v.box('NorteB', (6.5, 4.5, 1), (4.75, 2.25, -15.5))
# O estreito: dois paredões deixam uma passagem de 3 m, onde a Fome espera.
v.box('EstreitoOeste', (6.5, 3, 2), (-4.75, 1.5, -9))
v.box('EstreitoLeste', (6.5, 1, 2), (4.75, 0.5, -9))
v.column('Olmo', (-4, 0, 4), height=4.5, radius=1.2)
v.examinable('OlmoOlhar', (-2.4, 0, 4.6), 'obj.vestibulo.olmo')
v.examinable('Entrada', (0, 0, 11.5), 'obj.vestibulo.entrada')
v.column('FormaMedo', (5, 0, 8), height=1.1, radius=0.45)
v.examinable('Medo', (4, 0, 7.2), 'obj.vestibulo.medo', texture='46_medo')
v.box('FormaDoenca', (1.8, 0.4, 0.8), (5, 0.2, 0), 'Mat_parede', collide=True)
v.examinable('Doenca', (3.6, 0, 0.4), 'obj.vestibulo.doenca')
v.column('FormaGuerra', (-5, 0, -4), height=2, radius=0.5)
v.examinable('Guerra', (-3.8, 0, -3.4), 'obj.vestibulo.guerra')
v.encounter('Fome', (0, 0, -9), 'vestibulo_fome', texture='33_fome')
# Tabela da Maré (06/nov/2026, PENDENCIAS 89): com o degrau 1, mais formas paradas na névoa e um soldado que foge.
v.column('FormaMare1', (-5.5, 0, 9), height=1.4, radius=0.4)
v.tide(tide_min=1)
v.column('FormaMare2', (5.5, 0, -4.5), height=1.7, radius=0.45)
v.tide(tide_min=2)
v.wanderer('SombraFugitivo', (2, 0, 4), 'vestibulo_fugitivo', 'soldado', (-4, 0, 0))
v.light('LuzSalao', (0, 4, 2), '0.45, 0.45, 0.55, 1', 0.8, 16)
v.exit('ParaTenaro', (0, 1, 15.4), (3, 2, 1.2), 'tenaro', 'SaidaVestibulo')
v.exit('ParaAqueronte', (0, 1, -15.4), (3, 2, 1.2), 'aqueronte', 'ChegadaVestibulo')
v.markers({
    'ChegadaTenaro': (0, 0, 12.5),
    'SaidaAqueronte': (0, 0, -12.5),
    'Olmo': (-2, 0, 5.5),
    # Em cima do encontro: teleportar para ele começa a luta com a Fome.
    'Fome': (0, 0, -9),
    'Fugitivo': (2, 0, 4),
    'AtravessarTenaro': (0, 0, 15.4),
    'AtravessarAqueronte': (0, 0, -15.4),
}, player=(0, 0, 12.5))
v.write('Vestibulo.tscn')

# ---- Margem do Aqueronte ----
a = Scene('Aqueronte', '0.42, 0.46, 0.52, 1', '0.36, 0.42, 0.46, 1', 0.6)
a.box('ChaoMargem', (20, 1, 14), (0, -0.5, 0), 'Mat_piso')
a.box('Oeste', (1, 3, 15), (-10.5, 1.5, 0))
a.box('Leste', (1, 0.5, 15), (10.5, 0.25, 0))
a.box('SulA', (8.5, 0.5, 1), (-5.75, 0.25, 7.5))
a.box('SulB', (8.5, 0.5, 1), (5.75, 0.25, 7.5))
a.water('Rio', (40, 0.04, 14), (0, -0.3, -14))
a.blocker('RioBarreira', (22, 2, 1), (0, 1, -7.5))
a.box('Pier', (2, 0.3, 4), (4, -0.15, -9), 'Mat_terracota', collide=False)
a.box('Barca', (1.6, 0.5, 3), (6.2, -0.05, -10), 'Mat_terracota', collide=False)
a.column('PosteCaronte', (4, 0, -6.6), height=2.6, radius=0.15)
a.examinable('Margem', (-1, 0, -6), 'obj.aqueronte.margem')
# Caronte (só a voz, sem arte ainda) e os insepultos: conversas (content/dialogue/caronte.json e insepultos.json).
a.talkable('Caronte', (3.2, 0, -5.6), 'caronte', texture='27_caronte', idle='caronte')
a.talkable('Insepultos', (-7, 0, -2), 'insepultos', texture='28_insepultos', idle='insepultos')
# O afogado (capítulo 1 do Pescador: "um afogado pede para ser achado"), na água rasa da margem.
a.talkable('Afogado', (-3.5, 0, -6.2), 'afogado', texture='29_afogado', idle='afogado')
# A esposa do Padeiro (a missão dele, 03/nov/2026): não atravessou; espera na margem, do lado de cá.
a.talkable('Esposa', (-8, 0, 4.5), 'esposa_padeiro', texture='31_esposa', idle='esposa_padeiro')
a.examinable('BarcaOlhar', (7.6, 0, -5.4), 'obj.aqueronte.barca')
# Tabela da Maré (06/nov/2026, PENDENCIAS 89): o rio sobe na margem e traz uma afogada.
a.water('MargemMare1', (20, 0.05, 1.6), (0, 0.025, -6.5), tide_min=1)
a.water('MargemMare2', (20, 0.05, 1.4), (0, 0.03, -5.2), tide_min=2)
a.wanderer('SombraAfogada', (-5, 0, 1), 'aqueronte_afogada', 'lavadeira', (4, 0, 0))
# O Convite (1ª Pesada) aflora diante da barca, depois de Caronte falar da "outra pessoa" (gate na Pesada).
a.heavy_place('PesadaConvite', (5.8, 0, -4.2), 'o_convite')
# A barca: com a chave do Aqueronte (óbolo, insepultos, origem Pescador...), leva a Asfódelos; é o 1º marco da Maré.
a.exit('SubirNaBarca', (5.8, 1, -6.9), (1.6, 2, 0.8), 'asfodelos', 'ChegadaAqueronte',
       path_key='aqueronte', blocked='obj.aqueronte.barca.fechada', milestone='aqueronte')
a.light('LuzMargem', (0, 4, -2), '0.5, 0.55, 0.6, 1', 0.9, 14)
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
# Os três caminhos da Trívia (Luto, Tribunal, Guerreiros) ainda estão fechados (fora da fatia).
f = Scene('Asfodelos', '0.5, 0.52, 0.5, 1', '0.42, 0.45, 0.4, 1', 0.75)
f.box('Chao', (32, 1, 26), (0, -0.5, 0), 'Mat_piso')
f.box('ChaoPlanicie', (34, 1, 26), (33, -0.5, 0), 'Mat_piso')
f.water('Aqueronte', (100, 0.04, 14), (17, -0.3, 20))
f.blocker('MargemBarreira', (66, 2, 1), (17, 1, 13.5))
# A oeste da Rua dos Ofícios, por uma brecha no muro, o Poço das Sem-Nome (Ato II, 06/nov/2026).
f.box('OesteA', (1, 3, 5.5), (-16.5, 1.5, -10.75))
f.box('OesteB', (1, 3, 18.5), (-16.5, 1.5, 4.25))
f.box('ChaoPoco', (16, 1, 26), (-25, -0.5, 0), 'Mat_piso')
f.box('PocoOeste', (1, 3, 27), (-33.5, 1.5, 0))
f.box('PocoNorte', (17, 3, 1), (-25, 1.5, -13.5))
f.blocker('PocoSul', (17, 2, 1), (-25, 1, 13.5))
f.box('Leste', (1, 0.5, 27), (50.5, 0.25, 0))
f.box('Norte', (66, 3, 1), (17, 1.5, -13.5))
f.box('Pier', (2, 0.3, 3), (0, -0.15, 12.5), 'Mat_terracota', collide=False)
f.examinable('Planicie', (19, 0, 4), 'obj.asfodelos.planicie')
# A entrada da Planície: um muro baixo com uma passagem, e Cérbero nela (HISTORIA 6; 03/nov/2026). Acalmado, libertado,
# presenciado, absorvido ou desfeito, a passagem fica livre.
f.box('MuroPlanicieNorte', (1, 1, 16), (17, 0.5, -5))
f.box('MuroPlanicieSul', (1, 1, 7), (17, 0.5, 9.5))
f.encounter('Cerbero', (17, 0, 4.5), 'cerbero', texture='25_cerbero')
# A Planície: três sombras comuns vagando, repetindo o gesto; a luta começa quando uma nota o jogador.
f.column('PedraPlanicie1', (22, 0, 8), height=1.2, radius=0.6)
f.column('PedraPlanicie2', (30, 0, -10), height=1.6, radius=0.7)
f.column('PedraPlanicie3', (36, 0, 8.5), height=1.0, radius=0.5)
f.wanderer('SombraSoldado', (22, 0, -6), 'planicie_soldado', 'soldado', (6, 0, 0))
f.wanderer('SombraLavadeira', (27, 0, 6), 'planicie_lavadeira', 'lavadeira', (0, 0, -5))
f.wanderer('SombraPastor', (33, 0, -2), 'planicie_pastor', 'pastor', (4, 0, 4))
# Cérbero combatido até o fim (destruído ou absorvido): a Planície fica agitada, e uma camponesa vaga perto do muro (05/nov/2026).
f.wanderer('SombraCamponesa', (21, 0, 1), 'planicie_camponesa', 'camponesa', (3, 0, 4))
# O mercado no fundo da Planície: bancas, o Mercador (vinho e leite, que o Altar não tem) e a Lia, que desenha rostos.
f.box('Banca1', (2.5, 0.9, 1.2), (42, 0.45, -8), 'Mat_terracota')
f.box('Banca2', (2.5, 0.9, 1.2), (46.5, 0.45, -8), 'Mat_terracota')
f.box('Banca3', (1.2, 0.9, 2.5), (48, 0.45, -3), 'Mat_terracota')
f.examinable('Mercado', (44, 0, -5.5), 'obj.asfodelos.mercado')
f.merchant('Mercador', (42, 0, -6.6), 'mercador', '32_mercador_planicie', idle='mercador_planicie')
f.companion('Lia', (46.5, 0, 2.5), 'lia', '23_lia')
f.light('LuzMercado', (44, 2.5, -4), '0.9, 0.7, 0.45, 1', 0.8, 10)
f.exit('ParaAqueronte', (0, 1, 13), (2, 2, 0.8), 'aqueronte', 'ChegadaBarca')
# A grande Trívia (liga a todas as acesas) e os três caminhos ainda fechados.
f.trivia('Trivia', (0, 0, -8), 'trivia_asfodelos')
# Os três caminhos: fechados pela névoa até o segundo degrau da Maré (escolher o companheiro; decisão do João, 06/nov/2026).
f.examinable('CaminhoLuto', (-6, 0, -12.2), 'obj.asfodelos.caminho_luto')
f.tide(tide_max=1)
f.examinable('CaminhoTribunal', (0, 0, -12.2), 'obj.asfodelos.caminho_tribunal')
f.tide(tide_max=1)
f.examinable('CaminhoGuerreiros', (6, 0, -12.2), 'obj.asfodelos.caminho_guerreiros')
f.tide(tide_max=1)
for nome, x in (('NevoaLuto', -6), ('NevoaTribunal', 0), ('NevoaGuerreiros', 6)):
    f.box(nome, (3, 2.2, 0.6), (x, 1.1, -12.9), 'Mat_agua', collide=False)
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
# A viúva chega com o segundo degrau da Maré e procura entre as bancas da Rua dos Ofícios.
f.wanderer('SombraViuva', (-7, 0, -1), 'asfodelos_viuva', 'carpideira', (0, 0, 6))
# O Poço das Sem-Nome: o poço escuro, as inscrições com os pedaços do nome dela (decisão do João, 06/nov/2026: o nome
# está espalhado; só com os quatro a Escuta revela o nome) e a Sem-Nome, chefe opcional, à beira do poço.
f.water('Poco', (5, 0.04, 5), (-26, 0.02, 1))
f.blocker('PocoBarreira', (5, 2, 5), (-26, 1, 1))
f.examinable('PocoOlhar', (-22.5, 0, 1), 'obj.poco.entrada')
f.examinable('Inscricao1', (-30, 0, -9), 'obj.poco.nome.1', flag='sem_nome_pedaco_1')
f.examinable('Inscricao2', (-31, 0, 8), 'obj.poco.nome.2', flag='sem_nome_pedaco_2')
f.examinable('Inscricao3', (-19.5, 0, 10.5), 'obj.poco.nome.3', flag='sem_nome_pedaco_3')
f.examinable('Inscricao4', (-21, 0, -10.5), 'obj.poco.nome.4', flag='sem_nome_pedaco_4')
f.encounter('SemNome', (-26, 0, -3.5), 'asfodelos_sem_nome', texture='39_sem_nome')
f.light('LuzPoco', (-26, 3, 0), '0.5, 0.55, 0.65, 1', 0.6, 12)
# A Rua dos Ofícios: bancadas, o forno do Padeiro e a banca do Coveiro.
f.box('Bancada1', (3, 0.9, 1.2), (-12, 0.45, -4), 'Mat_terracota')
f.box('Forno', (2, 1.6, 2), (-13, 0.8, 1), 'Mat_terracota')
f.box('Bancada2', (3, 0.9, 1.2), (-12, 0.45, 6))
f.talkable('Padeiro', (-10.5, 0, 1.5), 'padeiro', texture='16_padeiro')
f.merchant('Coveiro', (-10.5, 0, 6.5), 'coveiro', '17_coveiro')
f.examinable('Oficios', (-9, 0, -4), 'obj.asfodelos.oficios')
# O oleiro sem mãos (capítulo 1 do Artesão: "um oleiro sem mãos pede reparo"), na bancada da Rua.
f.talkable('Oleiro', (-12, 0, -2.6), 'oleiro', texture='30_oleiro', idle='oleiro')
# Os companheiros que se conhecem nesta parte (HISTORIA 5.2): Dimas na Rua dos Ofícios, Cálias no Altar; a Lia no
# mercado da Planície (decisão do João, 03/nov/2026: longe dos outros, numa área de mercado).
f.companion('Dimas', (-13.5, 0, 4), 'dimas', '20_dimas')
# O Altar das Oferendas: o que os vivos deixam (recolher respeita o limite por tipo).
f.box('Altar', (3, 1, 2), (11, 0.5, -3), 'Mat_terracota')
f.spring('AltarFarinha', (10.2, 0, -2), 'farinha', 2, 'obj.asfodelos.altar', 'ui.altar.recolheu')
f.spring('AltarMel', (11.8, 0, -2), 'mel', 1, 'obj.asfodelos.altar', 'ui.altar.recolheu')
f.companion('Calias', (14, 0, -0.5), 'calias', '21_calias')
f.light('LuzAltar', (11, 2.5, -3), '0.95, 0.8, 0.55, 1', 0.9, 8)
f.light('LuzRua', (-11, 2.5, 2), '0.9, 0.62, 0.38, 1', 0.8, 9)
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

# Campos do Luto: bosque de murtas, os que morreram de amor e de saudade; Ismena se conhece aqui.
l = ato2('Luto', '0.42, 0.46, 0.5, 1', '0.36, 0.4, 0.46, 1', 0.55, 'SaidaLuto')
for i, (x, z) in enumerate(((-7, -6), (-4, 4), (6, -8), (7, 3), (-8, 8), (2, -2))):
    l.column(f'Murta{i + 1}', (x, 0, z), height=2.2, radius=0.5)
l.examinable('Murtas', (-2.5, 0, 5), 'obj.luto.murtas')
l.examinable('Choro', (5, 0, -5.5), 'obj.luto.choro')
l.companion('Ismena', (3, 0, 6), 'ismena', '36_ismena')
l.wanderer('SombraNoiva', (-3, 0, -4), 'luto_noiva', 'camponesa', (4, 0, 0))
l.trivia('Trivia', (-6, 0, -10), 'trivia_luto')
l.markers({'ChegadaAsfodelos': (0, 0, 10.5), 'Trivia': (-4.5, 0, -10), 'Ismena': (3.5, 0, 7), 'Noiva': (-3, 0, -4),
           'AtravessarAsfodelos': (0, 0, 13.4)}, player=(0, 0, 10.5))
l.write('Luto.tscn')

# O Tribunal: a pedra onde as sombras são julgadas; Septímia se conhece aqui.
tr = ato2('Tribunal', '0.48, 0.46, 0.44, 1', '0.4, 0.38, 0.36, 1', 0.6, 'SaidaTribunal')
tr.box('Estrado', (8, 0.6, 4), (0, 0.3, -9), 'Mat_terracota')
for i, x in enumerate((-6, -2, 2, 6)):
    tr.column(f'Coluna{i + 1}', (x, 0, -4), height=3.2, radius=0.45)
tr.tribunal('Pedra', (0, 0, -6.5), 'julgamento_padeiro')
# O Plano (2ª Pesada) aflora junto da pedra, depois de conhecer Septímia (06/nov/2026).
tr.heavy_place('PesadaPlano', (2.5, 0, -5), 'o_plano')
tr.examinable('Juizes', (-3, 0, -7), 'obj.tribunal.juizes')
tr.companion('Septimia', (-6, 0, 4), 'septimia', '37_septimia')
tr.wanderer('SombraPerjuro', (5, 0, 0), 'tribunal_perjuro', 'mercador', (-3, 0, 3))
tr.trivia('Trivia', (7, 0, 6), 'trivia_tribunal')
tr.markers({'ChegadaAsfodelos': (0, 0, 10.5), 'Pedra': (0, 0, -5.5), 'Trivia': (5.5, 0, 6), 'Septimia': (-5.5, 0, 5), 'Perjuro': (5, 0, 0), 'PesadaPlano': (2.5, 0, -4),
            'AtravessarAsfodelos': (0, 0, 13.4)}, player=(0, 0, 10.5))
tr.write('Tribunal.tscn')

# O Campo dos Guerreiros: armas fincadas no chão e o Campeão no fundo; Aulo se conhece aqui.
g = ato2('Guerreiros', '0.5, 0.48, 0.46, 1', '0.42, 0.4, 0.38, 1', 0.7, 'SaidaGuerreiros')
for i, (x, z) in enumerate(((-8, 2), (-5, -3), (4, 5), (8, -2), (-2, 8), (6, 9))):
    g.column(f'Lanca{i + 1}', (x, 0, z), height=1.6, radius=0.1)
g.examinable('Armas', (-4, 0, 1), 'obj.guerreiros.armas')
g.box('Arena', (8, 0.1, 6), (0, 0.05, -8), 'Mat_terracota', collide=False)
g.encounter('Campeao', (0, 0, -8), 'guerreiros_campeao', texture='38_campeao')
g.companion('Aulo', (-6, 0, 6), 'aulo', '35_aulo')
g.trivia('Trivia', (7, 0, 7), 'trivia_guerreiros')
g.markers({'ChegadaAsfodelos': (0, 0, 10.5), 'Campeao': (0, 0, -8), 'Trivia': (5.5, 0, 7), 'Aulo': (-5.5, 0, 7),
           'AtravessarAsfodelos': (0, 0, 13.4)}, player=(0, 0, 10.5))
g.write('Guerreiros.tscn')

# ---- Ato III (10/nov/2026, PENDENCIAS 92; blocos para revisar) ----
# O Estige: chega-se de Asfódelos pelo sul; a deusa (só a voz) à beira do rio oferece os juramentos; jurar abre a
# travessia, ao norte, que é o 3º marco da Maré e leva ao Palácio.
e = ato2('Estige', '0.36, 0.36, 0.42, 1', '0.26, 0.26, 0.32, 1', 0.45, 'SaidaEstige')
e.water('Rio', (22, 0.04, 6), (0, -0.3, -7))
e.blocker('RioBarreira', (22, 2, 5), (0, 1, -8))
e.examinable('RioOlhar', (-5, 0, -3), 'obj.estige.rio')
e.talkable('Estige', (0, 0, -3), 'estige')
e.exit('ParaPalacio', (0, 1, -4.8), (3, 2, 0.8), 'palacio', 'ChegadaEstige', path_key='estige',
       blocked='obj.estige.travessia.fechada', milestone='cruzar_estige')
e.trivia('Trivia', (7, 0, 6), 'trivia_estige')
# Maré do Ato III (11/nov/2026, PENDENCIAS 96): o rio sobe até a margem; o desertor caminha por ela.
e.water('MargemMare4', (22, 0.05, 1.6), (0, 0.025, -3.9), tide_min=4)
e.wanderer('SombraDesertor', (-6, 0, 2), 'estige_desertor', 'desertor', (0, 0, 5))
e.light('LuzRio', (0, 3, -6), '0.55, 0.6, 0.75, 1', 0.7, 14)
e.markers({'ChegadaAsfodelos': (0, 0, 10.5), 'ChegadaPalacio': (0, 0, -1), 'Estige': (0, 0, -1.6), 'Trivia': (5.5, 0, 6), 'Desertor': (-6, 0, 2),
           'AtravessarPalacio': (0, 0, -4.8), 'AtravessarAsfodelos': (0, 0, 13.4)}, player=(0, 0, 10.5))
e.write('Estige.tscn')

# O Palácio: os tronos ao norte (Hades, só a voz, e Perséfone), o jardim a oeste (a romãzeira e Ascálafo sob a pedra);
# a escada para Hipno a leste, que pede o caminho da descida (o favor de Hades, o jardim ou a escada dos jardineiros).
pa = ato2('Palacio', '0.44, 0.4, 0.42, 1', '0.32, 0.28, 0.3, 1', 0.55, 'ChegadaPalacio', 'estige')
pa.box('Estrado', (8, 0.6, 3), (2, 0.3, -11), 'Mat_terracota')
pa.box('TronoHades', (1.4, 2, 1), (0.5, 1, -11.5), 'Mat_parede')
pa.box('TronoPersefone', (1.4, 1.6, 1), (3.5, 0.8, -11.5), 'Mat_terracota')
pa.examinable('Tronos', (2, 0, -8.8), 'obj.palacio.trono')
pa.talkable('Hades', (0.5, 0, -9.2), 'hades')
pa.talkable('Persefone', (3.5, 0, -9.2), 'persefone')
for i, (x, z) in enumerate(((-8, -2), (-9, 4), (-5, 8))):
    pa.column(f'Cipreste{i + 1}', (x, 0, z), height=3, radius=0.4)
pa.column('Romanzeira', (-6, 0, 2), height=1.8, radius=0.6)
pa.examinable('Roma', (-4.6, 0, 2.5), 'obj.palacio.roma')
# A pedra já vem desenhada na figura de Ascálafo (11/nov/2026).
pa.talkable('Ascalafo', (-7.4, 0, 9), 'ascalafo', texture='45_ascalafo')
# A Venda (3ª Pesada; decisão do João, 11/nov/2026): aflora diante do trono, depois da audiência com Hades.
pa.heavy_place('PesadaVenda', (2, 0, -7.4), 'a_venda')
# O capítulo 3 das origens (Prova, 11/nov/2026, PENDENCIAS 99): a mesa do pacto, ao lado do estrado.
pa.box('MesaPacto', (1.8, 0.9, 1), (-3.5, 0.45, -8.5), 'Mat_terracota')
pa.talkable('Prova', (-3.5, 0, -7.4), 'prova')
pa.exit('ParaHipno', (11, 1, 0), (0.8, 2, 3), 'hipno', 'ChegadaPalacio', path_key='palacio_descida',
        blocked='obj.palacio.descida.fechada')
pa.trivia('Trivia', (7, 0, 6), 'trivia_palacio')
# Maré do Ato III (11/nov/2026, PENDENCIAS 96): o sacristão vaga pelos corredores, acendendo lamparinas.
pa.wanderer('SombraSacristao', (6, 0, -3), 'palacio_sacristao', 'sacristao', (0, 0, 6))
pa.light('LuzTronos', (2, 3, -9), '0.85, 0.55, 0.45, 1', 1.0, 12)
pa.light('LuzJardim', (-6, 3, 5), '0.6, 0.7, 0.55, 1', 0.8, 12)
pa.markers({'ChegadaEstige': (0, 0, 10.5), 'ChegadaHipno': (9, 0, 0), 'Hades': (0.5, 0, -8), 'Persefone': (3.5, 0, -8),
            'Ascalafo': (-5.5, 0, 9), 'PesadaVenda': (2, 0, -6.4), 'Sacristao': (6, 0, -3), 'Prova': (-3.5, 0, -6.4), 'Trivia': (5.5, 0, 6), 'AtravessarHipno': (11, 0, 0), 'AtravessarEstige': (0, 0, 13.4)},
           player=(0, 0, 10.5))
pa.write('Palacio.tscn')
# A volta do Palácio vai ao Estige, do lado de cá do rio (o marcador ChegadaPalacio de lá).

# A morada de Hipno: papoulas; Hipno (só a voz), o Sonho no fundo, Tânato (sem luta), O Olhar (4ª Pesada) e, no canto
# leste, o som de água que leva, escondido, a Mnemósine.
h = ato2('Hipno', '0.4, 0.36, 0.46, 1', '0.34, 0.3, 0.42, 1', 0.5, 'ChegadaHipno', 'palacio')
for i, (x, z) in enumerate(((-8, -4), (-3, 6), (5, -3), (8, 3), (-7, 9), (1, 2))):
    h.column(f'Papoula{i + 1}', (x, 0, z), height=0.8, radius=0.6)
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
for i, (x, z) in enumerate(((-4, -2), (3, 7), (7, -1))):
    h.box(f'PapoulaMare{i + 1}', (1.2, 0.6, 1.2), (x, 0.3, z), 'Mat_terracota', tide_min=4)
h.wanderer('SombraTrombeteiro', (4, 0, 4), 'hipno_trombeteiro', 'trombeteiro', (-5, 0, 0))
h.light('LuzPapoulas', (0, 3, 0), '0.7, 0.55, 0.8, 1', 0.9, 16)
h.light('LuzSonho', (0, 3, -8), '0.6, 0.65, 0.85, 1', 0.8, 10)
h.markers({'ChegadaPalacio': (0, 0, 10.5), 'ChegadaMnemosine': (9, 0, 7), 'Hipno': (-5, 0, -4.8), 'Sonho': (0, 0, -9),
           'Tanato': (6, 0, -5.8), 'PesadaOlhar': (-6, 0, 7), 'Trombeteiro': (4, 0, 4), 'Agua': (8, 0, 8), 'Trivia': (-6.5, 0, -10),
           'AtravessarMnemosine': (11, 0, 9), 'AtravessarPalacio': (0, 0, 13.4),
           'ChegadaBifurcacao': (9, 0, -10.5), 'AtravessarBifurcacao': (9, 0, -12.6)}, player=(0, 0, 10.5))
h.write('Hipno.tscn')

# Mnemósine (secreta): duas fontes iguais; a do cipreste branco (a do Lete, à esquerda) e a fria, com os guardiões.
m = ato2('Mnemosine', '0.42, 0.46, 0.5, 1', '0.36, 0.42, 0.48, 1', 0.5, 'ChegadaMnemosine', 'hipno')
m.water('FonteEsquerda', (3, 0.04, 3), (-5, 0.02, -6))
m.column('CipresteBranco', (-7.5, 0, -7), height=3.2, radius=0.4)
m.talkable('FonteLete', (-5, 0, -4), 'fonte_lete')
m.water('FonteDireita', (3, 0.04, 3), (5, 0.02, -6))
m.column('Guardiao1', (3.2, 0, -3.8), height=2, radius=0.3)
m.column('Guardiao2', (6.8, 0, -3.8), height=2, radius=0.3)
m.talkable('FonteMnemosine', (5, 0, -3), 'fonte_mnemosine')
m.examinable('Tabuinha', (0, 0, 2), 'obj.mnemosine.tabuinha')
m.light('LuzFontes', (0, 3, -5), '0.6, 0.75, 0.85, 1', 1.0, 16)
m.markers({'ChegadaHipno': (0, 0, 10.5), 'FonteLete': (-5, 0, -2.6), 'FonteMnemosine': (5, 0, -1.6), 'Tabuinha': (0, 0, 3.5),
           'AtravessarHipno': (0, 0, 13.4)}, player=(0, 0, 10.5))
m.write('Mnemosine.tscn')
# ---- Ato IV (11/nov/2026, PENDENCIAS 97; decisões do João na PENDENCIAS 94; blocos para revisar) ----
# A bifurcação: à esquerda o Tártaro, à direita os Elísios; o caminho do meio desce ao Vale do Lete, aberto quando os dois
# lados foram vistos e com a Chave de Memória (5º marco da Maré).
bi = ato2('Bifurcacao', '0.38, 0.36, 0.4, 1', '0.3, 0.28, 0.32, 1', 0.5, 'ChegadaBifurcacao', 'hipno')
bi.examinable('Caminhos', (0, 0, -3), 'obj.bifurcacao.caminhos')
bi.box('MuroFogo', (1, 2.4, 6), (-9.5, 1.2, -6), 'Mat_terracota')
bi.column('ArvoreClara', (9, 0, -6), height=2.8, radius=0.5)
bi.exit('ParaTartaro', (-11, 1, 0), (0.8, 2, 3), 'tartaro', 'ChegadaBifurcacao')
bi.exit('ParaElisios', (11, 1, 0), (0.8, 2, 3), 'elisios', 'ChegadaBifurcacao')
bi.exit('ParaLete', (0, 1, -12.6), (3, 2, 1), 'vale_lete', 'ChegadaBifurcacao', path_key='vale_do_lete',
        blocked='obj.bifurcacao.lete.fechado', milestone='vale_do_lete',
        condition={'all': [{'region_visited': 'tartaro'}, {'region_visited': 'elisios'}]})
bi.trivia('Trivia', (7, 0, 6), 'trivia_bifurcacao')
bi.markers({'ChegadaHipno': (0, 0, 10.5), 'ChegadaTartaro': (-9, 0, 0), 'ChegadaElisios': (9, 0, 0), 'ChegadaLete': (0, 0, -10.5),
            'Caminhos': (0, 0, -1.8), 'Trivia': (5.5, 0, 6), 'AtravessarTartaro': (-11, 0, 0), 'AtravessarElisios': (11, 0, 0),
            'AtravessarLete': (0, 0, -12.6), 'AtravessarHipno': (0, 0, 13.4)}, player=(0, 0, 10.5))
bi.write('Bifurcacao.tscn')

# O Tártaro: o Flegetonte (fogo) atravessa o campo; os condenados; no portão de ferro, as Fúrias.
ta = ato2('Tartaro', '0.5, 0.3, 0.26, 1', '0.4, 0.2, 0.16, 1', 0.3, 'ChegadaTartaro', 'bifurcacao')
ta.box('Flegetonte', (22, 0.06, 2.4), (0, 0.03, 1), 'Mat_terracota', collide=False)
ta.box('Portao', (6, 3.5, 0.6), (0, 1.75, -12.6))
ta.encounter('Furias', (0, 0, -9.5), 'tartaro_furias', texture='41_furias')
ta.examinable('Sisifo', (-7, 0, -5), 'obj.tartaro.sisifo')
ta.box('PedraSisifo', (1.4, 1.4, 1.4), (-8, 0.7, -6.5), 'Mat_parede')
ta.examinable('Tantalo', (7, 0, -5), 'obj.tartaro.tantalo')
ta.examinable('Danaides', (-7, 0, 6), 'obj.tartaro.danaides')
ta.wanderer('SombraAncora', (5, 0, 5), 'tartaro_ancora', 'ancora', (-6, 0, 0))
ta.trivia('Trivia', (7, 0, 8), 'trivia_tartaro')
ta.light('LuzFogo', (0, 3, 1), '1, 0.45, 0.2, 1', 1.4, 14)
ta.markers({'ChegadaBifurcacao': (0, 0, 10.5), 'Furias': (0, 0, -8), 'Sisifo': (-7, 0, -4), 'Tantalo': (7, 0, -4),
            'Danaides': (-7, 0, 7), 'Ancora': (5, 0, 5), 'Trivia': (5.5, 0, 8), 'AtravessarBifurcacao': (0, 0, 13.4)},
           player=(0, 0, 10.5))
ta.write('Tartaro.tscn')

# Os Campos Elísios: luz baixa e dourada; Anquises (só a voz) à beira do rio onde as almas bebem para voltar; A Mentira.
el = ato2('Elisios', '0.6, 0.58, 0.48, 1', '0.5, 0.48, 0.38, 1', 0.8, 'ChegadaElisios', 'bifurcacao')
for i, (x, z) in enumerate(((-8, -6), (-4, 6), (6, -9), (8, 4), (-8, 9))):
    el.column(f'Loureiro{i + 1}', (x, 0, z), height=2.6, radius=0.5)
el.water('RioAlmas', (8, 0.04, 2.4), (4, 0.02, -7))
el.examinable('Almas', (4, 0, -5), 'obj.elisios.almas')
el.talkable('Anquises', (-2, 0, -5), 'anquises')
el.heavy_place('PesadaMentira', (-5, 0, 1), 'a_mentira')
el.wanderer('SombraOferente', (3, 0, 4), 'elisios_oferente', 'oferente', (-5, 0, 0))
el.trivia('Trivia', (7, 0, 8), 'trivia_elisios')
el.markers({'ChegadaBifurcacao': (0, 0, 10.5), 'Anquises': (-2, 0, -3.8), 'Almas': (4, 0, -4), 'PesadaMentira': (-5, 0, 2),
            'Oferente': (3, 0, 4), 'Trivia': (5.5, 0, 8), 'AtravessarBifurcacao': (0, 0, 13.4)}, player=(0, 0, 10.5))
el.write('Elisios.tscn')

# O Vale do Lete: o rio do esquecimento; a Cadeira do Esquecimento (O Gole); no fundo, a Sombra do Esquecido; depois
# dela, os Portões.
vl = ato2('ValeLete', '0.34, 0.38, 0.44, 1', '0.3, 0.34, 0.4, 1', 0.4, 'ChegadaLete', 'bifurcacao')
vl.water('Lete', (22, 0.04, 3), (0, 0.02, -4))
vl.box('Cadeira', (1.2, 1.4, 1.2), (-6, 0.7, 2), 'Mat_parede')
vl.talkable('CadeiraEsquecimento', (-4.6, 0, 2.5), 'cadeira')
vl.heavy_place('PesadaGole', (-6, 0, 4), 'o_gole')
vl.encounter('SombraEsquecido', (0, 0, -9), 'lete_sombra', texture='42_esquecido', idle='esquecido_m',
             feminine=('43_esquecido_f', 'esquecido_f'))
# O capítulo 4 das origens (Acerto, PENDENCIAS 99): na margem, entre as Sem-Nome, a pessoa que ele traiu.
vl.talkable('Acerto', (6, 0, -1.5), 'acerto', idle='sem_nome')
vl.exit('ParaPortoes', (0, 1, -12.6), (3, 2, 1), 'portoes', 'ChegadaLete', blocked='obj.lete.portoes.fechados',
        condition={'any': [{'flag': 'trocou_lugar'}] + [{'shade_state': {'id': 'sombra_esquecido', 'state': s}}
                                                         for s in ('destroyed', 'freed', 'absorbed')]})
vl.trivia('Trivia', (7, 0, 8), 'trivia_lete')
vl.markers({'ChegadaBifurcacao': (0, 0, 10.5), 'ChegadaPortoes': (0, 0, -10.5), 'Cadeira': (-4.6, 0, 3.6),
            'PesadaGole': (-6, 0, 5), 'Sombra': (0, 0, -9), 'Acerto': (6, 0, -0.4), 'Trivia': (5.5, 0, 8), 'AtravessarPortoes': (0, 0, -12.6),
            'AtravessarBifurcacao': (0, 0, 13.4)}, player=(0, 0, 10.5))
vl.write('ValeLete.tscn')

# Os Portões: dois portões iguais na forma, um de chifre (fosco) e um de marfim (que brilha).
po = ato2('Portoes', '0.36, 0.36, 0.4, 1', '0.28, 0.28, 0.32, 1', 0.6, 'ChegadaPortoes', 'vale_lete')
po.box('PortaoChifre', (3, 4, 0.6), (-4, 2, -11), 'Mat_terracota')
po.box('PortaoMarfim', (3, 4, 0.6), (4, 2, -11), 'Mat_parede')
po.talkable('Portoes', (0, 0, -8), 'portoes')
po.markers({'ChegadaLete': (0, 0, 10.5), 'Portoes': (0, 0, -6.8), 'AtravessarLete': (0, 0, 13.4)}, player=(0, 0, 10.5))
po.write('Portoes.tscn')
print('ok')
