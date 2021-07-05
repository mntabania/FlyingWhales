using Inner_Maps.Location_Structures;
namespace Factions.Faction_Types {
    public class DemonCult : FactionType {
        public override RESOURCE mainResource => RESOURCE.STONE;
        public override bool usesCorruptedStructures => true;
        
        public DemonCult() : base(FACTION_TYPE.Demon_Cult) {
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
        }
        public DemonCult(SaveDataFactionType saveData) : base(FACTION_TYPE.Demon_Cult, saveData) {
            succession = FactionManager.Instance.GetFactionSuccession(FACTION_SUCCESSION_TYPE.Power);
        }

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
            AddCrime(CRIME_TYPE.Theft, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Assault, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Cannibalism, CRIME_SEVERITY.Misdemeanor);
            AddCrime(CRIME_TYPE.Murder, CRIME_SEVERITY.Serious);
            AddCrime(CRIME_TYPE.Nature_Worship, CRIME_SEVERITY.Heinous);
            AddCrime(CRIME_TYPE.Divine_Worship, CRIME_SEVERITY.Heinous);
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
                case CRIME_TYPE.Theft:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Assault:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Cannibalism:
                    return CRIME_SEVERITY.Misdemeanor;
                case CRIME_TYPE.Murder:
                    return CRIME_SEVERITY.Serious;
                case CRIME_TYPE.Nature_Worship:
                    return CRIME_SEVERITY.Heinous;
                case CRIME_TYPE.Divine_Worship:
                    return CRIME_SEVERITY.Heinous;
            }
            return CRIME_SEVERITY.None;
        }
        public override void ProcessNewMember(Character character) {
            if (character.isNormalCharacter && !character.traitContainer.HasTrait("Cultist")) {
                //https://trello.com/c/au0rNNT6/3219-non-cultist-in-demon-cult
                character.traitContainer.AddTrait(character, "Cultist");
            }
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
            if (!structureType.RequiresResourceToBuild()) { return new StructureSetting(structureType, RESOURCE.NONE, true); }
            if (structureType == STRUCTURE_TYPE.FISHERY) { return new StructureSetting(structureType, RESOURCE.WOOD, true); }
            if (structureType == STRUCTURE_TYPE.BUTCHERS_SHOP) { return new StructureSetting(structureType, RESOURCE.STONE, true); }
            if (p_settlement.settlementJobTriggerComponent.HasAccessToResource(RESOURCE.WOOD)) {
                return new StructureSetting(structureType, RESOURCE.WOOD, true);    
            } else {
                return new StructureSetting(structureType, RESOURCE.STONE, true);    
            }
        }
    }
}