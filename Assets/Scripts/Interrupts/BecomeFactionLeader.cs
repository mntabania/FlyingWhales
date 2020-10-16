using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;

namespace Interrupts {
    public class BecomeFactionLeader : Interrupt {
        public BecomeFactionLeader() : base(INTERRUPT.Become_Faction_Leader) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Life_Changes};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            interruptHolder.actor.faction.SetLeader(interruptHolder.actor);

            Faction faction = interruptHolder.actor.faction;
            if (faction.isMajorNonPlayer) {
                faction.factionType.ClearIdeologies();

                //Set Peace-Type Ideology:
                FactionManager.Instance.RerollPeaceTypeIdeology(faction, interruptHolder.actor);

                //Set Inclusivity-Type Ideology:
               FactionManager.Instance.RerollInclusiveTypeIdeology(faction, interruptHolder.actor);
               
                //Set Religion-Type Ideology:
                FactionManager.Instance.RerollReligionTypeIdeology(faction, interruptHolder.actor);

                //Set Faction Leader Trait Based Ideology:
                FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, interruptHolder.actor);

                Messenger.Broadcast(Signals.FACTION_IDEOLOGIES_CHANGED, faction);
                Messenger.Broadcast(Signals.FACTION_CRIMES_CHANGED, faction);

                //create relationships
                //NOTE: Should not default relationships to neutral when leader changes, because we only want to overwrite relationships if other leader is friend/enemy 
                FactionManager.Instance.RerollFactionRelationships(faction, interruptHolder.actor, false, OnFactionRelationshipSet);

                Log changeIdeologyLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "ideology_change", null, LOG_TAG.Life_Changes);
                changeIdeologyLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                changeIdeologyLog.AddLogToDatabase();

                
                //check if faction characters still meets ideology requirements
                List<Character> charactersToCheck = new List<Character>(faction.characters);
                charactersToCheck.Remove(interruptHolder.actor);
                for (int i = 0; i < charactersToCheck.Count; i++) {
                    Character factionMember = charactersToCheck[i];
                    faction.CheckIfCharacterStillFitsIdeology(factionMember);
                }
            }

            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Become Faction Leader", "became_leader", null, LOG_TAG.Life_Changes);
            overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(interruptHolder.actor.faction, interruptHolder.actor.faction.name, LOG_IDENTIFIER.FACTION_1);
            return true;
        }
        #endregion

        private void OnFactionRelationshipSet(FACTION_RELATIONSHIP_STATUS status, Faction faction, Faction otherFaction) {
            if (!otherFaction.isPlayerFaction) {
                //If this one's Faction Leader considers that an Enemy or Rival, war with that faction
                if (status == FACTION_RELATIONSHIP_STATUS.Hostile) {
                    Log dislikeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "dislike_leader", null, LOG_TAG.Life_Changes);
                    dislikeLog.AddToFillers(faction.leader as Character, faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    dislikeLog.AddToFillers(otherFaction.leader as Character, otherFaction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "declare_war", null, LOG_TAG.Life_Changes);
                    log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                    log.AddToFillers(otherFaction, otherFaction.name, LOG_IDENTIFIER.FACTION_2);
                    log.AddToFillers(dislikeLog.fillers);
                    log.AddToFillers(null, dislikeLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                    log.AddLogToDatabase();    
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                } else if (status == FACTION_RELATIONSHIP_STATUS.Friendly) {
                    //If this one's Faction Leader considers that a Friend or Close Friend, friendly with that faction
                    Log likeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "like_leader", null, LOG_TAG.Life_Changes);
                    likeLog.AddToFillers(faction.leader as Character, faction.leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    likeLog.AddToFillers(otherFaction.leader as Character, otherFaction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "declare_peace", null, LOG_TAG.Life_Changes);
                    log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
                    log.AddToFillers(otherFaction, otherFaction.name, LOG_IDENTIFIER.FACTION_2);
                    log.AddToFillers(likeLog.fillers);
                    log.AddToFillers(null, likeLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                    log.AddLogToDatabase();
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                }    
            }
            
        }
    }
}
