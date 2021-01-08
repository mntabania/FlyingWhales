using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

[System.Serializable]
public class FactionSetting {
    public string name { get; private set; }
    public string factionTypeString { get; private set; } //NOTE: This can be set as "Random"
    public Sprite factionEmblem { get; private set; }
    public List<VillageSetting> villageSettings { get; }
    public FACTION_TYPE factionType { get; private set; }

    public FactionSetting(int p_villageCount) {
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
        if (factionTypeString == "Random") {
            //randomize faction type value now
            factionType = CollectionUtilities.GetRandomElement(GameUtilities.customWorldFactionTypeChoices);
        } else {
            FACTION_TYPE type = (FACTION_TYPE)System.Enum.Parse(typeof(FACTION_TYPE), factionTypeString, true);
            factionType = type;
        }
    }
    public void ChangeName(string p_newName) {
        Debug.Log($"Changed name of {name} to {p_newName}");
        name = p_newName;
    }
    public bool IsTilePreferredByFaction(HexTile p_tile) {
        switch (factionType) {
            case FACTION_TYPE.Human_Empire:
                return p_tile.biomeType == BIOMES.GRASSLAND || p_tile.biomeType == BIOMES.DESERT;
            case FACTION_TYPE.Elven_Kingdom:
                return p_tile.biomeType == BIOMES.FOREST || p_tile.biomeType == BIOMES.SNOW;
            default:
                return true;
        }
    }
}