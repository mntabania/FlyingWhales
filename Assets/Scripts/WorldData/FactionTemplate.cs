using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

[System.Serializable]
public class FactionTemplate {
    public string name { get; private set; }
    public string factionTypeString { get; private set; } //NOTE: This can be set as "Random"
    public Sprite factionEmblem { get; private set; }
    public List<VillageSetting> villageSettings { get; }
    public FACTION_TYPE factionType { get; private set; }

    public FactionTemplate(int p_villageCount) {
        name = RandomNameGenerator.GenerateFactionName();
        factionTypeString = "Human Empire";
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
    public void ChangeFactionType(string p_factionType) {
        Debug.Log($"Changed faction type of {name} to {p_factionType}");
        factionTypeString = p_factionType;
        if (factionTypeString != "Random") {
            FACTION_TYPE type = (FACTION_TYPE)System.Enum.Parse(typeof(FACTION_TYPE), factionTypeString, true);
            SetFactionType(type);
        }
    }
    public void SetFactionType(FACTION_TYPE p_factionType) {
        factionType = p_factionType;
    }
    public void ChangeName(string p_newName) {
        Debug.Log($"Changed name of {name} to {p_newName}");
        name = p_newName;
    }
}