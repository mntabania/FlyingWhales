using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class CreateFaction : Interrupt {
        public CreateFaction() : base(INTERRUPT.Create_Faction) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            Faction newFaction = FactionManager.Instance.CreateNewFaction(actor.race);
            actor.ChangeFactionTo(newFaction);
            newFaction.SetLeader(actor);

            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Create Faction", "character_create_faction");
            overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
            overrideEffectLog.AddToFillers(actor.currentRegion, actor.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
            //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(createFactionLog, onlyClickedCharacter: false);
            return true;
        }
        #endregion
    }
}
