using Traits;

public partial class InteractionManager {
    
    public bool CanDoPatrol(Character character) {
        return character.canCombat;
    }
    public bool CanDoCraftFurnitureJob(Character character, JobQueueItem item) {
        TILE_OBJECT_TYPE furnitureToCreate = ((item as GoapPlanJob).targetPOI as TileObject).tileObjectType;
        return furnitureToCreate.CanBeCraftedBy(character);
    }
    public bool CanDoCleanseRegionJob(Character character) {
        return character.traitContainer.HasTrait("Purifier");
//        return true;
    }
    public bool CanDoClaimRegionJob(Character character) {
        return character.traitContainer.HasTrait("Royalty");
    }
    public bool CanDoJudgementJob(Character character) {
        return character.isSettlementRuler || character.isFactionLeader || character.characterClass.className == "Noble";
        //return character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER;
    }
    public bool CanCraftTool(Character character) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.TOOL);
        return TILE_OBJECT_TYPE.TOOL.CanBeCraftedBy(character);
    }
    public bool CanDoObtainSupplyJob(Character character) {
        return true;
        //NOTE: No longer checks for combatant if can do produce resource job since it is already part of the priority jobs list
        //return character.traitContainer.HasTrait("Combatant");
    }
    public bool CanDoProduceWoodJob(Character character) {
        return character.characterClass.className == "Miner" || character.characterClass.className == "Peasant";
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoProduceMetalJob(Character character) {
        return character.characterClass.className == "Miner";
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
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
            if (character.isFactionless || character.isFriendlyFactionless) {
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
    public bool CanCharacterTakeKnockoutJob(Character character, Character targetCharacter) {
        return character.traitContainer.HasTrait("Combatant");
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
        //       character.role.roleType == CHARACTER_ROLE.ADVENTURER; // && !HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)
    }
    public bool CanCharacterTakeRepairStructureJob(Character character) {
        return character.characterClass.className == "Craftsman";
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