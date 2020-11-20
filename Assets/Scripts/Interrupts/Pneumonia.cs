using UtilityScripts;
namespace Interrupts {
    public class Pneumonia : Interrupt {
        public Pneumonia() : base(INTERRUPT.Pneumonia) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Shock_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Player};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            if (GameUtilities.RollChance(15) && interruptHolder.actor.homeSettlement != null && 
                Locations.Settlements.Settlement_Events.PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(interruptHolder.actor.homeSettlement)) {
                interruptHolder.actor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
            }
            interruptHolder.actor.Death("Pneumonia", _deathLog: interruptHolder.effectLog, interrupt: this);
            return true;
        }
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
            
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if ((witness.relationshipContainer.IsFamilyMember(actor) || 
                        witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) && 
                       !witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Rival)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if (opinionLabel == RelationshipManager.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
            }

            if (status == REACTION_STATUS.WITNESSED && actor.homeSettlement != null && actor.homeSettlement is NPCSettlement settlement) {
                //When a resident has been witnessed to die due to Pneumonia, the Settlement will be flagged as Plagued
                settlement.SetIsPlagued(true);
            }
            
            return response;
        }
        #endregion
    }
}