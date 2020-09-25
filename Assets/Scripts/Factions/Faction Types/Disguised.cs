namespace Factions.Faction_Types {
    public class Disguised : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public Disguised() : base(FACTION_TYPE.Disguised) { }
        
        public override void SetAsDefault() { }
    }
}