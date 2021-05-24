using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
public abstract class BaseBed : TileObject {
    private Character[] bedUsers; //array of characters, currently using the bed

    public override Character[] users {
        get { return bedUsers; } //.Where(x => x != null).ToArray() //Remove use of ToArray
    }
    public BaseBed(int slots) {
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
        bedUsers = new Character[slots];
    }
    public BaseBed(SaveDataTileObject data, int slots) : base(data) {
        AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
        AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
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
     protected override bool AddUser(Character character) {
        for (int i = 0; i < bedUsers.Length; i++) {
            if (bedUsers[i] == null) {
                bedUsers[i] = character;
                character.SetTileObjectLocation(this);
                UpdateUsedBedAsset();
                //disable the character's marker

                //Put character marker on a walkable node when he enters a bed so that when another character targets the character it will always be reachable
                //https://trello.com/c/pLXVewRw/4162-combat-not-starting-when-character-is-targeting-someone-on-bed-near-wall-corner
                Vector3 pos = gridTileLocation.GetPositionWithinTileThatIsOnAWalkableNode();
                character.marker.pathfindingAI.Teleport(pos);
                // character.marker.PlaceMarkerAt(gridTileLocation);
                character.marker.SetVisualState(false);
                character.tileObjectComponent.SetBedBeingUsed(this);
                Messenger.Broadcast(TileObjectSignals.ADD_TILE_OBJECT_USER, GetBase(), character);

                //Once a character enters a bed and the current selector highlight (the white square outline) is on them, transfer the highlight to the bed because the character's marker will become invisible
                //https://trello.com/c/kaVJHBV5/3459-selection-on-sleeping
                if (Selector.Instance.IsSelected(character)) {
                    if (mapObjectVisual != null) {
                        Selector.Instance.Select(this, mapObjectVisual.transform);
                    }
                }
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
                character.tileObjectComponent.SetBedBeingUsed(null);
                if (character.gridTileLocation != null && character.traitContainer.HasTrait("Paralyzed")) {
                    //When a paralyzed character awakens, place it on an adjacent tile in the same Structure
                    LocationGridTile gridTile = character.gridTileLocation.GetFirstNeighborThatIsPassableAndSameStructureAs(character.gridTileLocation.structure);
                    if(gridTile != null) {
                        character.marker.PlaceMarkerAt(gridTile);
                    } else {
                        character.marker.PlaceMarkerAt(character.gridTileLocation);
                    }
                } else {
                    character.marker.UpdateAnimation();
                }
                Messenger.Broadcast(TileObjectSignals.REMOVE_TILE_OBJECT_USER, GetBase(), character);
                if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == character) {
                    //https://trello.com/c/A2IPtNEN/3907-unity-v034290318-camera-follow-bug
                    if (character.hasMarker) {
                        Selector.Instance.Select(character, character.marker.transform);
                        character.CenterOnCharacter();
                    }
                }
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
