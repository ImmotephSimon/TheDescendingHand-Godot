shader_type spatial;

render_mode unshaded, cull_disabled, depth_draw_opaque;

uniform vec4 light_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4 dark_color : source_color = vec4(0.2, 0.0, 0.0, 1.0);
uniform float fill_percentage : hint_range(0.0, 1.0, 0.01) = 1.0;
uniform vec3 current_tilt = vec3(0.0);

varying vec3 position_os;
varying vec3 normal_ws;
varying vec3 view_dir_ws;

void vertex()
{
    position_os = VERTEX;

    normal_ws = normalize(MODEL_NORMAL_MATRIX * NORMAL);

    vec3 position_ws = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    view_dir_ws = normalize(CAMERA_POSITION_WORLD - position_ws);
}

void fragment()
{
    // 2D Wave Motion across X and Z
    float wave_x = sin(position_os.x * 7.0 + TIME * 2.5) * 0.012;
    float wave_z = cos(position_os.z * 7.0 + TIME * 2.0) * 0.012;
    float wave = wave_x + wave_z;

    float height = (position_os.y - (-0.5)) / (0.5 - (-0.5));
    float tilted_height = height + dot(position_os, current_tilt) + wave;

    float fill_line = tilted_height - fill_percentage;

    if (fill_line > 0.0)
    {
        discard;
    }

    // Dynamic cap normal based on tilt and wave slope
    vec3 tilt_normal = normalize(vec3(
        -current_tilt.x - wave_x * 2.0,
        1.0,
        -current_tilt.z - wave_z * 2.0
    ));

    vec3 view_dir = normalize(view_dir_ws);

    vec3 normal = FRONT_FACING
        ? normalize(normal_ws)
        : tilt_normal;

    if (FRONT_FACING)
    {
        float edge = clamp(1.0 - abs(fill_line * 10.0), 0.0, 1.0);
        float surface_glow = pow(edge, 8.0) * 5.0;

        float fresnel = pow(
            1.0 - clamp(dot(normal, view_dir), 0.0, 1.0),
            3.0
        );

        float depth = clamp(height, 0.0, 1.0);

        vec3 liquid_color = mix(
            dark_color.rgb,
            light_color.rgb,
            depth
        );

        vec3 base_color = mix(
            liquid_color,
            light_color.rgb,
            fresnel
        );

        float pulse = 0.8 + sin(TIME * 3.0) * 0.2;
        vec3 emissive_color = light_color.rgb * surface_glow * pulse;

        ALBEDO = base_color + emissive_color;
    }
    else
    {
        // Radial distance for meniscus contact rim
        float radial_dist = clamp(length(position_os.xz) * 2.0, 0.0, 1.0);
        float meniscus = smoothstep(0.55, 1.0, radial_dist);

        // Fixed HUD Light Specular Glint
        vec3 fake_light_dir = normalize(vec3(-0.4, 1.0, -0.4));
        vec3 half_dir = normalize(fake_light_dir + view_dir);

        float spec = pow(
            clamp(dot(normal, half_dir), 0.0, 1.0),
            24.0
        );

        // Radial center depth gradient
        float center_depth = 1.0 -
            clamp(length(position_os.xz) * 1.5, 0.0, 1.0);

        vec3 cap_color = mix(
            dark_color.rgb,
            light_color.rgb,
            center_depth * 0.8 + 0.2
        );

        // Apply Meniscus Rim Darkening and Specular Highlight
        cap_color *= mix(1.0, 0.3, meniscus);
        cap_color += spec * light_color.rgb * 0.9;

        ALBEDO = cap_color;
    }
}