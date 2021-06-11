using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class CharacterStructureComponent : CharacterComponent {
    public ManMadeStructure workPlaceStructure { get; private set; } //Do not save this because the loading of this is handled in ManMadeStructure - LoadReferences

    public CharacterStructureComponent() {
    }
    public CharacterStructureComponent(SaveDataCharacterStructureComponent data) {
    }


    #region General
    public void SetWorkPlaceStructure(ManMadeStructure p_structure) {
        workPlaceStructure = p_structure;
    }
    public bool HasWorkPlaceStructure() {
        return workPlaceStructure != null;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterStructureComponent data) {
        //Currently N/A
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterStructureComponent : SaveData<CharacterStructureComponent> {

    #region Overrides
    public override void Save(CharacterStructureComponent data) {

    }

    public override CharacterStructureComponent Load() {
        CharacterStructureComponent component = new CharacterStructureComponent(this);
        return component;
    }
    #endregion
}