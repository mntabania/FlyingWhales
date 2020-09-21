using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class FactionIdeologyComponent {
    public Faction owner { get; private set; }
    public List<FactionIdeology> currentIdeologies => owner.factionType.ideologies;

    public FactionIdeologyComponent(Faction owner) {
        this.owner = owner;
    }

    //public void SwitchToIdeology(FACTION_IDEOLOGY ideologyType) {
    //    if(currentIdeologies != null && currentIdeologies.ideologyType == FACTION_IDEOLOGY.INCLUSIVE && ideologyType == FACTION_IDEOLOGY.INCLUSIVE) { return; }
    //    currentIdeologies = CreateIdeology(ideologyType);
    //    currentIdeologies.SetRequirements(owner);
    //    ReEvaluateFactionMembers();
    //}
    // public void RerollIdeologies(bool willLog = true) {
    //     FACTION_IDEOLOGY[][] categorizedIdeologies = FactionManager.Instance.categorizedFactionIdeologies;
    //     for (int i = 0; i < currentIdeologies.Length; i++) {
    //         FactionIdeology ideology = currentIdeologies[i];
    //         FACTION_IDEOLOGY categorizedIdeology =
    //             categorizedIdeologies[i][Random.Range(0, categorizedIdeologies[i].Length)];
    //         ideology = FactionManager.Instance.CreateIdeology<FactionIdeology>(categorizedIdeology);
    //         ideology.SetRequirements(owner);
    //         currentIdeologies[i] = ideology;
    //     }
    //     ReEvaluateFactionMembers(willLog);
    // }
    // public void SetCurrentIdeology(int index, FactionIdeology ideology) {
    //     currentIdeologies[index] = ideology;
    // }
    public bool DoesCharacterFitCurrentIdeologies(Character character) {
        if(currentIdeologies == null) { return true; }
        for (int i = 0; i < currentIdeologies.Count; i++) {
            FactionIdeology ideology = currentIdeologies[i]; ;
            if(ideology != null && !ideology.DoesCharacterFitIdeology(character)) {
                return false;
            }
        }
        return true;
    }

    // private void ReEvaluateFactionMembers(bool willLog = true) {
    //     for (int i = 0; i < owner.characters.Count; i++) {
    //         Character member = owner.characters[i];
    //         if(member == owner.leader) { continue; }
    //         if (owner.CheckIfCharacterStillFitsIdeology(member, willLog)) {
    //             i--;
    //         }
    //     }
    // }

    public void OnLeaderBecameCultist(Character leader) {
        FactionIdeology currentInclusivityIdeology = null;
        for (int i = 0; i < owner.factionType.ideologies.Count; i++) {
            FactionIdeology ideology = owner.factionType.ideologies[i];
            if (ideology.ideologyType == FACTION_IDEOLOGY.Exclusive ||
                ideology.ideologyType == FACTION_IDEOLOGY.Inclusive) {
                currentInclusivityIdeology = ideology;
                break;
            }
        }
        owner.factionType.ClearIdeologies();

        //Set Peace-Type Ideology:
        if (leader.traitContainer.HasTrait("Hothead", "Treacherous", "Evil")) {
            Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
            owner.factionType.AddIdeology(warmonger);
        } else {
            Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
            owner.factionType.AddIdeology(peaceful);
        }

        //Set Inclusivity-Type Ideology:
        if (currentInclusivityIdeology != null) {
            owner.factionType.AddIdeology(currentInclusivityIdeology);
        } else {
            if (GameUtilities.RollChance(60)) {
                Inclusive inclusive = FactionManager.Instance.CreateIdeology<Inclusive>(FACTION_IDEOLOGY.Inclusive);
                owner.factionType.AddIdeology(inclusive);
            } else {
                Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
                if (GameUtilities.RollChance(60)) {
                    exclusive.SetRequirement(leader.race);
                } else {
                    exclusive.SetRequirement(leader.gender);
                }
                owner.factionType.AddIdeology(exclusive);
            }    
        }

        //Set Religion-Type Ideology:
        FactionRelationship relationshipToPlayerFaction = owner.GetRelationshipWith(PlayerManager.Instance.player.playerFaction);
        DemonWorship demonWorship = FactionManager.Instance.CreateIdeology<DemonWorship>(FACTION_IDEOLOGY.Demon_Worship);
        //If Demon Worshipper, friendly with player faction
        relationshipToPlayerFaction.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
        owner.factionType.AddIdeology(demonWorship);

        Log changeIdeologyLog = new Log(GameManager.Instance.Today(), "Faction", "Generic", "ideology_change", providedTags: LOG_TAG.Life_Changes);
        changeIdeologyLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.FACTION_1);
        changeIdeologyLog.AddLogToDatabase();

        Log changeRelationsLog = new Log(GameManager.Instance.Today(), "Faction", "Generic", "relation_change", providedTags: LOG_TAG.Life_Changes);
        changeRelationsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.FACTION_1);
        changeRelationsLog.AddLogToDatabase();
        
        Messenger.Broadcast(Signals.FACTION_IDEOLOGIES_CHANGED, owner);
    }
}