namespace Factions.Faction_Types {
    public class Undead : FactionType {
        public Undead() : base(FACTION_TYPE.Undead) { }
        
        public override void SetAsDefault() {
            Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
            AddIdeology(warmonger);
        }
    }
}