using System;
using UnityEngine;
using Random = UnityEngine.Random;
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

        #region Temperature
        /// <summary>
        /// Generate a float map of a linear gradient
        /// </summary>
        /// <param name="p_width">The width of the array</param>
        /// <param name="p_height">The height of the array</param>
        /// <param name="gradientDirection">The direction of the gradient. From black to white.</param>
        /// <param name="warpNoiseScale">The scale of the warp Perlin Noise.</param>
        /// <param name="warpSeed">The seed of the warp Perlin Noise.</param>
        /// <param name="warpStrength">The strength of the warp.</param>
        /// <param name="warpWeight">The weight of the warp (How much the gradient is affected by the warp)</param>
        /// <returns>A float map that represents a linear gradient.</returns>
        public static float[,] GenerateTemperatureGradient(int p_width, int p_height, Gradient_Direction gradientDirection, float warpNoiseScale, float warpSeed, float warpStrength, float warpWeight, float seed) {
            float xSeed = warpSeed;
            float ySeed = warpSeed;
            float random = seed;
            float[,] noiseMap = new float[p_width, p_height];
            for (int x = 0; x < p_width; x++) {
                for (int y = 0; y < p_height; y++) {
                    float value = GetGraidentValue(new Vector2(x, y), p_width, p_height, gradientDirection, random);
                    value += (DistortedNoise((float)x / p_width * warpNoiseScale + xSeed, (float)y / p_height * warpNoiseScale + ySeed, warpStrength)) * warpWeight;
                    noiseMap[x, y] = value;
                }
            }
            return noiseMap;
        }

        private static float GetGraidentValue(Vector2 p, int p_width, int p_height, Gradient_Direction gradientDirection, float random) {
            float dividend;
            float divisor;
            switch (gradientDirection) {
                case Gradient_Direction.Top:
                    dividend = p_height - p.y;
                    divisor = p_height;
                    break;
                case Gradient_Direction.Bottom:
                    dividend =  p.y;
                    divisor = p_height;
                    break;
                case Gradient_Direction.Left:
                    dividend = p.x;
                    divisor = p_width;
                    break;
                case Gradient_Direction.Right:
                    dividend = p_width - p.x;
                    divisor = p_width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gradientDirection), gradientDirection, null);
            }
            float value = ((dividend / divisor) * 0.75f) + random;
            return value;
        }
        private static float DistortedNoise(float x, float y, float distortionStrength) {
            // Take two samples from our distortion function
            // (Shifted in space by > 1 so they don't correlate with each other)
            float xDistortion = distortionStrength * Distort(x + 2.3f, y + 2.9f);
            float yDistortion = distortionStrength * Distort(x - 2.1f, y - 2.3f);

            return Mathf.PerlinNoise(x + xDistortion, y + yDistortion);
        }

        private static float Distort(float x, float y) {
            // Optionally, you can scale your internal noise frequency
            // or layer several octaves of noise to control the wiggly shapes.
            float wiggleDensity = 3.7f; 
            return Mathf.PerlinNoise(x * wiggleDensity, y * wiggleDensity);
        }
        #endregion
    }

}