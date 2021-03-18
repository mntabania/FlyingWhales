using UnityEngine;

namespace Perlin_Noise {
    public static class Noise {
    public static float[,] GenerateNoiseMap(PerlinNoiseSettings p_settings, int p_width, int p_height) {
        float[,] noiseMap = new float[p_width, p_height];

        System.Random prng = new System.Random(p_settings.seed);
        Vector2[] octaveOffsets = new Vector2[p_settings.octaves];
        for (int i = 0; i < p_settings.octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + p_settings.offset.x;
            float offsetY = prng.Next(-100000, 100000) + p_settings.offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        
        if (p_settings.noiseScale <= 0) {
            p_settings.noiseScale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = p_width / 2f;
        float halfHeight = p_height / 2f;
        
        for (int y = 0; y < p_height; y++) {
            for (int x = 0; x < p_width; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < p_settings.octaves; i++) {
                    float sampleX = (x - halfWidth) / p_settings.noiseScale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / p_settings.noiseScale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseMap[x, y] = perlinValue;
                    noiseHeight += perlinValue * amplitude;
                    
                    amplitude *= p_settings.persistance;
                    frequency *= p_settings.lacunarity;

                }
                if (noiseHeight > maxNoiseHeight) {
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < p_height; y++) {
            for (int x = 0; x < p_width; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }    
}

}