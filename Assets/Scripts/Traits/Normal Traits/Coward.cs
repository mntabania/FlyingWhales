using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

namespace Traits {
    public class Coward : Trait {
        public override bool isSingleton => true;

        public Coward() {
            name = "Coward";
            description = "A scaredy-cat. Will often flee from combat. If afflicted by the player, will produce a Chaos Orb each time it flees from combat.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character targetCharacter) {
                bool notFactionmate = targetCharacter.faction != characterThatWillDoJob.faction || targetCharacter.faction == null;
                bool noRelationship = !characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter);
                if(notFactionmate && noRelationship) {
                    if (characterThatWillDoJob.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.COWARDICE)) {
                        if (PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.COWARDICE).currentLevel >= 3) {
                            characterThatWillDoJob.combatComponent.hostilesInRange.Remove(targetCharacter);
                            characterThatWillDoJob.combatComponent.avoidInRange.Remove(targetCharacter);
                            if (characterThatWillDoJob.combatComponent.Flight(targetCharacter, CombatManager.Coward)) {
                                characterThatWillDoJob.ForceCancelAllJobsTargetingPOI(targetCharacter, "actor fled");
                            }
                        }
                    }
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override string TriggerFlaw(Character character) {
            //If outside and the character lives in a house, the character will flee and go back home.
            string successLogKey = base.TriggerFlaw(character);
            if (character.homeStructure != null && !character.homeStructure.hasBeenDestroyed && character.homeStructure.tiles.Count > 0) {
                if (character.currentStructure != character.homeStructure) {
                    if (character.currentActionNode != null) {
                        character.StopCurrentActionNode();
                    }
                    if (character.stateComponent.currentState != null) {
                        character.stateComponent.ExitCurrentState();
                    } 

                    ActualGoapNode node = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], character, character, null, 0);
                    GoapPlan goapPlan = ObjectPoolManager.Instance.CreateNewGoapPlan(node, character);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.RETURN_HOME, character, character);
                    goapPlan.SetDoNotRecalculate(true);
                    job.SetCannotBePushedBack(true);
                    job.SetAssignedPlan(goapPlan);
                    character.jobQueue.AddJobInQueue(job);
                    return successLogKey;
                } else {
                    return "fail_at_home";
                }
            } else {
                return "fail_no_home";
            }
        }
        #endregion

        public bool TryActivatePassOut(Character p_character) {
            if (GameUtilities.RollChance(20)) {
                bool activatePassOut = p_character.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.COWARDICE) && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.COWARDICE).currentLevel >= 2;
                if (activatePassOut) {
                    return p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Pass_Out, p_character, "coward");
                }
            }
            return false;
        }
    }
}