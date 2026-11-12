// Superfície do cenário (paisagem HD-2D, PENDENCIAS 105): textura em pixel art projetada pelos três eixos, na mesma
// densidade de pixel dos personagens (texel_density, px por metro, definida pelo Main), sem filtro. A cor do vértice
// tinge a peça (variação e sujeira); a base escurece perto do chão para a peça assentar.
shader_type spatial;
render_mode diffuse_lambert, specular_disabled;

global uniform float texel_density;

uniform sampler2D albedo : source_color, filter_nearest, repeat_enable;
// Projeção no espaço do mundo (chão, rochas) ou no da peça (telhado e tábuas acompanham a rotação).
uniform bool mundo = false;
uniform float escurecer_base = 0.0;
uniform float brilho = 1.0;

varying vec3 pos;
varying vec3 nrm;
varying float altura;

void vertex() {
	vec3 world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	pos = mundo ? world : VERTEX;
	nrm = mundo ? normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz) : NORMAL;
	altura = world.y;
}

void fragment() {
	vec3 a = abs(nrm);
	vec2 uv = a.y >= a.x && a.y >= a.z ? pos.xz : (a.x >= a.z ? pos.zy : pos.xy);
	vec2 size = vec2(textureSize(albedo, 0));
	vec3 color = texture(albedo, uv * texel_density / size * vec2(1.0, -1.0)).rgb;
	float base = 1.0 - escurecer_base * (1.0 - smoothstep(0.0, 0.9, altura));
	ALBEDO = color * COLOR.rgb * base * brilho;
	ROUGHNESS = 1.0;
}
