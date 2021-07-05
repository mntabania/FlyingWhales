using Inner_Maps.Location_Structures;
namespace Factions.Faction_Types {
    public class VampireClan : FactionType {
        public override RESOURCE mainResource => RESOURCE.STONE;
        
        public VampireClan() : base(FACTION_TYPE.Vampire_Clan) {
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Popularity);
        }
        public VampireClan(SaveDataFactionType saveData) : base(FACTION_TYPE.Vampire_Clan, saveData) {
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Popularity);
        }

        public override void SetAsDefault() {
            ReveresVampires reveresVampires = FactionManager.Instance.CreateIdeology<ReveresVampires>(FACTION_IDEOLOGY.Reveres_Vampires);
            AddIdeology(reveresVampires);

            //combatant classes
            AddCombatantClass("Archer");
            AddCombatantClass("Hunter");
            AddCombatantClass("Druid");
            AddCombatantClass("Shaman");
            
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
        public override void SetFixedData() {
            //combatant classes
            AddCombatantClass("Archer");
            AddCombatantClass("Hunter");
            AddCombatantClass("Druid");
            AddCombatantClass("Shaman");

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
                    return CRIME_SEVERITY.Heinous;
                case CRIME_TYPE.Werewolf:
                    return CRIME_SEVERITY.Heinous;
            }
            return CRIME_SEVERITY.None;
        }
        public override StructureSetting ProcessStructureSetting(StructureSetting p_setting, NPCSettlement p_settlement) {
            if (p_settlement.settlementJobTriggerComponent.HasAccessToResource(p_setting.resource)) {
                //if settlement has that resource amount then use default setting
                return p_setting;
            } else {
                //if settlement doesn't have that resource amount then check if other resource is available.
                RESOURCE otherResource = p_setting.resource == RESOURCE.WOOD ? RESOURCE.STONE : RESOURCE.WOOD;
                return new StructureSetting(p_setting.structureType, otherResource, p_setting.isCorrupted);
            }
        }
        public override StructureSetting CreateStructureSettingForStructure(STRUCTURE_TYPE structureType, NPCSettlement p_settlement) {
            if (!structureType.RequiresResourceToBuild()) { return new StructureSetting(structureType, RESOURCE.NONE); }
            if (structureType == STRUCTURE_TYPE.FISHERY) { return new StructureSetting(structureType, RESOURCE.WOOD); }
            if (structureType == STRUCTURE_TYPE.BUTCHERS_SHOP) { return new StructureSetting(structureType, RESOURCE.STONE); }
            if (p_settlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.WOOD)) {
                return new StructureSetting(structureType, RESOURCE.WOOD);    
            } else {
                return new StructureSetting(structureType, RESOURCE.STONE);    
            }
        }
    }
}