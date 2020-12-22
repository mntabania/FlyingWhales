using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class ZombieDeath : Interrupt {
        public ZombieDeath() : base(INTERRUPT.Zombie_Death) {
            duration = 3;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Death_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Player};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            interruptHolder.actor.Death("Zombie Virus", _deathLog: interruptHolder.effectLog, interrupt: this);
            return true;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
            reactions.Add(EMOTION.Shock);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend ||
                opinionLabel == RelationshipManager.Close_Friend) {
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