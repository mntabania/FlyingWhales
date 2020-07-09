using System.Collections.Generic;
using Inner_Maps.Location_Structures;
namespace Factions.Faction_Types {
    public abstract class FactionType {
        public readonly FACTION_TYPE type;
        public readonly List<FactionIdeology> ideologies;
        public readonly List<StructureSetting> neededStructures;
        
        protected FactionType(FACTION_TYPE type) {
            this.type = type;
            ideologies = new List<FactionIdeology>();
            neededStructures = new List<StructureSetting>();
        }

        #region Initialization
        public abstract void SetAsDefault();
        #endregion

        #region Ideologies
        public void AddIdeology(FactionIdeology ideology) {
            ideologies.Add(ideology);
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
        #endregion

        #region Crimes
        public virtual bool IsActionConsideredCrime(ActualGoapNode action) {
            return true;
        }
        #endregion

        #region Structures
        public void AddNeededStructure(STRUCTURE_TYPE structureType, RESOURCE resource) {
            StructureSetting structureSetting = new StructureSetting(structureType, resource);
            neededStructures.Add(structureSetting);
        }
        #endregion
        
    }
}