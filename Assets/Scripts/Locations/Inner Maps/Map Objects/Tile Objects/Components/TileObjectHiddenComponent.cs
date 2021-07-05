using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class TileObjectHiddenComponent : TileObjectComponent {
    public bool isHidden { get; private set; }
    public bool affectAlpha { get; private set; }
    public TileObjectHiddenComponent() {
    }
    public TileObjectHiddenComponent(SaveDataTileObjectHiddenComponent data) {
        isHidden = data.isHidden;
        affectAlpha = data.affectAlpha;
    }

    #region Utilities
    public void SetIsHidden(TileObject owner, bool state, bool affectAlpha = true) {
        this.affectAlpha = affectAlpha;
        if (isHidden != state) {
            isHidden = state;
            LocationGridTile gridTile = owner.gridTileLocation;
            if (gridTile != null) {
                gridTile.structure.RemovePOI(owner);
                gridTile.structure.AddPOI(owner, gridTile);
            }
            OnSetHiddenState(owner);
        }
    }
    public void OnSetHiddenState(TileObject owner) {
        if (!affectAlpha) { return; }
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

    #region Loading
    public void LoadSecondWave(TileObject owner) {
        OnSetHiddenState(owner);
    }
    #endregion
}

public class SaveDataTileObjectHiddenComponent : SaveData<TileObjectHiddenComponent> {
    public bool isHidden;
    public bool affectAlpha;

    public override void Save(TileObjectHiddenComponent data) {
        base.Save(data);
        isHidden = data.isHidden;
        affectAlpha = data.affectAlpha;
    }
    public override TileObjectHiddenComponent Load() {
        TileObjectHiddenComponent component = new TileObjectHiddenComponent(this);
        return component;
    }
}