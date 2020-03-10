using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.EventSystems;

public class BedObjectGameObject : TileObjectGameObject {

    [SerializeField] private Sprite bed1Sleeping;
    [SerializeField] private Sprite bed2Sleeping;

    public override void UpdateTileObjectVisual(TileObject bed) {
        int userCount = bed.users.Length;
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
}
