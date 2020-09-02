using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SocialParty : Party {

    public LocationStructure targetStructure { get; private set; }

    #region getters
    public override IPartyTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataSocialParty);
    #endregion

    public SocialParty() : base(PARTY_TYPE.Social) {
        minimumPartySize = 5;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(4);
        relatedBehaviour = typeof(SocialPartyBehaviour);
        jobQueueOwnerType = JOB_OWNER.SETTLEMENT;
    }
    public SocialParty(SaveDataParty data) : base(data) {
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        if(UnityEngine.Random.Range(0, 2) == 0) {
            return !character.relationshipContainer.IsEnemiesWith(leader) && !character.traitContainer.HasTrait("Agoraphobic") && character.isSociable;
        }
        return false;
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        DisbandParty();
    }
    //protected override void OnAddMember(Character member) {
    //    base.OnAddMember(member);
    //    member.movementComponent.SetEnableDigging(true);
    //}
    protected override void OnRemoveMember(Character member) {
        base.OnRemoveMember(member);
        if (members.Count > 0) {
            bool stillHasMemberInTargetStructure = false;
            for (int i = 0; i < members.Count; i++) {
                if (members[i].currentStructure == targetStructure) {
                    stillHasMemberInTargetStructure = true;
                    break;
                }
            }
            if (!stillHasMemberInTargetStructure) {
                DisbandParty();
            }
        }
    }
    protected override void OnDisbandParty() {
        base.OnDisbandParty();
        targetStructure?.SetHasActiveSocialParty(false);
    }
    #endregion

    #region General
    public void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
            if(targetStructure != null) {
                targetStructure.SetHasActiveSocialParty(true);
            }
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataParty data) {
        base.LoadReferences(data);
        if (data is SaveDataSocialParty subData) {
            if (subData.targetStructure != string.Empty) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataSocialParty : SaveDataParty {
    public string targetStructure;

    #region Overrides
    public override void Save(Party data) {
        base.Save(data);
        if (data is SocialParty subData) {
            if (subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }
        }
    }
    #endregion
}