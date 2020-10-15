namespace Factions.Faction_Types {
    public class Undead : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public Undead() : base(FACTION_TYPE.Undead) { }
        public Undead(SaveDataFactionType saveData) : base(FACTION_TYPE.Undead, saveData) { }
        
        public override void SetAsDefault() {
            Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
            AddIdeology(warmonger);
        }
        public override void SetFixedData() { }
    }
}