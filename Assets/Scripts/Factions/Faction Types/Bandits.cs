namespace Factions.Faction_Types {
    public class Bandits : FactionType {
        public override RESOURCE mainResource => RESOURCE.WOOD;
        public Bandits() : base(FACTION_TYPE.Bandits) { }
        
        public override void SetAsDefault() { }
    }
}