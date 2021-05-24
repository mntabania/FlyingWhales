using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Biomes : MonoBehaviour {
	public static Biomes Instance;

	void Awake(){
		Instance = this;
	}
    // public void GenerateElevation(List<Area> tiles, int mapWidth, int mapHeight) {
    //     float elevationFrequency = WorldConfigManager.Instance.isTutorialWorld ? 15f : 8.93f;
    //     float moistureFrequency = 12.34f;
    //     float tempFrequency = 2.64f;
    //
    //     float elevationRand = Random.Range(500f,2000f);
    //     float moistureRand = Random.Range(500f,2000f);
    //     float temperatureRand = Random.Range(500f,2000f);
    //
    //     int equatorY = mapHeight / 2;
    //     
    //     for(int i = 0; i < tiles.Count; i++){
    //         Area currTile = tiles[i];
    //         int x = currTile.areaData.xCoordinate;
    //         int y = currTile.areaData.yCoordinate;
    //
    //         float nx = ((float)x/mapWidth);
    //         float ny = ((float)y/mapHeight);
    //
    //         float elevationNoise = Mathf.PerlinNoise((nx + elevationRand) * elevationFrequency, (ny + elevationRand) * elevationFrequency);
    //         ELEVATION elevationType = GetElevationType(elevationNoise);
    //
    //         currTile.areaData.elevationNoise = elevationNoise;
    //         currTile.SetElevation (elevationType);
    //         currTile.areaData.moistureNoise = Mathf.PerlinNoise((nx + moistureRand) * moistureFrequency, (ny + moistureRand) * moistureFrequency);
    //
    //         int distanceToEquator = Mathf.Abs (y - equatorY);
    //         float tempGradient = 1.23f / mapHeight;
    //         currTile.areaData.temperature = distanceToEquator * tempGradient;
    //         currTile.areaData.temperature += (Mathf.PerlinNoise((nx + temperatureRand) * tempFrequency, (ny + temperatureRand) * tempFrequency)) * 0.6f;
    //     }
    // }
    private ELEVATION GetElevationType(float elevationNoise){
        if (elevationNoise <= 0.20f) {
			return ELEVATION.WATER;
		} else if (elevationNoise > 0.20f && elevationNoise <= 0.39f) {
			return ELEVATION.TREES;
        } else if (elevationNoise > 0.39f && elevationNoise <= 0.7f) {
            return ELEVATION.PLAIN;
        } else { 
            return ELEVATION.MOUNTAIN;
        }
    }
}