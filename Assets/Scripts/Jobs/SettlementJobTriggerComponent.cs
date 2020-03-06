using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Interrupts;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class SettlementJobTriggerComponent : JobTriggerComponent {

	private readonly Settlement _owner;

	private const int MinimumFood = 100;
	private const int MinimumMetal = 100;
	private const int MinimumStone = 100;
	private const int MinimumWood = 100;
	
	public SettlementJobTriggerComponent(Settlement owner) {
		_owner = owner;
	}
	
	#region Listeners
	public void SubscribeToListeners() {
		Messenger.AddListener(Signals.HOUR_STARTED, HourlyJobActions);
		Messenger.AddListener<ResourcePile>(Signals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChanged);
		Messenger.AddListener<IPointOfInterest>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
		Messenger.AddListener<IPointOfInterest>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
		Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.AddListener<Table>(Signals.FOOD_IN_DWELLING_CHANGED, OnFoodInDwellingChanged);
		Messenger.AddListener<Settlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.AddListener<Character, IPointOfInterest>(Signals.CHARACTER_SAW, OnCharacterSaw);
		Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
		Messenger.AddListener<Settlement>(Signals.SETTLEMENT_CHANGE_STORAGE, OnSettlementChangedStorage);
		Messenger.AddListener<BurningSource>(Signals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
	}
	public void UnsubscribeListeners() {
		Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyJobActions);
		Messenger.RemoveListener<ResourcePile>(Signals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChanged);
		Messenger.RemoveListener<IPointOfInterest>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
		Messenger.RemoveListener<IPointOfInterest>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
		Messenger.RemoveListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.RemoveListener<Table>(Signals.FOOD_IN_DWELLING_CHANGED, OnFoodInDwellingChanged);
		Messenger.RemoveListener<Settlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
		Messenger.RemoveListener<Settlement>(Signals.SETTLEMENT_CHANGE_STORAGE, OnSettlementChangedStorage);
		Messenger.RemoveListener<BurningSource>(Signals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
	}
	private void HourlyJobActions() {
		CreatePatrolJobs();
	}
	private void OnResourceInPileChanged(ResourcePile resourcePile) {
		if (resourcePile.gridTileLocation != null && resourcePile.structureLocation == _owner.mainStorage) {
			CheckResource(resourcePile.tileObjectType, resourcePile.providedResource);
			Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.COMBINE_STOCKPILE, resourcePile as IPointOfInterest);
			TryCreateCombineStockpile(resourcePile);
		}
	}
	private void OnObjectDamaged(IPointOfInterest poi) {
		Assert.IsTrue(poi is TileObject); // || poi is SpecialToken
		if (poi.gridTileLocation != null && poi.gridTileLocation.IsPartOfSettlement(_owner)) {
			TryCreateRepairJob(poi);
		}
	}
	private void OnObjectRepaired(IPointOfInterest poi) {
		Assert.IsTrue(poi is TileObject); // || poi is SpecialToken
		if (poi.gridTileLocation != null && poi.gridTileLocation.IsPartOfSettlement(_owner)) {
			//cancel existing repair job
			Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.REPAIR, poi);
		}
	}
	private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
		if (tileObject is ResourcePile resourcePile) {
			if (resourcePile.resourceInPile > 0) {
				Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.HAUL, resourcePile as IPointOfInterest);
				Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.COMBINE_STOCKPILE, resourcePile as IPointOfInterest);
				if (tile.IsPartOfSettlement(_owner)) {
					CheckResource(resourcePile.tileObjectType, resourcePile.providedResource);
					if (_owner.mainStorage == resourcePile.structureLocation) {
						TryCreateCombineStockpile(resourcePile);	
					}
				}
				TryCreateHaulJob(resourcePile);	
			}
		}
	}
	private void OnTileObjectRemoved(TileObject tileObject, Character removedBy, LocationGridTile removedFrom) {
		if (tileObject is ResourcePile) {
			ResourcePile resourcePile = tileObject as ResourcePile;
			if (removedFrom.IsPartOfSettlement(_owner)) {
				CheckResource(resourcePile.tileObjectType, resourcePile.providedResource);	
			}
		}
	}
	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
		if (structure.settlementLocation == _owner) {
			if (structure.structureType == STRUCTURE_TYPE.PRISON) {
				TryCreateJudgePrisoner(character);
			}
		}
	}
	private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
		if (traitable is Character) {
			Character target = traitable as Character;
			if (trait is Restrained) {
				TryCreateJudgePrisoner(target);
			} else if (trait is Criminal) {
				TryCreateApprehend(target);
			}
		}
	}
	private void OnCharacterEnteredHexTile(Character character, HexTile tile) {
		if (_owner.tiles.Contains(tile)) {
			if (character.traitContainer.HasTrait("Criminal")) {
				TryCreateApprehend(character);
			}
		}
	}
	private void OnFoodInDwellingChanged(Table table) {
		if (table.gridTileLocation.IsPartOfSettlement(_owner)) {
			TryTriggerObtainPersonalFood(table);
		}
	}
	private void OnSettlementUnderSiegeChanged(Settlement settlement, bool isUnderSiege) {
		if (settlement == _owner) {
			if (isUnderSiege) {
				TryCreateKnockoutJobs();
			}	
		}
	}
	private void OnCharacterSaw(Character character, IPointOfInterest seenPOI) {
		if (character.homeSettlement == _owner && character.homeSettlement.isUnderSeige 
		                                       && character.currentSettlement == character.homeSettlement) {
			if (seenPOI is Character) {
				Character target = seenPOI as Character;
				TryCreateKnockoutJobs(target);
			}	
		}
	}
	private void OnCharacterFinishedJobSuccessfully(Character character, GoapPlanJob goapPlanJob) {
		if (goapPlanJob.originalOwner == _owner) {
			if (goapPlanJob.jobType == JOB_TYPE.PRODUCE_FOOD || goapPlanJob.jobType == JOB_TYPE.PRODUCE_WOOD ||
			    goapPlanJob.jobType == JOB_TYPE.PRODUCE_METAL || goapPlanJob.jobType == JOB_TYPE.PRODUCE_STONE) {
				ResourcePile resourcePile = goapPlanJob.targetPOI as ResourcePile;
				CheckResource(resourcePile.tileObjectType, resourcePile.providedResource);
			}
		}
	}
	private void OnSettlementChangedStorage(Settlement settlement) {
		if (settlement == _owner) {
			List<ResourcePile> resourcePiles = _owner.region.GetTileObjectsOfType<ResourcePile>();
			for (int i = 0; i < resourcePiles.Count; i++) {
				ResourcePile resourcePile = resourcePiles[i];
				TryCreateHaulJob(resourcePile);
			}
		}
	}
	private void OnBurningSourceInactive(BurningSource burningSource) {
		if (burningSource.location == _owner.region) {
			CheckDouseFireJobsValidity();
		}
	}
	#endregion

	#region Resources
	private int GetTotalResource(RESOURCE resourceType) {
		int resource = 0;
		List<ResourcePile> piles = _owner.mainStorage.GetTileObjectsOfType<ResourcePile>();
		for (int i = 0; i < piles.Count; i++) {
			ResourcePile resourcePile = piles[i];
			if (resourcePile.providedResource == resourceType) {
				resource += piles[i].resourceInPile;	
			}
		}
		return resource;
	}
	private int GetMinimumResource(RESOURCE resource) {
		switch (resource) {
			case RESOURCE.FOOD:
				return MinimumFood;
			case RESOURCE.WOOD:
				return MinimumWood;
			case RESOURCE.METAL:
				return MinimumMetal;
			case RESOURCE.STONE:
				return MinimumStone;
		}
		throw new Exception($"There is no minimum resource for {resource.ToString()}");
	}
	private JOB_TYPE GetProduceResourceJobType(RESOURCE resource) {
		switch (resource) {
			case RESOURCE.FOOD:
				return JOB_TYPE.PRODUCE_FOOD;
			case RESOURCE.WOOD:
				return JOB_TYPE.PRODUCE_WOOD;
			case RESOURCE.METAL:
				return JOB_TYPE.PRODUCE_METAL;
			case RESOURCE.STONE:
				return JOB_TYPE.PRODUCE_STONE;
		}
		throw new Exception($"There is no produce resource job type for {resource.ToString()}");
	}
	private GOAP_EFFECT_CONDITION GetProduceResourceGoapEffect(RESOURCE resource) {
		switch (resource) {
			case RESOURCE.FOOD:
				return GOAP_EFFECT_CONDITION.PRODUCE_FOOD;
			case RESOURCE.WOOD:
				return GOAP_EFFECT_CONDITION.PRODUCE_WOOD;
			case RESOURCE.METAL:
				return GOAP_EFFECT_CONDITION.PRODUCE_METAL;
			case RESOURCE.STONE:
				return GOAP_EFFECT_CONDITION.PRODUCE_STONE;
		}
		throw new Exception($"There is no produce resource goap effect type for {resource.ToString()}");
	}
	
	private void CheckResource(TILE_OBJECT_TYPE resourcePile, RESOURCE resource) {
		int totalResource = GetTotalResource(resource);
		int minimumResource = GetMinimumResource(resource);
		JOB_TYPE jobType = GetProduceResourceJobType(resource);
		if (totalResource < minimumResource) {
			TriggerProduceResource(resource, resourcePile, jobType);
		} else {
			ResourcePile pile = _owner.mainStorage.GetResourcePileObjectWithLowestCount(resourcePile, false);
			Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, jobType, pile as IPointOfInterest);
			Assert.IsNotNull(pile, $"{_owner.name} is trying to cancel produce resource {resource.ToString()}, but could not find any pile of type {resourcePile.ToString()}");
			if (IsProduceResourceJobStillValid(resource) == false && pile.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
				_owner.mainStorage.RemovePOI(pile); //remove unbuilt pile
			}
		}
	}
	private void TriggerProduceResource(RESOURCE resourceType, TILE_OBJECT_TYPE resourcePile, JOB_TYPE jobType) {
		if (_owner.HasJob(jobType) == false) {
			ResourcePile targetPile = _owner.mainStorage.GetTileObjectOfType<ResourcePile>(resourcePile);
			if (targetPile == null) {
				ResourcePile newPile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(resourcePile);
				_owner.mainStorage.AddPOI(newPile);
				newPile.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
				targetPile = newPile;
			}
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(
				GetProduceResourceGoapEffect(resourceType), string.Empty, 
				false, GOAP_EFFECT_TARGET.ACTOR), targetPile, _owner);
			if (jobType == JOB_TYPE.PRODUCE_WOOD) {
				job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoProduceWoodJob);
			} else if (jobType == JOB_TYPE.PRODUCE_METAL) {
				job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoProduceMetalJob);
			} else {
				job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);	
			}
			
			job.SetStillApplicableChecker(() => IsProduceResourceJobStillValid(resourceType));
			_owner.AddToAvailableJobs(job);	
		}
	}
	private bool IsProduceResourceJobStillValid(RESOURCE resource) {
		return GetTotalResource(resource) < GetMinimumResource(resource);
	}
	#endregion

	#region Repair
	private void TryCreateRepairJob(IPointOfInterest target) {
		if (_owner.HasJob(JOB_TYPE.REPAIR, target) == false) {
			GoapPlanJob job =
				JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR, target, _owner);
			job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRepairJob);
			job.SetStillApplicableChecker(() => IsRepairJobStillValid(target));
			if (target is TileObject) {
				TileObject tileObject = target as TileObject;
				job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] {
					(int) (TileObjectDB.GetTileObjectData(tileObject.tileObjectType).constructionCost * 0.5f)
				});	
			} 
			// else if (target is SpecialToken) {
			// 	SpecialToken specialToken = target as SpecialToken;
			// 	job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] {
			// 		TokenManager.Instance.itemData[specialToken.specialTokenType].craftCost
			// 	});
			// }
			
			_owner.AddToAvailableJobs(job);
		}
	}
	private bool IsRepairJobStillValid(IPointOfInterest target) {
		return target.currentHP < target.maxHP && target.gridTileLocation != null 
		                                       && target.gridTileLocation.IsPartOfSettlement(_owner);
	}
	#endregion

	#region Haul
	private void TryCreateHaulJob(ResourcePile target) {
		//if target is in this settlement and is not in the main storage, then create a haul job.
		//if target is not in this settlement, check if it is in the wilderness, if it is, then create haul job
		bool isInValidLocation = (target.gridTileLocation.IsPartOfSettlement(_owner) &&
		                          target.gridTileLocation.structure != _owner.mainStorage)
		                         || (target.gridTileLocation.IsPartOfSettlement(_owner) == false &&
		                             target.gridTileLocation.structure.isInterior == false);
		if (isInValidLocation && _owner.HasJob(JOB_TYPE.HAUL, target) == false && target.gridTileLocation.parentMap.location == _owner.region) {
			ResourcePile chosenPileToBeDeposited = _owner.mainStorage.GetResourcePileObjectWithLowestCount(target.tileObjectType);
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, 
				new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, 
					false, GOAP_EFFECT_TARGET.TARGET), 
				target, _owner);
			if (chosenPileToBeDeposited != null) {
			    job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[] { chosenPileToBeDeposited });
			}
			job.SetStillApplicableChecker(() => IsHaulResourcePileStillApplicable(target));
			_owner.AddToAvailableJobs(job);
		}
	}
	private bool IsHaulResourcePileStillApplicable(ResourcePile resourcePile) {
		return resourcePile.isBeingCarriedBy != null || (resourcePile.gridTileLocation != null
		       && resourcePile.gridTileLocation.structure != _owner.mainStorage);
	}
	#endregion

	#region Judge Prisoner
	private void TryCreateJudgePrisoner(Character target) {
		if (target.traitContainer.HasTrait("Restrained")
		    && target.currentStructure.structureType == STRUCTURE_TYPE.PRISON
		    && target.currentStructure.settlementLocation == _owner) {
			if (!target.HasJobTargetingThis(JOB_TYPE.JUDGE_PRISONER)) {
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.JUDGE_CHARACTER, target, _owner);
				job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoJudgementJob);
				job.SetStillApplicableChecker(() => InteractionManager.Instance.IsJudgementJobStillApplicable(target));
				_owner.AddToAvailableJobs(job);
			}
		}
	}
	#endregion

	#region Apprehend
	private void TryCreateApprehend(Character target) {
		if (target.currentSettlement == _owner && target.traitContainer.HasTrait("Criminal")) {
			if (target.faction == _owner.owner || target.faction.IsHostileWith(_owner.owner)) {
				if (_owner.HasJob(JOB_TYPE.APPREHEND, target) == false) {
					GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, 
						target, _owner);
					job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeApprehendJob);
					job.SetStillApplicableChecker(() => IsApprehendStillApplicable(target));
					job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { _owner.prison });
					_owner.AddToAvailableJobs(job);	
				}
			}
		}
	}
	private bool IsApprehendStillApplicable(Character target) {
		return target.gridTileLocation != null && target.gridTileLocation.IsNextToOrPartOfSettlement(_owner);
	}
	#endregion

	#region Patrol
	private void CreatePatrolJobs() {
		int patrolChance = UnityEngine.Random.Range(0, 100);
		if (patrolChance < 15 && _owner.GetNumberOfJobsWith(CHARACTER_STATE.PATROL) < 2) {
			CharacterStateJob stateJob = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.PATROL, CHARACTER_STATE.PATROL, _owner);
			stateJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoPatrol);
			_owner.AddToAvailableJobs(stateJob);
		}
	}
	#endregion

	#region Obtain Personal Food
	private void TryTriggerObtainPersonalFood(Table table) {
		if (table.food < 20 && _owner.HasJob(JOB_TYPE.OBTAIN_PERSONAL_FOOD, table) == false) {
			int neededFood = table.GetMaxResourceValue(RESOURCE.FOOD) - table.food;
			GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", false, GOAP_EFFECT_TARGET.TARGET);
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_FOOD, goapEffect, table, _owner);
			job.SetCanTakeThisJobChecker(CanTakeObtainPersonalFoodJob);
			job.SetStillApplicableChecker(() => table.gridTileLocation != null);
			job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { neededFood });
			_owner.AddToAvailableJobs(job);
		}
	}
	private bool CanTakeObtainPersonalFoodJob(Character character, JobQueueItem job) {
		GoapPlanJob goapPlanJob = job as GoapPlanJob;
		if (goapPlanJob.targetPOI.gridTileLocation.structure is IDwelling) {
			IDwelling dwelling = goapPlanJob.targetPOI.gridTileLocation.structure as IDwelling;
			return dwelling.IsResident(character);
		}
		return false;
	}
	#endregion

	#region Combine Stockpile
	private void TryCreateCombineStockpile(ResourcePile pile) {
		if (pile.IsAtMaxResource(pile.providedResource)) {
			return; //if given pile is at maximum capacity, then do not create combine job for it
		}
		if (_owner.HasJob(JOB_TYPE.COMBINE_STOCKPILE, pile)) {
			return; //already has job to combine stockpile.
		}
		//get all resource piles inside the main storage, then check if iny of them are not at max capacity,
		//if not at max capacity, check if the pile can handle the resources of the new pile,
		//if it can, then create combine job
		List<ResourcePile> resourcePiles = _owner.mainStorage.GetTileObjectsOfType<ResourcePile>(pile.tileObjectType);
		ResourcePile targetPile = null;
		for (int i = 0; i < resourcePiles.Count; i++) {
			ResourcePile currPile = resourcePiles[i];
			if (currPile != pile && currPile.IsAtMaxResource(pile.providedResource) == false
			    && currPile.HasEnoughSpaceFor(pile.providedResource, pile.resourceInPile)) {
				targetPile = currPile;
				break;
			}
		}
		if (targetPile != null && _owner.HasJob(JOB_TYPE.COMBINE_STOCKPILE, targetPile) == false) { //only create job if chosen target pile does not already have a job to combine it with another pile
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COMBINE_STOCKPILE, 
				INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, pile, _owner);
			job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, 
				new object[] { targetPile });
			job.SetStillApplicableChecker(() => IsCombineStockpileStillApplicable(targetPile, pile, _owner));
			job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
			_owner.AddToAvailableJobs(job);
		}
	}
	private bool IsCombineStockpileStillApplicable(ResourcePile targetPile, ResourcePile pileToDeposit, Settlement settlement) {
		return targetPile.gridTileLocation != null
		       && targetPile.gridTileLocation.IsPartOfSettlement(settlement)
		       && targetPile.structureLocation == settlement.mainStorage
		       && pileToDeposit.gridTileLocation != null
		       && pileToDeposit.gridTileLocation.IsPartOfSettlement(settlement)
		       && pileToDeposit.structureLocation == settlement.mainStorage
		       && targetPile.HasEnoughSpaceFor(pileToDeposit.providedResource, pileToDeposit.resourceInPile);
	}
	#endregion

	#region Knockout
	private void TryCreateKnockoutJobs() {
		string summary = $"{GameManager.Instance.TodayLogString()}{_owner.name} is under siege, trying to create knockout jobs...";
		if (CanCreateKnockoutJob()) {
			int combatantResidents = 
				_owner.residents.Count(x => x.traitContainer.HasTrait("Combatant"));
			int existingKnockoutJobs = _owner.GetNumberOfJobsWith(JOB_TYPE.KNOCKOUT);
			summary += $"\nCombatant residents: {combatantResidents.ToString()}";
			summary += $"\nExisting knockout jobs: {existingKnockoutJobs.ToString()}";
			List<Character> hostileCharacters = _owner.GetHostileCharactersInSettlement();
			if (hostileCharacters.Count > 0) {
				Character target = hostileCharacters.First();
				int jobsToCreate = combatantResidents - existingKnockoutJobs;
				summary += $"\nWill create {jobsToCreate.ToString()} knockout jobs.";
				for (int i = 0; i < jobsToCreate; i++) {
					summary += $"\nWill create knockout job targeting {target.name}.";
					CreateKnockoutJob(target);
				}	
			}
		} else {
			summary += $"\nCannot create knockout jobs";
		}
		Debug.Log(summary);
	}
	private void TryCreateKnockoutJobs(Character target) {
		if (CanCreateKnockoutJob() && target.faction.IsHostileWith(_owner.owner)) {
			int combatantResidents = 
				_owner.residents.Count(x => x.traitContainer.HasTrait("Combatant"));
			int existingKnockoutJobs = _owner.GetNumberOfJobsWith(JOB_TYPE.KNOCKOUT);
			int jobsToCreate = combatantResidents - existingKnockoutJobs;
			for (int i = 0; i < jobsToCreate; i++) {
				CreateKnockoutJob(target);
			}	
		}
	}
	private bool CanCreateKnockoutJob() {
		int combatantResidents = 
			_owner.residents.Count(x => x.traitContainer.HasTrait("Combatant"));
		int existingKnockoutJobs = _owner.GetNumberOfJobsWith(JOB_TYPE.KNOCKOUT);
		return existingKnockoutJobs < combatantResidents;
	}
	private void CreateKnockoutJob(Character target) {
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.KNOCKOUT, 
			new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Unconscious",
				false, GOAP_EFFECT_TARGET.TARGET), 
			target, _owner);
		job.SetStillApplicableChecker(() => IsKnockOutJobStillApplicable(target));
		job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeKnockoutJob);
		_owner.AddToAvailableJobs(job, 0);
	}
	private bool IsKnockOutJobStillApplicable(Character target) {
		return target.canPerform && target.gridTileLocation != null
		    && target.gridTileLocation.IsNextToOrPartOfSettlement(_owner);
	}
	#endregion

	#region Douse Fire
	public void TriggerDouseFire() {
        if (_owner.region.innerMap.activeBurningSources.Count(x => x.HasFireInSettlement(_owner)) > 0) {
			int existingDouseFire = _owner.GetNumberOfJobsWith(CHARACTER_STATE.DOUSE_FIRE);
			int douseFireJobs = 3;
			if (existingDouseFire < douseFireJobs) {
				int missing = douseFireJobs - existingDouseFire;
				for (int i = 0; i < missing; i++) {
					CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.DOUSE_FIRE, 
						CHARACTER_STATE.DOUSE_FIRE, _owner);
					job.SetCanTakeThisJobChecker(CanTakeRemoveFireJob);
					_owner.AddToAvailableJobs(job, 0);
				}	
			}	
		}
	}
	private void CheckDouseFireJobsValidity() {
		if (_owner.region.innerMap.activeBurningSources.Count(x => x.HasFireInSettlement(_owner)) == 0) {
			//cancel all douse fire jobs
			List<JobQueueItem> jobs = _owner.GetJobs(JOB_TYPE.DOUSE_FIRE);
			for (int i = 0; i < jobs.Count; i++) {
				JobQueueItem jqi = jobs[i];
				if (jqi.assignedCharacter == null) {
					jqi.ForceCancelJob(false, "no more fires");	
				}
			}
		}
	}
	private bool CanTakeRemoveFireJob(Character character, IPointOfInterest target) {
		if (target is Character targetCharacter) {
			if (character == targetCharacter) {
				//the burning character is himself
				return HasWaterAvailable(character);
			} else {
				//if burning character is other character, make sure that the character that will do the job is not burning.
				return !character.traitContainer.HasTrait("Burning", "Pyrophobic") 
				       && !character.relationshipContainer.IsEnemiesWith(targetCharacter)
				       && HasWaterAvailable(character);
			}
		} else {
			//make sure that the character that will do the job is not burning.
			return !character.traitContainer.HasTrait("Burning", "Pyrophobic") && HasWaterAvailable(character);
		}
	}
	private bool HasWaterAvailable(Character character) {
		return character.currentRegion.HasTileObjectOfType(TILE_OBJECT_TYPE.WATER_WELL);
	}
	#endregion
}
