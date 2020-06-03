﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Interrupts {
    public class RecallAttack : Interrupt {
        public RecallAttack() : base(INTERRUPT.Recall_Attack) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Magic_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            for (int i = 0; i < actor.faction.characters.Count; i++) {
                Character follower = actor.faction.characters[i];
                if (follower.race == RACE.SKELETON && !follower.isDead) {
                    follower.behaviourComponent.SetAttackVillageTarget(null);
                    follower.behaviourComponent.RemoveBehaviourComponent(typeof(AttackVillageBehaviour));
                }
            }
            return true;
        }
        //public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
        //    if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
        //        Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "effect");
        //        effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        effectLog.AddToFillers(actor.necromancerTrait.attackVillageTarget, actor.necromancerTrait.attackVillageTarget.name, LOG_IDENTIFIER.LANDMARK_1);
        //        return effectLog;
        //    }
        //    return null;
        //}
        #endregion
    }
}