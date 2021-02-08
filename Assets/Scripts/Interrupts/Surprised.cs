using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class Surprised : Interrupt {

        public Surprised() : base(INTERRUPT.Surprised) {
            duration = 3;
            doesStopCurrentAction = true;
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
        // public override bool ShouldAddLogs(InterruptHolder interruptHolder) {
        //     if (interruptHolder.identifier == "Shocked") {
        //         return false; //do not log surprised if surprised came from shock 
        //     }
        //     return base.ShouldAddLogs(interruptHolder);
        // }
        #endregion
        
        //#region Overrides
        //public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
        //    actor.Death("Septic Shock", interrupt: this);
        //    return true;
        //}
        //public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
        //    Interrupt interrupt, REACTION_STATUS status) {
        //    string response = base.ReactionToActor(witness, actor, target, interrupt, status);
        //    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
        //    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
        //    if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
        //        opinionLabel == RelationshipManager.Close_Friend) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
        //    } else if (opinionLabel == RelationshipManager.Rival) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
        //    }
        //    if (witness.traitContainer.HasTrait("Coward")) {
        //        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
        //    }
        //    return response;
        //}
        //#endregion
    }
}