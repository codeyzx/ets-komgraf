shader_type canvas_item;

uniform float noise_density = 0.5;
uniform float noise_speed = 0.1;
uniform vec2 noise_scale = vec2(1.0, 1.0);

// Random function from Book of Shaders
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

// Noise function
float noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    
    // Four corners in 2D of a tile
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));

    // Smooth interpolation
    vec2 u = f * f * (3.0 - 2.0 * f);

    // Mix 4 corners
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

void fragment() {
    // Animated noise
    vec2 st = UV * noise_scale;
    st.y += TIME * noise_speed;
    
    // Layer multiple noise functions for more organic look
    float n = noise(st * 10.0) * 0.5;
    n += noise(st * 20.0) * 0.3;
    n += noise(st * 40.0) * 0.2;
    
    // Apply noise density
    n = n * noise_density;
    
    // Output - make sure alpha is visible
    COLOR = vec4(vec3(n), n * 0.7);
}
