namespace Factions.Faction_Types {
    public class Ratmen : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public Ratmen() : base(FACTION_TYPE.Ratmen) { }
        public Ratmen(SaveDataFactionType saveData) : base(FACTION_TYPE.Ratmen, saveData) { }
        
        public override void SetAsDefault() { }
        public override void SetFixedData() { }
        public override CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType) {
            return CRIME_SEVERITY.Unapplicable;
        }
    }
}