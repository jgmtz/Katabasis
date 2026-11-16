// Véu das transições (mapa ↔ luta): névoa escura que fecha e abre em blocos de pixel, por um pontilhado que se
// arrasta, em vez de um escurecer liso. amount 0 = aberto, 1 = fechado.
shader_type canvas_item;

uniform float amount : hint_range(0.0, 1.0) = 0.0;
uniform vec4 mist : source_color = vec4(0.05, 0.06, 0.08, 1.0);
uniform vec4 glow : source_color = vec4(0.18, 0.24, 0.26, 1.0);
// Tamanho do bloco, em pixels da tela.
uniform float block = 6.0;

const float BAYER[16] = float[](
	0.0, 8.0, 2.0, 10.0,
	12.0, 4.0, 14.0, 6.0,
	3.0, 11.0, 1.0, 9.0,
	15.0, 7.0, 13.0, 5.0);

float hash(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);
	f = f * f * (3.0 - 2.0 * f);
	return mix(mix(hash(i), hash(i + vec2(1.0, 0.0)), f.x), mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0, 1.0)), f.x), f.y);
}

void fragment() {
	vec2 cell = floor(FRAGCOORD.xy / block);
	// Névoa: rolos grandes que andam devagar, mais o pontilhado ordenado do pixel art.
	float n = noise(cell * 0.06 + vec2(TIME * 0.35, TIME * 0.12)) * 0.6 + noise(cell * 0.17 - vec2(TIME * 0.2, 0.0)) * 0.4;
	float dither = (BAYER[int(mod(cell.x, 4.0)) + int(mod(cell.y, 4.0)) * 4] + 0.5) / 16.0;
	float key = n * 0.7 + dither * 0.3;
	float edge = amount * 1.25 - 0.12;
	if (key > edge) {
		discard;
	}
	// A borda do véu, onde ele está chegando, é mais clara: fios de névoa.
	COLOR = mix(mist, glow, step(edge - 0.08, key) * step(amount, 0.98));
}
