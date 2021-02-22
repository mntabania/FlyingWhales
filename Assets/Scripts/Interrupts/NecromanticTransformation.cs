﻿using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

namespace Interrupts {
    public class NecromanticTransformation : Interrupt {
        public NecromanticTransformation() : base(INTERRUPT.Necromantic_Transformation) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Major};
            shouldShowNotif = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            actor.AssignClass("Necromancer");
            actor.ChangeFactionTo(FactionManager.Instance.undeadFaction);
            FactionManager.Instance.undeadFaction.OnlySetLeader(actor); //TODO: needed to call this even though Become Faction Leader is called because it calls a version of set leader that prevents setting the leader of The Undead Faction
            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, actor);
            CharacterManager.Instance.SetNecromancerInTheWorld(actor);
            actor.MigrateHomeStructureTo(null);
            actor.ClearTerritory();
            return true;
        }
        public override Log CreateEffectLog(Character actor, IPointOfInterest target) {
            if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", name, "effect")) {
                string adjective = "treacherous";
                if (actor.traitContainer.HasTrait("Evil")) {
                    adjective = "evil";
                }
                Log effectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect", null, logTags);
                effectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                effectLog.AddToFillers(null, adjective, LOG_IDENTIFIER.STRING_1);
                return effectLog;
            }
            return default;
        }
        #endregion
    }
}