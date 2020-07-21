using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class TileObjectGameObject : MapObjectVisual<TileObject> {
    
    private System.Func<bool> _isMenuShowing;

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
        _isMenuShowing = () => IsMenuShowing(tileObject);
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
        HexTile hex = tileObject.structureLocation.location.coreTile;
        if (tileObject.gridTileLocation.collectionOwner != null && tileObject.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            hex = tileObject.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
        }
        SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, 
            tileObject.state,
            hex.biomeType,
            tileObject.gridTileLocation?.isCorrupted ?? false));
    }

    public virtual void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) {
        this.SetRotation(furnitureSetting.rotation.z);
    }

    #region Inquiry
    private bool IsMenuShowing(TileObject obj) {
        return UIManager.Instance.tileObjectInfoUI.isShowing &&
               UIManager.Instance.tileObjectInfoUI.activeTileObject == obj;
    }
    public virtual bool IsMapObjectMenuVisible() {
        return _isMenuShowing.Invoke();
    }
    #endregion
    
    #region Pointer Events
    protected override void OnPointerLeftClick(TileObject poi) {
        base.OnPointerLeftClick(poi);
        UIManager.Instance.ShowTileObjectInfo(poi);
    }
    protected override void OnPointerRightClick(TileObject poi) {
        base.OnPointerRightClick(poi);
        Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter;
        if (activeCharacter == null) {
            activeCharacter = UIManager.Instance.monsterInfoUI.activeMonster;
        }
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
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    // #region Colliders
    // public override void UpdateCollidersState(TileObject obj) {
    //     if (obj is GenericTileObject) {
    //         //Generic tile object is always visible
    //         SetAsVisibleToCharacters();
    //     } else {
    //         if (obj.advertisedActions != null && obj.advertisedActions.Count > 0) {
    //             SetAsVisibleToCharacters();
    //         } else {
    //             SetAsInvisibleToCharacters();
    //         }    
    //     }
    //     
    // }
    // #endregion
}