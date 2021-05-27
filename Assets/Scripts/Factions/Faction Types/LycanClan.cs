﻿using Inner_Maps.Location_Structures;
namespace Factions.Faction_Types {
    public class LycanClan : FactionType {
        public override RESOURCE mainResource => RESOURCE.WOOD;
        
        public LycanClan() : base(FACTION_TYPE.Lycan_Clan) {
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
        }
        public LycanClan(SaveDataFactionType saveData) : base(FACTION_TYPE.Lycan_Clan, saveData) {
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
        }

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

            //AddCivilianClass("Peasant");
            AddCivilianClass("Miner");
            AddCivilianClass("Craftsman");
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

            //AddCivilianClass("Peasant");
            AddCivilianClass("Miner");
            AddCivilianClass("Craftsman");
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
        
        public override StructureSetting ProcessStructureSetting(StructureSetting p_setting, NPCSettlement p_settlement) {
            if (p_settlement.SettlementResources.HasResourceAmount(p_settlement, p_setting.resource, p_setting.structureType.GetResourceBuildCost())) {
                //if settlement has that resource amount then use default setting
                return p_setting;
            } else {
                //if settlement doesn't have that resource amount then check if other resource is available.
                RESOURCE otherResource = p_setting.resource == RESOURCE.WOOD ? RESOURCE.STONE : RESOURCE.WOOD;
                if (p_settlement.SettlementResources.HasResourceAmount(p_settlement, otherResource, p_setting.structureType.GetResourceBuildCost())) {
                    return new StructureSetting(p_setting.structureType, otherResource, p_setting.isCorrupted);
                } else {
                    return p_setting;    
                }
            }
        }
        public override StructureSetting CreateStructureSettingForStructure(STRUCTURE_TYPE structureType, NPCSettlement p_settlement) {
            if (!structureType.RequiresResourceToBuild()) { return new StructureSetting(structureType, RESOURCE.NONE); }
            if (p_settlement.SettlementResources.HasResourceAmount(p_settlement, RESOURCE.WOOD, structureType.GetResourceBuildCost())) {
                return new StructureSetting(structureType, RESOURCE.WOOD);
            } else {
                return new StructureSetting(structureType, RESOURCE.STONE);
            }
        }
    }
}