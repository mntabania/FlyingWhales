﻿using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LandmarkNameplate : PooledObject {

    private BaseLandmark landmark;

    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private FactionEmblem factionEmblem;
    public void SetLandmark(BaseLandmark landmark) {
        this.landmark = landmark;
        name = $"{landmark.tileLocation.region.name} Nameplate";
        UpdateVisuals();
        UpdatePosition();
        UpdateFactionEmblem();
    }

    public void UpdateVisuals() {
        if (landmark.tileLocation.settlementOnTile != null) {
            if (landmark.tileLocation.settlementOnTile.areas[0] == landmark.tileLocation) {
                gameObject.SetActive(true);
            }
            else {
                gameObject.SetActive(false);
            }
        } else {
            gameObject.SetActive(false);
        }
        nameLbl.text = landmark.tileLocation.GetDisplayName();
        UpdateFactionEmblem();
    }

    private void UpdatePosition() {
        //Vector2 originalPos = npcSettlement.coreTile.transform.position;
        //originalPos.y -= 1f;
        //Vector2 ScreenPosition = Camera.main.WorldToScreenPoint(npcSettlement.nameplatePos);
        this.transform.position = landmark.nameplatePos;
    }
    private void UpdateFactionEmblem() {
        if (landmark.tileLocation.settlementOnTile != null) {
            factionEmblem.gameObject.SetActive(landmark.tileLocation.settlementOnTile.owner != null);
            if (factionEmblem.gameObject.activeSelf) {
                factionEmblem.SetFaction(landmark.tileLocation.settlementOnTile.owner);
            }
        } else {
            factionEmblem.gameObject.SetActive(false);
        }
        
    }

    public void LateUpdate() {
        if (landmark == null) {
            return;
        }
        UpdatePosition();
    }
}

