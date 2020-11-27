using Traits;

public partial class InteractionManager {
    
    public bool CanDoPatrol(Character character) {
        return true; //character.characterClass.CanDoJob(JOB_TYPE.PATROL);
    }
    public bool CanDoCraftFurnitureJob(Character character, JobQueueItem item) {
        TILE_OBJECT_TYPE furnitureToCreate = ((item as GoapPlanJob).targetPOI as TileObject).tileObjectType;
        return furnitureToCreate.CanBeCraftedBy(character);
    }
    public bool CanDoJudgementJob(Character character) {
        return character.isSettlementRuler || character.isFactionLeader;
    }
    public bool CanCraftTool(Character character) {
        return /*character.characterClass.CanDoJob(JOB_TYPE.CRAFT_OBJECT) &&*/ TILE_OBJECT_TYPE.TOOL.CanBeCraftedBy(character);
    }
    public bool CanDoProduceFoodJob(Character character) {
        return true; //character.characterClass.CanDoJob(JOB_TYPE.PRODUCE_FOOD);
    }
    public bool CanDoProduceWoodJob(Character character) {
        return true; //character.characterClass.CanDoJob(JOB_TYPE.PRODUCE_WOOD);
    }
    public bool CanDoProduceMetalJob(Character character) {
        return true; //character.characterClass.CanDoJob(JOB_TYPE.PRODUCE_METAL);
    }
    public bool CanDoProduceStoneJob(Character character) {
        return true; //character.characterClass.CanDoJob(JOB_TYPE.PRODUCE_STONE);
    }
    public bool CanBrewPotion(Character character) {
        return TILE_OBJECT_TYPE.HEALING_POTION.CanBeCraftedBy(character);
    }
    public bool CanBrewAntidote(Character character) {
        return TILE_OBJECT_TYPE.ANTIDOTE.CanBeCraftedBy(character);
    }
    public bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeRegion) {
            if (character.isFactionless || character.isVagrantOrFactionless) {
                return character.race == targetCharacter.race && character.homeRegion == targetCharacter.homeRegion &&
                       !targetCharacter.relationshipContainer.IsEnemiesWith(character);
            }
            return !character.relationshipContainer.IsEnemiesWith(targetCharacter);
        }
        return false;
    }
    public bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter) {
        if (!character.traitContainer.HasTrait("Coward")) {
            bool canTakeApprehend = true;
            if (character.traitContainer.HasTrait("Cultist") && targetCharacter.traitContainer.HasTrait("Cultist")) {
                canTakeApprehend = false;
            }
            if (character.traitContainer.HasTrait("Hemophiliac")) {
                Vampire vampireTrait = targetCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (vampireTrait != null && vampireTrait.DoesCharacterKnowThisVampire(character)) {
                    canTakeApprehend = false;
                }
            }
            if (character.traitContainer.HasTrait("Lycanphiliac")) {
                if (targetCharacter.isLycanthrope && targetCharacter.lycanData.DoesCharacterKnowThisLycan(character)) {
                    canTakeApprehend = false;
                }
            }
            if (canTakeApprehend) {
                if (character.relationshipContainer.IsFriendsWith(targetCharacter)) {
                    return false;
                } else if ((character.relationshipContainer.IsFamilyMember(targetCharacter) || character.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                           && !character.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                    return false;
                }
                return true;
            }
        }
        return false;
    }
    public bool CanCharacterTakeRepairJob(Character character, JobQueueItem job) {
        bool canTakeRepairJob = false;
        if(job is GoapPlanJob planJob) {
            if(planJob.targetPOI is TileObject targetTileObject) {
                canTakeRepairJob = targetTileObject.canBeRepaired;
            }
        }
        return canTakeRepairJob /*&& character.characterClass.CanDoJob(JOB_TYPE.REPAIR)*/;
    }
    public bool CanCharacterTakeRepairJob(Character character, TileObject targetTileObject) {
        return targetTileObject.canBeRepaired /*&& character.characterClass.CanDoJob(JOB_TYPE.REPAIR)*/;
    }
    public bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter) {
        if (targetCharacter.traitContainer.HasTrait("Restrained")) {
            return false;
        }
        if (targetCharacter.isAlliedWithPlayer) {
            //if target character is allied with player, only take restrain job if character is not allied with player
            return character.isAlliedWithPlayer == false;  
        }
        return true; //character.characterClass.CanDoJob(JOB_TYPE.RESTRAIN);
    }
    public bool CanCharacterTakeRepairStructureJob(Character character) {
        return true; //character.characterClass.CanDoJob(JOB_TYPE.REPAIR);
    }
    //public bool CanCharacterTakeJoinPartyJob(Character character, Character targetCharacter) {
    //    Party partyToJoin = targetCharacter.partyComponent.currentParty;
    //    return !character.partyComponent.hasParty && partyToJoin != null && !partyToJoin.isWaitTimeOver && !partyToJoin.isDisbanded && partyToJoin.IsAllowedToJoin(character);
    //}
    public bool CanCharacterTakeExterminateJob(Character character) {
        Party partyToJoin = character.partyComponent.currentParty;
        return !character.partyComponent.hasParty;
    }
    public bool CanCharacterTakeRaidJob(Character character) {
        Party partyToJoin = character.partyComponent.currentParty;
        return !character.partyComponent.hasParty;
    }
    public bool CanCharacterTakeCounterattackPartyJob(Character character) {
        Party partyToJoin = character.partyComponent.currentParty;
        return !character.partyComponent.hasParty;
    }
    public bool CanCharacterTakeHuntHeirloomJob(Character character) {
        Party partyToJoin = character.partyComponent.currentParty;
        return !character.partyComponent.hasParty;
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