using System.Collections.Generic;

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
    
}