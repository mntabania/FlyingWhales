﻿using System.Collections;
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
            if (targetCharacter.IsConsideredInDangerBy(witness)) {
                if (witness.faction != null && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, targetCharacter)
                    && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, targetCharacter)) {
                    witness.faction.partyQuestBoard.CreateRescuePartyQuest(witness, witness.homeSettlement, targetCharacter);
                }
                //if (witness.characterClass.IsCombatant()) {
                //    if (witness.homeSettlement != null && !witness.homeSettlement.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, targetCharacter)) {
                //        PartyManager.Instance.CreateRescuePartyQuest(witness.homeSettlement, targetCharacter);
                //    }
                    //Party activeRescueParty = witness.faction.GetActivePartywithTarget(PARTY_QUEST_TYPE.Rescue, targetCharacter);
                    //if (activeRescueParty != null && !activeRescueParty.isWaitTimeOver && !activeRescueParty.isDisbanded) {
                    //    activeRescueParty.AddMember(witness);
                    //} else {
                    //    witness.jobComponent.TriggerRescueJob(targetCharacter);
                    //}
                //} else {
                //    witness.interruptComponent.TriggerInterrupt(INTERRUPT.Cry_Request, targetCharacter, targetCharacter.name + " is in danger");
                //}
            } else {
                witness.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
            }
        }
        return base.ProcessEmotion(witness, target, status, goapNode, reason);
    }
    #endregion
}
