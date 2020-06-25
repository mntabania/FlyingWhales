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
        log += $"\n-{character.name} is berserked";
        if (!character.isInCombat) {
            log += $"\n-{character.name} is not in combat will try to attack nearby characters/objects";
            bool hasCreatedJob = false;
            for (int i = 0; i < character.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest inVisionPOI = character.marker.inVisionPOIs[i];
                if (inVisionPOI is MovingTileObject || inVisionPOI is GenericTileObject || (inVisionPOI.mapObjectVisual != null && inVisionPOI.mapObjectVisual.IsInvisibleToPlayer())) {
                    //should not target moving tile objects and generic tile objects
                    continue;
                }
                if(!character.combatComponent.hostilesInRange.Contains(inVisionPOI) && !character.combatComponent.avoidInRange.Contains(inVisionPOI)) {
                    if (inVisionPOI is Character targetCharacter) {
                        if (!targetCharacter.isDead) {
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
                log += $"\n-{character.name} did not create berserk attack job, will stroll instead";
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
