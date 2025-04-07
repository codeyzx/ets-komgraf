shader_type canvas_item;

// Base colors and parameters
uniform vec4 background_color : source_color = vec4(0.03, 0.01, 0.02, 1.0);
uniform vec4 blood_color : source_color = vec4(0.6, 0.0, 0.0, 1.0);
uniform vec4 ghost_color : source_color = vec4(0.7, 0.7, 0.8, 0.3);
uniform float noise_scale = 20.0;
uniform float noise_speed = 0.2;
uniform float distortion_strength = 0.05;
uniform float blood_drip_speed = 0.5;
uniform float blood_drip_density = 8.0;
uniform float flicker_speed = 5.0;
uniform float flicker_intensity = 0.15;
uniform float ghost_face_intensity = 0.2;
uniform float fog_density = 0.4;
uniform float lightning_frequency = 0.05;
uniform float lightning_intensity = 0.3;
uniform float static_noise_intensity = 0.15;
uniform float handprint_intensity = 0.3;
uniform float eye_intensity = 0.4;

// Noise functions
float random(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453123);
}

float noise(vec2 uv) {
    vec2 i = floor(uv);
    vec2 f = fract(uv);
    
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));
    
    vec2 u = f * f * (3.0 - 2.0 * f);
    
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

float fbm(vec2 uv) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    
    for (int i = 0; i < 5; i++) {
        value += amplitude * noise(frequency * uv);
        amplitude *= 0.5;
        frequency *= 2.0;
    }
    
    return value;
}

// Blood drip effect
float blood_drip(vec2 uv, float time) {
    float drips = 0.0;
    
    for (float i = 0.0; i < blood_drip_density; i++) {
        float x_pos = fract(random(vec2(i, i * 0.5)) + 0.01 * time);
        float y_flow = mod(time * blood_drip_speed + random(vec2(i, i * 0.2)), 1.0);
        float drip_width = 0.02 * (0.5 + random(vec2(i, i * 0.7)));
        
        if (uv.x > x_pos - drip_width && uv.x < x_pos + drip_width) {
            float drip_shape = smoothstep(0.0, 0.1, 1.0 - abs((uv.x - x_pos) / drip_width));
            float drip_mask = smoothstep(0.0, 0.1, uv.y - y_flow);
            drips += drip_shape * drip_mask;
        }
    }
    
    return clamp(drips, 0.0, 1.0);
}

// Ghost face effect
float ghost_face(vec2 uv, float time) {
    // Center the coordinates
    vec2 centered = uv - 0.5;
    
    // Create a pulsing effect
    float pulse = 0.5 + 0.5 * sin(time * 0.5);
    
    // Create a face-like shape
    float face = smoothstep(0.3, 0.29, length(centered / vec2(1.0, 1.5)));
    
    // Create eyes
    float left_eye = smoothstep(0.05, 0.04, length(centered - vec2(-0.1, 0.1)));
    float right_eye = smoothstep(0.05, 0.04, length(centered - vec2(0.1, 0.1)));
    
    // Create a mouth
    float mouth = smoothstep(0.1, 0.09, length(centered - vec2(0.0, -0.1)) - 0.1);
    
    // Combine features and apply pulsing
    return (face - left_eye - right_eye - mouth) * pulse * ghost_face_intensity;
}

// Handprint effect
float handprint(vec2 uv, float time) {
    // Create several handprints at different positions
    float print1 = smoothstep(0.1, 0.09, length(uv - vec2(0.2, 0.3)));
    float print2 = smoothstep(0.1, 0.09, length(uv - vec2(0.8, 0.7)));
    float print3 = smoothstep(0.1, 0.09, length(uv - vec2(0.5, 0.9)));
    
    // Add some fingers to make it look like handprints
    float fingers1 = smoothstep(0.05, 0.04, length(uv - vec2(0.25, 0.25)));
    float fingers2 = smoothstep(0.05, 0.04, length(uv - vec2(0.85, 0.65)));
    float fingers3 = smoothstep(0.05, 0.04, length(uv - vec2(0.55, 0.85)));
    
    // Combine and fade based on time
    float fade = 0.5 + 0.5 * sin(time * 0.2);
    return ((print1 + print2 + print3) - (fingers1 + fingers2 + fingers3)) * fade * handprint_intensity;
}

// Eye effect
float eye_effect(vec2 uv, float time) {
    // Create a pulsing eye that follows the mouse
    vec2 eye_pos = vec2(0.5 + 0.3 * sin(time * 0.3), 0.5 + 0.3 * cos(time * 0.2));
    
    // Create the eye shape
    float eye = smoothstep(0.1, 0.09, length((uv - eye_pos) / vec2(1.5, 1.0)));
    
    // Create the pupil
    float pupil = smoothstep(0.04, 0.03, length(uv - eye_pos));
    
    // Create the iris
    float iris = smoothstep(0.06, 0.05, length(uv - eye_pos)) - pupil;
    
    // Combine and apply intensity
    return (eye - iris - pupil) * eye_intensity;
}

// Lightning effect
float lightning(float time) {
    return step(1.0 - lightning_frequency, random(vec2(floor(time * 10.0), 0.0))) * lightning_intensity;
}

// Static noise effect
float static_noise(vec2 uv, float time) {
    return random(uv + time) * static_noise_intensity;
}

void fragment() {
    // Get the base UV coordinates
    vec2 uv = UV;
    
    // Apply time-based distortion
    float time = TIME;
    float distortion = fbm(uv * noise_scale + time * noise_speed) * distortion_strength;
    vec2 distorted_uv = uv + vec2(distortion, distortion);
    
    // Create base color with fog effect
    float fog = fbm(distorted_uv * 4.0 + vec2(time * 0.1, 0.0)) * fog_density;
    vec4 color = mix(background_color, ghost_color, fog);
    
    // Add blood drips
    float blood = blood_drip(distorted_uv, time);
    color = mix(color, blood_color, blood);
    
    // Add ghost face
    float ghost = ghost_face(distorted_uv, time);
    color = mix(color, ghost_color, ghost);
    
    // Add handprint effect
    float hand = handprint(distorted_uv, time);
    color = mix(color, blood_color, hand);
    
    // Add eye effect
    float eye = eye_effect(distorted_uv, time);
    color = mix(color, vec4(1.0, 0.0, 0.0, 1.0), eye);
    
    // Add flickering effect
    float flicker = 1.0 - flicker_intensity * random(vec2(floor(time * flicker_speed), 0.0));
    color.rgb *= flicker;
    
    // Add lightning flashes
    float light = lightning(time);
    color.rgb += light;
    
    // Add static noise
    float static = static_noise(uv, time);
    color.rgb += static;
    
    // Output the final color
    COLOR = color;
}
