﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpiritGameObject : MapObjectVisual<TileObject> {
    private System.Func<bool> _isMenuShowing;

    public bool isRoaming { get; private set; }
    public LocationGridTile destinationTile { get; private set; }
    
    public float speed { get; private set; }
    public Region region { get; private set; }
    
    private float _startTime;  // Time when the movement started.
    private float _journeyLength; // Total distance between the markers.
    private Vector3 _startPosition;

    public override void Initialize(TileObject tileObject) {
        base.Initialize(tileObject);
        this.name = tileObject.ToString();
        bool isCorrupted = tileObject.gridTileLocation.corruptionComponent.isCorrupted;
        SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject, 
            tileObject.state,
            tileObject.gridTileLocation.mainBiomeType,
            isCorrupted));  
        visionTrigger = this.transform.GetComponentInChildren<TileObjectVisionTrigger>();
        _isMenuShowing = () => IsMenuShowing(tileObject);
        UpdateSortingOrders(tileObject);
    }
    public override void UpdateSortingOrders(TileObject obj) {
        if (objectVisual != null) {
            objectVisual.sortingLayerName = "Area Maps";
            objectVisual.sortingOrder = InnerMapManager.DefaultCharacterSortingOrder;
        }
        if (hoverObject != null) {
            hoverObject.sortingLayerName = "Area Maps";
            hoverObject.sortingOrder = objectVisual.sortingOrder - 1;
        }
        //if (obj.tileObjectType == TILE_OBJECT_TYPE.TREE_OBJECT) {
        //    if (objectVisual != null) {
        //        objectVisual.sortingLayerName = "Area Maps";
        //        objectVisual.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder + 5;    
        //    }
        //    if (hoverObject != null) {
        //        hoverObject.sortingLayerName = "Area Maps";
        //        hoverObject.sortingOrder = objectVisual.sortingOrder - 1;    
        //    }   
        //} else if (obj.tileObjectType == TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
        //    if (objectVisual != null) {
        //        objectVisual.sortingLayerName = "Area Maps";
        //        objectVisual.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder + 10;    
        //    }
        //    if (hoverObject != null) {
        //        hoverObject.sortingLayerName = "Area Maps";
        //        hoverObject.sortingOrder = objectVisual.sortingOrder - 1;    
        //    }   
        //} else {
        //    base.UpdateSortingOrders(obj);
        //}
    }
    
    
    public override void UpdateTileObjectVisual(TileObject tileObject) {
        SetVisual(InnerMapManager.Instance.GetTileObjectAsset(tileObject,
            tileObject.state,
            tileObject.gridTileLocation.mainBiomeType,
            tileObject.gridTileLocation?.corruptionComponent.isCorrupted ?? false));
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
        UIManager.Instance.ShowPlayerActionContextMenu(poi, poi.worldPosition, false);
    }
    protected override void OnPointerMiddleClick(TileObject poi) {
        base.OnPointerMiddleClick(poi);
        Character activeCharacter = UIManager.Instance.characterInfoUI.activeCharacter ?? UIManager.Instance.monsterInfoUI.activeMonster;
        if (activeCharacter != null) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UIManager.Instance.poiTestingUI.ShowUI(poi, activeCharacter);
#endif
//            if (activeCharacter.minion == null) {
//#if UNITY_EDITOR || DEVELOPMENT_BUILD
//                UIManager.Instance.poiTestingUI.ShowUI(poi, activeCharacter);
//#endif
//            } else {
//                UIManager.Instance.minionCommandsUI.ShowUI(poi);
//            }
        }
    }
    protected override void OnPointerEnter(TileObject character) {
        if (character.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
            return;
        }
        base.OnPointerEnter(character);
        InnerMapManager.Instance.SetCurrentlyHoveredPOI(character);
        InnerMapManager.Instance.ShowTileData(character.gridTileLocation);
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
    #endregion

    #region Spirit
    public void SetIsRoaming(bool state) {
        isRoaming = state;
    }
    public void SetRegion(Region region) {
        this.region = region;
    }
    public void SetDestinationTile(LocationGridTile tile) {
        destinationTile = tile;
        if (destinationTile != null) {
            RecalculatePathingValues();
            // Messenger.Broadcast(Signals.SPIRIT_OBJECT_NO_DESTINATION, this);
        }
    }
    public void SetSpeed(float amount) {
        speed = amount;
    }
    public LocationGridTile GetLocationGridTileByXy(int x, int y) {
        return region.innerMap.map[x, y];
    }
    #endregion
    
    #region Monobehaviour
    private void Update() {
        if (isRoaming) {
            if (destinationTile == null) {
                return;
            }
            if (gameObject.activeSelf == false) {
                return;
            }
            if (GameManager.Instance.isPaused) {
                return;
            }
            if (region == null) {
                return;
            }
            if (GameManager.Instance.gameHasStarted == false) {
                return;
            }
            obj.SetGridTileLocation(GetLocationGridTileByXy(Mathf.FloorToInt(transform.localPosition.x), Mathf.FloorToInt(transform.localPosition.y)));
            
            float distCovered = (Time.time - _startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / _journeyLength;

            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(_startPosition, destinationTile.centeredWorldLocation, fractionOfJourney);
            
            if (Mathf.Approximately(transform.position.x, destinationTile.centeredWorldLocation.x) 
                && Mathf.Approximately(transform.position.y, destinationTile.centeredWorldLocation.y)) {
                SetDestinationTile(null);
                if (obj is RavenousSpirit) {
                    (obj as RavenousSpirit).GoToRandomTileInRadius();
                } else if (obj is FeebleSpirit) {
                    (obj as FeebleSpirit).GoToRandomTileInRadius();
                } else if (obj is ForlornSpirit) {
                    (obj as ForlornSpirit).GoToRandomTileInRadius();
                }
            }
        }
    }
    public void RecalculatePathingValues() {
        // Keep a note of the time the movement started.
        _startTime = Time.time;
        
        var position = transform.position;
        _startPosition = position;
        // Calculate the journey length.
        _journeyLength = Vector3.Distance(position, destinationTile.centeredWorldLocation);
    }
    public override void Reset() {
        base.Reset();
        isRoaming = false;
        destinationTile = null;
        region = null;
        _journeyLength = 0f;
        _startPosition = Vector3.zero;
        _startTime = 0f;
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
    }
    public override void LookAt(Vector3 target, bool force = false) {
        Vector3 diff = target - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        Rotate(Quaternion.Euler(0f, 0f, rot_z - 90), force);
    }
    /// <summary>
    /// Rotate this marker to a specific angle.
    /// </summary>
    /// <param name="target">The angle this character must rotate to.</param>
    /// <param name="force">Should this object be forced to rotate?</param>
    public override void Rotate(Quaternion target, bool force = false) {
        transform.rotation = target;
    }
    
    #endregion
}
