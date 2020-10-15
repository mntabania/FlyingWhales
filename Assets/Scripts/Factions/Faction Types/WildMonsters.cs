namespace Factions.Faction_Types {
    public class WildMonsters : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public WildMonsters() : base(FACTION_TYPE.Wild_Monsters) { }
        public WildMonsters(SaveDataFactionType saveData) : base(FACTION_TYPE.Wild_Monsters, saveData) { }
        
        public override void SetAsDefault() { }
        public override void SetFixedData() { }

    }
}