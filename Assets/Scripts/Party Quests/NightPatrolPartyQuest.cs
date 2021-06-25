using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class NightPatrolPartyQuest : PartyQuest {
    #region getters
    public override IPartyQuestTarget target => madeInLocation;
    public override System.Type serializedData => typeof(SaveDataNightPatrolPartyQuest);
    public override bool workingStateImmediately => true;
    public override bool canStillJoinQuestAnytime => true;
    #endregion

    public NightPatrolPartyQuest() : base(PARTY_QUEST_TYPE.Night_Patrol) {
        minimumPartySize = 1;
        relatedBehaviour = typeof(NightPatrolBehaviour);
    }
    public NightPatrolPartyQuest(SaveDataNightPatrolPartyQuest data) : base(data) {
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
        return "Night Patrol";
    }
    #endregion
}

[System.Serializable]
public class SaveDataNightPatrolPartyQuest : SaveDataPartyQuest {
}