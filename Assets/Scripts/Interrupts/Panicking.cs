using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class Panicking : Interrupt {

        public Panicking() : base(INTERRUPT.Panicking) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Shock_Icon;
            shouldShowNotif = false;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            Log effectLog = base.CreateEffectLog(actor, target);
            if (effectLog != null && actor.interruptComponent.currentInterrupt != null) {
                effectLog.AddToFillers(null, actor.interruptComponent.currentInterrupt.reason, LOG_IDENTIFIER.STRING_1);
                return effectLog;
            }
            return default;
        }
        public override void AddAdditionalFillersToThoughtLog(Log log, Character actor) {
            base.AddAdditionalFillersToThoughtLog(log, actor);
            if (log != null && actor.interruptComponent.currentInterrupt != null) {
                log.AddToFillers(null, actor.interruptComponent.currentInterrupt.reason, LOG_IDENTIFIER.STRING_1);
            }
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);

            reactions.Add(EMOTION.Shock);
            if (witness.relationshipContainer.IsFriendsWith(actor)) {
                reactions.Add(EMOTION.Concern);
            } else if ((witness.relationshipContainer.IsFamilyMember(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR)) &&
                  !witness.relationshipContainer.IsEnemiesWith(actor)) {
                reactions.Add(EMOTION.Concern);
            } else if (opinionLabel == RelationshipManager.Acquaintance) {
                reactions.Add(EMOTION.Concern);
            }
        }
        #endregion

    }
}