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

    //Works the same as forced structure but with area
    public Area forcedArea { get; private set; }

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
    public void SetForcedArea(Area p_area) {
        forcedArea = p_area;
    }
    public bool SatisfiesForcedArea(IPointOfInterest target) {
        if (forcedArea == null) {
            return true;
        }
        return target.gridTileLocation != null && target.gridTileLocation.area == forcedArea;
    }
    #endregion

    #region Utilities
    public void ResetAllTrapStructures() {
        SetStructureAndDuration(null, 0);
        SetForcedStructure(null);
    }
    public void ResetTrapArea() {
        SetForcedArea(null);
    }
    public void ResetAllTrappedValues() {
        if (IsTrapped()) {
            ResetAllTrapStructures();
        }
        if (IsTrappedInArea()) {
            ResetTrapArea();
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
    public bool IsTrappedInArea() {
        return forcedArea != null;
    }
    public bool IsTrapArea(Area p_area) {
        return forcedArea != null && forcedArea == p_area;
    }
    public bool IsTrappedAndTrapAreaIs(Area p_area) {
        return IsTrappedInArea() && IsTrapArea(p_area);
    }
    public bool IsTrappedAndTrapAreaIsNot(Area p_area) {
        return IsTrappedInArea() && !IsTrapArea(p_area);
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
            forcedArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.forcedHex);
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
        if (data.forcedArea != null) {
            forcedHex = data.forcedArea.persistentID;
        }
    }
    public override TrapStructure Load() {
        TrapStructure component = new TrapStructure(this);
        return component;
    }
}