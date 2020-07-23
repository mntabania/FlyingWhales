using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Interrupts {
    public class DeclareRaid : Interrupt {
        public DeclareRaid() : base(INTERRUPT.Declare_Raid) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Hostile_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            return interruptHolder.actor.faction.factionJobTriggerComponent.TriggerRaidJob(interruptHolder.actor.interruptComponent.raidTargetStructure);
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
                Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", name, "effect");
                effectLog.AddToFillers(actor.faction, actor.faction.name, LOG_IDENTIFIER.FACTION_1);
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(actor.interruptComponent.raidTargetStructure.settlementLocation, actor.interruptComponent.raidTargetStructure.settlementLocation.name, LOG_IDENTIFIER.LANDMARK_1);
                return effectLog;
            }
            return null;
        }
        #endregion
    }
}