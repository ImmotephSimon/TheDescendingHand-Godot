shader_type spatial;

render_mode
    unshaded,
    blend_mix,
    depth_draw_never,
    cull_back;


// ============================================================
// Glass Surface
// ============================================================

uniform sampler2D bump_map : hint_normal;
uniform float bump_scale : hint_range(0.0, 1.0) = 0.5;


// ============================================================
// Glass Tint / Surface Texture
// ============================================================

uniform vec4 base_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform sampler2D main_tex : source_color;
uniform float albedo_opacity : hint_range(0.0, 0.5) = 0.05;


// ============================================================
// Refraction
// ============================================================

uniform sampler2D distortion_tex;
uniform float distortion : hint_range(0.0, 0.05) = 0.01;


// Godot's screen texture.
// This contains the scene behind the transparent object.
uniform sampler2D screen_texture : hint_screen_texture, filter_linear;


// ============================================================
// Fresnel
// ============================================================

uniform vec4 fresnel_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float fresnel_power : hint_range(1.0, 8.0) = 5.0;
uniform float min_alpha : hint_range(0.0, 0.3) = 0.05;


// ============================================================
// Fragment
// ============================================================

void fragment()
{
    // --------------------------------------------------------
    // 1. Normal map
    // --------------------------------------------------------

    vec3 normal_ts = texture(bump_map, UV).rgb * 2.0 - 1.0;

    // Match Unity's UnpackNormalScale()
    normal_ts.xy *= bump_scale;

    // TANGENT / BINORMAL / NORMAL are available in view space
    // in the fragment shader.
    vec3 N = normalize(
        TANGENT * normal_ts.x +
        BINORMAL * normal_ts.y +
        NORMAL * normal_ts.z
    );

    // VIEW points from the surface toward the camera.
    vec3 V = normalize(VIEW);


    // --------------------------------------------------------
    // 2. Distortion + tint textures
    // --------------------------------------------------------

    float distortion_mask =
        texture(distortion_tex, UV).r;

    vec3 tint =
        texture(main_tex, UV).rgb *
        base_color.rgb;


    // --------------------------------------------------------
    // 3. Screen-space refraction
    // --------------------------------------------------------

    vec2 refracted_uv =
        SCREEN_UV +
        normal_ts.xy * distortion * distortion_mask;


    // --------------------------------------------------------
    // 4. Scene behind the glass
    // --------------------------------------------------------

    vec3 scene_color =
        texture(screen_texture, refracted_uv).rgb;


    // --------------------------------------------------------
    // 5. Fresnel
    // --------------------------------------------------------

    float NdotV =
        clamp(dot(N, V), 0.0, 1.0);

    float fresnel =
        pow(1.0 - NdotV, fresnel_power);


    // --------------------------------------------------------
    // 6. Fake specular glint
    // --------------------------------------------------------

    vec3 fake_light_dir =
        normalize(vec3(-0.4, 0.8, -0.4));

    vec3 half_dir =
        normalize(fake_light_dir + V);

    float spec =
        pow(
            clamp(dot(N, half_dir), 0.0, 1.0),
            64.0
        );


    // --------------------------------------------------------
    // 7. Composite
    // --------------------------------------------------------

    vec3 final_color = scene_color;

    // Subtle glass tint toward the edges
    final_color =
        mix(
            final_color,
            tint,
            albedo_opacity * fresnel
        );

    // Fresnel rim
    final_color +=
        fresnel_color.rgb *
        fresnel *
        0.35;

    // Sharp glint
    final_color +=
        fresnel_color.rgb *
        spec *
        1.5;


    // --------------------------------------------------------
    // 8. Transparency
    // --------------------------------------------------------

    float alpha =
        clamp(
            fresnel * 0.85 + min_alpha,
            0.0,
            1.0
        );


    ALBEDO = final_color;
    ALPHA = alpha;
}