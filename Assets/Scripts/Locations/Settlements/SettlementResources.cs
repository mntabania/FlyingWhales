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
    public List<Summon> animalsThatProducesMats = new List<Summon>();
    public List<Summon> shearables = new List<Summon>();
    public List<Summon> skinnables = new List<Summon>();
    public List<Summon> butcherables = new List<Summon>();

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

    public void AddAnimalToSettlement(Summon p_character) {
        if (!animalsThatProducesMats.Contains(p_character)) {
            animalsThatProducesMats.Add(p_character);
            if (p_character.race.IsButcherableWhenDead() || p_character.race.IsButcherableWhenDeadOrAlive()) {
                if (!butcherables.Contains(p_character)) {
                    if (p_character.race.IsButcherableWhenDead()) {
                        if (p_character.isDead) {
                            butcherables.Add(p_character);
                        }
                    } else {
                        butcherables.Add(p_character);
                    }
                }
            }
            if (p_character is Animal && p_character.race.IsShearable()) {
                shearables.Add(p_character);
            } else if (p_character.race.IsSkinnable()) {
                skinnables.Add(p_character);
            }
        }
    }

    public void RemoveAnimalFromSettlement(Summon p_character) {
        if (animalsThatProducesMats.Contains(p_character)) {
            animalsThatProducesMats.Remove(p_character);
        }
		if (shearables.Contains(p_character)) {
            shearables.Remove(p_character);
		}
        if (skinnables.Contains(p_character)) {
            skinnables.Remove(p_character);
        }
        if (butcherables.Contains(p_character)) {
            butcherables.Remove(p_character);
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
        //bool found = false;
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
                    //found = true;
                }    
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public ResourcePile GetRandomPileOfClothOrLeather() {
        //bool found = false;
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
                    //found = true;
                }
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public ResourcePile GetRandomPileOfMeats() {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.ANIMAL_MEAT ||
               pile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT ||
               pile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.BUTCHERS_SHOP && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    //found = true;
                }
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public ResourcePile GetRandomPileOfFishes() {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.FISH_PILE) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.FISHERY && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    //found = true;
                }
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public ResourcePile GetRandomPileOfWoodsForHaul() {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
                if (pile.mapObjectState == MAP_OBJECT_STATE.BUILT && pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.LUMBERYARD && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    //found = true;
                }
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public TreeObject GetAvailableTree() { 
        for(int x = 0; x < trees.Count; ++x) {
            if (!trees[x].HasJobTargetingThis(JOB_TYPE.PRODUCE_WOOD)) {
                return trees[x];
            }
		}
        return null;
    }

    public Summon GetAvailableSherable() {
        for (int x = 0; x < animalsThatProducesMats.Count; ++x) {
            if (animalsThatProducesMats[x] is Animal && animalsThatProducesMats[x].race.IsShearable() && !animalsThatProducesMats[x].HasJobTargetingThis(JOB_TYPE.SHEAR_ANIMAL)) {
                return animalsThatProducesMats[x];
            }
        }
        return null;
    }

    public List<Summon> GetAllAnimalsThatProducesMats() {
        List<Summon> allAvailableAnimals = new List<Summon>();
        for (int x = 0; x < animalsThatProducesMats.Count; ++x) {
            if ((animalsThatProducesMats[x].HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER) || animalsThatProducesMats[x].HasJobTargetingThis(JOB_TYPE.SHEAR_ANIMAL) || animalsThatProducesMats[x].HasJobTargetingThis(JOB_TYPE.SKIN_ANIMAL))) {
                allAvailableAnimals.Add(animalsThatProducesMats[x]);
            }
        }
        return allAvailableAnimals;
    }

    public List<Summon> GetAllAnimalsThatAreShearable() {
        List<Summon> ableToShearTodayList = new List<Summon>();
        for(int x = 0; x < shearables.Count; ++x) {
            if (shearables[x] is Animal target && target.isShearable && (target.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER) || target.HasJobTargetingThis(JOB_TYPE.SHEAR_ANIMAL) || target.HasJobTargetingThis(JOB_TYPE.SKIN_ANIMAL))) {
                ableToShearTodayList.Add(shearables[x]);
            }
		}
        return ableToShearTodayList;
    }

    public List<Summon> GetAllAnimalsThatAreSkinnable() {
        List<Summon> ableToSkinAnimals = new List<Summon>();
        for (int x = 0; x < skinnables.Count; ++x) {
            if ((skinnables[x].HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER) || skinnables[x].HasJobTargetingThis(JOB_TYPE.SHEAR_ANIMAL) || skinnables[x].HasJobTargetingThis(JOB_TYPE.SKIN_ANIMAL))) {
                ableToSkinAnimals.Add(skinnables[x]);
            }
        }
        return ableToSkinAnimals;
    }

    public Summon GetRandomButcherableAnimal() {
        for (int x = 0; x < butcherables.Count; ++x) {
            if (butcherables[x].currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && butcherables[x].currentStructure.structureType != STRUCTURE_TYPE.FARM && (butcherables[x].HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER) || skinnables[x].HasJobTargetingThis(JOB_TYPE.SHEAR_ANIMAL) || skinnables[x].HasJobTargetingThis(JOB_TYPE.SKIN_ANIMAL))) {
                return butcherables[x];
            }
        }
        return null;
    }

    public ResourcePile GetRandomPileOfMetalOrStone() {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.COPPER ||
               pile.tileObjectType == TILE_OBJECT_TYPE.IRON ||
               pile.tileObjectType == TILE_OBJECT_TYPE.MITHRIL ||
               pile.tileObjectType == TILE_OBJECT_TYPE.ORICHALCUM ||
               pile.tileObjectType == TILE_OBJECT_TYPE.DIAMOND ||
               pile.tileObjectType == TILE_OBJECT_TYPE.GOLD ||
               pile.tileObjectType == TILE_OBJECT_TYPE.STONE_PILE) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.HUNTER_LODGE && !pile.HasJobTargetingThis(JOB_TYPE.HAUL)) {
                    pilePool.Add(pile);
                    //found = true;
                }
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }
}