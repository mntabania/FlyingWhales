using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class CreateFaction : Interrupt {
        public CreateFaction() : base(INTERRUPT.Create_Faction) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {

            FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForRace(interruptHolder.actor.race);
            Faction faction = FactionManager.Instance.CreateNewFaction(factionType);
            
            //Set Peace-Type Ideology:
            FactionManager.Instance.RerollPeaceTypeIdeology(faction, interruptHolder.actor);
            
            //Set Inclusivity-Type Ideology:
            FactionManager.Instance.RerollInclusiveTypeIdeology(faction, interruptHolder.actor);
            
            //Set Religion-Type Ideology:
            FactionManager.Instance.RerollReligionTypeIdeology(faction, interruptHolder.actor);

            //Validate crimes
            FactionManager.Instance.RevalidateFactionCrimes(faction, interruptHolder.actor);
            
            interruptHolder.actor.ChangeFactionTo(faction);
            faction.SetLeader(interruptHolder.actor);
            
            //create relationships
            FactionManager.Instance.RerollFactionRelationships(faction, interruptHolder.actor, true);
            

            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Create Faction", "character_create_faction", null, LOG_TAG.Life_Changes);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
            overrideEffectLog.AddToFillers(interruptHolder.actor.currentRegion, interruptHolder.actor.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
            
            return true;
        }
        #endregion
    }
}
