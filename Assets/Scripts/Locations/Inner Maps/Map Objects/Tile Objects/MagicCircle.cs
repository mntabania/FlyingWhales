using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagicCircle : TileObject {
    public MagicCircle() {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, };
        Initialize(TILE_OBJECT_TYPE.MAGIC_CIRCLE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DARK_RITUAL);
    }
    public MagicCircle(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT, };
        Initialize(data, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DARK_RITUAL);
    }

    #region Overrides
    protected override void OnSetObjectAsUnbuilt() {
        base.OnSetObjectAsUnbuilt();
        AddAdvertisedAction(INTERACTION_TYPE.DRAW_MAGIC_CIRCLE);
    }
    #endregion

    public override string ToString() {
        return $"Magic Circle {id.ToString()}";
    }
}
