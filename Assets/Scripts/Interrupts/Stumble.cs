using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Stumble : Interrupt {
        public Stumble() : base(INTERRUPT.Stumble) {
            duration = 2;
            doesStopCurrentAction = true;
            interruptIconString = GoapActionStateDB.Injured_Icon;
            isIntel = true;
            logTags = new[] {LOG_TAG.Work};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            int randomHpToLose = UnityEngine.Random.Range(1, 6);
            float percentMaxHPToLose = randomHpToLose / 100f;
            int actualHPToLose = Mathf.CeilToInt(interruptHolder.actor.maxHP * percentMaxHPToLose);
#if DEBUG_LOG
            Debug.Log(
                $"Stumble of {interruptHolder.actor.name} percent: {percentMaxHPToLose}, max hp: {interruptHolder.actor.maxHP}, lost hp: {actualHPToLose}");
#endif
            interruptHolder.actor.AdjustHP(-actualHPToLose, ELEMENTAL_TYPE.Normal, showHPBar: true);
            if (!interruptHolder.actor.HasHealth()) {
                interruptHolder.actor.Death("Stumble");
            }
            return true;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);
            if (witness.relationshipContainer.IsFriendsWith(actor)) {
                reactions.Add(EMOTION.Concern);
            } else if (witness.relationshipContainer.IsEnemiesWith(actor)) {
                reactions.Add(EMOTION.Scorn);
            }
        }
#endregion
    }
}