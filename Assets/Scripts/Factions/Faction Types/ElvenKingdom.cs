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
            AddNeededStructure(STRUCTURE_TYPE.TAVERN, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.WOOD);
            AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.WOOD);
            
            //combatant classes
            AddCombatantClass("Archer");
            AddCombatantClass("Hunter");
            AddCombatantClass("Druid");
            AddCombatantClass("Shaman");
        }
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType, ICrimeable crime) {
            switch (crimeType) {
                case CRIME_TYPE.Infidelity:
                    return CRIME_SEVERITY.Infraction;
                case CRIME_TYPE.Theft:
                case CRIME_TYPE.Disturbances:
                case CRIME_TYPE.Assault:
                case CRIME_TYPE.Arson:
                case CRIME_TYPE.Trespassing:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Murder:
                case CRIME_TYPE.Animal_Killing:
                case CRIME_TYPE.Cannibalism:
                    return CRIME_SEVERITY.Serious;
                case CRIME_TYPE.Vampire:
                case CRIME_TYPE.Demon_Worship:
                    return CRIME_SEVERITY.Heinous;
            }
            return CRIME_SEVERITY.None;
        }
    }
}