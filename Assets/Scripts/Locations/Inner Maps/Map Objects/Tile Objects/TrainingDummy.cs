using System.Collections.Generic;

public class TrainingDummy : TileObject{
    public TrainingDummy() {
        Initialize(TILE_OBJECT_TYPE.TRAINING_DUMMY);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public TrainingDummy(SaveDataTileObject data) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }
}
