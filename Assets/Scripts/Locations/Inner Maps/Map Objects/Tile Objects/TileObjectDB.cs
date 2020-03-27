using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public static class TileObjectDB {

    //tile objects
    public static Dictionary<TILE_OBJECT_TYPE, TileObjectData> tileObjectData = new Dictionary<TILE_OBJECT_TYPE, TileObjectData>() {
        { TILE_OBJECT_TYPE.WOOD_PILE, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 10000,
            neededTraitTypes =  new string[] { "Builder" },
        } },

        { TILE_OBJECT_TYPE.BED, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes =  new string[] { "Builder" },
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.TIREDNESS_RECOVERY, value = 20 }
            }
        } },

        { TILE_OBJECT_TYPE.DESK, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes =  new string[] { "Builder" },
            providedFacilities = new ProvidedFacility[] {
                 new ProvidedFacility() { type = FACILITY_TYPE.SIT_DOWN_SPOT, value = 10 }
            }
        } },

        { TILE_OBJECT_TYPE.GUITAR, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes =  new string[] { "Builder" },
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.HAPPINESS_RECOVERY, value = 10 }
            }
        } },
        { TILE_OBJECT_TYPE.TABLE, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes =  new string[] { "Builder" },
            providedFacilities = new ProvidedFacility[] {
                new ProvidedFacility() { type = FACILITY_TYPE.FULLNESS_RECOVERY, value = 20 },
                new ProvidedFacility() { type = FACILITY_TYPE.SIT_DOWN_SPOT, value = 5 }
            }
        } },
        { TILE_OBJECT_TYPE.TREE_OBJECT, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes =  new string[] { "Builder" },
        } },
        { TILE_OBJECT_TYPE.BIG_TREE_OBJECT, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes =  new string[] { "Builder" },
            occupiedSize =  new Point(2, 2),
        } },
        { TILE_OBJECT_TYPE.HEALING_POTION, new TileObjectData() {
            constructionCost = 25,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes = null,
            itemRequirementsForCreation = new[] { TILE_OBJECT_TYPE.WATER_FLASK, TILE_OBJECT_TYPE.HERB_PLANT },
        } },
        { TILE_OBJECT_TYPE.POISON_FLASK, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes = null,
            itemRequirementsForCreation = new[] { TILE_OBJECT_TYPE.WATER_FLASK, TILE_OBJECT_TYPE.HERB_PLANT },
        } },
         { TILE_OBJECT_TYPE.ANTIDOTE, new TileObjectData() {
            constructionCost = 10,
            constructionTime = 12,
            maxHP = 1000,
            neededTraitTypes = null,
            itemRequirementsForCreation = new[] { TILE_OBJECT_TYPE.POISON_FLASK },
        } },
        { TILE_OBJECT_TYPE.LOCUST_SWARM, new TileObjectData() {
            maxHP = 100,
        } },
        { TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT, new TileObjectData() {
            maxHP = 10000,
        } },
        
        //{ TILE_OBJECT_TYPE.WATER_BUCKET, new TileObjectData() {
        //    constructionCost = 25,
        //    constructionTime = 12,
        //    maxHP = 1000,
        //    neededTraitTypes =  new string[] { "Builder" },
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
        return TileObjectData.Default;
    }
    public static bool TryGetTileObjectData(TILE_OBJECT_TYPE objType, out TileObjectData data) {
        if (tileObjectData.ContainsKey(objType)) {
            data = tileObjectData[objType];
            return true;
        }
        data = new TileObjectData();
        return false;
    }
}

public struct TileObjectData {
    public int constructionCost;
    public int constructionTime; //in ticks
    public int maxHP;
    public string[] neededTraitTypes;
    public TILE_OBJECT_TYPE[] itemRequirementsForCreation;
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

    public static TileObjectData Default {
        get {
            return new TileObjectData() {
                constructionCost = 10,
                constructionTime = 12,
                maxHP = 1000,
                neededTraitTypes = new string[] { "Builder" },
                providedFacilities = null,
                occupiedSize = new Point(1, 1),
                itemRequirementsForCreation = null,
            };
        }
    }
}
public struct ProvidedFacility {
    public FACILITY_TYPE type;
    public int value;
}