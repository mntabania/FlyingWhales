using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine.Assertions;

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
    private List<string> _traitsGainedByCurrentOwner;
    private HashSet<int> _finishedCharacters; //characters that have already inspected this.
    private string _previousClassOfCurrentOwner;

    #region Getters
    public List<string> traitsGainedByCurrentOwner => _traitsGainedByCurrentOwner;
    public HashSet<int> finishedCharacters => _finishedCharacters;
    public string previousClassOfCurrentOwner => _previousClassOfCurrentOwner;
    public override System.Type serializedData => typeof(SaveDataExcalibur);
    #endregion
    
    public Excalibur() {
        Initialize(TILE_OBJECT_TYPE.EXCALIBUR);
        SetLockedState(Locked_State.Locked);
        _traitsGainedByCurrentOwner = new List<string>();
        _finishedCharacters = new HashSet<int>();
    }
    public Excalibur(SaveDataExcalibur data) : base(data) {
        //SaveDataExcalibur saveDataExcalibur = data as SaveDataExcalibur;
        Assert.IsNotNull(data);
        _traitsGainedByCurrentOwner = data.traitsGainedByCurrentOwner; //new List<string>(data.traitsGainedByCurrentOwner);
        _finishedCharacters = new HashSet<int>(data.finishedCharacters);
        _previousClassOfCurrentOwner = data.previousClass;
    }

    #region General
    public void SetLockedState(Locked_State lockedState) {
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
            if (inspector.traitContainer.IsBlessed() && 
                inspector.traitContainer.HasTrait("Evil", "Treacherous", "Cultist") == false) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "Excalibur", "on_inspect_success", providedTags: LOG_TAG.Life_Changes);
                log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase(true);
                UnlockSword(inspector);
            } else {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "Excalibur", "on_inspect_fail", providedTags: LOG_TAG.Work);
                log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase(true);
            }
        }
    }
    private void AddFinishedCharacter(Character character) {
        if (!_finishedCharacters.Contains(character.id)) {
            _finishedCharacters.Add(character.id);    
        }
    }
    public bool HasInspectedThis(Character character) {
        return _finishedCharacters.Contains(character.id);
    }
    #endregion

    #region Ownership
    private void UnlockSword(Character character) {
        LocationGridTile tile = gridTileLocation;
        SetLockedState(Locked_State.Unlocked);
        character.PickUpItem(this, true);
        //replace sword with rock
        tile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ROCK), tile);
    }
    public override void SetInventoryOwner(Character p_newOwner) {
        Character previousOwner = isBeingCarriedBy; 
        base.SetInventoryOwner(p_newOwner);
        if (previousOwner != p_newOwner) {
            if (previousOwner != null) {
                TryRevertClassOfOwner(previousOwner);
                //remove traits gained from previous owner
                for (int i = 0; i < _traitsGainedByCurrentOwner.Count; i++) {
                    string trait = _traitsGainedByCurrentOwner[i];
                    previousOwner.traitContainer.RemoveTrait(previousOwner, trait);
                }
                SetCharacterOwner(null);
            }
            _previousClassOfCurrentOwner = string.Empty;
            _traitsGainedByCurrentOwner.Clear();
            if (p_newOwner != null) {
                SetCharacterOwner(p_newOwner);
                if (CanBecomeHero(p_newOwner)) {
                    _previousClassOfCurrentOwner = p_newOwner.characterClass.className;
                    p_newOwner.structureComponent.TryUnassignFromCurrentWorkStructureOnClassChange(p_newOwner, "Hero");
                    p_newOwner.classComponent.AssignClass("Hero");    
                }
                //add traits to new carrier
                for (int i = 0; i < traitsToBeGainedFromOwnership.Length; i++) {
                    string traitName = traitsToBeGainedFromOwnership[i];
                    if (p_newOwner.traitContainer.AddTrait(p_newOwner, traitName)) {
                        _traitsGainedByCurrentOwner.Add(traitName);
                    }
                }  
                p_newOwner.combatComponent.UpdateMaxHPAndReset();
            }
        }
    }
    private void TryRevertClassOfOwner(Character p_owner) {
        if (p_owner.characterClass.className == "Hero") {
            p_owner.classComponent.AssignClass(previousClassOfCurrentOwner);
        } else if (p_owner.characterClass.className == "Werewolf" && p_owner.classComponent.previousClassName == "Hero") {
            //if character is currently a werewolf, then set his/her previous class to _previousClassOfCurrentOwner
            //so that when he/she reverts to normal, he/she will no longer be a Hero.
            p_owner.classComponent.OverridePreviousClassName(previousClassOfCurrentOwner); 
        }
    }
    private bool CanBecomeHero(Character p_character) {
        if (!p_character.traitContainer.IsBlessed()) {
            return false;
        }
        if (p_character.traitContainer.HasTrait("Evil", "Treacherous", "Cultist")) {
            return false;
        }
        if (p_character.characterClass.className == "Werewolf" || p_character.characterClass.className == "Necromancer" || p_character.characterClass.className == "Vampire Lord") {
            return false;
        }
        return true;
    }
    #endregion
}

#region Save Data
public class SaveDataExcalibur : SaveDataTileObject {
    public Excalibur.Locked_State lockedState;
    public List<string> traitsGainedByCurrentOwner;
    public List<int> finishedCharacters;
    public string previousClass;
    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        Excalibur excalibur = tileObject as Excalibur;
        Assert.IsNotNull(excalibur);
        lockedState = excalibur.lockedState;
        traitsGainedByCurrentOwner = new List<string>(excalibur.traitsGainedByCurrentOwner);
        finishedCharacters = new List<int>(excalibur.finishedCharacters);
        previousClass = excalibur.previousClassOfCurrentOwner;
    }
    public override TileObject Load() {
        TileObject tileObject = base.Load();
        Excalibur excalibur = tileObject as Excalibur;
        Assert.IsNotNull(excalibur);
        excalibur.SetLockedState(lockedState);
        return tileObject;
    }
}
#endregion
