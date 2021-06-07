using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
using Inner_Maps.Location_Structures;

public class SettlementResources
{
    public enum StructureRequirement { NONE = 0, ROCK, TREE, FISHING_SPOT, FEATURE_GAME, MINE_SHACK_SPOT, CHARACTER, HERB_PLANT, }
    public List<Rock> rocks = new List<Rock>();
    public List<TreeObject> trees = new List<TreeObject>();
    public List<FishingSpot> fishingSpots = new List<FishingSpot>();
    public List<HerbPlant> herbPlants = new List<HerbPlant>();
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
            case StructureRequirement.HERB_PLANT:
            if (!herbPlants.Contains(p_tileObject as HerbPlant)) {
                herbPlants.Add(p_tileObject as HerbPlant);
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
        characters.Remove(p_character);
    }

    public void RemoveHerbPlant(HerbPlant p_plant) {
        if (herbPlants.Contains(p_plant)) {
            herbPlants.Remove(p_plant);
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
            if (p_character.race.IsShearable()) {
                shearables.Add(p_character);
            } else if (p_character.race.IsSkinnable()) {
                skinnables.Add(p_character);
            }
        }
    }

    public void RemoveAnimalFromSettlement(Summon p_character) {
        animalsThatProducesMats.Remove(p_character);
        shearables.Remove(p_character);
        skinnables.Remove(p_character);
        butcherables.Remove(p_character);
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
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.CORN ||
               pile.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY ||
               pile.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB ||
               pile.tileObjectType == TILE_OBJECT_TYPE.POTATO) {
                if (currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.FARM && 
                    currentStructure.structureType != STRUCTURE_TYPE.DWELLING && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
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
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.MINK_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.RABBIT_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.BEAR_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.BOAR_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.DRAGON_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.SCALE_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.WOLF_HIDE) {
                if (currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.HUNTER_LODGE && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
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
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.ANIMAL_MEAT ||
               pile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT ||
               pile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) {
                if (currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.BUTCHERS_SHOP && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
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
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.FISH_PILE) {
                if (currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.FISHERY && 
                    currentStructure.structureType != STRUCTURE_TYPE.DWELLING && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
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
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
                if (pile.mapObjectState == MAP_OBJECT_STATE.BUILT && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.LUMBERYARD && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
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

    public void PopulateAllAnimalsThatProducesMats(List<Character> allAvailableAnimals) {
        for (int x = 0; x < animalsThatProducesMats.Count; ++x) {
            if (!animalsThatProducesMats[x].HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER, JOB_TYPE.SHEAR_ANIMAL, JOB_TYPE.SKIN_ANIMAL)) {
                allAvailableAnimals.Add(animalsThatProducesMats[x]);
            }
        }
    }

    public void PopulateAllAnimalsThatAreShearable(List<Character> ableToShearTodayList) {
        for(int x = 0; x < shearables.Count; ++x) {
            if (shearables[x] is Animal target && target.isShearable && !target.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER, JOB_TYPE.SHEAR_ANIMAL, JOB_TYPE.SKIN_ANIMAL)) {
                ableToShearTodayList.Add(shearables[x]);
            }
		}
    }

    //public List<Summon> GetAllAnimalsThatAreSkinnable() {
    //    List<Summon> ableToSkinAnimals = new List<Summon>();
    //    for (int x = 0; x < skinnables.Count; ++x) {
    //        if (!skinnables[x].HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER, JOB_TYPE.SHEAR_ANIMAL, JOB_TYPE.SKIN_ANIMAL)) {
    //            ableToSkinAnimals.Add(skinnables[x]);
    //        }
    //    }
    //    return ableToSkinAnimals;
    //}

    public Summon GetFirstButcherableAnimal() {
        for (int x = 0; x < butcherables.Count; ++x) {
            Summon monster = butcherables[x];
            LocationStructure currentStructure = butcherables[x].currentStructure;
            if (currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.FARM
                && !monster.HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER, JOB_TYPE.SHEAR_ANIMAL, JOB_TYPE.SKIN_ANIMAL)) {
                return monster;
            }
        }
        return null;
    }

    public ResourcePile GetRandomPileOfMetalOrStone() {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType.IsMetal() || pile.tileObjectType == TILE_OBJECT_TYPE.STONE_PILE) {
                if (pile.currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && pile.currentStructure.structureType != STRUCTURE_TYPE.MINE && pile.currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
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

    public ResourcePile GetRandomPileOfMetalForCraftsman(Character p_getter) {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType.IsMetal()) {
                if (pile.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
                    //found = true;
                } else if (pile.currentStructure.structureType == STRUCTURE_TYPE.MINE) {
                    if (!p_getter.relationshipContainer.IsEnemiesWith((pile.currentStructure as Inner_Maps.Location_Structures.Mine).assignedWorker)) {
                        pilePool.Add(pile);
                    }
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

    public ResourcePile GetRandomPileOfStoneForCraftsman(Character p_getter) {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.STONE_PILE) {
                if (pile.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
                    //found = true;
                } else if (pile.currentStructure.structureType == STRUCTURE_TYPE.MINE) {
                    if (!p_getter.relationshipContainer.IsEnemiesWith((pile.currentStructure as Inner_Maps.Location_Structures.Mine).assignedWorker)) {
                        pilePool.Add(pile);
                    }
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

    public ResourcePile GetRandomPileOfWoodForCraftsman(Character p_getter) {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
                if (pile.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
                    //found = true;
                } else if (pile.currentStructure.structureType == STRUCTURE_TYPE.LUMBERYARD) {
                    if (!p_getter.relationshipContainer.IsEnemiesWith((pile.currentStructure as Inner_Maps.Location_Structures.Mine).assignedWorker)) {
                        pilePool.Add(pile);
                    }
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

    public ResourcePile GetRandomPileOfClothForCraftsman(Character p_getter) {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType.IsCloth()) {
                if (pile.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
                    //found = true;
                } else if (pile.currentStructure.structureType == STRUCTURE_TYPE.HUNTER_LODGE) {
                    if (!p_getter.relationshipContainer.IsEnemiesWith((pile.currentStructure as Inner_Maps.Location_Structures.Mine).assignedWorker)) {
                        pilePool.Add(pile);
                    }
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

    public ResourcePile GetRandomPileOfLeatherForCraftsman(Character p_getter) {
        //bool found = false;
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int x = 0; x < resourcePiles.Count; ++x) {
            ResourcePile pile = resourcePiles[x];
            if (pile.tileObjectType.IsLeather()) {
                if (pile.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
                    //found = true;
                } else if (pile.currentStructure.structureType == STRUCTURE_TYPE.HUNTER_LODGE) {
                    if (!p_getter.relationshipContainer.IsEnemiesWith((pile.currentStructure as Inner_Maps.Location_Structures.Mine).assignedWorker)) {
                        pilePool.Add(pile);
                    }
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

    public HerbPlant GetAvailableHerbPlant() {
        HerbPlant plant = null;
        for (int x = 0; x < herbPlants.Count; ++x) {
            if ((!herbPlants[x].HasJobTargetingThis(JOB_TYPE.GATHER_HERB) || !herbPlants[x].HasJobTargetingThis(JOB_TYPE.HAUL)) && herbPlants[x].currentStructure?.structureType != STRUCTURE_TYPE.HOSPICE) {
                plant = herbPlants[x];
                break;
            }
        }
        return plant;
    }
}