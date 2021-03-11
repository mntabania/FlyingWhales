using System;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Perlin_Noise {
    [System.Serializable]
    public struct PerlinNoiseSettings {
        public float noiseScale;

        public int octaves;
        [Range(0f, 1f)]
        public float persistance;
        public float lacunarity;

        public int seed;
        public Vector2 offset;
        
        public PerlinNoiseRegion[] regions;

        public PerlinNoiseRegion GetPerlinNoiseRegion(float height) {
            for (int i = 0; i < regions.Length; i++) {
                PerlinNoiseRegion noiseRegion = regions[i];
                if (height <= noiseRegion.height) {
                    return noiseRegion;
                }
            }
            throw new Exception($"Could not find region for height {height.ToString()}");
        }
    }
}

[System.Serializable]
public struct PerlinNoiseRegion {
    public string name;
    public float height;
    public Color color;
}