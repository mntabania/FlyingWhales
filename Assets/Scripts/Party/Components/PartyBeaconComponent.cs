using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyBeaconComponent : PartyComponent {
    public Character currentBeaconCharacter { get; private set; }

    public PartyBeaconComponent() {
    }

    public void Initialize() {
        SubscribeToSignals();
    }
    public void Initialize(SaveDataPartyBeaconComponent data) {
        SubscribeToSignals();
    }

    #region Listeners
    private void SubscribeToSignals() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCannotMove);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCannotPerform);
        Messenger.AddListener<Character, Character>(CharacterSignals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, )
    }
    private void UnsubscribeFromSignals() {
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCannotMove);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCannotPerform);
        Messenger.RemoveListener<Character, Character>(CharacterSignals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
    }
    private void OnCharacterCannotMove(Character p_character) {
        if(currentBeaconCharacter == p_character) {
            UpdateBeaconCharacter();
        }
    }
    private void OnCharacterCannotPerform(Character p_character) {
        if (currentBeaconCharacter == p_character) {
            UpdateBeaconCharacter();
        }
    }
    private void OnSeizePOI(IPointOfInterest p_poi) {
        if(currentBeaconCharacter == p_poi) {
            UpdateBeaconCharacter();
        }
    }
    private void OnCharacterRemovedFromVision(Character p_character, Character p_target) {
        if(p_target == currentBeaconCharacter && p_character.partyComponent.IsAMemberOfParty(owner) && owner.partyState == PARTY_STATE.Moving) {
            if (p_character.partyComponent.isMemberThatJoinedQuest) {
                if (p_character.partyComponent.CanFollowBeacon()) {
                    p_character.partyComponent.FollowBeacon();
                }
            }
        }
    }
    #endregion

    #region Beacon
    private void SetBeaconCharacter(Character p_character) {
        if(currentBeaconCharacter != p_character) {
            currentBeaconCharacter = p_character;
            UpdateMovementOfAllMembersAccordingToBeacon();
        }
    }

    public void UpdateBeaconCharacter() {
        if(owner.partyState == PARTY_STATE.Moving) {
            for (int i = 0; i < owner.membersThatJoinedQuest.Count; i++) {
                Character member = owner.membersThatJoinedQuest[i];
                if (!member.isBeingSeized && member.limiterComponent.canPerform && member.limiterComponent.canMove && !member.isDead) {
                    if (owner.IsMemberActive(member)) {
                        SetBeaconCharacter(member);
                        break;
                    }
                }
            }
        } else {
            SetBeaconCharacter(null);
        }
    }
    public void UpdateMovementOfAllMembersAccordingToBeacon() {
        if(owner.partyState == PARTY_STATE.Moving) {
            if(currentBeaconCharacter != null) {
                for (int i = 0; i < owner.membersThatJoinedQuest.Count; i++) {
                    Character member = owner.membersThatJoinedQuest[i];
                    if (member.partyComponent.CanFollowBeacon()) {
                        member.partyComponent.FollowBeacon();
                    }
                }
            } else {
                for (int i = 0; i < owner.membersThatJoinedQuest.Count; i++) {
                    Character member = owner.membersThatJoinedQuest[i];
                    member.partyComponent.UnfollowBeacon();
                }
            }
        }
    }
    #endregion

    #region Utilities
    public void OnDestroyParty() {
        UnsubscribeFromSignals();
        SetBeaconCharacter(null);
    }
    public void OnRemoveMemberThatJoinedQuest(Character p_member) {
        p_member.partyComponent.UnfollowBeacon();
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataPartyBeaconComponent data) {
        if (!string.IsNullOrEmpty(data.currentBeaconCharacter)) {
            currentBeaconCharacter = CharacterManager.Instance.GetCharacterByPersistentID(data.currentBeaconCharacter);
        }
    }
    #endregion
}

public class SaveDataPartyBeaconComponent : SaveData<PartyBeaconComponent> {
    public string currentBeaconCharacter;

    #region Overrides
    public override void Save(PartyBeaconComponent data) {
        if(data.currentBeaconCharacter != null) {
            currentBeaconCharacter = data.currentBeaconCharacter.persistentID;
        }
    }
    #endregion
}