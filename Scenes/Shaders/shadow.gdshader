shader_type canvas_item;

uniform float distortion = 0.2;
uniform float shadow_opacity = 0.7;
uniform float shadow_height = 1.5;
uniform float shadow_width = 0.5;
uniform float time_scale = 1.0;
uniform vec2 shadow_position = vec2(0.5, 0.5);

// Noise function
float noise(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453123);
}

float fbm(vec2 p) {
    float f = 0.0;
    float w = 0.5;
    for (int i = 0; i < 5; i++) {
        f += w * noise(p);
        p *= 2.0;
        w *= 0.5;
    }
    return f;
}

void fragment() {
    // Normalized coordinates centered at shadow_position
    vec2 uv = UV - shadow_position;
    
    // Scale for the humanoid shape
    uv.x /= shadow_width;
    uv.y /= shadow_height;
    
    // Distance from center
    float r = length(uv);
    
    // Create a basic humanoid silhouette
    float shadow = 0.0;
    
    // Head
    float head = smoothstep(0.2, 0.19, length(uv - vec2(0.0, -0.7)));
    
    // Body
    float body = smoothstep(0.15, 0.14, length(vec2(uv.x * 1.5, uv.y * 0.7)));
    
    // Arms
    float arms = smoothstep(0.08, 0.07, length(vec2(uv.x * 0.8 - 0.3, uv.y * 1.2)));
    arms += smoothstep(0.08, 0.07, length(vec2(uv.x * 0.8 + 0.3, uv.y * 1.2)));
    
    // Legs
    float legs = smoothstep(0.1, 0.09, length(vec2(uv.x * 1.2 - 0.15, uv.y * 0.7 + 0.5)));
    legs += smoothstep(0.1, 0.09, length(vec2(uv.x * 1.2 + 0.15, uv.y * 0.7 + 0.5)));
    
    // Combine all parts
    shadow = max(max(max(head, body), arms), legs);
    
    // Add distortion effect
    float time = TIME * time_scale;
    vec2 q = vec2(0.0);
    q.x = fbm(uv + vec2(0.0, time * 0.1));
    q.y = fbm(uv + vec2(0.0, time * -0.1));
    
    vec2 r2 = vec2(0.0);
    r2.x = fbm(uv + q + vec2(1.7, 9.2) + time * 0.15);
    r2.y = fbm(uv + q + vec2(8.3, 2.8) + time * 0.12);
    
    float distort = fbm(uv + r2 * distortion);
    
    // Apply distortion to shadow
    shadow = mix(shadow, shadow * (0.5 + distort * 0.5), distortion);
    
    // Final color with opacity
    COLOR = vec4(0.0, 0.0, 0.0, shadow * shadow_opacity);
}
