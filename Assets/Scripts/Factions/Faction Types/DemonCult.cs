namespace Factions.Faction_Types {
    public class DemonCult : FactionType {
        public override RESOURCE mainResource => RESOURCE.STONE;
        
        public DemonCult() : base(FACTION_TYPE.Demon_Cult) { }
        public DemonCult(SaveDataFactionType saveData) : base(FACTION_TYPE.Demon_Cult, saveData) { }
        
        public override void SetAsDefault() {
            Warmonger warmonger = FactionManager.Instance.CreateIdeology<Warmonger>(FACTION_IDEOLOGY.Warmonger);
            AddIdeology(warmonger);

            DemonWorship demonWorship = FactionManager.Instance.CreateIdeology<DemonWorship>(FACTION_IDEOLOGY.Demon_Worship);
            AddIdeology(demonWorship);

            //Demon Worshipper Exclusive Ideology
            Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
            exclusive.SetRequirement(RELIGION.Demon_Worship);
            AddIdeology(exclusive);

            BoneGolemMakers boneGolemMakers = FactionManager.Instance.CreateIdeology<BoneGolemMakers>(FACTION_IDEOLOGY.Bone_Golem_Makers);
            AddIdeology(boneGolemMakers);

            //TODO: Corrupted Resource?
            //structures
            // AddNeededStructure(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.STONE);

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
            AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Nature_Worship, CRIME_SEVERITY.Heinous);
            AddCrime(CRIME_TYPE.Divine_Worship, CRIME_SEVERITY.Heinous);
        }
        public override void SetFixedData() {
            //structures
            // AddNeededStructure(STRUCTURE_TYPE.CITY_CENTER, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.DWELLING, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.MINE_SHACK, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.MAGE_QUARTERS, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.CEMETERY, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.TAVERN, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.BARRACKS, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.HUNTER_LODGE, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.WAREHOUSE, RESOURCE.STONE);
            // AddNeededStructure(STRUCTURE_TYPE.PRISON, RESOURCE.STONE);

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
            // AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Misdemeanor);
            // AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
            // AddCrime(CRIME_TYPE.Nature_Worship, CRIME_SEVERITY.Heinous);
            // AddCrime(CRIME_TYPE.Divine_Worship, CRIME_SEVERITY.Heinous);
        }
    }
}