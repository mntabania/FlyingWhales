﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class SettlementResources
{
    public enum StructureRequirement { NONE = 0, ROCK, TREE, FISHING_SPOT, FEATURE_GAME, ORE_VEIN, CHARACTER }
    public List<Rock> rocks = new List<Rock>();
    public List<TreeObject> trees = new List<TreeObject>();
    public List<FishingSpot> fishingSpots = new List<FishingSpot>();
    public List<OreVein> oreVeins = new List<OreVein>();
    public List<Character> characters = new List<Character>();
    public bool IsRequirementAvailable(StructureRequirement p_structureRequirement) {
        switch (p_structureRequirement) {
            case StructureRequirement.ROCK: if (rocks.Count > 0) return true; else return false;
            case StructureRequirement.TREE: if (trees.Count > 0) return true; else return false;
            case StructureRequirement.FISHING_SPOT: if (fishingSpots.Count > 0) return true; else return false;
            case StructureRequirement.ORE_VEIN: if (oreVeins.Count > 0) return true; else return false;
        }
        return true;
    }

    public void AddToListBasedOnRequirement(StructureRequirement p_structureRequirement, TileObject p_tileObject) {
        switch (p_structureRequirement) {
            case StructureRequirement.ROCK:
            if (!rocks.Contains(p_tileObject as Rock)) {
                rocks.Add(p_tileObject as Rock);
            }
            break;
            case StructureRequirement.TREE:
            if (!trees.Contains(p_tileObject as TreeObject)) {
                trees.Add(p_tileObject as TreeObject);
            }
            break;
            case StructureRequirement.FISHING_SPOT:
            if (!fishingSpots.Contains(p_tileObject as FishingSpot)) {
                fishingSpots.Add(p_tileObject as FishingSpot);
            }
            break;
            case StructureRequirement.ORE_VEIN:
            if (!oreVeins.Contains(p_tileObject as OreVein)) {
                oreVeins.Add(p_tileObject as OreVein);
            }
            break;
        }
    }

    public void AddCharacterToSettlement(Character p_character) {
        if (!characters.Contains(p_character)) {
            characters.Add(p_character);
        }
	}

    public void RemoveCharacterFromSettlement(Character p_character) {
        if (characters.Contains(p_character)) {
            characters.Remove(p_character);
        }
    }
    public bool HasResourceAmount(NPCSettlement p_settlement, RESOURCE p_resource, int p_amount) {
        if (p_resource == RESOURCE.NONE) { return true; }
        int totalResource = p_settlement.mainStorage.GetTotalResourceInStructure(p_resource);
        return totalResource >= p_amount;
    }
}