using System;
using System.Collections.Generic;
using System.Linq;
using Goap.Job_Checkers;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Jobs;
using Locations;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using Locations.Settlements;
using Locations.Settlements.Components;
using UnityEngine.Profiling;
using UtilityScripts;
public class SettlementJobTriggerComponent : JobTriggerComponent/*, SettlementClassTracker.ISettlementTrackerListener*/, NPCSettlementEventDispatcher.ITileListener {

	private readonly NPCSettlement _owner;

	// public List<LocationGridTile> wetTiles { get; }
	public List<LocationGridTile> poisonedTiles { get; }
	public List<Character> poisonCleansers { get; }
	public List<Character> tileDryers { get; set; }
	public List<Character> dousers { get; }

	private Dictionary<SETTLEMENT_JOB_TRIGGER, SettlementJobTrigger> _jobTriggers;
	
	public SettlementJobTriggerComponent(NPCSettlement owner) {
		_owner = owner;
		// wetTiles = new List<LocationGridTile>();
		poisonedTiles = new List<LocationGridTile>();
		poisonCleansers = new List<Character>();
		tileDryers = new List<Character>();
		dousers = new List<Character>();
		_jobTriggers = new Dictionary<SETTLEMENT_JOB_TRIGGER, SettlementJobTrigger>();
	}

	#region Job Triggers
	public void AddJobTrigger(NPCSettlement p_settlement, SETTLEMENT_JOB_TRIGGER p_jobTriggerType) {
		if (!_jobTriggers.ContainsKey(p_jobTriggerType)) {
			SettlementJobTrigger jobTrigger = CreateSettlementJobTrigger<SettlementJobTrigger>(p_jobTriggerType);
			_jobTriggers.Add(p_jobTriggerType, jobTrigger);
			jobTrigger.HookTriggerToSettlement(p_settlement);
		}
	}
	public void RemoveJobTrigger(NPCSettlement p_settlement, SETTLEMENT_JOB_TRIGGER p_jobTriggerType) {
		if (_jobTriggers.ContainsKey(p_jobTriggerType)) {
			SettlementJobTrigger jobTrigger = _jobTriggers[p_jobTriggerType];
			jobTrigger.UnhookTriggerToSettlement(p_settlement);
			_jobTriggers.Remove(p_jobTriggerType);
		}
	}
	private T CreateSettlementJobTrigger<T>(SETTLEMENT_JOB_TRIGGER p_jobTriggerType) where T : SettlementJobTrigger {
		string typeName = $"Jobs.{p_jobTriggerType.ToString()}_Job_Trigger, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
		Type type = Type.GetType(typeName);
		Assert.IsNotNull(type, $"type for {p_jobTriggerType.ToString()} is null!");
		return Activator.CreateInstance(type) as T;
	} 
	#endregion

	#region Listeners
	public void SubscribeToVillageListeners() {
		Messenger.AddListener(Signals.HOUR_STARTED, HourlyJobActions);
		// Messenger.AddListener<ResourcePile>(TileObjectSignals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChangedVillage);
		Messenger.AddListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, OnTileObjectDamaged);
		Messenger.AddListener<TileObject>(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, OnTileObjectFullyRepaired);
		Messenger.AddListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		// Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		//Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
		Messenger.AddListener<Table>(StructureSignals.FOOD_IN_DWELLING_CHANGED, OnFoodInDwellingChanged);
		Messenger.AddListener<NPCSettlement, bool>(SettlementSignals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.AddListener<Character, IPointOfInterest>(CharacterSignals.CHARACTER_SAW, OnCharacterSaw);
		// Messenger.AddListener<NPCSettlement>(SettlementSignals.SETTLEMENT_CHANGE_STORAGE, OnSettlementChangedStorage);
		Messenger.AddListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
		Messenger.AddListener(Signals.GAME_LOADED, OnGameLoadedVillage);
		Messenger.AddListener<BaseSettlement>(CharacterSignals.TRY_CREATE_BURY_JOBS, TryCreateBuryJobs);

		_owner.npcSettlementEventDispatcher.SubscribeToTileRemovedEvent(this);
	}
	public void UnsubscribeFromVillageListeners() {
		Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyJobActions);
		// Messenger.RemoveListener<ResourcePile>(TileObjectSignals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChangedVillage);
		Messenger.RemoveListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, OnTileObjectDamaged);
		Messenger.RemoveListener<TileObject>(TileObjectSignals.TILE_OBJECT_FULLY_REPAIRED, OnTileObjectFullyRepaired);
		Messenger.RemoveListener<TileObject, LocationGridTile>(GridTileSignals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		// Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
		Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.RemoveListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.RemoveListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		//Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
		Messenger.RemoveListener<Table>(StructureSignals.FOOD_IN_DWELLING_CHANGED, OnFoodInDwellingChanged);
		Messenger.RemoveListener<NPCSettlement, bool>(SettlementSignals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		// Messenger.RemoveListener<NPCSettlement>(SettlementSignals.SETTLEMENT_CHANGE_STORAGE, OnSettlementChangedStorage);
		Messenger.RemoveListener<BurningSource>(InnerMapSignals.BURNING_SOURCE_INACTIVE, OnBurningSourceInactive);
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoadedVillage);
        Messenger.RemoveListener<BaseSettlement>(CharacterSignals.TRY_CREATE_BURY_JOBS, TryCreateBuryJobs);
        _owner.npcSettlementEventDispatcher.UnsubscribeToTileRemovedEvent(this);
    }

	private void TryCreateBuryJobs(BaseSettlement p_settlement) {
		if (p_settlement == _owner) {
			for (int i = 0; i < _owner.areas.Count; i++) {
				Area area = _owner.areas[i];
				for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++) {
					Character character = area.locationCharacterTracker.charactersAtLocation[j];
					if (character.isDead) {
						character.jobComponent.TriggerBuryMe();
					}
				}
			}
		}
	}

	public void SubscribeToDungeonListeners() {
        // Messenger.AddListener<ResourcePile>(TileObjectSignals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChangedDungeon);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoadedDungeon);
    }
    public void UnsubscribeFromDungeonListeners() {
        // Messenger.RemoveListener<ResourcePile>(TileObjectSignals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChangedDungeon);
    }
    //public void HookToSettlementClassTrackerEvents(SettlementClassTracker p_classTracker) {
	   // p_classTracker.SubscribeToNeededClassRemoved(this);
    //}
    //public void UnHookToSettlementClassTrackerEvents(SettlementClassTracker p_classTracker) {
	   // p_classTracker.UnsubscribeToNeededClassRemoved(this);
    //}
    private void OnGameLoadedVillage() {
		Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoadedVillage);
		if (SaveManager.Instance.useSaveData) {
			//LoadTendFarmCheck();
			// LoadCheckResource();
		} else {
			KickstartJobs();
		}
	}
    private void OnGameLoadedDungeon() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoadedDungeon);
        // if (SaveManager.Instance.useSaveData) {
        //     LoadCheckFoodPile();
        // } else {
        //     ScheduledCheckFoodPile();
        // }
    }
    public void KickstartJobs() {
		//CheckIfFarmShouldBeTended(true);
		// ScheduledCheckResource();
		// TryCreateMiningJob();
	}
	private void HourlyJobActions() {
#if DEBUG_PROFILER
		Profiler.BeginSample($"{_owner.name} settlement Hourly Job Actions");
#endif
		//No more patrol jobs since patrol is now party behaviour
		//CreatePatrolJobs();
		// TryCreateMiningJob();

		//HourlyCheckForNeededCharacterClasses();
		// TryCreateMissingFoodProducingStructure();
#if DEBUG_PROFILER
		Profiler.EndSample();
#endif
	}
	// private void OnResourceInPileChangedVillage(ResourcePile resourcePile) {
	// 	if (resourcePile.gridTileLocation != null && resourcePile.structureLocation == _owner.mainStorage) {
	// 		if (resourcePile.providedResource == RESOURCE.FOOD || resourcePile.providedResource == RESOURCE.WOOD || resourcePile.providedResource == RESOURCE.STONE) {
	// 			CheckResource(resourcePile.providedResource);
	// 		}
	// 		//Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.COMBINE_STOCKPILE, resourcePile as IPointOfInterest);
	// 		//TryCreateCombineStockpile(resourcePile);
	// 	}
	// }
 //    private void OnResourceInPileChangedDungeon(ResourcePile resourcePile) {
 //        if(resourcePile is FoodPile) {
 //            if (resourcePile.gridTileLocation != null && resourcePile.structureLocation == _owner.mainStorage) {
 //                CheckFoodPile();
 //            }
 //        }
 //    }
    private void OnTileObjectDamaged(TileObject tileObject, int amount, bool isPlayerSource) {
		if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.IsPartOfSettlement(_owner) && tileObject.tileObjectType.CanBeRepaired()) {
			TryCreateRepairTileObjectJob(tileObject);
		}
	}
	private void OnTileObjectFullyRepaired(TileObject tileObject) {
		if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.IsPartOfSettlement(_owner)) {
			//cancel existing repair job
			Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.REPAIR, tileObject as IPointOfInterest);
		}
	}
	private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
		if (tileObject is ResourcePile resourcePile) {
			if (resourcePile.resourceInPile > 0) {
				Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.HAUL, resourcePile as IPointOfInterest);
				//Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, JOB_TYPE.COMBINE_STOCKPILE, resourcePile as IPointOfInterest);
				// if (tile.IsPartOfSettlement(_owner)) {
				// 	if (_owner.mainStorage == resourcePile.structureLocation) {
				// 		if (resourcePile.providedResource == RESOURCE.FOOD || resourcePile.providedResource == RESOURCE.WOOD || resourcePile.providedResource == RESOURCE.STONE) {
				// 			CheckResource(resourcePile.providedResource);
				// 		}
				// 		//TryCreateCombineStockpile(resourcePile);	
				// 	}
				// }
				// TryCreateHaulJob(resourcePile);	
			}
		}
	}
	// private void OnTileObjectRemoved(TileObject tileObject, Character removedBy, LocationGridTile removedFrom) {
	// 	if (tileObject is ResourcePile resourcePile) {
	// 		if (removedFrom.parentMap.region == _owner.region && removedFrom.structure == _owner.mainStorage) {
	// 			if (resourcePile.providedResource == RESOURCE.FOOD || resourcePile.providedResource == RESOURCE.WOOD || resourcePile.providedResource == RESOURCE.STONE) {
	// 				CheckResource(resourcePile.providedResource);
	// 			}
	// 		}
	// 	}
	// }
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
			} 
   //         else if (trait is Criminal) {
			//	TryCreateApprehend(target);
			//}
		} else if (traitable is TileObject) {
			if (traitable is GenericTileObject && traitable.gridTileLocation.IsPartOfSettlement(_owner)) {
				// if (trait is Wet) {
				// 	AddWetTile(traitable.gridTileLocation);
				// } else 
				if (trait is Poisoned) {
					AddPoisonedTile(traitable.gridTileLocation);
				}	
			}
		}
	}
	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character character) {
		if (traitable is TileObject) {
			if (traitable is GenericTileObject && traitable.gridTileLocation.IsPartOfSettlement(_owner)) {
				// if (trait is Wet) {
				// 	RemoveWetTile(traitable.gridTileLocation);
				// } else 
				if (trait is Poisoned) {
					RemovePoisonedTile(traitable.gridTileLocation);
				}	
			}
			
		}
	}
	private void OnCharacterEnteredArea(Character character, Area p_area) {
        //Note: No more apprehension in settlement when criminal enters the settlement hex tiles
        //Now, creation of apprehend job towards criminal in settlement job queue is done in ReactionComponent, so this means, it is only added in settlement job queue when another character sees the criminal
        //The reason for this is due to a bug in burn at stake that when a criminal is brought outside to be burnt, when the criminal is dropped this will create another apprehend job in settlement even though the criminal is already being burnt at stake

		//if (_owner.tiles.Contains(tile)) {
		//	TryCreateApprehend(character);
		//}
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
                if(target.reactionComponent.disguisedCharacter != null) {
                    target = target.reactionComponent.disguisedCharacter;
                }
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
	// private void OnSettlementChangedStorage(NPCSettlement npcSettlement) {
	// 	if (npcSettlement == _owner) {
	// 		List<TileObject> resourcePiles = RuinarchListPool<TileObject>.Claim();
	// 		_owner.region.PopulateTileObjectsOfType<ResourcePile>(resourcePiles);
	// 		for (int i = 0; i < resourcePiles.Count; i++) {
	// 			ResourcePile resourcePile = resourcePiles[i] as ResourcePile;
	// 			TryCreateHaulJob(resourcePile);
	// 		}
	// 		RuinarchListPool<TileObject>.Release(resourcePiles);
	// 	}
	// }
	private void OnBurningSourceInactive(BurningSource burningSource) {
		// if (burningSource.location == _owner.region) {
			CheckDouseFireJobsValidity();
		// }
	}
	public void OnItemRemovedFromStructure(TileObject item, LocationStructure structure, LocationGridTile removedFrom) {
		if (structure is CityCenter && item is WaterWell) {
			//immediately create a new unbuilt water well at tile to reserve it.
			TileObject waterWell = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_WELL);
			structure.AddPOI(waterWell, removedFrom);
			waterWell.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
			//create craft water well job any time water well is destroyed.
			StartCraftWaterWellCheck();
		}
	}
	public void OnItemAddedToStructure(TileObject item, LocationStructure structure) {
		if (structure is CityCenter && item is WaterWell) {
			CheckIfShouldStopWaterWellCheck();
		}
	}
	public void OnSettlementAreaRemoved(Area p_area, NPCSettlement p_settlement) {
		for (int i = 0; i < poisonedTiles.Count; i++) {
			LocationGridTile tile = poisonedTiles[i];
			if (p_area.gridTileComponent.gridTiles.Contains(tile)) {
				RemovePoisonedTile(tile);
				i--;
			}
		}
		// for (int i = 0; i < wetTiles.Count; i++) {
		// 	LocationGridTile tile = wetTiles[i];
		// 	if (p_area.gridTileComponent.gridTiles.Contains(tile)) {
		// 		RemoveWetTile(tile);
		// 		i--;
		// 	}
		// }
	}
#endregion

	//#region Utilities
	//private bool HasNearbyCave() {
	//	List<HexTile> nearbyTiles = new List<HexTile>();
	//	for (int i = 0; i < _owner.tiles.Count; i++) {
	//		HexTile tile = _owner.tiles[i];
	//		nearbyTiles.AddRange(tile.GetTilesInRange(2));
	//	}
	//	for (int j = 0; j < nearbyTiles.Count; j++) {
	//		HexTile neighbour = nearbyTiles[j];
	//		if (neighbour.elevationType == ELEVATION.MOUNTAIN) {
	//			return true;
	//		}
	//	}
	//	return false;
	//}
	//#endregion

	#region Resources
	public bool HasTotalResource(RESOURCE resourceType, int neededResource) {
		int resource = 0;
		List<TileObject> piles = RuinarchListPool<TileObject>.Claim();
		_owner.mainStorage.PopulateBuiltTileObjectsOfType<ResourcePile>(piles);
		for (int i = 0; i < piles.Count; i++) {
			ResourcePile resourcePile = piles[i] as ResourcePile;
			if (resourcePile.providedResource == resourceType) {
				if (resourcePile.resourceInPile >= neededResource) {
					RuinarchListPool<TileObject>.Release(piles);
					return true;
				}
			}
		}
		RuinarchListPool<TileObject>.Release(piles);

		List<LocationStructure> lumberyards = _owner.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
		if (lumberyards != null) {
			for (int i = 0; i < lumberyards.Count; i++) {
				LocationStructure lumberyard = lumberyards[i];
				piles = RuinarchListPool<TileObject>.Claim();
				lumberyard.PopulateBuiltTileObjectsOfType<ResourcePile>(piles);
				for (int j = 0; j < piles.Count; j++) {
					ResourcePile resourcePile = piles[j] as ResourcePile;
					if (resourcePile.resourceInPile >= neededResource) {
						RuinarchListPool<TileObject>.Release(piles);
						return true;
					}
				}
				RuinarchListPool<TileObject>.Release(piles);
			}	
		}
		
		List<LocationStructure> mines = _owner.GetStructuresOfType(STRUCTURE_TYPE.MINE);
		if (mines != null) {
			for (int i = 0; i < mines.Count; i++) {
				LocationStructure mine = mines[i];
				piles = RuinarchListPool<TileObject>.Claim();
				mine.PopulateBuiltTileObjectsOfType<ResourcePile>(piles);
				for (int j = 0; j < piles.Count; j++) {
					ResourcePile resourcePile = piles[j] as ResourcePile;
					if (resourcePile.resourceInPile >= neededResource) {
						RuinarchListPool<TileObject>.Release(piles);
						return true;
					}
				}
				RuinarchListPool<TileObject>.Release(piles);
			}	
		}
		return false;
	}
	public bool HasAccessToResource(RESOURCE p_resource) {
		switch (p_resource) {
			case RESOURCE.STONE:
				return _owner.HasStructure(STRUCTURE_TYPE.MINE);
			case RESOURCE.WOOD:
				return _owner.HasStructure(STRUCTURE_TYPE.LUMBERYARD);
			case RESOURCE.FOOD:
				return _owner.HasFoodProducingStructure();
			default:
				return false;
		}
	}
	// private int GetMinimumResource(RESOURCE resource) {
	// 	switch (resource) {
	// 		case RESOURCE.FOOD:
	// 			return ProduceResourceApplicabilityChecker.MinimumFood;
	// 		case RESOURCE.WOOD:
	// 			return ProduceResourceApplicabilityChecker.MinimumWood;
	// 		case RESOURCE.METAL:
	// 			return ProduceResourceApplicabilityChecker.MinimumMetal;
	// 		case RESOURCE.STONE:
	// 			return ProduceResourceApplicabilityChecker.MinimumStone;
	// 	}
	// 	throw new Exception($"There is no minimum resource for {resource.ToString()}");
	// }
	// private JOB_TYPE GetProduceResourceJobType(RESOURCE resource) {
	// 	switch (resource) {
	// 		case RESOURCE.FOOD:
	// 			return JOB_TYPE.PRODUCE_FOOD;
	// 		case RESOURCE.WOOD:
	// 			return JOB_TYPE.PRODUCE_WOOD;
	// 		case RESOURCE.METAL:
	// 			return JOB_TYPE.PRODUCE_METAL;
	// 		case RESOURCE.STONE:
	// 			return JOB_TYPE.PRODUCE_STONE;
	// 	}
	// 	throw new Exception($"There is no produce resource job type for {resource.ToString()}");
	// }
	// private GOAP_EFFECT_CONDITION GetProduceResourceGoapEffect(RESOURCE resource) {
	// 	switch (resource) {
	// 		case RESOURCE.FOOD:
	// 			return GOAP_EFFECT_CONDITION.PRODUCE_FOOD;
	// 		case RESOURCE.WOOD:
	// 			return GOAP_EFFECT_CONDITION.PRODUCE_WOOD;
	// 		case RESOURCE.METAL:
	// 			return GOAP_EFFECT_CONDITION.PRODUCE_METAL;
	// 		case RESOURCE.STONE:
	// 			return GOAP_EFFECT_CONDITION.PRODUCE_STONE;
	// 	}
	// 	throw new Exception($"There is no produce resource goap effect type for {resource.ToString()}");
	// }
	// private void ScheduledCheckResource() {
	// 	CheckAllResources();
	// 	GameDate dueDate = GameManager.Instance.Today();
	// 	dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
	// 	SchedulingManager.Instance.AddEntry(dueDate, ScheduledCheckResource, this);
	// }
 //    private void ScheduledCheckFoodPile() {
 //        CheckFoodPile();
 //        GameDate dueDate = GameManager.Instance.Today();
 //        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
 //        SchedulingManager.Instance.AddEntry(dueDate, ScheduledCheckFoodPile, this);
 //    }
 //    private void LoadCheckResource() {
	// 	if (GameManager.Instance.Today().tick < GameManager.Instance.GetTicksBasedOnHour(20)) {
	// 		//schedule check at 8pm
	// 		GameDate dueDate = GameManager.Instance.Today();
	// 		dueDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(20));
	// 		SchedulingManager.Instance.AddEntry(dueDate, ScheduledCheckResource, this);
	// 	} else {
	// 		//schedule check at 8am the next day
	// 		GameDate dueDate = GameManager.Instance.Today();
	// 		dueDate.AddDays(1);
	// 		dueDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(12));
	// 		SchedulingManager.Instance.AddEntry(dueDate, ScheduledCheckResource, this);	
	// 	}
	// }
 //    private void LoadCheckFoodPile() {
 //        if (GameManager.Instance.Today().tick < GameManager.Instance.GetTicksBasedOnHour(20)) {
 //            //schedule check at 8pm
 //            GameDate dueDate = GameManager.Instance.Today();
 //            dueDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(20));
 //            SchedulingManager.Instance.AddEntry(dueDate, ScheduledCheckFoodPile, this);
 //        } else {
 //            //schedule check at 8am the next day
 //            GameDate dueDate = GameManager.Instance.Today();
 //            dueDate.AddDays(1);
 //            dueDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(12));
 //            SchedulingManager.Instance.AddEntry(dueDate, ScheduledCheckFoodPile, this);
 //        }
 //    }
 //    private void CheckFoodPile() {
 //        if(_owner.owner?.factionType.type == FACTION_TYPE.Ratmen) {
 //            CheckResource(RESOURCE.FOOD);
 //        }
 //    }
 //    private void CheckAllResources() {
	// 	CheckResource(RESOURCE.FOOD);
	// 	CheckResource(RESOURCE.WOOD);
	// 	CheckResource(RESOURCE.STONE);
	// }
	// private void CheckResource(RESOURCE resource) {
	// 	switch (resource) {
	// 		case RESOURCE.FOOD:
	// 			CheckResource<FoodPile>(resource);
	// 			break;
	// 		case RESOURCE.WOOD:
	// 			CheckResource<WoodPile>(resource);
	// 			break;
	// 		case RESOURCE.STONE:
	// 			CheckResource<StonePile>(resource);
	// 			break;
	// 	}
	// }
	// private void CheckResource<T>(RESOURCE resource) where T : ResourcePile{
	// 	int totalResource = GetTotalResource(resource);
	// 	int minimumResource = GetMinimumResource(resource);
	// 	JOB_TYPE jobType = GetProduceResourceJobType(resource);
	// 	if (totalResource < minimumResource) {
	// 		TriggerProduceResource<T>(resource, jobType);
	// 	} else {
	// 		ResourcePile pile = _owner.mainStorage.GetResourcePileObjectWithLowestCount<T>(false);
	// 		Assert.IsNotNull(pile, $"{_owner.name} is trying to cancel produce resource {resource.ToString()}, but could not find any pile that produces {resource.ToString()}");
	// 		Messenger.Broadcast(JobSignals.CHECK_JOB_APPLICABILITY, jobType, pile as IPointOfInterest);
	// 		Messenger.Broadcast(TileObjectSignals.CHECK_UNBUILT_OBJECT_VALIDITY);
	// 	}
	// }
	// private void TriggerProduceResource<T>(RESOURCE resourceType, JOB_TYPE jobType) where T : ResourcePile {
	// 	if (_owner.HasJob(jobType) == false && _owner.HasStructureForProducingResource(resourceType)) {
	// 		ResourcePile targetPile = _owner.mainStorage.GetTileObjectOfType<T>();
	// 		if (targetPile == null) {
	// 			TILE_OBJECT_TYPE tileObjectType;
	// 			switch (resourceType) {
	// 				case RESOURCE.FOOD:
	// 					tileObjectType = TILE_OBJECT_TYPE.ANIMAL_MEAT;
	// 					break;
	// 				case RESOURCE.WOOD:
	// 					tileObjectType = TILE_OBJECT_TYPE.WOOD_PILE;
	// 					break;
	// 				case RESOURCE.STONE:
	// 					tileObjectType = TILE_OBJECT_TYPE.STONE_PILE;
	// 					break;
	// 				default:
	// 					throw new Exception($"There was no tile object type found for resource {resourceType.ToString()}");
	// 			}
	// 			ResourcePile newPile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(tileObjectType);
	// 			_owner.mainStorage.AddPOI(newPile);
	// 			newPile.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
	// 			targetPile = newPile;
	// 		}
 //            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType,
 //                new GoapEffect(GetProduceResourceGoapEffect(resourceType), string.Empty, false, GOAP_EFFECT_TARGET.ACTOR),
 //                targetPile, _owner);
 //            UtilityScripts.JobUtilities.PopulatePriorityLocationsForProduceResources(_owner, job, resourceType);
 //            job.SetStillApplicableChecker(JobManager.Produce_Resource_Applicability);
 //            _owner.AddToAvailableJobs(job);
 //        }
	// }
	#endregion

#region Repair
	private void TryCreateRepairTileObjectJob(TileObject target) {
		if (_owner.HasJob(JOB_TYPE.REPAIR, target) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR, target, _owner);
			job.SetCanTakeThisJobChecker(JobManager.Can_Take_Repair);
			job.SetStillApplicableChecker(JobManager.Repair_Applicability);
            UtilityScripts.JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(_owner, job, INTERACTION_TYPE.TAKE_RESOURCE);
            if (target is TileObject tileObject) {
				job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] {TileObjectDB.GetTileObjectData(tileObject.tileObjectType).repairCost});	
			}
			_owner.AddToAvailableJobs(job);
		}
	}
#endregion

#region Haul
	public void TryCreateHaulJob(ResourcePile target) {
		if (_owner.HasJob(JOB_TYPE.HAUL, target) == false && target.gridTileLocation.parentMap.region == _owner.region) {

            //Exclude Haul resource if pile is in settlement owned by major faction or ratmen faction
            if(target.gridTileLocation.IsPartOfSettlement(out var settlement) && _owner != settlement && settlement.owner != null && 
               (settlement.owner.isMajorNonPlayer || settlement.owner.factionType.type == FACTION_TYPE.Ratmen)) {
                return;
            }
            
			ResourcePile chosenPileToDepositTo = _owner.mainStorage.GetResourcePileObjectWithLowestCount(target.tileObjectType);
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, 
				new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
			if (chosenPileToDepositTo != null) {
			    job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[] { chosenPileToDepositTo });
			}
			job.SetStillApplicableChecker(JobManager.Haul_Applicability);
			job.SetCanTakeThisJobChecker(JobManager.Can_Take_Haul);
			_owner.AddToAvailableJobs(job);
		}
	}
    public void TryCreateHaulJobForItems(TileObject target, LocationStructure dropLocation) {
        if (_owner.HasJob(JOB_TYPE.HAUL, target) == false) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, INTERACTION_TYPE.DROP_ITEM, target, _owner);
            job.AddOtherData(INTERACTION_TYPE.DROP_ITEM, new object[] { dropLocation });
            job.SetStillApplicableChecker(JobManager.Haul_Applicability);
            job.SetCanTakeThisJobChecker(JobManager.Can_Take_Haul);
            _owner.AddToAvailableJobs(job);
        }
    }
#endregion

#region Judge Prisoner
    public void TryCreateJudgePrisoner(Character target) {
		if (target.traitContainer.HasTrait("Restrained") && target.traitContainer.HasTrait("Criminal")
            && target.gridTileLocation != null
		    && target.gridTileLocation.IsPartOfSettlement(_owner)
            && _owner.owner != null) {
            NPCSettlement npcSettlement = _owner as NPCSettlement;
            if(npcSettlement != null && npcSettlement.prison == target.currentStructure && !npcSettlement.HasJob(JOB_TYPE.JUDGE_PRISONER, target)) {
                if (!target.HasJobTargetingThis(JOB_TYPE.JUDGE_PRISONER)) {
                    if (target.crimeComponent.IsWantedBy(_owner.owner)) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.JUDGE_CHARACTER, target, _owner);
                        job.SetCanTakeThisJobChecker(JobManager.Can_Take_Judgement);
                        job.SetStillApplicableChecker(JobManager.Judge_Applicability);
                        _owner.AddToAvailableJobs(job);
                    }
                }
            }
		}
	}
#endregion

#region Apprehend
	public void TryCreateApprehend(Character target) {
		if (target.currentSettlement == _owner && _owner.owner != null && target.traitContainer.HasTrait("Criminal") && !target.isDead && target.currentStructure != _owner.prison) {
			if (_owner.HasJob(JOB_TYPE.APPREHEND, target) == false) {
                if (target.crimeComponent.IsWantedBy(_owner.owner)) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP_RESTRAINED, target, _owner);
                    job.SetCanTakeThisJobChecker(JobManager.Can_Take_Apprehend);
                    job.SetStillApplicableChecker(JobManager.Apprehend_Settlement_Applicability);
                    job.SetShouldBeRemovedFromSettlementWhenUnassigned(true);
                    job.SetDoNotRecalculate(true);
                    job.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[] { _owner.prison });
                    _owner.AddToAvailableJobs(job);
                }
			}
		}
	}
#endregion

#region Patrol
	private void CreatePatrolJobs() {
		int patrolChance = UnityEngine.Random.Range(0, 100);
		if (patrolChance < 15 && _owner.GetNumberOfJobsWith(JOB_TYPE.PATROL) < 2) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PATROL, INTERACTION_TYPE.START_PATROL, null, _owner);
			job.SetCannotBePushedBack(true);
			_owner.AddToAvailableJobs(job);
		}
	}
#endregion

#region Obtain Personal Food
	private void TryTriggerObtainPersonalFood(Table table) {
		if (table.food < 20 && _owner.HasJob(JOB_TYPE.OBTAIN_PERSONAL_FOOD, table) == false) {
			//Only get the amount of food that is missing from 30
			int neededFood = 30 - table.food; 
			GoapEffect goapEffect = new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile", false, GOAP_EFFECT_TARGET.TARGET);
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.OBTAIN_PERSONAL_FOOD, goapEffect, table, _owner);
			job.SetCanTakeThisJobChecker(JobManager.Can_Take_Obtain_Personal_Food);
			job.SetStillApplicableChecker(JobManager.Obtain_Personal_Food_Applicability);
			job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { neededFood });
            UtilityScripts.JobUtilities.PopulatePriorityLocationsForTakingPersonalItem(_owner, job, INTERACTION_TYPE.TAKE_RESOURCE);
            _owner.AddToAvailableJobs(job);
		}
	}
	private bool CanTakeObtainPersonalFoodJob(Character character, JobQueueItem job) {
		GoapPlanJob goapPlanJob = job as GoapPlanJob;
        if(goapPlanJob.targetPOI != null && goapPlanJob.targetPOI.gridTileLocation != null) {
            return goapPlanJob.targetPOI.gridTileLocation.structure.IsResident(character);
        } else if (goapPlanJob.targetPOI != null && goapPlanJob.targetPOI.gridTileLocation != null && goapPlanJob.targetPOI is TileObject targetTileObject) {
            if (targetTileObject.IsOwnedBy(character)) {
                return true;
            }
        }
        return false;
        //      if (goapPlanJob.targetPOI.gridTileLocation.structure is IDwelling) {
        //	IDwelling dwelling = goapPlanJob.targetPOI.gridTileLocation.structure as IDwelling;
        //	return dwelling.IsResident(character);
        //}
        //return false;
    }
#endregion

#region Combine Stockpile
	private void TryCreateCombineStockpile(ResourcePile pile) {
		if (pile.mapObjectState != MAP_OBJECT_STATE.BUILT) {
			return;
		}
		if (pile.resourceStorageComponent.IsAtMaxResource(pile.providedResource)) {
			return; //if given pile is at maximum capacity, then do not create combine job for it
		}
		if (_owner.HasJob(JOB_TYPE.COMBINE_STOCKPILE, pile)) {
			return; //already has job to combine stockpile.
		}
		//get all resource piles inside the main storage, then check if iny of them are not at max capacity,
		//if not at max capacity, check if the pile can handle the resources of the new pile,
		//if it can, then create combine job
		List<TileObject> resourcePiles = _owner.mainStorage.GetTileObjectsOfType(pile.tileObjectType);
		ResourcePile targetPile = null;
        if(resourcePiles != null) {
            for (int i = 0; i < resourcePiles.Count; i++) {
                ResourcePile currPile = resourcePiles[i] as ResourcePile;
                if (currPile != pile && currPile.mapObjectState == MAP_OBJECT_STATE.BUILT && 
                    currPile.resourceStorageComponent.IsAtMaxResource(pile.providedResource) == false
                    && currPile.resourceStorageComponent.HasEnoughSpaceFor(pile.providedResource, pile.resourceInPile)) {
                    targetPile = currPile;
                    break;
                }
            }
        }
		if (targetPile != null && _owner.HasJob(JOB_TYPE.COMBINE_STOCKPILE, targetPile) == false) { //only create job if chosen target pile does not already have a job to combine it with another pile
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.COMBINE_STOCKPILE, 
				INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, pile, _owner);
			job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[] { targetPile });
			job.SetStillApplicableChecker(JobManager.Combine_Stockpile_Applicability);
			_owner.AddToAvailableJobs(job);
		}
	}
#endregion

#region Knockout
	private void TryCreateRestrainJobs() {
#if DEBUG_LOG
		string summary = $"{GameManager.Instance.TodayLogString()}{_owner.name} is under siege, trying to create knockout jobs...";
#endif
		if (CanCreateRestrainJob()) {
			int combatantResidents = _owner.GetNumberOfResidentsThatHasTrait("Combatant");
			int existingRestrainJobs = _owner.GetNumberOfJobsWith(JOB_TYPE.RESTRAIN);
#if DEBUG_LOG
			summary += $"\nCombatant residents: {combatantResidents.ToString()}";
			summary += $"\nExisting restrain jobs: {existingRestrainJobs.ToString()}";
#endif
			Character hostile = _owner.GetFirstHostileCharacterInSettlement();
			if (hostile != null) {
				int jobsToCreate = combatantResidents - existingRestrainJobs;
#if DEBUG_LOG
				summary += $"\nWill create {jobsToCreate.ToString()} restrain jobs.";
#endif
				for (int i = 0; i < jobsToCreate; i++) {
#if DEBUG_LOG
					summary += $"\nWill create restrain job targeting {hostile.name}.";
#endif
					CreateRestrainJob(hostile);
				}	
			}
		} else {
#if DEBUG_LOG
			summary += $"\nCannot create restrain jobs";
#endif
		}
#if DEBUG_LOG
		Debug.Log(summary);
#endif
	}
	private void TryCreateRestrainJobs(Character target) {
		if (CanCreateRestrainJob() && target.faction.IsHostileWith(_owner.owner) && target.limiterComponent.canPerform && 
		    target.limiterComponent.canMove && !target.traitContainer.HasTrait("Restrained") && !target.traitContainer.HasTrait("Enslaved")) {
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
		job.SetStillApplicableChecker(JobManager.Restrain_Applicability);
		job.SetCanTakeThisJobChecker(JobManager.Can_Take_Restrain);
		job.SetShouldBeRemovedFromSettlementWhenUnassigned(true);
		job.SetDoNotRecalculate(true);
		// job.SetOnUnassignJobAction(OnUnassignRestrain);
		_owner.AddToAvailableJobs(job, 0);
	}
	// private void OnUnassignRestrain(Character character, JobQueueItem job) {
	// 	//force cancel restrain job when 
	// 	job.ForceCancelJob(false);
	// }
	// private bool IsRestrainJobStillApplicable(Character target) {
	// 	return target.traitContainer.HasTrait("Restrained") == false && target.gridTileLocation != null
	// 	    && target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(_owner);
	// }
	// private void OnUnassignRestrain(Character character, JobQueueItem job) {
	// 	job.ForceCancelJob(false); //automatically cancel job if assigned character drops the job
	// }
	private bool IsRestrainJobStillApplicable(Character target, GoapPlanJob job) {
        bool isApplicable = !target.traitContainer.HasTrait("Restrained"); //|| target.currentStructure != _owner.prison; //Removed check for structure must be in prison because restrain job only puts restrains on the target they dont actually carry them to the prison
		if (target.gridTileLocation != null && isApplicable) {
			if (target.gridTileLocation.IsPartOfSettlement(_owner)) {
				//if target is within settlement job is always valid
				return true;
			} else {
				//if target is no longer within settlement then check if job is already taken
				if (job.assignedCharacter != null) {
					//if job is taken, check if assigned character is in actual combat with the target (aka. is already fighting target and not just pursuing)
					return job.assignedCharacter.combatComponent.IsInActualCombatWith(target);
				} else {
					//if job is not yet taken, then it is invalid.
					return false;
				}
				// return job.assignedCharacter != null;
			}
			// return target.gridTileLocation != null && target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(_owner) && isApplicable;    
		}
		return false;
	}
#endregion

#region Douse Fire
	public void TriggerDouseFire() {
        if (_owner.firesInSettlement.Count > 0) {
			int existingDouseFire = dousers.Count + _owner.GetNumberOfJobsWith(JOB_TYPE.DOUSE_FIRE);
			int maxDouseFireJobs = 3;
			if (existingDouseFire < maxDouseFireJobs) {
				int missing = maxDouseFireJobs - existingDouseFire;
				for (int i = 0; i < missing; i++) {
					GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DOUSE_FIRE, INTERACTION_TYPE.START_DOUSE, null, _owner);
					job.SetCanTakeThisJobChecker(JobManager.Can_Take_Remove_Fire);
					_owner.AddToAvailableJobs(job, 0);
				}	
			}	
		}
	}
	private void CheckDouseFireJobsValidity() {
		if (_owner.firesInSettlement.Count == 0) {
			//cancel all douse fire jobs
			List<JobQueueItem> jobs = RuinarchListPool<JobQueueItem>.Claim();
			_owner.PopulateJobsOfType(jobs, JOB_TYPE.DOUSE_FIRE);
			for (int i = 0; i < jobs.Count; i++) {
				JobQueueItem jqi = jobs[i];
				if (jqi.assignedCharacter == null) {
					jqi.ForceCancelJob("no more fires");	
				}
			}
			RuinarchListPool<JobQueueItem>.Release(jobs);
		}
	}
	private bool CanTakeRemoveFireJob(Character character, IPointOfInterest target) {
		if (character.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE)) { return false; }
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
	public void OnTakeDouseFireJob(Character character) {
		character.behaviourComponent.SetDouseFireSettlement(_owner);
	}
	public void AddDouser(Character character) {
		dousers.Add(character);
	}
	public void RemoveDouser(Character character) {
		dousers.Remove(character);
	}
#endregion

// #region Dry Tiles
	// private void AddWetTile(LocationGridTile tile) {
	// 	if (!wetTiles.Contains(tile)) {
	// 		wetTiles.Add(tile);
	// 	}	
	// }
	// private void RemoveWetTile(LocationGridTile tile) {
	// 	if (wetTiles.Remove(tile)) {
	// 		CheckDryTilesValidity();
	// 	}	
	// }
	// public void TriggerDryTiles() {
	// 	if (wetTiles.Count > 0) {
	// 		int dryerCount = tileDryers.Count + _owner.GetNumberOfJobsWith(JOB_TYPE.DRY_TILES);
	// 		int maxDryers = 1;
	// 		if (dryerCount < maxDryers) {
	// 			int missing = maxDryers - dryerCount;
	// 			for (int i = 0; i < missing; i++) {
	// 				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DRY_TILES, INTERACTION_TYPE.START_DRY, null, _owner);
	// 				_owner.AddToAvailableJobs(job);
	// 			}	
	// 		}	
	// 	}
	// }
	// public void OnTakeDryTileJob(Character character) {
	// 	character.behaviourComponent.SetDryingTilesForSettlement(_owner);
	// }
	// private void CheckDryTilesValidity() {
	// 	if (wetTiles.Count == 0) {
	// 		//cancel all dry tiles jobs
	// 		List<JobQueueItem> jobs = RuinarchListPool<JobQueueItem>.Claim();
	// 		_owner.PopulateJobsOfType(jobs, JOB_TYPE.DRY_TILES);
	// 		for (int i = 0; i < jobs.Count; i++) {
	// 			JobQueueItem jqi = jobs[i];
	// 			if (jqi.assignedCharacter == null) {
	// 				jqi.ForceCancelJob("no more wet floors");
	// 			}
	// 		}
	// 		RuinarchListPool<JobQueueItem>.Release(jobs);
	// 	}
	// }
	// public void AddTileDryer(Character character) {
	// 	tileDryers.Add(character);
	// }
	// public void RemoveTileDryer(Character character) {
	// 	tileDryers.Remove(character);
	// }
// #endregion
	
#region Cleanse Tiles
	private void AddPoisonedTile(LocationGridTile tile) {
		if (!poisonedTiles.Contains(tile)) {
			poisonedTiles.Add(tile);
		}	
	}
	private void RemovePoisonedTile(LocationGridTile tile) {
		if (poisonedTiles.Remove(tile)) {
			CheckCleanseTilesValidity();
		}	
	}
	public void TriggerCleanseTiles() {
		if (poisonedTiles.Count > 0) {
			int cleansersCount = poisonCleansers.Count + _owner.GetNumberOfJobsWith(JOB_TYPE.CLEANSE_TILES);
			int maxCleansers = 1;
			if (cleansersCount < maxCleansers) {
				int missing = maxCleansers - cleansersCount;
				for (int i = 0; i < missing; i++) {
					GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLEANSE_TILES, INTERACTION_TYPE.START_CLEANSE, null, _owner);
					_owner.AddToAvailableJobs(job);
				}	
			}	
		}
	}
	public void OnTakeCleanseTileJob(Character character) {
		character.behaviourComponent.SetCleansingTilesForSettlement(_owner);
	}
	private void CheckCleanseTilesValidity() {
		if (poisonedTiles.Count == 0) {
			//cancel all cleanse tiles jobs
			List<JobQueueItem> jobs = RuinarchListPool<JobQueueItem>.Claim();
			_owner.PopulateJobsOfType(jobs, JOB_TYPE.CLEANSE_TILES);
			for (int i = 0; i < jobs.Count; i++) {
				JobQueueItem jqi = jobs[i];
				if (jqi.assignedCharacter == null) {
					jqi.ForceCancelJob("no more poisoned floors");	
				}
			}
			RuinarchListPool<JobQueueItem>.Release(jobs);
		}
	}
	public void AddPoisonCleanser(Character character) {
		poisonCleansers.Add(character);
	}
	public void RemovePoisonCleanser(Character character) {
		poisonCleansers.Remove(character);
	}
#endregion

#region Tend Farm
	private void ScheduleTendFarmCheck() {
		//GameDate checkDate = GameManager.Instance.Today();
		//checkDate.AddDays(1);
		//checkDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(6));
		//SchedulingManager.Instance.AddEntry(checkDate, () => CheckIfFarmShouldBeTended(true), this);
	}
	private void LoadTendFarmCheck() {
		//if (GameManager.Instance.Today().tick < GameManager.Instance.GetTicksBasedOnHour(6)) {
		//	//if current tick is before check time, then schedule check for today at the specified check time.
		//	GameDate checkDate = GameManager.Instance.Today();
		//	checkDate.SetTicks(GameManager.Instance.GetTicksBasedOnHour(6));
		//	SchedulingManager.Instance.AddEntry(checkDate, () => CheckIfFarmShouldBeTended(true), this);
		//} else {
		//	ScheduleTendFarmCheck();
		//}
	}
	public void CheckIfFarmShouldBeTended(bool reschedule) {
		//if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) > 16) {
		//	return; //already 4pm do not create tend job
		//}
		//if (_owner.HasJob(JOB_TYPE.TEND_FARM)) {
		//	return; //already has tend job
		//}
		//List<LocationStructure> farms = _owner.GetStructuresOfType(STRUCTURE_TYPE.FARM);
		//if (farms != null) {
		//	int untendedCornCrops = 0;
		//	for (int i = 0; i < farms.Count; i++) {
		//		LocationStructure farm = farms[i];
		//		List<CornCrop> cornCrops = farm.GetTileObjectsOfType<CornCrop>(TILE_OBJECT_TYPE.CORN_CROP);
		//		for (int j = 0; j < cornCrops.Count; j++) {
		//			CornCrop cornCrop = cornCrops[j];
		//			if (cornCrop.traitContainer.HasTrait("Tended") == false) {
		//				untendedCornCrops++;
		//				if (untendedCornCrops >= 3) {
		//					break;
		//				}
		//			}
		//		}
		//		if (untendedCornCrops >= 3) {
		//			break;
		//		}
		//	}
		//	if (untendedCornCrops >= 3) {
		//		CreateTendFarmJob();
		//	}	
		//}
		//if (reschedule) {
		//	//reschedule check for next day
		//	ScheduleTendFarmCheck();	
		//}
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
		List<JobQueueItem> jobs = RuinarchListPool<JobQueueItem>.Claim();
		_owner.PopulateJobsOfType(jobs, JOB_TYPE.TEND_FARM);
		for (int i = 0; i < jobs.Count; i++) {
			JobQueueItem job = jobs[i];
			job.ForceCancelJob();
		}
		RuinarchListPool<JobQueueItem>.Release(jobs);
	}
#endregion

// #region Mining
// 	private void TryCreateMiningJob() {
// 		if (GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 6 && !_owner.HasJob(JOB_TYPE.MINE) && _owner.HasStructure(STRUCTURE_TYPE.MINE)) { //6
// 			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MINE, INTERACTION_TYPE.BEGIN_MINE, null, _owner);
// 			_owner.AddToAvailableJobs(job);
// 		}
// 	}
// #endregion

#region Party
    //public bool TriggerExterminationJob(LocationStructure targetStructure) { //bool forceDoAction = false
    //    if (!_owner.HasJob(JOB_TYPE.EXTERMINATE)) {
    //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.EXTERMINATE, INTERACTION_TYPE.EXTERMINATE, null, _owner);
    //        job.AddOtherData(INTERACTION_TYPE.EXTERMINATE, new object[] { targetStructure, _owner });
    //        job.SetCanTakeThisJobChecker(JobManager.Can_Take_Exterminate);
    //        _owner.AddToAvailableJobs(job);
    //        return true;
    //    }
    //    return false;
    //}
    public void TriggerJoinGatheringJob(Gathering gathering) {
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JOIN_GATHERING, INTERACTION_TYPE.JOIN_GATHERING, gathering.host, _owner);
        job.SetCanTakeThisJobChecker(JobManager.Can_Take_Join_Gathering);
        _owner.AddToAvailableJobs(job);
    }
#endregion

#region Craft Water Well
    private void StartCraftWaterWellCheck() {
	    CheckIfShouldCraftWaterWell();
	    //check every hour if water well should be crafted
	    Messenger.AddListener(Signals.HOUR_STARTED, CheckIfShouldCraftWaterWell);
    }
    private void CheckIfShouldStopWaterWellCheck() {
	    LocationStructure cityCenter = _owner.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
	    TileObject waterWell = cityCenter.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.WATER_WELL);
	    if (waterWell != null && waterWell.mapObjectState == MAP_OBJECT_STATE.BUILT) {
		    //already has a built water well at city center, stop hourly check
		    StopCraftWaterWellCheck();
	    }
    }
    private void StopCraftWaterWellCheck() {
	    Messenger.RemoveListener(Signals.HOUR_STARTED, CheckIfShouldCraftWaterWell);
    }
    
    public void CheckIfShouldCraftWaterWell() {
#if DEBUG_PROFILER
	    Profiler.BeginSample($"{_owner.name} settlementt Craft Water Well Check");
#endif
	    LocationStructure cityCenter = _owner.GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
	    TileObject waterWell = cityCenter.GetFirstTileObjectOfType<TileObject>(TILE_OBJECT_TYPE.WATER_WELL);
	    Assert.IsNotNull(waterWell);
	    //there is water well present, check if water well is unbuilt
	    if (waterWell.mapObjectState == MAP_OBJECT_STATE.BUILT) {
		    return; //already has built water well! Do not create craft object job.
	    }
	    
	    if (!_owner.HasJob(JOB_TYPE.CRAFT_OBJECT, waterWell)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, waterWell, _owner);
            UtilityScripts.JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(_owner, job, INTERACTION_TYPE.TAKE_RESOURCE);
            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.WATER_WELL).mainRecipe });
		    job.SetCanTakeThisJobChecker(JobManager.Can_Craft_Well);
		    _owner.AddToAvailableJobs(job);
		}
#if DEBUG_PROFILER
	    Profiler.EndSample();
#endif
    }
#endregion

#region Steal Corpse
    public bool CreateStealCorpseJob(LocationStructure dropLocation) {
        if(!_owner.HasJob(JOB_TYPE.STEAL_CORPSE)) {
            LocationGridTile targetTile = dropLocation.GetRandomUnoccupiedTile();
            if (HasStealCorpseTarget()) {
                IPointOfInterest target = GetStealCorpseTarget();
                if(target != null) {
                    if(target is Character) {
                        targetTile = dropLocation.GetRandomPassableTile();
                    }
                    if(targetTile != null) {
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL_CORPSE, INTERACTION_TYPE.DROP_CORPSE, target, _owner);
                        job.AddOtherData(INTERACTION_TYPE.DROP_CORPSE, new object[] { dropLocation, targetTile });
                        job.SetCanTakeThisJobChecker(JobManager.Can_Steal_Corpse);
                        _owner.AddToAvailableJobs(job);
                    }
                }
            }
        }
        return false;
    }
    private bool HasStealCorpseTarget() {
        Region region = _owner.region;
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character target = region.charactersAtLocation[i];
            IPointOfInterest targetPOI = target;
            if(target.grave != null) {
                targetPOI = target.grave;
            }
            if (target.isDead && targetPOI.gridTileLocation != null && targetPOI.mapObjectVisual) {
                return true;
            }
        }
        return false;
    }
    private IPointOfInterest GetStealCorpseTarget() {
        Region region = _owner.region;
        Faction factionOwner = _owner.owner;
        WeightedDictionary<IPointOfInterest> targetWeights = null;
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character target = region.charactersAtLocation[i];
            IPointOfInterest targetPOI = target;
            if (target.grave != null) {
                targetPOI = target.grave;
            }
            if (target.isDead && targetPOI.gridTileLocation != null && targetPOI.mapObjectVisual) {

                if (targetPOI.gridTileLocation.IsPartOfSettlement(_owner)) {
                    //If the target is inside this settlement, only take corpse those corpses that are not in the cult temple
                    //We should not take corpses from our own settlement that are already in the Cult Temple
                    if (targetPOI.gridTileLocation.structure.structureType != STRUCTURE_TYPE.CULT_TEMPLE) {
                        if (targetWeights == null) { targetWeights = new WeightedDictionary<IPointOfInterest>(); }
                        targetWeights.AddElement(targetPOI, 50);
                    }
                } else {
                    BaseSettlement settlement = null;
                    Faction targetLocationFactionOwner = null;
                    if (targetPOI.gridTileLocation.IsPartOfSettlement(out settlement)) {
                        targetLocationFactionOwner = settlement.owner;
                    }
                    int weight = 50;
                    if(factionOwner != null && targetLocationFactionOwner != null && factionOwner.IsHostileWith(targetLocationFactionOwner)) {
                        //Corpses that are inside the settlement of a hostile faction should be less likely to be targeted
                        weight = 10;
                    }

                    if (targetWeights == null) { targetWeights = new WeightedDictionary<IPointOfInterest>(); }
                    targetWeights.AddElement(targetPOI, weight);
                }
            }
        }
        if(targetWeights != null && targetWeights.Count > 0) {
            return targetWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
#endregion

#region Summon Bone Golem
    public bool CreateSummonBoneGolemJob(LocationStructure cultTemple) {
        if (!_owner.HasJob(JOB_TYPE.SUMMON_BONE_GOLEM)) {
            object[] corpses = Get3CorpsesToSummonBoneGolem(cultTemple);
            if (corpses != null) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SUMMON_BONE_GOLEM, new GoapEffect(GOAP_EFFECT_CONDITION.SUMMON, "Bone Golem", false, GOAP_EFFECT_TARGET.ACTOR), null, _owner);
                job.AddPriorityLocation(INTERACTION_TYPE.SUMMON_BONE_GOLEM, cultTemple);
                job.AddOtherData(INTERACTION_TYPE.SUMMON_BONE_GOLEM, corpses);
                job.SetCanTakeThisJobChecker(JobManager.Can_Summon_Bone_Golem);
                job.SetForceCancelOnInvalid(true);
                _owner.AddToAvailableJobs(job);
            }
        }
        return false;
    }
    public object[] Get3CorpsesToSummonBoneGolem(LocationStructure cultTemple) {
        Character corpse1 = null;
        Character corpse2 = null;
        Character corpse3 = null;

        for (int i = 0; i < cultTemple.charactersHere.Count; i++) {
            Character character = cultTemple.charactersHere[i];
            if(character.isDead && character.gridTileLocation != null) {
                if(corpse1 == null) {
                    corpse1 = character;
                } else if (corpse2 == null) {
                    corpse2 = character;
                } else if (corpse3 == null) {
                    corpse3 = character;
                    break;
                }
            }
        }
        if(corpse1 == null || corpse2 == null || corpse3 == null) {
            return null;
        } else {
            return new object[] { corpse1, corpse2, corpse3 };
        }
    }
#endregion
    
#region Quarantine
    public void TriggerQuarantineJob(Character target) {
	    if (!_owner.HasJob(JOB_TYPE.QUARANTINE, target)) {
		    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.QUARANTINE, INTERACTION_TYPE.QUARANTINE, target, _owner);
		    _owner.AddToAvailableJobs(job);
	    }
    }
#endregion

#region Change Class
    //public void OnNeededClassRemoved(string p_removedClass) {
	   // //cancel all change class jobs targeting removed class
	   // List<JobQueueItem> changeClassJobs = _owner.availableJobs.GetJobsWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, p_removedClass);
	   // changeClassJobs?.CancelJobs(false, $"{p_removedClass} is no longer needed.");
    //}
    //private void HourlyCheckForNeededCharacterClasses() {
	   // if (_owner.settlementClassTracker.neededClasses.Count > 0) {
		  //  ProfessionPedestal professionPedestal = _owner.GetFirstTileObjectOfTypeThatIsAvailable<ProfessionPedestal>();
		  //  if (professionPedestal != null) {
			 //   for (int i = 0; i < _owner.settlementClassTracker.neededClasses.Count; i++) {
				//    string neededClass = _owner.settlementClassTracker.neededClasses[i];
				//    if (ShouldCreateChangeClassJob(neededClass)) {
				//	    TriggerChangeClassJob(professionPedestal, neededClass);
				//    }
			 //   }    
		  //  }
	   // }
    //}
    //private int GetAbleResidentsClassAmount(NPCSettlement p_settlement, string p_className) {
	   // int classCount = 0;
	   // for (int i = 0; i < p_settlement.residents.Count; i++) {
		  //  Character resident = p_settlement.residents[i];
		  //  if (resident.characterClass.className == p_className && !resident.traitContainer.HasTrait("Paralyzed", "Quarantined")) {
			 //   classCount++;
		  //  }
	   // }
	   // return classCount;
    //}
    //private bool ShouldCreateChangeClassJob(string p_className) {
	   // int neededAmount = Mathf.FloorToInt((float)_owner.residents.Count * 0.15f);
	   // neededAmount = Mathf.Max(1, neededAmount);
	   // int currentClassAmount = GetAbleResidentsClassAmount(_owner, p_className);
	   // if (currentClassAmount < neededAmount) {
		  //  return true;
	   // }
	   // return false;
    //}
    //private void TriggerChangeClassJob(ProfessionPedestal professionPedestal, string className) {
	   // if (!_owner.availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, className)) {
		  //  GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, professionPedestal, _owner);
		  //  job.SetCanTakeThisJobChecker(JobManager.Can_Take_Change_Class);
		  //  job.AddOtherData(INTERACTION_TYPE.CHANGE_CLASS, new object[] { className });
		  //  _owner.AddToAvailableJobs(job);    
	   // }
    //}
	public void TriggerChangeClassJob(string className, LocationStructure p_reservedStructure) {
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, null, _owner);
		job.SetCanTakeThisJobChecker(JobManager.Can_Take_Change_Class);
        if (p_reservedStructure != null) {
			job.AddOtherData(INTERACTION_TYPE.CHANGE_CLASS, new object[] { className, p_reservedStructure });
		} else {
			job.AddOtherData(INTERACTION_TYPE.CHANGE_CLASS, new object[] { className });
		}
		_owner.AddToAvailableJobs(job);
	}
	#endregion

// 	#region Food Producing Structure
// 	private void TryCreateMissingFoodProducingStructure() {
// 	    if (!_owner.HasFoodProducingStructure()) {
// 		    TriggerBuildFoodProducingStructure();
// 	    }
//     }
//     private void TriggerBuildFoodProducingStructure() {
// 	    if (_owner.owner != null && !_owner.HasJob(JOB_TYPE.PLACE_BLUEPRINT) && !_owner.HasJob(JOB_TYPE.BUILD_BLUEPRINT)) {
// 		    StructureSetting foodProducingStructure = _owner.GetValidFoodProducingStructure();
// 		    if (LandmarkManager.Instance.CanPlaceStructureBlueprint(_owner, foodProducingStructure, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
// 			    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_BLUEPRINT, INTERACTION_TYPE.PLACE_BLUEPRINT, targetTile.tileObjectComponent.genericTileObject, _owner);
// 			    job.AddOtherData(INTERACTION_TYPE.PLACE_BLUEPRINT, new object[] { structurePrefabName, connectorTile, foodProducingStructure });
// 			    _owner.AddToAvailableJobs(job);
// 		    }
// 	    }
//     }
// #endregion
}
