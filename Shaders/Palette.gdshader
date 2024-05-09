shader_type spatial;
render_mode specular_disabled;

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

const float shadow = 0.5;
const float shadow_width = 0.01;
const vec3 white = vec3(1,1,1);

varying vec3 paint;
varying vec3 paint_shaded;
varying vec3 paint_black;

void fragment() {
	ALBEDO = white;
	
	paint = vec3(0);
	paint_shaded = vec3(0);
	
	paint = mix(paint,colorA.rgb,texture(palette, UV).r);
	paint = mix(paint,colorB.rgb,texture(palette, UV).g);
	paint = mix(paint,colorC.rgb,texture(palette, UV).b);
	paint = mix(paint,colorD.rgb,texture(palette, UV).a);
	
	paint_shaded = mix(paint_shaded,colorA_shaded.rgb,texture(palette, UV).r);
	paint_shaded = mix(paint_shaded,colorB_shaded.rgb,texture(palette, UV).g);
	paint_shaded = mix(paint_shaded,colorC_shaded.rgb,texture(palette, UV).b);
	paint_shaded = mix(paint_shaded,colorD_shaded.rgb,texture(palette, UV).a);
}

void light() {
	float NdotL = dot(NORMAL, LIGHT) * ATTENUATION;
	SPECULAR_LIGHT += NdotL + 0.33;
	
	DIFFUSE_LIGHT = mix(paint_shaded.rgb, paint.rgb, smoothstep(shadow - shadow_width, shadow + shadow_width,SPECULAR_LIGHT)) - SPECULAR_LIGHT;
}