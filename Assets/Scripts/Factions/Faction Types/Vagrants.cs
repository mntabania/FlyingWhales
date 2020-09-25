namespace Factions.Faction_Types {
    public class Vagrants : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public Vagrants() : base(FACTION_TYPE.Vagrants) { }
        
        public override void SetAsDefault() { }

        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType, ICrimeable crime) {
            switch (crimeType) {
                case CRIME_TYPE.Infidelity:
                    return CRIME_SEVERITY.Infraction;
                case CRIME_TYPE.Theft:
                case CRIME_TYPE.Disturbances:
                case CRIME_TYPE.Assault:
                case CRIME_TYPE.Arson:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Murder:
                case CRIME_TYPE.Cannibalism:
                    return CRIME_SEVERITY.Serious;
                case CRIME_TYPE.Werewolf:
                case CRIME_TYPE.Vampire:
                case CRIME_TYPE.Demon_Worship:
                    return CRIME_SEVERITY.Heinous;
            }
            return CRIME_SEVERITY.None;
        }
    }
}