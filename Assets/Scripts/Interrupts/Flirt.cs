using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Flirt : Interrupt {
        public Flirt() : base(INTERRUPT.Flirt) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
            isIntel = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.nonActionEventsComponent.NormalFlirtCharacter(target as Character, ref overrideEffectLog);
            return true;
        }
        public override string ReactionToActor(Character witness, Character actor, IPointOfInterest target,
            Interrupt interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(witness, actor, target, interrupt, status);
            if(target != witness) {
                bool isLoverOrAffair = witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
                if (isLoverOrAffair) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                } else {
                    //if (witness.relationshipContainer.IsFriendsWith(actor)) {

                    //}
                    //TODO
                }
            }
            return response;
        }
        #endregion
    }
}