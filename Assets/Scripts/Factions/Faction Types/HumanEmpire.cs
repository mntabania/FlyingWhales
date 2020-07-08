namespace Factions.Faction_Types {
    public class HumanEmpire : FactionType {
        public HumanEmpire() : base(FACTION_TYPE.Human_Empire) { }
        
        public override void SetAsDefault() {
            Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
            AddIdeology(peaceful);
            
            Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
            exclusive.SetRequirement(RACE.HUMANS);
            AddIdeology(exclusive);

            DivineWorship divineWorship =
                FactionManager.Instance.CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
            AddIdeology(divineWorship);
            
            //structures
            AddNeededStructure(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.INN, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.STONE);
        }
    }
}