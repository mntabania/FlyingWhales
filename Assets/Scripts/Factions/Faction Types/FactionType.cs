using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
namespace Factions.Faction_Types {
    public abstract class FactionType {
        public readonly string name;
        public readonly FACTION_TYPE type;
        public readonly List<FactionIdeology> ideologies;
        public readonly List<StructureSetting> neededStructures;
        public readonly List<string> combatantClasses;
        
        protected FactionType(FACTION_TYPE type) {
            this.type = type;
            name = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
            ideologies = new List<FactionIdeology>();
            neededStructures = new List<StructureSetting>();
            combatantClasses = new List<string>();
        }

        #region Initialization
        public abstract void SetAsDefault();
        #endregion

        #region Ideologies
        public void AddIdeology(FactionIdeology ideology) {
            ideologies.Add(ideology);
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
        public virtual bool IsActionConsideredCrime(ActualGoapNode action) {
            return true;
        }
        #endregion

        #region Structures
        public void AddNeededStructure(StructureSetting structureSetting) {
            neededStructures.Add(structureSetting);
        }
        public void AddNeededStructure(STRUCTURE_TYPE structureType, RESOURCE resource) {
            StructureSetting structureSetting = new StructureSetting(structureType, resource);
            neededStructures.Add(structureSetting);
        }
        public StructureSetting GetStructureSettingFor(STRUCTURE_TYPE structureType) {
            for (int i = 0; i < neededStructures.Count; i++) {
                StructureSetting structureSetting = neededStructures[i];
                if (structureSetting.structureType == structureType) {
                    return structureSetting;
                }
            }
            Debug.LogWarning($"{type} has no structure setting for {structureType}");
            return default;
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
        
    }
}