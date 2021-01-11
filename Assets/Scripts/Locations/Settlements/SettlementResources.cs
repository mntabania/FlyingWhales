using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementResources
{
    public enum StructureRequirement { NONE = 0, ROCK, TREE, WATER_WELL }
    public List<Rock> rocks = new List<Rock>();
    public List<TreeObject> trees = new List<TreeObject>();
    public List<WaterWell> waterWells = new List<WaterWell>();
    public List<OreVein> oreVeins = new List<OreVein>();

    public TileObject GetAvailableRequiredObject(StructureRequirement p_structureRequirement) {
        switch (p_structureRequirement) {
            case StructureRequirement.ROCK: return rocks[UnityEngine.Random.Range(0, rocks.Count - 1)];
            case StructureRequirement.TREE: return trees[UnityEngine.Random.Range(0, rocks.Count - 1)];
            case StructureRequirement.WATER_WELL: return waterWells[UnityEngine.Random.Range(0, rocks.Count - 1)];
        }
        return null;
    }

    public bool IsRequirementAvailable(StructureRequirement p_structureRequirement) {
        switch (p_structureRequirement) {
            case StructureRequirement.ROCK: if (rocks.Count > 0) return false; else return false;
            case StructureRequirement.TREE: if (trees.Count > 0) return true; else return false;
            case StructureRequirement.WATER_WELL: if (waterWells.Count > 0) return true; else return false;
        }
        return true;
    }
}