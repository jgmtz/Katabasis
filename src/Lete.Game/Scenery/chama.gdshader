// Chama em pixel art: um cartão virado para a câmera, com a forma da labareda feita de ruído que sobe, em três cores.
shader_type spatial;
render_mode unshaded, cull_disabled, shadows_disabled, world_vertex_coords, fog_disabled;

uniform sampler2D ruido : filter_nearest, repeat_enable;
uniform vec2 tamanho = vec2(0.42, 0.7);
uniform vec2 pixels = vec2(14.0, 23.0);
uniform vec3 miolo : source_color = vec3(1.0, 0.86, 0.5);
uniform vec3 corpo : source_color = vec3(0.95, 0.52, 0.18);
uniform vec3 ponta : source_color = vec3(0.62, 0.2, 0.1);
uniform float fase = 0.0;
uniform float energia = 1.6;

void vertex() {
	vec3 origin = MODEL_MATRIX[3].xyz;
	vec3 right = normalize(vec3(INV_VIEW_MATRIX[0].x, 0.0, INV_VIEW_MATRIX[0].z));
	VERTEX = origin + right * (UV.x - 0.5) * tamanho.x + vec3(0.0, (1.0 - UV.y) * tamanho.y, 0.0);
}

void fragment() {
	vec2 uv = (floor(UV * pixels) + 0.5) / pixels;
	float t = floor((TIME + fase) * 10.0) / 10.0;
	float up = 1.0 - uv.y;
	float n = texture(ruido, vec2(uv.x * 0.35 + fase, uv.y * 0.5 + t * 0.9)).r;
	float width = (1.0 - up) * 0.5 + 0.06;
	float shape = 1.0 - abs(uv.x - 0.5 + (n - 0.5) * 0.35 * up) / width;
	float heat = shape - up * (0.55 + n * 0.7);
	if (heat < 0.0) {
		discard;
	}
	vec3 color = heat > 0.45 ? miolo : (heat > 0.18 ? corpo : ponta);
	ALBEDO = color * energia;
}
