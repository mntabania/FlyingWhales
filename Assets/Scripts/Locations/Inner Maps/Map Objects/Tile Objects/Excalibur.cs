using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;

public class Excalibur : TileObject {
    
    public enum Locked_State { Locked, Unlocked }
    private static readonly string[] traitsToBeGainedFromOwnership = new[] {
        "Inspiring", "Robust", "Mighty"
    };
    
    public Locked_State lockedState { get; private set; }
    /// <summary>
    /// List of traits that the current owner has gained by obtaining this.
    /// This is kept track of so that when the current owner loses possession of this, only
    /// the traits in this list will be removed from him/her. 
    /// </summary>
    private List<string> traitsGainedByCurrentOwner;
    private HashSet<Character> _finishedCharacters; //characters that have already inspected this.

    public Excalibur() {
        Initialize(TILE_OBJECT_TYPE.EXCALIBUR);
        SetLockedState(Locked_State.Locked);
        traitsGainedByCurrentOwner = new List<string>();
        _finishedCharacters = new HashSet<Character>();
    }
    public Excalibur(SaveDataTileObject data) {
        Initialize(data);
    }

    #region General
    private void SetLockedState(Locked_State lockedState) {
        this.lockedState = lockedState;
        if (mapVisual != null) {
            mapVisual.UpdateTileObjectVisual(this);    
        }
        switch (lockedState) {
            case Locked_State.Locked:
                traitContainer.AddTrait(this, "Indestructible");
                AddAdvertisedAction(INTERACTION_TYPE.INSPECT);
                break;
            case Locked_State.Unlocked:
                AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
                traitContainer.RemoveTrait(this, "Indestructible");
                traitContainer.AddTrait(this, "Treasure");
                break;
        }
    }
    #endregion

    #region Inspection
    public override void OnInspect(Character inspector) {
        base.OnInspect(inspector);
        AddFinishedCharacter(inspector);
        if (lockedState == Locked_State.Locked) {
            if (inspector.traitContainer.HasTrait("Blessed") && 
                inspector.traitContainer.HasTrait("Evil", "Treacherous") == false) {
                Log log = new Log(GameManager.Instance.Today(), "Tile Object", "Excalibur", "on_inspect_success");
                log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToInvolvedObjects();
                UnlockSword(inspector);
            } else {
                Log log = new Log(GameManager.Instance.Today(), "Tile Object", "Excalibur", "on_inspect_fail");
                log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToInvolvedObjects();
            }
        }
    }
    private void AddFinishedCharacter(Character character) {
        if (!_finishedCharacters.Contains(character)) {
            _finishedCharacters.Add(character);    
        }
    }
    public bool HasInspectedThis(Character character) {
        return _finishedCharacters.Contains(character);
    }
    #endregion

    #region Ownership
    private void UnlockSword(Character character) {
        LocationGridTile tile = gridTileLocation;
        SetLockedState(Locked_State.Unlocked);
        character.AssignClass("Hero");
        character.PickUpItem(this, true);
        //replace sword with rock
        tile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ROCK), tile);
    }
    public override void SetInventoryOwner(Character character) {
        Character previousOwner = isBeingCarriedBy; 
        base.SetInventoryOwner(character);
        if (previousOwner != character) {
            //remove traits gained from previous owner
            if (previousOwner != null) {
                for (int i = 0; i < traitsGainedByCurrentOwner.Count; i++) {
                    string trait = traitsGainedByCurrentOwner[i];
                    previousOwner.traitContainer.RemoveTrait(previousOwner, trait);
                }
            }
            traitsGainedByCurrentOwner.Clear();
            if (character != null) {
                //add traits to new carrier
                for (int i = 0; i < traitsToBeGainedFromOwnership.Length; i++) {
                    string traitName = traitsToBeGainedFromOwnership[i];
                    if (character.traitContainer.AddTrait(character, traitName)) {
                        traitsGainedByCurrentOwner.Add(traitName);
                    }
                }  
                character.combatComponent.UpdateMaxHPAndReset();
            }
        }
    }
    #endregion

    #region Testing
    public override string GetAdditionalTestingData() {
        string data = base.GetAdditionalTestingData();
        data += "\nInspected Characters: ";
        for (int i = 0; i < _finishedCharacters.Count; i++) {
            Character finishedCharacter = _finishedCharacters.ElementAt(i);
            data += $"{finishedCharacter.name}, ";
        }
        return data;
    }
    #endregion
    
}
