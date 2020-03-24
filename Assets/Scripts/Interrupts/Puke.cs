using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Puke : Interrupt {
        public Puke() : base(INTERRUPT.Puke) {
            duration = 2;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Sick_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.SetPOIState(POI_STATE.INACTIVE);
            return true;
        }
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.SetPOIState(POI_STATE.ACTIVE);
            return true;
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(witness, actor, target, interrupt, status);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status);
            return response;
        }
        #endregion
    }
}