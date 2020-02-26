using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileObjectGameObject : MapObjectVisual<TileObject> {
    
    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;
    
    private System.Func<bool> _isMenuShowing;

    public override void Initialize(TileObject tileObject) {
        base.Initialize(tileObject);
        this.name = tileObject.ToString();
        bool isCorrupted = false;
        if (tileObject.gridTileLocation != null) {
            isCorrupted = tileObject.gridTileLocation.isCorrupted;
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, 
                tileObject.state, 
                tileObject.structureLocation.location.coreTile.biomeType,
                isCorrupted));
        } else {
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject,
                tileObject.state,
                isCorrupted));
        }

        collisionTrigger = this.transform.GetComponentInChildren<TileObjectCollisionTrigger>();
        if (collisionTrigger == null) {
            Debug.LogError("NO COLLISION TRIGGER FOR " + tileObject.nameWithID);
        }
        _isMenuShowing = () => IsMenuShowing(tileObject);
        UpdateSortingOrders(tileObject);
    }

    protected override void UpdateSortingOrders(TileObject obj) {
        if (obj.tileObjectType == TILE_OBJECT_TYPE.TREE_OBJECT) {
            if (objectVisual != null) {
                objectVisual.sortingLayerName = "Area Maps";
                objectVisual.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder + 5;    
            }
            if (hoverObject != null) {
                hoverObject.sortingLayerName = "Area Maps";
                hoverObject.sortingOrder = objectVisual.sortingOrder - 1;    
            }   
        } else if (obj.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
            if (objectVisual != null) {
                objectVisual.sortingLayerName = "Area Maps";
                objectVisual.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder + 10;    
            }
            if (hoverObject != null) {
                hoverObject.sortingLayerName = "Area Maps";
                hoverObject.sortingOrder = objectVisual.sortingOrder - 1;    
            }   
        } else {
            base.UpdateSortingOrders(obj);
        }
    }
    
    
    public override void UpdateTileObjectVisual(TileObject tileObject) {
        if (tileObject is Bed bed) {
            UpdateBedVisual(bed); //TODO: Transfer this to it's own object
        } else {
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, 
                tileObject.state, 
                tileObject.structureLocation.location.coreTile.biomeType,
                tileObject.gridTileLocation?.isCorrupted ?? false));
        }
        
    }

    private void UpdateBedVisual(Bed bed) {
        int userCount = bed.GetActiveUserCount();
        if (userCount == 0) {
            SetVisual(InnerMapManager.Instance.GetTileObjectAsset(bed, 
                bed.state, 
                bed.structureLocation.location.coreTile.biomeType,
                bed.gridTileLocation?.isCorrupted ?? false));
        } else if (userCount == 1) {
            SetVisual(bed1Sleeping);
        } else if (userCount == 2) {
            SetVisual(bed2Sleeping);
        }
    }

    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) {
        this.SetRotation(furnitureSetting.rotation.z);
        //this.OverrideVisual(furnitureSetting.assetToUse);
    }

    #region Inquiry
    private bool IsMenuShowing(TileObject obj) {
        return UIManager.Instance.tileObjectInfoUI.isShowing &&
               UIManager.Instance.tileObjectInfoUI.activeTileObject == obj;
    }
    public override bool IsMapObjectMenuVisible() {
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
        if (activeCharacter != null) {
            if(activeCharacter.minion == null) {
#if UNITY_EDITOR
                UIManager.Instance.poiTestingUI.ShowUI(poi);
#endif
            } else {
                UIManager.Instance.minionCommandsUI.ShowUI(poi);
            }
        }
    }
    protected override void OnPointerEnter(TileObject poi) {
        if (poi.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            return;
        }
        base.OnPointerEnter(poi);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(poi);
        InnerMapManager.Instance.ShowTileData(poi.gridTileLocation);
    }
    protected override void OnPointerExit(TileObject poi) {
        if (poi.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            return;
        }
        base.OnPointerExit(poi);
        if (InnerMapManager.Instance.currentlyHoveredPoi == poi) {
            InnerMapManager.Instance.SetCurrentlyHoveredPOI(null);
        }
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Colliders
    public override void UpdateCollidersState(TileObject obj) {
        if (obj is GenericTileObject) {
            //Generic tile object is always visible
            SetAsVisibleToCharacters();
        } else {
            if (obj.advertisedActions.Count > 0) {
                SetAsVisibleToCharacters();
            } else {
                SetAsInvisibleToCharacters();
            }    
        }
        
    }
    #endregion
}
