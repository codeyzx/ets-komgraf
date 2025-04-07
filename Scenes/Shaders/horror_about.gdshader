shader_type canvas_item;

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

// Random and noise functions
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

float noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));
    
    vec2 u = f * f * (3.0 - 2.0 * f);
    
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

// Function to create blood drips
float blood_drip(vec2 uv, float time, float seed) {
    float drip_x = fract(random(vec2(seed, 0.0)) + 0.1 * noise(uv * 5.0));
    float drip_length = 0.1 + 0.3 * random(vec2(seed, 1.0));
    float drip_speed = 0.1 + 0.2 * random(vec2(seed, 2.0));
    
    float drip_pos = mod(time * drip_speed + random(vec2(seed, 3.0)), 1.0 + drip_length) - drip_length;
    
    float drip_width = 0.02 + 0.02 * noise(uv * 10.0 + time);
    float drip = smoothstep(drip_width, 0.0, abs(uv.x - drip_x)) * 
                 smoothstep(0.0, 0.1, uv.y - drip_pos) * 
                 smoothstep(0.0, drip_length, drip_pos + drip_length - uv.y);
    
    // Add some variation to the drip
    drip *= 0.7 + 0.3 * noise(uv * 50.0 + time);
    
    // Add blood pools at bottom
    float pool = smoothstep(0.9, 1.0, uv.y) * smoothstep(drip_width * 2.0, 0.0, abs(uv.x - drip_x));
    
    return drip + pool * 0.5;
}

// Function to create ghost faces
float ghost_face(vec2 uv, float time, float seed) {
    // Position the face
    vec2 face_pos = vec2(
        0.3 + 0.4 * random(vec2(seed, 0.0)),
        0.3 + 0.4 * random(vec2(seed, 1.0))
    );
    
    // Make face appear and disappear
    float visibility = smoothstep(0.0, 0.3, sin(time * 0.2 + random(vec2(seed, 2.0)) * 6.28) * 0.5 + 0.5);
    
    // Create face shape
    vec2 face_uv = (uv - face_pos) / vec2(0.15, 0.2);
    float face_shape = smoothstep(1.0, 0.9, length(face_uv));
    
    // Create eyes
    vec2 left_eye_uv = (uv - (face_pos + vec2(-0.05, -0.03))) / vec2(0.03, 0.04);
    float left_eye = smoothstep(1.0, 0.8, length(left_eye_uv));
    
    vec2 right_eye_uv = (uv - (face_pos + vec2(0.05, -0.03))) / vec2(0.03, 0.04);
    float right_eye = smoothstep(1.0, 0.8, length(right_eye_uv));
    
    // Create mouth
    vec2 mouth_uv = (uv - (face_pos + vec2(0.0, 0.05))) / vec2(0.07, 0.03);
    float mouth = smoothstep(1.0, 0.8, length(mouth_uv));
    
    // Combine features with some distortion
    float face = face_shape - left_eye - right_eye - mouth;
    face *= visibility * (0.7 + 0.3 * noise(uv * 20.0 + time));
    
    return max(0.0, face);
}

// Function to create lightning effect
float lightning(float time, float frequency) {
    return smoothstep(0.9, 1.0, sin(time * 10.0) * 0.5 + 0.5) * step(1.0 - frequency, random(vec2(floor(time * 2.0))));
}

// Function to create static noise like in horror games
float static_noise(vec2 uv, float time) {
    return random(uv + time) * random(uv * 2.0 - time);
}

// Function to create handprint effect
float handprint(vec2 uv, float time, float seed) {
    // Position the handprint
    vec2 hand_pos = vec2(
        0.2 + 0.6 * random(vec2(seed, 0.0)),
        0.2 + 0.6 * random(vec2(seed, 1.0))
    );
    
    // Make handprint appear and disappear
    float visibility = smoothstep(0.0, 0.3, sin(time * 0.1 + random(vec2(seed, 2.0)) * 6.28) * 0.5 + 0.5);
    
    // Create handprint shape
    vec2 hand_uv = (uv - hand_pos) / vec2(0.15, 0.2);
    float hand_shape = smoothstep(1.0, 0.9, length(hand_uv));
    
    // Add finger details
    float fingers = 0.0;
    for (int i = 0; i < 5; i++) {
        float angle = float(i) * 0.2 - 0.4;
        vec2 finger_pos = hand_pos + vec2(cos(angle), sin(angle)) * 0.1;
        vec2 finger_uv = (uv - finger_pos) / vec2(0.03, 0.06);
        fingers += smoothstep(1.0, 0.8, length(finger_uv));
    }
    
    return (hand_shape + fingers * 0.3) * visibility * blood_color.a;
}

// Function to create creepy eye that follows the mouse
float creepy_eye(vec2 uv, float time) {
    // Position the eye in center of screen
    vec2 eye_pos = vec2(0.5, 0.5);
    
    // Make eye appear and disappear randomly
    float visibility = smoothstep(0.9, 1.0, sin(time * 0.3) * 0.5 + 0.5);
    
    // Create eye shape
    vec2 eye_uv = (uv - eye_pos) / vec2(0.1, 0.15);
    float eye_shape = smoothstep(1.0, 0.9, length(eye_uv));
    
    // Create pupil that moves slightly
    vec2 pupil_offset = vec2(
        sin(time * 0.5) * 0.03,
        cos(time * 0.7) * 0.02
    );
    
    vec2 pupil_uv = (uv - (eye_pos + pupil_offset)) / vec2(0.03, 0.04);
    float pupil = smoothstep(1.0, 0.8, length(pupil_uv));
    
    // Create bloodshot effect
    float bloodshot = noise(eye_uv * 5.0 + time * 0.1) * smoothstep(0.6, 1.0, length(eye_uv)) * smoothstep(1.1, 0.9, length(eye_uv));
    
    // Combine features
    float eye = eye_shape - pupil;
    eye = eye + bloodshot * 0.5;
    eye *= visibility * (0.8 + 0.2 * noise(uv * 10.0 + time));
    
    return max(0.0, eye);
}

void fragment() {
    vec2 uv = UV;
    float time = TIME;
    
    // Create distortion effect
    vec2 distorted_uv = uv + vec2(
        distortion_strength * sin(uv.y * 10.0 + time * noise_speed),
        distortion_strength * cos(uv.x * 10.0 + time * noise_speed)
    );
    
    // Create base color with noise
    float noise_val = noise(distorted_uv * noise_scale + time * noise_speed * 0.5);
    vec4 color = background_color;
    
    // Add flickering effect
    float flicker = 1.0 - flicker_intensity * smoothstep(0.3, 0.7, sin(time * flicker_speed));
    color *= flicker;
    
    // Add blood drips
    float blood = 0.0;
    for (float i = 0.0; i < blood_drip_density; i++) {
        blood += blood_drip(distorted_uv, time * blood_drip_speed, i);
    }
    color = mix(color, blood_color, blood * 0.7);
    
    // Add ghost faces
    float ghost = 0.0;
    for (float i = 0.0; i < 3.0; i++) {
        ghost += ghost_face(distorted_uv, time, i) * ghost_face_intensity;
    }
    color = mix(color, ghost_color, ghost);
    
    // Add fog effect
    float fog = noise(distorted_uv * 3.0 + time * 0.1) * fog_density;
    color = mix(color, vec4(ghost_color.rgb, 0.1), fog);
    
    // Add lightning effect
    float light = lightning(time, lightning_frequency) * lightning_intensity;
    color = mix(color, vec4(1.0), light);
    
    // Add static noise like in horror games
    float static_val = static_noise(distorted_uv, time) * static_noise_intensity;
    color = mix(color, vec4(vec3(random(distorted_uv + time)), 1.0), static_val);
    
    // Add handprints
    float hand = 0.0;
    for (float i = 0.0; i < 2.0; i++) {
        hand += handprint(distorted_uv, time, i) * handprint_intensity;
    }
    color = mix(color, blood_color, hand);
    
    // Add creepy eye
    float eye = creepy_eye(distorted_uv, time) * eye_intensity;
    color = mix(color, vec4(1.0, 0.2, 0.1, 1.0), eye);
    
    // Add vignette effect
    float vignette = smoothstep(0.5, 1.5, length(uv - 0.5) * 2.0);
    color = mix(color, vec4(0.0, 0.0, 0.0, 1.0), vignette * 0.7);
    
    // Output final color
    COLOR = color;
}
