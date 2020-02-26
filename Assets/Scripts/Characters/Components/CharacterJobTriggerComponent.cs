using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;
using Random = UnityEngine.Random;

public class CharacterJobTriggerComponent : JobTriggerComponent {

	private Character _owner;
	
	private bool hasStartedScreamCheck;

    public Dictionary<GoapAction, int> numOfTimesActionDone { get; private set; }

    private string[] removeStatusTraits = new[] {
		nameof(Unconscious), nameof(Injured), nameof(Sick), nameof(Plagued),
		nameof(Infected), nameof(Cursed)
	};
	private string[] specialIllnessTraits = new[] {
		nameof(Sick), nameof(Plagued), nameof(Infected)
	};
	
	public CharacterJobTriggerComponent(Character owner) {
		_owner = owner;
        numOfTimesActionDone = new Dictionary<GoapAction, int>();
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
		Messenger.AddListener<Settlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
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
		Messenger.RemoveListener<Settlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        Messenger.RemoveListener<IPointOfInterest>(Signals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.RemoveListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, OnUnseizePOI);
        TryStopScreamCheck();
	}
	private void OnCharacterCanPerformAgain(Character character) {
		if (character == _owner) {
			if (_owner.currentSettlement != null && _owner.currentSettlement.isUnderSeige) {
				TriggerFleeHome();	
			}
			_owner.needsComponent.CheckExtremeNeeds();
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
		}
	}
	private void OnCharacterCanNoLongerMove(Character character) {
		if (character == _owner) {
			TryStartScreamCheck();
			TryTriggerRestrain();
		}
	}
	private void OnCharacterCanMoveAgain(Character character) {
		if (character == _owner) {
			// Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RESTRAIN, _owner as IPointOfInterest);
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
			if (removeStatusTraits.Contains(trait.name)) {
				TryCreateRemoveStatusJob(trait);
			}
			TryStartScreamCheck();
		}
	}
	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner) {
			TryStopScreamCheck();
			if (removeStatusTraits.Contains(nameof(trait))) {
				_owner.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_STATUS, trait.name, removedBy); //so that the character that cured him will not cancel his job.
			}
		}
	}
	private void OnSettlementUnderSiegeChanged(Settlement settlement, bool siegeState) {
		if (settlement == _owner.currentSettlement && siegeState) {
			//characters current settlement is under siege
			_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stopped, _owner);
			Messenger.AddListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfStopInterruptFinished);
		}
	}
	private void OnCharacterEnteredHexTile(Character character, HexTile tile) {
		if (character == _owner) {
			TryCreateRemoveStatusJob();
		}
	}
	private void OnCharacterExitedHexTile(Character character, HexTile tile) {
		if (character == _owner) {
			// Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RESTRAIN, _owner as IPointOfInterest);
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
		GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, target = GOAP_EFFECT_TARGET.TARGET };
		if (_owner.homeSettlement.HasJob(goapEffect, _owner) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, _owner, _owner.homeSettlement);
			job.SetCanTakeThisJobChecker((Character character, JobQueueItem jqi) => CanTakeRemoveStatus(character, job, trait));
			job.SetStillApplicableChecker(() => IsRemoveStatusJobStillApplicable(_owner, job, trait));
			// job.AddOtherData(INTERACTION_TYPE.CRAFT_TILE_OBJECT, new object[] { TILE_OBJECT_TYPE.HEALING_POTION });
			// job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.HEALING_POTION].craftCost });
			_owner.homeSettlement.AddToAvailableJobs(job);
		}
	}
	private void TriggerFeed(Character target) {
		GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, target = GOAP_EFFECT_TARGET.TARGET };
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, target, _owner);
		job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 12 });
		_owner.jobQueue.AddJobInQueue(job);
	}
	private void TriggerRestrain(Settlement settlement) {
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESTRAIN, INTERACTION_TYPE.RESTRAIN_CHARACTER, _owner, settlement);
		job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRestrainJob);
		job.SetStillApplicableChecker(() => IsRestrainApplicable(_owner, settlement));
		settlement.AddToAvailableJobs(job);
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
		if (target.gridTileLocation.IsNextToOrPartOfSettlement(job.originalOwner as Settlement) == false) {
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
	private bool IsRestrainApplicable(Character target, Settlement settlement) {
		return target.canMove == false && target.gridTileLocation != null &&
		       target.gridTileLocation.IsNextToOrPartOfSettlement(settlement);
	}
	#endregion

	#region Job Checkers
	private bool CanTakeRemoveStatus(Character character, JobQueueItem job, Trait trait) {
		if (job is GoapPlanJob) {
			GoapPlanJob goapPlanJob = job as GoapPlanJob;
			Character targetCharacter = goapPlanJob.targetPOI as Character;
			if (character != targetCharacter) {
				bool isHostile = character.IsHostileWith(targetCharacter, false);
				bool isResponsibleForTrait = trait.IsResponsibleForTrait(character);

				//if special illness, check if character is healer
				if (specialIllnessTraits.Contains(nameof(trait))) {
					return isHostile == false &&
					       character.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter,
						       OpinionComponent.Rival, OpinionComponent.Enemy) == false 
					       && isResponsibleForTrait == false
                           && !character.traitContainer.HasTrait("Psychopath")
                           && character.traitContainer.HasTrait("Healer");	
				}
				
				return isHostile == false &&
				       character.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter,
					       OpinionComponent.Rival, OpinionComponent.Enemy) == false 
				       && isResponsibleForTrait == false
                       && !character.traitContainer.HasTrait("Psychopath");
			}
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
			List<Trait> statusTraits = _owner.traitContainer.GetNormalTraits<Trait>(this.removeStatusTraits);
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
		Settlement nearSettlement;
		if (_owner.gridTileLocation.IsPartOfSettlement(out nearSettlement) 
		    || _owner.gridTileLocation.IsNextToSettlement(out nearSettlement)) {
			if (nearSettlement.owner != null && _owner.faction != nearSettlement.owner) {
				// bool isHostileWithFaction =
				// 	_owner.faction.GetRelationshipWith(nearSettlement.owner).relationshipStatus ==
				// 	FACTION_RELATIONSHIP_STATUS.HOSTILE;
				if (_owner.faction.IsHostileWith(nearSettlement.owner)) {
					TriggerRestrain(nearSettlement);
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
	// 		bool isSameHome = target.homeSettlement == _owner.homeSettlement;
	// 		bool isNotHostileFaction = target.faction == _owner.faction
	// 			|| target.faction.GetRelationshipWith(_owner.faction).relationshipStatus
	// 			!= FACTION_RELATIONSHIP_STATUS.HOSTILE;
	// 		bool isNotEnemy =
	// 			_owner.opinionComponent.HasOpinionLabelWithCharacter(target, OpinionComponent.Enemy,
	// 				OpinionComponent.Rival) == false;
	// 		if ((isSameHome || isNotHostileFaction) && isNotEnemy) {
	// 			return TriggerMoveCharacterToBed(target);
	// 		}
	// 	}
	// 	return false;
	// }
	// public bool TryTriggerMoveCharacterHappinessRecovery(Character target) {
	// 	if (target.traitContainer.GetNormalTrait<Trait>("Bored", "Sulking", "Forlorn", "Lonely") != null) {
	// 		bool isSameHome = target.homeSettlement == _owner.homeSettlement;
	// 		bool isNotHostileFaction = target.faction == _owner.faction
	// 		                           || target.faction.GetRelationshipWith(_owner.faction).relationshipStatus
	// 		                           != FACTION_RELATIONSHIP_STATUS.HOSTILE;
	// 		bool isNotEnemy =
	// 			_owner.opinionComponent.HasOpinionLabelWithCharacter(target, OpinionComponent.Enemy,
	// 				OpinionComponent.Rival) == false;
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
    
    #region Monsters/Minions
    public bool TriggerRoamAroundTerritory() {
	    if (_owner is Summon) {
		    if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TERRITORY)) {
			    Summon summon = _owner as Summon;
			    HexTile chosenTerritory = summon.territorries[UnityEngine.Random.Range(0, summon.territorries.Count)];
			    BuildingSpot chosenBuildSpot = chosenTerritory.ownedBuildSpots[UnityEngine.Random.Range(0, chosenTerritory.ownedBuildSpots.Length)];
			    LocationGridTile chosenTile = chosenBuildSpot.tilesInTerritory[UnityEngine.Random.Range(0, chosenBuildSpot.tilesInTerritory.Length)];
			    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
			    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
			    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_TERRITORY, INTERACTION_TYPE.ROAM, _owner, _owner);
			    goapPlan.SetDoNotRecalculate(true);
			    job.SetCannotBePushedBack(true);
			    job.SetAssignedPlan(goapPlan);
			    _owner.jobQueue.AddJobInQueue(job);
			    return true;
		    }
	    }
	    return false;
    }
    public bool TriggerRoamAroundCorruption() {
	    if (_owner.minion != null) {
		    if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_CORRUPTION)) {
			    HexTile chosenTerritory = PlayerManager.Instance.player.playerSettlement.tiles[UnityEngine.Random.Range(0, PlayerManager.Instance.player.playerSettlement.tiles.Count)];
			    BuildingSpot chosenBuildSpot = chosenTerritory.ownedBuildSpots[UnityEngine.Random.Range(0, chosenTerritory.ownedBuildSpots.Length)];
			    LocationGridTile chosenTile = chosenBuildSpot.tilesInTerritory[UnityEngine.Random.Range(0, chosenBuildSpot.tilesInTerritory.Length)];
			    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
			    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
			    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_CORRUPTION, INTERACTION_TYPE.ROAM, _owner, _owner);
			    goapPlan.SetDoNotRecalculate(true);
			    job.SetCannotBePushedBack(true);
			    job.SetAssignedPlan(goapPlan);
			    _owner.jobQueue.AddJobInQueue(job);
			    return true;
		    }
	    }
	    return false;
    }
    public bool TriggerRoamAroundPortal() {
	    if (_owner.minion != null) {
		    if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_PORTAL)) {
			    HexTile chosenTerritory = PlayerManager.Instance.player.portalTile;
			    BuildingSpot chosenBuildSpot = chosenTerritory.ownedBuildSpots[UnityEngine.Random.Range(0, chosenTerritory.ownedBuildSpots.Length)];
			    LocationGridTile chosenTile = chosenBuildSpot.tilesInTerritory[UnityEngine.Random.Range(0, chosenBuildSpot.tilesInTerritory.Length)];
			    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
			    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
			    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ROAM_AROUND_PORTAL, INTERACTION_TYPE.ROAM, _owner, _owner);
			    goapPlan.SetDoNotRecalculate(true);
			    job.SetCannotBePushedBack(true);
			    job.SetAssignedPlan(goapPlan);
			    _owner.jobQueue.AddJobInQueue(job);
			    return true;
		    }
	    }
	    return false;
    }
    public bool TriggerRoamAroundTile(LocationGridTile tile = null) {
	    if (_owner is Summon || _owner.minion != null) {
		    if (!_owner.jobQueue.HasJob(JOB_TYPE.ROAM_AROUND_TILE)) {
                LocationGridTile chosenTile = tile;
                if (chosenTile == null) {
	                if (_owner.gridTileLocation.buildSpotOwner.isPartOfParentRegionMap == false) {
		                return false;
	                }
                    HexTile chosenTerritory = _owner.gridTileLocation.buildSpotOwner.hexTileOwner;
                    BuildingSpot chosenBuildSpot = chosenTerritory.ownedBuildSpots[UnityEngine.Random.Range(0, chosenTerritory.ownedBuildSpots.Length)];
                    chosenTile = chosenBuildSpot.tilesInTerritory[UnityEngine.Random.Range(0, chosenBuildSpot.tilesInTerritory.Length)];
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
	    }
	    return false;
    }
    public bool TriggerMonsterStand() {
	    if (_owner is Summon || _owner.minion != null) {
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
	    }
	    return false;
    }
    public bool TriggerReturnTerritory() {
	    if (_owner is Summon) {
		    if (!_owner.jobQueue.HasJob(JOB_TYPE.RETURN_TERRITORY)) {
			    Summon summon = _owner as Summon;
			    HexTile chosenTerritory = summon.territorries[UnityEngine.Random.Range(0, summon.territorries.Count)];
			    BuildingSpot chosenBuildSpot = chosenTerritory.ownedBuildSpots[UnityEngine.Random.Range(0, chosenTerritory.ownedBuildSpots.Length)];
			    LocationGridTile chosenTile = chosenBuildSpot.tilesInTerritory[UnityEngine.Random.Range(0, chosenBuildSpot.tilesInTerritory.Length)];
			    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
			    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
			    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_TERRITORY, INTERACTION_TYPE.ROAM, _owner, _owner);
			    goapPlan.SetDoNotRecalculate(true);
			    job.SetCannotBePushedBack(true);
			    job.SetAssignedPlan(goapPlan);
			    _owner.jobQueue.AddJobInQueue(job);
			    return true;
		    }
	    }
	    return false;
    }
    public bool TriggerReturnPortal() {
	    if (_owner.minion != null) {
		    if (!_owner.jobQueue.HasJob(JOB_TYPE.RETURN_PORTAL)) {
			    HexTile chosenTerritory = PlayerManager.Instance.player.portalTile;
			    BuildingSpot chosenBuildSpot = chosenTerritory.ownedBuildSpots[UnityEngine.Random.Range(0, chosenTerritory.ownedBuildSpots.Length)];
			    LocationGridTile chosenTile = chosenBuildSpot.tilesInTerritory[UnityEngine.Random.Range(0, chosenBuildSpot.tilesInTerritory.Length)];
			    ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ROAM], _owner, _owner, new object[] { chosenTile }, 0);
			    GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
			    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RETURN_PORTAL, INTERACTION_TYPE.ROAM, _owner, _owner);
			    goapPlan.SetDoNotRecalculate(true);
			    job.SetCannotBePushedBack(true);
			    job.SetAssignedPlan(goapPlan);
			    _owner.jobQueue.AddJobInQueue(job);
			    return true;
		    }
	    }
	    return false;
    }
    public bool TriggerMonsterSleep() {
	    if (_owner is Summon) {
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
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.PICK_UP, target, _owner);
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
        LocationStructure dropLocationStructure = PlayerManager.Instance.player.portalTile.region.GetRandomStructureOfType(STRUCTURE_TYPE.TORTURE_CHAMBER);
        if (dropLocationStructure == null) {
	        dropLocationStructure = PlayerManager.Instance.player.portalTile.locationGridTiles[0].structure;
        }
        job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure }); //For now drop in portal, this will be changed to Demonic Prison
        _owner.jobQueue.AddJobInQueue(job);
    }
    #endregion
    
    #region Other Characters
    public void CreateKnockoutJob(Character targetCharacter) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.KNOCKOUT, targetCharacter)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, _owner);
		    _owner.jobQueue.AddJobInQueue(job);
	    }
    }
    public void CreateKillJob(Character targetCharacter) {
	    if (!_owner.jobQueue.HasJob(JOB_TYPE.KILL, targetCharacter)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KILL, new GoapEffect(GOAP_EFFECT_CONDITION.DEATH, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), targetCharacter, _owner);
		    _owner.jobQueue.AddJobInQueue(job);
	    }
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
    #endregion
}
