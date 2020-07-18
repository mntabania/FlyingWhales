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
using Random = UnityEngine.Random;

public class CharacterJobTriggerComponent : JobTriggerComponent {
	private Character _owner;

    public JobQueueItem finalJobAssignment { get; private set; }
    public JOB_TYPE primaryJob { get; private set; }
    public List<JOB_TYPE> priorityJobs { get; private set; }
    public Dictionary<GoapAction, int> numOfTimesActionDone { get; private set; }
    public List<JOB_TYPE> primaryJobCandidates;

    private List<string> obtainPersonalItemRandomList;
    private List<string> obtainPersonalItemUnownedRandomList;
    private bool hasStartedScreamCheck;

    public CharacterJobTriggerComponent(Character owner) {
		_owner = owner;
        _canReportDemonicStructure = true;
        numOfTimesActionDone = new Dictionary<GoapAction, int>();
        primaryJobCandidates = new List<JOB_TYPE>();
        priorityJobs = new List<JOB_TYPE>();
        SetPrimaryJob(JOB_TYPE.NONE);
	}

    #region Listeners
    public void SubscribeToListeners() {
		Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		Messenger.AddListener<Character>(Signals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
		Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
		Messenger.AddListener<Character>(Signals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
		Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.AddListener<NPCSettlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        Messenger.AddListener<IPointOfInterest>(Signals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.AddListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, OnUnseizePOI);
        //Messenger.AddListener<Character>(Signals.ON_SEIZE_CHARACTER, OnSeizedCharacter);
        //Messenger.AddListener<Character>(Signals.ON_UNSEIZE_CHARACTER, OnUnseizeCharacter);
    }
    public void UnsubscribeListeners() {
		Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
		Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
		Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.RemoveListener<NPCSettlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        Messenger.RemoveListener<IPointOfInterest>(Signals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.RemoveListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, OnUnseizePOI);
        TryStopScreamCheck();
	}
	private void OnCharacterCanPerformAgain(Character character) {
		if (character == _owner) {
			// if (_owner.currentSettlement is NPCSettlement npcSettlement && npcSettlement.isUnderSiege) {
			// 	TriggerFleeHome();	
			// }
			// character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.RESTRAIN); //cancel all restrain jobs.
			_owner.needsComponent.CheckExtremeNeeds();

            //Add all in vision poi to process again
            for (int i = 0; i < _owner.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest inVision = _owner.marker.inVisionPOIs[i];
                _owner.marker.AddUnprocessedPOI(inVision);
            }
            //for (int i = 0; i < _owner.marker.inVisionCharacters.Count; i++) {
            //    Character inVisionCharacter = _owner.marker.inVisionCharacters[i];
            //    _owner.marker.AddUnprocessedPOI(inVisionCharacter);
            //}
        }
	}
	private void OnCharacterCanNoLongerPerform(Character character) {
		if (character == _owner && character.isDead == false) {
			//TODO: THIS IS ONLY TEMPORARY! REDO THIS!
			if (character.interruptComponent.isInterrupted &&
			           character.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Narcoleptic_Attack) {
				//Don't do anything
			} else if (character.currentActionNode != null && character.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && InteractionManager.Instance.IsActionTirednessRecovery(character.currentActionNode.action)) {
				character.CancelAllJobsExceptForCurrent();
			} else {
				character.jobQueue.CancelAllJobs();
			}
            if (character.marker) {
                character.marker.StopMovement();
                character.marker.pathfindingAI.ClearAllCurrentPathData();
            }

            character.UncarryPOI();
            if (character.traitContainer.HasTrait("Unconscious")) {
                character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
            }

            //_owner.behaviourComponent.SetIsHarassing(false, null);
            //_owner.behaviourComponent.SetIsInvading(false, null);
            //_owner.behaviourComponent.SetIsDefending(false, null);
            // TryTriggerRestrain();
        }
	}
	private void OnCharacterCanNoLongerMove(Character character) {
		if (character == _owner) {
			TryStartScreamCheck();
		}
	}
	private void OnCharacterCanMoveAgain(Character character) {
		if (character == _owner) {
			TryStopScreamCheck();
		}
	}
	private void OnCharacterFinishedJob(Character character, GoapPlanJob job) {
        // if (character == _owner && job.jobType == JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM) {
        // 	TriggerBuryPsychopathVictim(job);
        // }
    }
    private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
		if (traitable == _owner) {
			if (TraitManager.Instance.removeStatusTraits.Contains(trait.name)) {
				TryCreateRemoveStatusJob(trait);
			}
			if (trait is Burning || trait is Poisoned) {
				TriggerRemoveStatusSelf(trait);
			}
			TryStartScreamCheck();
		}
    }
	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner) {
			TryStopScreamCheck();
			if (TraitManager.Instance.removeStatusTraits.Contains(nameof(trait))) {
				_owner.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_STATUS, trait.name, removedBy); //so that the character that cured him will not cancel his job.
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
		if (character == _owner) {
			TryCreateRemoveStatusJob();
		}
	}
	private void OnCharacterExitedHexTile(Character character, HexTile tile) {
		if (character == _owner) {
            Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RESTRAIN, _owner as IPointOfInterest);
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
		if (character == _owner) {
			TryStopScreamCheck();
		}
	}
	private void OnUnseizeCharacter(Character character) {
		if (character == _owner) {
			TryStartScreamCheck();
		}
	}
    #endregion

    #region Utilities
    public void SetPrimaryJob(JOB_TYPE jobType) {
        primaryJob = jobType;
    }
    public void SetFinalJobAssignment(JobQueueItem job) {
        finalJobAssignment = job;
    }
    public string GetPriorityJobs() {
        string jobs = string.Empty;
        if (_owner.characterClass.priorityJobs != null && _owner.characterClass.priorityJobs.Length > 0) {
            for (int i = 0; i < _owner.characterClass.priorityJobs.Length; i++) {
                if (i > 0) {
                    jobs += ",";
                }
                jobs += _owner.characterClass.priorityJobs[i].ToString();
            }
        }
        if(_owner.jobComponent.priorityJobs.Count > 0) {
            if(jobs != string.Empty) {
                jobs += ",";
            }
            for (int i = 0; i < _owner.jobComponent.priorityJobs.Count; i++) {
                if (i > 0) {
                    jobs += ",";
                }
                jobs += _owner.jobComponent.priorityJobs[i].ToString();
            }
        }
        return jobs;
    }
    public string GetSecondaryJobs() {
        string jobs = string.Empty;
        if (_owner.characterClass.secondaryJobs != null && _owner.characterClass.secondaryJobs.Length > 0) {
            for (int i = 0; i < _owner.characterClass.secondaryJobs.Length; i++) {
                if (i > 0) {
                    jobs += ",";
                }
                jobs += _owner.characterClass.secondaryJobs[i].ToString();
            }
        }
        return jobs;
    }
    public string GetAbleJobs() {
        string jobs = string.Empty;
        if (_owner.characterClass.ableJobs != null && _owner.characterClass.ableJobs.Length > 0) {
            for (int i = 0; i < _owner.characterClass.ableJobs.Length; i++) {
                if (i > 0) {
                    jobs += ",";
                }
                jobs += _owner.characterClass.ableJobs[i].ToString();
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
    #endregion

    #region General Jobs
    public bool PlanIdleStroll(LocationStructure targetStructure, LocationGridTile targetTile = null) {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL, _owner);
        _owner.jobQueue.AddJobInQueue(job);
        //if (currentStructure == targetStructure) {
        //    stateComponent.SwitchToState(CHARACTER_STATE.STROLL);
        //} else {
        //    MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL));
        //}
        return true;
    }
    public bool PlanIdleStrollOutside() {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL_OUTSIDE, _owner);
        _owner.jobQueue.AddJobInQueue(job);
        //if (currentStructure == targetStructure) {
        //    stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE);
        //} else {
        //    MoveToAnotherStructure(targetStructure, targetTile, null, () => stateComponent.SwitchToState(CHARACTER_STATE.STROLL_OUTSIDE));
        //}
        return true;
    }
    public bool PlanIdleStrollOutside(out JobQueueItem producedJob) {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.STROLL, CHARACTER_STATE.STROLL_OUTSIDE, _owner);
        producedJob = job;
        return true;
    }
    public bool PlanIdleBerserkStrollOutside(out JobQueueItem producedJob) {
        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.BERSERK_STROLL, CHARACTER_STATE.STROLL_OUTSIDE, _owner);
        producedJob = job;
        return true;
    }
    public bool PlanIdleReturnHome() { //bool forceDoAction = false
        if (_owner.homeStructure != null && _owner.homeStructure.tiles.Count > 0 && !_owner.homeStructure.hasBeenDestroyed) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], _owner, _owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE_RETURN_HOME, INTERACTION_TYPE.RETURN_HOME, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        } else if (_owner.HasTerritory()) {
            return TriggerReturnTerritory();
        }
        return false;
    }
    public bool PlanIdleReturnHome(out JobQueueItem producedJob) { //bool forceDoAction = false
        if (_owner.homeStructure != null && _owner.homeStructure.tiles.Count > 0 && !_owner.homeStructure.hasBeenDestroyed) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], _owner, _owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE_RETURN_HOME, INTERACTION_TYPE.RETURN_HOME, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        } else if (_owner.HasTerritory()) {
            return TriggerReturnTerritory(out producedJob);
        }
        producedJob = null;
        return false;
    }
    public bool PlanReturnHomeUrgent() { //bool forceDoAction = false
        if (_owner.homeStructure != null && _owner.homeStructure.tiles.Count > 0 && !_owner.homeStructure.hasBeenDestroyed) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], _owner, _owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_HOME_URGENT, INTERACTION_TYPE.RETURN_HOME, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        } else if (_owner.HasTerritory()) {
            return TriggerReturnTerritoryUrgent();
        }
        return false;
    }
    #endregion

    #region Job Triggers
    private void TriggerScreamJob() {
		if (_owner.jobQueue.HasJob(JOB_TYPE.SCREAM) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SCREAM, INTERACTION_TYPE.SCREAM_FOR_HELP, _owner, _owner);
			_owner.jobQueue.AddJobInQueue(job);
		}
	}
	public void TriggerBuryPsychopathVictim(Character target) {
		JobQueueItem buryJob = target.homeSettlement.GetJob(JOB_TYPE.BURY, target);
		buryJob?.ForceCancelJob(false);
		
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY_SERIAL_KILLER_VICTIM,
			INTERACTION_TYPE.BURY_CHARACTER, target, _owner);
		LocationStructure wilderness = _owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
		List<LocationGridTile> choices = wilderness.unoccupiedTiles
			.Where(x => x.IsPartOfSettlement(_owner.homeSettlement) == false).ToList();
		LocationGridTile targetTile = CollectionUtilities.GetRandomElement(choices);
		job.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[] {
			wilderness, targetTile
		});
		_owner.jobQueue.AddJobInQueue(job);
	}
	public bool TriggerFleeHome(JOB_TYPE jobType = JOB_TYPE.FLEE_TO_HOME) {
        if(_owner.homeStructure != null && !_owner.homeStructure.hasBeenDestroyed && _owner.homeStructure.tiles.Count > 0 && !_owner.isAtHomeStructure) {
            if (!_owner.jobQueue.HasJob(jobType)) {
                ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], _owner, _owner, null, 0);
                GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.RETURN_HOME, _owner, _owner);
                goapPlan.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                job.SetAssignedPlan(goapPlan);
                _owner.jobQueue.AddJobInQueue(job);
            }
            return true;
        }
        return false;
	}
	public bool TriggerDestroy(IPointOfInterest target) {
		if (!_owner.jobQueue.HasJob(JOB_TYPE.DESTROY, target)) {
			GoapPlanJob destroyJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.ASSAULT, target, _owner);
			destroyJob.SetStillApplicableChecker(() => IsDestroyJobApplicable(target));
			_owner.jobQueue.AddJobInQueue(destroyJob);
			return true;
		}
		return false;
	}
	public bool TriggerDestroy(IPointOfInterest target, out JobQueueItem producedJob) {
		if (!_owner.jobQueue.HasJob(JOB_TYPE.DESTROY, target)) {
			GoapPlanJob destroyJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.ASSAULT, target, _owner);
			destroyJob.SetStillApplicableChecker(() => IsDestroyJobApplicable(target));
			producedJob = destroyJob;
			return true;
		}
		producedJob = null;
		return false;
	}
	private void TriggerSettlementRemoveStatusJob(Trait trait) {
		if (_owner.isDead) { return; }
		if (trait.gainedFromDoing == null || trait.gainedFromDoing.isStealth == false) { //only create remove status job if trait was not gained from a stealth action
			GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, target = GOAP_EFFECT_TARGET.TARGET };
			if (_owner.homeSettlement.HasJob(goapEffect, _owner) == false) {
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, _owner, _owner.homeSettlement);
				job.SetCanTakeThisJobChecker((Character character, JobQueueItem jqi) => CanTakeRemoveStatus(character, job, trait));
				job.SetStillApplicableChecker(() => IsSettlementRemoveStatusJobStillApplicable(_owner, job, trait));
				_owner.homeSettlement.AddToAvailableJobs(job);
			}	
		}
	}
	private void TriggerRemoveStatusSelf(Trait trait) {
		if (trait.gainedFromDoing == null || trait.gainedFromDoing.isStealth == false) { //only create remove status job if trait was not gained from a stealth action
			GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, target = GOAP_EFFECT_TARGET.TARGET };
			if (_owner.jobQueue.HasJob(goapEffect, _owner) == false) {
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, _owner, _owner);
				job.SetStillApplicableChecker(() => IsRemoveStatusSelfJobStillApplicable(_owner, job, trait));
				_owner.jobQueue.AddJobInQueue(job);
			}	
		}
	}
    public void TriggerRemoveStatusTarget(IPointOfInterest target, string traitName) {
        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = traitName, target = GOAP_EFFECT_TARGET.TARGET };
        if (_owner.jobQueue.HasJob(goapEffect, _owner) == false) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, target, _owner);
            job.SetStillApplicableChecker(() => IsRemoveStatusTargetJobStillApplicable(target, job, traitName));
            _owner.jobQueue.AddJobInQueue(job);
        }
    }
    private void TriggerFeed(Character target) {
		GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, target = GOAP_EFFECT_TARGET.TARGET };
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, target, _owner);
		job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
		_owner.jobQueue.AddJobInQueue(job);
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
	private bool IsDestroyJobApplicable(IPointOfInterest target) {
		return target.gridTileLocation != null;
	}
	private bool IsSettlementRemoveStatusJobStillApplicable(Character target, GoapPlanJob job, Trait trait) {
		if (target.gridTileLocation == null || target.isDead) {
			return false;
		}
		if (target.gridTileLocation.IsNextToOrPartOfSettlement(job.originalOwner as NPCSettlement) == false) {
			return false;
		}
		if (target.traitContainer.HasTrait("Criminal")) {
			return false;
		}
		if (!target.traitContainer.HasTrait(trait.name)) {
			return false; //target no longer has the given trait
		}
		return true;
	}
	private bool IsRemoveStatusSelfJobStillApplicable(Character target, GoapPlanJob job, Trait trait) {
		if (target.gridTileLocation == null || target.isDead) {
			return false;
		}
		if (!target.traitContainer.HasTrait(trait.name)) {
			return false; //target no longer has the given trait
		}
		return true;
	}
    private bool IsRemoveStatusSelfJobStillApplicable(IPointOfInterest target, GoapPlanJob job, string traitName) {
        if (target.gridTileLocation == null || target.isDead) {
            return false;
        }
        if (!target.traitContainer.HasTrait(traitName)) {
            return false; //target no longer has the given trait
        }
        return true;
    }
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

	#region Job Checkers
	private bool CanTakeRemoveStatus(Character character, JobQueueItem job, Trait trait) {
		if (job is GoapPlanJob) {
			GoapPlanJob goapPlanJob = job as GoapPlanJob;
			Character targetCharacter = goapPlanJob.targetPOI as Character;
			// if (character != targetCharacter) {
				bool isNotHostileAndNotDead = !character.IsHostileWith(targetCharacter) && !targetCharacter.isDead;
				bool isResponsibleForTrait = trait.IsResponsibleForTrait(character);

				//if special illness, check if character is healer
				if (TraitManager.Instance.specialIllnessTraits.Contains(trait.name)) {
					return isNotHostileAndNotDead &&
					       character.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter,
						       RelationshipManager.Rival, RelationshipManager.Enemy) == false 
					       && isResponsibleForTrait == false
                           && !character.traitContainer.HasTrait("Psychopath")
                           && character.traitContainer.HasTrait("Healing Expert");	
				}
				
				return isNotHostileAndNotDead &&
				       character.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter,
					       RelationshipManager.Rival, RelationshipManager.Enemy) == false 
				       && isResponsibleForTrait == false
                       && !character.traitContainer.HasTrait("Psychopath");
			// }
		}
		return false;
	}
	#endregion
	
	#region Scream
	private void TryStartScreamCheck() {
		if (hasStartedScreamCheck) {
			return;
		}
        if (!_owner.isNormalCharacter) {
            return;
        }
		if ((_owner.canMove == false && 
		     _owner.traitContainer.HasTrait("Exhausted", "Starving", "Sulking"))
            || (_owner.traitContainer.HasTrait("Restrained") && _owner.currentStructure.structureType != STRUCTURE_TYPE.PRISON)) {
			hasStartedScreamCheck = true;
			Messenger.AddListener(Signals.HOUR_STARTED, HourlyScreamCheck);
			Debug.Log($"<color=green>{GameManager.Instance.TodayLogString()}{_owner.name} has started scream check</color>");
		}
	}
	private void TryStopScreamCheck() {
		if (hasStartedScreamCheck == false) {
			return;
		}
		bool isNotNeedy = !_owner.traitContainer.HasTrait("Exhausted", "Starving", "Sulking");
		bool isNotRestrained = !_owner.traitContainer.HasTrait("Restrained");
		bool isRestrainedButInPrison = _owner.traitContainer.HasTrait("Restrained") &&
		                               _owner.currentStructure.structureType == STRUCTURE_TYPE.PRISON;
		
		//scream will stop check if
		// - character can already move or
		// - character is no longer exhausted, starving or sulking and
		// - character is no longer restrained or
		// - character is still restrained, but is at prison.
		if (((_owner.canMove || isNotNeedy) && (isNotRestrained || isRestrainedButInPrison)) 
		    || _owner.gridTileLocation == null || _owner.isDead) {
			hasStartedScreamCheck = false;
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyScreamCheck);
			Debug.Log($"<color=red>{GameManager.Instance.TodayLogString()}{_owner.name} has stopped scream check</color>");
		}
	}
	private void HourlyScreamCheck() {
		if (_owner.canPerform == true) {
			return;
		}
        if (_owner.needsComponent.isExhausted) {
            _owner.needsComponent.PlanExtremeTirednessRecoveryActionsForCannotPerform();
            return;
        }
		string summary = $"{_owner.name} is checking for scream.";
		int chance = 50;
		if (_owner.canMove == false && 
		    _owner.traitContainer.HasTrait("Starving", "Sulking")) { //"Exhausted", 
            chance = 75;
		}
		summary += $"Chance is {chance.ToString()}.";
		int roll = Random.Range(0, 100); 
		summary += $"Roll is {roll.ToString()}.";
		Debug.Log($"<color=blue>{summary}</color>");
		if (roll < chance) {
			TriggerScreamJob();
		}
	}
	#endregion

	// #region Flee to home
	// private void CheckIfStopInterruptFinished(INTERRUPT interrupt, Character character) {
	// 	if (character == _owner && interrupt == INTERRUPT.Stopped) {
	// 		Messenger.RemoveListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfStopInterruptFinished);
	// 		if (_owner.canPerform) {
	// 			TriggerFleeHome();	
	// 		}
	// 	}
	// }
	// #endregion

	#region Remove Status
	private void TryCreateRemoveStatusJob(Trait trait) {
		if (_owner.homeSettlement != null && _owner.gridTileLocation != null && _owner.gridTileLocation.IsNextToOrPartOfSettlement(_owner.homeSettlement)
		    && _owner.traitContainer.HasTrait("Criminal") == false) {
			TriggerSettlementRemoveStatusJob(trait);
		}
	}
	private void TryCreateRemoveStatusJob() {
		if (_owner.homeSettlement != null && _owner.gridTileLocation.IsNextToOrPartOfSettlement(_owner.homeSettlement)
		    && _owner.traitContainer.HasTrait("Criminal") == false) {
			List<Trait> statusTraits = _owner.traitContainer.GetNormalTraits<Trait>(TraitManager.Instance.removeStatusTraits.ToArray());
			for (int i = 0; i < statusTraits.Count; i++) {
				Trait trait = statusTraits[i];
				TryCreateRemoveStatusJob(trait);
			}
		}
	}
	#endregion

	#region Feed
	public bool TryTriggerFeed(Character targetCharacter) {
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.FEED)) {
			GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, target = GOAP_EFFECT_TARGET.TARGET };
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, targetCharacter, _owner);
			job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
			return _owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}
	#endregion

	#region Move Character
	public bool TryTriggerMoveCharacter(Character targetCharacter, LocationStructure dropLocationStructure) {
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP,
				targetCharacter, _owner);
			job.AddOtherData(INTERACTION_TYPE.DROP, new object[] {dropLocationStructure});
			return _owner.jobQueue.AddJobInQueue(job);
		}
		return false;
	}
	public bool TryTriggerMoveCharacter(Character targetCharacter, LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
		if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, targetCharacter, _owner);
			job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure, dropGridTile });
			return _owner.jobQueue.AddJobInQueue(job);   
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

	#region Suicide
	public GoapPlanJob TriggerSuicideJob() {
		if (_owner.jobQueue.HasJob(JOB_TYPE.COMMIT_SUICIDE) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COMMIT_SUICIDE, 
				new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, 
					false, GOAP_EFFECT_TARGET.ACTOR),
				_owner,  _owner);
			_owner.jobQueue.AddJobInQueue(job);
			return job;	
		}
		return null;
	}
	#endregion

    #region Actions
    public void IncreaseNumOfTimesActionDone(GoapAction action) {
        if (!numOfTimesActionDone.ContainsKey(action)) {
            numOfTimesActionDone.Add(action, 1);
        } else {
            numOfTimesActionDone[action]++;
        }
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(3);
        SchedulingManager.Instance.AddEntry(dueDate, () => DecreaseNumOfTimesActionDone(action), _owner);
    }
    private void DecreaseNumOfTimesActionDone(GoapAction action) {
        numOfTimesActionDone[action]--;
    }
    public int GetNumOfTimesActionDone(GoapAction action) {
        if (numOfTimesActionDone.ContainsKey(action)) {
            return numOfTimesActionDone[action];
        }
        return 0;
    }
    #endregion
    
    #region Roam
    public bool TriggerRoamAroundTerritory() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TERRITORY)) {
            LocationGridTile chosenTile;
            if (_owner.homeStructure != null) {
                chosenTile = CollectionUtilities.GetRandomElement(_owner.homeStructure.unoccupiedTiles);
            } else {
                HexTile chosenTerritory = _owner.territorries[UnityEngine.Random.Range(0, _owner.territorries.Count)];
                chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TERRITORY, INTERACTION_TYPE.ROAM, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerRoamAroundTerritory(out JobQueueItem producedJob, bool checkIfPathPossible = false) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TERRITORY)) {
		    LocationGridTile chosenTile;
		    if (_owner.homeStructure != null) {
			    if (checkIfPathPossible) {
				    List<LocationGridTile> choices = _owner.homeStructure.unoccupiedTiles
					    .Where(t => _owner.movementComponent.HasPathToEvenIfDiffRegion(t)).ToList();
				    chosenTile = choices.Count > 0 ? CollectionUtilities.GetRandomElement(choices) : CollectionUtilities.GetRandomElement(_owner.homeStructure.unoccupiedTiles);
			    } else {
				    chosenTile = CollectionUtilities.GetRandomElement(_owner.homeStructure.unoccupiedTiles);    
			    }
		    } else {
			    HexTile chosenTerritory = _owner.territorries[UnityEngine.Random.Range(0, _owner.territorries.Count)];
			    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
		    }
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TERRITORY, INTERACTION_TYPE.ROAM, _owner, _owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerRoamAroundCorruption() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_CORRUPTION)) {
            HexTile chosenTerritory = PlayerManager.Instance.player.playerSettlement.tiles[UnityEngine.Random.Range(0, PlayerManager.Instance.player.playerSettlement.tiles.Count)];
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_CORRUPTION, INTERACTION_TYPE.ROAM, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerRoamAroundPortal() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_PORTAL)) {
            HexTile chosenTerritory = PlayerManager.Instance.player.portalTile;
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_PORTAL, INTERACTION_TYPE.ROAM, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerRoamAroundTile(LocationGridTile tile = null) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE)) {
            LocationGridTile chosenTile = tile;
            if (chosenTile == null) {
                if (_owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
                    HexTile chosenTerritory = _owner.gridTileLocation.collectionOwner.GetNearestHexTileWithinRegion();
                    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                } else {
                    HexTile chosenTerritory = _owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                }
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TILE, INTERACTION_TYPE.ROAM, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerRoamAroundTile(out JobQueueItem producedJob, LocationGridTile tile = null) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE)) {
		    LocationGridTile chosenTile = tile;
		    if (chosenTile == null) {
			    if (_owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
				    HexTile chosenTerritory = _owner.gridTileLocation.collectionOwner.GetNearestHexTileWithinRegion();
				    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
			    } else {
				    HexTile chosenTerritory = _owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
				    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
			    }
		    }
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TILE, INTERACTION_TYPE.ROAM, _owner, _owner);
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
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_STRUCTURE)) {
            LocationGridTile chosenTile = tile;
            if (chosenTile == null) {
                if (_owner.currentStructure != null) {
                    chosenTile = CollectionUtilities.GetRandomElement(_owner.currentStructure.passableTiles);
                }
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_STRUCTURE, INTERACTION_TYPE.ROAM, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerAttackDemonicStructure(LocationGridTile tile = null) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.COUNTERATTACK)) {
            LocationGridTile chosenTile = tile;
            if (chosenTile == null) {
                if (_owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
                    TriggerStand();
                    return false;
                } else {
                    HexTile chosenTerritory = _owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
                    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                }
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COUNTERATTACK, INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            // job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerAttackDemonicStructure(out JobQueueItem producedJob, LocationGridTile tile = null) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.COUNTERATTACK)) {
		    LocationGridTile chosenTile = tile;
		    if (chosenTile == null) {
			    if (_owner.gridTileLocation.collectionOwner.isPartOfParentRegionMap == false) {
				    TriggerStand(out producedJob);
				    return false;
			    } else {
				    HexTile chosenTerritory = _owner.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
				    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
			    }
		    }
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE], _owner, _owner, new object[] { chosenTile }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COUNTERATTACK, INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE, _owner, _owner);
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
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE)) {
	        LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(hex.locationGridTiles);
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TILE, INTERACTION_TYPE.ROAM, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerMoveToHex(out JobQueueItem producedJob, HexTile hex) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE)) {
		    LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(hex.locationGridTiles);
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TILE, INTERACTION_TYPE.ROAM, _owner, _owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerStand() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.STAND)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND], _owner, _owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND, INTERACTION_TYPE.STAND, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerStand(out JobQueueItem producedJob) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.STAND)) {
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND], _owner, _owner, null, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND, INTERACTION_TYPE.STAND, _owner, _owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerReturnTerritory() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.RETURN_TERRITORY)) {
            LocationGridTile chosenTile;
            if (_owner.homeStructure != null && !_owner.homeStructure.hasBeenDestroyed) {
                chosenTile = CollectionUtilities.GetRandomElement(_owner.homeStructure.unoccupiedTiles);
            } else {
                if (_owner.territorries.Count > 0) {
                    HexTile chosenTerritory = _owner.territorries[UnityEngine.Random.Range(0, _owner.territorries.Count)];
                    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                } else {
                    //If has no territory, roam around tile instead
                    return TriggerRoamAroundTile();
                }
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_TERRITORY, INTERACTION_TYPE.ROAM, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerReturnTerritoryUrgent() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.RETURN_HOME_URGENT)) {
            LocationGridTile chosenTile;
            if (_owner.homeStructure != null && !_owner.homeStructure.hasBeenDestroyed) {
                chosenTile = CollectionUtilities.GetRandomElement(_owner.homeStructure.unoccupiedTiles);
            } else {
                if (_owner.territorries.Count > 0) {
                    HexTile chosenTerritory = _owner.territorries[UnityEngine.Random.Range(0, _owner.territorries.Count)];
                    chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
                } else {
                    //If has no territory, roam around tile instead
                    return TriggerRoamAroundTile();
                }
            }
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.GO_TO_TILE], _owner, chosenTile.genericTileObject, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_HOME_URGENT, INTERACTION_TYPE.GO_TO_TILE, chosenTile.genericTileObject, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerReturnTerritory(out JobQueueItem producedJob) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.RETURN_TERRITORY)) {
		    LocationGridTile chosenTile;
		    if (_owner.homeStructure != null) {
			    chosenTile = CollectionUtilities.GetRandomElement(_owner.homeStructure.unoccupiedTiles);
		    } else {
			    if (_owner.territorries.Count > 0) {
				    HexTile chosenTerritory = _owner.territorries[UnityEngine.Random.Range(0, _owner.territorries.Count)];
				    List<LocationGridTile> validTiles = chosenTerritory.locationGridTiles
					    .Where(t => _owner.movementComponent.HasPathToEvenIfDiffRegion(t)).ToList();
				    chosenTile = CollectionUtilities.GetRandomElement(validTiles.Count > 0 ? validTiles : chosenTerritory.locationGridTiles);
			    } else {
				    //If has no territory, roam around tile instead
				    return TriggerRoamAroundTile(out producedJob);
			    }
		    }
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_TERRITORY, INTERACTION_TYPE.ROAM, _owner, _owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerReturnPortal() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.RETURN_PORTAL)) {
            HexTile chosenTerritory = PlayerManager.Instance.player.portalTile;
            LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(chosenTerritory.locationGridTiles);
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_PORTAL, INTERACTION_TYPE.ROAM, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerMonsterSleep() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SLEEP_OUTSIDE], _owner, _owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, INTERACTION_TYPE.SLEEP_OUTSIDE, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerMonsterSleep(out JobQueueItem producedJob) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL)) {
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.SLEEP_OUTSIDE], _owner, _owner, null, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ENERGY_RECOVERY_NORMAL, INTERACTION_TYPE.SLEEP_OUTSIDE, _owner, _owner);
		    goapPlan.SetDoNotRecalculate(true);
		    job.SetCannotBePushedBack(true);
		    job.SetAssignedPlan(goapPlan);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public void CreateLearnMonsterJob(Character target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.LEARN_MONSTER, INTERACTION_TYPE.STUDY_MONSTER, target, _owner);
        _owner.jobQueue.AddJobInQueue(job);
    }
    public void CreateTakeArtifactJob(TileObject target, LocationStructure dropLocation) {
        //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TAKE_ARTIFACT, INTERACTION_TYPE.DROP_ITEM, target, _owner);
        //job.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[] { dropLocation });
        //_owner.jobQueue.AddJobInQueue(job);
    }
    //public void CreatePickUpJob(TileObject target) {
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TAKE_ITEM, INTERACTION_TYPE.PICK_UP, target, _owner);
    //    _owner.jobQueue.AddJobInQueue(job);
    //}
    public void CreateOpenChestJob(TileObject target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OPEN_CHEST, INTERACTION_TYPE.OPEN, target, _owner);
        _owner.jobQueue.AddJobInQueue(job);
    }
    public void CreateDestroyResourceAmountJob(ResourcePile target, int amount) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT, target, _owner);
        if(amount > 0) {
            job.AddOtherData(INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT, new object[] { amount });
        }
        _owner.jobQueue.AddJobInQueue(job);
    }
    public void TriggerStopJobs() {
	    if (_owner.marker) {
		    _owner.marker.StopMovement();
	    }
	    _owner.CancelAllJobs();
    }
    #endregion

    #region Abduct
    public void CreateAbductJob(Character target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABDUCT, INTERACTION_TYPE.DROP, target, _owner);
        LocationStructure dropLocationStructure = PlayerManager.Instance.player.portalTile.region.GetRandomStructureOfType(STRUCTURE_TYPE.TORTURE_CHAMBERS);
        if (dropLocationStructure == null) {
	        dropLocationStructure = PlayerManager.Instance.player.portalTile.locationGridTiles[0].structure;
        }
        job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure }); //For now drop in portal, this will be changed to Demonic Prison
        _owner.jobQueue.AddJobInQueue(job);
    }
    #endregion
    
    #region Violence
    public GoapPlanJob CreateKnockoutJob(Character targetCharacter) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.KNOCKOUT, targetCharacter)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, _owner);
		    _owner.jobQueue.AddJobInQueue(job);
            return job;
	    }
        return null;
    }
    public GoapPlanJob CreateBrawlJob(Character targetCharacter) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.BRAWL, targetCharacter)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BRAWL, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, _owner);
            _owner.jobQueue.AddJobInQueue(job);
            return job;
        }
        return null;
    }
    public GoapPlanJob CreateDemonKillJob(Character targetCharacter) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.DEMON_KILL, targetCharacter)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEMON_KILL, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, _owner);
		    _owner.jobQueue.AddJobInQueue(job);
            return job;
	    }
        return null;
    }
    public GoapPlanJob CreateBerserkAttackJob(IPointOfInterest targetPOI) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.BERSERK_ATTACK, targetPOI)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BERSERK_ATTACK, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetPOI, _owner);
            _owner.jobQueue.AddJobInQueue(job);
            return job;
        }
        return null;
    }
    #endregion

    #region Needs
    public void CreateProduceFoodJob() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.PRODUCE_FOOD)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PRODUCE_FOOD, new GoapEffect(GOAP_EFFECT_CONDITION.PRODUCE_FOOD, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), _owner, _owner);
            _owner.jobQueue.AddJobInQueue(job);
        }
    }
    #endregion

    #region Items
    public void CreateTakeItemJob(TileObject targetItem) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TAKE_ITEM, INTERACTION_TYPE.PICK_UP, targetItem, _owner);
        _owner.jobQueue.AddJobInQueue(job);
    }
    public bool TryCreateObtainPersonalItemJob() {
        if (!_owner.IsInventoryAtFullCapacity()) {
            string chosenItemName = GetItemNameForObtainPersonalItemJob();
            if(chosenItemName != string.Empty) {
                GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, chosenItemName, false, GOAP_EFFECT_TARGET.ACTOR);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM, goapEffect, _owner, _owner);
                _owner.jobQueue.AddJobInQueue(job);
                return true;
            }
        }
        return false;

    }
    public bool TryCreateObtainPersonalItemJob(out JobQueueItem producedJob) {
	    if (!_owner.IsInventoryAtFullCapacity()) {
            string chosenItemName = GetItemNameForObtainPersonalItemJob();
            if (chosenItemName != string.Empty) {
                GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, chosenItemName, false, GOAP_EFFECT_TARGET.ACTOR);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM, goapEffect, _owner, _owner);
                producedJob = job;
                return true;
            }
	    }
	    producedJob = null;
	    return false;

    }
    public bool TryCreateObtainPersonalItemJob(string chosenItemName, out JobQueueItem producedJob) {
	    if (!_owner.IsInventoryAtFullCapacity()) {
		    GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, chosenItemName, false, GOAP_EFFECT_TARGET.ACTOR);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM, goapEffect, _owner, _owner);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;

    }
    public void CreateDropItemJob(TileObject target, LocationStructure dropLocation) {
        if(!_owner.jobQueue.HasJob(JOB_TYPE.DROP_ITEM, target)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DROP_ITEM, INTERACTION_TYPE.DROP_ITEM, target, _owner);
            job.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[] { dropLocation });
            _owner.jobQueue.AddJobInQueue(job);
        }
    }
    private string GetItemNameForObtainPersonalItemJob() {
        if (_owner.homeSettlement != null && _owner.interestedItemNames != null && _owner.interestedItemNames.Count > 0) {
            if (obtainPersonalItemRandomList == null) { obtainPersonalItemRandomList = new List<string>(); }
            if (obtainPersonalItemUnownedRandomList == null) { obtainPersonalItemUnownedRandomList = new List<string>(); }
            obtainPersonalItemRandomList.Clear();
            obtainPersonalItemUnownedRandomList.Clear();
            for (int i = 0; i < _owner.interestedItemNames.Count; i++) {
                string itemName = _owner.interestedItemNames[i];
                bool itemHasBeenAdded = false;
                for (int j = 0; j < _owner.homeSettlement.tiles.Count; j++) {
                    HexTile hexInSettlement = _owner.homeSettlement.tiles[j];
                    for (int k = 0; k < hexInSettlement.itemsInHex.Count; k++) {
                        TileObject itemInHex = hexInSettlement.itemsInHex[k];
                        if (itemInHex.name == itemName) {
                            if (itemInHex.gridTileLocation != null && itemInHex.IsOwnedBy(_owner) && itemInHex.gridTileLocation.structure == _owner.homeStructure) {
                                //Should not obtain personal item if item is already personally owned is in the home structure of the owner
                                continue;
                            }
                            itemHasBeenAdded = true;
                            obtainPersonalItemRandomList.Add(itemName);
                            if (!_owner.HasItem(itemName)) {
                                obtainPersonalItemUnownedRandomList.Add(itemName);
                            }
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
            } else if (obtainPersonalItemRandomList.Count > 0) {
                return obtainPersonalItemRandomList[UnityEngine.Random.Range(0, obtainPersonalItemRandomList.Count)];
            }
        }
        return string.Empty;
    }
    #endregion

    #region Hide At Home
    public bool CreateHideAtHomeJob() {
        if (_owner.homeStructure != null && !_owner.homeStructure.hasBeenDestroyed && _owner.homeStructure.tiles.Count > 0) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HIDE_AT_HOME, INTERACTION_TYPE.RETURN_HOME, _owner, _owner);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;

    }
    #endregion

    #region Idle
    public bool TriggerStandStill() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.STAND_STILL)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND_STILL], _owner, _owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND_STILL, INTERACTION_TYPE.STAND_STILL, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerStandStill(out JobQueueItem producedJob) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.STAND_STILL)) {
		    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.STAND_STILL], _owner, _owner, null, 0);
		    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STAND_STILL, INTERACTION_TYPE.STAND_STILL, _owner, _owner);
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
        if (_owner.jobQueue.HasJob(JOB_TYPE.UNDERMINE, targetCharacter)) {
            return false;
        }
        if (targetCharacter.isDead || _owner.traitContainer.HasTrait("Diplomatic")) {
            return false;
        }
        if (targetCharacter.homeRegion == null) {
            targetCharacter.logComponent.PrintLogIfActive(_owner.name + " cannot undermine " + targetCharacter.name + " because he/she does not have a home region");
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
            targetCharacter.logComponent.PrintLogIfActive(_owner.name + " cannot undermine " + targetCharacter.name + " because he/she does not have an owned item on the floor in his/her home region");
            return false;
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET), chosenObject, _owner);
        _owner.jobQueue.AddJobInQueue(job);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", $"{reason}_and_undermine");
        log.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        _owner.logComponent.AddHistory(log);
        return true;
    }
    private bool CreatePlaceTrapPOIJob(IPointOfInterest target, JOB_TYPE jobType = JOB_TYPE.PLACE_TRAP) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
        _owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    private bool CreatePlaceTrapPOIJob(IPointOfInterest target, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
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
    private bool _canReportDemonicStructure;
    public void CreateReportDemonicStructure(LocationStructure structureToReport) {
	    if (_canReportDemonicStructure && !_owner.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE)) {
		    // UIManager.Instance.ShowYesNoConfirmation("Demonic Structure Seen", 
			   //  $"Your demonic structure {structureToReport.name} has been seen by {_owner.name}!", 
			   //  onClickNoAction: _owner.CenterOnCharacter, yesBtnText: "OK", noBtnText: $"Jump to {_owner}", 
			   //  showCover:true, pauseAndResume: true);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE, INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE, _owner, _owner);
            job.AddOtherData(INTERACTION_TYPE.REPORT_CORRUPTED_STRUCTURE, new object[] { structureToReport });
            _owner.jobQueue.AddJobInQueue(job);
            Messenger.Broadcast(Signals.DEMONIC_STRUCTURE_DISCOVERED, structureToReport, _owner, job);
        }
    }
    /// <summary>
    /// Disable report demonic structure until this character steps foot in his/her home.
    /// </summary>
    public void DisableReportStructure() {
	    _canReportDemonicStructure = false;
	    Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, TryEnableReportStructure);
    }
    public void EnableReportStructure() {
	    _canReportDemonicStructure = true;
	    Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, TryEnableReportStructure);
    }
    private void TryEnableReportStructure(Character character, LocationStructure structure) {
	    if (character == _owner) {
		    if (character.homeStructure != null && structure == character.homeStructure) {
			    EnableReportStructure();    
		    }
	    }
    }
    #endregion

    #region Abduct
    public bool TriggerAbduct() {
	    if (_owner.homeStructure == null) { return false; }
	    List<Character> choices = _owner.currentRegion.charactersAtLocation.Where(x => x.isNormalCharacter).ToList();
	    if (choices.Count > 0) {
		    Character targetCharacter = CollectionUtilities.GetRandomElement(choices);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, target = GOAP_EFFECT_TARGET.TARGET }, targetCharacter, _owner);
		    job.AddOtherData(INTERACTION_TYPE.DROP, new object[]{_owner.homeStructure});
		    _owner.jobQueue.AddJobInQueue(job);
		    return true;
	    }
	    return false;
    }
    public bool TriggerAbduct(out JobQueueItem producedJob) {
	    if (_owner.homeStructure == null) {
		    producedJob = null;
		    return false;
	    }
	    List<Character> choices = _owner.currentRegion.charactersAtLocation.Where(x => x.isNormalCharacter).ToList();
	    if (choices.Count > 0) {
		    Character targetCharacter = CollectionUtilities.GetRandomElement(choices);
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, target = GOAP_EFFECT_TARGET.TARGET }, targetCharacter, _owner);
		    job.AddOtherData(INTERACTION_TYPE.DROP, new object[]{_owner.homeStructure});
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Heal Self
    public void OnHPReduced() {
	    if (_owner.jobQueue.HasJob(JOB_TYPE.RECOVER_HP) == false && _owner.isNormalCharacter
	        && _owner.currentHP > 0 && _owner.currentHP < Mathf.FloorToInt(_owner.maxHP * 0.5f)) {
		    CreateHealSelfJob();	
	    }
    }
    private void CreateHealSelfJob() {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RECOVER_HP, INTERACTION_TYPE.HEAL_SELF, _owner, _owner);
	    job.SetStillApplicableChecker(IsHealSelfJobStillApplicable);
	    _owner.jobQueue.AddJobInQueue(job);
    }
    private bool IsHealSelfJobStillApplicable() {
	    return _owner.currentHP < _owner.maxHP;
    }
    #endregion
    
    #region Undermine
    public bool CreatePoisonFoodJob(Character targetCharacter, JOB_TYPE jobType = JOB_TYPE.POISON_FOOD) {
	    if (_owner.jobQueue.HasJob(jobType, targetCharacter)) {
		    return false;
	    }
	    if (targetCharacter.isDead) {
		    return false;
	    }
	    if (targetCharacter.homeRegion == null) {
		    targetCharacter.logComponent.PrintLogIfActive(_owner.name + " cannot poison food " + targetCharacter.name + " because he/she does not have a home region");
		    return false;
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
		    targetCharacter.logComponent.PrintLogIfActive(_owner.name + " cannot poison food " + targetCharacter.name + " because he/she does not have an owned item on the floor in his/her home region");
		    return false;
	    }
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Poisoned", false, GOAP_EFFECT_TARGET.TARGET), chosenObject, _owner);
	    _owner.jobQueue.AddJobInQueue(job);

	    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "poison_undermine");
	    log.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
	    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
	    _owner.logComponent.AddHistory(log);
	    return true;
    }
    public bool CreatePoisonFoodJob(Character targetCharacter, out JobQueueItem producedJob) {
	    if (_owner.jobQueue.HasJob(JOB_TYPE.POISON_FOOD, targetCharacter)) {
		    producedJob = null;
		    return false;
	    }
	    if (targetCharacter.isDead) {
		    producedJob = null;
		    return false;
	    }
	    if (targetCharacter.homeRegion == null) {
		    targetCharacter.logComponent.PrintLogIfActive(_owner.name + " cannot poison food " + targetCharacter.name + " because he/she does not have a home region");
		    producedJob = null;
		    return false;
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
		    targetCharacter.logComponent.PrintLogIfActive(_owner.name + " cannot poison food " + targetCharacter.name + " because he/she does not have an owned item on the floor in his/her home region");
		    producedJob = null;
		    return false;
	    }
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.POISON_FOOD, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Poisoned", false, GOAP_EFFECT_TARGET.TARGET), chosenObject, _owner);
	    producedJob = job;

	    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "poison_undermine");
	    log.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
	    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
	    _owner.logComponent.AddHistory(log);
	    return true;
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
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SHARE_NEGATIVE_INFO, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, _owner);
        job.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[] { negativeInfo });
        _owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    public bool CreateSpreadRumorJob(Character targetCharacter, Rumor rumor) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (rumor == null) {
            return false;
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPREAD_RUMOR, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, _owner);
        job.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[] { rumor });
        _owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    public bool CreateConfirmRumorJob(Character targetCharacter, ActualGoapNode action) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (action == null) {
            return false;
        }
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CONFIRM_RUMOR, INTERACTION_TYPE.SHARE_INFORMATION, targetCharacter, _owner);
        job.AddOtherData(INTERACTION_TYPE.SHARE_INFORMATION, new object[] { action });
        _owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    #endregion

    #region Visit Different Region
    public bool TriggerVisitDifferentRegion() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.VISIT_DIFFERENT_REGION)) {
            Region chosenRegion = null;
            List<Region> adjacentRegions = _owner.currentRegion.AdjacentRegions();
            if(adjacentRegions != null && adjacentRegions.Count > 0) {
                chosenRegion = adjacentRegions[UnityEngine.Random.Range(0, adjacentRegions.Count)];
            }
            if(chosenRegion != null) {
                HexTile hex = chosenRegion.GetRandomPlainHex();
                if(hex != null) {
                    LocationGridTile chosenTile = CollectionUtilities.GetRandomElement(hex.locationGridTiles);
                    if (_owner.gridTileLocation != null && _owner.movementComponent.HasPathToEvenIfDiffRegion(chosenTile)) {
                        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
                        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.VISIT_DIFFERENT_REGION, INTERACTION_TYPE.ROAM, _owner, _owner);
                        goapPlan.SetDoNotRecalculate(true);
                        job.SetCannotBePushedBack(true);
                        job.SetAssignedPlan(goapPlan);
                        _owner.jobQueue.AddJobInQueue(job);
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
	    if (_owner.minion == null && !(_owner is Animal) && _owner.gridTileLocation != null && _owner.gridTileLocation.IsNextToOrPartOfSettlement(out var settlement)
	        && settlement is NPCSettlement npcSettlement) {
		    LocationStructure targetStructure = npcSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) ??
		                                        npcSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
		    GoapPlanJob buryJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, _owner, npcSettlement);
		    buryJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanTakeBuryJob);
		    buryJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[]{ targetStructure });
		    buryJob.SetStillApplicableChecker(() => IsBuryJobStillApplicable(_owner, npcSettlement));
		    npcSettlement.AddToAvailableJobs(buryJob);
	    }
    }
    private bool IsBuryJobStillApplicable(Character target, NPCSettlement npcSettlement) {
	    return target.gridTileLocation != null && target.gridTileLocation.IsNextToOrPartOfSettlement(npcSettlement) && target.marker != null;
    }
    private bool IsBuryJobStillApplicable(Character target) {
        return target.gridTileLocation != null && target.marker != null;
    }
    public void TriggerPersonalBuryJob(Character targetCharacter) {
        if (_owner.gridTileLocation != null) {
            LocationStructure targetStructure = _owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY) ??
                                                _owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            GoapPlanJob buryJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, targetCharacter, _owner);
            buryJob.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[] { targetStructure });
            buryJob.SetStillApplicableChecker(() => IsBuryJobStillApplicable(_owner));
            _owner.jobQueue.AddJobInQueue(buryJob);
        }
    }
    #endregion

    #region Go To
    public bool CreateGoToJob(IPointOfInterest target) {
        if(!_owner.jobQueue.HasJob(JOB_TYPE.GO_TO, target)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO, target, _owner);
            job.SetCannotBePushedBack(true);
            return _owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public bool CreateGoToJob(IPointOfInterest target, out JobQueueItem producedJob) {
	    if(!_owner.jobQueue.HasJob(JOB_TYPE.GO_TO, target)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO, target, _owner);
            job.SetCannotBePushedBack(true);
            producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool CreateGoToJob(LocationGridTile tile, out JobQueueItem producedJob) {
	    if(!_owner.jobQueue.HasJob(JOB_TYPE.GO_TO)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.GO_TO, INTERACTION_TYPE.GO_TO_TILE, tile.genericTileObject, _owner);
            job.SetCannotBePushedBack(true);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Build
    public void TriggerSpawnLair(LocationGridTile targetTile) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_LAIR, _owner, _owner);
            job.AddOtherData(INTERACTION_TYPE.BUILD_LAIR, new object[] { targetTile });
            _owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void TriggerSpawnLair(LocationGridTile targetTile, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.SPAWN_LAIR)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_LAIR, INTERACTION_TYPE.BUILD_LAIR, _owner, _owner);
		    job.AddOtherData(INTERACTION_TYPE.BUILD_LAIR, new object[] { targetTile });
		    producedJob = job;
	    }
    }
    #endregion

    #region Necromancer
    public void TriggerAbsorbLife() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ABSORB_LIFE)) {
            GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_LIFE, effect, _owner, _owner);
            _owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void TriggerAbsorbLife(out JobQueueItem producedJob) {
        producedJob = null;
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ABSORB_LIFE)) {
            GoapEffect effect = new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_LIFE, effect, _owner, _owner);
            producedJob = job;
        }
    }
    public void TriggerAbsorbLife(IPointOfInterest target) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ABSORB_LIFE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_LIFE, INTERACTION_TYPE.ABSORB_LIFE, target, _owner);
            _owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void TriggerAbsorbLife(IPointOfInterest target, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.ABSORB_LIFE)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_LIFE, INTERACTION_TYPE.ABSORB_LIFE, target, _owner);
		    producedJob = job;
	    }
    }
    public bool TriggerSpawnSkeleton() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.SPAWN_SKELETON)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_SKELETON, INTERACTION_TYPE.SPAWN_SKELETON, _owner, _owner);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerSpawnSkeleton(out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.SPAWN_SKELETON)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SPAWN_SKELETON, INTERACTION_TYPE.SPAWN_SKELETON, _owner, _owner);
		    producedJob = job;
		    return true;
	    }
	    return false;
    }
    public void TriggerRaiseCorpse(IPointOfInterest target) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.RAISE_CORPSE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RAISE_CORPSE, INTERACTION_TYPE.RAISE_CORPSE, target, _owner);
            _owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void TriggerRaiseCorpse(IPointOfInterest target, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.RAISE_CORPSE)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RAISE_CORPSE, INTERACTION_TYPE.RAISE_CORPSE, target, _owner);
		    producedJob = job;
	    }
    }
    public void TriggerAbsorbPower(IPointOfInterest target) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ABSORB_POWER)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_POWER, INTERACTION_TYPE.ABSORB_POWER, target, _owner);
            _owner.jobQueue.AddJobInQueue(job);
        }
    }
    public void TriggerAbsorbPower(IPointOfInterest target, out JobQueueItem producedJob) {
	    producedJob = null;
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.ABSORB_POWER)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABSORB_POWER, INTERACTION_TYPE.ABSORB_POWER, target, _owner);
		    producedJob = job;
	    }
    }
    public bool TriggerReadNecronomicon(out JobQueueItem producedJob) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.READ_NECRONOMICON, _owner, _owner);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerMeditate() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.MEDITATE, _owner, _owner);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerMeditate(out JobQueueItem producedJob) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.MEDITATE, _owner, _owner);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    public bool TriggerRegainEnergy() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.REGAIN_ENERGY, _owner, _owner);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerRegainEnergy(out JobQueueItem producedJob) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.REGAIN_ENERGY, _owner, _owner);
		    producedJob = job;
		    return true;
	    }
	    producedJob = null;
	    return false;
    }
    #endregion

    #region Apprehend
    public bool TryCreateApprehend(Character target, ref bool canDoJob) {
        NPCSettlement settlementToGoTo = target.currentSettlement as NPCSettlement;
        if(settlementToGoTo == null || (settlementToGoTo.locationType != LOCATION_TYPE.SETTLEMENT)) {
            settlementToGoTo = _owner.homeSettlement;
            if (settlementToGoTo == null || (settlementToGoTo.locationType != LOCATION_TYPE.SETTLEMENT)) {
                settlementToGoTo = null;
            }
        }
        canDoJob = InteractionManager.Instance.CanCharacterTakeApprehendJob(_owner, target) && settlementToGoTo != null;
        if (target.traitContainer.HasTrait("Criminal") && canDoJob) {
            if (_owner.jobQueue.HasJob(JOB_TYPE.APPREHEND, target) == false) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, target, _owner);
                job.SetStillApplicableChecker(() => IsApprehendStillApplicable(target, settlementToGoTo));
                job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { settlementToGoTo.prison });
                return _owner.jobQueue.AddJobInQueue(job);
            }
        }
        return false;
    }
    public bool TryCreateApprehend(Character target) {
        bool canDoJob = false;
        return TryCreateApprehend(target, ref canDoJob);
    }
    private bool IsApprehendStillApplicable(Character target, NPCSettlement settlement) {
        bool isApplicable = !target.traitContainer.HasTrait("Restrained") || target.currentStructure != settlement.prison;
        return target.gridTileLocation != null && target.gridTileLocation.IsNextToOrPartOfSettlement(settlement) && isApplicable;
    }
    #endregion

    #region Sabotage
    public bool TryCreateSabotageNeighbourJob(Character target, out JobQueueItem producedJob) {
	    //create predetermined plan and job
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SABOTAGE_NEIGHBOUR, INTERACTION_TYPE.REMOVE_BUFF, target, _owner);
	    List<JobNode> jobNodes = new List<JobNode>();
	    if (_owner.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT) == false) {
		    //Pick up cultist kit at home
		    TileObject cultistKitAtHome = _owner.homeStructure?.GetTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT);
		    Assert.IsNotNull(cultistKitAtHome, $"{_owner.name} wants to sabotage neighbour but has no cultist kit at home or in inventory. This should never happen, because the Cultist Behaviour checks this beforehand");
		    ActualGoapNode pickupNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PICK_UP], _owner, cultistKitAtHome, null, 0);
		    jobNodes.Add(new SingleJobNode(pickupNode));
	    }
	    
	    ActualGoapNode removeBuffNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.REMOVE_BUFF], _owner, target, null, 0);
	    jobNodes.Add(new SingleJobNode(removeBuffNode));
	    
	    GoapPlan goapPlan = new GoapPlan(jobNodes, target);
	    goapPlan.SetDoNotRecalculate(true);
	    job.SetCannotBePushedBack(true);
	    job.SetAssignedPlan(goapPlan);
	    
	    producedJob = job;
	    return true;

    }
    private bool IsValidSabotageNeighbourTarget(Character character) {
	    AWARENESS_STATE awarenessState = _owner.relationshipContainer.GetAwarenessState(character);
	    return character.isNormalCharacter && character.traitContainer.HasTrait("Resting", "Unconscious") &&
	           character.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF) &&
	           character.traitContainer.HasTrait("Cultist") == false && _owner.HasSameHomeAs(character) &&
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
			    string opinionLabel = _owner.relationshipContainer.GetOpinionLabel(character);
			    if (opinionLabel == BaseRelationshipContainer.Close_Friend) {
				    weight += Random.Range(10, 51);
			    } else if (opinionLabel == BaseRelationshipContainer.Friend) {
				    weight += Random.Range(100, 151);
			    } else if (opinionLabel == BaseRelationshipContainer.Acquaintance) {
				    weight += Random.Range(150, 251);
			    } else if (opinionLabel == BaseRelationshipContainer.Enemy || opinionLabel == BaseRelationshipContainer.Rival) {
				    weight += Random.Range(200, 351);
			    }
			    targetWeights.AddElement(character, weight);
		    }
		    targetWeights.LogDictionaryValues($"{GameManager.Instance.TodayLogString()}{_owner.name}'s Sabotage Neighbour Weights:");
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
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, INTERACTION_TYPE.PRAY, _owner, _owner);
	    ActualGoapNode prayNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PRAY], _owner, _owner, null, 0);
	    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(prayNode) }, _owner);
	    goapPlan.SetDoNotRecalculate(true);
	    job.SetCannotBePushedBack(true);
	    _owner.jobQueue.AddJobInQueue(job);
    }
    #endregion

    #region Spawn Objects
    public bool TriggerSpawnPoisonCloud(out JobQueueItem producedJob) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.SPAWN_POISON_CLOUD, _owner, _owner);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    #endregion
    
    #region Decrease Mood
    public bool TriggerDecreaseMood(Character target, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DECREASE_MOOD,
		    INTERACTION_TYPE.DECREASE_MOOD, target, _owner);
	    producedJob = job;
	    return true;
    }
    public bool TriggerDecreaseMoodInTerritory(Character target, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DECREASE_MOOD,
		    INTERACTION_TYPE.DECREASE_MOOD, target, _owner);
	    job.SetStillApplicableChecker(() => IsDecreaseMoodJobInTerritoryStillApplicable(target));
	    producedJob = job;
	    return true;
    }
    private bool IsDecreaseMoodJobInTerritoryStillApplicable(Character target) {
	    return target.hexTileLocation != null && _owner.territorries.Contains(target.hexTileLocation);
    }
    #endregion

    #region Disable
    public bool TriggerDisable(Character target, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DISABLE,
		    INTERACTION_TYPE.DISABLE, target, _owner);
	    producedJob = job;
	    return true;
    }
    #endregion

    #region Monsters
    public bool TriggerLayEgg(out JobQueueItem producedJob) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.IDLE)) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.IDLE, INTERACTION_TYPE.LAY_EGG, _owner, _owner);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerMonsterAbduct(Character targetCharacter, out JobQueueItem producedJob, LocationGridTile targetTile = null) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT,
		    INTERACTION_TYPE.DROP, targetCharacter, _owner);
	    job.SetCannotBePushedBack(true);
	    job.AddOtherData(INTERACTION_TYPE.DROP,
		    targetTile != null ? new object[] {targetTile.structure, targetTile} : new object[] {_owner.homeStructure});
	    producedJob = job;
	    return true;
    }
    public bool TriggerEatAlive(Character webbedCharacter, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_EAT,
		    INTERACTION_TYPE.EAT_ALIVE, webbedCharacter, _owner);
	    producedJob = job;
	    return true;
    }
    #endregion

    #region Arson
    public bool TriggerArson(TileObject target, out JobQueueItem producedJob) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ARSON,
		    INTERACTION_TYPE.BURN, target, _owner);
	    producedJob = job;
	    return true;
    }
    #endregion

    #region Seek Shelter
    public bool TriggerSeekShelterJob() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.SEEK_SHELTER) && _owner.gridTileLocation != null) {
            List<LocationStructure> exclusions = null;
            if (_owner.traitContainer.HasTrait("Freezing")) {
                Freezing freezing = _owner.traitContainer.GetNormalTrait<Freezing>("Freezing");
                exclusions = freezing.excludedStructuresInSeekingShelter;
            } else if (_owner.traitContainer.HasTrait("Overheating")) {
                Overheating overheating = _owner.traitContainer.GetNormalTrait<Overheating>("Overheating");
                exclusions = overheating.excludedStructuresInSeekingShelter;
            }
            LocationStructure nearestInteriorStructure = _owner.gridTileLocation.GetNearestInteriorStructureFromThisExcept(exclusions);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SEEK_SHELTER, INTERACTION_TYPE.TAKE_SHELTER, _owner, _owner);
            job.AddOtherData(INTERACTION_TYPE.TAKE_SHELTER, new object[] { nearestInteriorStructure });
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    #endregion

    #region Dark Ritual
    public bool TryCreateDarkRitualJob(out JobQueueItem producedJob) {
	    if (_owner.currentRegion != null) {
		    MagicCircle magicCircle = null;
		    if (_owner.currentRegion.HasTileObjectOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE)) {
			    List<MagicCircle> magicCircles = _owner.currentRegion.GetTileObjectsOfType<MagicCircle>();
			    magicCircle = CollectionUtilities.GetRandomElement(magicCircles);
		    } else {
			    MagicCircle newCircle = InnerMapManager.Instance.CreateNewTileObject<MagicCircle>(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
			    List<LocationGridTile> choices = _owner.currentRegion
				    .GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).unoccupiedTiles.ToList();
			    if (choices.Count > 0) {
				    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(choices);
				    targetTile.structure.AddPOI(newCircle, targetTile);
				    newCircle.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT, IsUnbuiltMagicCircleStillValid);
				    magicCircle = newCircle;
			    }
		    }

		    if (magicCircle != null) {
			    GoapPlanJob ritualJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DARK_RITUAL,
				    INTERACTION_TYPE.DARK_RITUAL, magicCircle, _owner);

			    if (magicCircle.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
				    //if provided magic circle is unbuilt, add a pre-made plan to draw that magic circle.
				    ActualGoapNode drawNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DRAW_MAGIC_CIRCLE], _owner, magicCircle, null, 0);
				    ActualGoapNode ritualNode = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.DARK_RITUAL], _owner, magicCircle, null, 0);
				    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(drawNode), new SingleJobNode(ritualNode) }, magicCircle);
				    goapPlan.SetDoNotRecalculate(true);
				    // ritualJob.SetCannotBePushedBack(true);
				    ritualJob.SetAssignedPlan(goapPlan);
			    }

			    producedJob = ritualJob;
			    Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, CheckIfDarkRitualJobRemoved);
			    return true;
		    }
	    }
	    producedJob = null;
	    return false;
    }
    private void CheckIfDarkRitualJobRemoved(JobQueueItem job, Character character) {
	    if (character == _owner && job.jobType == JOB_TYPE.DARK_RITUAL) {
		    //check if unbuilt magic circle is still valid, if any.
		    Messenger.Broadcast(Signals.CHECK_UNBUILT_OBJECT_VALIDITY);
		    Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, CheckIfDarkRitualJobRemoved);
	    }
	    
    }
    private bool IsUnbuiltMagicCircleStillValid(BaseMapObject mapObject) {
	    return _owner.jobQueue.HasJob(JOB_TYPE.DARK_RITUAL);
    }
    #endregion

    #region Cultist
    public void TriggerCultistTransform() {
	    if (_owner.jobQueue.HasJob(JOB_TYPE.CULTIST_TRANSFORM) == false) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CULTIST_TRANSFORM,
			    INTERACTION_TYPE.CULTIST_TRANSFORM, _owner, _owner);
		    _owner.jobQueue.AddJobInQueue(job);
	    }
    }
    #endregion

    #region Party
    public bool TriggerExploreJob(out JobQueueItem producedJob) { //bool forceDoAction = false
        if (!_owner.partyComponent.hasParty) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.EXPLORE], _owner, _owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.EXPLORE, INTERACTION_TYPE.EXPLORE, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerRescueJob(Character targetCharacter, out JobQueueItem producedJob) {
        if (!_owner.partyComponent.hasParty) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RESCUE], _owner, targetCharacter, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESCUE, INTERACTION_TYPE.RESCUE, targetCharacter, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            producedJob = job;
            return true;
        }
        producedJob = null;
        return false;
    }
    public bool TriggerRescueJob(Character targetCharacter) {
        if (!_owner.partyComponent.hasParty) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RESCUE], _owner, targetCharacter, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESCUE, INTERACTION_TYPE.RESCUE, targetCharacter, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
        return false;
    }
    public bool TriggerMonsterInvadeJob(LocationStructure targetStructure, out JobQueueItem producedJob) {
        if (!_owner.partyComponent.hasParty) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MONSTER_INVADE], _owner, _owner, new object[] { targetStructure }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_INVADE, INTERACTION_TYPE.MONSTER_INVADE, _owner, _owner);
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
        if (!_owner.partyComponent.hasParty) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.MONSTER_INVADE], _owner, _owner, new object[] { targetHex }, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_INVADE, INTERACTION_TYPE.MONSTER_INVADE, _owner, _owner);
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

    #region Party
    public bool TriggerReleaseJob(Character targetCharacter) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.RELEASE_CHARACTER)) {
            ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RELEASE_CHARACTER], targetCharacter, _owner, null, 0);
            GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, targetCharacter);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RELEASE_CHARACTER, INTERACTION_TYPE.RELEASE_CHARACTER, targetCharacter, _owner);
            goapPlan.SetDoNotRecalculate(true);
            job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            return _owner.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    #endregion

    public void TriggerInspect(TileObject item) {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.INSPECT, item, _owner);
	    //create predetermined plan
	    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.INSPECT],
		    _owner, item, null, 0);
	    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, item);
	    goapPlan.SetDoNotRecalculate(true);
	    job.SetCannotBePushedBack(true);
	    job.SetAssignedPlan(goapPlan);
	    _owner.jobQueue.AddJobInQueue(job);
    }
}
