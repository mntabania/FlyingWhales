using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UtilityScripts;
using System;

public class TileObjectGameObject : MapObjectVisual<TileObject> {

    public Action<TileObjectGameObject> onObjectClicked;
    public override void Initialize(TileObject tileObject) {
        base.Initialize(tileObject);
        this.name = tileObject.ToString();
        if (tileObject.gridTileLocation != null) {
           UpdateTileObjectVisual(tileObject);
        } else {
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject,
                tileObject.state,
                false));
        }

        visionTrigger = this.transform.GetComponentInChildren<TileObjectVisionTrigger>();
        Assert.IsNotNull(visionTrigger, $"NO COLLISION TRIGGER FOR {tileObject.nameWithID}");
        UpdateSortingOrders(tileObject);
    }

    private void SetSortingOrder(int sortingOrder, string layerName = "Area Maps") {
        if (objectVisual != null) {
            objectVisual.sortingLayerName = layerName;
            objectVisual.sortingOrder = sortingOrder;    
        }
        if (hoverObject != null) {
            hoverObject.sortingLayerName = layerName;
            hoverObject.sortingOrder = objectVisual.sortingOrder - 1;    
        }   
    }
    public override void UpdateSortingOrders(TileObject obj) {
        if (obj.IsCurrentlySelected()) {
            SetSortingOrder(InnerMapManager.SelectedSortingOrder);
        } else if (obj.isBeingCarriedBy != null) {
            SetSortingOrder(obj.isBeingCarriedBy.marker.sortingOrder);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.TREE_OBJECT) {
            SetSortingOrder(InnerMapManager.DetailsTilemapSortingOrder + 5);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
            SetSortingOrder(InnerMapManager.DetailsTilemapSortingOrder + 10);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.MAGIC_CIRCLE) {
            SetSortingOrder(InnerMapManager.DetailsTilemapSortingOrder - 1);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
            SetSortingOrder(InnerMapManager.GroundTilemapSortingOrder + 2);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT) {
            SetSortingOrder(InnerMapManager.GroundTilemapSortingOrder + 3);
        } else {
            base.UpdateSortingOrders(obj);
        }
    }
    
    
    public override void UpdateTileObjectVisual(TileObject tileObject) {
        HexTile hex = tileObject.gridTileLocation.collectionOwner.GetConnectedHextileOrNearestHextile();
        SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, 
            tileObject.state,
            hex.biomeType,
            tileObject.gridTileLocation?.isCorrupted ?? false));
    }

    #region Pointer Events
    protected override void OnPointerLeftClick(TileObject poi) {
        base.OnPointerLeftClick(poi);
        UIManager.Instance.ShowTileObjectInfo(poi);
    }
    protected override void OnPointerRightClick(TileObject poi) {
        base.OnPointerRightClick(poi);
        UIManager.Instance.ShowPlayerActionContextMenu(poi, poi.worldPosition, false);
    }
    protected override void OnPointerMiddleClick(TileObject poi) {
        base.OnPointerMiddleClick(poi);
        Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter ?? UIManager.Instance.monsterInfoUI.activeMonster;
        if (activeCharacter != null) {
            if(activeCharacter.minion == null) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                UIManager.Instance.poiTestingUI.ShowUI(poi,activeCharacter);
#endif
            } else {
                UIManager.Instance.minionCommandsUI.ShowUI(poi);
            }
        }
    }
    protected override void OnPointerEnter(TileObject character) {
        if (character.mapObjectState == MAP_OBJECT_STATE.UNBUILT) { return; }
        if (character.CanBeSelected() == false) { return; }
        base.OnPointerEnter(character);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(character);
        InnerMapManager.Instance.ShowTileData(character.gridTileLocation);
    }
    protected override void OnPointerExit(TileObject poi) {
        if (poi.mapObjectState == MAP_OBJECT_STATE.UNBUILT) { return; }
        if (poi.CanBeSelected() == false) { return; }
        base.OnPointerExit(poi);
        if (InnerMapManager.Instance.currentlyHoveredPoi == poi) {
            InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
        }
        UIManager.Instance?.HideSmallInfo();
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        PooledObject[] pooledObjects = GameUtilities.GetComponentsInDirectChildren<PooledObject>(gameObject);
        for (int i = 0; i < pooledObjects.Length; i++) {
            PooledObject pooledObject = pooledObjects[i];
            ObjectPoolManager.Instance.DestroyObject(pooledObject);
        }
    }
    #endregion
}