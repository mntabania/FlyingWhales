﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class ReduceConflict : Interrupt {
        public ReduceConflict() : base(INTERRUPT.Reduce_Conflict) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Social};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character targetCharacter = interruptHolder.target as Character;
            List<Character> enemyOrRivalCharacters = targetCharacter.relationshipContainer.GetEnemyCharacters();
            if(enemyOrRivalCharacters.Count > 0) {
                Character chosenEnemyOrRival = enemyOrRivalCharacters[UnityEngine.Random.Range(0, enemyOrRivalCharacters.Count)];
                string logKey = "reduce_conflict";
                if (UnityEngine.Random.Range(0, 2) == 0 && chosenEnemyOrRival.traitContainer.HasTrait("Hothead")) {
                    logKey = "reduce_conflict_rebuffed";
                } else {
                    targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, chosenEnemyOrRival, "Base", 15);
                }
                if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Reduce Conflict", logKey, null, logTags);
                overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                overrideEffectLog.AddToFillers(chosenEnemyOrRival, chosenEnemyOrRival.name, LOG_IDENTIFIER.CHARACTER_3);
                //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            }
            return true;
        }
        #endregion
    }
}