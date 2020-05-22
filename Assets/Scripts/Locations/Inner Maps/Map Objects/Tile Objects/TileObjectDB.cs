using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public static class TileObjectDB {

    public static TileObjectData Default = new TileObjectData() {
        constructionCost = 10,
        constructionTime = 12,
        maxHP = 200,
        neededCharacterClass = new string[] { "Craftsman" },
        providedFacilities = null,
        occupiedSize = new Point(1, 1),
        itemRequirementsForCreation = null,
    };

    //tile objects
    public static Dictionary<TILE_OBJECT_TYPE, TileObjectData> tileObjectData = new Dictionary<TILE_OBJECT_TYPE, TileObjectData>() {
        { TILE_OBJECT_TYPE.WOOD_PILE, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 600,
            neededCharacterClass =  new string[] { "Craftsman" },
        } },
        { TILE_OBJECT_TYPE.ANIMAL_MEAT, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.ELF_MEAT, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.HUMAN_MEAT, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.VEGETABLES, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.FISH_PILE, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.STONE_PILE, new TileObjectData() {
            maxHP = 600,
        } },
        { TILE_OBJECT_TYPE.METAL_PILE, new TileObjectData() {
            maxHP = 600,
        } },
        { TILE_OBJECT_TYPE.BED, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 400,
            neededCharacterClass =  new string[] { "Craftsman" },
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.TIREDNESS_RECOVERY, value = 20 }
            }
        } },

        { TILE_OBJECT_TYPE.DESK, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 250,
            neededCharacterClass =  new string[] { "Craftsman" },
            providedFacilities = new ProvidedFacility[] {
                 new ProvidedFacility() { type = FACILITY_TYPE.SIT_DOWN_SPOT, value = 10 }
            }
        } },

        { TILE_OBJECT_TYPE.GUITAR, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 120,
            neededCharacterClass =  new string[] { "Craftsman" },
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.HAPPINESS_RECOVERY, value = 10 }
            }
        } },
        { TILE_OBJECT_TYPE.TABLE, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 250,
            neededCharacterClass =  new string[] { "Craftsman" },
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.FULLNESS_RECOVERY, value = 20 },
                new ProvidedFacility() { type = FACILITY_TYPE.SIT_DOWN_SPOT, value = 5 }
            }
        } },
        { TILE_OBJECT_TYPE.TREE_OBJECT, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 600,
            neededCharacterClass =  new string[] { "Craftsman" },
        } },
        { TILE_OBJECT_TYPE.BIG_TREE_OBJECT, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1200,
            neededCharacterClass =  new string[] { "Craftsman" },
            occupiedSize =  new Point(2, 2),
        } },
        { TILE_OBJECT_TYPE.HEALING_POTION, new TileObjectData() {
            constructionCost = 25,
            constructionTime = 12,
            maxHP = 150,
            neededCharacterClass = null,
            itemRequirementsForCreation = new[] { "Water Flask", "Herb Plant" },
        } },
        { TILE_OBJECT_TYPE.POISON_FLASK, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 150,
            neededCharacterClass = null,
            itemRequirementsForCreation = new[] { "Water Flask", "Herb Plant" },
        } },
         { TILE_OBJECT_TYPE.ANTIDOTE, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 150,
            neededCharacterClass = null,
            itemRequirementsForCreation = new[] { "Poison Flask" },
        } },
        { TILE_OBJECT_TYPE.LOCUST_SWARM, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.POISON_CLOUD, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.TORNADO, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.BALL_LIGHTNING, new TileObjectData() {
            maxHP = 500,
        } },
        { TILE_OBJECT_TYPE.FIRE_BALL, new TileObjectData() {
            maxHP = 500,
        } },
        { TILE_OBJECT_TYPE.FROSTY_FOG, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.VAPOR, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT, new TileObjectData() {
            maxHP = 12000,
        } },
        { TILE_OBJECT_TYPE.FIRE_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.WATER_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.POISON_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.ICE_CRYSTAL, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.BLOCK_WALL, new TileObjectData() {
            maxHP = 500,
        } },
        { TILE_OBJECT_TYPE.ROCK, new TileObjectData() {
            maxHP = 200,
        } },
        { TILE_OBJECT_TYPE.MAGIC_CIRCLE, new TileObjectData() {
            maxHP = 750,
        } },
        { TILE_OBJECT_TYPE.ORE, new TileObjectData() {
            maxHP = 350,
        } },
        { TILE_OBJECT_TYPE.BERRY_SHRUB, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.WATER_WELL, new TileObjectData() {
            maxHP = 1000,
        } },
        { TILE_OBJECT_TYPE.TOMBSTONE, new TileObjectData() {
            maxHP = 400,
        } },
        { TILE_OBJECT_TYPE.MUSHROOM, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.CORN_CROP, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.SMITHING_FORGE, new TileObjectData() {
            maxHP = 350,
        } },
        { TILE_OBJECT_TYPE.TABLE_HERBALISM, new TileObjectData() {
            maxHP = 250,
        } },
        { TILE_OBJECT_TYPE.TABLE_ALCHEMY, new TileObjectData() {
            maxHP = 250,
        } },
        { TILE_OBJECT_TYPE.CAULDRON, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.HERB_PLANT, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT, new TileObjectData() {
            maxHP = 300,
        } },
        { TILE_OBJECT_TYPE.TOOL, new TileObjectData() {
            maxHP = 150,
        } },
        { TILE_OBJECT_TYPE.EMBER, new TileObjectData() {
            maxHP = 150,
        } },
        { TILE_OBJECT_TYPE.WATER_FLASK, new TileObjectData() {
            maxHP = 150,
        } },
        { TILE_OBJECT_TYPE.WINTER_ROSE, new TileObjectData() {
            maxHP = 700,
        } },
        { TILE_OBJECT_TYPE.DESERT_ROSE, new TileObjectData() {
            maxHP = 700,
        } },
        { TILE_OBJECT_TYPE.RACK_STAVES, new TileObjectData() {
            maxHP = 250,
        } },
        { TILE_OBJECT_TYPE.QUICKSAND, new TileObjectData() {
            maxHP = 3000,
        } },
        { TILE_OBJECT_TYPE.SNOW_MOUND, new TileObjectData() {
            maxHP = 200,
        } },
        { TILE_OBJECT_TYPE.ICE, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.TREASURE_CHEST, new TileObjectData() {
            maxHP = 200,
        } },
        
        

        //{ TILE_OBJECT_TYPE.WATER_BUCKET, new TileObjectData() {
        //    constructionCost = 25,
        //    constructionTime = 12,
        //    maxHP = 1000,
        //    neededTraitTypes =  new string[] { "Craftsman" },
        //} },
    };

    public static bool HasTileObjectData(TILE_OBJECT_TYPE objType) {
        return tileObjectData.ContainsKey(objType);
    }
    public static TileObjectData GetTileObjectData(TILE_OBJECT_TYPE objType) {
        if (tileObjectData.ContainsKey(objType)) {
            return tileObjectData[objType];
        }
        //Debug.LogWarning("No tile data for type " + objType.ToString() + " used default tileobject data");
        return Default;
    }
    public static bool TryGetTileObjectData(TILE_OBJECT_TYPE objType, out TileObjectData data) {
        if (tileObjectData.ContainsKey(objType)) {
            data = tileObjectData[objType];
            return true;
        }
        data = Default;
        return false;
    }
}

public class TileObjectData {
    public int constructionCost;
    public int constructionTime; //in ticks
    public int maxHP;
    public string[] neededCharacterClass;
    public string[] itemRequirementsForCreation;
    public ProvidedFacility[] providedFacilities;
    //when this object is placed, how many tiles does it occupy? (Default is 0,0) meaning this object only occupies 1 tile.
    public Point occupiedSize; 

    public bool CanProvideFacility(FACILITY_TYPE type) {
        if (providedFacilities != null) {
            for (int i = 0; i < providedFacilities.Length; i++) {
                if (providedFacilities[i].type == type) {
                    return true;
                }
            }
        }
        return false;
    }
}
public struct ProvidedFacility {
    public FACILITY_TYPE type;
    public int value;
}