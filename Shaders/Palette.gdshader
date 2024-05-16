shader_type spatial;
render_mode specular_disabled,world_vertex_coords,ambient_light_disabled;
uniform vec4 colorA : source_color = vec4(1,0.4,0,1);
uniform vec4 colorB : source_color = vec4(0.4,1,0,1);
uniform vec4 colorC : source_color = vec4(0.5,0.5,1,1);
uniform vec4 colorD : source_color = vec4(0.5,0.5,0.5,1);
uniform vec4 colorA_shaded: source_color = vec4(0.5,0,0,1);
uniform vec4 colorB_shaded: source_color = vec4(0,0.5,0,1);
uniform vec4 colorC_shaded: source_color = vec4(0,0,0.5,1);
uniform vec4 colorD_shaded: source_color = vec4(0.25,0.25,0.25,1);
uniform vec4 colorA_black: source_color = vec4(0.1,0,0,1);
uniform vec4 colorB_black: source_color = vec4(0,0.1,0,1);
uniform vec4 colorC_black: source_color = vec4(0,0,0.1,1);
uniform vec4 colorD_black: source_color = vec4(0,0,0,1);

uniform sampler2D palette : source_color, hint_default_white;
uniform vec3 triplanar_scale = vec3(1.0, 1.0, 1.0);
	
const float shadow = 0.2;
const float shadow_width = 0.001;
const vec3 white = vec3(1,1,1);
const float triplanar_sharpness = 10.0;

varying vec3 paint;
varying vec3 paint_shaded;
varying vec3 paint_black;
varying vec3 triplanar_power_normal;
varying vec3 triplanar_pos;

vec4 triplanar_texture(sampler2D p_sampler, vec3 p_weights, vec3 p_triplanar_pos) {
	vec4 samp = vec4(0.0);
	samp += texture(p_sampler, p_triplanar_pos.xy) * p_weights.z;
	samp += texture(p_sampler, p_triplanar_pos.xz) * p_weights.y;
	samp += texture(p_sampler, p_triplanar_pos.zy * vec2(-1.0, 1.0)) * p_weights.x;
	return samp;
}

void vertex() {
// TextureParameterTriplanar
	{
		triplanar_power_normal = pow(abs(NORMAL), vec3(triplanar_sharpness));
		triplanar_power_normal /= dot(triplanar_power_normal, vec3(1.0));
		triplanar_pos = VERTEX * triplanar_scale;
		triplanar_pos *= vec3(1.0, -1.0, 1.0);
	}
}

void fragment() {
	ALBEDO = white;
	
	paint = vec3(0);
	paint_shaded = vec3(0);
	
	paint = mix(paint,colorA.rgb,triplanar_texture(palette, triplanar_power_normal, triplanar_pos).r);
	paint = mix(paint,colorB.rgb,triplanar_texture(palette, triplanar_power_normal, triplanar_pos).g);
	paint = mix(paint,colorC.rgb,triplanar_texture(palette, triplanar_power_normal, triplanar_pos).b);
	paint = mix(paint,colorD.rgb,triplanar_texture(palette, triplanar_power_normal, triplanar_pos).a);
	
	paint_shaded = mix(paint_shaded,colorA_shaded.rgb,triplanar_texture(palette, triplanar_power_normal, triplanar_pos).r);
	paint_shaded = mix(paint_shaded,colorB_shaded.rgb,triplanar_texture(palette, triplanar_power_normal, triplanar_pos).g);
	paint_shaded = mix(paint_shaded,colorC_shaded.rgb,triplanar_texture(palette, triplanar_power_normal, triplanar_pos).b);
	paint_shaded = mix(paint_shaded,colorD_shaded.rgb,triplanar_texture(palette, triplanar_power_normal, triplanar_pos).a);
}

void light() { 
	SPECULAR_LIGHT += ATTENUATION * LIGHT_COLOR.r * max(dot(LIGHT,NORMAL), 0.0);
	
	DIFFUSE_LIGHT = mix(paint_shaded.rgb, paint.rgb, smoothstep(shadow - shadow_width, shadow + shadow_width,SPECULAR_LIGHT)) - SPECULAR_LIGHT;
}