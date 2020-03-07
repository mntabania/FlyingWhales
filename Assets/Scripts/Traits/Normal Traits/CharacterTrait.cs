using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    //This trait is present in all characters
    //A dummy trait in order for some jobs to be created
    public class CharacterTrait : Trait {
        //IMPORTANT NOTE: When the owner of this trait changed its alter ego, this trait will not be present in the alter ego anymore
        //Meaning that he/she cannot do the things specified in here anymore unless he/she switch to the ego which this trait is present
        public List<TileObject> alreadyInspectedTileObjects { get; private set; }
        public List<Character> charactersAlreadySawForHope { get; private set; }
        public bool hasSurvivedApprehension { get; private set; } //If a criminal character (is in original alter ego), and survived being apprehended, this must be turned on
        public Character owner { get; private set; }

        public CharacterTrait() {
            name = "Character Trait";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            alreadyInspectedTileObjects = new List<TileObject>();
            charactersAlreadySawForHope = new List<Character>();
        }
        public void AddAlreadyInspectedObject(TileObject to) {
            if (!alreadyInspectedTileObjects.Contains(to)) {
                alreadyInspectedTileObjects.Add(to);
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
        }

        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is TileObject tileObj) {
                if (tileObj.isSummonedByPlayer && !characterThatWillDoJob.traitContainer.HasTrait("Suspicious") && !alreadyInspectedTileObjects.Contains(tileObj)) {
                    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.INSPECT, tileObj)) {
                        GoapPlanJob inspectJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.INSPECT, tileObj, characterThatWillDoJob);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(inspectJob);
                        return true;
                    }
                }
            }
            if (targetPOI is TileObject item) {
                if (!characterThatWillDoJob.traitContainer.HasTrait("Beast") /*characterThatWillDoJob.role.roleType != CHARACTER_ROLE.BEAST*/) {
                    if (item.CanBePickedUpNormallyUponVisionBy(characterThatWillDoJob)
                        && !characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.PICK_UP)) {
                        characterThatWillDoJob.jobComponent.CreatePickUpJob(item);
                        return true;
                    }
                }
            }
            if (targetPOI is Character || targetPOI is Tombstone) {
                Character targetCharacter = null;
                if (targetPOI is Character character) {
                    targetCharacter = character;
                } else {
                    targetCharacter = (targetPOI as Tombstone).character;
                }
                if (targetCharacter.isDead) {
                    Dead deadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
                    if (deadTrait != null && deadTrait.responsibleCharacter != characterThatWillDoJob 
                                          && !deadTrait.charactersThatSawThisDead.Contains(characterThatWillDoJob)) {
                        deadTrait.AddCharacterThatSawThisDead(characterThatWillDoJob);
                    
                        // Log sawDeadLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "saw_dead");
                        // sawDeadLog.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        // sawDeadLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        // characterThatWillDoJob.logComponent.AddHistory(sawDeadLog);
                        // PlayerManager.Instance.player.ShowNotificationFrom(sawDeadLog, characterThatWillDoJob, false);
                        //
                        //
                        // if (characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER)) {
                        //     characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Heartbroken");
                        //     bool hasCreatedJob = RandomizeBetweenShockAndCryJob(characterThatWillDoJob);
                        //     //characterThatWillDoJob.needsComponent.AdjustHappiness(-6000);
                        //     return hasCreatedJob;
                        // } else if (characterThatWillDoJob.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.RELATIVE)) {
                        //     characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Griefstricken");
                        //     bool hasCreatedJob = RandomizeBetweenShockAndCryJob(characterThatWillDoJob);
                        //     //characterThatWillDoJob.needsComponent.AdjustHappiness(-4000);
                        //     return hasCreatedJob;
                        // } else if (characterThatWillDoJob.opinionComponent.IsFriendsWith(targetCharacter)) {
                        //     characterThatWillDoJob.traitContainer.AddTrait(characterThatWillDoJob, "Griefstricken");
                        //     bool hasCreatedJob = CreatePrioritizedShockJob(characterThatWillDoJob);
                        //     //characterThatWillDoJob.needsComponent.AdjustHappiness(-2000);
                        //     return hasCreatedJob;
                        // }
                    }
                } else { 
                    //character is not dead
                    // if (targetCharacter.canMove == false || targetCharacter.canWitness == false) {
                    //     if (characterThatWillDoJob.jobComponent.TryTriggerFeed(targetCharacter) == false) {
                    //         if (characterThatWillDoJob.jobComponent.TryTriggerMoveCharacterTirednessRecovery(targetCharacter) == false) {
                    //             characterThatWillDoJob.jobComponent.TryTriggerMoveCharacterHappinessRecovery(targetCharacter);
                    //         }    
                    //     }
                    // }
                    if (targetCharacter.race == RACE.SKELETON || targetCharacter.characterClass.className == "Zombie") {
                        string opinionLabel = characterThatWillDoJob.relationshipContainer.GetOpinionLabel(targetCharacter);
                        if (opinionLabel == OpinionComponent.Friend) {
                            if (!charactersAlreadySawForHope.Contains(targetCharacter)) {
                                charactersAlreadySawForHope.Add(targetCharacter);
                                characterThatWillDoJob.needsComponent.AdjustHope(-5f);
                            }
                        } else if (opinionLabel == OpinionComponent.Close_Friend) {
                            if (!charactersAlreadySawForHope.Contains(targetCharacter)) {
                                charactersAlreadySawForHope.Add(targetCharacter);
                                characterThatWillDoJob.needsComponent.AdjustHope(-10f);
                            }
                        }
                    }
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
            if(node.action.goapType == INTERACTION_TYPE.MAKE_LOVE) {
                bool triggered = node.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Invite_To_Make_Love, node.poiTarget);
                willStillContinueAction = node.actor.interruptComponent.identifier == "Accept";
                node.actor.interruptComponent.SetIdentifier(string.Empty);
                return triggered;
            }
            return false;
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            if (hasSurvivedApprehension) {
                CheckAsCriminal();
            }
        }
        #endregion

        private void CheckAsCriminal() {
            if (owner.stateComponent.currentState == null && !owner.isAtHomeRegion && !owner.jobQueue.HasJob(JOB_TYPE.IDLE_RETURN_HOME, INTERACTION_TYPE.RETURN_HOME)) {
                if (owner.jobQueue.jobsInQueue.Count > 0) {
                    owner.CancelAllJobs();
                }
                // CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.RETURN_HOME, CHARACTER_STATE.MOVE_OUT, owner);
                // owner.jobQueue.AddJobInQueue(job);
                owner.PlanIdleReturnHome();
            } else if (owner.isAtHomeRegion) {
                SetHasSurvivedApprehension(false);
            }
        }

        public void SetHasSurvivedApprehension(bool state) {
            if (hasSurvivedApprehension != state) {
                hasSurvivedApprehension = state;
                //if (hasSurvivedApprehension) {
                //    Messenger.AddListener(Signals.TICK_STARTED, CheckAsCriminal);
                //} else {
                //    if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
                //        Messenger.RemoveListener(Signals.TICK_STARTED, CheckAsCriminal);
                //    }
                //}
            }
        }

        private bool RandomizeBetweenShockAndCryJob(Character characterThatWillDoJob) {
            if (UnityEngine.Random.Range(0, 2) == 0) {
                return CreatePrioritizedShockJob(characterThatWillDoJob);
            } else {
                return CreatePrioritizedCryJob(characterThatWillDoJob);
            }
        }
        private bool CreatePrioritizedShockJob(Character characterThatWillDoJob) {
            //if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.SHOCK)) {
            //    GoapPlanJob shockJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.SHOCK, characterThatWillDoJob, characterThatWillDoJob);
            //    characterThatWillDoJob.jobQueue.AddJobInQueue(shockJob);
            //    return true;
            //}
            //return false;
            return characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, characterThatWillDoJob);
        }
        private bool CreatePrioritizedCryJob(Character characterThatWillDoJob) {
            if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.CRY)) {
                GoapPlanJob cryJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.CRY, characterThatWillDoJob, characterThatWillDoJob);
                characterThatWillDoJob.jobQueue.AddJobInQueue(cryJob);
                return true;
            }
            return false;
        }
        private bool CreateLaughAtJob(Character characterThatWillDoJob, Character target) {
            //if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.LAUGH_AT)) {
            //    GoapPlanJob laughJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.LAUGH_AT, target, characterThatWillDoJob);
            //    characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
            //    return true;
            //}
            //return false;
            return characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, target);
        }
        private bool CreateFeelingConcernedJob(Character characterThatWillDoJob, Character target) {
            return characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Concerned, target);
            //if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_CONCERNED)) {
            //    GoapPlanJob laughJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_CONCERNED, target, characterThatWillDoJob);
            //    characterThatWillDoJob.jobQueue.AddJobInQueue(laughJob);
            //    return true;
            //}
            //return false;
        }
        private bool CreateMockJob(Character characterThatWillDoJob, Character target) {
            return characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, target);
        }
    }

    public class SaveDataCharacterTrait : SaveDataTrait {
        public List<TileObjectSerializableData> alreadyInspectedTileObjects;
        public bool hasSurvivedApprehension;

        public override void Save(Trait trait) {
            base.Save(trait);
            alreadyInspectedTileObjects = new List<TileObjectSerializableData>();
            CharacterTrait derivedTrait = trait as CharacterTrait;
            for (int i = 0; i < derivedTrait.alreadyInspectedTileObjects.Count; i++) {
                TileObject to = derivedTrait.alreadyInspectedTileObjects[i];
                TileObjectSerializableData toData = new TileObjectSerializableData {
                    id = to.id,
                    type = to.tileObjectType,
                };
                alreadyInspectedTileObjects.Add(toData);
            }
            hasSurvivedApprehension = derivedTrait.hasSurvivedApprehension;
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            CharacterTrait derivedTrait = trait as CharacterTrait;
            for (int i = 0; i < alreadyInspectedTileObjects.Count; i++) {
                TileObjectSerializableData toData = alreadyInspectedTileObjects[i];
                derivedTrait.AddAlreadyInspectedObject(InnerMapManager.Instance.GetTileObject(toData.type, toData.id));
            }
            derivedTrait.SetHasSurvivedApprehension(hasSurvivedApprehension);
            return trait;
        }
    }
}

