using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class MorningPatrolPartyQuest : PartyQuest {
    #region getters
    public override IPartyQuestTarget target => madeInLocation;
    public override System.Type serializedData => typeof(SaveDataMorningPatrolPartyQuest);
    public override bool workingStateImmediately => true;
    public override bool canStillJoinQuestAnytime => true;
    #endregion

    public MorningPatrolPartyQuest() : base(PARTY_QUEST_TYPE.Morning_Patrol) {
        minimumPartySize = 1;
        relatedBehaviour = typeof(MorningPatrolBehaviour);
    }
    public MorningPatrolPartyQuest(SaveDataMorningPatrolPartyQuest data) : base(data) {
    }

    #region Overrides
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if (toState == PARTY_STATE.Working) {
            SetIsSuccessful(true);
        }
    }
    public override IPartyTargetDestination GetTargetDestination() {
        return madeInLocation;
    }
    public override string GetPartyQuestTextInLog() {
        return "Morning Patrol";
    }
    #endregion
}

[System.Serializable]
public class SaveDataMorningPatrolPartyQuest : SaveDataPartyQuest {
}