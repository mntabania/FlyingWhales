namespace Factions.Faction_Types {
    public class Disguised : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public Disguised() : base(FACTION_TYPE.Disguised) { }
        public Disguised(SaveDataFactionType saveData) : base(FACTION_TYPE.Disguised, saveData) { }
        
        public override void SetAsDefault() { }
        public override void SetFixedData() { }
    }
}