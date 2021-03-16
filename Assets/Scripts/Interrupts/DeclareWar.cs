using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using Object_Pools;
namespace Interrupts {
    public class DeclareWar : Interrupt {
        public DeclareWar() : base(INTERRUPT.Declare_War) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Hostile_Icon;
            logTags = new[] {LOG_TAG.Major, LOG_TAG.Important};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;
            Character targetCharacter = interruptHolder.target as Character;
            if (actor.faction != null && targetCharacter != null) {
                Faction targetFaction = targetCharacter.faction;
                actor.faction.SetRelationshipFor(targetFaction, FACTION_RELATIONSHIP_STATUS.Hostile);
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", name, "effect", null, logTags);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(actor.faction, actor.faction.name, LOG_IDENTIFIER.FACTION_1);
                overrideEffectLog.AddToFillers(targetFaction, targetFaction.name, LOG_IDENTIFIER.FACTION_2);
            }
            return false;
        }
        #endregion
    }
}