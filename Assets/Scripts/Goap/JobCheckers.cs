using Traits;

public partial class InteractionManager {
    
    public bool CanDoPatrol(Character character) {
        return character.canCombat;
    }
    public bool IsSuicideJobStillValid(Character character) {
        return character.traitContainer.HasTrait("Sulking");
    }
    public bool CanDoCraftFurnitureJob(Character character, JobQueueItem item) {
        TILE_OBJECT_TYPE furnitureToCreate = ((item as GoapPlanJob).targetPOI as TileObject).tileObjectType;
        return furnitureToCreate.CanBeCraftedBy(character);
    }
    public bool CanDoDestroyProfaneJob(Character character) {
        return character.traitContainer.HasTrait("Combatant");
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoCombatJob(Character character) {
        return character.traitContainer.HasTrait("Combatant");
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoObtainFoodOutsideJob(Character character) {
        return character.traitContainer.HasTrait("Worker");
        //return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanDoObtainSupplyOutsideJob(Character character) {
        return character.traitContainer.HasTrait("Worker");
        //return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanDoHolyIncantationJob(Character character) {
        return character.traitContainer.HasTrait("Combatant");
        //return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanDoExploreJob(Character character) {
        return character.traitContainer.HasTrait("Combatant");
        //return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanDoCleanseRegionJob(Character character) {
        return character.traitContainer.HasTrait("Purifier");
//        return true;
    }
    public bool CanDoClaimRegionJob(Character character) {
        return character.traitContainer.HasTrait("Royalty");
    }
    public bool CanDoInvadeRegionJob(Character character) {
        return character.traitContainer.HasTrait("Raider");
    }
    public bool CanDoAttackNonDemonicRegionJob(Character character) {
        return character.traitContainer.HasTrait("Combatant") || character.characterClass.className.Equals("Noble") || character.characterClass.className.Equals("Leader");
    }
    public bool CanDoAttackDemonicRegionJob(Character character) {
        return character.traitContainer.HasTrait("Combatant") || character.characterClass.className.Equals("Noble") || character.characterClass.className.Equals("Leader");
    }
    public bool CanDoJudgementJob(Character character) {
        return character.isSettlementRuler || character.isFactionLeader || character.characterClass.className == "Noble";
        //return character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER;
    }
    public bool CanDoSabotageFactionJob(Character character) {
        return character.traitContainer.HasTrait("Cultist");
    }
    public bool CanCraftTool(Character character) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.TOOL);
        return TILE_OBJECT_TYPE.TOOL.CanBeCraftedBy(character);
    }
    public bool CanDoObtainSupplyJob(Character character) {
        return character.traitContainer.HasTrait("Combatant");
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoProduceWoodJob(Character character) {
        return character.traitContainer.HasTrait("Logger");
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoProduceMetalJob(Character character) {
        return character.traitContainer.HasTrait("Miner");
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanCharacterTakeBuildGoddessStatueJob(Character character) {
        return character.traitContainer.HasTrait("Builder");
    }
    public bool CanBrewPotion(Character character) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.HEALING_POTION);
        return TILE_OBJECT_TYPE.HEALING_POTION.CanBeCraftedBy(character);
    }
    public bool CanTakeBuryJob(Character character) {
        if (!character.traitContainer.HasTrait("Criminal") && character.isAtHomeRegion &&
            character.isPartOfHomeFaction
            && !character.traitContainer.HasTrait("Beast") /*character.role.roleType != CHARACTER_ROLE.BEAST*/) {
            return character.traitContainer.HasTrait("Worker", "Combatant");
            //return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
            //       character.role.roleType == CHARACTER_ROLE.CIVILIAN;
        }
        return false;
    }
    public bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
            //if(job != null) {
            //    GoapPlanJob goapJob = job as GoapPlanJob;
            //    if (targetCharacter.traitContainer.GetNormalTrait<Trait>((string) goapJob.goal.conditionKey).IsResponsibleForTrait(character)) {
            //        return false;
            //    }
            //}
            if (character.isFactionless) {
                return character.race == targetCharacter.race && character.homeRegion == targetCharacter.homeRegion &&
                       !targetCharacter.relationshipContainer.IsEnemiesWith(character);
            }
            return !character.relationshipContainer.IsEnemiesWith(targetCharacter);
        }
        return false;
    }
    public bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter) {
        if (character.isAtHomeRegion && !character.traitContainer.HasTrait("Criminal") &&
            !character.traitContainer.HasTrait("Coward") && character.homeSettlement != null && character.homeSettlement.prison != null) {
            Restrained restrainedTrait = targetCharacter.traitContainer.GetNormalTrait<Restrained>("Restrained");
            if (restrainedTrait == null || !restrainedTrait.isPrisoner) {
                return character.traitContainer.HasTrait("Combatant") /*character.role.roleType == CHARACTER_ROLE.SOLDIER*/ &&
                   character.relationshipContainer.GetRelationshipEffectWith(targetCharacter) !=
                   RELATIONSHIP_EFFECT.POSITIVE;
            }
        }
        return false;
    }
    public bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter) {
        return 
            // character.faction != targetCharacter.faction
            // && character.faction.GetRelationshipWith(targetCharacter.faction).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE 
            character.faction.IsHostileWith(targetCharacter.faction)
            && character.isAtHomeRegion
            && character.isPartOfHomeFaction && character.currentSettlement is NPCSettlement
            //&& (character.role.roleType == CHARACTER_ROLE.SOLDIER ||
            //character.role.roleType == CHARACTER_ROLE.CIVILIAN ||
            //character.role.roleType == CHARACTER_ROLE.ADVENTURER)
            && character.traitContainer.HasTrait("Worker", "Combatant")
            && character.relationshipContainer.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE 
            && !character.traitContainer.HasTrait("Criminal")
            && !targetCharacter.traitContainer.HasTrait("Restrained");
    }
    public bool CanCharacterTakeRepairJob(Character character, JobQueueItem job) {
        bool canTakeRepairJob = false;
        if(job is GoapPlanJob planJob) {
            if(planJob.targetPOI is TileObject targetTileObject) {
                canTakeRepairJob = targetTileObject.canBeRepaired;
            }
        }
        return canTakeRepairJob && character.traitContainer.HasTrait("Worker", "Combatant");
    }
    public bool CanCharacterTakeRepairJob(Character character, TileObject targetTileObject) {
        return targetTileObject.canBeRepaired && character.traitContainer.HasTrait("Worker", "Combatant");
    }
    public bool CanCharacterTakeReplaceTileObjectJob(Character character, JobQueueItem job) {
        object[] otherData = (job as GoapPlanJob).otherData[INTERACTION_TYPE.REPLACE_TILE_OBJECT];
        TileObject removedObj = otherData[0] as TileObject;
        return removedObj.tileObjectType.CanBeCraftedBy(character);
    }
    public bool CanCharacterTakeParalyzedFeedJob(Character sourceCharacter, Character character) {
        return sourceCharacter != character && sourceCharacter.faction == character.faction &&
               sourceCharacter.relationshipContainer.GetRelationshipEffectWith(character) !=
               RELATIONSHIP_EFFECT.NEGATIVE;
    }
    public bool CanCharacterTakeRestrainedFeedJob(Character sourceCharacter, Character character) {
        if (sourceCharacter.currentRegion.IsResident(character)) {
            if (!character.isFactionless) {
                return character.traitContainer.HasTrait("Worker", "Combatant");
                //return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
                //       character.role.roleType == CHARACTER_ROLE.CIVILIAN;
            }
            else {
                return !character.traitContainer.HasTrait("Beast") /*character.role.roleType != CHARACTER_ROLE.BEAST*/ &&
                       sourceCharacter.currentStructure.structureType.IsOpenSpace();
            }
        }
        return false;
    }
    public bool CanCharacterTakeDropJob(Character sourceCharacter, Character character) {
        return sourceCharacter != character && sourceCharacter.faction == character.faction &&
               character.relationshipContainer.GetRelationshipEffectWith(sourceCharacter) !=
               RELATIONSHIP_EFFECT.NEGATIVE;
    }
    public bool CanCharacterTakeKnockoutJob(Character character, Character targetCharacter) {
        return character.traitContainer.HasTrait("Combatant");
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
        //       character.role.roleType == CHARACTER_ROLE.ADVENTURER; // && !HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)
    }
    public bool CanCharacterTakeBuildJob(Character character) {
        return character.traitContainer.HasTrait("Builder");
    }
    public bool CanCharacterTakeRepairStructureJob(Character character) {
        return character.traitContainer.HasTrait("Builder");
    }


    #region Job Applicability
    public bool IsJudgementJobStillApplicable(Character criminal) {
        if (criminal.isDead) {
            //Character is dead
            return false;
        }
        if (criminal.currentSettlement is NPCSettlement npcSettlement && 
            criminal.currentStructure != npcSettlement.prison) {
            //Character is no longer in jail
            return false;
        }
        if (!criminal.traitContainer.HasTrait("Restrained")) {
            //Character is no longer restrained
            return false;
        }
        return true;
    }
    #endregion
}