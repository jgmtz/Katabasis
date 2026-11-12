// Flora em cartões (asfódelos, capim): cada instância é um cartão virado para a câmera (só no eixo vertical), cortado
// de uma folha de variantes, que balança ao vento pela ponta. Iluminado como o chão (normal para cima).
shader_type spatial;
render_mode cull_disabled, diffuse_lambert, specular_disabled, world_vertex_coords, shadows_disabled;

uniform sampler2D folha : source_color, filter_nearest;
uniform float variantes = 4.0;
uniform vec2 tamanho = vec2(0.5, 1.2);
uniform float vento = 0.06;
uniform float brilho = 1.0;

varying float variante;

void vertex() {
	// INSTANCE_CUSTOM: x = variante, y = escala, z = fase do vento.
	variante = INSTANCE_CUSTOM.x;
	vec3 origin = MODEL_MATRIX[3].xyz;
	vec3 right = normalize(vec3(INV_VIEW_MATRIX[0].x, 0.0, INV_VIEW_MATRIX[0].z));
	float scale = INSTANCE_CUSTOM.y;
	float top = 1.0 - UV.y;
	float sway = sin(TIME * 1.3 + origin.x * 0.7 + origin.z * 0.9 + INSTANCE_CUSTOM.z * 6.28) * vento * top * top;
	VERTEX = origin + right * ((UV.x - 0.5) * tamanho.x * scale + sway) + vec3(0.0, top * tamanho.y * scale, 0.0);
	NORMAL = vec3(0.0, 1.0, 0.0);
}

void fragment() {
	vec2 uv = vec2((UV.x + floor(variante)) / variantes, UV.y);
	vec4 texel = texture(folha, uv);
	if (texel.a < 0.5) {
		discard;
	}
	ALBEDO = texel.rgb * brilho;
	ROUGHNESS = 1.0;
}
