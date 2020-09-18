using System.Linq;

namespace Traits {
    public class Agoraphobic : Trait {
        public override bool isSingleton => true;

        public bool hasReactedThisTick;
        
        public Agoraphobic() {
            name = "Agoraphobic";
            description = "Crowds? Oh no!";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            hasReactedThisTick = false;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                ApplyAgoraphobicEffect(character);
                //if (character.marker.inVisionCharacters.Count >= 3) {
                //    ApplyAgoraphobicEffect(character, true);
                //}
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                // //Character targetCharacter = targetPOI as Character;
                // if (characterThatWillDoJob.traitContainer.HasTrait("Berserked")) {
                //     return false;
                // }
                // ApplyAgoraphobicEffect(characterThatWillDoJob);
                if (hasReactedThisTick) {
                    return false;
                }
                if (characterThatWillDoJob.canWitness && characterThatWillDoJob.marker.inVisionCharacters.Count(x => x.isNormalCharacter && x.isDead == false) >= 3) {
                    string debugLog = $"{characterThatWillDoJob.name} Is agoraphobic and has 3+ alive villagers in vision. Character became anxious.";
                    characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Anxious");
                    int roll = UnityEngine.Random.Range(0, 100);
                    if (roll < 10) {
                        debugLog += $"{characterThatWillDoJob.name} became catatonic";
                        characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Catatonic");
                    } else if (roll < 25) {
                        debugLog += $"{characterThatWillDoJob.name} became berserked";
                        characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Berserked");
                    } else if (roll < 40) {
                        debugLog += $"{characterThatWillDoJob.name} Had a seizure";
                        characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, characterThatWillDoJob);
                    } else if (roll < 50 && (characterThatWillDoJob.characterClass.className == "Druid" || characterThatWillDoJob.characterClass.className == "Shaman" || characterThatWillDoJob.characterClass.className == "Mage")) {
                        debugLog += $"{characterThatWillDoJob.name} Had a loss of control";
                        characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Loss_Of_Control, characterThatWillDoJob);
                    } else {
                        debugLog += $"{characterThatWillDoJob.name} became anxious and is cowering.";
                        characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, characterThatWillDoJob, reason: "Agoraphobic");
                    }
                    characterThatWillDoJob.logComponent.PrintLogIfActive(debugLog);
                    Log log = new Log(GameManager.Instance.Today(), "Trait", "Agoraphobic", "on_see_first");
                    log.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddLogToDatabase();
                    hasReactedThisTick = true;

                    GameDate date = GameManager.Instance.Today();
                    date.AddTicks(1);
                    SchedulingManager.Instance.AddEntry(date, () => hasReactedThisTick = false, this);
                }
                return true;
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override string TriggerFlaw(Character character) {
            //If outside and the character lives in a house, the character will flee and go back home.
            //string successLogKey = base.TriggerFlaw(character);
            //if (character.homeStructure != null) {
            //    if (character.currentStructure != character.homeStructure) {
            //        if (character.currentActionNode != null) {
            //            character.StopCurrentActionNode(false);
            //        }
            //        if (character.stateComponent.currentState != null) {
            //            character.stateComponent.ExitCurrentState();
            //        }
            //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], character, character, null, 0);
            //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, character);
            //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.RETURN_HOME, character, character);
            //        goapPlan.SetDoNotRecalculate(true);
            //        job.SetCannotBePushedBack(true);
            //        job.SetAssignedPlan(goapPlan);
            //        character.jobQueue.AddJobInQueue(job);
            //        return successLogKey;
            //    } else {
            //        return "fail_at_home";
            //    }
            //} else {
            //    return "fail_no_home";
            //}
            ApplyAgoraphobicEffect(character, JOB_TYPE.TRIGGER_FLAW);
            return base.TriggerFlaw(character);

        }
        #endregion

        private bool ApplyAgoraphobicEffect(Character character, JOB_TYPE jobType = JOB_TYPE.FLEE_TO_HOME/*, bool processCombat*/) {
            if (!character.canPerform || !character.canWitness) {
                return false;
            }
            if(!WillTriggerAgoraphobia(character)) {
                return false;
            }
            character.StopCurrentActionNode(false);
            character.jobQueue.CancelAllJobs();
            character.traitContainer.AddTrait(character, "Anxious");
            if(character.homeStructure != null && character.currentStructure != character.homeStructure) {
                if (!character.jobComponent.TriggerFleeHome(jobType)) {
                    character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: "Agoraphobic");
                }
            } else {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: "Agoraphobic");
            }
            return true;
            //character.marker.AddAvoidsInRange(character.marker.inVisionCharacters, processCombat, "agoraphobia");
            //character.needsComponent.AdjustHappiness(-50);
            //character.needsComponent.AdjustTiredness(-150);
        }
        private bool WillTriggerAgoraphobia(Character character) {
            int count = 0;
            if (character.marker.inVisionCharacters.Count >= 3) {
                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                    if (!character.marker.inVisionCharacters[i].isDead) {
                        count++;
                    }
                }
            }
            return count >= 3;
        }
    }
}

