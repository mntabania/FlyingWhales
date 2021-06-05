﻿using System.Collections.Generic;

public class Torch : TileObject{

    private InnerMapLight m_innerMapLight;
    public InnerMapLight InnerMap {
        get {
            if (m_innerMapLight == null) {
                m_innerMapLight = baseMapObjectVisual.GetComponentInChildren<InnerMapLight>(true);
            }
            return m_innerMapLight;
        }
    }

    public void EnableInnermapLight() {
        InnerMap.gameObject.SetActive(true);
    }
    public void DisableInnerMapLight() {
        InnerMap.gameObject.SetActive(false);
    }

    public Torch() {
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        Initialize(TILE_OBJECT_TYPE.TORCH);
        traitContainer.AddTrait(this, "Immovable");
    }
    public Torch(SaveDataTileObject data) : base(data) {
        
    }

	protected override void OnSetObjectAsUnbuilt() {
        base.OnSetObjectAsUnbuilt();
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
        AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
    }
    protected override void OnSetObjectAsBuilt() {
        base.OnSetObjectAsBuilt();
        RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
        RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
    }
}
