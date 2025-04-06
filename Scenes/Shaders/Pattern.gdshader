shader_type canvas_item;

// Shader parameters
uniform float distortion_intensity : hint_range(0.0, 2.0) = 0.3;
uniform vec2 mouse_position = vec2(0.5, 0.5);
uniform float time_offset = 0.0; // For additional randomization
uniform float glitch_intensity : hint_range(0.0, 1.0) = 0.0; // For glitch effects

// Random function for noise generation
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

// Improved noise function
vec2 hash(vec2 p) {
    p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3)));
    return -1.0 + 2.0 * fract(sin(p) * 43758.5453123);
}

float noise(vec2 p) {
    const float K1 = 0.366025404; // (sqrt(3)-1)/2
    const float K2 = 0.211324865; // (3-sqrt(3))/6
    
    vec2 i = floor(p + (p.x + p.y) * K1);
    vec2 a = p - i + (i.x + i.y) * K2;
    vec2 o = (a.x > a.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    vec2 b = a - o + K2;
    vec2 c = a - 1.0 + 2.0 * K2;
    
    vec3 h = max(0.5 - vec3(dot(a, a), dot(b, b), dot(c, c)), 0.0);
    vec3 n = h * h * h * h * vec3(dot(a, hash(i + 0.0)), dot(b, hash(i + o)), dot(c, hash(i + 1.0)));
    
    return dot(n, vec3(70.0));
}

// Fractal Brownian Motion for layered noise
float fbm(vec2 uv) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 0.0;
    
    // Loop of octaves
    for (int i = 0; i < 5; i++) {
        value += amplitude * noise(uv);
        uv *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

void vertex() {
    // Subtle vertex displacement for a more dynamic feel
    VERTEX.x += sin(TIME * 0.3 + VERTEX.y * 0.01) * 1.0;
    VERTEX.y += cos(TIME * 0.2 + VERTEX.x * 0.01) * 1.0;
    
    // Modify UV coordinates dynamically
	UV.x += TIME * 0.02;
	UV.y += TIME * 0.01;
}

void fragment() {
	// Calculate distance from mouse position with more influence
	float mouse_dist = distance(mouse_position, UV);
	float mouse_influence = smoothstep(0.4, 0.0, mouse_dist);
	
	// Create a creepy pattern that follows the mouse
	vec2 uv = UV;
	
	// Create pulsating effects
	float slow_pulse = (sin(TIME * 0.5) * 0.5 + 0.5);
	float fast_pulse = (sin(TIME * 2.0) * 0.5 + 0.5);
	
	// Create horror-inspired patterns
	
	// 1. Resident Evil style mold/infection pattern
	float mold_pattern = fbm(uv * 8.0 + TIME * 0.1);
	mold_pattern = smoothstep(0.4, 0.6, mold_pattern);
	
	// 2. Silent Hill style rust/decay pattern
	float rust_pattern = fbm(uv * 5.0 - TIME * 0.05);
	rust_pattern = pow(rust_pattern, 2.0) * 0.8;
	
	// 3. Outlast night vision noise effect
	float noise_pattern = random(uv + TIME * 0.1) * 0.1;
	
	// 4. Create a "The Ring" hair-like effect
	float hair_effect = 0.0;
	if (mouse_dist < 0.3) {
	    for (float i = 0.0; i < 12.0; i++) {
	        float angle = i * 3.14159 / 6.0;
	        vec2 dir = vec2(cos(angle), sin(angle));
	        float dist = smoothstep(0.3, 0.0, mouse_dist) * 0.3;
	        
	        vec2 hair_uv = uv - mouse_position;
	        hair_uv = vec2(
	            hair_uv.x * cos(TIME * 0.2) - hair_uv.y * sin(TIME * 0.2),
	            hair_uv.x * sin(TIME * 0.2) + hair_uv.y * cos(TIME * 0.2)
	        );
	        
	        float hair_line = smoothstep(0.01, 0.0, abs(dot(normalize(hair_uv), dir) - 0.5) - 0.01 * (1.0 + sin(TIME * 2.0 + i)));
	        hair_effect = max(hair_effect, hair_line * (1.0 - length(hair_uv) / dist));
	    }
	}
	
	// 5. Create handprints that appear near mouse
	float handprint = 0.0;
	if (mouse_dist < 0.2 && random(vec2(floor(TIME))) < 0.3) {
	    vec2 hand_uv = uv - mouse_position;
	    hand_uv = vec2(
	        hand_uv.x * cos(TIME * 0.1) - hand_uv.y * sin(TIME * 0.1),
	        hand_uv.x * sin(TIME * 0.1) + hand_uv.y * cos(TIME * 0.1)
	    );
	    
	    // Create a simple hand shape
	    float palm = smoothstep(0.05, 0.04, length(hand_uv));
	    
	    // Add fingers
	    for (float i = 0.0; i < 5.0; i++) {
	        float angle = mix(-0.3, 0.3, i / 4.0);
	        vec2 finger_dir = vec2(cos(angle), sin(angle));
	        float finger = smoothstep(0.03, 0.02, length(hand_uv - finger_dir * 0.05));
	        palm = max(palm, finger);
	    }
	    
	    handprint = palm * smoothstep(0.2, 0.0, mouse_dist);
	}
	
	// 6. Create blood drips
	float blood_drips = 0.0;
	for (float i = 0.0; i < 8.0; i++) {
	    // Random positions for blood
	    vec2 blood_pos = vec2(
	        sin(TIME * 0.05 + i * 1.0) * 0.4 + 0.5,
	        cos(TIME * 0.07 + i * 2.0) * 0.4 + 0.5
	    );
	    
	    // Blood drop shape
	    float blood_drop = smoothstep(0.05, 0.0, length(uv - blood_pos));
	    
	    // Create dripping effect
	    float drip_length = 0.1 + 0.1 * sin(TIME * 0.2 + i);
	    float drip_width = 0.01 + 0.005 * cos(TIME * 0.3 + i);
	    
	    float drip = 0.0;
	    if (uv.y < blood_pos.y && uv.y > blood_pos.y - drip_length) {
	        drip = smoothstep(drip_width, 0.0, abs(uv.x - blood_pos.x));
	        drip *= smoothstep(0.0, 0.05, blood_pos.y - uv.y); // Fade at top
	        drip *= smoothstep(0.05, 0.0, blood_pos.y - drip_length - uv.y); // Fade at bottom
	    }
	    
	    blood_drips = max(blood_drips, max(blood_drop, drip));
	}
	
	// 7. Create a "scratches on the screen" effect
	float scratches = 0.0;
	for (float i = 0.0; i < 5.0; i++) {
	    float y_pos = fract(0.2 * i + TIME * 0.01);
	    float x_variance = sin(y_pos * 20.0) * 0.1;
	    float scratch_width = 0.002 * (1.0 + sin(TIME + i));
	    
	    scratches += smoothstep(scratch_width, 0.0, abs(uv.y - y_pos)) * 0.1;
	}
	
	// 8. Create a "eyes watching you" effect
	float eyes_effect = 0.0;
	if (random(vec2(floor(TIME * 0.2))) < 0.15) { // Occasionally show eyes
	    for (float i = 0.0; i < 3.0; i++) {
	        vec2 eye_pos = vec2(
	            random(vec2(i, floor(TIME * 0.1))) * 0.8 + 0.1,
	            random(vec2(i + 10.0, floor(TIME * 0.1))) * 0.8 + 0.1
	        );
	        
	        float eye_shape = smoothstep(0.03, 0.02, length(uv - eye_pos));
	        float pupil = smoothstep(0.01, 0.0, length(uv - eye_pos - normalize(mouse_position - eye_pos) * 0.01));
	        
	        eyes_effect = max(eyes_effect, eye_shape - pupil);
	    }
	}
	
	// Combine all patterns with weights
	float pattern = 0.0;
	pattern = max(pattern, mold_pattern * 0.5);
	pattern = max(pattern, rust_pattern * 0.4);
	pattern = max(pattern, blood_drips * 0.9);
	pattern = max(pattern, handprint * 0.8);
	pattern = max(pattern, hair_effect * 0.7);
	pattern = max(pattern, eyes_effect * 0.6);
	pattern += scratches * 0.3;
	pattern += noise_pattern * 0.2;
	
	// Enhance pattern near mouse
	pattern = mix(pattern, pattern * 1.5, mouse_influence);
	
	// Apply color - dark blood red base
	vec3 pattern_color = vec3(0.4, 0.0, 0.0);
	
	// Add variation to color
	pattern_color = mix(pattern_color, vec3(0.3, 0.0, 0.0), rust_pattern);
	pattern_color = mix(pattern_color, vec3(0.5, 0.0, 0.0), blood_drips);
	pattern_color = mix(pattern_color, vec3(0.1, 0.0, 0.0), mold_pattern);
	
	// Make it more intense near mouse - add a sickly green tint for infection feel
	pattern_color = mix(pattern_color, vec3(0.2, 0.3, 0.0), mouse_influence * mold_pattern);
	
	// Apply pulse to alpha
	float alpha = pattern * (0.6 + 0.4 * slow_pulse);
	
	// Add occasional flicker
	if (random(vec2(floor(TIME * 5.0))) < 0.1) {
		alpha *= random(vec2(floor(TIME * 20.0)));
	}
	
	// Add glitch effect
	if (random(vec2(floor(TIME * 2.0))) < 0.05) {
	    pattern_color = mix(pattern_color, vec3(1.0, 1.0, 1.0), 0.8);
	    alpha *= 0.8;
	}
	
	// Apply glitch intensity
	pattern_color = mix(pattern_color, vec3(1.0, 1.0, 1.0), glitch_intensity);
	alpha *= 1.0 - glitch_intensity;
	
	// Output final color with transparency
	COLOR = vec4(pattern_color, alpha);
}
