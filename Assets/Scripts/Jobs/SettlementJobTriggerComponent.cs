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

	private readonly NPCSettlement _owner;

	private const int MinimumFood = 100;
	private const int MinimumMetal = 100;
	private const int MinimumStone = 100;
	private const int MinimumWood = 100;

	public List<LocationGridTile> wetTiles { get; }
	public List<LocationGridTile> poisonedTiles { get; }
	
	public SettlementJobTriggerComponent(NPCSettlement owner) {
		_owner = owner;
		wetTiles = new List<LocationGridTile>();
		poisonedTiles = new List<LocationGridTile>();
	}
	
	#region Listeners
	public void SubscribeToListeners() {
		Messenger.AddListener(Signals.HOUR_STARTED, HourlyJobActions);
		Messenger.AddListener<ResourcePile>(Signals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChanged);
		Messenger.AddListener<IPointOfInterest, int>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
		Messenger.AddListener<IPointOfInterest>(Signals.OBJECT_FULLY_REPAIRED, OnObjectFullyRepaired);
		Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.AddListener<Table>(Signals.FOOD_IN_DWELLING_CHANGED, OnFoodInDwellingChanged);
		Messenger.AddListener<NPCSettlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.AddListener<Character, IPointOfInterest>(Signals.CHARACTER_SAW, OnCharacterSaw);
		// Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
		Messenger.AddListener<NPCSettlement>(Signals.SETTLEMENT_CHANGE_STORAGE, OnSettlementChangedStorage);
		Messenger.AddListener<BurningSource>(Signals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
	}
	public void UnsubscribeListeners() {
		Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyJobActions);
		Messenger.RemoveListener<ResourcePile>(Signals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChanged);
		Messenger.RemoveListener<IPointOfInterest, int>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
		Messenger.RemoveListener<IPointOfInterest>(Signals.OBJECT_FULLY_REPAIRED, OnObjectFullyRepaired);
		Messenger.RemoveListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.RemoveListener<Table>(Signals.FOOD_IN_DWELLING_CHANGED, OnFoodInDwellingChanged);
		Messenger.RemoveListener<NPCSettlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		// Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJobSuccessfully);
		Messenger.RemoveListener<NPCSettlement>(Signals.SETTLEMENT_CHANGE_STORAGE, OnSettlementChangedStorage);
		Messenger.RemoveListener<BurningSource>(Signals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
	}
	private void OnGameLoaded() {
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
		CheckIfFarmShouldBeTended(true);
		ScheduledCheckResource();
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
	private void OnObjectDamaged(IPointOfInterest poi, int amount) {
		Assert.IsTrue(poi is TileObject); // || poi is SpecialToken
		TileObject tileObject = poi as TileObject;
		if (poi.gridTileLocation != null && poi.gridTileLocation.IsPartOfSettlement(_owner) && tileObject.tileObjectType.CanBeRepaired()) {
			TryCreateRepairJob(poi);
		}
	}
	private void OnObjectFullyRepaired(IPointOfInterest poi) {
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
					if (_owner.mainStorage == resourcePile.structureLocation) {
						CheckResource(resourcePile.tileObjectType, resourcePile.providedResource);
						TryCreateCombineStockpile(resourcePile);	
					}
				}
				TryCreateHaulJob(resourcePile);	
			}
		}
	}
	private void OnTileObjectRemoved(TileObject tileObject, Character removedBy, LocationGridTile removedFrom) {
		if (tileObject is ResourcePile resourcePile) {
			if (removedFrom.parentMap.region == _owner.region && removedFrom.structure == _owner.mainStorage) {
				CheckResource(resourcePile.tileObjectType, resourcePile.providedResource);	
			}
		}
	}
	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
		if (structure.settlementLocation == _owner && structure.settlementLocation is NPCSettlement npcSettlement) {
			if (structure == npcSettlement.prison) {
				TryCreateJudgePrisoner(character);
			}
		}
	}
	private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
		if (traitable is Character target) {
			if (trait is Restrained) {
				TryCreateJudgePrisoner(target);
			} else if (trait is Criminal) {
				TryCreateApprehend(target);
			}
		} else if (traitable is TileObject) {
			if (traitable is GenericTileObject) {
				if (trait is Wet) {
					AddWetTile(traitable.gridTileLocation);
				} else if (trait is Poisoned) {
					AddPoisonedTile(traitable.gridTileLocation);
				}	
			}
		}
	}
	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character character) {
		if (traitable is TileObject) {
			if (traitable is GenericTileObject) {
				if (trait is Wet) {
					RemoveWetTile(traitable.gridTileLocation);
				} else if (trait is Poisoned) {
					RemovePoisonedTile(traitable.gridTileLocation);
				}	
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
	private void OnSettlementUnderSiegeChanged(NPCSettlement npcSettlement, bool isUnderSiege) {
		if (npcSettlement == _owner) {
			if (isUnderSiege) {
				TryCreateRestrainJobs();
			}	
		}
	}
	private void OnCharacterSaw(Character character, IPointOfInterest seenPOI) {
		if (character.homeSettlement == _owner && character.homeSettlement.isUnderSiege 
		    && character.currentSettlement == character.homeSettlement) {
			if (seenPOI is Character target && target.combatComponent.combatMode != COMBAT_MODE.Passive) {
				TryCreateRestrainJobs(target);
			}	
		}
	}
	// private void OnCharacterFinishedJobSuccessfully(Character character, GoapPlanJob goapPlanJob) {
	// 	if (goapPlanJob.originalOwner == _owner) {
	// 		if (goapPlanJob.jobType == JOB_TYPE.PRODUCE_FOOD || goapPlanJob.jobType == JOB_TYPE.PRODUCE_WOOD ||
	// 		    goapPlanJob.jobType == JOB_TYPE.PRODUCE_METAL || goapPlanJob.jobType == JOB_TYPE.PRODUCE_STONE) {
	// 			ResourcePile resourcePile = goapPlanJob.targetPOI as ResourcePile;
	// 			CheckResource(resourcePile.tileObjectType, resourcePile.providedResource);
	// 		}
	// 	}
	// }
	private void OnSettlementChangedStorage(NPCSettlement npcSettlement) {
		if (npcSettlement == _owner) {
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
	private void ScheduledCheckResource() {
		CheckAllResources();
		GameDate dueDate = GameManager.Instance.Today();
		dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
		SchedulingManager.Instance.AddEntry(dueDate, ScheduledCheckResource, this);
	}
	private void CheckAllResources() {
		CheckResource(TILE_OBJECT_TYPE.FOOD_PILE, RESOURCE.FOOD);
		CheckResource(TILE_OBJECT_TYPE.WOOD_PILE, RESOURCE.WOOD);
		CheckResource(TILE_OBJECT_TYPE.STONE_PILE, RESOURCE.STONE);
		CheckResource(TILE_OBJECT_TYPE.METAL_PILE, RESOURCE.METAL);
	}
	private void CheckResource(TILE_OBJECT_TYPE resourcePile, RESOURCE resource) {
		int totalResource = GetTotalResource(resource);
		int minimumResource = GetMinimumResource(resource);
		JOB_TYPE jobType = GetProduceResourceJobType(resource);
		if (totalResource < minimumResource) {
			TriggerProduceResource(resource, resourcePile, jobType);
		} else {
			ResourcePile pile = _owner.mainStorage.GetResourcePileObjectWithLowestCount(resourcePile, false);
			Assert.IsNotNull(pile, $"{_owner.name} is trying to cancel produce resource {resource.ToString()}, but could not find any pile of type {resourcePile.ToString()}");
			Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, jobType, pile as IPointOfInterest);
			Messenger.Broadcast(Signals.CHECK_UNBUILT_OBJECT_VALIDITY);
			// if (IsProduceResourceJobStillValid(resource) == false && pile.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
			// 	_owner.mainStorage.RemovePOI(pile); //remove unbuilt pile
			// }
		}
	}
	private void TriggerProduceResource(RESOURCE resourceType, TILE_OBJECT_TYPE resourcePile, JOB_TYPE jobType) {
		if (_owner.HasJob(jobType) == false) {
			ResourcePile targetPile = _owner.mainStorage.GetTileObjectOfType<ResourcePile>(resourcePile);
			if (targetPile == null) {
				//creation of job will be handled by OnTileObjectPlaced, when unbuilt object is placed.
				ResourcePile newPile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(resourcePile);
				_owner.mainStorage.AddPOI(newPile);
				newPile.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT, IsResourcePileStillValid);
			} else {
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
	}
	private bool IsProduceResourceJobStillValid(RESOURCE resource) {
		return GetTotalResource(resource) < GetMinimumResource(resource);
	}
	private bool IsResourcePileStillValid(BaseMapObject mapObject) {
		if (mapObject is ResourcePile resourcePile) {
			return IsProduceResourceJobStillValid(resourcePile.providedResource);
		}
		return false;
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
		//if target is in this npcSettlement and is not in the main storage, then create a haul job.
		//if target is not in this npcSettlement, check if it is in the wilderness, if it is, then create haul job
		bool isAtValidLocation = (target.gridTileLocation.IsPartOfSettlement(_owner) &&
		                          target.gridTileLocation.structure != _owner.mainStorage)
		                         || (target.gridTileLocation.IsPartOfSettlement(_owner) == false &&
		                             target.gridTileLocation.structure.isInterior == false);
		if (isAtValidLocation && _owner.HasJob(JOB_TYPE.HAUL, target) == false && target.gridTileLocation.parentMap.region == _owner.region) {
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
		    && target.currentStructure.settlementLocation is NPCSettlement
		    && target.currentStructure.settlementLocation == _owner) {
            NPCSettlement npcSettlement = target.currentStructure.settlementLocation as NPCSettlement;
            if(npcSettlement.prison == target.currentStructure) {
                if (!target.HasJobTargetingThis(JOB_TYPE.JUDGE_PRISONER)) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.JUDGE_CHARACTER, target, _owner);
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoJudgementJob);
                    job.SetStillApplicableChecker(() => InteractionManager.Instance.IsJudgementJobStillApplicable(target));
                    _owner.AddToAvailableJobs(job);
                }
            }
		}
	}
	#endregion

	#region Apprehend
	private void TryCreateApprehend(Character target) {
		if (target.currentSettlement == _owner && target.traitContainer.HasTrait("Criminal")) {
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
			stateJob.SetCannotBePushedBack(true);
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
        return goapPlanJob.targetPOI.gridTileLocation.structure.IsResident(character);
  //      if (goapPlanJob.targetPOI.gridTileLocation.structure is IDwelling) {
		//	IDwelling dwelling = goapPlanJob.targetPOI.gridTileLocation.structure as IDwelling;
		//	return dwelling.IsResident(character);
		//}
		//return false;
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
	private bool IsCombineStockpileStillApplicable(ResourcePile targetPile, ResourcePile pileToDeposit, NPCSettlement npcSettlement) {
		return targetPile.gridTileLocation != null
		       && targetPile.gridTileLocation.IsPartOfSettlement(npcSettlement)
		       && targetPile.structureLocation == npcSettlement.mainStorage
		       && pileToDeposit.gridTileLocation != null
		       && pileToDeposit.gridTileLocation.IsPartOfSettlement(npcSettlement)
		       && pileToDeposit.structureLocation == npcSettlement.mainStorage
		       && targetPile.HasEnoughSpaceFor(pileToDeposit.providedResource, pileToDeposit.resourceInPile);
	}
	#endregion

	#region Knockout
	private void TryCreateRestrainJobs() {
		string summary = $"{GameManager.Instance.TodayLogString()}{_owner.name} is under siege, trying to create knockout jobs...";
		if (CanCreateRestrainJob()) {
			int combatantResidents = 
				_owner.residents.Count(x => x.traitContainer.HasTrait("Combatant"));
			int existingRestrainJobs = _owner.GetNumberOfJobsWith(JOB_TYPE.RESTRAIN);
			summary += $"\nCombatant residents: {combatantResidents.ToString()}";
			summary += $"\nExisting restrain jobs: {existingRestrainJobs.ToString()}";
			List<Character> hostileCharacters = _owner.GetHostileCharactersInSettlement();
			if (hostileCharacters.Count > 0) {
				Character target = hostileCharacters.First();
				int jobsToCreate = combatantResidents - existingRestrainJobs;
				summary += $"\nWill create {jobsToCreate.ToString()} restrain jobs.";
				for (int i = 0; i < jobsToCreate; i++) {
					summary += $"\nWill create restrain job targeting {target.name}.";
					CreateRestrainJob(target);
				}	
			}
		} else {
			summary += $"\nCannot create restrain jobs";
		}
		Debug.Log(summary);
	}
	private void TryCreateRestrainJobs(Character target) {
		if (CanCreateRestrainJob() && target.faction.IsHostileWith(_owner.owner) && target.canPerform) {
			int combatantResidents = 
				_owner.residents.Count(x => x.traitContainer.HasTrait("Combatant"));
			int existingKnockoutJobs = _owner.GetNumberOfJobsWith(JOB_TYPE.RESTRAIN);
			int jobsToCreate = combatantResidents - existingKnockoutJobs;
			for (int i = 0; i < jobsToCreate; i++) {
				CreateRestrainJob(target);
			}	
		}
	}
	private bool CanCreateRestrainJob() {
		int combatantResidents = 
			_owner.residents.Count(x => x.traitContainer.HasTrait("Combatant"));
		int existingRestrainJobs = _owner.GetNumberOfJobsWith(JOB_TYPE.RESTRAIN);
		return existingRestrainJobs < combatantResidents;
	}
	private void CreateRestrainJob(Character target) {
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESTRAIN, 
			new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained",
				false, GOAP_EFFECT_TARGET.TARGET), 
			target, _owner);
		job.SetStillApplicableChecker(() => IsRestrainJobStillApplicable(target));
		job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeKnockoutJob);
		_owner.AddToAvailableJobs(job, 0);
	}
	private bool IsRestrainJobStillApplicable(Character target) {
		return target.traitContainer.HasTrait("Restrained") == false && target.gridTileLocation != null
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

	#region Dry Tiles
	private void AddWetTile(LocationGridTile tile) {
		if (tile.IsPartOfSettlement(_owner)) {
			if (wetTiles.Contains(tile) == false) {
				wetTiles.Add(tile);
			}	
		}
	}
	private void RemoveWetTile(LocationGridTile tile) {
		if (tile.IsPartOfSettlement(_owner)) {
			if (wetTiles.Remove(tile)) {
				CheckDryTilesValidity();
			}	
		}
	}
	public void TriggerDryTiles() {
		if (wetTiles.Count > 0) {
			int existingJobs = _owner.GetNumberOfJobsWith(CHARACTER_STATE.DRY_TILES);
			int jobsToCreate = 1;
			if (existingJobs < jobsToCreate) {
				int missing = jobsToCreate - existingJobs;
				for (int i = 0; i < missing; i++) {
					CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.DRY_TILES, 
						CHARACTER_STATE.DRY_TILES, _owner);
					_owner.AddToAvailableJobs(job);
				}	
			}	
		}
	}
	private void CheckDryTilesValidity() {
		if (wetTiles.Count == 0) {
			//cancel all dry tiles jobs
			List<JobQueueItem> jobs = _owner.GetJobs(JOB_TYPE.DRY_TILES);
			for (int i = 0; i < jobs.Count; i++) {
				JobQueueItem jqi = jobs[i];
				if (jqi.assignedCharacter == null) {
					jqi.ForceCancelJob(false, "no more wet floors");	
				}
			}
		}
	}
	#endregion
	
	#region Cleanse Tiles
	private void AddPoisonedTile(LocationGridTile tile) {
		if (tile.IsPartOfSettlement(_owner)) {
			if (poisonedTiles.Contains(tile) == false) {
				poisonedTiles.Add(tile);
			}	
		}
	}
	private void RemovePoisonedTile(LocationGridTile tile) {
		if (tile.IsPartOfSettlement(_owner)) {
			if (poisonedTiles.Remove(tile)) {
				CheckCleanseTilesValidity();
			}	
		}
	}
	public void TriggerCleanseTiles() {
		if (poisonedTiles.Count > 0) {
			int existingJobs = _owner.GetNumberOfJobsWith(CHARACTER_STATE.CLEANSE_TILES);
			int jobsToCreate = 1;
			if (existingJobs < jobsToCreate) {
				int missing = jobsToCreate - existingJobs;
				for (int i = 0; i < missing; i++) {
					CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.CLEANSE_TILES, 
						CHARACTER_STATE.CLEANSE_TILES, _owner);
					_owner.AddToAvailableJobs(job);
				}	
			}	
		}
	}
	private void CheckCleanseTilesValidity() {
		if (poisonedTiles.Count == 0) {
			//cancel all dry tiles jobs
			List<JobQueueItem> jobs = _owner.GetJobs(JOB_TYPE.CLEANSE_TILES);
			for (int i = 0; i < jobs.Count; i++) {
				JobQueueItem jqi = jobs[i];
				if (jqi.assignedCharacter == null) {
					jqi.ForceCancelJob(false, "no more poisoned floors");	
				}
			}
		}
	}
	#endregion

	#region Tend Farm
	private void ScheduleTendFarmCheck() {
		GameDate checkDate = GameManager.Instance.Today();
		checkDate.AddDays(1);
		checkDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(6));
		SchedulingManager.Instance.AddEntry(checkDate, () => CheckIfFarmShouldBeTended(true), this);
	}
	public void CheckIfFarmShouldBeTended(bool reschedule) {
		if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) > 16) {
			return; //already 4pm do not create tend job
		}
		List<LocationStructure> farms = _owner.GetStructuresOfType(STRUCTURE_TYPE.FARM);
		if (farms != null) {
			int untendedCornCrops = 0;
			for (int i = 0; i < farms.Count; i++) {
				LocationStructure farm = farms[i];
				List<CornCrop> cornCrops = farm.GetTileObjectsOfType<CornCrop>(TILE_OBJECT_TYPE.CORN_CROP);
				for (int j = 0; j < cornCrops.Count; j++) {
					CornCrop cornCrop = cornCrops[j];
					if (cornCrop.traitContainer.HasTrait("Tended") == false) {
						untendedCornCrops++;
						if (untendedCornCrops >= 3) {
							break;
						}
					}
				}
				if (untendedCornCrops >= 3) {
					break;
				}
			}
			if (untendedCornCrops >= 3) {
				CreateTendFarmJob();
			}	
		}
		if (reschedule) {
			//reschedule check for next day
			ScheduleTendFarmCheck();	
		}
	}
	private void CreateTendFarmJob() {
		GameDate expiry = GameManager.Instance.Today();
		expiry.SetTicks(GameManager.Instance.GetTicksBasedOnHour(21));
		SchedulingManager.Instance.AddEntry(expiry, CancelTendJobs, this);
		
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TEND_FARM, INTERACTION_TYPE.START_TEND, 
			null, _owner);
		_owner.AddToAvailableJobs(job);
	}
	private void CancelTendJobs() {
		List<JobQueueItem> jobs = _owner.GetJobs(JOB_TYPE.TEND_FARM);
		for (int i = 0; i < jobs.Count; i++) {
			JobQueueItem job = jobs[i];
			job.ForceCancelJob(false);
		}
	}
	#endregion
}
