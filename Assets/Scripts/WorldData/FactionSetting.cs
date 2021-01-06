using System.Collections.Generic;
using UnityEngine;

public class FactionSetting {
    public string name { get; private set; }
    public FACTION_TYPE factionType { get; private set; }
    public Sprite factionEmblem { get; private set; }
    public List<VillageSetting> villageSettings { get; private set; }

    public FactionSetting(int p_villageCount) {
        name = RandomNameGenerator.GenerateFactionName();
        factionType = FACTION_TYPE.Human_Empire;
        villageSettings = new List<VillageSetting>();
        for (int i = 0; i < p_villageCount; i++) {
            villageSettings.Add(VillageSetting.Default);
        }
    }
    public void AddVillageSetting(VillageSetting p_villageSetting) {
        villageSettings.Add(p_villageSetting);
    }
    public void SetFactionEmblem(Sprite p_emblem) {
        factionEmblem = p_emblem;
    }
    public void ChangeFactionType(FACTION_TYPE p_factionType) {
        Debug.Log($"Changed faction type of {name} to {p_factionType}");
        factionType = p_factionType;
    }
    public void ChangeName(string p_newName) {
        Debug.Log($"Changed name of {name} to {p_newName}");
        name = p_newName;
    }
}

public struct VillageSetting {
    public string villageName;
    public VILLAGE_SIZE villageSize;

    public static VillageSetting Default {
        get {
            return new VillageSetting() {
                villageName = RandomNameGenerator.GenerateSettlementName(RACE.HUMANS),
                villageSize = VILLAGE_SIZE.Small
            };
        }
    }
}