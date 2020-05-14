using Traits;

public partial class InteractionManager {
    
    public bool CanDoPatrol(Character character) {
        return character.characterClass.CanDoJob(JOB_TYPE.PATROL);
    }
    public bool CanDoCraftFurnitureJob(Character character, JobQueueItem item) {
        TILE_OBJECT_TYPE furnitureToCreate = ((item as GoapPlanJob).targetPOI as TileObject).tileObjectType;
        return furnitureToCreate.CanBeCraftedBy(character);
    }
    public bool CanDoJudgementJob(Character character) {
        return character.isSettlementRuler ||
               character.isFactionLeader; //|| character.characterClass.className == "Noble";
    }
    public bool CanCraftTool(Character character) {
        return character.characterClass.CanDoJob(JOB_TYPE.CRAFT_OBJECT) && TILE_OBJECT_TYPE.TOOL.CanBeCraftedBy(character);
    }
    public bool CanDoProduceFoodJob(Character character) {
        return character.characterClass.CanDoJob(JOB_TYPE.PRODUCE_FOOD);
    }
    public bool CanDoProduceWoodJob(Character character) {
        return character.characterClass.CanDoJob(JOB_TYPE.PRODUCE_WOOD);
    }
    public bool CanDoProduceMetalJob(Character character) {
        return character.characterClass.CanDoJob(JOB_TYPE.PRODUCE_METAL);
    }
    public bool CanDoProduceStoneJob(Character character) {
        return character.characterClass.CanDoJob(JOB_TYPE.PRODUCE_STONE);
    }
    public bool CanBrewPotion(Character character) {
        return TILE_OBJECT_TYPE.HEALING_POTION.CanBeCraftedBy(character);
    }
    public bool CanTakeBuryJob(Character character) {
        if (!character.traitContainer.HasTrait("Criminal") && character.isAtHomeRegion &&
            character.isPartOfHomeFaction && !character.traitContainer.HasTrait("Beast")) {
            return character.characterClass.CanDoJob(JOB_TYPE.BURY);
        }
        return false;
    }
    public bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
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
                return character.characterClass.CanDoJob(JOB_TYPE.APPREHEND) &&
                   character.relationshipContainer.GetRelationshipEffectWith(targetCharacter) !=
                   RELATIONSHIP_EFFECT.POSITIVE;
            }
        }
        return false;
    }
    public bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter) {
        return character.faction.IsHostileWith(targetCharacter.faction)
               && character.isAtHomeRegion
               && character.isPartOfHomeFaction && character.currentSettlement is NPCSettlement
               && character.characterClass.CanDoJob(JOB_TYPE.RESTRAIN)
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
        return canTakeRepairJob && character.characterClass.CanDoJob(JOB_TYPE.REPAIR);
    }
    public bool CanCharacterTakeRepairJob(Character character, TileObject targetTileObject) {
        return targetTileObject.canBeRepaired && character.characterClass.CanDoJob(JOB_TYPE.REPAIR);
    }
    public bool CanCharacterTakeKnockoutJob(Character character, Character targetCharacter) {
        return character.characterClass.CanDoJob(JOB_TYPE.RESTRAIN);
    }
    public bool CanCharacterTakeRepairStructureJob(Character character) {
        return character.characterClass.CanDoJob(JOB_TYPE.REPAIR);
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