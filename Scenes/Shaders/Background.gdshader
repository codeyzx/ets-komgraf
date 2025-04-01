shader_type canvas_item;

// Declare a uniform for the PNG texture.
uniform sampler2D texture_pattern;

void vertex() {
    // Modify UV coordinates dynamically based on TIME.
    UV.x += TIME * 0.15;
    UV.y += TIME * 0.15;
}

void fragment() {
    // Sample the texture using the modified UV coordinates.
    vec4 tex_color = texture(texture_pattern, UV);

    // Define the three colors: blue, purple, green.
    vec3 blue = vec3(0.0, 0.0, 1.0); // RGB for blue
    vec3 purple = vec3(0.5, 0.0, 0.5); // RGB for purple
    vec3 green = vec3(0.0, 1.0, 0.0); // RGB for green

    // Use TIME to smoothly transition between the three colors.
    float t = mod(TIME, 3.0); // Cycle every 3 seconds.
    float blend1 = smoothstep(0.0, 1.0, t); // Blend from blue to purple (0-1 seconds).
    float blend2 = smoothstep(1.0, 2.0, t); // Blend from purple to green (1-2 seconds).
    float blend3 = smoothstep(2.0, 3.0, t); // Blend from green back to blue (2-3 seconds).

    // Combine the colors based on the blending factors.
    vec3 final_color = mix(blue, purple, blend1); // Blue to Purple
    final_color = mix(final_color, green, blend2); // Purple to Green
    final_color = mix(final_color, blue, blend3); // Green to Blue

    // Apply the final color to the texture.
    COLOR = vec4(final_color, tex_color.a); // Preserve the texture's alpha.

    // Optional: Add distortion effect.
    vec2 distorted_uv = UV + vec2(sin(UV.x * 10.0 + TIME), cos(UV.y * 10.0 + TIME)) * 0.05;
    vec4 distorted_color = texture(texture_pattern, distorted_uv);
    COLOR.rgb = mix(COLOR.rgb, distorted_color.rgb, 0.5); // Blend with distorted texture.
}