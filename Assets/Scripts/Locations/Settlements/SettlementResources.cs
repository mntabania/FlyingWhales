using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementResources
{
    public enum StructureRequirement { NONE = 0, ROCK, TREE, FISHING_SPOT, FEATURE_GAME, ORE_VEIN }
    public List<Rock> rocks = new List<Rock>();
    public List<TreeObject> trees = new List<TreeObject>();
    public List<FishingSpot> fishingSpots = new List<FishingSpot>();
    public List<OreVein> oreVeins = new List<OreVein>();

    public TileObject GetAvailableRequiredObject(StructureRequirement p_structureRequirement) {
        switch (p_structureRequirement) {
            case StructureRequirement.ROCK: return rocks[UnityEngine.Random.Range(0, rocks.Count - 1)];
            case StructureRequirement.TREE: return trees[UnityEngine.Random.Range(0, rocks.Count - 1)];
            case StructureRequirement.FISHING_SPOT: return fishingSpots[UnityEngine.Random.Range(0, rocks.Count - 1)];
            case StructureRequirement.ORE_VEIN: return oreVeins[UnityEngine.Random.Range(0, rocks.Count - 1)];
        }
        return null;
    }

    public bool IsRequirementAvailable(StructureRequirement p_structureRequirement) {
        switch (p_structureRequirement) {
            case StructureRequirement.ROCK: if (rocks.Count > 0) return true; else return false;
            case StructureRequirement.TREE: if (trees.Count > 0) return true; else return false;
            case StructureRequirement.FISHING_SPOT: if (fishingSpots.Count > 0) return true; else return false;
            case StructureRequirement.ORE_VEIN: if (oreVeins.Count > 0) return true; else return false;
        }
        return true;
    }
}