using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Lazy : Trait {
        public Character owner { get; private set; }

        public Lazy() {
            name = "Lazy";
            description = "Would rather loaf around than work. If afflicted by the player, will produce a Chaos Orb each time it starts feeling lazy.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Per_Tick_While_Stationary_Unoccupied);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is Character character) {
                owner = character;
            }
        }
        public override bool PerTickWhileStationaryOrUnoccupied(Character p_character) {
            if (p_character.HasAfflictedByPlayerWith(this)) {
                if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.LAZINESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Likes_To_Sleep) ||
                    PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.LAZINESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Loves_To_Sleep)) {
                    SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.LAZINESS);
                    PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.LAZINESS);
                    bool wasChanceMet = ChanceData.RollChance(skillData.currentLevel == 2 ? CHANCE_TYPE.Laziness_Nap_Level_2 : CHANCE_TYPE.Laziness_Nap_Level_3);
                    if (wasChanceMet && !p_character.jobQueue.HasJob(JOB_TYPE.LAZY_NAP)) {
                        if (p_character.tileObjectComponent.primaryBed != null) {
                            p_character.PlanFixedJob(JOB_TYPE.LAZY_NAP, INTERACTION_TYPE.NAP, p_character.tileObjectComponent.primaryBed);    
                        } else {
                            p_character.PlanFixedJob(JOB_TYPE.LAZY_NAP, INTERACTION_TYPE.SLEEP_OUTSIDE, p_character);
                        }
                        return true;
                    }
                }
            }
            return base.PerTickWhileStationaryOrUnoccupied(p_character);
        }
        public override string TriggerFlaw(Character character) {
            //Will drop current action and will perform Happiness Recovery.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                if (character.currentActionNode != null) {
                    character.StopCurrentActionNode();
                }
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.ExitCurrentState();
                }

                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetTraitOrStatus<Heartbroken>("Heartbroken");
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < (25 * owner.traitContainer.stacks[heartbroken.name]);
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
                    }
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW,  new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), character, character);
                    UtilityScripts.JobUtilities.PopulatePriorityLocationsForHappinessRecovery(character, job);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            } else {
                return "has_trigger_flaw";
            }
            return base.TriggerFlaw(character);
        }
        #endregion

        public bool TriggerLazy() {
            if (owner.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Lazy, owner)) {
                if (owner.HasAfflictedByPlayerWith(this)) {
                    DispenseChaosOrbsForAffliction(owner, 1);
                }
                return true;
            }
            return false;
        }
        public bool TryIgnoreUrgentTask(JOB_TYPE job) {
            if (ChanceData.RollChance(CHANCE_TYPE.Ignore_Urgent_Task) && owner.HasAfflictedByPlayerWith(this) && 
                PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.LAZINESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Ignore_Urgent_Tasks)) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", "Lazy", "ignore_urgent_job", null, LOG_TAG.Work, LOG_TAG.Player);
                log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(job.ToString()), LOG_IDENTIFIER.STRING_1);
                owner.logComponent.RegisterLog(log);
                // PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                // PlayerManager.Instance.player.ShowNotificationFrom(owner, log);
                return true;
            }
            return false;
        }
        public float GetTriggerChance(Character p_character) {
            if (p_character.HasAfflictedByPlayerWith(this)) {
                PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.LAZINESS);
                SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.LAZINESS);
                return playerSkillData.afflictionUpgradeData.GetRateChancePerLevel(skillData.currentLevel);
            } else {
                PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.LAZINESS);
                return playerSkillData.afflictionUpgradeData.GetRateChancePerLevel(0);
            }
        }
    }
}

