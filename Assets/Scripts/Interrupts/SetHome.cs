using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class SetHome : Interrupt {
        public SetHome() : base(INTERRUPT.Set_Home) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            if(actor.homeSettlement != null) {
                Settlement settlement = actor.homeSettlement;
                IDwelling chosenHomeStructure = null;
                if(target != actor) {
                    chosenHomeStructure = (target as Character).homeStructure;
                }
                actor.MigrateHomeStructureTo(null);
                settlement.AssignCharacterToDwellingInArea(actor, chosenHomeStructure);
                //if(actor.homeStructure != null) {
                //    Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Set Home", "set_new_home_structure");
                //    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //    log.AddToFillers(null, actor.homeStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.STRING_1);
                //    actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                //}
            }
            return true;
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Set Home", "set_new_home_structure");
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, actor.homeStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.STRING_1);
            return log;
        }
        #endregion
    }
}