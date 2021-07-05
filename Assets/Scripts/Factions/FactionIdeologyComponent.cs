using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public class FactionIdeologyComponent : FactionComponent {
    public List<FactionIdeology> currentIdeologies => owner.factionType.ideologies;

    public FactionIdeologyComponent() { }
    public FactionIdeologyComponent(SaveDataFactionIdeologyComponent data) { }
    
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
    public bool DoesCharacterFitCurrentIdeologies(PreCharacterData character) {
        if(currentIdeologies == null) { return true; }
        for (int i = 0; i < currentIdeologies.Count; i++) {
            FactionIdeology ideology = currentIdeologies[i]; ;
            if(ideology != null && !ideology.DoesCharacterFitIdeology(character)) {
                return false;
            }
        }
        return true;
    }

    public void OnLeaderBecameCultist(Character leader) {
        if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges) {
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
            FactionManager.Instance.RerollPeaceTypeIdeology(owner, leader);

            //Set Inclusivity-Type Ideology:
            if (currentInclusivityIdeology != null) {
                owner.factionType.AddIdeology(currentInclusivityIdeology);
            } else {
                FactionManager.Instance.RerollInclusiveTypeIdeology(owner, leader);
            }

            //Set Religion-Type Ideology:
            FactionManager.Instance.RerollReligionTypeIdeology(owner, leader);
        }
        
        //Validate crimes
        FactionManager.Instance.RevalidateFactionCrimes(owner, leader);
        
        //If Demon Worshipper, friendly with player faction
        FactionRelationship relationshipToPlayerFaction = owner.GetRelationshipWith(PlayerManager.Instance.player.playerFaction);
        relationshipToPlayerFaction.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Friendly);
        

        Log changeIdeologyLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "ideology_change", providedTags: LOG_TAG.Life_Changes);
        changeIdeologyLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.FACTION_1);
        changeIdeologyLog.AddLogToDatabase(true);

        Log changeRelationsLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Faction", "Generic", "relation_change", providedTags: LOG_TAG.Life_Changes);
        changeRelationsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.FACTION_1);
        changeRelationsLog.AddLogToDatabase(true);
        
        Messenger.Broadcast(FactionSignals.FACTION_IDEOLOGIES_CHANGED, owner);
        Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, owner);
    }
}

[System.Serializable]
public class SaveDataFactionIdeologyComponent : SaveData<FactionIdeologyComponent> {
    //No unique data as of now

    #region Overrides
    public override void Save(FactionIdeologyComponent data) {
    }

    public override FactionIdeologyComponent Load() {
        FactionIdeologyComponent component = new FactionIdeologyComponent(this);
        return component;
    }
    #endregion
}