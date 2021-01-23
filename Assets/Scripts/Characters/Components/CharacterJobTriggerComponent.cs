using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine.Assertions;
using UtilityScripts;
using Crime_System;
using Random = UnityEngine.Random;

public class CharacterJobTriggerComponent : JobTriggerComponent {
	public Character owner { get; private set; }

    public JOB_TYPE primaryJob { get; private set; }
    public List<JOB_TYPE> priorityJobs { get; private set; }
    public Dictionary<INTERACTION_TYPE, int> numOfTimesActionDone { get; private set; }
    public List<JOB_TYPE> primaryJobCandidates { get; private set; }
    public List<string> obtainPersonalItemUnownedRandomList { get; private set; }
    public bool hasStartedScreamCheck { get; private set; }
    public bool doNotDoRecoverHPJob { get; private set; }
    public bool canReportDemonicStructure { get; private set; }
    public List<JOB_TYPE> additionalPriorityJobs { get; }

    public CharacterJobTriggerComponent() {
        canReportDemonicStructure = true;
        numOfTimesActionDone = new Dictionary<INTERACTION_TYPE, int>();
        primaryJobCandidates = new List<JOB_TYPE>();
        priorityJobs = new List<JOB_TYPE>();
        additionalPriorityJobs = new List<JOB_TYPE>();
        SetPrimaryJob(JOB_TYPE.NONE);
	}
    public CharacterJobTriggerComponent(SaveDataCharacterJobTriggerComponent data) {
        primaryJob = data.primaryJob;
        priorityJobs = data.priorityJobs;
        numOfTimesActionDone = data.numOfTimesActionDone;
        primaryJobCandidates = data.primaryJobCandidates;
        obtainPersonalItemUnownedRandomList = data.obtainPersonalItemUnownedRandomList;
        hasStartedScreamCheck = data.hasStartedScreamCheck;
        doNotDoRecoverHPJob = data.doNotDoRecoverHPJob;
        canReportDemonicStructure = data.canReportDemonicStructure;
        additionalPriorityJobs = data.additionalPriorityJobs;
        if (!canReportDemonicStructure) {
	        //make character listen to this so that he/she can report again after reaching home
	        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, TryEnableReportStructure);
        }
    }

    public void SetOwner(Character owner) {
        this.owner = owner;
    }

    #region Listeners
    public void SubscribeToListeners() {
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
		Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.AddListener<NPCSettlement, bool>(SettlementSignals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
        Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
        //Messenger.AddListener<Character>(Signals.ON_SEIZE_CHARACTER, OnSeizedCharacter);
        //Messenger.AddListener<Character>(Signals.ON_UNSEIZE_CHARACTER, OnUnseizeCharacter);
    }
    public void UnsubscribeListeners() {
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
        Messenger.RemoveListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
		Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.RemoveListener<NPCSettlement, bool>(SettlementSignals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.RemoveListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.RemoveListener<Character, HexTile>(CharacterSignals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.RemoveListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
        Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
        TryStopScreamCheck();
	}
    private void OnCharacterCanPerformAgain(Character character) {
		if (character == owner) {
			// if (_owner.currentSettlement is NPCSettlement npcSettlement && npcSettlement.isUnderSiege) {
			// 	TriggerFleeHome();	
			// }
			// character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.RESTRAIN); //cancel all restrain jobs.
			owner.needsComponent.CheckExtremeNeeds();

            //Add all in vision poi to process again
            for (int i = 0; i < owner.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest inVision = owner.marker.inVisionPOIs[i];
                owner.marker.AddUnprocessedPOI(inVision);
            }
            Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, owner as IPlayerActionTarget);
            //for (int i = 0; i < _owner.marker.inVisionCharacters.Count; i++) {
            //    Character inVisionCharacter = _owner.marker.inVisionCharacters[i];
            //    _owner.marker.AddUnprocessedPOI(inVisionCharacter);
            //}
        }
	}
	private void OnCharacterCanNoLongerPerform(Character character) {
		if (character == owner && character.isDead == false) {
			//TODO: THIS IS ONLY TEMPORARY! REDO THIS!
			if (character.interruptComponent.isInterrupted &&
			           character.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Narcoleptic_Attack) {
				//Don't do anything
			} else if (character.currentActionNode != null && character.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && character.currentActionNode.action.goapType.IsRestingAction()) {
				character.CancelAllJobsExceptForCurrent();
			} else {
				character.jobQueue.CancelAllJobs();
			}
            if (character.marker) {
                character.marker.StopMovement();
                character.marker.pathfindingAI.ClearAllCurrentPathData();
            }
            character.reactionComponent.SetIsHidden(false);
            //character.RevertFromVampireBatForm();
            //character.RevertFromWerewolfForm();

            character.UncarryPOI();
            if (character.traitContainer.HasTrait("Unconscious")) {
                character.ForceCancelAllJobsTargetingThisCharacter(JOB_TYPE.KNOCKOUT);
            }
            Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, owner as IPlayerActionTarget);
            //_owner.behaviourComponent.SetIsHarassing(false, null);
            //_owner.behaviourComponent.SetIsInvading(false, null);
            //_owner.behaviourComponent.SetIsDefending(false, null);
            // TryTriggerRestrain();
        }
	}
	private void OnCharacterCanNoLongerMove(Character character) {
		if (character == owner) {
            character.reactionComponent.SetIsHidden(false);
            TryStartScreamCheck();
		}
	}
	private void OnCharacterCanMoveAgain(Character character) {
		if (character == owner) {
			TryStopScreamCheck();
		}
	}
	private void OnCharacterFinishedJob(Character character, GoapPlanJob job) {
        // if (character == _owner && job.jobType == JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM) {
        // 	TriggerBuryPsychopathVictim(job);
        // }
    }
    private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
		if (traitable == owner) {
			if (TraitManager.Instance.removeStatusTraits.Contains(trait.name)) {
				TryCreateSettlementRemoveStatusJob(trait);
			}
			if (trait is Burning || trait is Poisoned) {
				TriggerRemoveStatusSelf(trait);
			}
			TryStartScreamCheck();
		}
    }
	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == owner) {
			TryStopScreamCheck();
			if (TraitManager.Instance.removeStatusTraits.Contains(nameof(trait))) {
				owner.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_STATUS, trait.name, removedBy); //so that the character that cured him will not cancel his job.
			}
		}
	}
	private void OnSettlementUnderSiegeChanged(NPCSettlement npcSettlement, bool siegeState) {
		//if (npcSettlement == _owner.currentSettlement && siegeState 
		//	&& (_owner.stateComponent.currentState is CombatState) == false && _owner.isNormalCharacter) {
  //          //characters current npcSettlement is under siege
  //          if (!_owner.combatComponent.isInCombat) {
  //              _owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stopped, _owner);
  //              // Messenger.AddListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfStopInterruptFinished);
  //          }
  //      }
	}
	private void OnCharacterEnteredHexTile(Character character, HexTile tile) {
		if (character == owner) {
			TryCreateRemoveStatusJob();
		}
	}
	private void OnCharacterExitedHexTile(Character character, HexTile tile) {
		if (character == owner) {
            Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RESTRAIN, owner as IPointOfInterest);
        }
	}
    private void OnSeizePOI(IPointOfInterest poi) {
        if(poi is Character) {
            OnSeizedCharacter(poi as Character);
        }
    }
    private void OnUnseizePOI(IPointOfInterest poi) {
        if (poi is Character) {
            OnUnseizeCharacter(poi as Character);
        }
    }
    private void OnSeizedCharacter(Character character) {
		if (character == owner) {
			TryStopScreamCheck();
		}
	}
	private void OnUnseizeCharacter(Character character) {
		if (character == owner) {
			TryStartScreamCheck();
		}
	}
	private void OnJobRemovedFromQueue(JobQueueItem jobQueueItem, Character character) {
		if (character == owner && jobQueueItem.jobType == JOB_TYPE.CRAFT_MISSING_FURNITURE) {
			Messenger.Broadcast(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY);
		}
	}
    #endregion

    #region Utilities
    public void SetPrimaryJob(JOB_TYPE jobType) {
        primaryJob = jobType;
    }
    public string GetPriorityJobs() {
        string jobs = string.Empty;
        if (owner.characterClass.priorityJobs != null && owner.characterClass.priorityJobs.Length > 0) {
            for (int i = 0; i < owner.characterClass.priorityJobs.Length; i++) {
                if (i > 0) {
                    jobs += ",";
                }
                jobs += owner.characterClass.priorityJobs[i].ToString();
            }
        }
        if(owner.jobComponent.priorityJobs.Count > 0) {
            if(jobs != string.Empty) {
                jobs += ",";
            }
            for (int i = 0; i < owner.jobComponent.priorityJobs.Count; i++) {
                if (i > 0) {
                    jobs += ",";
                }
                jobs += owner.jobComponent.priorityJobs[i].ToString();
            }
        }
        return jobs;
    }
    public string GetSecondaryJobs() {
        string jobs = string.Empty;
        if (owner.characterClass.secondaryJobs != null && owner.characterClass.secondaryJobs.Length > 0) {
            for (int i = 0; i < owner.characterClass.secondaryJobs.Length; i++) {
                if (i > 0) {
                    jobs += ",";
                }
                jobs += owner.characterClass.secondaryJobs[i].ToString();
            }
        }
        return jobs;
    }
    public string GetAbleJobs() {
        string jobs = string.Empty;
        if (owner.characterClass.ableJobs != null && owner.characterClass.ableJobs.Length > 0) {
            for (int i = 0; i < owner.characterClass.ableJobs.Length; i++) {
                if (i > 0) {
                    jobs += ",";
                }
                jobs += owner.characterClass.ableJobs[i].ToString();
            }
        }
        return jobs;
    }
    public string GetAdditionalPriorityJobs() {
	    string jobs = string.Empty;
	    if (additionalPriorityJobs != null && additionalPriorityJobs.Count > 0) {
		    for (int i = 0; i < additionalPriorityJobs.Count; i++) {
			    if (i > 0) {
				    jobs += ",";
			    }
			    jobs += additionalPriorityJobs[i].ToString();
		    }
	    }
	    return jobs;
    }
    public void AddPriorityJob(JOB_TYPE jobType) {
        if (!priorityJobs.Contains(jobType)) {
            priorityJobs.Add(jobType);
        }
    }
    public bool RemovePriorityJob(JOB_TYPE jobType) {
        return priorityJobs.Remove(jobType);
    }
    /// <summary>
    /// Does this character have a job that is higher priority than the
    /// given job type.
    /// </summary>
    /// <param name="jobType">The job type to compare to.</param>
    /// <returns>True or false</returns>
    public bool HasHigherPriorityJobThan(JOB_TYPE jobType) {
	    if (owner.jobQueue.jobsInQueue.Count == 0) {
		    return false;
	    } else {
		    return owner.jobQueue.jobsInQueue[0].priority > jobType.GetJobTypePriority();
	    }
    }
    public bool CanDoJob(JOB_TYPE jobType) {
	    return owner.jobComponent.primaryJob == jobType || owner.characterClass.CanDoJob(jobType) || priorityJobs.Contains(jobType) || additionalPriorityJobs.Contains(jobType);
    }
    public void AddAdditionalPriorityJob(JOB_TYPE jobType) {
		additionalPriorityJobs.Add(jobType);    
    }
    public void RemoveAdditionalPriorityJob(JOB_TYPE jobType) {
	    additionalPriorityJobs.Remove(jobType);
    }
    public void AddAdditionalPriorityJob(params JOB_TYPE[] jobType) {
	    for (int i = 0; i < jobType.Length; i++) {
		    AddAdditionalPriorityJob(jobType[i]);    
	    }
    }
    public void RemoveAdditionalPriorityJob(params JOB_TYPE[] jobType) {
	    for (int i = 0; i < jobType.Length; i++) {
		    RemoveAdditionalPriorityJob(jobType[i]);    
	    }
    }
    #endregion

    #region General Jobs
    public bool PlanIdleLongStandStill(out JobQueueItem p_producedJob) {
	    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.LONG_STAND_STILL], owner, owner, null, 0);
	    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.LONG_STAND_STILL, owner, owner);
	    goapPlan.SetDoNotRecalculate(true);
	    job.SetCannotBePushedBack(true);
	    job.SetAssignedPlan(goapPlan);
	    p_producedJob = job;
	    return true;
    }
    public bool PlanIdleStrollOutside() {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL_OUTSIDE, owner);
        owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    public bool PlanIdleStrollOutside(out JobQueueItem producedJob) {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL_OUTSIDE, owner);
        producedJob = job;
        return true;
    }
    public bool PlanZombieStrollOutside(out JobQueueItem producedJob) {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.ZOMBIE_STROLL, CHARACTER_STATE.STROLL_OUTSIDE, owner);
        producedJob = job;
        return true;
    }
    public bool PlanIdleBerserkStrollOutside(out JobQueueItem producedJob) {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.BERSERK_STROLL, CHARACTER_STATE.STROLL_OUTSIDE, owner);
        producedJob = job;
        return true;
    }
    //public bool PlanReturnHome() { //bool forceDoAction = false
    //    if (owner.homeStructure != null && owner.homeStructure.tiles.Count > 0 && !owner.homeStructure.hasBeenDestroyed) {
    //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], owner, owner, null, 0);
    //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE_RETURN_HOME, INTERACTION_TYPE.RETURN_HOME, owner, owner);
    //        goapPlan.SetDoNotRecalculate(true);
    //        job.SetCannotBePushedBack(true);
    //        job.SetAssignedPlan(goapPlan);
    //        return owner.jobQueue.AddJobInQueue(job);
    //    } else if (owner.HasTerritory()) {
    //        return TriggerReturnTerritory(JOB_TYPE.IDLE_RETURN_HOME);
    //    }
    //    return false;
    //}
    //public bool PlanReturnHome(out JobQueueItem producedJob) { //bool forceDoAction = false
    //    if (owner.homeStructure != null && owner.homeStructure.tiles.Count > 0 && !owner.homeStructure.hasBeenDestroyed) {
    //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], owner, owner, null, 0);
    //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE_RETURN_HOME, INTERACTION_TYPE.RETURN_HOME, owner, owner);
    //        goapPlan.SetDoNotRecalculate(true);
    //        job.SetCannotBePushedBack(true);
    //        job.SetAssignedPlan(goapPlan);
    //        producedJob = job;
    //        return true;
    //    } else if (owner.HasTerritory()) {
    //        return TriggerReturnTerritory(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
    //    }
    //    producedJob = null;
    //    return false;
    //}
    //public bool PlanReturnHomeUrgent() { //bool forceDoAction = false
    //    if (owner.homeStructure != null && owner.homeStructure.tiles.Count > 0 && !owner.homeStructure.hasBeenDestroyed) {
    //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], owner, owner, null, 0);
    //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_HOME_URGENT, INTERACTION_TYPE.RETURN_HOME, owner, owner);
    //        goapPlan.SetDoNotRecalculate(true);
    //        job.SetCannotBePushedBack(true);
    //        job.SetAssignedPlan(goapPlan);
    //        return owner.jobQueue.AddJobInQueue(job);
    //    } else if (owner.homeSettlement != null) {

    //    } else if (owner.HasTerritory()) {
    //        return TriggerReturnTerritory(JOB_TYPE.RETURN_HOME_URGENT);
    //    }
    //    return false;
    //}
    #endregion

    #region Job Triggers
    private void TriggerScreamJob() {
		if (owner.jobQueue.HasJob(JOB_TYPE.SCREAM) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SCREAM, INTERACTION_TYPE.SCREAM_FOR_HELP, owner, owner);
			owner.jobQueue.AddJobInQueue(job);
		}
	}
	public void TriggerBuryPsychopathVictim(Character target, NPCSettlement settlementOfTarget) {
		if (settlementOfTarget != null) {
			JobQueueItem buryJob = settlementOfTarget.GetJob(JOB_TYPE.BURY, target);
			buryJob?.ForceCancelJob(false);	
		}

		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY_SERIAL_KILLER_VICTIM, INTERACTION_TYPE.BURY_CHARACTER, target, owner);

        bool hasChosenTile = false;
        HexTile chosenHex = owner.currentRegion.GetRandomHexThatMeetCriteria(h => h.elevationType != ELEVATION.MOUNTAIN && h.elevationType != ELEVATION.WATER && h.IsNextToVillage() && h.settlementOnTile == null && owner.movementComponent.HasPathTo(h));
        if(chosenHex != null) {
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles.Where(t => owner.movementComponent.HasPathTo(t)));
            if(chosenTile != null) {
                hasChosenTile = true;
                job.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[] { chosenTile.structure, chosenTile });
            }
        }
        if (!hasChosenTile) {
            LocationStructure wilderness = owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            List<LocationGridTile> choices = wilderness.unoccupiedTiles.Where(x => x.IsPartOfSettlement(owner.homeSettlement) == false).ToList();
            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(choices);
            job.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[] { wilderness, targetTile });
        }
		owner.jobQueue.AddJobInQueue(job);
	}
	public bool TriggerFleeHome(JOB_TYPE jobType = JOB_TYPE.FLEE_TO_HOME) {
        if(owner.homeStructure != null && !owner.homeStructure.hasBeenDestroyed && owner.homeStructure.tiles.Count > 0 && !owner.isAtHomeStructure) {
            if (!owner.jobQueue.HasJob(jobType)) {
                ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], owner, owner, null, 0);
                GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.RETURN_HOME, owner, owner);
                goapPlan.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                job.SetAssignedPlan(goapPlan);
                owner.jobQueue.AddJobInQueue(job);
            }
            return true;
        }
        return false;
	}
	public bool TriggerDestroy(IPointOfInterest target) {
		if (!owner.jobQueue.HasJob(JOB_TYPE.DESTROY, target)) {
			GoapPlanJob destroyJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.ASSAULT, target, owner);
			destroyJob.SetStillApplicableChecker(JobManager.Destroy_Applicability);
            return owner.jobQueue.AddJobInQueue(destroyJob);
		}
		return false;
	}
	public bool TriggerDestroy(IPointOfInterest target, out JobQueueItem producedJob) {
		if (!owner.jobQueue.HasJob(JOB_TYPE.DESTROY, target)) {
			GoapPlanJob destroyJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.ASSAULT, target, owner);
			destroyJob.SetStillApplicableChecker(JobManager.Destroy_Applicability);
			producedJob = destroyJob;
			return true;
		}
		producedJob = null;
		return false;
	}
	private void TriggerSettlementRemoveStatusJob(Trait trait) {
		if (owner.isDead) { return; }
		if (trait.gainedFromDoing == null || trait.gainedFromDoing.isStealth == false) { //only create remove status job if trait was not gained from a stealth action
			GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, target = GOAP_EFFECT_TARGET.TARGET };
			if (owner.homeSettlement.HasJob(goapEffect, owner) == false) {
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, owner, owner.homeSettlement);
                JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, job, INTERACTION_TYPE.NONE);
                job.SetCanTakeThisJobChecker(JobManager.Can_Take_Remove_Status);
				job.SetStillApplicableChecker(JobManager.Remove_Status_Applicability);
				owner.homeSettlement.AddToAvailableJobs(job);
			}	
		}
	}
	private void TriggerRemoveStatusSelf(Trait trait) {
		if (owner is Summon) {
			return; //Reference: https://trello.com/c/LkTisHji/3023-live-03356-spider-in-villager-faction-able-to-remove-poison
		}
		if (trait.gainedFromDoing == null || trait.gainedFromDoing.isStealth == false) { //only create remove status job if trait was not gained from a stealth action
			GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, target = GOAP_EFFECT_TARGET.TARGET };
			if (owner.jobQueue.HasJob(goapEffect, owner) == false) {
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, owner, owner);
                JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, job, INTERACTION_TYPE.NONE);
                job.SetStillApplicableChecker(JobManager.Remove_Status_Self_Applicability);
				owner.jobQueue.AddJobInQueue(job);
			}	
		}
	}
    public void TriggerRemoveStatusTarget(IPointOfInterest target, string traitName) {
	    if (owner is Summon) {
		    return; //Reference: https://trello.com/c/LkTisHji/3023-live-03356-spider-in-villager-faction-able-to-remove-poison
	    }
        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = traitName, target = GOAP_EFFECT_TARGET.TARGET };
        if (owner.jobQueue.HasJob(goapEffect, owner) == false) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, target, owner);
            JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, job, INTERACTION_TYPE.NONE);
            job.SetStillApplicableChecker(JobManager.Remove_Status_Target_Applicability);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
    private void TriggerFeed(Character target) {
		GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, target = GOAP_EFFECT_TARGET.TARGET };
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, target, owner);
        JobUtilities.PopulatePriorityLocationsForTakingEdibleResources(owner, job, INTERACTION_TYPE.TAKE_RESOURCE);
        job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
		owner.jobQueue.AddJobInQueue(job);
	}
    //private bool TriggerMoveCharacterToBed(Character target) {
	//	if (target.homeStructure != null && target.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER) == false) {
	//		Bed bed = target.homeStructure.GetTileObjectOfType<Bed>(TILE_OBJECT_TYPE.BED);
	//		if (bed != null && bed.CanSleepInBed(target)) {
	//			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, target, _owner);
	//			job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { target.homeStructure, bed.gridTileLocation });
	//			_owner.jobQueue.AddJobInQueue(job);
	//			return true;
	//		}
	//	}
	//	return false;
	//}
	//private bool TriggerMoveCharacterForHappinessRecovery(Character target) {
	//	if (target.currentStructure == target.homeStructure.GetLocationStructure() || 
	//	    target.currentStructure == target.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS)) {
	//		return false; //character is already at 1 of the target structures, do not create move job.
	//	}
	//	if (target.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
	//		return false;
	//	}
	//	int chance = UnityEngine.Random.Range(0, 2);
	//	LocationStructure targetStructure = chance == 0 ? target.homeStructure.GetLocationStructure() : target.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
	//	GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, target, _owner);
	//	job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { targetStructure });
	//	_owner.jobQueue.AddJobInQueue(job);
	//	return true;
	//}
	#endregion

	#region Applicability Checkers
	private bool IsRemoveStatusTargetJobStillApplicable(IPointOfInterest target, GoapPlanJob job, string traitName) {
	    if (target.gridTileLocation == null || target.isDead) {
		    return false;
	    }
	    if (!target.traitContainer.HasTrait(traitName)) {
		    return false; //target no longer has the given trait
	    }
	    return true;
    }
	#endregion

	#region Scream
	private void TryStartScreamCheck() {
		if (hasStartedScreamCheck) {
			return;
		}
        if (!owner.isNormalCharacter) {
            return;
        }
		if ((owner.limiterComponent.canMove == false && 
		     owner.traitContainer.HasTrait("Exhausted", "Starving", "Sulking"))
            || (owner.traitContainer.HasTrait("Restrained") && owner.currentStructure.structureType != STRUCTURE_TYPE.PRISON)) {
			hasStartedScreamCheck = true;
			Messenger.AddListener(Signals.HOUR_STARTED, HourlyScreamCheck);
			owner.logComponent.PrintLogIfActive($"<color=green>{GameManager.Instance.TodayLogString()}{owner.name} has started scream check</color>");
		}
	}
	private void TryStopScreamCheck() {
		if (hasStartedScreamCheck == false) {
			return;
		}
		bool isNotNeedy = !owner.traitContainer.HasTrait("Exhausted", "Starving", "Sulking");
		bool isNotRestrained = !owner.traitContainer.HasTrait("Restrained");
		bool isRestrainedButInPrison = owner.traitContainer.HasTrait("Restrained") &&
		                               owner.currentStructure.structureType == STRUCTURE_TYPE.PRISON;
		
		//scream will stop check if
		// - character can already move or
		// - character is no longer exhausted, starving or sulking and
		// - character is no longer restrained or
		// - character is still restrained, but is at prison.
		if (((owner.limiterComponent.canMove || isNotNeedy) && (isNotRestrained || isRestrainedButInPrison)) 
		    || owner.gridTileLocation == null || owner.isDead) {
			hasStartedScreamCheck = false;
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyScreamCheck);
			owner.logComponent.PrintLogIfActive($"<color=red>{GameManager.Instance.TodayLogString()}{owner.name} has stopped scream check</color>");
		}
	}
	private void HourlyScreamCheck() {
		if (owner.limiterComponent.canPerform) {
			return;
		}
        if (owner.needsComponent.isExhausted) {
            owner.needsComponent.PlanExtremeTirednessRecoveryActionsForCannotPerform();
            return;
        }
		string summary = $"{owner.name} is checking for scream.";
		int chance = 50;
		if (owner.limiterComponent.canMove == false && 
		    owner.traitContainer.HasTrait("Starving", "Sulking")) { //"Exhausted", 
            chance = 75;
		}
		summary += $"Chance is {chance.ToString()}.";
		int roll = Random.Range(0, 100); 
		summary += $"Roll is {roll.ToString()}.";
		owner.logComponent.PrintLogIfActive($"<color=blue>{summary}</color>");
		if (roll < chance) {
			TriggerScreamJob();
		}
	}
	#endregion
	
	#region Remove Status
	private void TryCreateSettlementRemoveStatusJob(Trait trait) {
		if (owner.homeSettlement != null && owner.gridTileLocation != null && owner.gridTileLocation.IsNextToOrPartOfSettlement(owner.homeSettlement)
		    && owner.traitContainer.HasTrait("Criminal") == false) {
			TriggerSettlementRemoveStatusJob(trait);
		}
	}
	private void TryCreateRemoveStatusJob() {
		if (owner.homeSettlement != null && owner.gridTileLocation.IsNextToOrPartOfSettlement(owner.homeSettlement)
		    && owner.traitContainer.HasTrait("Criminal") == false) {
			List<Trait> statusTraits = owner.traitContainer.GetTraitsOrStatuses<Trait>(TraitManager.Instance.removeStatusTraits.ToArray());
			for (int i = 0; i < statusTraits.Count; i++) {
				Trait trait = statusTraits[i];
				TryCreateSettlementRemoveStatusJob(trait);
			}
		}
	}
	#endregion

	#region Feed
	public bool TryTriggerFeed(Character targetCharacter) {
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.FEED)) {
			GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.TARGET);
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, targetCharacter, owner);
            JobUtilities.PopulatePriorityLocationsForTakingEdibleResources(owner, job, INTERACTION_TYPE.TAKE_RESOURCE);
            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}
	public bool TriggerFeed(Character targetCharacter, out JobQueueItem producedJob) {
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.FEED)) {
			GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.TARGET);
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, targetCharacter, owner);
            JobUtilities.PopulatePriorityLocationsForTakingEdibleResources(owner, job, INTERACTION_TYPE.TAKE_RESOURCE);
            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
			producedJob = job;
			return true;
		}
		producedJob = null;
		return false;
	}
    #endregion

    #region Move Character
    public bool TryTriggerMoveCharacter(Character targetCharacter, LocationStructure dropLocationStructure, bool doNotRecalculate = false) {
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
			job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {dropLocationStructure});
            job.SetDoNotRecalculate(doNotRecalculate);
			return owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}
    public bool TryTriggerMoveCharacter(Character targetCharacter, LocationStructure dropLocationStructure, out JobQueueItem producedJob, bool doNotRecalculate = false) {
        producedJob = null;
        if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure });
            job.SetDoNotRecalculate(doNotRecalculate);
            producedJob = job;
            return true;
        }
        return false;
    }
    public bool TryTriggerMoveCharacter(Character targetCharacter, LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, owner);
			job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure, dropGridTile });
			return owner.jobQueue.AddJobInQueue(job);   
		}
		return false;
	}
    // public bool TryTriggerMoveCharacterTirednessRecovery(Character target) {
    // 	if (target.traitContainer.GetNormalTrait<Trait>("Tired", "Exhausted") != null) {
    // 		bool isSameHome = target.homeNpcSettlement == _owner.homeNpcSettlement;
    // 		bool isNotHostileFaction = target.faction == _owner.faction
    // 			|| target.faction.GetRelationshipWith(_owner.faction).relationshipStatus
    // 			!= FACTION_RELATIONSHIP_STATUS.HOSTILE;
    // 		bool isNotEnemy =
    // 			_owner.RelationshipManager.HasOpinionLabelWithCharacter(target, RelationshipManager.Enemy,
    // 				RelationshipManager.Rival) == false;
    // 		if ((isSameHome || isNotHostileFaction) && isNotEnemy) {
    // 			return TriggerMoveCharacterToBed(target);
    // 		}
    // 	}
    // 	return false;
    // }
    // public bool TryTriggerMoveCharacterHappinessRecovery(Character target) {
    // 	if (target.traitContainer.GetNormalTrait<Trait>("Bored", "Sulking", "Forlorn", "Lonely") != null) {
    // 		bool isSameHome = target.homeNpcSettlement == _owner.homeNpcSettlement;
    // 		bool isNotHostileFaction = target.faction == _owner.faction
    // 		                           || target.faction.GetRelationshipWith(_owner.faction).relationshipStatus
    // 		                           != FACTION_RELATIONSHIP_STATUS.HOSTILE;
    // 		bool isNotEnemy =
    // 			_owner.RelationshipManager.HasOpinionLabelWithCharacter(target, RelationshipManager.Enemy,
    // 				RelationshipManager.Rival) == false;
    // 		if ((isSameHome || isNotHostileFaction) && isNotEnemy) {
    // 			return TriggerMoveCharacterForHappinessRecovery(target);
    // 		}
    // 	}
    // 	return false;
    // }
    #endregion

    #region Capture Character
    public bool TryTriggerCaptureCharacter(Character targetCharacter, LocationStructure dropLocationStructure, bool doNotRecalculate = false) {
        if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CAPTURE_CHARACTER, INTERACTION_TYPE.DROP_RESTRAINED,
                targetCharacter, owner);
            job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { dropLocationStructure });
            job.SetDoNotRecalculate(doNotRecalculate);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool TryTriggerCaptureCharacter(Character targetCharacter, LocationStructure dropLocationStructure, out JobQueueItem producedJob, bool doNotRecalculate = false) {
        producedJob = null;
        if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CAPTURE_CHARACTER, INTERACTION_TYPE.DROP_RESTRAINED,
                targetCharacter, owner);
            job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { dropLocationStructure });
            job.SetDoNotRecalculate(doNotRecalculate);
            producedJob = job;
            return true;
        }
        return false;
    }
    public bool TryTriggerCaptureCharacter(Character targetCharacter, LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
        if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.CAPTURE_CHARACTER)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CAPTURE_CHARACTER, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
            job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { dropLocationStructure, dropGridTile });
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    #region Suicide
    public GoapPlanJob TriggerSuicideJob(string reason) {
		if (owner.jobQueue.HasJob(JOB_TYPE.COMMIT_SUICIDE) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COMMIT_SUICIDE, 
				new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR),
				owner,  owner);
            JobUtilities.PopulatePriorityLocationsForSuicide(owner, job);
            job.AddOtherData(INTERACTION_TYPE.NONE, new object[] {reason});
			owner.jobQueue.AddJobInQueue(job);
			return job;	
		}
		return null;
	}
    public bool TriggerSuicideJob(out JobQueueItem producedJob, string reason) {
        producedJob = null;
        if (owner.jobQueue.HasJob(JOB_TYPE.COMMIT_SUICIDE) == false) { 
	        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COMMIT_SUICIDE, 
		        new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR),
                owner, owner);
            JobUtilities.PopulatePriorityLocationsForSuicide(owner, job);
            job.AddOtherData(INTERACTION_TYPE.NONE, new object[] {reason});
            producedJob = job;
            return true;
        }
        return false;
    }
    #endregion

    #region Actions
    public void IncreaseNumOfTimesActionDone(GoapAction action) {
        if (!numOfTimesActionDone.ContainsKey(action.goapType)) {
            numOfTimesActionDone.Add(action.goapType, 1);
        } else {
            numOfTimesActionDone[action.goapType]++;
        }
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(3);
        SchedulingManager.Instance.AddEntry(dueDate, () => DecreaseNumOfTimesActionDone(action), owner);
    }
    private void DecreaseNumOfTimesActionDone(GoapAction action) {
        numOfTimesActionDone[action.goapType]--;
    }
    public int GetNumOfTimesActionDone(GoapAction action) {
        if (numOfTimesActionDone.ContainsKey(action.goapType)) {
            return numOfTimesActionDone[action.goapType];
        }
        return 0;
    }
    #endregion
    
    #region Roam
    public bool TriggerRoamAroundTerritory() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TERRITORY)) {
            LocationGridTile chosenTile;
            if (owner.homeStructure != null) {
                chosenTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
            } 
            else if (owner.homeSettlement != null) {
                chosenTile = owner.homeSettlement.GetRandomPassableGridTileInSettlementThatMeetCriteria(t => owner.movementComponent.HasPathToEvenIfDiffRegion(t));
            } 
            else if (owner.HasTerritory()) {
                HexTile chosenTerritory = owner.territory;
                chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
            } else {
                if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                    if (owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
                        HexTile chosenHex = owner.gridTileLocation.GetNearestHexTileWithinRegion();
                        chosenTile = CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles);
                    } else {
                        HexTile chosenHex = owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                        chosenTile = CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles);
                    }
                } else {
                    chosenTile = CollectionUtilities.GetRandomElement(owner.currentStructure.passableTiles);
                }
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile),  }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TERRITORY, INTERACTION_TYPE.ROAM, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerRoamAroundTerritory(out JobQueueItem producedJob, bool checkIfPathPossibleWithoutDigging = false) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TERRITORY)) {
		    LocationGridTile chosenTile;
		    if (owner.homeSettlement != null) {
			    chosenTile = checkIfPathPossibleWithoutDigging ? 
				    owner.homeSettlement.GetRandomPassableGridTileInSettlementThatMeetCriteria(t => owner.movementComponent.HasPathToEvenIfDiffRegion(t)) : 
				    owner.homeSettlement.GetRandomPassableGridTileInSettlementThatMeetCriteria(t => owner.movementComponent.HasPathToEvenIfDiffRegion(t));
		    } else if (owner.homeStructure != null) {
                if (checkIfPathPossibleWithoutDigging) {
				    List<LocationGridTile> choices = owner.homeStructure.passableTiles
                        .Where(t => owner.movementComponent.HasPathToEvenIfDiffRegion(t)).ToList();
				    chosenTile = choices.Count > 0 ? CollectionUtilities.GetRandomElement(choices) : CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
			    } else {
				    chosenTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
                }
		    } else if(owner.HasTerritory()) {
			    HexTile chosenTerritory = owner.territory;
			    if (checkIfPathPossibleWithoutDigging) {
				    List<LocationGridTile> choices = chosenTerritory.locationGridTiles
					    .Where(t => owner.movementComponent.HasPathToEvenIfDiffRegion(t)).ToList();
				    if (choices.Count > 0) {
					    chosenTile = CollectionUtilities.GetRandomElement(choices);	    
				    } else {
					    //only added this so there is a fallback if ever no valid tiles were found.
					    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);    
				    }
			    } else {
				    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);    
			    }
		    } else {
                if(owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                    if (owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
                        HexTile chosenHex = owner.gridTileLocation.GetNearestHexTileWithinRegion();
                        chosenTile = CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles);
                    } else {
                        HexTile chosenHex = owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                        chosenTile = CollectionUtilities.GetRandomElement(chosenHex.locationGridTiles);
                    }
                } else {
                    chosenTile = CollectionUtilities.GetRandomElement(owner.currentStructure.passableTiles);
                }
            }
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile),  }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TERRITORY, INTERACTION_TYPE.ROAM, owner, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerRoamAroundTile(JOB_TYPE jobType, LocationGridTile tile = null) {
        JobQueueItem job = null;
        if(TriggerRoamAroundTile(jobType, out job, tile)) {
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool TriggerRoamAroundTile(JOB_TYPE jobType, out JobQueueItem producedJob, LocationGridTile tile = null) {
        producedJob = null;
        if (!owner.jobQueue.HasJob(jobType)) {
            LocationGridTile chosenTile = tile;
            if (chosenTile == null) {
                if (owner.IsInHomeSettlement()) {
                    chosenTile = owner.homeSettlement.GetRandomPassableGridTileInSettlementThatMeetCriteria(t => owner.movementComponent.HasPathToEvenIfDiffRegion(t));
                } else if (owner.isAtHomeStructure) {
                    chosenTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
                } else if (owner.IsInTerritory()) {
                    HexTile chosenTerritory = owner.territory;
                    //chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                    chosenTile = chosenTerritory.GetRandomPassableTile();
                } else if (owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
                    HexTile chosenTerritory = owner.gridTileLocation.GetNearestHexTileWithinRegionThatMeetCriteria(h => owner.movementComponent.HasPathTo(h));
                    //chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                    chosenTile = chosenTerritory.GetRandomPassableTile();
                } else {
                    HexTile chosenTerritory = owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                    //chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                    chosenTile = chosenTerritory.GetRandomPassableTile();
                }
            }
            if (chosenTile == null) {
                return false;
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.ROAM, owner, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    return false;
    }
    public bool TriggerRoamAroundTile(out JobQueueItem producedJob, LocationGridTile tile = null) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE)) {
		    LocationGridTile chosenTile = tile;
		    if (chosenTile == null) {
                if (owner.IsInHomeSettlement()) {
                    chosenTile = owner.homeSettlement.GetRandomPassableGridTileInSettlementThatMeetCriteria(t => owner.movementComponent.HasPathToEvenIfDiffRegion(t));
                } else if (owner.isAtHomeStructure) {
                    chosenTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
                } else if (owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
				    HexTile chosenTerritory = owner.gridTileLocation.GetNearestHexTileWithinRegion();
				    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
			    } else {
				    HexTile chosenTerritory = owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
				    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
			    }
		    }
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TILE, INTERACTION_TYPE.ROAM, owner, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerRoamAroundStructure(out JobQueueItem producedJob, LocationGridTile tile = null) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_STRUCTURE)) {
            LocationGridTile chosenTile = tile;
            if (chosenTile == null) {
                if (owner.currentStructure != null) {
                    //OPTIMIZE THIS!
                    //chosenTile = CollectionUtilities.GetRandomElement(_owner.currentStructure.passableTiles);
                    if(owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                        chosenTile = owner.gridTileLocation.GetNearestHexTileWithinRegion().GetRandomTile();
                    } else {
                        chosenTile = CollectionUtilities.GetRandomElement(owner.currentStructure.passableTiles);
                        //List<LocationGridTile> choices = owner.currentStructure.passableTiles.Where(t => PathfindingManager.Instance.HasPathEvenDiffRegion(owner.gridTileLocation, t)).ToList();
                        //chosenTile = choices.Count > 0 ? CollectionUtilities.GetRandomElement(choices) : CollectionUtilities.GetRandomElement(owner.currentStructure.passableTiles);
                    }

                }
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_STRUCTURE, INTERACTION_TYPE.ROAM, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerAttackDemonicStructure(out JobQueueItem producedJob, LocationGridTile tile = null) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.COUNTERATTACK)) {
		    LocationGridTile chosenTile = tile;
		    if (chosenTile == null) {
			    if (owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
				    TriggerStand(out producedJob);
				    return false;
			    } else {
				    HexTile chosenTerritory = owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
				    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
			    }
		    }
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COUNTERATTACK, INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE, owner, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    // job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerMoveToHex(HexTile hex) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE)) {
	        LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(hex.locationGridTiles);
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TILE, INTERACTION_TYPE.ROAM, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerMoveToHex(out JobQueueItem producedJob, HexTile hex) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE)) {
		    LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(hex.locationGridTiles);
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TILE, INTERACTION_TYPE.ROAM, owner, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerStand(JOB_TYPE jobType) {
        if (!owner.jobQueue.HasJob(jobType)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND], owner, owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.STAND, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerStand(out JobQueueItem producedJob) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.STAND)) {
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND], owner, owner, null, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND, INTERACTION_TYPE.STAND, owner, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    //public bool TriggerReturnTerritoryUrgent() {
    //    if (!owner.jobQueue.HasJob(JOB_TYPE.RETURN_HOME_URGENT)) {
    //        LocationGridTile chosenTile;
    //        if (owner.homeStructure != null && !owner.homeStructure.hasBeenDestroyed) {
    //            chosenTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
    //        } else {
    //            if (owner.HasTerritory()) {
    //                HexTile chosenTerritory = owner.territory;
    //                chosenTile = chosenTerritory.GetRandomPassableTile();
    //            } else {
    //                //If has no territory, roam around tile instead
    //                return TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE);
    //            }
    //        }
    //        if(chosenTile == null) {
    //            return TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE);
    //        }
    //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.GO_TO_TILE], owner, chosenTile.genericTileObject, null, 0);
    //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_HOME_URGENT, INTERACTION_TYPE.GO_TO_TILE, chosenTile.genericTileObject, owner);
    //        goapPlan.SetDoNotRecalculate(true);
    //        job.SetCannotBePushedBack(true);
    //        job.SetAssignedPlan(goapPlan);
    //        return owner.jobQueue.AddJobInQueue(job);
    //    }
    //    return false;
    //}
    public bool PlanReturnHome(JOB_TYPE jobType) {
        JobQueueItem job = null;
        if (PlanReturnHome(jobType, out job)) {
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
        //if (!owner.jobQueue.HasJob(JOB_TYPE.RETURN_TERRITORY)) {
        //    LocationGridTile chosenTile;
        //    if (owner.homeStructure != null && !owner.homeStructure.hasBeenDestroyed) {
        //        chosenTile = CollectionUtilities.GetRandomElement(owner.homeStructure.unoccupiedTiles);
        //    } else {
        //        if (owner.HasTerritory()) {
        //            HexTile chosenTerritory = owner.territory;
        //            chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
        //        } else {
        //            //If has no territory, roam around tile instead
        //            return TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE);
        //        }
        //    }
        //    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
        //    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
        //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_TERRITORY, INTERACTION_TYPE.ROAM, owner, owner);
        //    goapPlan.SetDoNotRecalculate(true);
        //    job.SetCannotBePushedBack(true);
        //    job.SetAssignedPlan(goapPlan);
        //    owner.jobQueue.AddJobInQueue(job);
        //    return true;
        //}
        //return false;
    }
    public bool PlanReturnHome(JOB_TYPE jobType, out JobQueueItem producedJob) {
        producedJob = null;
        if (!owner.jobQueue.HasJob(jobType)) {
            LocationGridTile chosenTile = null;
            if (owner.homeStructure != null && !owner.homeStructure.hasBeenDestroyed) {
                chosenTile = CollectionUtilities.GetRandomElement(owner.homeStructure.passableTiles);
            } else if (owner.homeSettlement != null) {
                LocationStructure chosenStructure = owner.homeSettlement.GetRandomStructure();
                if(chosenStructure != null) {
                    chosenTile = CollectionUtilities.GetRandomElement(chosenStructure.passableTiles);
                }
            } else {
                if (owner.HasTerritory()) {
                    HexTile chosenTerritory = owner.territory;
                    chosenTile = chosenTerritory.GetRandomPassableTile();
                } else {
                    //If has no territory, roam around tile instead
                    return TriggerRoamAroundTile(out producedJob);
                }
            }
            if (chosenTile == null) {
                return TriggerRoamAroundTile(out producedJob);
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.GO_TO_TILE], owner, chosenTile.genericTileObject, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.GO_TO_TILE, chosenTile.genericTileObject, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        return false;
        //if (!owner.jobQueue.HasJob(JOB_TYPE.RETURN_TERRITORY)) {
        // LocationGridTile chosenTile;
        // if (owner.homeStructure != null) {
        //  chosenTile = CollectionUtilities.GetRandomElement(owner.homeStructure.unoccupiedTiles);
        // } else {
        //  if (owner.HasTerritory()) {
        //   HexTile chosenTerritory = owner.territory;
        //   List<LocationGridTile> validTiles = chosenTerritory.locationGridTiles
        //    .Where(t => owner.movementComponent.HasPathToEvenIfDiffRegion(t)).ToList();
        //   chosenTile = CollectionUtilities.GetRandomElement(validTiles.Count > 0 ? validTiles : chosenTerritory.locationGridTiles);
        //  } else {
        //   //If has no territory, roam around tile instead
        //   return TriggerRoamAroundTile(out producedJob);
        //  }
        // }
        // ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
        // GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
        // GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_TERRITORY, INTERACTION_TYPE.ROAM, owner, owner);
        // goapPlan.SetDoNotRecalculate(true);
        // job.SetCannotBePushedBack(true);
        // job.SetAssignedPlan(goapPlan);
        // producedJob = job;
        // return true;
        //}
        //producedJob = null;
        //return false;
    }
    public bool TriggerReturnPortal() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.RETURN_PORTAL)) {
            HexTile chosenTerritory = PlayerManager.Instance.player.portalTile;
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_PORTAL, INTERACTION_TYPE.ROAM, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerMonsterSleep() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SLEEP_OUTSIDE], owner, owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, INTERACTION_TYPE.SLEEP_OUTSIDE, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerMonsterSleep(out JobQueueItem producedJob) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL)) {
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SLEEP_OUTSIDE], owner, owner, null, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, INTERACTION_TYPE.SLEEP_OUTSIDE, owner, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public void CreateOpenChestJob(TileObject target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OPEN_CHEST, INTERACTION_TYPE.OPEN, target, owner);
        owner.jobQueue.AddJobInQueue(job);
    }
    public void CreateDestroyResourceAmountJob(ResourcePile target, int amount) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT, target, owner);
        if(amount > 0) {
            job.AddOtherData(INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT, new object[] { amount });
        }
        owner.jobQueue.AddJobInQueue(job);
    }
    public void TriggerStopJobs() {
	    if (owner.marker) {
		    owner.marker.StopMovement();
	    }
	    owner.jobQueue.CancelAllJobs();
    }
    #endregion

    #region Abduct
    public void CreateAbductJob(Character target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABDUCT, INTERACTION_TYPE.DROP, target, owner);
        LocationStructure dropLocationStructure = PlayerManager.Instance.player.portalTile.region.GetRandomStructureOfType(STRUCTURE_TYPE.TORTURE_CHAMBERS);
        if (dropLocationStructure == null) {
	        dropLocationStructure = PlayerManager.Instance.player.portalTile.locationGridTiles[0].structure;
        }
        job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure }); //For now drop in portal, this will be changed to Demonic Prison
        owner.jobQueue.AddJobInQueue(job);
    }
    #endregion
    
    #region Violence
    public GoapPlanJob CreateKnockoutJob(Character targetCharacter) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.KNOCKOUT, targetCharacter)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, owner);
		    owner.jobQueue.AddJobInQueue(job);
            return job;
	    }
        return null;
    }
    public GoapPlanJob CreateBrawlJob(Character targetCharacter) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.BRAWL, targetCharacter)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BRAWL, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, owner);
            job.SetCannotBePushedBack(true);
            job.SetDoNotRecalculate(true);
            owner.jobQueue.AddJobInQueue(job);
            return job;
        }
        return null;
    }
    public GoapPlanJob CreateDemonKillJob(Character targetCharacter) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.DEMON_KILL, targetCharacter)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEMON_KILL, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, owner);
		    owner.jobQueue.AddJobInQueue(job);
            return job;
	    }
        return null;
    }
    public bool CreateDemonKillJob(Character targetCharacter, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.DEMON_KILL, targetCharacter)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEMON_KILL, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, owner);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public GoapPlanJob CreateBerserkAttackJob(IPointOfInterest targetPOI) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.BERSERK_ATTACK, targetPOI)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BERSERK_ATTACK, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetPOI, owner);
            owner.jobQueue.AddJobInQueue(job);
            return job;
        }
        return null;
    }
    #endregion

    #region Needs
    public void CreateProduceFoodJob() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, new GoapEffect(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
            JobUtilities.PopulatePriorityLocationsForProduceResources(owner.homeSettlement, job, RESOURCE.FOOD);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
    public bool CreateProduceFoodForCampJob(out JobQueueItem producedJob) {
        producedJob = null;
        if (!owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD_FOR_CAMP)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD_FOR_CAMP, new GoapEffect(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
            producedJob = job;
            return true;
        }
        return false;
    }
    public bool CreateButcherJob(Character target, JOB_TYPE jobType, out JobQueueItem producedJob) {
        producedJob = null;
        if (!owner.jobQueue.HasJob(jobType)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.BUTCHER, target, owner);
            job.SetCancelOnDeath(false);
            producedJob = job;
            return true;
        }
        return false;
    }
    public void CreateEatJob(IPointOfInterest target) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT)) {
            if (owner.partyComponent.isActiveMember) {
                return;
            }
            if (!owner.limiterComponent.canDoFullnessRecovery) {
                return;
            }
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT, INTERACTION_TYPE.EAT, target, owner);
            JobUtilities.PopulatePriorityLocationsForFullnessRecovery(owner, job);
            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
            if (owner.jobQueue.AddJobInQueue(job)) {
                owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
            }
        }
    }
    #endregion

    #region Vampire
    public bool CreateDrinkBloodJob(JOB_TYPE jobType, IPointOfInterest target) {
        if (!owner.jobQueue.HasJob(jobType)) {
            if (owner.partyComponent.isActiveMember) {
                return false;
            }
            if (!owner.limiterComponent.canDoFullnessRecovery) {
                return false;
            }
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DRINK_BLOOD, target, owner);
            if (owner.jobQueue.AddJobInQueue(job)) {
                owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
            }
            return true;
        }
        return false;
    }
    public bool CreateVampiricEmbraceJob(JOB_TYPE jobType, IPointOfInterest target) {
        if (!owner.jobQueue.HasJob(jobType)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.VAMPIRIC_EMBRACE, target, owner);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool CreateVampiricEmbraceJob(JOB_TYPE jobType, IPointOfInterest target, out JobQueueItem producedJob) {
        producedJob = null;
        if (!owner.jobQueue.HasJob(jobType)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.VAMPIRIC_EMBRACE, target, owner);
            producedJob = job;
            return true;
        }
        return false;
    }
    public bool CreateFeedSelfToVampireJob(Character vampire) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.OFFER_BLOOD)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OFFER_BLOOD, INTERACTION_TYPE.FEED_SELF, vampire, owner);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    #region Items
    public void CreateTakeItemJob(JOB_TYPE jobType, TileObject targetItem) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.PICK_UP, targetItem, owner);
        owner.jobQueue.AddJobInQueue(job);
    }
    public bool TryCreateObtainPersonalItemJob() {
        if (!owner.IsInventoryAtFullCapacity()) {
            string chosenItemName = GetItemNameForObtainPersonalItemJob();
            if(chosenItemName != string.Empty) {
                GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, chosenItemName, false, GOAP_EFFECT_TARGET.ACTOR);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM, goapEffect, owner, owner);
                JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(owner, job, INTERACTION_TYPE.PICK_UP);
                owner.jobQueue.AddJobInQueue(job);
                return true;
            }
        }
        return false;

    }
    public bool TryCreateObtainPersonalItemJob(out JobQueueItem producedJob) {
	    if (!owner.IsInventoryAtFullCapacity() && !owner.jobQueue.HasJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM)) {
            string chosenItemName = GetItemNameForObtainPersonalItemJob();
            if (chosenItemName != string.Empty) {
                GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, chosenItemName, false, GOAP_EFFECT_TARGET.ACTOR);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM, goapEffect, owner, owner);
                JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(owner, job, INTERACTION_TYPE.PICK_UP);
                producedJob = job;
                return true;
            }
	    }
	    producedJob = null;
	    return false;

    }
    public bool TryCreateObtainPersonalItemJob(string chosenItemName, out JobQueueItem producedJob) {
	    if (!owner.IsInventoryAtFullCapacity() && !owner.jobQueue.HasJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM)) {
		    GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, chosenItemName, false, GOAP_EFFECT_TARGET.ACTOR);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM, goapEffect, owner, owner);
            JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(owner, job, INTERACTION_TYPE.PICK_UP);
            producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;

    }
    public void CreateDropItemJob(JOB_TYPE jobType, TileObject target, LocationStructure dropLocation, bool doNotRecalculate = false) {
        if(!owner.jobQueue.HasJob(jobType, target)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DROP_ITEM, target, owner);
            job.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[] { dropLocation });
            job.SetDoNotRecalculate(doNotRecalculate);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void CreateHoardItemJob(TileObject target, LocationStructure dropLocation, bool doNotRecalculate = false) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.HOARD, target)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HOARD, INTERACTION_TYPE.DROP_ITEM, target, owner);
            job.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[] { dropLocation });
            job.SetDoNotRecalculate(doNotRecalculate);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
    private string GetItemNameForObtainPersonalItemJob() {
        if (owner.homeSettlement != null && owner.interestedItemNames != null && owner.interestedItemNames.Count > 0) {
            if (obtainPersonalItemUnownedRandomList == null) { obtainPersonalItemUnownedRandomList = new List<string>(); }
            obtainPersonalItemUnownedRandomList.Clear();
            for (int i = 0; i < owner.interestedItemNames.Count; i++) {
                string itemName = owner.interestedItemNames[i];
                if (owner.HasItem(itemName)) {
                    //Pick one of the character class's needed items and only choose the ones that he doesn't have yet in his inventory
                    continue;
                }
                bool itemHasBeenAdded = false;
                for (int j = 0; j < owner.homeSettlement.tiles.Count; j++) {
                    HexTile hexInSettlement = owner.homeSettlement.tiles[j];
                    for (int k = 0; k < hexInSettlement.itemsInHex.Count; k++) {
                        TileObject itemInHex = hexInSettlement.itemsInHex[k];
                        if (itemInHex.name == itemName) {
                            if (itemInHex.gridTileLocation != null && itemInHex.IsOwnedBy(owner) && itemInHex.gridTileLocation.structure == owner.homeStructure) {
                                //Should not obtain personal item if item is already personally owned is in the home structure of the owner
                                continue;
                            }
                            itemHasBeenAdded = true;
                            obtainPersonalItemUnownedRandomList.Add(itemName);
                            break;
                        }
                    }
                    if (itemHasBeenAdded) {
                        break;
                    }
                }
            }
            if (obtainPersonalItemUnownedRandomList.Count > 0) {
                return obtainPersonalItemUnownedRandomList[UnityEngine.Random.Range(0, obtainPersonalItemUnownedRandomList.Count)];
            }
        }
        return string.Empty;
    }
    #endregion

    #region Hide At Home
    public bool CreateHideAtHomeJob() {
        if (owner.homeStructure != null && !owner.homeStructure.hasBeenDestroyed && owner.homeStructure.tiles.Count > 0) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HIDE_AT_HOME, INTERACTION_TYPE.RETURN_HOME, owner, owner);
            owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;

    }
    #endregion

    #region Idle
    public bool TriggerStandStill() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.STAND_STILL)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND_STILL], owner, owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND_STILL, INTERACTION_TYPE.STAND_STILL, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerStandStill(out JobQueueItem producedJob) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.STAND_STILL)) {
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND_STILL], owner, owner, null, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND_STILL, INTERACTION_TYPE.STAND_STILL, owner, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Undermine
    public bool CreateUndermineJob(Character targetCharacter, string reason) {
        if (owner.jobQueue.HasJob(JOB_TYPE.UNDERMINE, targetCharacter)) {
            return false;
        }
        if (targetCharacter.isDead || owner.traitContainer.HasTrait("Diplomatic")) {
            return false;
        }
        if (targetCharacter.homeRegion == null) {
            targetCharacter.logComponent.PrintLogIfActive(owner.name + " cannot undermine " + targetCharacter.name + " because he/she does not have a home region");
            return false;
        }
        IPointOfInterest chosenObject = null;
        for (int i = 0; i < targetCharacter.ownedItems.Count; i++) {
            TileObject item = targetCharacter.ownedItems[i];
            if (item.gridTileLocation != null && item.mapVisual) {
                chosenObject = item;
                break;
            }
        }
        //IPointOfInterest chosenObject = targetCharacter.homeRegion.GetFirstTileObjectOnTheFloorOwnedBy(targetCharacter);
        if (chosenObject == null) {
            targetCharacter.logComponent.PrintLogIfActive(owner.name + " cannot undermine " + targetCharacter.name + " because he/she does not have an owned item on the floor in his/her home region");
            return false;
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET), chosenObject, owner);
        owner.jobQueue.AddJobInQueue(job);

        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", $"{reason}_and_undermine", null, LOG_TAG.Social, LOG_TAG.Crimes);
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToDatabase();
        // owner.logComponent.AddHistory(log);
        return true;
    }
    private bool CreatePlaceTrapPOIJob(IPointOfInterest target, JOB_TYPE jobType = JOB_TYPE.PLACE_TRAP) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET), target, owner);
        owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    private bool CreatePlaceTrapPOIJob(IPointOfInterest target, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET), target, owner);
	    producedJob = job;
	    return true;
    }
    public bool CreatePlaceTrapJob(Character targetCharacter, JOB_TYPE jobType = JOB_TYPE.PLACE_TRAP) {
        IPointOfInterest chosenObject = null;
        for (int i = 0; i < targetCharacter.ownedItems.Count; i++) {
            TileObject item = targetCharacter.ownedItems[i];
            if (item.gridTileLocation != null && item.mapVisual) {
                chosenObject = item;
                break;
            }
        }
        if(chosenObject != null) {
            return CreatePlaceTrapPOIJob(chosenObject, jobType);
        }
        return false;
    }
    public bool CreatePlaceTrapJob(Character targetCharacter, out JobQueueItem producedJob) {
	    IPointOfInterest chosenObject = null;
	    for (int i = 0; i < targetCharacter.ownedItems.Count; i++) {
		    TileObject item = targetCharacter.ownedItems[i];
		    if (item.gridTileLocation != null && item.mapVisual) {
			    chosenObject = item;
			    break;
		    }
	    }
	    if(chosenObject != null) {
		    return CreatePlaceTrapPOIJob(chosenObject, out producedJob);
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Report Demonic Structure
    public void CreateReportDemonicStructure(LocationStructure structureToReport) {
        NPCSettlement homeSettlement = owner.homeSettlement;
	    if (canReportDemonicStructure && homeSettlement != null && homeSettlement.mainStorage != null && !owner.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE)) {
		    // UIManager.Instance.ShowYesNoConfirmation("Demonic Structure Seen", 
			   //  $"Your demonic structure {structureToReport.name} has been seen by {_owner.name}!", 
			   //  onClickNoAction: _owner.CenterOnCharacter, yesBtnText: "OK", noBtnText: $"Jump to {_owner}", 
			   //  showCover:true, pauseAndResume: true);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE, INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE, owner, owner);
            job.AddOtherData(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE, new object[] { structureToReport, homeSettlement.mainStorage });
            owner.jobQueue.AddJobInQueue(job);
            Messenger.Broadcast(JobSignals.DEMONIC_STRUCTURE_DISCOVERED, structureToReport, owner, job);
        }
    }
    /// <summary>
    /// Disable report demonic structure until this character steps foot in his/her home.
    /// </summary>
    public void DisableReportStructure() {
	    canReportDemonicStructure = false;
	    Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, TryEnableReportStructure);
    }
    public void EnableReportStructure() {
	    canReportDemonicStructure = true;
	    Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, TryEnableReportStructure);
    }
    private void TryEnableReportStructure(Character character, LocationStructure structure) {
	    if (character == owner) {
		    if (character.homeStructure != null && structure == character.homeStructure) {
			    EnableReportStructure();    
		    }
	    }
    }
    #endregion

    #region Heal Self
    public void SetDoNotDoRecoverHPJob(bool state) {
        if(doNotDoRecoverHPJob != state) {
            doNotDoRecoverHPJob = state;
            if (doNotDoRecoverHPJob) {
                GameDate dueDate = GameManager.Instance.Today();
                dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
                SchedulingManager.Instance.AddEntry(dueDate, () => SetDoNotDoRecoverHPJob(false), null);
            }
        }
    }
    public void OnHPReduced() {
        if (!doNotDoRecoverHPJob) {
            if (owner.jobQueue.HasJob(JOB_TYPE.RECOVER_HP) == false && owner.isNormalCharacter && owner.currentHP > 0 && owner.currentHP < Mathf.FloorToInt(owner.maxHP * 0.5f)) {
                CreateHealSelfJob();
            }
        }
    }
    private void CreateHealSelfJob() {
        //Creating heal self job should only for sapient since sapient are the only one that advertises it anyway
        //This is so that non sapient will no longer try to heal self if they do not advertise it so that they wont drop their current action
        //Example: Poisoned Ratman while carrying abducted character will drop carried character because he will try to heal himself but since he cannot do it, he will no longer resume the abduction because the Monster Abduct job is not recalculatable
        if (owner.race.IsSapient()) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECOVER_HP, INTERACTION_TYPE.HEAL_SELF, owner, owner);
            JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(owner, job, INTERACTION_TYPE.PICK_UP);
            job.SetStillApplicableChecker(JobManager.Heal_Self_Applicability);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
    #endregion
    
    #region Undermine
    private GoapPlanJob CreatePoisonFood(Character targetCharacter, JOB_TYPE jobType) {
        if (owner.jobQueue.HasJob(jobType, targetCharacter)) {
            return null;
        }
        if (targetCharacter.isDead) {
            return null;
        }
        if (targetCharacter.homeRegion == null) {
            targetCharacter.logComponent.PrintLogIfActive(owner.name + " cannot poison food " + targetCharacter.name + " because he/she does not have a home region");
            return null;
        }
        IPointOfInterest chosenObject = null;
        for (int i = 0; i < targetCharacter.ownedItems.Count; i++) {
            TileObject item = targetCharacter.ownedItems[i];
            if (item.gridTileLocation != null && item.mapVisual && item.advertisedActions.Contains(INTERACTION_TYPE.EAT)) {
                chosenObject = item;
                break;
            }
        }
        if (chosenObject == null) {
            //if no owned items was found, check the items in character's home structure if he/she has one
            if (targetCharacter.homeStructure != null) {
                List<TileObject> tileObjects = targetCharacter.homeStructure.GetTileObjectsOfType<TileObject>(
                    item => item.gridTileLocation != null && item.mapVisual &&
                            item.advertisedActions.Contains(INTERACTION_TYPE.EAT));
                if (tileObjects.Count > 0) {
                    chosenObject = CollectionUtilities.GetRandomElement(tileObjects);
                }
            }
            if (chosenObject == null) {
                targetCharacter.logComponent.PrintLogIfActive(owner.name + " cannot poison food " +
                                                              targetCharacter.name +
                                                              " because he/she does not have an owned item on the floor in his/her home region");
                return null;
            }
        }
        return JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Poisoned", false, GOAP_EFFECT_TARGET.TARGET), chosenObject, owner);
    }
    public bool CreatePoisonFoodJob(Character targetCharacter, JOB_TYPE jobType = JOB_TYPE.POISON_FOOD) {
        GoapPlanJob job = CreatePoisonFood(targetCharacter, jobType);
        if(job != null) {
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool CreatePoisonFoodJob(Character targetCharacter, out JobQueueItem producedJob) {
	    producedJob = CreatePoisonFood(targetCharacter, JOB_TYPE.POISON_FOOD);
	    return producedJob != null;
    }
    #endregion

    #region Share Info
    public bool CreateSpreadNegativeInfoJob(Character targetCharacter, ActualGoapNode negativeInfo) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (negativeInfo == null) {
            return false;
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SHARE_NEGATIVE_INFO, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, owner);
        job.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[] { negativeInfo });
        owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    public bool CreateSpreadRumorJob(Character targetCharacter, Rumor rumor) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (rumor == null) {
            return false;
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPREAD_RUMOR, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, owner);
        job.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[] { rumor });
        owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    public bool CreateSpreadRumorJob(Character targetCharacter, Rumor rumor, out JobQueueItem producedJob) {
        producedJob = null;
        if (targetCharacter.isDead) {
            return false;
        }
        if (rumor == null) {
            return false;
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPREAD_RUMOR, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, owner);
        job.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[] { rumor });
        producedJob = job;
        return true;
    }
    public bool CreateConfirmRumorJob(Character targetCharacter, ActualGoapNode action) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (action == null) {
            return false;
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CONFIRM_RUMOR, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, owner);
        job.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[] { action });
        owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    public void TryCreateReportCrimeJob(Character actor, IPointOfInterest target, CrimeData crimeData, ICrimeable crime) {
        if(owner.crimeComponent.CanCreateReportCrimeJob(actor, target, crimeData, crime)) {
            GoapPlanJob job = CreateReportCrimeJob(crimeData, crime);
            if (job != null) {
	            owner.jobQueue.AddJobInQueue(job);
            }
        } else {
            Log addLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing", null, LOG_TAG.Crimes);
            addLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            addLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            addLog.AddToFillers(null, crimeData.crimeTypeObj.name, LOG_IDENTIFIER.STRING_1);
            addLog.AddLogToDatabase();
            owner.crimeComponent.AddReportedCrime(crimeData);
            if (owner.characterClass.className == "Shaman" && owner.relationshipContainer.IsFriendsWith(actor) && 
                (crimeData.crimeType == CRIME_TYPE.Vampire || crimeData.crimeType == CRIME_TYPE.Werewolf)) {
	            //Create Cure Magical Affliction job
	            string traitToRemove = string.Empty;
	            if (actor.traitContainer.HasTrait("Vampire")) {
		            traitToRemove = "Vampire";
	            } else if (actor.traitContainer.HasTrait("Lycanthrope")) {
		            traitToRemove = "Lycanthrope";
	            }
	            if (!string.IsNullOrEmpty(traitToRemove)) {
		            owner.jobComponent.TriggerCureMagicalAffliction(actor, traitToRemove);    
	            }
            }
        }
    }
    /// <summary>
    /// Try to create a job to report a crime.
    /// </summary>
    /// <param name="actor">The criminal</param>
    /// <param name="target">The target of the crime</param>
    /// <param name="crimeData">The crime data</param>
    /// <param name="crime">The crime committed.</param>
    /// <param name="producedJob">The created job.</param>
    /// <returns>Whether or not a job was created. NOTE: This doesn't always guarantee that a
    /// report job was created even if it returns true. It can sometimes create a Cure Magical Affliction Job.</returns>
    public bool TryCreateReportCrimeJob(Character actor, IPointOfInterest target, CrimeData crimeData, ICrimeable crime, out JobQueueItem producedJob) {
        producedJob = null;
        if (owner.crimeComponent.CanCreateReportCrimeJob(actor, target, crimeData, crime)) {
            producedJob = CreateReportCrimeJob(crimeData, crime);
            if(producedJob != null) {
                return true;
            }
        } else {
            Log addLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "CrimeSystem", "report_do_nothing", null, LOG_TAG.Crimes);
            addLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            addLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            addLog.AddToFillers(null, crimeData.crimeTypeObj.name, LOG_IDENTIFIER.STRING_1);
            addLog.AddLogToDatabase();
            owner.crimeComponent.AddReportedCrime(crimeData);
            
            //Try to Create Cure Magical Affliction job
            if (owner.characterClass.className == "Shaman" && owner.relationshipContainer.IsFriendsWith(actor) && 
                (crimeData.crimeType == CRIME_TYPE.Vampire || crimeData.crimeType == CRIME_TYPE.Werewolf)) {
	            string traitToRemove = string.Empty;
	            if (actor.traitContainer.HasTrait("Vampire")) {
		            traitToRemove = "Vampire";
	            } else if (actor.traitContainer.HasTrait("Lycanthrope")) {
		            traitToRemove = "Lycanthrope";
	            }
	            if (!string.IsNullOrEmpty(traitToRemove)) {
		            return TriggerCureMagicalAffliction(actor, traitToRemove, out producedJob);    
	            }
            }
        }
        return false;
    }
    private GoapPlanJob CreateReportCrimeJob(CrimeData crimeData, ICrimeable crime) {
        if(owner.faction != null && owner.faction.isMajorNonPlayer) {
            if (crimeData.IsWantedBy(owner.faction)) {
                //Should no longer report if already wanted by the source's faction
                return null;
            }
        }
        bool canReportToFactionLeader = false;
        bool canReportToSettlementRuler = false;
        if (owner.faction != null && owner.faction.isMajorNonPlayer && owner.faction.leader != null && owner.faction.leader is Character characterLeader
            && !characterLeader.isDead && !characterLeader.traitContainer.HasTrait("Travelling") && characterLeader != owner && characterLeader != crimeData.criminal && characterLeader != crimeData.target) {
            if(!crimeData.IsWitness(characterLeader)) {
                canReportToFactionLeader = true;
            }
        }
        if (owner.homeSettlement != null && owner.homeSettlement.ruler != null && !owner.homeSettlement.ruler.isDead && !owner.homeSettlement.ruler.traitContainer.HasTrait("Travelling") && owner.homeSettlement.ruler != owner && owner.homeSettlement.ruler != crimeData.criminal && owner.homeSettlement.ruler != crimeData.target) {
            if (!crimeData.IsWitness(owner.homeSettlement.ruler)) {
                canReportToSettlementRuler = true;
            }
        }
        Character targetCharacter = null;
        if(canReportToFactionLeader && canReportToSettlementRuler) {
            if(UnityEngine.Random.Range(0, 2) == 0) {
                targetCharacter = owner.faction.leader as Character;
            } else {
                targetCharacter = owner.homeSettlement.ruler;
            }
        } else {
            if (canReportToFactionLeader) {
                targetCharacter = owner.faction.leader as Character;
            } else if (canReportToSettlementRuler) {
                targetCharacter = owner.homeSettlement.ruler;
            }
        }
        if(targetCharacter != null) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPORT_CRIME, INTERACTION_TYPE.REPORT_CRIME, targetCharacter, owner);
            job.AddOtherData(INTERACTION_TYPE.REPORT_CRIME, new object[] { crime, crimeData });
            return job;
        }
        return null;
    }
    #endregion

    #region Visit Different Region
    public bool TriggerVisitDifferentRegion() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.VISIT_DIFFERENT_REGION)) {
            Region chosenRegion = null;
            List<Region> adjacentRegions = owner.currentRegion.neighbours;
            if(adjacentRegions != null && adjacentRegions.Count > 0) {
                chosenRegion = adjacentRegions[UnityEngine.Random.Range(0, adjacentRegions.Count)];
            }
            if(chosenRegion != null) {
                HexTile hex = chosenRegion.GetRandomHexThatMeetCriteria(currHex => currHex.elevationType != ELEVATION.WATER && currHex.elevationType != ELEVATION.MOUNTAIN);
                if(hex != null) {
                    LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(hex.locationGridTiles);
                    if (owner.gridTileLocation != null && owner.movementComponent.HasPathToEvenIfDiffRegion(chosenTile)) {
                        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], owner, owner, new OtherData[] { new LocationGridTileOtherData(chosenTile) }, 0);
                        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.VISIT_DIFFERENT_REGION, INTERACTION_TYPE.ROAM, owner, owner);
                        goapPlan.SetDoNotRecalculate(true);
                        job.SetCannotBePushedBack(true);
                        job.SetAssignedPlan(goapPlan);
                        owner.jobQueue.AddJobInQueue(job);
                        return true;
                    }
                }
            }
        } else {
            //If already has a Visit Different Region Job in queue, return true so that the character will not Roam Around Tile
            return true;
        }
        return false;
    }
    #endregion

    #region Bury
    public void TriggerBuryMe() {
	    if (owner.minion == null && !(owner is Animal) && owner.gridTileLocation != null && owner.gridTileLocation.IsNextToOrPartOfSettlement(out var settlement)
	        && settlement is NPCSettlement npcSettlement) {
		    LocationStructure targetStructure = npcSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CULT_TEMPLE) ??
                                                npcSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) ?? 
		                                        npcSettlement.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
		    GoapPlanJob buryJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, owner, npcSettlement);
		    buryJob.SetCanTakeThisJobChecker(JobManager.Can_Take_Bury_Job);
		    buryJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[]{ targetStructure });
		    buryJob.SetStillApplicableChecker(JobManager.Bury_Settlement_Applicability);
		    npcSettlement.AddToAvailableJobs(buryJob);
	    }
    }
    public void TriggerPersonalBuryJob(Character targetCharacter) {
        if (owner.gridTileLocation != null && !owner.jobQueue.HasJob(JOB_TYPE.BURY, targetCharacter)) {
            LocationStructure targetStructure = null;
            if (owner.homeSettlement != null) {
                targetStructure = owner.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CULT_TEMPLE) ??
                                    owner.homeSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
            }
            if (targetStructure == null) {
	            targetStructure = owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            }
            if (owner.movementComponent.HasPathToEvenIfDiffRegion(targetStructure.GetRandomPassableTile())) {
                GoapPlanJob buryJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, targetCharacter, owner);
                buryJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[] { targetStructure });
                buryJob.SetStillApplicableChecker(JobManager.Bury_Applicability);
                owner.jobQueue.AddJobInQueue(buryJob);
            }
        }
    }
    public void TriggerPersonalBuryInActivePartyJob(Character targetCharacter) {
        if (owner.gridTileLocation != null && !owner.jobQueue.HasJob(JOB_TYPE.BURY_IN_ACTIVE_PARTY, targetCharacter)) {
            GoapPlanJob buryJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY_IN_ACTIVE_PARTY, INTERACTION_TYPE.BURY_CHARACTER, targetCharacter, owner);
            buryJob.SetStillApplicableChecker(JobManager.Bury_Applicability);
            owner.jobQueue.AddJobInQueue(buryJob);
        }
    }
    #endregion

    #region Go To
    public bool CreateGoToJob(IPointOfInterest target) {
        if(!owner.jobQueue.HasJob(JOB_TYPE.GO_TO, target)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO, target, owner);
            job.SetCannotBePushedBack(true);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool CreateGoToJob(LocationGridTile tile, out JobQueueItem producedJob) {
	    if(!owner.jobQueue.HasJob(JOB_TYPE.GO_TO)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.genericTileObject, owner);
            job.SetCannotBePushedBack(true);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool CreateGoToJob(LocationGridTile tile) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.GO_TO)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.genericTileObject, owner);
            job.SetCannotBePushedBack(true);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool CreatePartyGoToJob(LocationGridTile tile, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.PARTY_GO_TO)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTY_GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.genericTileObject, owner);
            job.SetCannotBePushedBack(true);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool CreatePartyGoToJob(LocationGridTile tile) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.PARTY_GO_TO)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTY_GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.genericTileObject, owner);
            job.SetCannotBePushedBack(true);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool CreatePartyGoToSpecificTileJob(LocationGridTile tile, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.PARTY_GO_TO)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTY_GO_TO, INTERACTION_TYPE.GO_TO_SPECIFIC_TILE, tile.genericTileObject, owner);
            job.SetCannotBePushedBack(true);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool CreateGoToWaitingJob(LocationGridTile tile, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.GO_TO_WAITING)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO_WAITING, INTERACTION_TYPE.GO_TO_TILE, tile.genericTileObject, owner);
            job.SetCannotBePushedBack(true);
            job.SetDoNotRecalculate(true);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool CreateFleeCrimeJob(LocationGridTile tile) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.FLEE_CRIME)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FLEE_CRIME, INTERACTION_TYPE.FLEE_CRIME, tile.genericTileObject, owner);
            job.SetCannotBePushedBack(true);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    #region Build
    public void TriggerSpawnLair(LocationGridTile targetTile) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_LAIR, owner, owner);
            job.AddOtherData(INTERACTION_TYPE.BUILD_LAIR, new object[] { targetTile });
            owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void TriggerSpawnLair(LocationGridTile targetTile, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_LAIR, owner, owner);
		    job.AddOtherData(INTERACTION_TYPE.BUILD_LAIR, new object[] { targetTile });
		    producedJob = job;
	    }
    }
    #endregion

    #region Necromancer
    public bool TriggerAbsorbLife(out JobQueueItem producedJob) {
        producedJob = null;
        if (!owner.jobQueue.HasJob(JOB_TYPE.ABSORB_LIFE)) {
            GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_LIFE, effect, owner, owner);
            producedJob = job;
            return true;
        }
        return false;
    }
    public bool TriggerAbsorbLife(IPointOfInterest target) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.ABSORB_LIFE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_LIFE, INTERACTION_TYPE.ABSORB_LIFE, target, owner);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public void TriggerAbsorbLife(IPointOfInterest target, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!owner.jobQueue.HasJob(JOB_TYPE.ABSORB_LIFE)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_LIFE, INTERACTION_TYPE.ABSORB_LIFE, target, owner);
		    producedJob = job;
	    }
    }
    public bool TriggerSpawnSkeleton() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_SKELETON)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_SKELETON, INTERACTION_TYPE.SPAWN_SKELETON, owner, owner);
            owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerSpawnSkeleton(out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_SKELETON)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_SKELETON, INTERACTION_TYPE.SPAWN_SKELETON, owner, owner);
		    producedJob = job;
		    return true;
	    }
	    return false;
    }
    public void TriggerRaiseCorpse(IPointOfInterest target) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.RAISE_CORPSE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RAISE_CORPSE, INTERACTION_TYPE.RAISE_CORPSE, target, owner);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void TriggerRaiseCorpse(IPointOfInterest target, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!owner.jobQueue.HasJob(JOB_TYPE.RAISE_CORPSE)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RAISE_CORPSE, INTERACTION_TYPE.RAISE_CORPSE, target, owner);
		    producedJob = job;
	    }
    }
    public void TriggerAbsorbPower(IPointOfInterest target) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.ABSORB_POWER)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_POWER, INTERACTION_TYPE.ABSORB_POWER, target, owner);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void TriggerAbsorbPower(IPointOfInterest target, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!owner.jobQueue.HasJob(JOB_TYPE.ABSORB_POWER)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_POWER, INTERACTION_TYPE.ABSORB_POWER, target, owner);
		    producedJob = job;
	    }
    }
    public bool TriggerReadNecronomicon(out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.READ_NECRONOMICON, owner, owner);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerMeditate(out JobQueueItem producedJob) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.MEDITATE, owner, owner);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerRegainEnergy(out JobQueueItem producedJob) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.REGAIN_ENERGY, owner, owner);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Apprehend
    public bool TryCreateApprehend(Character target, ref bool canDoJob, LocationStructure intendedPrison = null) {
        if (owner.partyComponent.hasParty && owner.partyComponent.currentParty.isActive && owner.partyComponent.currentParty.DidMemberJoinQuest(owner)) {
            return false;
        }
        LocationStructure prison = intendedPrison;
        Prisoner prisonerStatus = target.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
        if(prison == null) {
            if (prisonerStatus != null) {
                prison = prisonerStatus.GetIntendedPrisonAccordingTo(owner);
            } else {
                prison = owner.GetSettlementPrisonFor(target);
            }
        }

        canDoJob = InteractionManager.Instance.CanCharacterTakeApprehendJob(owner, target) && prison != null && CanDoJob(JOB_TYPE.APPREHEND);
        if (canDoJob) {
            if (owner.jobQueue.HasJob(JOB_TYPE.APPREHEND, target) == false) {
                bool isCriminal = target.traitContainer.HasTrait("Criminal") && target.crimeComponent.IsWantedBy(owner.faction);
                bool isPrisoner = prisonerStatus != null && prisonerStatus.IsConsideredPrisonerOf(owner);
                if (isCriminal || isPrisoner) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP_RESTRAINED, target, owner);
                    job.SetStillApplicableChecker(JobManager.Apprehend_Applicability);
                    job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { prison });
                    return owner.jobQueue.AddJobInQueue(job);
                }
            }
        }
        return false;
    }
    #endregion

    #region Sabotage
    public bool TryCreateSabotageNeighbourJob(Character target, out JobQueueItem producedJob) {
        producedJob = null;
	    //create predetermined plan and job
	    List<JobNode> jobNodes = new List<JobNode>();
	    if (owner.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT) == false) {
		    //Pick up cultist kit at home
		    TileObject cultistKitAtHome = owner.homeStructure?.GetTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT);
		    Assert.IsNotNull(cultistKitAtHome, $"{owner.name} wants to sabotage neighbour but has no cultist kit at home or in inventory. This should never happen, because the Cultist Behaviour checks this beforehand");
		    ActualGoapNode pickupNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PICK_UP], owner, cultistKitAtHome, null, 0);
		    jobNodes.Add(new SingleJobNode(pickupNode));
	    }

        string buffToBeRemoved = string.Empty;
        List<Trait> buffs = target.traitContainer.GetAllTraitsOf(TRAIT_TYPE.BUFF);
        if(buffs != null && buffs.Count > 0) {
            Trait randomBuff = CollectionUtilities.GetRandomElement(buffs);
            if (randomBuff != null) {
                buffToBeRemoved = randomBuff.name;
            }
        }
        if(buffToBeRemoved != string.Empty) {
            ActualGoapNode removeBuffNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.REMOVE_BUFF], owner, target, new OtherData[] { new StringOtherData(buffToBeRemoved) }, 0);
            jobNodes.Add(new SingleJobNode(removeBuffNode));
            GoapPlan goapPlan = new GoapPlan(jobNodes, target);
            goapPlan.SetDoNotRecalculate(true);

            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SABOTAGE_NEIGHBOUR, INTERACTION_TYPE.REMOVE_BUFF, target, owner);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        return false;
    }
    private bool IsValidSabotageNeighbourTarget(Character character) {
	    AWARENESS_STATE awarenessState = owner.relationshipContainer.GetAwarenessState(character);
	    return character.isNormalCharacter && !character.traitContainer.HasTrait("Travelling") && 
	           character.traitContainer.HasTrait("Resting", "Unconscious") &&
	           character.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF) &&
	           character.traitContainer.HasTrait("Cultist") == false && owner.HasSameHomeAs(character) &&
	           awarenessState != AWARENESS_STATE.Missing && awarenessState != AWARENESS_STATE.Presumed_Dead;
    }
    public bool TryGetValidSabotageNeighbourTarget(out Character targetCharacter) {
	    List<Character> choices = null;
	    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
		    Character character = CharacterManager.Instance.allCharacters[i];
		    if (IsValidSabotageNeighbourTarget(character)) {
			    if (choices == null) { choices = new List<Character>(); }
			    choices.Add(character);
		    }
	    }

	    if (choices != null) {
		    WeightedDictionary<Character> targetWeights = new WeightedDictionary<Character>();
		    for (int i = 0; i < choices.Count; i++) {
			    Character character = choices[i];
			    int weight = 0;
			    string opinionLabel = owner.relationshipContainer.GetOpinionLabel(character);
			    if (opinionLabel == RelationshipManager.Close_Friend) {
				    weight += Random.Range(10, 51);
			    } else if (opinionLabel == RelationshipManager.Friend) {
				    weight += Random.Range(100, 151);
			    } else if (opinionLabel == RelationshipManager.Acquaintance) {
				    weight += Random.Range(150, 251);
			    } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
				    weight += Random.Range(200, 351);
			    }
			    targetWeights.AddElement(character, weight);
		    }
		    targetWeights.LogDictionaryValues($"{GameManager.Instance.TodayLogString()}{owner.name}'s Sabotage Neighbour Weights:");
		    if (targetWeights.GetTotalOfWeights() > 0) {
			    targetCharacter = targetWeights.PickRandomElementGivenWeights();
			    return true;
		    }
	    }
	    targetCharacter = null;
	    return false;
    }
    #endregion

    #region Pray
    public void TriggerPray() {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, INTERACTION_TYPE.PRAY, owner, owner);
	    ActualGoapNode prayNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PRAY], owner, owner, null, 0);
	    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(prayNode) }, owner);
	    goapPlan.SetDoNotRecalculate(true);
	    job.SetCannotBePushedBack(true);
	    owner.jobQueue.AddJobInQueue(job);
    }
    #endregion

    #region Spawn Objects
    public bool TriggerSpawnPoisonCloud(out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.SPAWN_POISON_CLOUD, owner, owner);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    #endregion
    
    #region Decrease Mood
    public bool TriggerDecreaseMood(out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DECREASE_MOOD,
		    INTERACTION_TYPE.DECREASE_MOOD, owner, owner);
	    producedJob = job;
	    return true;
    }
    private bool IsDecreaseMoodJobInTerritoryStillApplicable(Character target) {
        HexTile hex = target.hexTileLocation;
        return hex != null && owner.IsTerritory(hex);
    }
    #endregion

    #region Disable
    public bool TriggerDisable(out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DISABLE,
		    INTERACTION_TYPE.DISABLE, owner, owner);
	    producedJob = job;
	    return true;
    }
    #endregion

    #region Monsters
    public bool TriggerLayEgg(out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.LAY_EGG, owner, owner);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerMonsterAbduct(Character targetCharacter, out JobQueueItem producedJob, LocationGridTile targetTile = null) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacter, owner);
	    job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, targetTile != null ? new object[] {targetTile.structure, targetTile} : new object[] {owner.homeStructure});
        job.SetDoNotRecalculate(true);
	    producedJob = job;
	    return true;
    }
    public bool TriggerEatAlive(Character webbedCharacter, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT, INTERACTION_TYPE.EAT_ALIVE, webbedCharacter, owner);
	    producedJob = job;
	    return true;
    }
    public bool TriggerTorture(Character targetCharacter, out JobQueueItem producedJob) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TORTURE, INTERACTION_TYPE.TORTURE, targetCharacter, owner);
        producedJob = job;
        return true;
    }
    public bool TriggerBirthRatman(out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.BIRTH_RATMAN, owner, owner);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerEatCorpse(Character targetCharacter) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.MONSTER_EAT_CORPSE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT_CORPSE, INTERACTION_TYPE.EAT_CORPSE, targetCharacter, owner);
            job.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            if (owner.jobQueue.AddJobInQueue(job)) {
	            owner.jobQueue.CancelAllJobs(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, JOB_TYPE.FULLNESS_RECOVERY_URGENT);
	            return true;
            }
            return false;
        }
        return false;
    }
    #endregion

    #region Arson
    public bool TriggerArson(TileObject target, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ARSON,
		    INTERACTION_TYPE.BURN, target, owner);
	    producedJob = job;
	    return true;
    }
    #endregion

    #region Seek Shelter
    public bool TriggerSeekShelterJob() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.SEEK_SHELTER) && owner.gridTileLocation != null) {
            List<LocationStructure> exclusions = null;
            string traitCause = string.Empty;
            if (owner.traitContainer.HasTrait("Freezing")) {
                Freezing freezing = owner.traitContainer.GetTraitOrStatus<Freezing>("Freezing");
                exclusions = freezing.excludedStructuresInSeekingShelter;
                traitCause = "Freezing";
            } else if (owner.traitContainer.HasTrait("Overheating")) {
                Overheating overheating = owner.traitContainer.GetTraitOrStatus<Overheating>("Overheating");
                exclusions = overheating.excludedStructuresInSeekingShelter;
                traitCause = "Overheating";
            }
            LocationStructure nearestInteriorStructure = owner.gridTileLocation.GetNearestInteriorStructureFromThisExcept(exclusions);
            if(nearestInteriorStructure != null && !string.IsNullOrEmpty(traitCause)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SEEK_SHELTER, INTERACTION_TYPE.TAKE_SHELTER, owner, owner);
                job.AddOtherData(INTERACTION_TYPE.TAKE_SHELTER, new object[] { nearestInteriorStructure, traitCause });
                owner.jobQueue.AddJobInQueue(job);
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Dark Ritual
    public bool TryCreateDarkRitualJob(out JobQueueItem producedJob) {
	    if (owner.currentRegion != null) {
		    MagicCircle magicCircle = null;
		    if (owner.currentRegion.HasTileObjectOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE)) {
			    List<MagicCircle> magicCircles = owner.currentRegion.GetTileObjectsOfType<MagicCircle>();
			    magicCircle = CollectionUtilities.GetRandomElement(magicCircles);
		    } else {
			    MagicCircle newCircle = InnerMapManager.Instance.CreateNewTileObject<MagicCircle>(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
			    List<LocationGridTile> choices = owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).unoccupiedTiles.ToList();
			    if (choices.Count > 0) {
				    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(choices);
				    targetTile.structure.AddPOI(newCircle, targetTile);
				    newCircle.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
				    magicCircle = newCircle;
			    }
		    }

		    if (magicCircle != null) {
			    GoapPlanJob ritualJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DARK_RITUAL, INTERACTION_TYPE.DARK_RITUAL, magicCircle, owner);

			    if (magicCircle.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
				    //if provided magic circle is unbuilt, add a pre-made plan to draw that magic circle.
				    ActualGoapNode drawNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRAW_MAGIC_CIRCLE], owner, magicCircle, null, 0);
				    ActualGoapNode ritualNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DARK_RITUAL], owner, magicCircle, null, 0);
				    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(drawNode), new SingleJobNode(ritualNode) }, magicCircle);
				    goapPlan.SetDoNotRecalculate(true);
				    // ritualJob.SetCannotBePushedBack(true);
				    ritualJob.SetAssignedPlan(goapPlan);
			    }

			    producedJob = ritualJob;
			    Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, CheckIfDarkRitualJobRemoved);
			    return true;
		    }
	    }
	    producedJob = null;
	    return false;
    }
    private void CheckIfDarkRitualJobRemoved(JobQueueItem job, Character character) {
	    if (character == owner && job.jobType == JOB_TYPE.DARK_RITUAL) {
		    //check if unbuilt magic circle is still valid, if any.
		    Messenger.Broadcast(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY);
		    Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, CheckIfDarkRitualJobRemoved);
	    }
	    
    }
    private bool IsUnbuiltMagicCircleStillValid(BaseMapObject mapObject) {
	    return owner.jobQueue.HasJob(JOB_TYPE.DARK_RITUAL);
    }
    #endregion

    #region Cultist
    public void TriggerCultistTransform() {
	    if (owner.jobQueue.HasJob(JOB_TYPE.CULTIST_TRANSFORM) == false) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CULTIST_TRANSFORM,
			    INTERACTION_TYPE.CULTIST_TRANSFORM, owner, owner);
		    owner.jobQueue.AddJobInQueue(job);
	    }
    }
    #endregion

    #region Party
    //public bool TriggerExploreJob(out JobQueueItem producedJob) { //bool forceDoAction = false
    //    if (!owner.partyComponent.hasParty) {
    //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.EXPLORE], owner, owner, null, 0);
    //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.EXPLORE, INTERACTION_TYPE.EXPLORE, owner, owner);
    //        goapPlan.SetDoNotRecalculate(true);
    //        job.SetCannotBePushedBack(true);
    //        job.SetAssignedPlan(goapPlan);
    //        producedJob = job;
    //        return true;
    //    }
    //    producedJob = null;
    //    return false;
    //}
    //public bool TriggerRescueJob(Character targetCharacter, out JobQueueItem producedJob) {
    //    if (!owner.partyComponent.hasParty) {
    //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RESCUE], owner, targetCharacter, null, 0);
    //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESCUE, INTERACTION_TYPE.RESCUE, targetCharacter, owner);
    //        goapPlan.SetDoNotRecalculate(true);
    //        job.SetCannotBePushedBack(true);
    //        job.SetAssignedPlan(goapPlan);
    //        producedJob = job;
    //        return true;
    //    }
    //    producedJob = null;
    //    return false;
    //}
    //public bool TriggerRescueJob(Character targetCharacter) {
    //    if (!owner.partyComponent.hasParty) {
    //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RESCUE], owner, targetCharacter, null, 0);
    //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESCUE, INTERACTION_TYPE.RESCUE, targetCharacter, owner);
    //        goapPlan.SetDoNotRecalculate(true);
    //        job.SetCannotBePushedBack(true);
    //        job.SetAssignedPlan(goapPlan);
    //        return owner.jobQueue.AddJobInQueue(job);
    //    }
    //    return false;
    //}
    public bool TriggerMonsterInvadeJob(LocationStructure targetStructure, out JobQueueItem producedJob) {
        if (!owner.partyComponent.hasParty) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MONSTER_INVADE], owner, owner, new OtherData[] { new LocationStructureOtherData(targetStructure), }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_INVADE, INTERACTION_TYPE.MONSTER_INVADE, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerMonsterInvadeJob(HexTile targetHex, out JobQueueItem producedJob) {
        if (!owner.partyComponent.hasParty) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MONSTER_INVADE], owner, owner, new OtherData[] { new HexTileOtherData(targetHex) }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_INVADE, INTERACTION_TYPE.MONSTER_INVADE, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerReleaseJob(Character targetCharacter) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.RELEASE_CHARACTER, targetCharacter)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RELEASE_CHARACTER], owner, targetCharacter, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RELEASE_CHARACTER, INTERACTION_TYPE.RELEASE_CHARACTER, targetCharacter, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool TriggerHostSocialPartyJob(out JobQueueItem producedJob) { //bool forceDoAction = false
        if (!owner.partyComponent.hasParty) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.HOST_SOCIAL_PARTY], owner, owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HOST_SOCIAL_PARTY, INTERACTION_TYPE.HOST_SOCIAL_PARTY, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    #endregion

    #region Disguise
    public bool TriggerDisguiseJob(Character targetCharacter, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DISGUISE], owner, targetCharacter, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.DISGUISE, targetCharacter, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    #endregion

    #region Succubus
    public bool TriggerMakeLoveJob(Character targetCharacter, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.MAKE_LOVE, targetCharacter, owner);
            job.SetCannotBePushedBack(true);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    #endregion

    #region Inspect
    public void TriggerInspect(TileObject item) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.INSPECT, item, owner);
	    //create predetermined plan
	    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.INSPECT],
		    owner, item, null, 0);
	    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, item);
	    goapPlan.SetDoNotRecalculate(true);
	    job.SetCannotBePushedBack(true);
	    job.SetAssignedPlan(goapPlan);
	    owner.jobQueue.AddJobInQueue(job);
    }
    #endregion

    #region Kidnap
    public bool TriggerKidnapJob(Character target) {
        if (owner.homeSettlement != null && owner.homeSettlement.prison != null) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.KIDNAP)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KIDNAP, INTERACTION_TYPE.DROP_RESTRAINED,
                    target, owner);
                job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { owner.homeSettlement.prison });
                return owner.jobQueue.AddJobInQueue(job);
            }
        }
        return false;
    }
    public bool TriggerKidnapRaidJob(Character target) {
        if (owner.homeSettlement != null && owner.homeSettlement.prison != null) {
            if (!owner.jobQueue.HasJob(JOB_TYPE.KIDNAP_RAID)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KIDNAP_RAID, INTERACTION_TYPE.DROP_RESTRAINED,
                    target, owner);
                job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { owner.homeSettlement.prison });
                return owner.jobQueue.AddJobInQueue(job);
            }
        }
        return false;
    }
    #endregion

    #region Disguise
    public bool TriggerRecruitJob(Character targetCharacter, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.RECRUIT)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RECRUIT], owner, targetCharacter, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECRUIT, INTERACTION_TYPE.RECRUIT, targetCharacter, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    #endregion

    #region Troll
    public bool TriggerBuildTrollCauldronJob(out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_TROLL_CAULDRON], owner, owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.BUILD_TROLL_CAULDRON, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerCookJob(Character targetCharacter, TileObject whereToCook, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, INTERACTION_TYPE.COOK, targetCharacter, owner);
            job.AddOtherData(INTERACTION_TYPE.COOK, new object[] { whereToCook });
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerRestrainJob(Character target, JOB_TYPE jobType) {
        if (!owner.jobQueue.HasJob(jobType)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.RESTRAIN_CHARACTER, target, owner);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    #region Partying
    public bool TriggerSingJob(out JobQueueItem producedJob) { //bool forceDoAction = false
        if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SING], owner, owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.SING, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerDanceJob(out JobQueueItem producedJob) { //bool forceDoAction = false
        if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DANCE], owner, owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.DANCE, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerPartyDrinkJob(Table table, out JobQueueItem producedJob) { //bool forceDoAction = false
        if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRINK], owner, table, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.DRINK, table, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    //public bool TriggerPartyEatJob(Table table, out JobQueueItem producedJob) { //bool forceDoAction = false
    //    if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING)) {
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.EAT, table, owner);
    //        job.SetCannotBePushedBack(true);
    //        producedJob = job;
    //        return true;
    //    }
    //    producedJob = null;
    //    return false;
    //}
    public bool TriggerPlayCardsJob(Desk desk, out JobQueueItem producedJob) { //bool forceDoAction = false
        if (!owner.jobQueue.HasJob(JOB_TYPE.PARTYING)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PLAY_CARDS], owner, desk, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PARTYING, INTERACTION_TYPE.PLAY_CARDS, desk, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    #endregion

    #region Craft Missing Furniture
    public bool CreateCraftMissingFurniture(TILE_OBJECT_TYPE tileObjectType, LocationStructure targetStructure, out JobQueueItem producedJob) {
	    TileObject unbuiltFurniture = InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType);
	    targetStructure.AddPOI(unbuiltFurniture);
	    unbuiltFurniture.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_MISSING_FURNITURE, INTERACTION_TYPE.CRAFT_TILE_OBJECT, unbuiltFurniture, owner);
        JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(owner, job, INTERACTION_TYPE.TAKE_RESOURCE);
        job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(tileObjectType).mainRecipe });
	    producedJob = job;
	    return true;
    }
    #endregion

    #region Wolf Lair
    public void TriggerSpawnWolfLair(LocationGridTile targetTile, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_WOLF_LAIR, owner, owner);
		    job.AddOtherData(INTERACTION_TYPE.BUILD_WOLF_LAIR, new object[] { targetTile });
		    producedJob = job;
	    }
    }
    #endregion

    #region Outside Home Region
    public bool TriggerDrinkJob(JOB_TYPE jobType, Table table, out JobQueueItem producedJob) { //bool forceDoAction = false
        if (!owner.jobQueue.HasJob(jobType)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRINK], owner, table, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.DRINK, table, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerBuildCampfireJob(JOB_TYPE jobType, out JobQueueItem producedJob) {
        if (!owner.jobQueue.HasJob(jobType)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_CAMPFIRE], owner, owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.BUILD_CAMPFIRE, owner, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerWarmUp(Campfire campfire, out JobQueueItem producedJob) { //bool forceDoAction = false
        if (!owner.jobQueue.HasJob(JOB_TYPE.WARM_UP)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.WARM_UP], owner, campfire, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.WARM_UP, INTERACTION_TYPE.WARM_UP, campfire, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    #endregion

    #region Evangelize
    public bool IsValidEvangelizeTarget(Character character) {
	    AWARENESS_STATE awarenessState = owner.relationshipContainer.GetAwarenessState(character);
	    return character.isNormalCharacter && !character.traitContainer.HasTrait("Travelling") && 
	           character.traitContainer.HasTrait("Cultist") == false && owner.HasSameHomeAs(character) &&
	           awarenessState != AWARENESS_STATE.Missing && awarenessState != AWARENESS_STATE.Presumed_Dead;
    }
    public bool TryGetValidEvangelizeTarget(out Character targetCharacter) {
	    List<Character> choices = null;
	    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
		    Character character = CharacterManager.Instance.allCharacters[i];
		    if (IsValidEvangelizeTarget(character)) {
			    if (choices == null) { choices = new List<Character>(); }
			    choices.Add(character);
		    }
	    }

	    if (choices != null) {
		    WeightedDictionary<Character> targetWeights = new WeightedDictionary<Character>();
		    for (int i = 0; i < choices.Count; i++) {
			    Character character = choices[i];
			    int weight = 0;
			    string opinionLabel = owner.relationshipContainer.GetOpinionLabel(character);
			    if (opinionLabel == RelationshipManager.Close_Friend) {
				    weight += Random.Range(300, 451);
			    } else if (opinionLabel == RelationshipManager.Friend) {
				    weight += Random.Range(100, 251);
			    } else if (opinionLabel == RelationshipManager.Acquaintance) {
				    weight += Random.Range(10, 51);
			    }
			    targetWeights.AddElement(character, weight);
		    }
		    targetWeights.LogDictionaryValues($"{GameManager.Instance.TodayLogString()}{owner.name}'s Evangelize Weights:");
		    if (targetWeights.GetTotalOfWeights() > 0) {
			    targetCharacter = targetWeights.PickRandomElementGivenWeights();
			    return true;
		    }
	    }
	    targetCharacter = null;
	    return false;
    }
    public bool TryCreateEvangelizeJob(Character target, out JobQueueItem producedJob) {
	    //create predetermined plan and job
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.EVANGELIZE, INTERACTION_TYPE.EVANGELIZE, target, owner);
	    // List<JobNode> jobNodes = new List<JobNode>();
	    // ActualGoapNode evangelizeNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.EVANGELIZE], owner, target, null, 0);
	    // jobNodes.Add(new SingleJobNode(evangelizeNode));
	    //
	    // GoapPlan goapPlan = new GoapPlan(jobNodes, target);
	    // goapPlan.SetDoNotRecalculate(true);
	    // job.SetCannotBePushedBack(true);
	    // job.SetAssignedPlan(goapPlan);
	    
	    producedJob = job;
	    return true;
    }
    public bool TryCreateEvangelizeJob(Character target) {
        //create predetermined plan and job
        if(!owner.jobQueue.HasJob(JOB_TYPE.EVANGELIZE, target)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.EVANGELIZE, INTERACTION_TYPE.EVANGELIZE, target, owner);
            // List<JobNode> jobNodes = new List<JobNode>();
            // ActualGoapNode evangelizeNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.EVANGELIZE], owner, target, null, 0);
            // jobNodes.Add(new SingleJobNode(evangelizeNode));
            //
            // GoapPlan goapPlan = new GoapPlan(jobNodes, target);
            // goapPlan.SetDoNotRecalculate(true);
            // job.SetCannotBePushedBack(true);
            // job.SetAssignedPlan(goapPlan);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    #region Snatch
    public bool CreateSnatchJob(Character targetCharacter, LocationGridTile targetLocation, LocationStructure structure) {
	    if (owner.jobQueue.HasJob(JOB_TYPE.SNATCH, targetCharacter) == false) {
		    owner.behaviourComponent.SetIsSnatching(true);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SNATCH, INTERACTION_TYPE.DROP, targetCharacter, owner);
		    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { structure, targetLocation });
		    owner.logComponent.PrintLogIfActive($"{owner.name} will do snatch job towards {targetCharacter.name}. Will drop at {structure.name}, ({targetLocation.localPlace.ToString()})");
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    #region Steal Raid
    public bool TriggerStealRaidJob(ResourcePile target) {
        if (owner.jobQueue.HasJob(JOB_TYPE.STEAL_RAID) == false && target.gridTileLocation.parentMap.region == owner.currentRegion && owner.homeSettlement != null) {
            ResourcePile chosenPileToDepositTo = owner.homeSettlement.mainStorage.GetResourcePileObjectWithLowestCount(target.tileObjectType);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL_RAID,
                new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), target, owner);
            if (chosenPileToDepositTo != null) {
                job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[] { chosenPileToDepositTo });
            }
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    #region Place Blueprint
    public bool TriggerPlaceBlueprint(string structurePrefabName, int connectorIndex, StructureSetting structureSetting, LocationGridTile centerTile, LocationGridTile connectorTile, out JobQueueItem producedJob) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.PLACE_BLUEPRINT)) {
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PLACE_BLUEPRINT], owner, centerTile.genericTileObject, 
			    new OtherData[]{ new StringOtherData(structurePrefabName), new LocationGridTileOtherData(connectorTile), new StructureSettingOtherData(structureSetting), }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, centerTile.genericTileObject);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.PLACE_BLUEPRINT, centerTile.genericTileObject, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Build Vampire Castle
    public bool TriggerBuildVampireCastle(LocationGridTile targetTile, out JobQueueItem producedJob, string structurePrefabName = "") {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.BUILD_VAMPIRE_CASTLE)) {
		    var otherData = new OtherData[] {new StringOtherData(structurePrefabName)};
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE], owner, targetTile.genericTileObject, otherData, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetTile.genericTileObject);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_VAMPIRE_CASTLE, INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE, targetTile.genericTileObject, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public void TriggerBuildVampireCastle(LocationGridTile targetTile, string structurePrefabName = "") {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.BUILD_VAMPIRE_CASTLE)) {
		    var otherData = new OtherData[] {new StringOtherData(structurePrefabName)};
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE], owner, targetTile.genericTileObject, otherData, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetTile.genericTileObject);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BUILD_VAMPIRE_CASTLE, INTERACTION_TYPE.BUILD_VAMPIRE_CASTLE, targetTile.genericTileObject, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetAssignedPlan(goapPlan);
		    owner.jobQueue.AddJobInQueue(job);
	    }
    }
    public bool TriggerImprisonBloodSource(out JobQueueItem producedJob, ref string log) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.IMPRISON_BLOOD_SOURCE)) {
		    log += $"\nWill try to imprison blood source";
		    WeightedDictionary<Character> choices = new WeightedDictionary<Character>();
		    foreach (var relationship in owner.relationshipContainer.relationships) {
			    Character otherCharacter = CharacterManager.Instance.GetCharacterByID(relationship.Key);
			    if (otherCharacter != null) {
				    if (!otherCharacter.isDead && otherCharacter.traitContainer.GetTraitOrStatus<Trait>("Vampire") == null) {
					    string opinion = relationship.Value.opinions.GetOpinionLabel();
					    if (opinion == RelationshipManager.Acquaintance || opinion == RelationshipManager.Enemy || opinion == RelationshipManager.Rival) {
						    int weight = 0;
						    if (opinion == RelationshipManager.Acquaintance) {
							    weight += 10;
						    } else if (opinion == RelationshipManager.Enemy) {
							    weight += 50;
						    } else if (opinion == RelationshipManager.Rival) {
							    weight += 100;
						    }
						    if (otherCharacter.homeSettlement != owner.homeSettlement) {
							    weight += 200;
						    }
						    if (otherCharacter.faction != owner.faction) {
							    weight *= 3;
						    }
						    if (weight > 0) {
							    choices.AddElement(otherCharacter, weight);
						    }    
					    }
				    }
			    }
		    }
		    
		    //Pick random animals
		    List<Character> animalsInRegion = owner.currentRegion.charactersAtLocation.Where(x => x is Animal && !x.isDead).ToList();
		    for (int i = 0; i < 3; i++) {
			    if (animalsInRegion.Count == 0) { break; }
			    Character animal = UtilityScripts.CollectionUtilities.GetRandomElement(animalsInRegion);
			    choices.AddElement(animal, 100);
			    animalsInRegion.Remove(animal);
		    }
		    log += $"\n{choices.GetWeightsSummary("Weights are:")}";
		    if (choices.GetTotalOfWeights() > 0) {
			    Character target = choices.PickRandomElementGivenWeights();
			    log += $"\nChosen target is {target.name}";
			    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IMPRISON_BLOOD_SOURCE, INTERACTION_TYPE.DROP, target, owner);
			    job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { owner.homeStructure });
			    producedJob = job;
			    return true;
		    }
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Building New Village
    public bool TriggerFindNewVillage(LocationGridTile targetTile, string structurePrefabName = "") {
        if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !owner.jobQueue.HasJob(JOB_TYPE.FIND_NEW_VILLAGE)) {
            var otherData = new OtherData[] { new StringOtherData(structurePrefabName) };
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_NEW_VILLAGE], owner, targetTile.genericTileObject, otherData, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetTile.genericTileObject);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FIND_NEW_VILLAGE, INTERACTION_TYPE.BUILD_NEW_VILLAGE, targetTile.genericTileObject, owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetAssignedPlan(goapPlan);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool TriggerFindNewVillage(LocationGridTile targetTile, out JobQueueItem producedJob, string structurePrefabName = "") {
	    if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && !owner.jobQueue.HasJob(JOB_TYPE.FIND_NEW_VILLAGE)) {
		    var otherData = new OtherData[] { new StringOtherData(structurePrefabName) };
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BUILD_NEW_VILLAGE], owner, targetTile.genericTileObject, otherData, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetTile.genericTileObject);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FIND_NEW_VILLAGE, INTERACTION_TYPE.BUILD_NEW_VILLAGE, targetTile.genericTileObject, owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Cure Magical Affliction
    public void TriggerCureMagicalAffliction(Character target, string traitName) {
	    //NOTE: Added checking for limbo since behaviour might constantly try to target an unavailable character, causing him/her to be stuck
	    if (!owner.jobQueue.HasJob(JOB_TYPE.CURE_MAGICAL_AFFLICTION) && !target.isInLimbo) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CURE_MAGICAL_AFFLICTION, INTERACTION_TYPE.DISPEL, target, owner);
		    job.AddOtherData(INTERACTION_TYPE.DISPEL, new object[]{ traitName });
		    owner.jobQueue.AddJobInQueue(job);
	    }
    }
    public bool TriggerCureMagicalAffliction(Character target, string traitName, out JobQueueItem producedJob) {
	    //NOTE: Added checking for limbo since behaviour might constantly try to target an unavailable character, causing him/her to be stuck
	    if (!owner.jobQueue.HasJob(JOB_TYPE.CURE_MAGICAL_AFFLICTION) && !target.isInLimbo) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CURE_MAGICAL_AFFLICTION, INTERACTION_TYPE.DISPEL, target, owner);
		    job.AddOtherData(INTERACTION_TYPE.DISPEL, new object[]{ traitName });
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Werewolf Hunt for Prey
    public bool TriggerHuntPreyJob(Character target) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.LYCAN_HUNT_PREY)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.LYCAN_HUNT_PREY, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), target, owner);
		    job.SetDoNotRecalculate(true);
		    owner.jobQueue.AddJobInQueue(job);
		    return true;
	    }
	    return false;
    }
    #endregion

    #region Rat
    public bool CreateRatFullnessRecovery(BaseSettlement targetSettlement, out JobQueueItem producedJob) {
        producedJob = null;
        if (!owner.jobQueue.HasJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_NORMAL, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), owner, owner);
            job.AddPriorityLocation(INTERACTION_TYPE.EAT, targetSettlement);
            producedJob = job;
            return true;
        }
        return false;
    }
    #endregion

    #region Quarantine
    public void TriggerQuarantineJob(Character target) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.QUARANTINE, target)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.QUARANTINE, INTERACTION_TYPE.QUARANTINE, target, owner);
		    job.SetCannotBePushedBack(true);
		    owner.jobQueue.AddJobInQueue(job);
	    }
    }
    #endregion

    #region Patrol
    public bool TriggerPersonalPatrol(out JobQueueItem p_producedJob) {
	    if (!owner.jobQueue.HasJob(JOB_TYPE.PATROL)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PATROL, INTERACTION_TYPE.START_PATROL, owner, owner);
		    job.SetCannotBePushedBack(true);
		    p_producedJob = job;
		    return true;
	    }
	    p_producedJob = null;
	    return false;
    }
    #endregion

    #region Burrowing
    public bool TriggerIdleBurrow(LocationGridTile p_targetTile, out JobQueueItem p_producedJob) {
	    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.BURROW], owner, owner, new OtherData[] { new LocationGridTileOtherData(p_targetTile) }, 0);
	    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.BURROW, owner, owner);
	    goapPlan.SetDoNotRecalculate(true);
	    job.SetCannotBePushedBack(true);
	    job.SetAssignedPlan(goapPlan);
	    p_producedJob = job;
	    return true;
    }
    #endregion

    #region Triton
    public bool TriggerTritonKidnap(Character targetCharacter, LocationStructure dropLocationStructure, LocationGridTile dropTileLocation) {
        if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.TRITON_KIDNAP)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRITON_KIDNAP, INTERACTION_TYPE.DROP_RESTRAINED,
                targetCharacter, owner);
            job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { dropLocationStructure, dropTileLocation });
            job.SetDoNotRecalculate(true);
            return owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterJobTriggerComponent data) {
        //Currently N/A
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterJobTriggerComponent : SaveData<CharacterJobTriggerComponent> {
    public JOB_TYPE primaryJob;
    public List<JOB_TYPE> priorityJobs;
    public Dictionary<INTERACTION_TYPE, int> numOfTimesActionDone;
    public List<JOB_TYPE> primaryJobCandidates;
    public List<JOB_TYPE> additionalPriorityJobs;

    public List<string> obtainPersonalItemRandomList;
    public List<string> obtainPersonalItemUnownedRandomList;
    public bool hasStartedScreamCheck;
    public bool doNotDoRecoverHPJob;
    public bool canReportDemonicStructure;

    #region Overrides
    public override void Save(CharacterJobTriggerComponent data) {
        primaryJob = data.primaryJob;
        priorityJobs = data.priorityJobs;
        numOfTimesActionDone = data.numOfTimesActionDone;
        primaryJobCandidates = data.primaryJobCandidates;
        obtainPersonalItemUnownedRandomList = data.obtainPersonalItemUnownedRandomList;
        hasStartedScreamCheck = data.hasStartedScreamCheck;
        doNotDoRecoverHPJob = data.doNotDoRecoverHPJob;
        canReportDemonicStructure = data.canReportDemonicStructure;
        additionalPriorityJobs = data.additionalPriorityJobs;
    }

    public override CharacterJobTriggerComponent Load() {
        CharacterJobTriggerComponent component = new CharacterJobTriggerComponent(this);
        return component;
    }
    #endregion
}