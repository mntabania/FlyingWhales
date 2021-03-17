﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
namespace Interrupts {
    public class DeclareRaid : Interrupt {
        public DeclareRaid() : base(INTERRUPT.Declare_Raid) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Hostile_Icon;
            logTags = new[] {LOG_TAG.Combat, LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            if (actor.faction != null && !actor.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Raid, actor.interruptComponent.raidTargetSettlement)) {
                actor.faction.partyQuestBoard.CreateRaidPartyQuest(actor, actor.homeSettlement, actor.interruptComponent.raidTargetSettlement);
                return true;
            }
            return false;
            //return interruptHolder.actor.faction.factionJobTriggerComponent.TriggerRaidJob(interruptHolder.actor.interruptComponent.raidTargetSettlement);
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
                Log effectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect", null, logTags);
                effectLog.AddToFillers(actor.faction, actor.faction.name, LOG_IDENTIFIER.FACTION_1);
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(actor.interruptComponent.raidTargetSettlement, actor.interruptComponent.raidTargetSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
                return effectLog;
            }
            return default;
        }
        #endregion
    }
}