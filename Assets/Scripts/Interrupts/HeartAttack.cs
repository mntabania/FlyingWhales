using UtilityScripts;
using System.Collections.Generic;

namespace Interrupts {
    public class HeartAttack : Interrupt {
        public HeartAttack() : base(INTERRUPT.Heart_Attack) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Death_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Player};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if (interruptHolder.identifier != "plague" && !string.IsNullOrEmpty(interruptHolder.identifier)) {
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect_with_reason", null, logTags);
                overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(null, interruptHolder.identifier, LOG_IDENTIFIER.STRING_1);
            }
            return false;
        }
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            bool isPlayerSource = false;
            if (interruptHolder.identifier == "plague") {
                isPlayerSource = true;
                if (ChanceData.RollChance(CHANCE_TYPE.Plagued_Event_Heart_Attck) && interruptHolder.actor.homeSettlement != null &&
                    Locations.Settlements.Settlement_Events.PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement) &&
                    !interruptHolder.actor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && interruptHolder.actor.homeSettlement.eventManager.CanHaveEvents()) {
                    interruptHolder.actor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
                }
            }
            interruptHolder.actor.Death("Heart Attack", _deathLog: interruptHolder.effectLog, interrupt: this, isPlayerSource: isPlayerSource);
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);

            if (status == REACTION_STATUS.WITNESSED && actor.homeSettlement != null && actor.homeSettlement is NPCSettlement settlement && interrupt.identifier == "plague") {
                //When a resident has been witnessed to die due to Heart Attack, the Settlement will be flagged as Plagued
                settlement.SetIsPlagued(true);
            }
            
            return response;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            reactions.Add(EMOTION.Shock);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Concern);
            } else if ((witness.relationshipContainer.IsFamilyMember(actor) ||
                        witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) &&
                       !witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Rival)) {
                reactions.Add(EMOTION.Concern);
            } else if (opinionLabel == RelationshipManager.Rival) {
                reactions.Add(EMOTION.Scorn);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            }
        }
        #endregion
    }
}