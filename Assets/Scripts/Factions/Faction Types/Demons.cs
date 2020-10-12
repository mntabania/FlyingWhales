namespace Factions.Faction_Types {
    public class Demons : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.STONE;
        
        public Demons() : base(FACTION_TYPE.Demons) { }
        
        public override void SetAsDefault() { }
        public override void SetFixedData() { }
    }
}