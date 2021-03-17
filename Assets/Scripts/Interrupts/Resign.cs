using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;
using Locations.Settlements;
using Object_Pools;
namespace Interrupts {
    public class Resign : Interrupt {
        public Resign() : base(INTERRUPT.Resign) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Major};
            shouldShowNotif = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            Faction faction = actor.faction;
            NPCSettlement settlement = actor.homeSettlement;
            if (actor.isFactionLeader && actor.isSettlementRuler) {
                faction.SetLeader(null);
                settlement.SetRuler(null);
                //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "resign_both", null, LOG_TAG.Major);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                overrideEffectLog.AddToFillers(settlement, settlement.name, LOG_IDENTIFIER.LANDMARK_1);
            } else {
                if (actor.isFactionLeader) {
                    faction.SetLeader(null);
                    //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                    overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "resign_faction_leader", null, LOG_TAG.Major);
                    overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                } else if (actor.isSettlementRuler) {
                    settlement.SetRuler(null);
                    //if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                    overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "resign_ruler", null, LOG_TAG.Major);
                    overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    overrideEffectLog.AddToFillers(settlement, settlement.name, LOG_IDENTIFIER.LANDMARK_1);
                }
            }
            return true;
        }
        #endregion
    }
}
