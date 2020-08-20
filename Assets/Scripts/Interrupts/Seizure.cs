using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class Seizure : Interrupt {
        public Seizure() : base(INTERRUPT.Seizure) {
            interruptIconString = GoapActionStateDB.Injured_Icon;
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        #region Overrides
        //public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
        //    interruptHolder.actor.Death("Heatstroke", interrupt: this, _deathLog: interruptHolder.effectLog);
        //    return true;
        //}
        public override string ReactionToActor(Character actor, IPointOfInterest target,
            Character witness,
            InterruptHolder interrupt, REACTION_STATUS status) {
            string response = base.ReactionToActor(actor, target, witness, interrupt, status);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if ((witness.relationshipContainer.IsFamilyMember(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            } else if (opinionLabel != RelationshipManager.Enemy && opinionLabel != RelationshipManager.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status);
            }
            return response;
        }
        #endregion
    }
}