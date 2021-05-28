using System.Collections.Generic;
using System.Collections.Specialized;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
using Factions.Faction_Succession;
using Locations.Settlements;
namespace Factions.Faction_Types {
    public abstract class FactionType {
        public readonly string name;
        public readonly FACTION_TYPE type;
        public readonly List<FactionIdeology> ideologies;
        // public readonly List<StructureSetting> neededStructures;
        public readonly List<string> combatantClasses;
        public readonly List<string> civilianClasses;
        public readonly Dictionary<CRIME_TYPE, CRIME_SEVERITY> crimes; //FOR RESTRUCTURE: data must be Dictionary<CRIME_SEVERITY, List<CRIME_TYPE>>. Must restructure after build on Nov. 6, 2020. This will change save data that is why deletion must occur after build
        public FactionSuccession succession { get; protected set; }
        public bool hasCrimes { get; protected set; }

        #region getters
        public abstract RESOURCE mainResource { get; }
        public virtual bool usesCorruptedStructures => false;
        #endregion
        
        protected FactionType(FACTION_TYPE type) {
            this.type = type;
            name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
            ideologies = new List<FactionIdeology>();
            // neededStructures = new List<StructureSetting>();
            combatantClasses = new List<string>();
            civilianClasses = new List<string>();
            crimes = new Dictionary<CRIME_TYPE, CRIME_SEVERITY>();
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.None);
        }
        public FactionType(FACTION_TYPE type, SaveDataFactionType data) {
            this.type = type;
            name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
            ideologies = new List<FactionIdeology>();
            // neededStructures = new List<StructureSetting>();
            combatantClasses = new List<string>();
            civilianClasses = new List<string>();
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.None);

            for (int i = 0; i < data.ideologies.Count; i++) {
                SaveDataFactionIdeology saveIdeology = data.ideologies[i];
                ideologies.Add(saveIdeology.Load());
            }
            crimes = data.crimes != null ? new Dictionary<CRIME_TYPE, CRIME_SEVERITY>(data.crimes) : new Dictionary<CRIME_TYPE, CRIME_SEVERITY>();
            hasCrimes = data.hasCrimes;
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.None);

        }

        #region Initialization
        public abstract void SetAsDefault();
        public abstract void SetFixedData();
        #endregion

        #region Ideologies
        public FactionIdeology AddIdeology(FACTION_IDEOLOGY ideology) {
            if (!HasIdeology(ideology)) {
                FactionIdeology factionIdeology = FactionManager.Instance.CreateIdeology<FactionIdeology>(ideology);
                AddIdeologyBase(factionIdeology);
                return factionIdeology;
            }
            return null;
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
        public bool HasPeaceTypeIdeology() {
            for (int i = 0; i < ideologies.Count; i++) {
                FactionIdeology ideal = ideologies[i];
                if (ideal.ideologyType.IsPeaceType()) {
                    return true;
                }
            }
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
        public void AddCrime(CRIME_TYPE type, CRIME_SEVERITY severity, bool shouldBroadcastSignal = false) {
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
        public bool IsActionConsideredACrime(CRIME_TYPE p_crimeType) {
            CRIME_SEVERITY severity = GetCrimeSeverity(p_crimeType);
            return severity != CRIME_SEVERITY.Unapplicable && severity != CRIME_SEVERITY.None;
        }
        #endregion

        #region Structures
        public virtual StructureSetting ProcessStructureSetting(StructureSetting p_setting, NPCSettlement p_settlement) {
            return p_setting;
        }
        public virtual StructureSetting CreateStructureSettingForStructure(STRUCTURE_TYPE structureType, NPCSettlement p_settlement) {
            RESOURCE resource = structureType.RequiresResourceToBuild() ? RESOURCE.STONE : RESOURCE.NONE;
            if (structureType == STRUCTURE_TYPE.FISHERY) { resource = RESOURCE.WOOD; }
            if (structureType == STRUCTURE_TYPE.BUTCHERS_SHOP) { resource = RESOURCE.STONE; }
            return new StructureSetting(structureType, resource, false);
        }
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
        public bool IsCivilian(string className) {
            return civilianClasses != null && civilianClasses.Contains(className);
        }
        #endregion

        #region Migration
        public virtual int GetAdditionalMigrationMeterGain(NPCSettlement p_settlement) {
            return 0;
        }
        #endregion

        #region Members
        public virtual void ProcessNewMember(Character character) { }
        #endregion
        
    }
}