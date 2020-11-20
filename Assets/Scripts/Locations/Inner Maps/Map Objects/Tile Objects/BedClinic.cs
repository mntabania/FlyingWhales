using System;
using System.Collections.Generic;
using Characters.Components;
using Traits;

public class BedClinic : BaseBed, CharacterEventDispatcher.ITraitListener {
    public override Type serializedData => typeof(SaveDataBedClinic);
    public BedClinic() : base(1) {
        Initialize(TILE_OBJECT_TYPE.BED_CLINIC);
    }
    public BedClinic(SaveDataTileObject data) : base(data, 1) { }
    
    protected override string GenerateName() { return "Apothecary Bed"; }

    #region Loading
    public override void LoadAdditionalInfo(SaveDataTileObject data) {
        base.LoadAdditionalInfo(data);
        if (data is SaveDataBedClinic saveDataBedClinic) {
            for (int i = 0; i < saveDataBedClinic.userIDS.Count; i++) {
                string userID = saveDataBedClinic.userIDS[i];
                Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(userID);
                LoadUser(character);
                character.eventDispatcher.SubscribeToCharacterLostTrait(this);
            }
        }
    }
    #endregion
    
    #region Overrides
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.QUARANTINE:
                AddUser(action.poiTarget as Character);
                break;
        }
    }
    public override bool AddUser(Character character) {
        if (base.AddUser(character)) {
            character.eventDispatcher.SubscribeToCharacterLostTrait(this);
            return true;
        }
        return false;
    }
    public override bool RemoveUser(Character character) {
        if (base.RemoveUser(character)) {
            character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);
            return true;
        }
        return false;
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Character[] currentUsers = users;
        if (currentUsers != null && currentUsers.Length > 0) {
            for (int i = 0; i < currentUsers.Length; i++) {
                RemoveUser(currentUsers[i]);
            }
        }
    }
    #endregion

    public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait) { }
    public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy) {
        if (p_lostTrait is Quarantined) {
            RemoveUser(p_character); //character is no longer quarantined.
        }
    }
}

public class SaveDataBedClinic : SaveDataTileObject {

    public List<string> userIDS;
    
    public override void Save(TileObject data) {
        base.Save(data);
        userIDS = new List<string>();
        BedClinic bedClinic = data as BedClinic;
        for (int i = 0; i < bedClinic.users.Length; i++) {
            Character user = bedClinic.users[i];
            if (user != null) {
                userIDS.Add(user.persistentID);
            }
        }
    }
}
