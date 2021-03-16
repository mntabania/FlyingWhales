using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Taunted : Interrupt {
        public Taunted() : base(INTERRUPT.Taunted) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Anger_Icon;
            logTags = new[] {LOG_TAG.Combat};
            shouldAddLogs = false;
            shouldShowNotif = false;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character characterThatTaunt = interruptHolder.target as Character;
            Character taunted = interruptHolder.actor;
            bool isLethalCombat = taunted.combatComponent.GetCurrentTargetCombatLethality();

            taunted.traitContainer.AddTrait(taunted, "Taunted", characterResponsible: characterThatTaunt);
            taunted.combatComponent.ClearHostilesInRange(false);
            taunted.combatComponent.Fight(characterThatTaunt, CombatManager.Taunted, isLethal: isLethalCombat);
            return true;
        }
        #endregion
    }
}