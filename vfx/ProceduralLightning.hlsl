//void ProceduralLightning_float(
//    float2 UV,
//    float Time,
//    out float Result)
//{
//    float t = floor(Time * 50.0);
    
//    // Scale X to determine segment density
//    float xScaled = UV.x * 60.0;
//    float i = floor(xScaled);
//    float f = frac(xScaled);
    
//    // Smooth interpolation curve (smoothstep)
//    f = f * f * (3.0 - 2.0 * f);
    
//    // Sample noise at current and next step
//    float n0 = frac(sin(dot(float2(i, t), float2(12.9898, 78.233))) * 43758.5453);
//    float n1 = frac(sin(dot(float2(i + 1.0, t), float2(12.9898, 78.233))) * 43758.5453);
    
//    // Blend smoothly across X
//    float noise = lerp(n0, n1, f);
    
//    // Distance from center
//    float centered = abs((UV.y - 0.5) + (noise - 0.5) * 0.25);
    
//    Result = pow(saturate(1.0 - centered), 40.0);
//}
void ProceduralLightning_float(
    float2 UV,
    float Time,
    out float Result)
{
    float d = abs(UV.y - 0.5);

    float core = pow(saturate(1.0 - d * 8.0), 12.0);
    float glow = pow(saturate(1.0 - d * 2.0), 3.0);

    Result = core + glow * 0.3;
}