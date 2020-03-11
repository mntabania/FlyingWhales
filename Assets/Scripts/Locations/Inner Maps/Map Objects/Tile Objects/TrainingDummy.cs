using System.Collections.Generic;

public class TrainingDummy : TileObject{
    public TrainingDummy() {
        Initialize(TILE_OBJECT_TYPE.TRAINING_DUMMY);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public TrainingDummy(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
