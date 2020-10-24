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
            //combatant classes
            AddCombatantClass("Archer");
            AddCombatantClass("Hunter");
            AddCombatantClass("Druid");
            AddCombatantClass("Shaman");
            
            AddCivilianClass("Peasant");
            AddCivilianClass("Miner");
            AddCivilianClass("Craftsman");
        }
        
        public override CRIME_SEVERITY GetDefaultSeverity(CRIME_TYPE crimeType) {
            switch (crimeType) {
                case CRIME_TYPE.Infidelity:
                    return CRIME_SEVERITY.Infraction;
                case CRIME_TYPE.Theft:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Disturbances:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Assault:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Arson:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Trespassing:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Cannibalism:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Murder:
                    return CRIME_SEVERITY.Serious;
                case CRIME_TYPE.Vampire:
                    return CRIME_SEVERITY.Heinous;
                case CRIME_TYPE.Demon_Worship:
                    return CRIME_SEVERITY.Heinous;
            }
            return CRIME_SEVERITY.None;
        }
    }
}