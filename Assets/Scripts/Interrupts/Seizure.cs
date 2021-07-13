using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using UtilityScripts;
namespace Interrupts {
    public class Seizure : Interrupt {
        public Seizure() : base(INTERRUPT.Seizure) {
            interruptIconString = GoapActionStateDB.Injured_Icon;
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if (ChanceData.RollChance(CHANCE_TYPE.Plagued_Event_Seizure) && interruptHolder.actor.homeSettlement != null && 
                Locations.Settlements.Settlement_Events.PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement) &&
                !interruptHolder.actor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && interruptHolder.actor.homeSettlement.eventManager.CanHaveEvents()) {
                interruptHolder.actor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
            }
            return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            if (GameUtilities.RollChance(25) && witness.homeSettlement is NPCSettlement npcSettlement && npcSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && 
                !witness.relationshipContainer.IsFriendsWith(actor)) {
                witness.assumptionComponent.CreateAndReactToNewAssumption(actor, actor, INTERACTION_TYPE.IS_PLAGUED, REACTION_STATUS.WITNESSED);
            }
            
            return response;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            reactions.Add(EMOTION.Shock);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Concern);
            } else if ((witness.relationshipContainer.IsFamilyMember(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
                reactions.Add(EMOTION.Concern);
            } else if (opinionLabel != RelationshipManager.Enemy && opinionLabel != RelationshipManager.Rival) {
                reactions.Add(EMOTION.Concern);
            }
        }
        public override bool PerTickInterrupt(InterruptHolder interruptHolder) {
            interruptHolder.actor.needsComponent.AdjustTiredness(-2);
            return base.PerTickInterrupt(interruptHolder);
        }
        #endregion
    }
}