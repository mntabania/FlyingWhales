using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;

//This class is for characters
//If this is not empty the character will do idle actions and needs inside the structure referenced here
//If the duration for this is zero (0), the data will not be cleared out until it is forced to be cleared out
public class TrapStructure {
    public LocationStructure structure { get; private set; }
    public int duration { get; private set; }
    public int currentDuration { get; private set; }
    
    //when this is set the character that owns this, will not include objects not in this structure to his/her plans.
    //setting this is manual, and is in no way related to the trap structures duration
    public LocationStructure forcedStructure { get; private set; }

    //Works the same as forced structure but with hex tile
    public HexTile forcedHex { get; private set; }

    public TrapStructure() {

    }

    public TrapStructure(SaveDataTrapStructure data) {
        duration = data.duration;
        currentDuration = data.currentDuration;
    }

    //This will set the structure and its duration, as well as reset the current duration
    public void SetStructureAndDuration(LocationStructure structure, int duration) {
        this.structure = structure;
        this.duration = duration;
        currentDuration = 0;
    }

    public void IncrementCurrentDuration(int amount) {
        if(duration > 0 && structure != null) {
            currentDuration += amount;
            if (currentDuration >= duration) {
                //Clear out data, duration is reached;
                SetStructureAndDuration(null, 0);
            }
        }
    }

    #region Forced Structure
    public void SetForcedStructure(LocationStructure structure) {
        forcedStructure = structure;
    }
    public bool SatisfiesForcedStructure(IPointOfInterest target) {
        if (forcedStructure == null) {
            return true;
        }
        return target.gridTileLocation != null && target.gridTileLocation.structure == forcedStructure;
    }
    #endregion

    #region Forced HexTile
    public void SetForcedHex(HexTile hex) {
        forcedHex = hex;
    }
    public bool SatisfiesForcedHex(IPointOfInterest target) {
        if (forcedHex == null) {
            return true;
        }
        return target.gridTileLocation != null && target.gridTileLocation.collectionOwner.isPartOfParentRegionMap 
            && target.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == forcedHex;
    }
    #endregion

    #region Utilities
    public void ResetAllTrapStructures() {
        SetStructureAndDuration(null, 0);
        SetForcedStructure(null);
    }
    public void ResetAllTrapHexes() {
        SetForcedHex(null);
    }
    public void ResetAllTrappedValues() {
        if (IsTrapped()) {
            ResetAllTrapStructures();
        }
        if (IsTrappedInHex()) {
            ResetAllTrapHexes();
        }
    }
    public bool IsTrapped() {
        return forcedStructure != null || structure != null;
    }
    public bool IsTrapStructure(LocationStructure structure) {
        return structure != null && (structure == this.structure || structure == forcedStructure);
    }
    public bool IsTrappedAndTrapStructureIs(LocationStructure structure) {
        return IsTrapped() && IsTrapStructure(structure);
    }
    public bool IsTrappedAndTrapStructureIsNot(LocationStructure structure) {
        return IsTrapped() && !IsTrapStructure(structure);
    }
    public bool IsTrappedInHex() {
        return forcedHex != null;
    }
    public bool IsTrapHex(HexTile hex) {
        return forcedHex != null && forcedHex == hex;
    }
    public bool IsTrappedAndTrapHexIs(HexTile hex) {
        return IsTrappedInHex() && IsTrapHex(hex);
    }
    public bool IsTrappedAndTrapHexIsNot(HexTile hex) {
        return IsTrappedInHex() && !IsTrapHex(hex);
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataTrapStructure data) {
        if (!string.IsNullOrEmpty(data.structure)) {
            structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.structure);
        }
        if (!string.IsNullOrEmpty(data.forcedStructure)) {
            forcedStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.forcedStructure);
        }
        if (!string.IsNullOrEmpty(data.forcedHex)) {
            forcedHex = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.forcedHex);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataTrapStructure : SaveData<TrapStructure> {
    public string structure;
    public int duration;
    public int currentDuration;

    public string forcedStructure;
    public string forcedHex;

    public override void Save(TrapStructure data) {
        base.Save(data);
        duration = data.duration;
        currentDuration = data.currentDuration;

        if(data.structure != null) {
            structure = data.structure.persistentID;
        }
        if (data.forcedStructure != null) {
            forcedStructure = data.forcedStructure.persistentID;
        }
        if (data.forcedHex != null) {
            forcedHex = data.forcedHex.persistentID;
        }
    }
    public override TrapStructure Load() {
        TrapStructure component = new TrapStructure(this);
        return component;
    }
}