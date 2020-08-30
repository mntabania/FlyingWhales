using System.Collections.Generic;

public class WurmHole : TileObject{
    public WurmHole wurmHoleConnection { get; private set; }

    public WurmHole() {
        Initialize(TILE_OBJECT_TYPE.WURM_HOLE);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Fireproof");
    }
    public WurmHole(SaveDataTileObject data) {
        
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Fireproof");
    }

    public void SetWurmHoleConnection(WurmHole wurmHole) {
        wurmHoleConnection = wurmHole;
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        if(wurmHoleConnection.gridTileLocation != null) {
            wurmHoleConnection.gridTileLocation.structure.RemovePOI(wurmHoleConnection);
        }
    }

    public void TravelThroughWurmHole(Character character) {
        character.movementComponent.SetCameFromWurmHole(true);
        CharacterManager.Instance.Teleport(character, wurmHoleConnection.gridTileLocation);
        Messenger.Broadcast(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, character as IPointOfInterest, "");
        character.jobQueue.CancelAllJobs();
        character.combatComponent.ClearHostilesInRange();
        character.combatComponent.ClearAvoidInRange();
    }
}
