shader_type spatial;
render_mode unshaded, cull_disabled;

uniform sampler2D depth_texture : hint_depth_texture, filter_linear_mipmap;
uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;

uniform vec4 fog_color : source_color = vec4(0.75);
uniform vec3 fogCenterWorldPos = vec3(0);
uniform float fogMaxRadius : hint_range(0.01, 1000.0, 0.01) = 0.1;
uniform float fogMinRadius : hint_range(0.01, 1000.0, 0.01) = 0.1;
varying mat4 CAMERA;

void vertex() {
	POSITION = vec4(VERTEX, 1.0);
	CAMERA = INV_VIEW_MATRIX;
}

void fragment() {
	
	vec3 screen_color = texture(screen_texture, SCREEN_UV).rgb;

	float depth = texture(depth_texture, SCREEN_UV).x;
	

	vec3 ndc = vec3((SCREEN_UV * 2.0) - 1.0, depth);
	

	vec4 view = inverse(PROJECTION_MATRIX) * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float linear_depth = -view.z;
	vec4 world = CAMERA * inverse(PROJECTION_MATRIX) * vec4(ndc, 1.0);
  	vec3 world_position = world.xyz / world.w;
	
	float distance_to_fog_center = distance(fogCenterWorldPos, world_position);

    float fog_factor = clamp((distance_to_fog_center - fogMinRadius) / (fogMaxRadius - fogMinRadius), 0.0, 1.0);
	vec4 fogged_color = mix(vec4(screen_color, 1.0), fog_color, fog_factor);

	ALBEDO = fogged_color.rgb;
	ALPHA = fogged_color.a;
}