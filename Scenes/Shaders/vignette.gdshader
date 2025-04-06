shader_type canvas_item;

uniform float vignette_intensity = 0.4;
uniform float vignette_opacity = 0.5;
uniform vec2 vignette_position = vec2(0.5, 0.5);
uniform float pulse_speed = 1.0;
uniform float pulse_intensity = 0.0;

void fragment() {
    // Calculate distance from center (or custom position)
    vec2 relative_position = UV - vignette_position;
    float dist = length(relative_position);
    
    // Calculate pulsing effect
    float pulse = sin(TIME * pulse_speed) * 0.5 + 0.5;
    float intensity = vignette_intensity + pulse_intensity * pulse * 0.3;
    
    // Create vignette effect
    float vignette = smoothstep(0.8, intensity, dist);
    vignette = pow(vignette, 1.5);
    
    // Apply vignette with opacity
    COLOR = vec4(0.0, 0.0, 0.0, vignette * vignette_opacity);
}
