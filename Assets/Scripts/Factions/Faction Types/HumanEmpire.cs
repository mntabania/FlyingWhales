using Locations.Settlements;
using UnityEngine;
namespace Factions.Faction_Types {
    public class HumanEmpire : FactionType {
        
        public override RESOURCE mainResource => RESOURCE.STONE;
        
        public HumanEmpire() : base(FACTION_TYPE.Human_Empire) {
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
        }
        public HumanEmpire(SaveDataFactionType saveData) : base(FACTION_TYPE.Human_Empire, saveData) {
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
        }

        public override void SetAsDefault() {
            Peaceful peaceful = FactionManager.Instance.CreateIdeology<Peaceful>(FACTION_IDEOLOGY.Peaceful);
            AddIdeology(peaceful);
            
            Exclusive exclusive = FactionManager.Instance.CreateIdeology<Exclusive>(FACTION_IDEOLOGY.Exclusive);
            exclusive.SetRequirement(RACE.HUMANS);
            AddIdeology(exclusive);

            DivineWorship divineWorship = FactionManager.Instance.CreateIdeology<DivineWorship>(FACTION_IDEOLOGY.Divine_Worship);
            AddIdeology(divineWorship);

            //combatant classes
            AddCombatantClass("Knight");
            AddCombatantClass("Barbarian");
            AddCombatantClass("Mage");
            AddCombatantClass("Stalker");
            AddCombatantClass("Shaman");

            //TODO:
            AddCivilianClass("Miner");
            AddCivilianClass("Crafter");
            AddCivilianClass("Farmer");
            AddCivilianClass("Fisher");
            AddCivilianClass("Logger");
            AddCivilianClass("Merchant");
            AddCivilianClass("Butcher");
            AddCivilianClass("Skinner");
            AddCivilianClass("Trapper");


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
            AddCrime(CRIME_TYPE.Vampire, CRIME_SEVERITY.Heinous);
            AddCrime(CRIME_TYPE.Demon_Worship, CRIME_SEVERITY.Heinous);
        }
        public override void SetFixedData() {
            //combatant classes
            AddCombatantClass("Knight");
            AddCombatantClass("Barbarian");
            AddCombatantClass("Mage");
            AddCombatantClass("Stalker");

            //AddCivilianClass("Peasant");
            AddCivilianClass("Miner");
            AddCivilianClass("Crafter");
            AddCivilianClass("Farmer");
            AddCivilianClass("Fisher");
            AddCivilianClass("Logger");
            AddCivilianClass("Merchant");
            AddCivilianClass("Butcher");
            AddCivilianClass("Skinner");
            AddCivilianClass("Trapper");
        }
        //public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, CRIME_TYPE crimeType, ICrimeable crime) {
        //    switch (crimeType) {
        //        case CRIME_TYPE.Infidelity:
        //            return CRIME_SEVERITY.Infraction;
        //        case CRIME_TYPE.Theft:
        //        case CRIME_TYPE.Disturbances:
        //        case CRIME_TYPE.Assault:
        //        case CRIME_TYPE.Arson:
        //        case CRIME_TYPE.Trespassing:
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
                case CRIME_TYPE.Trespassing:
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
        public override int GetAdditionalMigrationMeterGain(NPCSettlement p_settlement) {
            int unoccupiedDwellings = p_settlement.GetUnoccupiedDwellingCount();
            return Mathf.Min(unoccupiedDwellings, 2);
        }
    }
}