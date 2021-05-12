﻿using UtilityScripts;

public class SuccubusBehaviour : BaseMonsterBehaviour {
	public SuccubusBehaviour() {
		priority = 8;
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a succubus";
        if (character.reactionComponent.disguisedCharacter != null) {
            if (character.isAtHomeStructure || character.IsInTerritory() || character.IsInHomeSettlement()) {
                if (character.previousCurrentActionNode != null && (character.previousCurrentActionNode.associatedJobType == JOB_TYPE.IDLE_RETURN_HOME || character.previousCurrentActionNode.associatedJobType == JOB_TYPE.RETURN_TERRITORY)) {
                    character.reactionComponent.SetDisguisedCharacter(null);
                    return true;
                }
            }
            if (character.previousCurrentActionNode != null && character.previousCurrentActionNode.action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                if (character.currentStructure != character.homeStructure && !character.IsInTerritory()) {
                    if (character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob)) {
                        return true;
                    }
                }
            } else {
                Character targetCharacter = character.currentRegion.GetRandomCharacterForSuccubusMakeLove(character);
                if (targetCharacter != null) {
                    log += $"\n-Target for make love is: " + targetCharacter.name;
                    if (character.movementComponent.HasPathToEvenIfDiffRegion(targetCharacter.gridTileLocation)) {
                        if (character.jobComponent.TriggerMakeLoveJob(targetCharacter, out producedJob)) {
                            return true;
                        }
                    } else {
                        log += $"\n-No path to target, drop disguise";
                        character.reactionComponent.SetDisguisedCharacter(null);
                        return true;
                    }
                } else {
                    log += $"\n-No Character to be targeted for make love, drop disguise";
                    character.reactionComponent.SetDisguisedCharacter(null);
                    return true;
                }
            }
        } else {
            if (character.isAtHomeStructure || character.IsInTerritory() || character.IsInHomeSettlement()) {
                log += $"\n-Character is in home and not in disguise, 1% chance to disguise";
                int roll = UnityEngine.Random.Range(0, 100);
                log += "\nRoll: " + roll;
                if (roll < 1) { //1
                    Character targetCharacter = character.currentRegion.GetRandomCharacterThatIsFemaleVillagerAndNotDead();
                    if (targetCharacter != null) {
                        log += $"\n-Target for disguise is: " + targetCharacter.name;
                        if (character.currentRegion.GetRandomCharacterForSuccubusMakeLove(character) != null) {
                            if (character.jobComponent.TriggerDisguiseJob(targetCharacter, out producedJob)) {
                                return true;
                            }
                        }
                    }
                }
            } else {
                log += $"\n-Character is not in home and not in disguise, will return home";
                if (character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob)) {
                    return true;
                }
            }
        }
        log += $"\n-Character will roam";
        character.jobComponent.TriggerRoamAroundTile(out producedJob);
        return true;
	}
    protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        if (DisguisedBehaviourTamed(p_character, ref p_log, out p_producedJob)) {
            return true;
        } else if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob)) {
            return true;
        } else {
            if (GameUtilities.RollChance(5)) {
                if (p_character.currentRegion.GetRandomCharacterThatIsMaleVillagerAndNotDeadAndFactionIsNotTheSameAs(p_character) != null) {
                    Character targetCharacter = p_character.currentRegion.GetRandomCharacterThatIsFemaleVillagerAndNotDead();
                    if (targetCharacter != null) {
                        p_log += $"\n-Target for disguise is: " + targetCharacter.name;
                        if (p_character.currentRegion.GetRandomCharacterForSuccubusMakeLoveTamed(p_character) != null) {
                            if (p_character.jobComponent.TriggerDisguiseJob(targetCharacter, out p_producedJob)) {
                                return true;
                            }
                        }
                    }
                }
            }
            return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
        }
    }
    private bool CanTargetCharacterForMakeLoveTamed(Character source, Character c) {
        if(c.gender == GENDER.MALE && !c.isDead && (source.tileObjectComponent.primaryBed != null || c.tileObjectComponent.primaryBed != null) && c.homeSettlement != null && !c.partyComponent.isActiveMember && c.faction != source.faction && c.homeRegion == source.currentRegion) {
            if(c.limiterComponent.canPerform && !c.combatComponent.isInCombat && !c.hasBeenRaisedFromDead && !c.carryComponent.masterCharacter.movementComponent.isTravellingInWorld && c.currentRegion == source.currentRegion) {
                return c.homeSettlement.GetFirstBuiltBedThatIsAvailableAndNoActiveUsers() != null;
            }
        }
        return false;
    }

    private bool DisguisedBehaviourTamed(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.reactionComponent.disguisedCharacter != null) {
            if (character.isAtHomeStructure || character.IsInTerritory() || character.IsInHomeSettlement()) {
                if (character.previousCurrentActionNode != null && (character.previousCurrentActionNode.associatedJobType == JOB_TYPE.IDLE_RETURN_HOME || character.previousCurrentActionNode.associatedJobType == JOB_TYPE.RETURN_TERRITORY)) {
                    character.reactionComponent.SetDisguisedCharacter(null);
                    return true;
                }
            }
            if (character.previousCurrentActionNode != null && character.previousCurrentActionNode.action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                if (character.currentStructure != character.homeStructure && !character.IsInTerritory()) {
                    if (character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob)) {
                        return true;
                    }
                }
            } else {
                Character targetCharacter = character.currentRegion.GetRandomCharacterForSuccubusMakeLoveTamed(character);
                if (targetCharacter != null) {
                    log += $"\n-Target for make love is: " + targetCharacter.name;
                    if (character.movementComponent.HasPathToEvenIfDiffRegion(targetCharacter.gridTileLocation)) {
                        if (character.jobComponent.TriggerMakeLoveJob(targetCharacter, out producedJob)) {
                            return true;
                        }
                    } else {
                        log += $"\n-No path to target, drop disguise";
                        character.reactionComponent.SetDisguisedCharacter(null);
                        return true;
                    }
                } else {
                    log += $"\n-No Character to be targeted for make love, drop disguise";
                    character.reactionComponent.SetDisguisedCharacter(null);
                    return true;
                }
            }
            return true;
        }
        return false;
    }
}
