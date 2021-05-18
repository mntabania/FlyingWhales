using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkBehaviour : CharacterBehaviourComponent {
    public BerserkBehaviour() {
        priority = 1085;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
#if DEBUG_LOG
        log += $"\n-{character.name} is berserked";
#endif
        if (!character.combatComponent.isInCombat) {
#if DEBUG_LOG
            log += $"\n-{character.name} is not in combat will try to attack nearby characters/objects";
#endif
            bool hasCreatedJob = false;
            for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest inVisionPOI = character.marker.inVisionPOIs[i];
                if (inVisionPOI is MovingTileObject || inVisionPOI is GenericTileObject || inVisionPOI is StructureTileObject 
                    || (inVisionPOI.mapObjectVisual != null && inVisionPOI.mapObjectVisual.IsInvisibleToPlayer()) || inVisionPOI.gridTileLocation == null
                    || inVisionPOI.traitContainer.HasTrait("Hibernating", "Indestructible")) {
                    //should not target moving tile objects and generic tile objects and structure tile objects
                    continue;
                }
                if(!character.movementComponent.HasPathToEvenIfDiffRegion(inVisionPOI.gridTileLocation)) {
                    //Should not attack objects that the character cannot reach
                    continue;
                }
                if(!character.combatComponent.IsHostileInRange(inVisionPOI) && !character.combatComponent.IsAvoidInRange(inVisionPOI)) {
                    if (inVisionPOI is Character targetCharacter) {
                        //Added checker for "Unconscious", "Paralyzed", "Restrained" because since berserk is no longer lethal.
                        //Because if we didn't limit this, it can cause berserked characters becoming stuck trying to attack a character that they can no longer attack because of the
                        //change in lethality (Non lethal combat will stop if the target becomes either "Unconscious", "Paralyzed" or "Restrained")
                        if (!targetCharacter.isDead && !targetCharacter.traitContainer.HasTrait("Unconscious", "Paralyzed", "Restrained")) {
                            producedJob = CreateBerserkAttackJob(character, targetCharacter);
                            if (producedJob != null) {
                                hasCreatedJob = true;
                                //character.combatComponent.Fight(targetPOI, CombatManager.Berserked, isLethal: false);
                                break;
                            }
                            //if (character.faction.isPlayerFaction) {
                            //    character.jobComponent.CreateBerserkAttackJob(targetCharacter);
                            //    //character.combatComponent.Fight(targetCharacter, CombatManager.Berserked, isLethal: true); //check hostility if from player faction, so as not to attack other characters that are also from the same faction.
                            //    break;
                            //} else {
                            //    if (!targetCharacter.traitContainer.HasTrait("Unconscious")) {
                            //        //character.combatComponent.Fight(targetCharacter, CombatManager.Berserked, isLethal: false);
                            //        break;
                            //    }
                            //}
                        }
                    } else if (inVisionPOI is TileObject targetPOI) { // || targetPOI is SpecialToken
                        if (Random.Range(0, 100) < 35) {
                            //character.jobComponent.TriggerDestroy(targetPOI);
                            producedJob = CreateBerserkAttackJob(character, targetPOI);
                            if (producedJob != null) {
                                hasCreatedJob = true;
                                //character.combatComponent.Fight(targetPOI, CombatManager.Berserked, isLethal: false);
                                break;
                            }
                        }
                    }
                }
            }
            if (!hasCreatedJob) {
#if DEBUG_LOG
                log += $"\n-{character.name} did not create berserk attack job, will stroll instead";
#endif
                character.jobComponent.PlanIdleBerserkStrollOutside(out producedJob);
            }
        }
        return true;
    }
    
    private GoapPlanJob CreateBerserkAttackJob(Character character, IPointOfInterest targetPOI) {
        if (!character.jobQueue.HasJob(JOB_TYPE.BERSERK_ATTACK, targetPOI)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BERSERK_ATTACK, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetPOI, character);
            return job;
        }
        return null;
    }
}
