using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

[System.Serializable]
public class FactionSettings {
    
    public List<FactionTemplate> factionTemplates;
    public bool disableNewFactions;
    public bool disableFactionIdeologyChanges;
    
    public FactionSettings() {
        factionTemplates = new List<FactionTemplate>();
    }
    public int GetCurrentTotalVillageCountBasedOnFactions() {
        int villageCount = 0;
        for (int i = 0; i < factionTemplates.Count; i++) {
            villageCount += factionTemplates[i].villageSettings.Count;
        }
        return villageCount;
    }
    public FactionTemplate AddFactionSetting(int p_villageCount) {
        FactionTemplate factionTemplate = new FactionTemplate(p_villageCount);
        AddFactionSetting(factionTemplate);
        return factionTemplate;
    }
    private void AddFactionSetting(FactionTemplate p_FactionTemplate) {
        factionTemplates.Add(p_FactionTemplate);
    }
    public void RemoveFactionSetting(FactionTemplate p_FactionTemplate) {
        factionTemplates.Remove(p_FactionTemplate);
    }
    public void ClearFactionSettings() {
        factionTemplates.Clear();
    }
    public void AllowNewFactions() {
        disableNewFactions = false;
    }
    public void BlockNewFactions() {
        disableNewFactions = true;
    }
    public void AllowFactionIdeologyChanges() {
        disableFactionIdeologyChanges = false;
    }
    public void BlockFactionIdeologyChanges() {
        disableFactionIdeologyChanges = true;
    }
    public void FinalizeFactionTemplates() {
        int randomFactionTypeCount = 0;
        List<FACTION_TYPE> otherFactionTypes = new List<FACTION_TYPE>(GameUtilities.customWorldFactionTypeChoices);
        otherFactionTypes.Remove(FACTION_TYPE.Human_Empire);
        otherFactionTypes.Remove(FACTION_TYPE.Elven_Kingdom);
        for (int i = 0; i < factionTemplates.Count; i++) {
            FactionTemplate factionTemplate = factionTemplates[i];
            if (factionTemplate.factionTypeString == "Random") {
                if (randomFactionTypeCount == 0) {
                    factionTemplate.SetFactionType(GameUtilities.RollChance(50) ? FACTION_TYPE.Elven_Kingdom : FACTION_TYPE.Human_Empire);
                } else {
                    if (GameUtilities.RollChance(80)) {
                        factionTemplate.SetFactionType(GameUtilities.RollChance(50) ? FACTION_TYPE.Elven_Kingdom : FACTION_TYPE.Human_Empire);    
                    } else {
                        factionTemplate.SetFactionType(CollectionUtilities.GetRandomElement(otherFactionTypes));
                    }
                }
                randomFactionTypeCount++;
                Debug.Log($"{factionTemplate.name} random faction type set to {factionTemplate.factionType.ToString()}");
            }
        }
    }
    
}