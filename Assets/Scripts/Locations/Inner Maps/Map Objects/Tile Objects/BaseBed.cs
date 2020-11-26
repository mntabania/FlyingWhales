using System.Linq;
using Inner_Maps;
using Traits;

public abstract class BaseBed : TileObject {
    private Character[] bedUsers; //array of characters, currently using the bed

    public override Character[] users {
        get { return bedUsers.Where(x => x != null).ToArray(); }
    }
    public BaseBed(int slots) {
        bedUsers = new Character[slots];
    }
    public BaseBed(SaveDataTileObject data, int slots) : base(data) {
        bedUsers = new Character[slots];
    }
    
    #region Overrides
    public override void SetPOIState(POI_STATE state) {
        base.SetPOIState(state);
        if (IsSlotAvailable()) {
            if (GetActiveUserCount() > 0) {
                UpdateUsedBedAsset();
            } else {
                if (gridTileLocation != null && mapVisual != null) {
                    mapVisual.UpdateTileObjectVisual(this);
                }
            }
        }
    }
    public override void OnTileObjectGainedTrait(Trait trait) {
        base.OnTileObjectGainedTrait(trait);
        if (trait.name == "Burning") {
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, this as IPointOfInterest, "bed is burning");
            Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, this as IPointOfInterest, "bed is burning");
        }
    }
    #endregion
    
     #region Users
     public bool IsSlotAvailable() {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == null) {
                return true; //there is an available slot
            }
        }
        return false;
    }
    public override bool AddUser(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == null) {
                bedUsers[i] = character;
                character.SetTileObjectLocation(this);
                UpdateUsedBedAsset();
                //disable the character's marker
                character.marker.SetVisualState(false);
                Messenger.Broadcast(TileObjectSignals.ADD_TILE_OBJECT_USER, GetBase(), character);
                return true;
            }
        }
        return false;
    }
    public override bool RemoveUser(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == character) {
                bedUsers[i] = null;
                character.SetTileObjectLocation(null);
                UpdateUsedBedAsset();
                //enable the character's marker
                character.marker.SetVisualState(true);
                if (character.gridTileLocation != null && character.traitContainer.HasTrait("Paralyzed")) {
                    //When a paralyzed character awakens, place it on an adjacent tile in the same Structure
                    LocationGridTile gridTile = character.gridTileLocation.GetFirstNeighborThatMeetCriteria(x => x.structure == character.gridTileLocation.structure && x.IsPassable());
                    if(gridTile != null) {
                        character.marker.PlaceMarkerAt(gridTile);
                    } else {
                        character.marker.PlaceMarkerAt(character.gridTileLocation);
                    }
                } else {
                    character.marker.UpdateAnimation();
                }
                Messenger.Broadcast(TileObjectSignals.REMOVE_TILE_OBJECT_USER, GetBase(), character);
                return true;
            }
        }
        return false;
    }
    public int GetActiveUserCount() {
        int count = 0;
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] != null) {
                count++;
            }
        }
        return count;
    }
    protected void LoadUser(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == null) {
                bedUsers[i] = character;
                character.SetTileObjectLocation(this);
                UpdateUsedBedAsset();
                //disable the character's marker
                character.marker.SetVisualState(false);
                break;
            }
        }
    }
    #endregion
    
    #region Inquiry
    public virtual bool CanUseBed(Character character) { return IsSlotAvailable(); }
    #endregion

    #region Visuals
    private void UpdateUsedBedAsset() {
        if (gridTileLocation == null) {
            return;
        }
        mapVisual.UpdateTileObjectVisual(this);
    }
    #endregion
}
