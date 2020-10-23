namespace Factions.Faction_Types {
    public class LycanClan : FactionType {
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public LycanClan() : base(FACTION_TYPE.Lycan_Clan) { }
        public LycanClan(SaveDataFactionType saveData) : base(FACTION_TYPE.Lycan_Clan, saveData) { }
        
        public override void SetAsDefault() {
            Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
            AddIdeology(warmonger);

            ReveresWerewolves reveresWerewolves = FactionManager.Instance.CreateIdeology<ReveresWerewolves>(FACTION_IDEOLOGY.Reveres_Werewolves);
            AddIdeology(reveresWerewolves);
            
            //structures
            // AddNeededStructure(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.DWELLING, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.FARM, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.APOTHECARY, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.TAVERN, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.WOOD);

            //combatant classes
            AddCombatantClass("Archer");
            AddCombatantClass("Hunter");
            AddCombatantClass("Druid");
            AddCombatantClass("Shaman");
            
            AddCivilianClass("Peasant");
            AddCivilianClass("Miner");
            AddCivilianClass("Craftsman");

            //crimes
            hasCrimes = true;
            AddCrime(CRIME_TYPE.Infidelity, CRIME_SEVERITY.Infraction);
            AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Disturbances, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Arson, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Trespassing, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
            AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
        }
        public override void SetFixedData() {
            //structures
            // AddNeededStructure(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.DWELLING, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.LUMBERYARD, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.FARM, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.APOTHECARY, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.CEMETERY, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.TAVERN, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.WOOD);
            // AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.WOOD);

            //combatant classes
            AddCombatantClass("Archer");
            AddCombatantClass("Hunter");
            AddCombatantClass("Druid");
            AddCombatantClass("Shaman");
            
            AddCivilianClass("Peasant");
            AddCivilianClass("Miner");
            AddCivilianClass("Craftsman");

            // //crimes
            // hasCrimes = true;
            // AddCrime(CRIME_TYPE.Infidelity, CRIME_SEVERITY.Infraction);
            // AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Disturbances, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Arson, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Trespassing, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
            // AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
            // AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
        }
    }
}