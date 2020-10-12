namespace Factions.Faction_Types {
    public class VampireClan : FactionType {
        public override RESOURCE mainResource => RESOURCE.STONE;
        
        public VampireClan() : base(FACTION_TYPE.Vampire_Clan) { }
        
        public override void SetAsDefault() {
            ReveresVampires reveresVampires = FactionManager.Instance.CreateIdeology<ReveresVampires>(FACTION_IDEOLOGY.Reveres_Vampires);
            AddIdeology(reveresVampires);
            
            //structures
            AddNeededStructure(STRUCTURE_TYPE.VAMPIRE_CASTLE, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.STONE);

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
            AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
        }
        public override void SetFromSaveData() {
            //structures
            AddNeededStructure(STRUCTURE_TYPE.VAMPIRE_CASTLE, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.STONE);
            AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.STONE);

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
            AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
        }
    }
}