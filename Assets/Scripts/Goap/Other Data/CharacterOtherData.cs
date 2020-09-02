using UnityEngine.Assertions;

public class CharacterOtherData : OtherData {
    public Character character { get; }
    public override object obj => character;
    
    public CharacterOtherData(Character character) {
        this.character = character;
    }
    public CharacterOtherData(SaveDataCharacterOtherData saveData) {
        this.character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveData.characterID);
    }
    
    public override SaveDataOtherData Save() {
        SaveDataCharacterOtherData saveDataCharacterOtherData = new SaveDataCharacterOtherData();
        saveDataCharacterOtherData.Save(this);
        return saveDataCharacterOtherData;
    }
}

#region Save Data
public class SaveDataCharacterOtherData : SaveDataOtherData {
    public string characterID;
    public override void Save(OtherData data) {
        base.Save(data);
        CharacterOtherData characterOtherData = data as CharacterOtherData;
        Assert.IsNotNull(characterOtherData);
        characterID = characterOtherData.character.persistentID;
    }
    public override OtherData Load() {
        return new CharacterOtherData(this);
    }
}
#endregion