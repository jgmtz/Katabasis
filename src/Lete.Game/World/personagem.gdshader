// Sprite de mapa de qualquer personagem (protagonista, companheiros, sombras): o cartão de frente para a câmera ganha
// volume falso (a luz das tochas pega de lado e por trás, em degraus, como pixel art), escurece junto ao chão, respira
// quando parado e, nas sombras dos mortos, ondula, se desfaz embaixo e brilha de leve. Tudo na grade de pixels do sprite.
shader_type spatial;
render_mode cull_disabled;

uniform sampler2D sprite_texture : source_color, filter_nearest, repeat_disable;

// A Marca: perda de cor por faixa.
uniform float desaturation : hint_range(0.0, 1.0) = 0.0;
// 0 = corpo sólido; 1 = sombra dos mortos (ondula, se desfaz embaixo, brilho próprio).
uniform float shade : hint_range(0.0, 1.0) = 0.0;
// Respiração: 0 desliga (andando), 1 liga. A fase desencontra um personagem do outro.
uniform float breath : hint_range(0.0, 1.0) = 1.0;
uniform float phase = 0.0;
// Quanto o corpo sobe (metros), para as sombras que flutuam.
uniform float lift = 0.0;
// Volume falso: quanto a normal vira para os lados perto da borda da silhueta.
uniform float bulge : hint_range(0.0, 2.0) = 0.6;
// Escurecimento junto ao chão (pés assentados).
uniform float ground_dark : hint_range(0.0, 1.0) = 0.45;
// Degraus de luz (pixel art): 0 = luz contínua.
uniform float bands = 4.0;
// Contorno: abaixo desta luminância o pixel da borda é a linha preta do sprite; a cor nova é a do corpo vezes
// outline_shade; outline_blend dosa o efeito (0 = como no arquivo).
uniform float outline_black : hint_range(0.0, 0.5) = 0.12;
uniform float outline_shade : hint_range(0.0, 1.0) = 0.2;
uniform float outline_blend : hint_range(0.0, 1.0) = 0.85;
// Fio de luz no contorno virado para a chama.
uniform float rim_light : hint_range(0.0, 8.0) = 0.7;
uniform vec3 glow_color : source_color = vec3(0.42, 0.62, 0.66);

varying float flip_x;

const float BAYER[16] = float[](
	0.0, 8.0, 2.0, 10.0,
	12.0, 4.0, 14.0, 6.0,
	3.0, 11.0, 1.0, 9.0,
	15.0, 7.0, 13.0, 5.0);

void vertex() {
	// De frente para a câmera, mantendo a escala do nó.
	MODELVIEW_MATRIX = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0], INV_VIEW_MATRIX[1], INV_VIEW_MATRIX[2], MODEL_MATRIX[3]);
	MODELVIEW_MATRIX = MODELVIEW_MATRIX * mat4(
		vec4(length(MODEL_MATRIX[0].xyz), 0.0, 0.0, 0.0),
		vec4(0.0, length(MODEL_MATRIX[1].xyz), 0.0, 0.0),
		vec4(0.0, 0.0, length(MODEL_MATRIX[2].xyz), 0.0),
		vec4(0.0, 0.0, 0.0, 1.0));
	MODELVIEW_NORMAL_MATRIX = mat3(MODELVIEW_MATRIX);
	VERTEX.y += lift / max(length(MODEL_MATRIX[1].xyz), 0.001);
}

float alpha_at(vec2 texel, vec2 size) {
	if (texel.x < 0.0 || texel.y < 0.0 || texel.x >= size.x || texel.y >= size.y) {
		return 0.0;
	}
	return step(0.5, texture(sprite_texture, (texel + 0.5) / size).a);
}

vec4 color_at(vec2 texel, vec2 size) {
	if (texel.x < 0.0 || texel.y < 0.0 || texel.x >= size.x || texel.y >= size.y) {
		return vec4(0.0);
	}
	return texture(sprite_texture, (texel + 0.5) / size);
}

void fragment() {
	vec2 size = vec2(textureSize(sprite_texture, 0));
	vec2 texel = floor(UV * size);
	// Um "pixel" do efeito cresce com o tamanho da arte (56 px = 1; 128 px = 2).
	float unit = max(1.0, floor(size.y / 60.0 + 0.5));
	float from_ground = size.y - 1.0 - texel.y;

	// Sombra dos mortos: as linhas de baixo ondulam para os lados, como pano na água parada.
	float hem = smoothstep(size.y * 0.66, size.y, texel.y) * shade;
	texel.x += floor(sin(TIME * 1.25 + texel.y * (0.14 / unit) + phase) * hem * 1.5 + 0.5) * unit;

	// Respiração: de tempos em tempos o peito e a cabeça descem um pixel (o tronco encurta).
	float chest = floor(size.y * 0.4);
	float inhale = step(0.15, sin(TIME * 1.7 + phase)) * breath;
	if (texel.y < chest) {
		texel.y -= inhale * unit;
	}

	float inside = (texel.x < 0.0 || texel.y < 0.0 || texel.x >= size.x) ? 0.0 : 1.0;
	vec4 color = texture(sprite_texture, (texel + 0.5) / size) * inside;

	// Se desfaz embaixo: pontilhado que sobe devagar.
	float melt = clamp(1.0 - from_ground / (size.y * 0.24), 0.0, 1.0) * shade;
	vec2 cell = mod(floor(texel / unit) + vec2(0.0, floor(TIME * 2.5)), 4.0);
	float threshold = (BAYER[int(cell.x) + int(cell.y) * 4] + 0.5) / 16.0;
	if (color.a < 0.5 || threshold < melt * 0.8) {
		discard;
	}

	// Volume falso: perto da borda esquerda da silhueta a normal vira para a esquerda, e assim por diante.
	float left = 0.0;
	float right = 0.0;
	float up = 0.0;
	float down = 0.0;
	for (float i = 1.0; i <= 3.0; i += 1.0) {
		left += alpha_at(texel - vec2(i * unit, 0.0), size);
		right += alpha_at(texel + vec2(i * unit, 0.0), size);
	}
	up = alpha_at(texel - vec2(0.0, unit), size) + alpha_at(texel - vec2(0.0, 2.0 * unit), size);
	down = alpha_at(texel + vec2(0.0, unit), size) + alpha_at(texel + vec2(0.0, 2.0 * unit), size);
	float side = (left - right) / 3.0 * sign(dFdx(UV.x));
	float rise = (down - up) / 2.0;
	NORMAL = normalize(vec3(side * bulge, rise * bulge * 0.45 + 0.12, 1.0));

	// Contorno do mesmo jeito em todos: a linha preta dos sprites de 56 px vira a cor do corpo, escurecida; quem foi
	// pintado sem linha ganha a borda escurecida. Assim ninguém parece recortado e colado.
	vec3 body = color.rgb;
	vec4 n1 = color_at(texel - vec2(1.0, 0.0), size);
	vec4 n2 = color_at(texel + vec2(1.0, 0.0), size);
	vec4 n3 = color_at(texel - vec2(0.0, 1.0), size);
	vec4 n4 = color_at(texel + vec2(0.0, 1.0), size);
	float is_border = 1.0 - step(0.5, n1.a) * step(0.5, n2.a) * step(0.5, n3.a) * step(0.5, n4.a);
	if (is_border > 0.5) {
		float luma = dot(body, vec3(0.299, 0.587, 0.114));
		if (luma < outline_black) {
			vec3 sum = vec3(0.0);
			float count = 0.0;
			float w1 = step(0.5, n1.a) * step(outline_black, dot(n1.rgb, vec3(0.299, 0.587, 0.114)));
			float w2 = step(0.5, n2.a) * step(outline_black, dot(n2.rgb, vec3(0.299, 0.587, 0.114)));
			float w3 = step(0.5, n3.a) * step(outline_black, dot(n3.rgb, vec3(0.299, 0.587, 0.114)));
			float w4 = step(0.5, n4.a) * step(outline_black, dot(n4.rgb, vec3(0.299, 0.587, 0.114)));
			sum = n1.rgb * w1 + n2.rgb * w2 + n3.rgb * w3 + n4.rgb * w4;
			count = w1 + w2 + w3 + w4;
			if (count > 0.0) {
				body = mix(body, sum / count * outline_shade, outline_blend);
			}
		} else {
			body *= mix(1.0, outline_shade + 0.35, outline_blend);
		}
	}

	vec3 albedo = body * COLOR.rgb;
	albedo = mix(albedo, vec3(dot(albedo, vec3(0.299, 0.587, 0.114))), desaturation);
	// Pés assentados: as últimas linhas escurecem (nas sombras dos mortos, menos).
	float contact = clamp(1.0 - from_ground / (4.0 * unit), 0.0, 1.0);
	albedo *= 1.0 - contact * ground_dark * (1.0 - shade * 0.6);

	ALBEDO = albedo;
	ROUGHNESS = 1.0;
	SPECULAR = 0.0;
	EMISSION = albedo * glow_color * shade * 0.1;
}

void light() {
	// Luz que dá a volta no corpo (pega de lado e um pouco por trás), em degraus.
	float wrap = clamp((dot(NORMAL, LIGHT) + 0.4) / 1.4, 0.0, 1.0);
	if (bands > 0.0) {
		wrap = floor(wrap * bands + 0.5) / bands;
	}
	// Borda acesa: quem está de lado para a chama ganha um fio de luz no contorno.
	float edge = 1.0 - NORMAL.z;
	float rim = edge * edge * clamp(dot(NORMAL, LIGHT) + 0.2, 0.0, 1.0) * rim_light;
	DIFFUSE_LIGHT += (wrap + rim) * ATTENUATION * LIGHT_COLOR / PI;
}
