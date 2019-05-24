﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObjectSlotItem : MonoBehaviour {

    private TileObject parentObj;
    private TileObjectSlotSetting settings;

    [SerializeField] private SpriteRenderer slotVisual;

    public Character user { get; private set; }

    public void ApplySettings(TileObject parentObj, TileObjectSlotSetting settings) {
        this.parentObj = parentObj;
        this.settings = settings;
        this.name = parentObj.ToString() + " - " + settings.slotName;
        slotVisual.sprite = settings.slotAsset;
        transform.localRotation = Quaternion.Euler(settings.assetRotation);
        UnusedPosition();
    }

    private void UnusedPosition() {
        transform.localPosition = settings.unusedPosition;
    }
    private void UsedPosition() {
        transform.localPosition = settings.usedPosition;
    }

    #region User
    public void Use(Character character) {
        user = character;
        UsedPosition();
        user.marker.pathfindingAI.Teleport(this.transform.position);
        //user.marker.transform.position = this.transform.TransformPoint(settings.characterPosition);
        //user.marker.transform.localRotation = Quaternion.Euler(settings.assetRotation);
        if (parentObj is Table && (parentObj as Table).usedAsset.name.Contains("Bartop")) {
            character.marker.LookAt(parentObj.gridTileLocation.parentAreaMap.objectsTilemap.GetTransformMatrix(parentObj.gridTileLocation.localPlace).rotation);
        } else {
            user.marker.LookAt(parentObj.gridTileLocation.centeredWorldLocation);
        }
    }
    public void StopUsing() {
        if (user != null) {
            UnusedPosition();
            user = null;
        }
    }
    #endregion
}
