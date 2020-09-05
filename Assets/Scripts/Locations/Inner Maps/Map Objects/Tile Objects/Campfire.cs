using System.Collections.Generic;

public class Campfire : TileObject{
    private int currentTimer;
    private int timer;
    
    public Campfire() {
        Initialize(TILE_OBJECT_TYPE.CAMPFIRE);
        AddAdvertisedAction(INTERACTION_TYPE.WARM_UP);
        timer = GameManager.Instance.GetTicksBasedOnHour(8);
    }
    public Campfire(SaveDataTileObject data) : base(data) {
        timer = GameManager.Instance.GetTicksBasedOnHour(8);
    }

    #region Overrides
    public override void SetCharacterOwner(Character newOwner) {
        Character prevOwner = characterOwner;
        base.SetCharacterOwner(newOwner);
        if(characterOwner != null && characterOwner != prevOwner) {
            //Reset unown timer
            currentTimer = 0;
        }
    }
    #endregion
}