using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
using Locations.Settlements;

public class AreaTileObjectComponent : AreaComponent {
    public List<TileObject> itemsInArea { get; private set; }
    public AreaTileObjectComponent() {
        itemsInArea = new List<TileObject>();
    }

    #region Utilities
    public void AddItemInArea(TileObject item) {
        if (!itemsInArea.Contains(item)) {
            itemsInArea.Add(item);
            if (item is ResourcePile resourcePile) {
                if (owner.settlementOnArea != null) {
                    owner.settlementOnArea.SettlementResources?.AddToResourcePiles(resourcePile);
                }
            }
        }
    }
    public bool RemoveItemInArea(TileObject item) {
        if (item is ResourcePile resourcePile) {
            if (owner.settlementOnArea != null) {
                owner.settlementOnArea.SettlementResources?.RemoveFromResourcePiles(resourcePile);
            }
        }
        return itemsInArea.Remove(item);
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
}
