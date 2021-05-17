using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagicCircle : TileObject {
    public MagicCircle() {
        Initialize(TILE_OBJECT_TYPE.MAGIC_CIRCLE, false);
        AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        AddAdvertisedAction(INTERACTION_TYPE.DARK_RITUAL);
    }
    public MagicCircle(SaveDataTileObject data) : base(data) { }

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
