using System.Collections;
using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;
using Locations.Settlements;
using Object_Pools;
namespace Interrupts {
    public class BecomeFactionLeader : Interrupt {
        public BecomeFactionLeader() : base(INTERRUPT.Become_Faction_Leader) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
            logTags = new[] {LOG_TAG.Major};
            shouldShowNotif = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            Character actor = interruptHolder.actor;

            actor.faction.SetLeader(actor);

            Faction faction = actor.faction;
            if (faction.isMajorNonPlayer) {
                //Do not clear ideologies when character becomes a leader of an existing faction
                //Instead it should only change the existing ones depending on the faction leader
                //Keep other ideologies that is not affected by rerolls
                //faction.factionType.ClearIdeologies();

                if(actor.characterClass.className == "Cult Leader") {
                    //Change faction type to Demon Cult
                    List<FactionIdeology> previousIdeologies = new List<FactionIdeology>(faction.factionType.ideologies);
                    faction.ChangeFactionType(FACTION_TYPE.Demon_Cult);

                    if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges) {
                        for (int i = 0; i < previousIdeologies.Count; i++) {
                            FactionIdeology ideology = previousIdeologies[i];
                            if (ideology.ideologyType.IsReligionType() || ideology.ideologyType.IsInclusivityType()) {
                                continue; //keep demon worship ideology
                            } else {
                                faction.factionType.AddIdeology(ideology.ideologyType);
                            }
                        }
                    }
                    
                    //Evaluate all character if they will stay or leave
                    for (int i = 0; i < faction.characters.Count; i++) {
                        Character member = faction.characters[i];
                        if (member != actor && !member.isDead) {
                            member.interruptComponent.TriggerInterrupt(INTERRUPT.Evaluate_Cultist_Affiliation, member);
                        }
                    }

                    //Transform all villages to Cult Towns
                    for (int i = 0; i < faction.ownedSettlements.Count; i++) {
                        BaseSettlement settlement = faction.ownedSettlements[i];
                        if (settlement is NPCSettlement village) {
                            if (village.settlementType == null || village.settlementType.settlementType != SETTLEMENT_TYPE.Cult_Town) {
                                village.SetSettlementType(SETTLEMENT_TYPE.Cult_Town);
                            }
                        }
                    }
                }
            }

            if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges) {
                //Set Peace-Type Ideology:
                FactionManager.Instance.RerollPeaceTypeIdeology(faction, actor);

                //Set Inclusivity-Type Ideology:
                FactionManager.Instance.RerollInclusiveTypeIdeology(faction, actor);
               
                //Set Religion-Type Ideology:
                FactionManager.Instance.RerollReligionTypeIdeology(faction, actor);

                //Set Faction Leader Trait Based Ideology:
                FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, actor);
                Messenger.Broadcast(FactionSignals.FACTION_IDEOLOGIES_CHANGED, faction);
            }

            //Validate crimes
            FactionManager.Instance.RevalidateFactionCrimes(faction, actor);
            
            Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, faction);

            if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges) {
                //create relationships
                //NOTE: Should not default relationships to neutral when leader changes, because we only want to overwrite relationships if other leader is friend/enemy 
                FactionManager.Instance.RerollFactionRelationships(faction, actor, false, true);
            }

            Log changeIdeologyLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "ideology_change", null, LOG_TAG.Life_Changes);
            changeIdeologyLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
            changeIdeologyLog.AddLogToDatabase(true);

            //check if faction characters still meets ideology requirements
            List<Character> charactersToCheck = RuinarchListPool<Character>.Claim();
            charactersToCheck.AddRange(faction.characters);
            charactersToCheck.Remove(actor);
            for (int i = 0; i < charactersToCheck.Count; i++) {
                Character factionMember = charactersToCheck[i];
                faction.CheckIfCharacterStillFitsIdeology(factionMember);
            }
            RuinarchListPool<Character>.Release(charactersToCheck);

            if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
            overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Become Faction Leader", "became_leader", null, LOG_TAG.Major);
            overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(actor.faction, actor.faction.name, LOG_IDENTIFIER.FACTION_1);
            return true;
        }
        #endregion
    }
}
