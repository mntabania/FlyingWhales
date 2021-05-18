using System.Collections.Generic;

public class TrainingDummy : TileObject{
    public TrainingDummy() {
        Initialize(TILE_OBJECT_TYPE.TRAINING_DUMMY);
    }
    public TrainingDummy(SaveDataTileObject data) : base(data) {
        
    }
}
