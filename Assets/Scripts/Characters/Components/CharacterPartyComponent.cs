using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

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
        if (currentParty != null) {
            return currentParty.IsPartyTheSameAsThisParty(party);
        } else {
            return false;
        }
    }
    #endregion

    #region Beacon
    public void FollowBeacon() {
        if (!isFollowingBeacon) {
            if (hasParty) {
                Character beacon = currentParty.beaconComponent.currentBeaconCharacter;
                if (beacon != null && owner.hasMarker) {
                    owner.marker.GoToPOI(beacon, p_arrivalActionBeforeDigging: OnArriveFollowingBeacon);

                    //Set to true after GoToPOI because ClearAllCurrentPathData is called there which will set this to false
                    //So to avoid setting to false, this is set to true after all process is done
                    isFollowingBeacon = true;
                    owner.movementComponent.UpdateSpeed();
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
                //Set to false first before clearing all current path data because UnfollowBeacon is also called there
                //So this is to avoid unending loop
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
            if (owner != beacon && owner.limiterComponent.canMove && owner.limiterComponent.canPerform && !owner.isDead) {
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
    public bool HasReachablePartymateToFleeTo() {
        LocationGridTile currentTileLocation = owner.gridTileLocation;
        if (isMemberThatJoinedQuest && currentTileLocation != null) {
            for (int i = 0; i < currentParty.membersThatJoinedQuest.Count; i++) {
                Character member = currentParty.membersThatJoinedQuest[i];
                LocationGridTile memberTileLocation = member.gridTileLocation;
                if (owner != member && member.limiterComponent.canPerform && member.limiterComponent.canMove && member.hasMarker && !member.isBeingSeized
                    && member.carryComponent.IsNotBeingCarried() && memberTileLocation != null) {
                    if (owner.movementComponent.HasPathToEvenIfDiffRegion(memberTileLocation)) {
                        float dist = currentTileLocation.GetDistanceTo(memberTileLocation);
                        if (dist <= 20f) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public bool HasPartymateInVision() {
        if (owner.hasMarker && isMemberThatJoinedQuest) {
            for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                Character inVision = owner.marker.inVisionCharacters[i];
                if (inVision.partyComponent.IsAMemberOfParty(owner.partyComponent.currentParty) && inVision.partyComponent.isMemberThatJoinedQuest) {
                    return true;
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