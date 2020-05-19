using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
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
    private bool hasStartedScreamCheck;

    public CharacterJobTriggerComponent(Character owner) {
		_owner = owner;
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
			if (_owner.currentSettlement is NPCSettlement npcSettlement && npcSettlement.isUnderSiege) {
				TriggerFleeHome();	
			}
			// character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.RESTRAIN); //cancel all restrain jobs.
			_owner.needsComponent.CheckExtremeNeeds();
            for (int i = 0; i < _owner.marker.inVisionCharacters.Count; i++) {
                Character inVisionCharacter = _owner.marker.inVisionCharacters[i];
                _owner.marker.AddUnprocessedPOI(inVisionCharacter);
            }
        }
	}
	private void OnCharacterCanNoLongerPerform(Character character) {
		if (character == _owner && character.isDead == false) {
			//TODO: THIS IS ONLY TEMPORARY! REDO THIS!
			if (character.interruptComponent.isInterrupted &&
			           character.interruptComponent.currentInterrupt.interrupt == INTERRUPT.Narcoleptic_Attack) {
				//Don't do anything
			} else if (character.currentActionNode != null && character.currentActionNode.actionStatus == ACTION_STATUS.PERFORMING && InteractionManager.Instance.IsActionTirednessRecovery(character.currentActionNode.action)) {
				character.CancelAllJobsExceptForCurrent();
			} else {
				character.jobQueue.CancelAllJobs();
			}
			character.marker.StopMovement();
			character.marker.pathfindingAI.ClearAllCurrentPathData();

            character.UncarryPOI();
            if (character.traitContainer.HasTrait("Unconscious")) {
                character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.KNOCKOUT);
            }

            _owner.behaviourComponent.SetIsHarassing(false, null);
            _owner.behaviourComponent.SetIsInvading(false, null);
            _owner.behaviourComponent.SetIsDefending(false, null);
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
		// 	TriggerBurySerialKillerVictim(job);
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
		if (npcSettlement == _owner.currentSettlement && siegeState && (_owner.stateComponent.currentState is CombatState) == false) {
            //characters current npcSettlement is under siege
            if (!_owner.isInCombat) {
                _owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stopped, _owner);
                Messenger.AddListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfStopInterruptFinished);
            }
        }
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

    #region Job Triggers
    private void TriggerScreamJob() {
		if (_owner.jobQueue.HasJob(JOB_TYPE.SCREAM) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SCREAM, INTERACTION_TYPE.SCREAM_FOR_HELP, _owner, _owner);
			_owner.jobQueue.AddJobInQueue(job);
		}
	}
	public void TriggerBurySerialKillerVictim(Character target) {
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
	public void TriggerFleeHome(JOB_TYPE jobType = JOB_TYPE.FLEE_TO_HOME) {
		if (!_owner.jobQueue.HasJob(jobType)) {
			ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], _owner, _owner, null, 0);
			GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.RETURN_HOME, _owner, _owner);
			goapPlan.SetDoNotRecalculate(true);
			job.SetCannotBePushedBack(true);
			job.SetAssignedPlan(goapPlan);
			_owner.jobQueue.AddJobInQueue(job);
		}
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
	private void TriggerRemoveStatus(Trait trait) {
		if (_owner.isDead) { return; }
		if (trait.gainedFromDoing == null || trait.gainedFromDoing.isStealth == false) { //only create remove status job if trait was not gained from a stealth action
			GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, target = GOAP_EFFECT_TARGET.TARGET };
			if (_owner.homeSettlement.HasJob(goapEffect, _owner) == false) {
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, _owner, _owner.homeSettlement);
				job.SetCanTakeThisJobChecker((Character character, JobQueueItem jqi) => CanTakeRemoveStatus(character, job, trait));
				job.SetStillApplicableChecker(() => IsRemoveStatusJobStillApplicable(_owner, job, trait));
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
	private void TriggerFeed(Character target) {
		GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, target = GOAP_EFFECT_TARGET.TARGET };
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, target, _owner);
		job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
		_owner.jobQueue.AddJobInQueue(job);
	}
	private void TriggerRestrain(NPCSettlement npcSettlement) {
		if (npcSettlement.HasJob(JOB_TYPE.RESTRAIN, _owner) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESTRAIN, INTERACTION_TYPE.RESTRAIN_CHARACTER, _owner, npcSettlement);
        	job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRestrainJob);
        	job.SetStillApplicableChecker(() => IsRestrainApplicable(_owner, npcSettlement));
        	npcSettlement.AddToAvailableJobs(job);	
		}
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
	private bool IsRemoveStatusJobStillApplicable(Character target, GoapPlanJob job, Trait trait) {
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
	private bool IsRestrainApplicable(Character target, NPCSettlement npcSettlement) {
		return target.canPerform == false && target.gridTileLocation != null &&
		       target.gridTileLocation.IsNextToOrPartOfSettlement(npcSettlement);
	}
	#endregion

	#region Job Checkers
	private bool CanTakeRemoveStatus(Character character, JobQueueItem job, Trait trait) {
		if (job is GoapPlanJob) {
			GoapPlanJob goapPlanJob = job as GoapPlanJob;
			Character targetCharacter = goapPlanJob.targetPOI as Character;
			// if (character != targetCharacter) {
				bool isHostile = character.IsHostileWith(targetCharacter, false);
				bool isResponsibleForTrait = trait.IsResponsibleForTrait(character);

				//if special illness, check if character is healer
				if (TraitManager.Instance.specialIllnessTraits.Contains(trait.name)) {
					return isHostile == false &&
					       character.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter,
						       RelationshipManager.Rival, RelationshipManager.Enemy) == false 
					       && isResponsibleForTrait == false
                           && !character.traitContainer.HasTrait("Psychopath")
                           && character.traitContainer.HasTrait("Healing Expert");	
				}
				
				return isHostile == false &&
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

	#region Flee to home
	private void CheckIfStopInterruptFinished(INTERRUPT interrupt, Character character) {
		if (character == _owner && interrupt == INTERRUPT.Stopped) {
			Messenger.RemoveListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfStopInterruptFinished);
			if (_owner.canPerform) {
				TriggerFleeHome();	
			}
		}
	}
	#endregion

	#region Remove Status
	private void TryCreateRemoveStatusJob(Trait trait) {
		if (_owner.homeSettlement != null && _owner.gridTileLocation.IsNextToOrPartOfSettlement(_owner.homeSettlement)
		    && _owner.traitContainer.HasTrait("Criminal") == false) {
			TriggerRemoveStatus(trait);
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

	#region Restrain
	private void TryTriggerRestrain() {
		if (_owner.gridTileLocation.IsPartOfSettlement(out var nearSettlement) 
		    || _owner.gridTileLocation.IsNextToSettlement(out nearSettlement)) {
			if (nearSettlement is NPCSettlement npcSettlement && nearSettlement.owner != null 
			    && _owner.faction != nearSettlement.owner) {
				if (_owner.faction.IsHostileWith(nearSettlement.owner)) {
					TriggerRestrain(npcSettlement);
				}
			}	
		}
		
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
    public bool TriggerAttackDemonicStructure(LocationGridTile tile = null) {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.ASSAULT_DEMONIC_STRUCTURE)) {
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
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ASSAULT_DEMONIC_STRUCTURE, INTERACTION_TYPE.ATTACK_DEMONIC_STRUCTURE, _owner, _owner);
            goapPlan.SetDoNotRecalculate(true);
            // job.SetCannotBePushedBack(true);
            job.SetAssignedPlan(goapPlan);
            _owner.jobQueue.AddJobInQueue(job);
            return true;
        }
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
    public bool TriggerReturnTerritory() {
        if (!_owner.jobQueue.HasJob(JOB_TYPE.RETURN_TERRITORY)) {
            LocationGridTile chosenTile;
            if (_owner.homeStructure != null) {
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
    public void CreateLearnMonsterJob(Character target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.LEARN_MONSTER, INTERACTION_TYPE.STUDY_MONSTER, target, _owner);
        _owner.jobQueue.AddJobInQueue(job);
    }
    public void CreateTakeArtifactJob(TileObject target, LocationStructure dropLocation) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TAKE_ARTIFACT, INTERACTION_TYPE.DROP_ITEM, target, _owner);
        job.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[] { dropLocation });
        _owner.jobQueue.AddJobInQueue(job);
    }
    public void CreatePickUpJob(TileObject target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TAKE_ITEM, INTERACTION_TYPE.PICK_UP, target, _owner);
        _owner.jobQueue.AddJobInQueue(job);
    }
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
    
    #region Other Characters
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
    public GoapPlanJob CreateKillJob(Character targetCharacter) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.KILL, targetCharacter)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KILL, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, _owner);
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
            if(_owner.characterClass.interestedItemNames != null && _owner.characterClass.interestedItemNames.Length > 0) {
                string chosenItemName;
                if(obtainPersonalItemRandomList == null) {
                    obtainPersonalItemRandomList = new List<string>();
                }
                obtainPersonalItemRandomList.Clear();
                for (int i = 0; i < _owner.characterClass.interestedItemNames.Length; i++) {
                    string itemName = _owner.characterClass.interestedItemNames[i];
                    if (!_owner.HasItem(itemName)) {
                        obtainPersonalItemRandomList.Add(itemName);
                    }
                }
                if(obtainPersonalItemRandomList.Count > 0) {
                    chosenItemName = obtainPersonalItemRandomList[UnityEngine.Random.Range(0, obtainPersonalItemRandomList.Count)];
                } else {
                    chosenItemName = _owner.characterClass.interestedItemNames[UnityEngine.Random.Range(0, _owner.characterClass.interestedItemNames.Length)];
                }
                GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, chosenItemName, false, GOAP_EFFECT_TARGET.ACTOR);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_ITEM, goapEffect, _owner, _owner);
                _owner.jobQueue.AddJobInQueue(job);
                return true;
            }
        }
        return false;

    }
    #endregion

    #region Hide At Home
    public void CreateHideAtHomeJob() {
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HIDE_AT_HOME, INTERACTION_TYPE.RETURN_HOME, _owner, _owner);
	    _owner.jobQueue.AddJobInQueue(job);
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
    public bool CreatePlaceTrapPOIJob(IPointOfInterest target) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Booby Trapped", false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
        _owner.jobQueue.AddJobInQueue(job);
        return true;
    }
    public bool CreatePlaceTrapJob(Character targetCharacter) {
        IPointOfInterest chosenObject = null;
        for (int i = 0; i < targetCharacter.ownedItems.Count; i++) {
            TileObject item = targetCharacter.ownedItems[i];
            if (item.gridTileLocation != null && item.mapVisual) {
                chosenObject = item;
                break;
            }
        }
        if(chosenObject != null) {
            return CreatePlaceTrapPOIJob(chosenObject);
        }
        return false;
    }
    #endregion

    #region Report Demonic Structure
    public void CreateReportDemonicStructure(LocationStructure structureToReport) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.REPORT_CORRUPTED_STRUCTURE)) {
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
    public bool CreatePoisonFoodJob(Character targetCharacter) {
	    if (_owner.jobQueue.HasJob(JOB_TYPE.POISON_FOOD, targetCharacter)) {
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
      //  IPointOfInterest chosenObject = targetCharacter.homeRegion.GetFirstTileObjectOnTheFloorOwnedBy(targetCharacter, 
		    //(poi) => poi.advertisedActions.Contains(INTERACTION_TYPE.EAT));
	    if (chosenObject == null) {
		    targetCharacter.logComponent.PrintLogIfActive(_owner.name + " cannot poison food " + targetCharacter.name + " because he/she does not have an owned item on the floor in his/her home region");
		    return false;
	    }
	    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.POISON_FOOD, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Poisoned", false, GOAP_EFFECT_TARGET.TARGET), chosenObject, _owner);
	    _owner.jobQueue.AddJobInQueue(job);

	    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "poison_undermine");
	    log.AddToFillers(_owner, _owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
	    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
	    _owner.logComponent.AddHistory(log);
	    return true;
    }
    #endregion

    #region Rumor
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
                    if (_owner.gridTileLocation != null && PathfindingManager.Instance.HasPathEvenDiffRegion(_owner.gridTileLocation, chosenTile)) {
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
	    if (_owner.minion == null && _owner.gridTileLocation != null && _owner.gridTileLocation.IsNextToOrPartOfSettlement(out var settlement)
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
    #endregion
}
