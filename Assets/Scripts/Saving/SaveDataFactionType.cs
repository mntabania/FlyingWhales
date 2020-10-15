using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Factions.Faction_Types;
using Inner_Maps.Location_Structures;
using BayatGames.SaveGameFree;

[System.Serializable]
public class SaveDataFactionType : SaveData<FactionType> {
    public FACTION_TYPE type;
    public List<SaveDataFactionIdeology> ideologies;
    //public List<StructureSetting> neededStructures;
    //public List<string> combatantClasses;
    public Dictionary<CRIME_TYPE, CRIME_SEVERITY> crimes;
    public bool hasCrimes;

    #region Overrides
    public override void Save(FactionType data) {
        type = data.type;

        ideologies = new List<SaveDataFactionIdeology>();
        if(data.ideologies != null) {
            for (int i = 0; i < data.ideologies.Count; i++) {
                FactionIdeology ideology = data.ideologies[i];
                SaveDataFactionIdeology saveIdeology = new SaveDataFactionIdeology();
                saveIdeology.Save(ideology);
                ideologies.Add(saveIdeology);
            }
        }

        crimes = new Dictionary<CRIME_TYPE, CRIME_SEVERITY>(data.crimes);
        hasCrimes = data.hasCrimes;
        //neededStructures = data.neededStructures;
        //combatantClasses = data.combatantClasses;
    }
    public override FactionType Load() {
        FactionType newFactionType = FactionManager.Instance.CreateFactionType(type, this);
        newFactionType.SetFixedData();
        //for (int i = 0; i < neededStructures.Count; i++) {
        //    newFactionType.AddNeededStructure(neededStructures[i]);
        //}
        //for (int i = 0; i < combatantClasses.Count; i++) {
        //    newFactionType.AddCombatantClass(combatantClasses[i]);
        //}
        return newFactionType;
    }
    #endregion
}

[System.Serializable]
public class SaveDataFactionIdeology : SaveData<FactionIdeology> {
    public FACTION_IDEOLOGY ideologyType;

    //Exclusive
    public EXCLUSIVE_IDEOLOGY_CATEGORIES category;
    public RACE raceRequirement;
    public GENDER genderRequirement;
    public string traitRequirement;

    #region Overrides
    public override void Save(FactionIdeology data) {
        ideologyType = data.ideologyType;
        
        if(data is Exclusive exclusive) {
            category = exclusive.category;
            raceRequirement = exclusive.raceRequirement;
            genderRequirement = exclusive.genderRequirement;
            traitRequirement = exclusive.traitRequirement;
        }
    }
    public override FactionIdeology Load() {
        FactionIdeology newIdeology = FactionManager.Instance.CreateIdeology<FactionIdeology>(ideologyType);
        if(newIdeology is Exclusive exclusive) {
            if(category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
                exclusive.SetRequirement(raceRequirement);
            } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
                exclusive.SetRequirement(genderRequirement);
            } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT) {
                exclusive.SetRequirement(traitRequirement);
            }
        }
        return newIdeology;
    }
    #endregion
}