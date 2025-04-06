shader_type canvas_item;

// Shader parameters
uniform float distortion_intensity : hint_range(0.0, 2.0) = 0.3;
uniform vec2 mouse_position = vec2(0.5, 0.5);
uniform float time_offset = 0.0; // For additional randomization
uniform float intense_mode : hint_range(0.0, 1.0) = 0.0; // For jumpscare moments
uniform float glitch_intensity : hint_range(0.0, 1.0) = 0.0; // For glitch effects

// Noise functions
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

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

// VHS distortion effect
vec2 vhs_distortion(vec2 uv, float time) {
    // Horizontal distortion
    float y_influence = sin(uv.y * 5.0 + time * 0.5) * 0.02;
    
    // Vertical jitter
    float jitter = random(vec2(time, uv.y)) * 0.01;
    
    // Apply distortion
    uv.x += y_influence + jitter;
    
    return uv;
}

// Glitch effect
vec3 glitch_effect(vec2 uv, vec3 color, float time) {
    // Create glitch bands
    float glitch_band = step(0.98, random(vec2(floor(uv.y * 20.0), floor(time * 10.0))));
    
    // Shift color channels
    if (glitch_band > 0.0) {
        float shift_amount = (random(vec2(floor(time * 5.0), floor(uv.y * 10.0))) - 0.5) * 0.1;
        // Don't use TEXTURE as it might not be available in canvas_item shaders without texture
        vec3 shifted_color = color;
        shifted_color.r = shifted_color.r * (1.0 + shift_amount);
        shifted_color.g = shifted_color.g * (1.0 - shift_amount);
        color = shifted_color;
    }
    
    // Add noise
    float noise_intensity = glitch_band * 0.2;
    color = mix(color, vec3(random(uv + time)), noise_intensity);
    
    return color;
}

// Create crawling shadows that move across the screen
float crawling_shadows(vec2 uv, float time) {
    // Create multiple shadow elements that loop
    float shadow = 0.0;
    
    for (float i = 0.0; i < 6.0; i++) {
        // Calculate a looping position for each shadow
        float speed = 0.05 + i * 0.01;
        float x_pos = fract(time * speed + i * 0.17);
        float width = 0.05 + random(vec2(i, 0.0)) * 0.1;
        float y_center = 0.2 + i * 0.15;
        float y_height = 0.1 + random(vec2(i, 1.0)) * 0.2;
        
        // Create a shadow shape that moves from right to left and loops
        float shadow_shape = smoothstep(width, 0.0, abs(uv.x - x_pos)) * 
                           smoothstep(y_height, 0.0, abs(uv.y - y_center));
        
        // Add shadow to the total
        shadow = max(shadow, shadow_shape * 0.5);
    }
    
    return shadow;
}

// Create floating ghostly faces
float ghostly_faces(vec2 uv, float time) {
    float faces = 0.0;
    
    for (float i = 0.0; i < 3.0; i++) {
        // Calculate looping position for each face
        float x_speed = 0.03 + i * 0.01;
        float y_speed = 0.02 + i * 0.005;
        
        // Create looping movement patterns
        float x_pos = fract(time * x_speed + i * 0.33);
        float y_pos = 0.3 + 0.4 * sin(time * y_speed + i * 1.7);
        
        // Face size varies with time
        float face_size = 0.05 + 0.02 * sin(time * 0.3 + i);
        
        // Create a basic face shape
        float face = smoothstep(face_size, 0.0, length(uv - vec2(x_pos, y_pos)));
        
        // Add eyes (darker spots)
        float eye_size = face_size * 0.2;
        float left_eye = smoothstep(eye_size, 0.0, length(uv - vec2(x_pos - face_size * 0.3, y_pos - face_size * 0.1)));
        float right_eye = smoothstep(eye_size, 0.0, length(uv - vec2(x_pos + face_size * 0.3, y_pos - face_size * 0.1)));
        
        // Add mouth (curved line)
        float mouth_y = y_pos + face_size * 0.3 + 0.01 * sin(time * 2.0 + i);
        float mouth_width = face_size * 0.6;
        float mouth_height = 0.005 + 0.003 * sin(time * 3.0 + i);
        float mouth = smoothstep(mouth_height, 0.0, abs(uv.y - mouth_y)) * 
                     smoothstep(mouth_width, 0.0, abs(uv.x - x_pos));
        
        // Combine face features
        float complete_face = face - (left_eye + right_eye) * 0.5 + mouth * 0.3;
        
        // Add to total faces
        faces = max(faces, complete_face * 0.4);
    }
    
    return faces;
}

// Create pulsating organic tendrils
float organic_tendrils(vec2 uv, float time, vec2 mouse_pos) {
    float tendrils = 0.0;
    
    // Create multiple tendrils that reach across the screen
    for (float i = 0.0; i < 8.0; i++) {
        // Base position that loops horizontally
        float base_x = fract(time * 0.03 + i * 0.125);
        float base_y = 0.1 + i * 0.1;
        
        // Create a curved path for each tendril
        float curve_strength = 0.2 + 0.1 * sin(time * 0.2 + i);
        float y_offset = curve_strength * sin((uv.x - base_x) * 10.0 + time + i);
        
        // Calculate distance to the curved path
        float dist = smoothstep(0.01, 0.0, abs(uv.y - (base_y + y_offset)));
        
        // Add pulsating effect
        dist *= 0.5 + 0.5 * sin(time * 0.5 + i + uv.x * 5.0);
        
        // Make tendrils reach toward mouse position (simplified)
        float mouse_dist = length(mouse_pos - vec2(base_x, base_y));
        float mouse_influence = smoothstep(1.0, 0.0, mouse_dist * 3.0);
        
        // Direction to mouse (safe calculation to avoid normalization issues)
        vec2 mouse_dir = vec2(0.0);
        if (mouse_dist > 0.001) {
            mouse_dir = (mouse_pos - vec2(base_x, base_y)) / mouse_dist;
        }
        
        // Create tendril reaching toward mouse
        float reach_dist = 0.2 * mouse_influence;
        float mouse_tendril = smoothstep(0.01, 0.0, 
                                       length(uv - (vec2(base_x, base_y) + mouse_dir * reach_dist)));
        
        // Combine regular tendril with mouse-influenced tendril
        tendrils = max(tendrils, max(dist * 0.3, mouse_tendril * 0.5));
    }
    
    return tendrils;
}

// Create appearing/disappearing symbols
float occult_symbols(vec2 uv, float time) {
    float symbols = 0.0;
    
    // Create multiple symbols that fade in and out
    for (float i = 0.0; i < 5.0; i++) {
        // Position that loops around the screen
        float angle = time * 0.1 + i * 1.256;
        float radius = 0.3 + 0.1 * sin(time * 0.2 + i);
        float x_pos = 0.5 + radius * cos(angle);
        float y_pos = 0.5 + radius * sin(angle);
        
        // Only show symbols periodically
        float visibility = step(0.7, sin(time * 0.2 + i * 0.5) * 0.5 + 0.5);
        
        // Create different symbol shapes
        float symbol_type = mod(i, 3.0);
        float symbol = 0.0;
        
        if (symbol_type < 1.0) {
            // Pentagram-like shape (simplified)
            float size = 0.03 + 0.01 * sin(time + i);
            float star = smoothstep(0.01, 0.0, abs(length(uv - vec2(x_pos, y_pos)) - size));
            symbol = star;
        } else if (symbol_type < 2.0) {
            // Circle with cross
            float size = 0.02 + 0.005 * sin(time + i);
            float circle = smoothstep(0.001, 0.0, abs(length(uv - vec2(x_pos, y_pos)) - size));
            float h_line = smoothstep(0.002, 0.0, abs(uv.y - y_pos)) * 
                          step(x_pos - size * 1.2, uv.x) * step(uv.x, x_pos + size * 1.2);
            float v_line = smoothstep(0.002, 0.0, abs(uv.x - x_pos)) * 
                          step(y_pos - size * 1.2, uv.y) * step(uv.y, y_pos + size * 1.2);
            symbol = max(circle, max(h_line, v_line));
        } else {
            // Triangle (simplified)
            float size = 0.025 + 0.01 * sin(time + i);
            float dist = length(uv - vec2(x_pos, y_pos));
            float triangle = smoothstep(size, size - 0.01, dist) * 
                           (0.5 + 0.5 * sin(atan(uv.y - y_pos, uv.x - x_pos) * 3.0 + time));
            symbol = triangle;
        }
        
        // Add to total symbols with visibility factor
        symbols = max(symbols, symbol * visibility * 0.6);
    }
    
    return symbols;
}

void vertex() {
    // Simplified vertex displacement for a more dynamic feel
    VERTEX.x += sin(TIME * 0.2 + VERTEX.y * 0.01) * 1.0;
    VERTEX.y += cos(TIME * 0.3 + VERTEX.x * 0.01) * 1.0;
    
    // Add more displacement during intense mode (simplified)
    if (intense_mode > 0.0) {
        VERTEX.x += sin(TIME * 2.0) * intense_mode * 2.0;
        VERTEX.y += cos(TIME * 2.0) * intense_mode * 2.0;
    }
}

void fragment() {
    // Get base UV coordinates
    vec2 uv = UV;
    
    // Calculate time with offset for variation
    float time = TIME + time_offset;
    
    // Calculate distance from mouse position
    float mouse_dist = distance(mouse_position, uv);
    float mouse_influence = smoothstep(0.4, 0.0, mouse_dist) * distortion_intensity;
    
    // Apply breathing effect (like a heartbeat)
    float breathing = (sin(time * 0.5) * 0.5 + 0.5) * 0.1;
    float fast_breathing = (sin(time * 2.0) * 0.5 + 0.5) * 0.05;
    
    // Combine breathing effects based on intense mode
    float combined_breathing = mix(breathing, fast_breathing, intense_mode);
    
    // Apply distortion based on breathing and mouse
    float distortion = combined_breathing + mouse_influence;
    
    // Create base distortion
    vec2 distorted_uv = uv;
    
    // Apply noise-based distortion
    float noise_val = fbm(uv * 3.0 + time * 0.1);
    distorted_uv += vec2(noise_val, noise_val) * distortion;
    
    // Apply VHS distortion effect
    distorted_uv = mix(distorted_uv, vhs_distortion(distorted_uv, time), 0.3 + intense_mode * 0.3);
    
    // Create a corrupted flesh-like effect that reacts to mouse
    float flesh_effect = fbm(distorted_uv * 4.0 - mouse_position * distortion + time * 0.05);
    flesh_effect = pow(flesh_effect, 2.0) * (1.0 + mouse_influence * 2.0);
    
    // Create pulsating veins
    float veins = abs(sin(distorted_uv.x * 20.0 + time) * cos(distorted_uv.y * 20.0 - time));
    veins = smoothstep(0.9, 1.0, veins) * (0.5 + combined_breathing * 3.0);
    
    // Create scan lines
    float scan_lines = sin(distorted_uv.y * 100.0) * 0.2 + 0.8;
    
    // Random glitches
    float glitch = 0.0;
    if (random(vec2(floor(time * 2.0))) < 0.05 || glitch_intensity > 0.0) {
        glitch = random(vec2(floor(distorted_uv.y * 50.0), floor(time * 20.0))) * 0.1;
    }
    
    // Add new horror elements that loop continuously
    float shadows = crawling_shadows(uv, time);
    float faces = ghostly_faces(uv, time);
    float tendrils = organic_tendrils(uv, time, mouse_position);
    float symbols = occult_symbols(uv, time);
    
    // Create base color - dark with a hint of red
    vec3 base_color = vec3(0.1, 0.02, 0.02);
    
    // Add flesh effect - sickly red/green
    vec3 flesh_color = mix(
        vec3(0.4, 0.0, 0.0),  // Blood red
        vec3(0.2, 0.3, 0.0),  // Sickly green
        flesh_effect
    );
    
    // Add veins - darker red
    vec3 vein_color = vec3(0.3, 0.0, 0.0);
    
    // Add shadow color - deep black with hint of purple
    vec3 shadow_color = vec3(0.05, 0.0, 0.08);
    
    // Add face color - pale ghostly blue
    vec3 face_color = vec3(0.2, 0.3, 0.4);
    
    // Add tendril color - dark red organic
    vec3 tendril_color = vec3(0.25, 0.02, 0.02);
    
    // Add symbol color - glowing red
    vec3 symbol_color = vec3(0.7, 0.1, 0.1);
    
    // Mix colors based on effects
    vec3 color = base_color;
    color = mix(color, flesh_color, flesh_effect * (0.5 + combined_breathing));
    color = mix(color, vein_color, veins);
    color = mix(color, shadow_color, shadows);
    color = mix(color, face_color, faces);
    color = mix(color, tendril_color, tendrils);
    color = mix(color, symbol_color, symbols);
    
    // Add intense mode effects - more red and contrast
    if (intense_mode > 0.0) {
        color = mix(color, vec3(0.8, 0.0, 0.0), intense_mode * 0.3);
        color = mix(color, vec3(0.0), intense_mode * abs(sin(time * 10.0)) * 0.5); // Flashing effect
    }
    
    // Apply scan lines
    color *= scan_lines;
    
    // Apply glitches
    color += vec3(glitch);
    
    // Apply additional glitch effect when glitch_intensity > 0
    if (glitch_intensity > 0.0) {
        color = glitch_effect(distorted_uv, color, time);
    }
    
    // Add vignette effect (darker edges)
    float vignette = smoothstep(1.0, 0.3, length(uv - 0.5));
    color *= vignette;
    
    // Add random noise
    color += (random(uv + time) - 0.5) * 0.05;
    
    // Output final color
    COLOR = vec4(color, 1.0);
}