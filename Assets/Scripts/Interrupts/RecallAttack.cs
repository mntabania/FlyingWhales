using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
namespace Interrupts {
    public class RecallAttack : Interrupt {
        public RecallAttack() : base(INTERRUPT.Recall_Attack) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Magic_Icon;
            logTags = new[] {LOG_TAG.Combat, LOG_TAG.Work};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            Log overrideEffectLog, ActualGoapNode goapNode = null) {
            for (int i = 0; i < interruptHolder.actor.faction.characters.Count; i++) {
                Character follower = interruptHolder.actor.faction.characters[i];
                if (follower.race == RACE.SKELETON && !follower.isDead) {
                    follower.behaviourComponent.SetAttackVillageTarget(null);
                    follower.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
                }
            }
            return true;
        }
        //public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
        //    if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
        //        Log effectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect");
        //        effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        effectLog.AddToFillers(actor.necromancerTrait.attackVillageTarget, actor.necromancerTrait.attackVillageTarget.name, LOG_IDENTIFIER.LANDMARK_1);
        //        return effectLog;
        //    }
        //    return null;
        //}
        #endregion
    }
}