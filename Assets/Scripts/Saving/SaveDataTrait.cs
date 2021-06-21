using System.Collections.Generic;
using Traits;

//IMPORTANT NOTE: Class name format for all derived classes of SaveDataTrait must be: "SaveData" + [Trait Name], example: SaveDataKleptomaniac
[System.Serializable]
public class SaveDataTrait : SaveData<Trait>, ISavableCounterpart {
    public string _persistentID;
    public OBJECT_TYPE _objectType;
    public string name;
    public List<string> responsibleCharacters;
    public INTERACTION_TYPE gainedFromDoingType;
    public bool isGainedFromDoingStealth;

    #region getters
    public string persistentID => _persistentID;
    public OBJECT_TYPE objectType => _objectType;
    #endregion
    
    public override void Save(Trait trait) {
        _persistentID = trait.persistentID;
        _objectType = trait.objectType;
        name = trait.name;
        if (trait.responsibleCharacters != null) {
            responsibleCharacters = new List<string>();
            for (int i = 0; i < trait.responsibleCharacters.Count; i++) {
                Character character = trait.responsibleCharacters[i];
                responsibleCharacters.Add(character.persistentID);
            }    
        }
        gainedFromDoingType = trait.gainedFromDoingType;
        isGainedFromDoingStealth = trait.isGainedFromDoingStealth;
        //if (trait.gainedFromDoing != null) {
        //    gainedFromDoing = trait.gainedFromDoing.persistentID;
        //    SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(trait.gainedFromDoing);
        //}
        
    }
    public override Trait Load() {
        Trait trait = TraitManager.Instance.LoadTrait(this);
        return trait;
    }
}
