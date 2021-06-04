using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class CryRequest : Interrupt {
        public CryRequest() : base(INTERRUPT.Cry_Request) {
            duration = 3;
            doesStopCurrentAction = true;
            //isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
            //isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Needs};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, interruptHolder.actor.marker.transform.position, 2, interruptHolder.actor.currentRegion.innerMap);
            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "cry", null, logTags);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(null, interruptHolder.identifier, LOG_IDENTIFIER.STRING_1);
            return true;
        }
        //public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        //    InterruptHolder interrupt, REACTION_STATUS status) {
        //    string response = base.ReactionToActor(actor, target, witness, interrupt, status);
        //    if (actor != target && witness != target && target is Character targetCharacter) {
        //        if (actor.relationshipContainer.GetAwarenessState(targetCharacter) == AWARENESS_STATE.Missing) {
        //            if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
        //                if (witness.faction != null && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, targetCharacter)
        //                     && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, targetCharacter)) {
        //                    if (targetCharacter.IsConsideredInDangerBy(witness)) {
        //                        witness.faction.partyQuestBoard.CreateRescuePartyQuest(witness, witness.homeSettlement, targetCharacter);
        //                    }
        //                }
        //                //witness.jobComponent.TriggerRescueJob(targetCharacter);
        //            }
        //        }
        //    }
        //    return response;
        //}
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        reactions.Add(EMOTION.Concern);
                    }
                }
            } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Concern);
                }
            } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    reactions.Add(EMOTION.Scorn);
                }
            }
        }
        #endregion
    }
}
