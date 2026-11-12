// Névoa rasteira: uma lâmina horizontal de ruído que se arrasta, em passos de pixel de arte e poucos degraus de
// opacidade (pixel art), que some perto do que ela corta (pela profundidade) e nas bordas da lâmina.
shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_never, cull_disabled, shadows_disabled, fog_disabled;

global uniform float texel_density;

uniform sampler2D ruido : filter_nearest, repeat_enable;
uniform sampler2D depth_texture : hint_depth_texture, filter_nearest;
uniform vec3 cor : source_color = vec3(0.2, 0.26, 0.27);
uniform float densidade = 0.5;
uniform vec2 deriva = vec2(0.035, 0.012);
uniform float escala = 0.05;
uniform float degraus = 4.0;
// Véu em pé (um portal fechado): o ruído corre pela altura, e não pelo chão.
uniform bool em_pe = false;

varying vec3 world;

void vertex() {
	world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	// Em pé, o ruído é esticado na altura e sobe devagar: fios de névoa, e não uma mancha parada.
	vec2 plane = em_pe ? vec2((world.x + world.z) * 2.2, world.y * 0.55 - TIME * 0.35) : world.xz;
	vec2 p = floor(plane * texel_density * 0.5) / (texel_density * 0.5);
	float n = texture(ruido, p * escala + deriva * TIME).r * 0.65
		+ texture(ruido, p * escala * 2.3 - deriva.yx * TIME * 1.4).r * 0.35;
	float a = (em_pe ? 0.12 + 0.88 * smoothstep(0.28, 0.72, n) : smoothstep(0.42, 0.85, n)) * densidade;

	float depth = texture(depth_texture, SCREEN_UV).r;
	vec4 behind = INV_PROJECTION_MATRIX * vec4(SCREEN_UV * 2.0 - 1.0, depth, 1.0);
	float gap = VERTEX.z - behind.z / behind.w;
	a *= smoothstep(0.0, 0.6, gap);

	vec2 edge = min(UV, 1.0 - UV);
	if (em_pe) {
		// A borda do véu é roída pelo próprio ruído e ele é mais ralo em cima do que ao pé.
		a *= smoothstep(0.0, 0.3, min(edge.x * 1.4, UV.y) + (n - 0.5) * 0.3);
		a *= mix(0.55, 1.0, UV.y);
	} else {
		a *= smoothstep(0.0, 0.12, min(edge.x, edge.y));
	}

	ALBEDO = cor;
	ALPHA = floor(a * degraus + 0.5) / degraus * 0.7;
}
