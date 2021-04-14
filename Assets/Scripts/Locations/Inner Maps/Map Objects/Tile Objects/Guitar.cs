using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Guitar : TileObject {

    public Guitar() {
        Initialize(TILE_OBJECT_TYPE.GUITAR);
        AddAdvertisedAction(INTERACTION_TYPE.PLAY_GUITAR);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
    }
    public Guitar(SaveDataTileObject data) {
    }
    public override string ToString() {
        return $"Guitar {id.ToString()}";
    }

    public virtual bool CanBeReplaced() {
        return true;
    }
}
