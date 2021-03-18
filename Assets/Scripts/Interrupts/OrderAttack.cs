﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
namespace Interrupts {
    public class OrderAttack : Interrupt {
        public OrderAttack() : base(INTERRUPT.Order_Attack) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Hostile_Icon;
            logTags = new[] {LOG_TAG.Combat, LOG_TAG.Work};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            for (int i = 0; i < interruptHolder.actor.faction.characters.Count; i++) {
                Character follower = interruptHolder.actor.faction.characters[i];
                if (follower.race == RACE.SKELETON && !follower.isDead && !follower.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour))) {
                    follower.behaviourComponent.SetAttackVillageTarget(interruptHolder.actor.necromancerTrait.attackVillageTarget);
                    follower.behaviourComponent.AddBehaviourComponent(typeof(AttackVillageBehaviour));
                }
            }
            return true;
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
                Log effectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect", null, logTags);
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(actor.necromancerTrait.attackVillageTarget, actor.necromancerTrait.attackVillageTarget.name, LOG_IDENTIFIER.LANDMARK_1);
                return effectLog;
            }
            return default;
        }
        #endregion
    }
}