using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distraught : Emotion {

    public Distraught() : base(EMOTION.Distraught) {
        responses = new[] { "Distraught" };
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null, string reason = "") {
        if(target is Character targetCharacter) {
            //if (targetCharacter.IsConsideredInDangerBy(witness)) {
            //    if (witness.faction != null && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, targetCharacter)
            //        && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, targetCharacter)) {
            //        witness.faction.partyQuestBoard.CreateRescuePartyQuest(witness, witness.homeSettlement, targetCharacter);
            //    }
            //} else {
            //    witness.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
            //}
            witness.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
        }
        return base.ProcessEmotion(witness, target, status, goapNode, reason);
    }
    #endregion
}
