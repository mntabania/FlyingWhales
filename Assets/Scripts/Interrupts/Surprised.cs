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
        }

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