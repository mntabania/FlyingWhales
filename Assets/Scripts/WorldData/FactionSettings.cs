using System.Collections.Generic;

public class FactionSettings {
    
    public List<FactionTemplate> factionSettings;
    public bool disableNewFactions;
    public bool disableFactionIdeologyChanges;
    
    public FactionSettings() {
        factionSettings = new List<FactionTemplate>();
    }
    public int GetCurrentTotalVillageCountBasedOnFactions() {
        int villageCount = 0;
        for (int i = 0; i < factionSettings.Count; i++) {
            villageCount += factionSettings[i].villageSettings.Count;
        }
        return villageCount;
    }
    public FactionTemplate AddFactionSetting(int p_villageCount) {
        FactionTemplate factionTemplate = new FactionTemplate(p_villageCount);
        AddFactionSetting(factionTemplate);
        return factionTemplate;
    }
    private void AddFactionSetting(FactionTemplate p_FactionTemplate) {
        factionSettings.Add(p_FactionTemplate);
    }
    public void RemoveFactionSetting(FactionTemplate p_FactionTemplate) {
        factionSettings.Remove(p_FactionTemplate);
    }
    public void ClearFactionSettings() {
        factionSettings.Clear();
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