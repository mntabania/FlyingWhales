using System.Collections.Generic;

public class ProfessionPedestal : TileObject{
    public ProfessionPedestal() {
        Initialize(TILE_OBJECT_TYPE.PROFESSION_PEDESTAL, false);
        traitContainer.AddTrait(this, "Indestructible");
        //AddAdvertisedAction(INTERACTION_TYPE.CHANGE_CLASS);
    }
    public ProfessionPedestal(SaveDataTileObject data) : base(data) {
        
    }
}