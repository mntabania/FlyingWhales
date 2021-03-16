using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPartyComponent : CharacterComponent {
    public Party currentParty { get; private set; }
    public bool isFollowingBeacon { get; private set; } //Do not save this because this will be set when character go to its behaviour

    #region getters
    public bool hasParty => currentParty != null;
    public bool isActiveMember => IsPartyActiveAndOwnerActivePartOfQuest();
    public bool isMemberThatJoinedQuest => IsPartyActiveAndOwnerJoinedQuest();
    #endregion

    public CharacterPartyComponent() {
    }

    public CharacterPartyComponent(SaveDataCharacterPartyComponent data) {
    }

    #region General
    public void SetCurrentParty(Party party) {
        currentParty = party;
    }
    private bool IsPartyActiveAndOwnerActivePartOfQuest() {
        if (hasParty && currentParty.isActive) {
            return currentParty.DidMemberJoinQuest(owner) && currentParty.IsMemberActive(owner);
        }
        return false;
    }
    private bool IsPartyActiveAndOwnerJoinedQuest() {
        if (hasParty && currentParty.isActive) {
            return currentParty.DidMemberJoinQuest(owner);
        }
        return false;
    }
    public bool IsAMemberOfParty(Party party) {
        return currentParty == party;
    }
    #endregion

    #region Beacon
    public void FollowBeacon() {
        if (!isFollowingBeacon) {
            if (hasParty) {
                Character beacon = currentParty.beaconComponent.currentBeaconCharacter;
                if (beacon != null && owner.hasMarker) {
                    isFollowingBeacon = true;
                    owner.movementComponent.UpdateSpeed();
                    owner.marker.GoToPOI(beacon, p_arrivalActionBeforeDigging: OnArriveFollowingBeacon);
                }
            }
        } else {
            UpdateFollowBeacon();
        }
    }
    private void OnArriveFollowingBeacon() {
        UnfollowBeacon();
    }
    private void UpdateFollowBeacon() {
        if (hasParty) {
            Character beacon = currentParty.beaconComponent.currentBeaconCharacter;
            if (beacon != null && owner.hasMarker) {
                owner.marker.GoToPOI(beacon, p_arrivalActionBeforeDigging: OnArriveFollowingBeacon);
            }
        }
    }
    public void UnfollowBeacon() {
        if (isFollowingBeacon) {
            if (hasParty && owner.hasMarker) {
                isFollowingBeacon = false;
                owner.movementComponent.UpdateSpeed();
                owner.marker.pathfindingAI.ClearAllCurrentPathData();
                owner.marker.StopMovement();
            }

        }
    }
    public bool CanFollowBeacon() {
        Character beacon = currentParty.beaconComponent.currentBeaconCharacter;
        if (hasParty && beacon != null) {
            if (owner != beacon) {
                if (isFollowingBeacon) {
                    return true;
                }
                if (owner.stateComponent.currentState == null && (owner.currentActionNode == null || owner.currentActionNode.associatedJobType == JOB_TYPE.PARTY_GO_TO)) {
                    if (owner.hasMarker && !owner.marker.IsPOIInVision(beacon)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterPartyComponent data) {
        if (!string.IsNullOrEmpty(data.currentParty)) {
            currentParty = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.currentParty);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterPartyComponent : SaveData<CharacterPartyComponent> {
    public string currentParty;

    #region Overrides
    public override void Save(CharacterPartyComponent data) {
        if (data.hasParty) {
            currentParty = data.currentParty.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentParty);
        }
    }

    public override CharacterPartyComponent Load() {
        CharacterPartyComponent component = new CharacterPartyComponent(this);
        return component;
    }
    #endregion
}