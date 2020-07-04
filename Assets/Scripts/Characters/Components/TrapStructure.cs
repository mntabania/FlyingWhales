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

    #region Utilities
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
    #endregion
}
