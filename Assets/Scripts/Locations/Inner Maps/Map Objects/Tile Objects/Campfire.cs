using System.Collections.Generic;

public class Campfire : TileObject{
    private int currentTimer;
    private int timer;
    
    public Campfire() {
        Initialize(TILE_OBJECT_TYPE.CAMPFIRE);
        AddAdvertisedAction(INTERACTION_TYPE.WARM_UP);
        timer = GameManager.Instance.GetTicksBasedOnHour(8);
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
    }
    public Campfire(SaveDataTileObject data) {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
        AddAdvertisedAction(INTERACTION_TYPE.WARM_UP);
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
