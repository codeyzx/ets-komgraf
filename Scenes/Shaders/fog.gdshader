shader_type canvas_item;

uniform float density = 0.5;
uniform float speed = 0.1;
uniform vec3 fog_color = vec3(0.05, 0.05, 0.05);
uniform float time_factor = 1.0;

// Simplex noise functions
vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec2 mod289(vec2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec3 permute(vec3 x) { return mod289(((x*34.0)+1.0)*x); }

float snoise(vec2 v) {
    const vec4 C = vec4(0.211324865405187, 0.366025403784439,
             -0.577350269189626, 0.024390243902439);
    vec2 i  = floor(v + dot(v, C.yy));
    vec2 x0 = v -   i + dot(i, C.xx);
    vec2 i1;
    i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    vec4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    i = mod289(i);
    vec3 p = permute(permute(i.y + vec3(0.0, i1.y, 1.0))
    + i.x + vec3(0.0, i1.x, 1.0));
    vec3 m = max(0.5 - vec3(dot(x0, x0), dot(x12.xy, x12.xy),
    dot(x12.zw, x12.zw)), 0.0);
    m = m*m;
    m = m*m;
    vec3 x = 2.0 * fract(p * C.www) - 1.0;
    vec3 h = abs(x) - 0.5;
    vec3 ox = floor(x + 0.5);
    vec3 a0 = x - ox;
    m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);
    vec3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

void fragment() {
    // Animated fog using simplex noise
    vec2 st = UV;
    float time = TIME * speed * time_factor;
    
    // Multiple layers of noise for more organic fog
    float n1 = snoise(st * 3.0 + vec2(time * 0.5, time * 0.2)) * 0.5 + 0.5;
    float n2 = snoise(st * 1.5 + vec2(time * -0.2, time * 0.3)) * 0.5 + 0.5;
    float n3 = snoise(st * 5.0 + vec2(time * 0.1, time * -0.1)) * 0.5 + 0.5;
    
    // Combine noise layers
    float fog = (n1 * 0.5 + n2 * 0.3 + n3 * 0.2) * density;
    
    // Output
    COLOR = vec4(fog_color, fog);
}
