using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class CreateFaction : Interrupt {
        public CreateFaction() : base(INTERRUPT.Create_Faction) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Major};
            shouldShowNotif = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {

            FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForCharacter(interruptHolder.actor);
            Faction faction = FactionManager.Instance.CreateNewFaction(factionType, race: interruptHolder.actor.race);

            //Sets the default data of faction type
            faction.factionType.SetAsDefault();
            
            //Set Peace-Type Ideology:
            FactionManager.Instance.RerollPeaceTypeIdeology(faction, interruptHolder.actor);
            
            //Set Inclusivity-Type Ideology:
            FactionManager.Instance.RerollInclusiveTypeIdeology(faction, interruptHolder.actor);
            
            //Set Religion-Type Ideology:
            FactionManager.Instance.RerollReligionTypeIdeology(faction, interruptHolder.actor);

            //Set Faction Leader Trait Based Ideology:
            FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, interruptHolder.actor);

            //Validate crimes
            FactionManager.Instance.RevalidateFactionCrimes(faction, interruptHolder.actor);
            
            if(!string.IsNullOrEmpty(interruptHolder.identifier) && interruptHolder.identifier == "own_settlement" && interruptHolder.actor.homeSettlement != null) {
                LandmarkManager.Instance.OwnSettlement(faction, interruptHolder.actor.homeSettlement);
            }

            interruptHolder.actor.ChangeFactionTo(faction, true);
            faction.SetLeader(interruptHolder.actor);
            
            //create relationships
            FactionManager.Instance.RerollFactionRelationships(faction, interruptHolder.actor, true, false);
            
            Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, faction);

            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Create Faction", "character_create_faction", null, LOG_TAG.Life_Changes);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
            overrideEffectLog.AddToFillers(interruptHolder.actor.currentRegion, interruptHolder.actor.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);

            Messenger.Broadcast(FactionSignals.CREATE_FACTION_INTERRUPT, faction, interruptHolder.actor);
            return true;
        }
        #endregion
    }
}
