using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class AreaTileObjectComponent : AreaComponent {
    public List<TileObject> itemsInArea { get; private set; }

    public AreaTileObjectComponent() {
        itemsInArea = new List<TileObject>();
    }

    #region Utilities
    public void AddItemInArea(TileObject item) {
        if (!itemsInArea.Contains(item)) {
            itemsInArea.Add(item);
        }
    }
    public bool RemoveItemInArea(TileObject item) {
        return itemsInArea.Remove(item);
    }
    public void PopulateTileObjectsInHexTile(List<TileObject> p_tileObjectList, TILE_OBJECT_TYPE type) {
        for (int i = 0; i < itemsInArea.Count; i++) {
            TileObject tileObject = itemsInArea[i];
            if (tileObject.tileObjectType == type) {
                p_tileObjectList.Add(tileObject);
            }
        }
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
    public List<T> GetTileObjectsInHexTile<T>() where T : TileObject {
        List<T> tileObjects = null;
        for (int i = 0; i < itemsInArea.Count; i++) {
            TileObject tileObject = itemsInArea[i];
            if (tileObject is T obj) {
                if (tileObjects == null) {
                    tileObjects = new List<T>();
                }
                tileObjects.Add(obj);
            }
        }
        return tileObjects;
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
