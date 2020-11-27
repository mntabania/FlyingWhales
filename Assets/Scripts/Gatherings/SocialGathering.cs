using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SocialGathering : Gathering {

    public LocationStructure targetStructure { get; private set; }

    #region getters
    public override IGatheringTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataSocialGathering);
    #endregion

    public SocialGathering() : base(GATHERING_TYPE.Social) {
        minimumGatheringSize = 5;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(4);
        relatedBehaviour = typeof(SocialGatheringBehaviour);
        jobQueueOwnerType = JOB_OWNER.SETTLEMENT;
    }
    public SocialGathering(SaveDataSocialGathering data) : base(data) {
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        if (UnityEngine.Random.Range(0, 2) == 0) {
            return !character.relationshipContainer.IsEnemiesWith(host) && !character.traitContainer.HasTrait("Agoraphobic") && character.limiterComponent.isSociable;
        }
        return false;
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        DisbandGathering();
    }
    //protected override void OnAddMember(Character member) {
    //    base.OnAddMember(member);
    //    member.movementComponent.SetEnableDigging(true);
    //}
    protected override void OnRemoveAttendee(Character member) {
        base.OnRemoveAttendee(member);
        if (attendees.Count > 0) {
            bool stillHasMemberInTargetStructure = false;
            for (int i = 0; i < attendees.Count; i++) {
                if (attendees[i].currentStructure == targetStructure) {
                    stillHasMemberInTargetStructure = true;
                    break;
                }
            }
            if (!stillHasMemberInTargetStructure) {
                DisbandGathering();
            }
        }
    }
    protected override void OnDisbandGathering() {
        base.OnDisbandGathering();
        targetStructure?.SetHasActiveSocialGathering(false);
    }
    #endregion

    #region General
    public void SetTargetStructure(LocationStructure structure) {
        if (targetStructure != structure) {
            targetStructure = structure;
            if (targetStructure != null) {
                targetStructure.SetHasActiveSocialGathering(true);
            }
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataGathering data) {
        base.LoadReferences(data);
        if (data is SaveDataSocialGathering subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataSocialGathering : SaveDataGathering {
    public string targetStructure;

    #region Overrides
    public override void Save(Gathering data) {
        base.Save(data);
        if (data is SocialGathering subData) {
            if (subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }
        }
    }
    #endregion
}