using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
using Locations.Settlements;

public class AreaTileObjectComponent : AreaComponent {
    public List<TileObject> itemsInArea { get; private set; }
    public List<TreeObject> trees { get; private set; }
    public List<FishingSpot> fishingSpots { get; private set; }
    public List<HerbPlant> herbPlants { get; private set; }
    public List<LocationGridTile> mineShackSpots { get; private set; }
    public List<ResourcePile> resourcePiles { get; private set; }

    public AreaTileObjectComponent() {
        itemsInArea = new List<TileObject>();
        trees = new List<TreeObject>();
        fishingSpots = new List<FishingSpot>();
        herbPlants = new List<HerbPlant>();
        mineShackSpots = new List<LocationGridTile>();
        resourcePiles = new List<ResourcePile>();
    }

    #region Utilities
    public void AddItemInArea(TileObject item) {
        if (!itemsInArea.Contains(item)) {
            itemsInArea.Add(item);
            if (item is ResourcePile resourcePile) {
                AddResourcePile(resourcePile);
                //if (owner.settlementOnArea != null) {
                //    owner.settlementOnArea.SettlementResources?.AddToResourcePiles(resourcePile, owner.settlementOnArea);
                //}
            } else if (item is TreeObject tree) {
                AddTree(tree);
            } else if (item is FishingSpot spot) {
                AddFishingSpot(spot);
            } else if (item is HerbPlant plant) {
                AddHerbPlant(plant);
            }
        }
    }
    public bool RemoveItemInArea(TileObject item) {
        if (itemsInArea.Remove(item)) {
            if (item is ResourcePile resourcePile) {
                RemoveResourcePile(resourcePile);
                //if (owner.settlementOnArea != null) {
                //    owner.settlementOnArea.SettlementResources?.RemoveFromResourcePiles(resourcePile, owner.settlementOnArea);
                //}
            } else if (item is TreeObject tree) {
                RemoveTree(tree);
            } else if (item is FishingSpot spot) {
                RemoveFishingSpot(spot);
            } else if (item is HerbPlant plant) {
                RemoveHerbPlant(plant);
            }
            return true;
        }
        return false;
    }
    public bool HasTileObjectOfTypeInHexTile(TILE_OBJECT_TYPE type) {
        for (int i = 0; i < itemsInArea.Count; i++) {
            if (itemsInArea[i].tileObjectType == type) {
                return true;
            }
        }
        return false;
    }
    public int GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE type) {
        int count = 0;
        for (int i = 0; i < itemsInArea.Count; i++) {
            if (itemsInArea[i].tileObjectType == type) {
                count++;
            }
        }
        return count;
    }
    public int GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE type, TILE_OBJECT_TYPE type2) {
        int count = 0;
        for (int i = 0; i < itemsInArea.Count; i++) {
            TileObject tileObject = itemsInArea[i];
            if (tileObject.tileObjectType == type || tileObject.tileObjectType == type2) {
                count++;
            }
        }
        return count;
    }
    public int GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE type, TILE_OBJECT_TYPE type2, MapGenerationData p_data) {
        int count = 0;
        for (int i = 0; i < owner.gridTileComponent.gridTiles.Count; i++) {
            LocationGridTile tile = owner.gridTileComponent.gridTiles[i];
            if (tile.tileObjectComponent.objHere != null) {
                TileObject tileObject = tile.tileObjectComponent.objHere;
                if (tileObject.tileObjectType == type || tileObject.tileObjectType == type2) {
                    count++;
                }    
            } else {
                TILE_OBJECT_TYPE tileObjectType = p_data.GetGeneratedObjectOnTile(tile);
                if (tileObjectType == type || tileObjectType == type2) {
                    count++;
                }
            }
            
        }
        return count;
    }
    public void PopulateTileObjectsInArea<T>(List<TileObject> tileObjects) where T : TileObject {
        for (int i = 0; i < itemsInArea.Count; i++) {
            TileObject tileObject = itemsInArea[i];
            if (tileObject is T obj) {
                tileObjects.Add(obj);
            }
        }
    }
    public bool HasBuiltFoodPileInArea() {
        for (int i = 0; i < itemsInArea.Count; i++) {
            TileObject obj = itemsInArea[i];
            if (obj is FoodPile && obj.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                return true;
            }
        }
        return false;
    }
    public TileObject GetRandomTileObject() {
        if (itemsInArea.Count > 0) {
            return itemsInArea[GameUtilities.RandomBetweenTwoNumbers(0, itemsInArea.Count - 1)];
        }
        return null;
    }
    public TileObject GetRandomTileObjectForRaidAttack() {
        TileObject chosenObject = null;
        if (itemsInArea.Count > 0) {
            List<TileObject> tileObjects = RuinarchListPool<TileObject>.Claim();
            for (int i = 0; i < itemsInArea.Count; i++) {
                TileObject item = itemsInArea[i];
                if (item.traitContainer.HasTrait("Indestructible")) {
                    continue;
                }
                if (item.IsUnpassable()) {
                    continue;
                }
                if (item.tileObjectType.IsTileObjectAnItem() || item.tileObjectType.IsTileObjectImportant() || item.tileObjectType.CanBeRepaired()) {
                    tileObjects.Add(item);
                }
            }
            if (tileObjects.Count > 0) {
                chosenObject = tileObjects[GameUtilities.RandomBetweenTwoNumbers(0, tileObjects.Count - 1)];
            }
            RuinarchListPool<TileObject>.Release(tileObjects);
        }
        return chosenObject;
    }
    #endregion

    #region Trees
    public void AddTree(TreeObject p_tree) {
        trees.Add(p_tree);
#if DEBUG_LOG
        Debug.Log($"Added resource pile {p_tree.nameWithID} to {owner.name}");
#endif
    }
    public void RemoveTree(TreeObject p_tree) {
        trees.Remove(p_tree);
#if DEBUG_LOG
        Debug.Log($"Removed resource pile {p_tree.nameWithID} from {owner.name}");
#endif
    }
    public int GetTreeCount() {
        return trees.Count;
    }
    public bool HasTree() {
        return trees.Count > 0;
    }
    public void PopulateAllTrees(List<TileObject> p_trees) {
        p_trees.AddRange(trees);
    }
    #endregion

    #region Fishing Spot
    public void AddFishingSpot(FishingSpot p_spot) {
        fishingSpots.Add(p_spot);
#if DEBUG_LOG
        Debug.Log($"Added fishing spot {p_spot.nameWithID} to {owner.name}");
#endif
    }
    public void RemoveFishingSpot(FishingSpot p_spot) {
        fishingSpots.Remove(p_spot);
#if DEBUG_LOG
        Debug.Log($"Removed fishing spot {p_spot.nameWithID} from {owner.name}");
#endif
    }
    public int GetFishingSpotCount() {
        return fishingSpots.Count;
    }
    public bool HasFishingSpot() {
        return fishingSpots.Count > 0;
    }
    public void PopulateAllFishingSpots(List<TileObject> p_spots) {
        p_spots.AddRange(fishingSpots);
    }
    #endregion

    #region Mine Shack Spot
    public void AddMineShackSpot(LocationGridTile p_spot) {
        if (!mineShackSpots.Contains(p_spot)) {
            mineShackSpots.Add(p_spot);
#if DEBUG_LOG
            Debug.Log($"Added mine shack spot {p_spot.ToString()} to {owner.name}");
#endif
        }
    }
    public void RemoveMineShackSpot(LocationGridTile p_spot) {
        if (mineShackSpots.Remove(p_spot)) {
#if DEBUG_LOG
            Debug.Log($"Removed mine shack spot {p_spot.ToString()} from {owner.name}");
#endif
        }
    }
    public int GetMineShackSpotCount() {
        return mineShackSpots.Count;
    }
    public bool HasMineShackSpot() {
        return mineShackSpots.Count > 0;
    }
    public void PopulateAllMineShackSpots(List<LocationGridTile> p_spots) {
        p_spots.AddRange(mineShackSpots);
    }
    #endregion

    #region Resource Piles
    public void AddResourcePile(ResourcePile p_newPile) {
        resourcePiles.Add(p_newPile);
#if DEBUG_LOG
        Debug.Log($"Added resource pile {p_newPile.nameWithID} to {owner.name}");
#endif
    }
    public void RemoveResourcePile(ResourcePile p_newPile) {
        resourcePiles.Remove(p_newPile);
#if DEBUG_LOG
        Debug.Log($"Removed resource pile {p_newPile.nameWithID} from {owner.name}");
#endif
    }
    public ResourcePile GetRandomPileOfCropsForFarmHaul() {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < resourcePiles.Count; i++) {
            ResourcePile pile = resourcePiles[i];
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.CORN ||
               pile.tileObjectType == TILE_OBJECT_TYPE.ICEBERRY ||
               pile.tileObjectType == TILE_OBJECT_TYPE.PINEAPPLE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.HYPNO_HERB ||
               pile.tileObjectType == TILE_OBJECT_TYPE.POTATO) {
                if (currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.FARM &&
                    currentStructure.structureType != STRUCTURE_TYPE.DWELLING && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
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
    public ResourcePile GetRandomPileOfClothOrLeatherForSkinnersLodgeHaul() {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < resourcePiles.Count; i++) {
            ResourcePile pile = resourcePiles[i];
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.MINK_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.RABBIT_CLOTH ||
               pile.tileObjectType == TILE_OBJECT_TYPE.BEAR_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.BOAR_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.DRAGON_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.SCALE_HIDE ||
               pile.tileObjectType == TILE_OBJECT_TYPE.WOLF_HIDE) {
                if (currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.HUNTER_LODGE && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
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
    public ResourcePile GetRandomPileOfMeatsForButchersShopHaul(Character p_butcher) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < resourcePiles.Count; i++) {
            ResourcePile pile = resourcePiles[i];
            LocationStructure currentStructure = pile.currentStructure;
            bool isFoodTypeValid;
            if (p_butcher.traitContainer.HasTrait("Cannibal")) {
                isFoodTypeValid = pile.tileObjectType == TILE_OBJECT_TYPE.ANIMAL_MEAT || pile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT || pile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT;
            } else {
                isFoodTypeValid = pile.tileObjectType == TILE_OBJECT_TYPE.ANIMAL_MEAT;
            }
            if (isFoodTypeValid) {
                if (currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.BUTCHERS_SHOP && currentStructure.structureType != STRUCTURE_TYPE.FARM && currentStructure.structureType != STRUCTURE_TYPE.DWELLING && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
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
    public ResourcePile GetRandomPileOfFishesForFisheryHaul() {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < resourcePiles.Count; i++) {
            ResourcePile pile = resourcePiles[i];
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.FISH_PILE) {
                if (currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.FISHERY &&
                    currentStructure.structureType != STRUCTURE_TYPE.DWELLING && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
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
    public ResourcePile GetRandomPileOfWoodsForLumberyardHaul() {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < resourcePiles.Count; i++) {
            ResourcePile pile = resourcePiles[i];
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
                if (pile.mapObjectState == MAP_OBJECT_STATE.BUILT && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER && currentStructure.structureType != STRUCTURE_TYPE.LUMBERYARD && currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
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
    public ResourcePile GetRandomPileOfMetalOrStoneForMineHaul() {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < resourcePiles.Count; i++) {
            ResourcePile pile = resourcePiles[i];
            LocationStructure currentStructure = pile.currentStructure;
            if (pile.tileObjectType.IsMetal() || pile.tileObjectType == TILE_OBJECT_TYPE.STONE_PILE) {
                if (pile.gridTileLocation != null && pile.mapObjectState == MAP_OBJECT_STATE.BUILT && currentStructure != null && currentStructure.structureType != STRUCTURE_TYPE.CITY_CENTER &&
                    currentStructure.structureType != STRUCTURE_TYPE.MINE && currentStructure.structureType != STRUCTURE_TYPE.WORKSHOP &&
                    !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                    pilePool.Add(pile);
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
    public ResourcePile GetRandomPileOfResourceTypeForWorkshopHaul(RESOURCE p_resourceType, STRUCTURE_TYPE p_structureType) {
        List<TileObject> pilePool = RuinarchListPool<TileObject>.Claim();
        for (int i = 0; i < resourcePiles.Count; i++) {
            ResourcePile pile = resourcePiles[i];
            if (pile.providedResource == p_resourceType && pile.resourceInPile >= 40) {
                LocationStructure currentStructure = pile.currentStructure;
                if (currentStructure != null) {
                    if ((currentStructure.structureType == p_structureType || currentStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) && !pile.HasJobTargetingThis(JOB_TYPE.HAUL, JOB_TYPE.COMBINE_STOCKPILE)) {
                        pilePool.Add(pile);
                    } 
                    //else if (pile.currentStructure is Inner_Maps.Location_Structures.Mine) {
                    //    pilePool.Add(pile);
                    //}
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
    public int GetBuiltResourcePileCount() {
        int count = 0;
        for (int i = 0; i < resourcePiles.Count; i++) {
            ResourcePile pile = resourcePiles[i];
            if (pile.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                count++;
            }
        }
        return count;
    }
    #endregion

    #region Herb Plant
    public void AddHerbPlant(HerbPlant p_plant) {
        herbPlants.Add(p_plant);
#if DEBUG_LOG
        Debug.Log($"Added herb plant {p_plant.nameWithID} to {owner.name}");
#endif
    }
    public void RemoveHerbPlant(HerbPlant p_plant) {
        herbPlants.Remove(p_plant);
#if DEBUG_LOG
        Debug.Log($"Removed herb plant {p_plant.nameWithID} from {owner.name}");
#endif
    }
    public HerbPlant GetFirstAvailableHerbPlant() {
        for (int i = 0; i < herbPlants.Count; i++) {
            HerbPlant plant = herbPlants[i];
            if (!plant.HasJobTargetingThis(JOB_TYPE.GATHER_HERB, JOB_TYPE.HAUL) && plant.currentStructure?.structureType != STRUCTURE_TYPE.HOSPICE) {
                return plant;
            }
        }
        return null;
    }
    #endregion
}
