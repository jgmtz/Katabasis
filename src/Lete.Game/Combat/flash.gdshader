// Sprite de batalha: textura lisa (linear + mipmaps) ou pixelizada (nearest), tinta, piscada branca, fade
// perda de cor da Marca (só no protagonista) e desintegração (some pixel a pixel de cima para baixo, com uma borda clara onde está se desfazendo).
// Com o palco (BattleSet): a tinta do lugar, a luz do braseiro na borda virada para ele (em degraus), a névoa nos pés,
// a respiração (o peito desce um pixel) e, nas sombras, a barra que ondula e se desfaz.
shader_type spatial;
render_mode blend_mix, cull_disabled, depth_prepass_alpha;

uniform sampler2D tex_nearest : source_color, filter_nearest, repeat_disable;
uniform sampler2D tex_linear : source_color, filter_linear_mipmap, repeat_disable;
uniform bool smooth_filter = false;
uniform vec4 tint : source_color = vec4(1.0);
uniform float desaturation : hint_range(0.0, 1.0) = 0.0;
uniform float flash : hint_range(0.0, 1.0) = 0.0;
uniform float fade : hint_range(0.0, 1.0) = 1.0;
uniform float dissolve : hint_range(0.0, 1.0) = 0.0;
uniform vec4 dissolve_edge : source_color = vec4(0.85, 0.8, 0.69, 1.0);
uniform float dissolve_edge_width = 0.06;

// O palco. place_tint: a cor do lugar (perto do branco). fire_dir: de onde vem a luz no plano do sprite (x para a
// direita da tela, y para cima). fire_strength 0 desliga a luz (cenas sem palco).
uniform vec3 place_tint : source_color = vec3(1.0);
uniform vec3 fire_color : source_color = vec3(1.0, 0.62, 0.3);
uniform vec2 fire_dir = vec2(-1.0, 0.3);
uniform float fire_strength : hint_range(0.0, 3.0) = 0.0;
uniform float fire_bands = 3.0;
// Névoa nos pés: a cor e até que altura do corpo ela sobe (fração da altura da arte).
uniform vec3 mist_color : source_color = vec3(0.3, 0.38, 0.4);
uniform float mist_height : hint_range(0.0, 0.5) = 0.0;
uniform float mist_amount : hint_range(0.0, 1.0) = 0.5;
// Respiração (vivos) e sombra dos mortos (ondula e se desfaz embaixo).
uniform float breath : hint_range(0.0, 1.0) = 0.0;
uniform float shade : hint_range(0.0, 1.0) = 0.0;
uniform float phase = 0.0;

const float BAYER[16] = float[](
	0.0, 8.0, 2.0, 10.0,
	12.0, 4.0, 14.0, 6.0,
	3.0, 11.0, 1.0, 9.0,
	15.0, 7.0, 13.0, 5.0);

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

vec4 art_at(vec2 texel, vec2 size) {
	if (texel.x < 0.0 || texel.y < 0.0 || texel.x >= size.x || texel.y >= size.y) {
		return vec4(0.0);
	}
	vec2 uv = (texel + 0.5) / size;
	return smooth_filter ? texture(tex_linear, uv) : texture(tex_nearest, uv);
}

void fragment() {
	vec2 size = vec2(smooth_filter ? textureSize(tex_linear, 0) : textureSize(tex_nearest, 0));
	// Um "pixel" dos efeitos cresce com a arte (160 px de altura = 2).
	float unit = max(1.0, floor(size.y / 80.0 + 0.5));
	vec2 texel = floor(UV * size);
	float from_ground = size.y - 1.0 - texel.y;

	// Sombra dos mortos: a barra ondula para os lados.
	float hem = smoothstep(size.y * 0.7, size.y, texel.y) * shade;
	texel.x += floor(sin(TIME * 1.2 + texel.y * (0.1 / unit) + phase) * hem * 1.5 + 0.5) * unit;

	// Respiração: o peito e a cabeça descem um pixel de tempos em tempos.
	if (texel.y < floor(size.y * 0.4)) {
		texel.y -= step(0.15, sin(TIME * 1.7 + phase)) * breath * unit;
	}

	bool moved = shade > 0.0 || breath > 0.0;
	vec4 color = moved ? art_at(texel, size) : (smooth_filter ? texture(tex_linear, UV) : texture(tex_nearest, UV));
	color *= tint;
	color.rgb *= place_tint;
	color.rgb = mix(color.rgb, vec3(dot(color.rgb, vec3(0.299, 0.587, 0.114))), desaturation);

	// Se desfaz embaixo, em pontilhado que sobe devagar.
	float melt = clamp(1.0 - from_ground / (size.y * 0.16), 0.0, 1.0) * shade;
	vec2 cell = mod(floor(texel / unit) + vec2(0.0, floor(TIME * 2.5)), 4.0);
	if ((BAYER[int(cell.x) + int(cell.y) * 4] + 0.5) / 16.0 < melt * 0.7) {
		discard;
	}

	if (fire_strength > 0.0 && color.a > 0.5) {
		// Borda da silhueta: de que lado falta corpo. A luz acende a borda virada para o braseiro.
		float left = 0.0;
		float right = 0.0;
		for (float i = 1.0; i <= 3.0; i += 1.0) {
			left += step(0.5, art_at(texel - vec2(i * unit, 0.0), size).a);
			right += step(0.5, art_at(texel + vec2(i * unit, 0.0), size).a);
		}
		float up = step(0.5, art_at(texel - vec2(0.0, unit), size).a) + step(0.5, art_at(texel - vec2(0.0, 2.0 * unit), size).a);
		float down = step(0.5, art_at(texel + vec2(0.0, unit), size).a) + step(0.5, art_at(texel + vec2(0.0, 2.0 * unit), size).a);
		vec2 outward = vec2((left - right) / 3.0 * sign(dFdx(UV.x)), (down - up) / 2.0);
		// Só o fio de fora acende inteiro; o pixel seguinte, pela metade.
		float facing = dot(outward, normalize(fire_dir));
		float rim = step(0.75, facing) + step(0.45, facing) * (1.0 - step(0.75, facing)) * 0.35;
		// Um pouco de luz no corpo todo do lado da chama, mais o fio na borda.
		float side = clamp(0.5 + (UV.x - 0.5) * sign(dFdx(UV.x)) * sign(fire_dir.x) * 1.2, 0.0, 1.0);
		side = floor(side * fire_bands + 0.5) / fire_bands;
		color.rgb += (color.rgb * (rim * 1.1 + side * 0.22) + vec3(rim * 0.16)) * fire_color * fire_strength;
	}

	// Névoa nos pés: o corpo afunda nela em degraus.
	if (mist_height > 0.0) {
		float sunk = clamp(1.0 - from_ground / (size.y * mist_height), 0.0, 1.0);
		sunk = floor(sunk * 4.0 + 0.5) / 4.0;
		color.rgb = mix(color.rgb, mist_color, sunk * mist_amount * (1.0 - shade * 0.5));
	}

	ALBEDO = mix(color.rgb, vec3(1.0), flash);
	EMISSION = vec3(flash);
	ALPHA = color.a * fade;

	if (dissolve > 0.0) {
		// Um valor por pixel da arte, para o pó sair em pixels inteiros.
		float key = 0.75 * UV.y + 0.25 * hash(floor(UV * size));
		float front = dissolve * (1.0 + dissolve_edge_width);
		if (key < front - dissolve_edge_width) {
			discard;
		}
		if (key < front) {
			ALBEDO = dissolve_edge.rgb;
			EMISSION = dissolve_edge.rgb * 0.6;
		}
	}
}
