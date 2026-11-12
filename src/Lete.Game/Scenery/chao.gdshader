// Mancha de chão (lajes de um caminho, terra batida, campo): a textura no espaço do mundo e a borda roída por ruído,
// para uma mancha se misturar à de baixo sem linha reta. UV vai de 0 a 1 na mancha; tamanho é o lado dela em metros.
shader_type spatial;
render_mode diffuse_lambert, specular_disabled;

global uniform float texel_density;

uniform sampler2D albedo : source_color, filter_nearest, repeat_enable;
uniform sampler2D ruido : filter_nearest, repeat_enable;
uniform vec2 tamanho = vec2(4.0, 4.0);
uniform float borda = 0.8;
uniform float brilho = 1.0;

varying vec3 world;

void vertex() {
	world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	// Tudo em passos de um pixel de arte, para a borda sair em degraus.
	vec2 snapped = floor(world.xz * texel_density) / texel_density;
	vec2 edge = min(UV, 1.0 - UV) * tamanho;
	float inside = min(edge.x, edge.y);
	float n = texture(ruido, snapped * 0.21).r * 0.7 + texture(ruido, snapped * 0.63).r * 0.3;
	if (borda > 0.0 && inside < n * borda) {
		discard;
	}
	vec2 size = vec2(textureSize(albedo, 0));
	ALBEDO = texture(albedo, world.xz * texel_density / size).rgb * COLOR.rgb * brilho;
	ROUGHNESS = 1.0;
}
