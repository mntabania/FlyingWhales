using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using UtilityScripts;
namespace Interrupts {
    public class Accident : Interrupt {
        public Accident() : base(INTERRUPT.Accident) {
            duration = 1;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            isIntel = true;
            interruptIconString = GoapActionStateDB.Injured_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            if(interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Injured")) {
                return true;
            }
            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);

            if ((witness.relationshipContainer.IsFamilyMember(actor) ||
                 witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) &&
                !witness.relationshipContainer.IsEnemiesWith(actor)) {
                reactions.Add(EMOTION.Concern);
            } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Concern);
            } else if (opinionLabel == RelationshipManager.Acquaintance) {
                if (GameUtilities.RollChance(50)) {
                    reactions.Add(EMOTION.Concern);
                }
            } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                reactions.Add(EMOTION.Scorn);
            }
        }
        #endregion
    }
}

