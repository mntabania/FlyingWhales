using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class SettlementResources
{
    public enum StructureRequirement { NONE = 0, ROCK, TREE, FISHING_SPOT, FEATURE_GAME, MINE_SHACK_SPOT, CHARACTER, }
    public List<Rock> rocks = new List<Rock>();
    public List<TreeObject> trees = new List<TreeObject>();
    public List<FishingSpot> fishingSpots = new List<FishingSpot>();
    public List<LocationGridTile> mineShackSpots = new List<LocationGridTile>();
    public List<ResourcePile> resourcePiles = new List<ResourcePile>();
    public List<Character> characters = new List<Character>();
    public bool IsRequirementAvailable(StructureRequirement p_structureRequirement) {
        switch (p_structureRequirement) {
            case StructureRequirement.ROCK: if (rocks.Count > 0) return true; else return false;
            case StructureRequirement.TREE: if (trees.Count > 0) return true; else return false;
            case StructureRequirement.FISHING_SPOT: if (fishingSpots.Count > 0) return true; else return false;
            case StructureRequirement.MINE_SHACK_SPOT: if (mineShackSpots.Count > 0) return true; else return false;
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
            case StructureRequirement.MINE_SHACK_SPOT:
            if (!mineShackSpots.Contains(p_tileObject.gridTileLocation)) {
                mineShackSpots.Add(p_tileObject.gridTileLocation);
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

    public void AddToResourcePiles(ResourcePile p_newPile) {
        resourcePiles.Add(p_newPile);
    }

    public void RemoveFromResourcePiles(ResourcePile p_newPile) {
        resourcePiles.Remove(p_newPile);
    }

    public ResourcePile GetRandomPileOfCrops() {
        bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.CORN ||
               pile.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY ||
               pile.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB ||
               pile.tileObjectType == TILE_OBJECT_TYPE.POTATO) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.FARM && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    found = true;
                }    
            }
        }
        ResourcePile chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        resourcePiles.Remove(chosenPile);
        RuinarchListPool<TileObject>.Release(pilePool);
        return found ? chosenPile : null;
    }

    public ResourcePile GetRandomPileOfClothOrLeather() {
        bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.MINK_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.RABBIT_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.BEAR_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.BOAR_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.DRAGON_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.SCALE_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.WOLF_HIDE) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.HUNTER_LODGE && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    found = true;
                }
            }
        }
        ResourcePile chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        resourcePiles.Remove(chosenPile);
        RuinarchListPool<TileObject>.Release(pilePool);
        return found ? chosenPile : null;
    }

    public ResourcePile GetRandomPileOfMeats() {
        bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.ANIMAL_MEAT ||
               pile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT ||
               pile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.BUTCHERS_SHOP && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    found = true;
                }
            }
        }
        ResourcePile chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        resourcePiles.Remove(chosenPile);
        RuinarchListPool<TileObject>.Release(pilePool);
        return found ? chosenPile : null;
    }

    public ResourcePile GetRandomPileOfFishes() {
        bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.FISH_PILE) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.FISHERY && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    found = true;
                }
            }
        }
        ResourcePile chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        resourcePiles.Remove(chosenPile);
        RuinarchListPool<TileObject>.Release(pilePool);
        return found ? chosenPile : null;
    }

    public ResourcePile GetRandomPileOfWoods() {
        bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.LUMBERYARD && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    found = true;
                }
            }
        }
        ResourcePile chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        resourcePiles.Remove(chosenPile);
        RuinarchListPool<TileObject>.Release(pilePool);
        return found ? chosenPile : null;
    }
}