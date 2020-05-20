using System.Collections.Generic;
using Locations.Features;

public class RegionTileObject : TileObject {

    public RegionTileObject() {
        Initialize(TILE_OBJECT_TYPE.REGION_TILE_OBJECT, false);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public RegionTileObject(SaveDataTileObject data) {
        Initialize(data, false);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public void SetName(Region region) {
        this.name = $"{region.name} Region tile object";
    }

    #region Overrides
    public override string ToString() {
        return $"{this.name} {this.id}";
    }
    #endregion

    public void UpdateAdvertisements(Region region) {
        advertisedActions.Clear();
        if (region.coreTile.isCorrupted) {
            advertisedActions.Add(INTERACTION_TYPE.CLEANSE_REGION);
            
            //landmark types
            if (region.mainLandmark != null) {
                if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE) {
                    advertisedActions.Add(INTERACTION_TYPE.ATTACK_REGION);
                }
            }

            //features
            if (region.HasTileWithFeature(TileFeatureDB.Hallowed_Ground_Feature)) {
                advertisedActions.Add(INTERACTION_TYPE.HOLY_INCANTATION);
                advertisedActions.Add(INTERACTION_TYPE.DEMONIC_INCANTATION);
            }
        } else {
            advertisedActions.Add(INTERACTION_TYPE.STUDY);
            advertisedActions.Add(INTERACTION_TYPE.SEARCHING);
            //TODO:
            // if (region.owner == null) {
            //     advertisedActions.Add(INTERACTION_TYPE.CLAIM_REGION);
            // } else {
            //     if (region.locationType.IsSettlementType()) {
            //         advertisedActions.Add(INTERACTION_TYPE.INVADE_REGION);      
            //     }
            // }
            
            //landmark types
            if (region.mainLandmark != null) {
                if (region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.NONE) {
                    advertisedActions.Add(INTERACTION_TYPE.ATTACK_REGION);
                }
                if (region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.FARM) {
                    advertisedActions.Add(INTERACTION_TYPE.HARVEST_FOOD_REGION);
                } else if (region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.LUMBERYARD) {
                    advertisedActions.Add(INTERACTION_TYPE.CHOP_WOOD_REGION);
                }    
            }

            //features
            if (region.HasTileWithFeature(TileFeatureDB.Game_Feature)) {
                advertisedActions.Add(INTERACTION_TYPE.FORAGE_FOOD_REGION);
            } 
            if (region.HasTileWithFeature(TileFeatureDB.Hallowed_Ground_Feature)) {
                advertisedActions.Add(INTERACTION_TYPE.HOLY_INCANTATION);
                advertisedActions.Add(INTERACTION_TYPE.DEMONIC_INCANTATION);
            }
        }
    }
}
