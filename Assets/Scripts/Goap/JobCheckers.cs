﻿using Traits;

public partial class InteractionManager {
    
    public bool CanDoPatrol(Character character) {
        return character.canCombat;
    }
    public bool IsSuicideJobStillValid(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Forlorn") != null;
    }
    public bool CanMoveOut(Character character) {
        TIME_IN_WORDS time = TIME_IN_WORDS.MORNING;
        if (character.traitContainer.GetNormalTrait<Trait>("Nocturnal") != null) {
            //if nocturnal get after midnight
            time = TIME_IN_WORDS.AFTER_MIDNIGHT;
        }
        return character.traitContainer.GetNormalTrait<Trait>("Leader") != null /*character.role.roleType != CHARACTER_ROLE.LEADER*/ &&
               GameManager.GetTimeInWordsOfTick(GameManager.Instance.tick) ==
               time; //Only non-leaders can take move out job, and it must also be in the morning time.
    }
    public bool CanDoCraftFurnitureJob(Character character, JobQueueItem item) {
        TILE_OBJECT_TYPE furnitureToCreate = ((item as GoapPlanJob).targetPOI as TileObject).tileObjectType;
        return furnitureToCreate.CanBeCraftedBy(character);
    }
    public bool CanDoDestroyProfaneJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Combatant") != null;
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoCombatJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Combatant") != null;
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoObtainFoodOutsideJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Worker") != null;
        //return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanDoObtainSupplyOutsideJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Worker") != null;
        //return character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    public bool CanDoHolyIncantationJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Combatant") != null;
        //return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanDoExploreJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Combatant") != null;
        //return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanDoCleanseRegionJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Purifier") != null;
//        return true;
    }
    public bool CanDoClaimRegionJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Royalty") != null;
    }
    public bool CanDoInvadeRegionJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Raider") != null;
    }
    public bool CanDoAttackNonDemonicRegionJob(Character character) {
        return character.characterClass.isNonCombatant == false || character.characterClass.className.Equals("Noble") || character.characterClass.className.Equals("Leader");
    }
    public bool CanDoAttackDemonicRegionJob(Character character) {
        return character.characterClass.isNonCombatant == false || character.characterClass.className.Equals("Noble") || character.characterClass.className.Equals("Leader");
    }
    public bool CanDoJudgementJob(Character character) {
        return character.isSettlementRuler || character.isFactionLeader || character.characterClass.className == "Noble" || character.traitContainer.GetNormalTrait<Trait>("Combatant") != null;
        //return character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER;
    }
    public bool CanDoSabotageFactionJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Cultist") != null;
    }
    public bool CanCraftTool(Character character) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.TOOL);
        return SPECIAL_TOKEN.TOOL.CanBeCraftedBy(character);
    }
    public bool CanDoObtainSupplyJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Combatant") != null;
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoProduceWoodJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Logger") != null;
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanDoProduceMetalJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Miner") != null;
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    public bool CanCharacterTakeBuildGoddessStatueJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Builder") != null;
    }
    public bool CanBrewPotion(Character character) {
        //return character.HasExtraTokenInInventory(SPECIAL_TOKEN.HEALING_POTION);
        return SPECIAL_TOKEN.HEALING_POTION.CanBeCraftedBy(character);
    }
    public bool CanTakeBuryJob(Character character) {
        if (!character.isCriminal && character.isAtHomeRegion &&
            character.isPartOfHomeFaction
            && character.traitContainer.GetNormalTrait<Trait>("Beast") == null /*character.role.roleType != CHARACTER_ROLE.BEAST*/) {
            return character.traitContainer.GetNormalTrait<Trait>("Worker", "Combatant") != null;
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
                       !targetCharacter.opinionComponent.IsEnemiesWith(character);
            }
            return !character.opinionComponent.IsEnemiesWith(targetCharacter);
        }
        return false;
    }
    public bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter) {
        if (character.isAtHomeRegion && !character.isCriminal &&
            character.traitContainer.GetNormalTrait<Trait>("Coward") == null && character.homeSettlement != null && character.homeSettlement.prison != null) {
            Restrained restrainedTrait = targetCharacter.traitContainer.GetNormalTrait<Trait>("Restrained") as Restrained;
            if (restrainedTrait == null || !restrainedTrait.isPrisoner) {
                return character.traitContainer.GetNormalTrait<Trait>("Combatant") != null /*character.role.roleType == CHARACTER_ROLE.SOLDIER*/ &&
                   character.opinionComponent.GetRelationshipEffectWith(targetCharacter) !=
                   RELATIONSHIP_EFFECT.POSITIVE;
            }
        }
        return false;
    }
    public bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter) {
        return character.faction != targetCharacter.faction
            && character.faction.GetRelationshipWith(targetCharacter.faction).relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE 
            && character.isAtHomeRegion
            && character.isPartOfHomeFaction && character.currentSettlement.prison != null
            //&& (character.role.roleType == CHARACTER_ROLE.SOLDIER ||
            //character.role.roleType == CHARACTER_ROLE.CIVILIAN ||
            //character.role.roleType == CHARACTER_ROLE.ADVENTURER)
            && character.traitContainer.GetNormalTrait<Trait>("Worker", "Combatant") != null
            && character.opinionComponent.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE 
            && !character.isCriminal
            && targetCharacter.traitContainer.GetNormalTrait<Trait>("Restrained") == null;
    }
    public bool CanCharacterTakeRepairJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Worker", "Combatant") != null;
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
        //       character.role.roleType == CHARACTER_ROLE.CIVILIAN ||
        //       character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    public bool CanCharacterTakeReplaceTileObjectJob(Character character, JobQueueItem job) {
        object[] otherData = (job as GoapPlanJob).otherData[INTERACTION_TYPE.REPLACE_TILE_OBJECT];
        TileObject removedObj = otherData[0] as TileObject;
        return removedObj.tileObjectType.CanBeCraftedBy(character);
    }
    public bool CanCharacterTakeParalyzedFeedJob(Character sourceCharacter, Character character) {
        return sourceCharacter != character && sourceCharacter.faction == character.faction &&
               sourceCharacter.opinionComponent.GetRelationshipEffectWith(character) !=
               RELATIONSHIP_EFFECT.NEGATIVE;
    }
    public bool CanCharacterTakeRestrainedFeedJob(Character sourceCharacter, Character character) {
        if (sourceCharacter.currentRegion.IsResident(character)) {
            if (!character.isFactionless) {
                return character.traitContainer.GetNormalTrait<Trait>("Worker", "Combatant") != null;
                //return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
                //       character.role.roleType == CHARACTER_ROLE.CIVILIAN;
            }
            else {
                return character.traitContainer.GetNormalTrait<Trait>("Beast") == null /*character.role.roleType != CHARACTER_ROLE.BEAST*/ &&
                       sourceCharacter.currentStructure.structureType.IsOpenSpace();
            }
        }
        return false;
    }
    public bool CanCharacterTakeDropJob(Character sourceCharacter, Character character) {
        return sourceCharacter != character && sourceCharacter.faction == character.faction &&
               character.opinionComponent.GetRelationshipEffectWith(sourceCharacter) !=
               RELATIONSHIP_EFFECT.NEGATIVE;
    }
    public bool CanCharacterTakeKnockoutJob(Character character, Character targetCharacter) {
        return character.traitContainer.GetNormalTrait<Trait>("Combatant") != null;
        //return character.role.roleType == CHARACTER_ROLE.SOLDIER ||
        //       character.role.roleType == CHARACTER_ROLE.ADVENTURER; // && !HasRelationshipOfEffectWith(targetCharacter, TRAIT_EFFECT.POSITIVE)
    }
    public bool CanCharacterTakeBuildJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Builder") != null;
    }
    public bool CanCharacterTakeRepairStructureJob(Character character) {
        return character.traitContainer.GetNormalTrait<Trait>("Builder") != null;
    }


    #region Job Applicability
    public bool IsJudgementJobStillApplicable(Character criminal) {
        if (criminal.isDead) {
            //Character is dead
            return false;
        }
        if (criminal.currentStructure != criminal.currentSettlement.prison) {
            //Character is no longer in jail
            return false;
        }
        if (criminal.traitContainer.GetNormalTrait<Trait>("Restrained") == null) {
            //Character is no longer restrained
            return false;
        }
        return true;
    }
    #endregion
}