﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class TileObjectHiddenComponent : TileObjectComponent {
    public bool isHidden { get; private set; }
    public TileObjectHiddenComponent() {
    }
    public TileObjectHiddenComponent(SaveDataTileObjectHiddenComponent data) {
    }

    #region Utilities
    public void SetIsHidden(bool state) {
        if(isHidden != state) {
            isHidden = state;
            LocationGridTile gridTile = owner.gridTileLocation;
            if (gridTile != null) {
                gridTile.structure.RemovePOI(owner);
                gridTile.structure.AddPOI(owner, gridTile);
            }
            OnSetHiddenState();
        }
    }
    public void OnSetHiddenState() {
        BaseMapObjectVisual visual = owner.mapObjectVisual;
        if (visual) {
            if (isHidden) {
                visual.SetVisualAlpha(0.5f);
            } else {
                visual.SetVisualAlpha(1f);
            }
        }
    }
    #endregion
}

public class SaveDataTileObjectHiddenComponent : SaveData<TileObjectHiddenComponent> {

    public override void Save(TileObjectHiddenComponent data) {
        base.Save(data);

    }
    public override TileObjectHiddenComponent Load() {
        TileObjectHiddenComponent component = new TileObjectHiddenComponent(this);
        return component;
    }
}