using System;
using System.Collections.Generic;
using Characters.Components;
using Traits;

public class BedClinic : BaseBed, CharacterEventDispatcher.ITraitListener {
    public override Type serializedData => typeof(SaveDataBedClinic);
    public BedClinic() : base(1) {
        Initialize(TILE_OBJECT_TYPE.BED_CLINIC);
        AddAdvertisedAction(INTERACTION_TYPE.SLEEP);
        AddAdvertisedAction(INTERACTION_TYPE.NAP);
        AddAdvertisedAction(INTERACTION_TYPE.RECUPERATE);
    }
    public BedClinic(SaveDataTileObject data) : base(data, 1) { }
    
    protected override string GenerateName() { return "Hospice Bed"; }

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
        AddAdvertisedAction(INTERACTION_TYPE.SLEEP);
        AddAdvertisedAction(INTERACTION_TYPE.NAP);
        AddAdvertisedAction(INTERACTION_TYPE.RECUPERATE);
    }
    #endregion
    
    #region Overrides
    public override void OnDoActionToObject(ActualGoapNode action) {
        base.OnDoActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.QUARANTINE:
                AddUser(action.poiTarget as Character);
                break;
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                mapVisual?.UpdateTileObjectVisual(this);
                break;
            case INTERACTION_TYPE.RECUPERATE:
                AddUser(action.actor);
                mapVisual?.UpdateTileObjectVisual(this);
                break;

        }
    }
    public override void OnDoneActionToObject(ActualGoapNode action) {
        base.OnDoneActionToObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                mapVisual?.UpdateTileObjectVisual(this);
                break;
            case INTERACTION_TYPE.RECUPERATE:
                RemoveUser(action.actor);
                mapVisual?.UpdateTileObjectVisual(this);
                break;
        }
    }
    public override void OnCancelActionTowardsObject(ActualGoapNode action) {
        base.OnCancelActionTowardsObject(action);
        switch (action.goapType) {
            case INTERACTION_TYPE.SLEEP:
            case INTERACTION_TYPE.NAP:
                mapVisual?.UpdateTileObjectVisual(this);
                break;
            case INTERACTION_TYPE.RECUPERATE:
                RemoveUser(action.actor);
                mapVisual?.UpdateTileObjectVisual(this);
                break;
        }
    }
    protected override bool AddUser(Character character) {
        if (base.AddUser(character)) {
            character.eventDispatcher.SubscribeToCharacterLostTrait(this);
            return true;
        }
        return false;
    }
    public override bool RemoveUser(Character character) {
        if (base.RemoveUser(character)) {
            character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);
            //whenever a character is removed from the bed, make sure to remove its quarantined status,
            //this is so that the character does not get stuck if anything happens to the bed it is in.
            character.traitContainer.RemoveTrait(character, "Quarantined");
            return true;
        }
        return false;
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Character[] currentUsers = users;
        if (currentUsers != null && currentUsers.Length > 0) {
            for (int i = 0; i < currentUsers.Length; i++) {
                Character currentUser = currentUsers[i];
                if (currentUser != null) {
                    RemoveUser(currentUser);
                }
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
