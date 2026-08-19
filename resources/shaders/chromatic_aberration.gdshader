shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;
uniform float strength : hint_range(0.0, 0.05) = 0.0;

void fragment() {
	vec2 dir = UV - vec2(0.5);
	vec4 col;
	col.r = texture(SCREEN_TEXTURE, SCREEN_UV + dir * strength).r;
	col.g = texture(SCREEN_TEXTURE, SCREEN_UV).g;
	col.b = texture(SCREEN_TEXTURE, SCREEN_UV - dir * strength).b;
	col.a = 1.0;
	COLOR = col;
}
