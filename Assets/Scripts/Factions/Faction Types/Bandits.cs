namespace Factions.Faction_Types {
    public class Bandits : FactionType {
        public override RESOURCE mainResource => RESOURCE.WOOD;
        public Bandits() : base(FACTION_TYPE.Bandits) { }
        public Bandits(SaveDataFactionType saveData) : base(FACTION_TYPE.Bandits, saveData) { }
        
        public override void SetAsDefault() { }
        public override void SetFixedData() { }
        public override CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType) {
            return CRIME_SEVERITY.Unapplicable;
        }
    }
}