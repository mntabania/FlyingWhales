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

    private void Awake() {
        if (clickCollider == null) {
            clickCollider = gameObject.GetComponent<Collider2D>();
        }
    }

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
#if DEBUG_LOG
        Debug.Log($"Updated sorting orders of {obj.nameWithID}");
#endif
        if (obj.IsCurrentlySelected()) {
            SetSortingOrder(InnerMapManager.SelectedSortingOrder);
        } else if (obj.isBeingCarriedBy != null) {
            SetSortingOrder(obj.isBeingCarriedBy.marker.sortingOrder);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.SMALL_TREE_OBJECT) {
            SetSortingOrder(InnerMapManager.DetailsTilemapSortingOrder + 5);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
            SetSortingOrder(InnerMapManager.DetailsTilemapSortingOrder + 10);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.MAGIC_CIRCLE || obj.tileObjectType == TILE_OBJECT_TYPE.RUG) {
            SetSortingOrder(InnerMapManager.DetailsTilemapSortingOrder - 1);
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
            SetSortingOrder(InnerMapManager.GroundTilemapSortingOrder + 2);
        }
        //Removed because this is not needed anymore. The reason we put this is because there are tree objects in the portal template before
        //else if (obj.tileObjectType == TILE_OBJECT_TYPE.PORTAL_TILE_OBJECT) {
        //    SetSortingOrder(InnerMapManager.GroundTilemapSortingOrder + 3);
        //} 
        else {
            base.UpdateSortingOrders(obj);
        }
    }
    
    
    public override void UpdateTileObjectVisual(TileObject tileObject) {
        SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, 
            tileObject.state,
            tileObject.gridTileLocation.mainBiomeType,
            tileObject.gridTileLocation?.corruptionComponent.isCorrupted ?? false));
        tileObject.hiddenComponent.OnSetHiddenState(tileObject);
    }

    #region Pointer Events
    protected override void OnPointerLeftClick(TileObject poi) {
        base.OnPointerLeftClick(poi);
        UIManager.Instance.ShowTileObjectInfo(poi);
    }
    protected override void OnPointerRightClick(TileObject poi) {
        base.OnPointerRightClick(poi);
        LocationGridTile gridTile = poi.gridTileLocation;
        UIManager.Instance.ShowPlayerActionContextMenu(poi, poi.worldPosition, false);
    }
    protected override void OnPointerMiddleClick(TileObject poi) {
        base.OnPointerMiddleClick(poi);
        Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter ?? UIManager.Instance.monsterInfoUI.activeMonster;
        if (activeCharacter != null) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UIManager.Instance.poiTestingUI.ShowUI(poi, activeCharacter);
#endif
            //            if(activeCharacter.minion == null) {
            //#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //                UIManager.Instance.poiTestingUI.ShowUI(poi,activeCharacter);
            //#endif
            //            } else {
            //                UIManager.Instance.minionCommandsUI.ShowUI(poi);
            //            }
        }
    }
    protected override void OnPointerEnter(TileObject to) {
        if (to.mapObjectState == MAP_OBJECT_STATE.UNBUILT) { return; }
        if (to.CanBeSelected() == false) { return; }
        base.OnPointerEnter(to);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(to);
        InnerMapManager.Instance.ShowTileData(to.gridTileLocation);

        if (to is Tombstone tombstone && tombstone.character != null) {
            if (tombstone.character.hasMarker && tombstone.character.marker.nameplate) {
                tombstone.character.marker.nameplate.UpdateNameActiveState();
            }
        }
    }
    protected override void OnPointerExit(TileObject to) {
        if (to.mapObjectState == MAP_OBJECT_STATE.UNBUILT) { return; }
        if (to.CanBeSelected() == false) { return; }
        base.OnPointerExit(to);
        if (InnerMapManager.Instance.currentlyHoveredPoi == to) {
            InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
        }
        UIManager.Instance?.HideSmallInfo();

        if (to is Tombstone tombstone && tombstone.character != null) {
            if (tombstone.character.hasMarker && tombstone.character.marker.nameplate) {
                tombstone.character.marker.nameplate.UpdateNameActiveState();
            }
        }
    }
    public void MakeObjectUnClickable() {
        if (clickCollider != null) {
            clickCollider.enabled = false;    
        }
    }
    public void MakeObjectClickable() {
        if (clickCollider != null) {
            clickCollider.enabled = true;    
        }
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
        obj = null;
    }
    #endregion
}