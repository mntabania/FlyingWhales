using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class SettlementResources
{
    public enum StructureRequirement { NONE = 0, ROCK, TREE, FISHING_SPOT, FEATURE_GAME, MINE_SHACK_SPOT, CHARACTER, HERB_PLANT, }
    //public List<Rock> rocks = new List<Rock>();
    //public List<LocationGridTile> mineShackSpots = new List<LocationGridTile>();
    public bool IsRequirementAvailable(StructureRequirement p_structureRequirement, BaseSettlement p_settlement) {
        switch (p_structureRequirement) {
            //case StructureRequirement.ROCK: if (rocks.Count > 0) return true; else return false;
            case StructureRequirement.TREE: if (HasTrees(p_settlement)) return true; else return false;
            case StructureRequirement.FISHING_SPOT: if (HasFishingSpot(p_settlement)) return true; else return false;
            case StructureRequirement.MINE_SHACK_SPOT: if (HasMineSpotShack(p_settlement)) return true; else return false;
        }
        return true;
    }
    #region Trees
    public bool HasTrees(BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            if (area.tileObjectComponent.HasTree()) {
                return true;
            }
        }
        return false;
    }
    public int GetTreesCount(BaseSettlement p_settlement) {
        int count = 0;
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            count += area.tileObjectComponent.GetTreeCount();
        }
        return count;
    }
    public void PopulateAllTrees(List<TileObject> p_trees, BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            area.tileObjectComponent.PopulateAllTrees(p_trees);
        }
    }
    #endregion

    #region Fishing Spot
    public int GetFishingSpotCount(BaseSettlement p_settlement) {
        int count = 0;
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            count += area.tileObjectComponent.GetFishingSpotCount();
        }
        return count;
    }
    public bool HasFishingSpot(BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            if (area.tileObjectComponent.HasFishingSpot()) {
                return true;
            }
        }
        return false;
    }
    public void PopulateAllFishingSpots(List<TileObject> p_spots, BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            area.tileObjectComponent.PopulateAllFishingSpots(p_spots);
        }
    }
    #endregion

    #region Mine Shack
    public int GetMineShackSpotCount(BaseSettlement p_settlement) {
        int count = 0;
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            count += area.tileObjectComponent.GetMineShackSpotCount();
        }
        return count;
    }
    public bool HasMineSpotShack(BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            if (area.tileObjectComponent.HasMineShackSpot()) {
                return true;
            }
        }
        return false;
    }
    public void PopulateAllMineShackSpots(List<LocationGridTile> p_spots, BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            area.tileObjectComponent.PopulateAllMineShackSpots(p_spots);
        }
    }
    #endregion

    #region Characters
    public int GetCharacterCount(BaseSettlement p_settlement) {
        int count = 0;
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            count += area.locationCharacterTracker.GetCharacterCount();
        }
        return count;
    }
    public void PopulateAllAnimalsForSkinnersLodgeSkinning(List<Character> allAvailableAnimals, BaseSettlement p_settlement, LocationStructure p_workerStructure) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            area.locationCharacterTracker.PopulateAllAnimalsForSkinnersLodgeSkinning(allAvailableAnimals, p_workerStructure);
        }
    }
    public void PopulateAllAnimalsForSkinnersLodgeShearing(List<Character> ableToShearTodayList, BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            area.locationCharacterTracker.PopulateAllAnimalsForSkinnersLodgeShearing(ableToShearTodayList);
        }
    }
    public Summon GetFirstButcherableAnimal(BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            Summon chosen = area.locationCharacterTracker.GetFirstButcherableAnimal();
            if (chosen != null) {
                return chosen;
            }
        }
        return null;
    }
    #endregion

    #region Resource Piles
    public int GetBuiltResourcePileCount(BaseSettlement p_settlement) {
        int count = 0;
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            count += area.tileObjectComponent.GetBuiltResourcePileCount();
        }
        return count;
    }
    public ResourcePile GetRandomPileOfCropsForFarmHaul(BaseSettlement p_settlement) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            ResourcePile pile = area.tileObjectComponent.GetRandomPileOfCropsForFarmHaul();
            if (pile != null) {
                pilePool.Add(pile);
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public ResourcePile GetRandomPileOfClothOrLeatherForSkinnersLodgeHaul(BaseSettlement p_settlement) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            ResourcePile pile = area.tileObjectComponent.GetRandomPileOfClothOrLeatherForSkinnersLodgeHaul();
            if (pile != null) {
                pilePool.Add(pile);
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public ResourcePile GetRandomPileOfMeatsForButchersShopHaul(BaseSettlement p_settlement, Character p_butcher) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            ResourcePile pile = area.tileObjectComponent.GetRandomPileOfMeatsForButchersShopHaul(p_butcher);
            if (pile != null) {
                pilePool.Add(pile);
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public ResourcePile GetRandomPileOfFishesForFisheryHaul(BaseSettlement p_settlement) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            ResourcePile pile = area.tileObjectComponent.GetRandomPileOfFishesForFisheryHaul();
            if (pile != null) {
                pilePool.Add(pile);
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }

    public ResourcePile GetRandomPileOfWoodsForLumberyardHaul(BaseSettlement p_settlement) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            ResourcePile pile = area.tileObjectComponent.GetRandomPileOfWoodsForLumberyardHaul();
            if (pile != null) {
                pilePool.Add(pile);
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }
    public ResourcePile GetRandomPileOfMetalOrStoneForMineHaul(BaseSettlement p_settlement) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            ResourcePile pile = area.tileObjectComponent.GetRandomPileOfMetalOrStoneForMineHaul();
            if (pile != null) {
                pilePool.Add(pile);
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }
    public ResourcePile GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE p_resourceType, STRUCTURE_TYPE p_structureType, BaseSettlement p_settlement) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            ResourcePile pile = area.tileObjectComponent.GetRandomPileOfResourceTypeForWorkshopHaul(p_resourceType, p_structureType);
            if (pile != null) {
                pilePool.Add(pile);
            }
        }
        ResourcePile chosenPile = null;
        if (pilePool.Count > 0) {
            chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
        }
        RuinarchListPool<TileObject>.Release(pilePool);
        return chosenPile;
    }
    #endregion

    #region Herb Plant
    public HerbPlant GetFirstAvailableHerbPlant(BaseSettlement p_settlement) {
        for (int i = 0; i < p_settlement.areas.Count; i++) {
            Area area = p_settlement.areas[i];
            HerbPlant chosen = area.tileObjectComponent.GetFirstAvailableHerbPlant();
            if (chosen != null) {
                return chosen;
            }
        }
        return null;
    }
    #endregion
    //public void AddToListBasedOnRequirement(StructureRequirement p_structureRequirement, TileObject p_tileObject) {
    //    switch (p_structureRequirement) {
    //        //case StructureRequirement.ROCK:
    //        //if (!rocks.Contains(p_tileObject as Rock)) {
    //        //    rocks.Add(p_tileObject as Rock);
    //        //}
    //        //break;
    //        case StructureRequirement.TREE:
    //        if (!trees.Contains(p_tileObject as TreeObject)) {
    //            trees.Add(p_tileObject as TreeObject);
    //        }
    //        break;
    //        case StructureRequirement.FISHING_SPOT:
    //        if (!fishingSpots.Contains(p_tileObject as FishingSpot)) {
    //            fishingSpots.Add(p_tileObject as FishingSpot);
    //        }
    //        break;
    //        case StructureRequirement.MINE_SHACK_SPOT:
    //        if (!mineShackSpots.Contains(p_tileObject.gridTileLocation)) {
    //            mineShackSpots.Add(p_tileObject.gridTileLocation);
    //        }
    //        break;
    //        case StructureRequirement.HERB_PLANT:
    //        if (!herbPlants.Contains(p_tileObject as HerbPlant)) {
    //            herbPlants.Add(p_tileObject as HerbPlant);
    //        }
    //        break;
    //    }
    //}

    //   public void AddCharacterToSettlement(Character p_character) {
    //       if (!characters.Contains(p_character)) {
    //           characters.Add(p_character);
    //       }
    //}

    //   public void RemoveCharacterFromSettlement(Character p_character) {
    //       characters.Remove(p_character);
    //   }

    //   public void RemoveHerbPlant(HerbPlant p_plant) {
    //       if (herbPlants.Contains(p_plant)) {
    //           herbPlants.Remove(p_plant);
    //       }
    //}

    //public void AddAnimalToSettlement(Summon p_character) {
    //    if (!animalsThatProducesMats.Contains(p_character)) {
    //        animalsThatProducesMats.Add(p_character);
    //        if (p_character.race.IsButcherableWhenDead() || p_character.race.IsButcherableWhenDeadOrAlive()) {
    //            if (!butcherables.Contains(p_character)) {
    //                butcherables.Add(p_character);
    //            }
    //        }
    //        if (p_character.race.IsShearable()) {
    //            shearables.Add(p_character);
    //        } else if (p_character.race.IsSkinnable()) {
    //            skinnables.Add(p_character);
    //        }
    //    }
    //}

    //public void RemoveAnimalFromSettlement(Summon p_character) {
    //    animalsThatProducesMats.Remove(p_character);
    //    shearables.Remove(p_character);
    //    skinnables.Remove(p_character);
    //    butcherables.Remove(p_character);
    //}
    //public bool HasResourceAmount(NPCSettlement p_settlement, RESOURCE p_resource, int p_amount) {
    //    if (p_resource == RESOURCE.NONE) { return true; }
    //    int totalResource = p_settlement.mainStorage.GetTotalResourceInStructure(p_resource);
    //    List<LocationStructure> mines = p_settlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
    //    if (mines != null) {
    //        for (int i = 0; i < mines.Count; i++) {
    //            LocationStructure mine = mines[i];
    //            totalResource += mine.GetTotalResourceInStructure(p_resource);
    //        }
    //    }
    //    List<LocationStructure> lumberyards = p_settlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
    //    if (lumberyards != null) {
    //        for (int i = 0; i < lumberyards.Count; i++) {
    //            LocationStructure lumberyard = lumberyards[i];
    //            totalResource += lumberyard.GetTotalResourceInStructure(p_resource);
    //        }
    //    }
    //    return totalResource >= p_amount;
    //}

    //    public void AddToResourcePiles(ResourcePile p_newPile, BaseSettlement settlement) {
    //        resourcePiles.Add(p_newPile);
    //#if DEBUG_LOG
    //        Debug.Log($"Added resource pile {p_newPile.nameWithID} to {settlement.name}");
    //#endif
    //    }

    //    public void RemoveFromResourcePiles(ResourcePile p_newPile, BaseSettlement settlement) {
    //        resourcePiles.Remove(p_newPile);
    //#if DEBUG_LOG
    //        Debug.Log($"Removed resource pile {p_newPile.nameWithID} from {settlement.name}");
    //#endif
    //    }

    //public ResourcePile GetRandomPileOfWoodForCrafter(Character p_getter) {
    //    //bool found = false;
    //    List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
    //    for (int x = 0; x < resourcePiles.Count; ++x) {
    //        ResourcePile pile = resourcePiles[x];
    //        if (pile.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE && pile.resourceInPile >= 40) {
    //            if (pile.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
    //                pilePool.Add(pile);
    //            } else if (pile.currentStructure is Lumberyard lumberyard) {
    //                pilePool.Add(pile);
    //            }
    //        }
    //    }
    //    ResourcePile chosenPile = null;
    //    if (pilePool.Count > 0) {
    //        chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
    //    }
    //    RuinarchListPool<TileObject>.Release(pilePool);
    //    return chosenPile;
    //}

    //public ResourcePile GetRandomPileOfClothForCrafter(Character p_getter) {
    //    //bool found = false;
    //    List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
    //    for (int x = 0; x < resourcePiles.Count; ++x) {
    //        ResourcePile pile = resourcePiles[x];
    //        if (pile.tileObjectType.IsCloth() && pile.resourceInPile >= 40) {
    //            if (pile.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
    //                pilePool.Add(pile);
    //                //found = true;
    //            } else if (pile.currentStructure is HunterLodge hunterLodge) {
    //                pilePool.Add(pile);
    //            }
    //        }
    //    }
    //    ResourcePile chosenPile = null;
    //    if (pilePool.Count > 0) {
    //        chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
    //    }
    //    RuinarchListPool<TileObject>.Release(pilePool);
    //    return chosenPile;
    //}
    //public ResourcePile GetRandomPileOfLeatherForCrafter(Character p_getter) {
    //    //bool found = false;
    //    List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
    //    for (int x = 0; x < resourcePiles.Count; ++x) {
    //        ResourcePile pile = resourcePiles[x];
    //        if (pile.tileObjectType.IsLeather() && pile.resourceInPile >= 40) {
    //            if (pile.currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
    //                pilePool.Add(pile);
    //                //found = true;
    //            } else if (pile.currentStructure is HunterLodge hunterLodge) {
    //                pilePool.Add(pile);
    //            }
    //        }
    //    }
    //    ResourcePile chosenPile = null;
    //    if (pilePool.Count > 0) {
    //        chosenPile = pilePool[GameUtilities.RandomBetweenTwoNumbers(0, pilePool.Count - 1)] as ResourcePile;
    //    }
    //    RuinarchListPool<TileObject>.Release(pilePool);
    //    return chosenPile;
    //}

    //public TreeObject GetAvailableTree() { 
    //    for(int x = 0; x < trees.Count; ++x) {
    //        return trees[x];
    //    }
    //    return null;
    //}

    //public Summon GetAvailableSherable() {
    //    for (int x = 0; x < animalsThatProducesMats.Count; ++x) {
    //        if (animalsThatProducesMats[x] is Animal && animalsThatProducesMats[x].race.IsShearable()) {
    //            return animalsThatProducesMats[x];
    //        }
    //    }
    //    return null;
    //}

    //public List<Summon> GetAllAnimalsThatAreSkinnable() {
    //    List<Summon> ableToSkinAnimals = new List<Summon>();
    //    for (int x = 0; x < skinnables.Count; ++x) {
    //        if (!skinnables[x].HasJobTargetingThis(JOB_TYPE.MONSTER_BUTCHER, JOB_TYPE.SHEAR_ANIMAL, JOB_TYPE.SKIN_ANIMAL)) {
    //            ableToSkinAnimals.Add(skinnables[x]);
    //        }
    //    }
    //    return ableToSkinAnimals;
    //}
}