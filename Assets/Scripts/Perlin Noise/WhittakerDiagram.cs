using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class WhittakerDiagram {
    [Header("Precipitation")]
    [Tooltip("The scale at which to take all precipitation values. Default is 0 - 1")]
    public Range totalPrecipitationRange = new Range(0f, 1f);
    public PrecipitationTypeDictionary precipitationTypeTable;
    [Header("Temperature")]
    [Tooltip("The scale at which to take all temperature values. Default is 0 - 1")]
    public Range totalTemperatureRange = new Range(0f, 1f);
    public TemperatureTypeDictionary temperatureTypeTable;

    Biome_Tile_Type[,] tileTypeTable = new Biome_Tile_Type[6,6] {   
        //COLDEST         //COLDER                    //COLD                  //HOT                       //HOTTER             //HOTTEST
        { Biome_Tile_Type.Taiga, Biome_Tile_Type.Grassland,    Biome_Tile_Type.Grassland,       Biome_Tile_Type.Grassland,  Biome_Tile_Type.Desert,       Biome_Tile_Type.Desert }, //DRYEST
        { Biome_Tile_Type.Snow, Biome_Tile_Type.Tundra,       Biome_Tile_Type.Grassland,     Biome_Tile_Type.Grassland,  Biome_Tile_Type.Oasis,       Biome_Tile_Type.Desert }, //DRYER
        { Biome_Tile_Type.Snow, Biome_Tile_Type.Tundra,       Biome_Tile_Type.Jungle,     Biome_Tile_Type.Grassland,  Biome_Tile_Type.Grassland,       Biome_Tile_Type.Oasis }, //DRY
        { Biome_Tile_Type.Snow, Biome_Tile_Type.Tundra,       Biome_Tile_Type.Jungle,      Biome_Tile_Type.Jungle,     Biome_Tile_Type.Grassland,   Biome_Tile_Type.Oasis },  //WET
        { Biome_Tile_Type.Snow, Biome_Tile_Type.Snow,         Biome_Tile_Type.Tundra,      Biome_Tile_Type.Jungle,     Biome_Tile_Type.Grassland,   Biome_Tile_Type.Grassland },  //WETTER
        { Biome_Tile_Type.Snow, Biome_Tile_Type.Snow,         Biome_Tile_Type.Taiga,     Biome_Tile_Type.Jungle,     Biome_Tile_Type.Jungle,   Biome_Tile_Type.Grassland }   //WETTEST
    };
    
    public Biome_Tile_Type GetTileType(float precipitation, float temperature) {
        Temperature_Type temperatureType = GetTemperatureType(temperature);
        Precipitation_Type precipitationType = GetPrecipitationType(precipitation);
        Biome_Tile_Type tileType = tileTypeTable[(int) precipitationType, (int) temperatureType];
        return tileType;
    }


    public Temperature_Type GetTemperatureType(float temperature) {
        float normalizedTemperature = Mathf.Lerp(totalTemperatureRange.minimum, totalTemperatureRange.maximum, temperature);
        foreach (KeyValuePair<Temperature_Type,Range> kvp in temperatureTypeTable) {
            if (kvp.Value.IsInRange(normalizedTemperature)) {
                return kvp.Key;
            }
        }
        throw new Exception($"Could not find Temperature type for {normalizedTemperature.ToString()}");
    }
    public Precipitation_Type GetPrecipitationType(float precipitation) {
        float normalizedPrecipitation = Mathf.Lerp(totalPrecipitationRange.minimum, totalPrecipitationRange.maximum, precipitation);
        foreach (KeyValuePair<Precipitation_Type,Range> kvp in precipitationTypeTable) {
            if (kvp.Value.IsInRange(normalizedPrecipitation)) {
                return kvp.Key;
            }
        }
        throw new Exception($"Could not find Precipitation type for {normalizedPrecipitation.ToString()}");
    }
}

[System.Serializable]
public struct Range {
    [Tooltip("Minimum value in range - Inclusive")]
    public float minimum;
    [Tooltip("Maximum value in range - Inclusive")]
    public float maximum;

    public Range(float p_minimum, float p_maximum) {
        minimum = p_minimum;
        maximum = p_maximum;
    }

    public bool IsInRange(float value) {
        return value >= minimum && value <= maximum;
    }
}
