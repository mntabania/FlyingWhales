using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Traits;

public class SuccubusBehaviour : CharacterBehaviourComponent {
	public SuccubusBehaviour() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
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
                    if (character.jobComponent.PlanIdleReturnHome(out producedJob)) {
                        return true;
                    }
                }
            } else {
                Character targetCharacter = character.currentRegion.GetRandomCharacterThatMeetCriteria((c) => CanTargetCharacterForMakeLove(character, c));
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
                if (roll < 1) {
                    Character targetCharacter = character.currentRegion.GetRandomCharacterThatMeetCriteria(c => !c.isDead && c.isNormalCharacter && c.gender == GENDER.FEMALE);
                    if (targetCharacter != null) {
                        log += $"\n-Target for disguise is: " + targetCharacter.name;
                        if (character.currentRegion.GetRandomCharacterThatMeetCriteria((c) => CanTargetCharacterForMakeLove(character, c)) != null) {
                            if (character.jobComponent.TriggerDisguiseJob(targetCharacter, out producedJob)) {
                                return true;
                            }
                        }
                    }
                }
            } else {
                log += $"\n-Character is not in home and not in disguise, will return home";
                if (character.jobComponent.PlanIdleReturnHome(out producedJob)) {
                    return true;
                }
            }
        }
        log += $"\n-Character will roam";
        character.jobComponent.TriggerRoamAroundTile(out producedJob);
        return true;
	}

    private bool CanTargetCharacterForMakeLove(Character source, Character c) {
        if(c.gender == GENDER.MALE && !c.isDead && (source.tileObjectComponent.primaryBed != null || c.tileObjectComponent.primaryBed != null) && c.homeSettlement != null) {
            if(c.limiterComponent.canPerform && !c.combatComponent.isInCombat && !c.raisedFromDeadAsSkeleton && !c.carryComponent.masterCharacter.movementComponent.isTravellingInWorld && c.currentRegion == source.currentRegion) {
                return c.homeSettlement.GetFirstTileObjectOfTypeThatMeetCriteria<Bed>(b => b.mapObjectState == MAP_OBJECT_STATE.BUILT && b.IsAvailable() && b.GetActiveUserCount() == 0) != null;
            }
        }
        return false;
    }
}
