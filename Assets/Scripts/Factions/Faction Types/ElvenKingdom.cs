namespace Factions.Faction_Types {
    public class ElvenKingdom : FactionType {
        public ElvenKingdom() : base(FACTION_TYPE.Elven_Kingdom) { }
        
        public override void SetAsDefault() {
            Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
            AddIdeology(peaceful);
            
            Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
            exclusive.SetRequirement(RACE.ELVES);
            AddIdeology(exclusive);

            NatureWorship natureWorship =
                FactionManager.Instance.CreateIdeology<NatureWorship>(FACTION_IDEOLOGY.Nature_Worship);
            AddIdeology(natureWorship);
            
            //structures
            AddNeededStructure(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.DWELLING, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.FARM, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.APOTHECARY, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.INN, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.WOOD);
        }
    }
}