// Água escura dos rios do Além: duas camadas de ruído correndo, cortadas em faixas (pixel art), brilho frio nas
// cristas e espuma pálida onde a água encosta na margem ou numa peça (pela profundidade da cena).
shader_type spatial;
render_mode diffuse_lambert, specular_schlick_ggx, depth_draw_never, fog_disabled;

global uniform float texel_density;

uniform sampler2D ruido : filter_nearest, repeat_enable;
uniform sampler2D depth_texture : hint_depth_texture, filter_nearest;
uniform vec3 fundo : source_color = vec3(0.07, 0.13, 0.16);
uniform vec3 meio : source_color = vec3(0.11, 0.21, 0.24);
uniform vec3 crista : source_color = vec3(0.26, 0.43, 0.45);
uniform vec3 espuma : source_color = vec3(0.55, 0.62, 0.6);
uniform vec2 corrente = vec2(0.12, 0.03);
uniform float largura_espuma = 0.35;
// Escurece ou clareia a água inteira (o mar lá embaixo é mais escuro que o rio ao pé).
uniform float tinta = 1.0;
// Acima de zero, a superfície tem luz própria (o Flegetonte, rio de fogo).
uniform float brasa = 0.0;

varying vec3 world;

void vertex() {
	world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	vec2 p = floor(world.xz * texel_density) / texel_density;
	float a = texture(ruido, p * 0.07 + corrente * TIME).r;
	float b = texture(ruido, p * 0.17 - corrente.yx * TIME * 1.7 + vec2(0.37, 0.11)).r;
	float wave = a * 0.6 + b * 0.4;
	vec3 color = wave > 0.64 ? crista : (wave > 0.44 ? meio : fundo);

	// Quanto de água há entre a superfície e o que está atrás dela (vale para a câmera ortográfica).
	float depth = texture(depth_texture, SCREEN_UV).r;
	vec4 behind = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, depth, 1.0);
	float gap = VERTEX.z - behind.z / behind.w;
	float foam_line = largura_espuma * (0.55 + 0.45 * b);
	if (largura_espuma > 0.0 && gap < foam_line) {
		color = mix(espuma, color, step(0.5, fract(wave * 6.0 + TIME * 0.3)) * 0.6);
	}

	ALBEDO = color * tinta;
	EMISSION = color * brasa;
	ROUGHNESS = wave > 0.66 ? 0.25 : 0.6;
	SPECULAR = 0.5;
	ALPHA = 0.94;
}
