using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Interrupts {
    public class CreateFaction : Interrupt {
        public CreateFaction() : base(INTERRUPT.Create_Faction) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.No_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {

            FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForRace(actor.race);
            Faction newFaction = FactionManager.Instance.CreateNewFaction(factionType);
            
            //Set Peace-Type Ideology:
            if (actor.traitContainer.HasTrait("Hothead", "Treacherous", "Evil")) {
                Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
                newFaction.factionType.AddIdeology(warmonger);
            } else {
                Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
                newFaction.factionType.AddIdeology(peaceful);
            }
            
            //Set Inclusivity-Type Ideology:
            if (GameUtilities.RollChance(60)) {
                Inclusive inclusive = FactionManager.Instance.CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
                newFaction.factionType.AddIdeology(inclusive);
            } else {
                Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
                if (GameUtilities.RollChance(60)) {
                    exclusive.SetRequirement(actor.race);
                } else {
                    exclusive.SetRequirement(actor.gender);
                }
                newFaction.factionType.AddIdeology(exclusive);
            }
            
            //Set Religion-Type Ideology:
            if (actor.traitContainer.HasTrait("Cultist")) {
                DemonWorship inclusive = FactionManager.Instance.CreateIdeology<DemonWorship>(FACTION_IDEOLOGY.Demon_Worship);
                newFaction.factionType.AddIdeology(inclusive);
            } else if (actor.race == RACE.ELVES) {
                NatureWorship natureWorship = FactionManager.Instance.CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship);
                newFaction.factionType.AddIdeology(natureWorship);
            } else if (actor.race == RACE.HUMANS) {
                DivineWorship divineWorship = FactionManager.Instance.CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
                newFaction.factionType.AddIdeology(divineWorship);
            }
            
            actor.ChangeFactionTo(newFaction);
            newFaction.SetLeader(actor);
            
            //create relationships
            for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
                Faction otherFaction = FactionManager.Instance.allFactions[i];
                if(otherFaction.id != newFaction.id) {
                    FactionRelationship factionRelationship = newFaction.GetRelationshipWith(otherFaction);
                    if (otherFaction.isPlayerFaction) {
                        //If Demon Worshipper, friendly with player faction
                        factionRelationship.SetRelationshipStatus(
                            newFaction.factionType.HasIdeology(FACTION_IDEOLOGY.Demon_Worship)
                                ? FACTION_RELATIONSHIP_STATUS.Friendly
                                : FACTION_RELATIONSHIP_STATUS.Hostile);
                    } else if (otherFaction.leader != null && otherFaction.leader is Character otherFactionLeader){
                        //Check each Faction Leader of other existing factions if available:
                        if (actor.relationshipContainer.IsEnemiesWith(otherFactionLeader)) {
                            //If this one's Faction Leader considers that an Enemy or Rival, war with that faction
                            factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
                        } else if (actor.relationshipContainer.IsFriendsWith(otherFactionLeader)) {
                            //If this one's Faction Leader considers that a Friend or Close Friend, friendly with that faction
                            factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
                        } else {
                            //The rest should be set as neutral
                            factionRelationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Neutral);
                        }
                    }
                }
            }
            
            overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Create Faction", "character_create_faction");
            overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideEffectLog.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
            overrideEffectLog.AddToFillers(actor.currentRegion, actor.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
            
            return true;
        }
        #endregion
    }
}
