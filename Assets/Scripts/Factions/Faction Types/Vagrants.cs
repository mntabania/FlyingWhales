namespace Factions.Faction_Types {
    public class Vagrants : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public Vagrants() : base(FACTION_TYPE.Vagrants) { }
        public Vagrants(SaveDataFactionType saveData) : base(FACTION_TYPE.Vagrants, saveData) { }
        
        public override void SetAsDefault() {
            //crimes
            hasCrimes = true;
            AddCrime(CRIME_TYPE.Infidelity, CRIME_SEVERITY.Infraction);
            AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Disturbances, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Arson, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Werewolf, CRIME_SEVERITY.Heinous);
            AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
            AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
        }
        public override void SetFixedData() {
          
        }
        //public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType, ICrimeable crime) {
        //    switch (crimeType) {
        //        case CRIME_TYPE.Infidelity:
        //            return CRIME_SEVERITY.Infraction;
        //        case CRIME_TYPE.Theft:
        //        case CRIME_TYPE.Disturbances:
        //        case CRIME_TYPE.Assault:
        //        case CRIME_TYPE.Arson:
        //            return CRIME_SEVERITY.Misdemeanor;
        //        case CRIME_TYPE.Murder:
        //        case CRIME_TYPE.Cannibalism:
        //            return CRIME_SEVERITY.Serious;
        //        case CRIME_TYPE.Werewolf:
        //        case CRIME_TYPE.Vampire:
        //        case CRIME_TYPE.Demon_Worship:
        //            return CRIME_SEVERITY.Heinous;
        //    }
        //    return CRIME_SEVERITY.None;
        //}
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
                case CRIME_TYPE.Murder:
                    return CRIME_SEVERITY.Serious;
                case CRIME_TYPE.Cannibalism:
                    return CRIME_SEVERITY.Serious;
                case CRIME_TYPE.Werewolf:
                    return CRIME_SEVERITY.Heinous;
                case CRIME_TYPE.Vampire:
                    return CRIME_SEVERITY.Heinous;
                case CRIME_TYPE.Demon_Worship:
                    return CRIME_SEVERITY.Heinous;
            }
            return CRIME_SEVERITY.None;
        }
    }
}