using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class HeatstrokeDeath : Interrupt {
        public HeatstrokeDeath() : base(INTERRUPT.Heatstroke_Death) {
            interruptIconString = GoapActionStateDB.Death_Icon;
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.Death("Heatstroke", _deathLog: interruptHolder.effectLog, interrupt: this);
            return true;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
            reactions.Add(EMOTION.Shock);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Concern);
            } else if ((witness.relationshipContainer.IsFamilyMember(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
                reactions.Add(EMOTION.Concern);
            } else if (opinionLabel == RelationshipManager.Rival) {
                reactions.Add(EMOTION.Scorn);
            }
            if (status == REACTION_STATUS.WITNESSED) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    reactions.Add(EMOTION.Fear);
                }
            }
        }
        #endregion
    }
}