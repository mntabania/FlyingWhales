﻿using System.Collections.Generic;
using System.Collections.Specialized;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Factions.Faction_Types {
    public abstract class FactionType {
        public readonly string name;
        public readonly FACTION_TYPE type;
        public readonly List<FactionIdeology> ideologies;
        // public readonly List<StructureSetting> neededStructures;
        public readonly List<string> combatantClasses;
        public readonly List<string> civilianClasses;
        public readonly Dictionary<CRIME_TYPE, CRIME_SEVERITY> crimes;
        public bool hasCrimes { get; protected set; }

        #region getters
        public abstract RESOURCE mainResource { get; }
        #endregion
        
        protected FactionType(FACTION_TYPE type) {
            this.type = type;
            name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
            ideologies = new List<FactionIdeology>();
            // neededStructures = new List<StructureSetting>();
            combatantClasses = new List<string>();
            civilianClasses = new List<string>();
            crimes = new Dictionary<CRIME_TYPE, CRIME_SEVERITY>();
        }
        public FactionType(FACTION_TYPE type, SaveDataFactionType data) {
            this.type = type;
            name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
            ideologies = new List<FactionIdeology>();
            // neededStructures = new List<StructureSetting>();
            combatantClasses = new List<string>();
            civilianClasses = new List<string>();
            for (int i = 0; i < data.ideologies.Count; i++) {
                SaveDataFactionIdeology saveIdeology = data.ideologies[i];
                ideologies.Add(saveIdeology.Load());
            }
            crimes = data.crimes != null ? new Dictionary<CRIME_TYPE, CRIME_SEVERITY>(data.crimes) : new Dictionary<CRIME_TYPE, CRIME_SEVERITY>();
            hasCrimes = data.hasCrimes;
        }

        #region Initialization
        public abstract void SetAsDefault();
        public abstract void SetFixedData();
        #endregion

        #region Ideologies
        public void AddIdeology(FACTION_IDEOLOGY ideology) {
            if (!HasIdeology(ideology)) {
                FactionIdeology factionIdeology = FactionManager.Instance.CreateIdeology<FactionIdeology>(ideology);
                AddIdeologyBase(factionIdeology);
            }
        }
        public void AddIdeology(FactionIdeology ideology) {
            if (!HasIdeology(ideology.ideologyType)) {
                AddIdeologyBase(ideology);
            }
        }
        private void AddIdeologyBase(FactionIdeology ideology) {
            ideologies.Add(ideology);
            ideology.OnAddIdeology(this);
        }
        public void RemoveIdeology(FACTION_IDEOLOGY ideology) {
            if (HasIdeology(ideology, out var factionIdeology)) {
                ideologies.Remove(factionIdeology);
            }
        }
        public void ClearIdeologies() {
            ideologies.Clear();
        }
        public bool HasIdeology(FACTION_IDEOLOGY ideology) {
            for (int i = 0; i < ideologies.Count; i++) {
                FactionIdeology ideal = ideologies[i];
                if (ideal.ideologyType == ideology) {
                    return true;
                }
            }
            return false;
        }
        public bool HasIdeology(FACTION_IDEOLOGY ideology, out FactionIdeology factionIdeology) {
            for (int i = 0; i < ideologies.Count; i++) {
                FactionIdeology ideal = ideologies[i];
                if (ideal.ideologyType == ideology) {
                    factionIdeology = ideal;
                    return true;
                }
            }
            factionIdeology = null;
            return false;
        }
        #endregion

        #region Crimes
        public CRIME_SEVERITY GetCrimeSeverity(Character actor, IPointOfInterest target, CRIME_TYPE crimeType) {
            if (hasCrimes) {
                if (crimes.ContainsKey(crimeType)) {
                    return crimes[crimeType];
                }
                return CRIME_SEVERITY.None;
            }
            return CRIME_SEVERITY.Unapplicable;
        }
        public CRIME_SEVERITY GetCrimeSeverity(CRIME_TYPE crimeType) {
            if (hasCrimes) {
                if (crimes.ContainsKey(crimeType)) {
                    return crimes[crimeType];
                }
                return CRIME_SEVERITY.None;
            }
            return CRIME_SEVERITY.Unapplicable;
        }
        public void AddCrime(CRIME_TYPE type, CRIME_SEVERITY severity) {
            if (!crimes.ContainsKey(type)) {
                crimes.Add(type, severity);
            } else {
                crimes[type] = severity;
            }
        }
        public void RemoveCrime(CRIME_TYPE type) {
            if (crimes.ContainsKey(type)) {
                crimes.Remove(type);
            }
        }
        public abstract CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType);
        public CRIME_TYPE GetRandomNonReligionSeriousCrime() {
            List<CRIME_TYPE> choices = new List<CRIME_TYPE>();
            foreach (var crime in crimes) {
                if (crime.Value == CRIME_SEVERITY.Serious && !crime.Key.IsReligiousCrime()) {
                    choices.Add(crime.Key);
                }
            }
            if (choices.Count > 0) {
                return CollectionUtilities.GetRandomElement(choices);
            }
            return CRIME_TYPE.None;
        }
        #endregion

        #region Structures
        // public void AddNeededStructure(StructureSetting structureSetting) {
        //     neededStructures.Add(structureSetting);
        // }
        // public void AddNeededStructure(STRUCTURE_TYPE structureType, RESOURCE resource) {
        //     StructureSetting structureSetting = new StructureSetting(structureType, resource);
        //     neededStructures.Add(structureSetting);
        // }
        // public StructureSetting GetStructureSettingFor(STRUCTURE_TYPE structureType) {
        //     for (int i = 0; i < neededStructures.Count; i++) {
        //         StructureSetting structureSetting = neededStructures[i];
        //         if (structureSetting.structureType == structureType) {
        //             return structureSetting;
        //         }
        //     }
        //     Debug.LogWarning($"{type} has no structure setting for {structureType}");
        //     return default;
        // }
        #endregion

        #region Combatants
        public void AddCombatantClass(string className) {
            combatantClasses.Add(className);
        }
        public void RemoveCombatantClass(string className) {
            combatantClasses.Remove(className);
        }
        #endregion
        
        #region Civilians
        public void AddCivilianClass(string className) {
            civilianClasses.Add(className);
        }
        public void RemoveCivilianClass(string className) {
            civilianClasses.Remove(className);
        }
        #endregion

    }
}