// Sombra do personagem no chão: a silhueta do próprio sprite, deitada para longe da luz mais forte (tocha, braseiro,
// lua). A malha é um quadrado 1x1; o vértice é posto no mundo a partir da direção e da largura que o código informa.
shader_type spatial;
render_mode unshaded, cull_disabled, depth_draw_never, world_vertex_coords, shadows_disabled, fog_disabled;

uniform sampler2D sprite_texture : filter_nearest, repeat_disable;
// Dos pés até a ponta da sombra (mundo, no plano do chão).
uniform vec3 reach = vec3(0.0, 0.0, -1.0);
// Largura da base (mundo): atravessada à direção da sombra, com a largura do sprite.
uniform vec3 width = vec3(1.0, 0.0, 0.0);
uniform float strength : hint_range(0.0, 1.0) = 0.5;
uniform float flip = 0.0;
// Altura sobre o chão (mundo): acima das lâminas de chão empilhadas do cenário.
uniform float lift = 0.075;
uniform vec3 tint : source_color = vec3(0.012, 0.018, 0.026);

void vertex() {
	vec3 feet = MODEL_MATRIX[3].xyz;
	VERTEX = feet + width * (UV.x - 0.5) + reach * (1.0 - UV.y) + vec3(0.0, lift, 0.0);
}

void fragment() {
	vec2 uv = vec2(mix(UV.x, 1.0 - UV.x, flip), UV.y);
	float silhouette = step(0.5, texture(sprite_texture, uv).a);
	// Firme nos pés, some para a ponta.
	float along = 1.0 - UV.y;
	ALBEDO = tint;
	ALPHA = silhouette * strength * mix(1.0, 0.4, along * along);
}
