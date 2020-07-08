using System.Collections.Generic;
namespace Factions.Faction_Types {
    public abstract class FactionType {
        public readonly FACTION_TYPE type;
        public readonly List<FactionIdeology> ideologies;
        
        public FactionType(FACTION_TYPE type) {
            this.type = type;
            ideologies = new List<FactionIdeology>();
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
    }
}