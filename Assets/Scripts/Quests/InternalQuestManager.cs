﻿/*
 This is the Internal Quest Manager. Each Faction has one,
 it is responsible for generating new quests for each faction.
 Reference: https://trello.com/c/Wf38ZqLM/737-internal-manager-ai
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InternalQuestManager : TaskCreator {

    private Faction _owner;

    private List<Quest> _activeQuests;

    #region getters/setters
    public Faction owner {
        get { return _owner; }
    }
    public List<Quest> activeQuests {
        get { return _activeQuests; }
    }
    #endregion

    public InternalQuestManager(Faction owner) {
        _owner = owner;
        _activeQuests = new List<Quest>();
        if(owner is Tribe) {
            GameDate dueDate = new GameDate(GameManager.Instance.month, 15, GameManager.Instance.year);
            SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
        }
    }

    /*
     Get the maximum number of active quests a faction
     can have.
         */
    private int GetMaxActiveQuests() {
        if(_owner.factionType == FACTION_TYPE.MINOR) {
            return 1 + _owner.settlements.Count;
        } else {
            switch (_owner.factionSize) {
                case FACTION_SIZE.SMALL:
                    return 2 + _owner.settlements.Count;
                case FACTION_SIZE.MEDIUM:
                    return 3 + _owner.settlements.Count;
                case FACTION_SIZE.LARGE:
                    return 4 + _owner.settlements.Count;
                default:
                    return 0;
            }
        }
    }

    #region Quest Generation
    /*
     At the start of each month, if the Faction has not yet reached the cap of active Internal Quests, 
     it will attempt to create a new one.
         */
    private void GenerateMonthlyQuests() {
        if(_activeQuests.Count < GetMaxActiveQuests()) {
            WeightedDictionary<Quest> questDictionary = GetQuestWeightedDictionary();
            questDictionary.LogDictionaryValues("Quest Creation Weights: ");
            if(questDictionary.GetTotalOfWeights() > 0) {
                Quest chosenQuestToCreate = questDictionary.PickRandomElementGivenWeights();
                AddNewQuest(chosenQuestToCreate);
            }
        }

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(15);
        SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
    }
    private WeightedDictionary<Quest> GetQuestWeightedDictionary() {
        WeightedDictionary<Quest> questWeights = new WeightedDictionary<Quest>();
        //Loop through each Region that the Faction has a Settlement in.
        for (int i = 0; i < _owner.settlements.Count; i++) {
            Settlement currSettlement = _owner.settlements[i];
            Region regionOfSettlement = currSettlement.location.region;

            AddExpandWeights(questWeights, currSettlement, regionOfSettlement);
            AddExploreTileWeights(questWeights, currSettlement, regionOfSettlement);
            AddBuildStructureWeights(questWeights, currSettlement, regionOfSettlement);
        }
        return questWeights;
    }
    private void AddExploreTileWeights(WeightedDictionary<Quest> questWeights, Settlement currSettlement, Region regionOfSettlement) {
        for (int j = 0; j < regionOfSettlement.connections.Count; j++) {
            object currConnection = regionOfSettlement.connections[j];
            if (currConnection is BaseLandmark) {
                BaseLandmark currLandmark = (BaseLandmark)currConnection;
                if (currLandmark.isHidden && !currLandmark.isExplored) {
                    if (currSettlement.GetQuestsOnBoardByType(QUEST_TYPE.EXPLORE_TILE).Count <= 0 && !AlreadyHasQuestOfType(QUEST_TYPE.EXPLORE_TILE, currLandmark)) {
                        ExploreTile newExploreTileQuest = new ExploreTile(this, currLandmark);
                        newExploreTileQuest.SetSettlement(currSettlement);
                        questWeights.AddElement(newExploreTileQuest, GetExploreLandmarkWeight(currLandmark));
                    }
                }
            }
        }
    }
    private void AddExpandWeights(WeightedDictionary<Quest> questWeights, Settlement currSettlement, Region regionOfSettlement) {
        List<Region> checkedExpandRegions = new List<Region>();
        if (currSettlement.civilians > 20 && !AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, currSettlement)) {
            //checkedExpandRegions.Clear();
            for (int j = 0; j < regionOfSettlement.connections.Count; j++) {
                object currConnection = regionOfSettlement.connections[j];
                if (currConnection is Region) {
                    Region region = (Region)currConnection;
                    if (!region.centerOfMass.isOccupied && !AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, region.centerOfMass)) {
                        checkedExpandRegions.Add(region);
                    }
                }
            }
            if (checkedExpandRegions.Count > 0) {
                Region chosenRegion = checkedExpandRegions[UnityEngine.Random.Range(0, checkedExpandRegions.Count)];
                Expand newExpandQuest = new Expand(this, chosenRegion.centerOfMass, currSettlement.location);
                newExpandQuest.SetSettlement(currSettlement);
                questWeights.AddElement(newExpandQuest, GetExpandWeight(currSettlement));
            }
        }
    }
    private void AddBuildStructureWeights(WeightedDictionary<Quest> questWeights, Settlement currSettlement, Region regionOfSettlement) {
        if(currSettlement.civilians >= 5) {
            for (int i = 0; i < regionOfSettlement.landmarks.Count; i++) {
                BaseLandmark currLandmark = regionOfSettlement.landmarks[i];
                if (currLandmark is ResourceLandmark && !currLandmark.location.HasStructure()) { //Loop through each Resource Tile that doesn't have any structures yet.
                    int totalWeightForLandmark = 0;
                    ResourceLandmark resourceLandmark = currLandmark as ResourceLandmark;
                    MATERIAL materialOnLandmark = resourceLandmark.materialOnLandmark;
                    if (currSettlement.HasTechnology(Utilities.GetNeededTechnologyForMaterial(materialOnLandmark)) && !AlreadyHasQuestOfType(QUEST_TYPE.BUILD_STRUCTURE, resourceLandmark)) {
                        totalWeightForLandmark += 60; //Add 60 Weight to Build Structure if relevant technology is available
                        totalWeightForLandmark -= 30 * regionOfSettlement.GetActivelyHarvestedMaterialsOfType(materialOnLandmark); //- Subtract 30 Weight to Build Structure for each Resource Tile of the same type already actively being harvested in the same region
                        totalWeightForLandmark -= 10 * _owner.GetActivelyHarvestedMaterialsOfType(materialOnLandmark, regionOfSettlement); //- Subtract 10 Weight to Build Structure for each Resource Tile of the same type already actively being harvested in other regions owned by the same Tribe
                        totalWeightForLandmark = Mathf.Max(0, totalWeightForLandmark);
                        BuildStructure buildStructureQuest = new BuildStructure(this, resourceLandmark);
                        buildStructureQuest.SetSettlement(currSettlement);
                        questWeights.AddElement(buildStructureQuest, totalWeightForLandmark);
                    }
                }
            }
        }
    }
    //private int GetExploreRegionWeight(Region region) {
    //    int weight = 0;
    //    for (int i = 0; i < region.landmarks.Count; i++) {
    //        BaseLandmark landmark = region.landmarks[i];
    //        if (landmark.isHidden) {
    //            weight += 20; //Add 20 Weight to Explore Region for each undiscovered Landmark
    //        }
    //    }
    //    return weight;
    //}
    private int GetExploreLandmarkWeight(BaseLandmark landmark) {
        int weight = 0;
        if (!landmark.isExplored) {
            weight += 50; //Add 50 Weight to Explore Tile Weight
        }
        return weight;
    }
	private int GetExpandWeight(BaseLandmark landmark) {
//		int weight = 1 + (5 * (region.landmarks.Count - 1));
//		for (int i = 0; i < region.connections.Count; i++) {
//			if(region.connections[i] is Region){
//				Region adjacentRegion = (Region)region.connections[i];
//				if (adjacentRegion.centerOfMass.landmarkOnTile.owner != null && adjacentRegion.centerOfMass.landmarkOnTile.owner.id == this._owner.id) {
//					int regionWeight = (int)(adjacentRegion.centerOfMass.landmarkOnTile.civilians - 40f);
//					if(regionWeight > 0){
//						weight += regionWeight;
//					}
//				}
//			}
//		}
		return 5;
	}
	internal void CreateExpandQuest(BaseLandmark originLandmark){
		HexTile unoccupiedTile = originLandmark.GetRandomAdjacentUnoccupiedTile ();
		if(unoccupiedTile != null){
			Expand expand = new Expand(this, unoccupiedTile, originLandmark.location);
            expand.SetSettlement((Settlement)originLandmark);
			AddNewQuest(expand);
		}
	}
	internal void CreateExploreTileQuest(BaseLandmark landmarkToExplore){
        ExploreTile exploreQuest = new ExploreTile(this, landmarkToExplore);
        exploreQuest.SetSettlement((Settlement)landmarkToExplore.location.region.centerOfMass.landmarkOnTile);
        AddNewQuest(exploreQuest);
    }
    internal void CreateBuildStructureQuest(BaseLandmark landmarkToExplore) {
        BuildStructure buildStructure = new BuildStructure(this, landmarkToExplore);
        buildStructure.SetSettlement((Settlement)landmarkToExplore.location.region.centerOfMass.landmarkOnTile);
        AddNewQuest(buildStructure);
    }
    #endregion

    #region Quest Management
    public void AddNewQuest(Quest quest) {
        if (!_activeQuests.Contains(quest)) {
            _activeQuests.Add(quest);
            _owner.AddNewQuest(quest);
            if(quest.postedAt != null) {
                quest.postedAt.AddQuestToBoard(quest);
            }
            //quest.ScheduleDeadline(); //Once a quest has been added to active quest, scedule it's deadline
        }
    }
    public void RemoveQuest(Quest quest) {
        _activeQuests.Remove(quest);
        _owner.RemoveQuest(quest);
    }
    public List<Quest> GetQuestsOfType(QUEST_TYPE questType) {
        List<Quest> quests = new List<Quest>();
        for (int i = 0; i < _activeQuests.Count; i++) {
            Quest currQuest = _activeQuests[i];
            if(currQuest.questType == questType) {
                quests.Add(currQuest);
            }
        }
        return quests;
    }
	public bool AlreadyHasQuestOfType(QUEST_TYPE questType, object identifier){
		for (int i = 0; i < _activeQuests.Count; i++) {
			Quest currQuest = _activeQuests[i];
			if(currQuest.questType == questType) {
				if(questType == QUEST_TYPE.EXPLORE_REGION){
					Region region = (Region)identifier;
					if(((ExploreRegion)currQuest).regionToExplore.id == region.id){
						return true;
					}
				} else if(questType == QUEST_TYPE.EXPAND){
					if(identifier is HexTile){
						HexTile hexTile = (HexTile)identifier;
						if(((Expand)currQuest).targetUnoccupiedTile.id == hexTile.id){
							return true;
						}
					}else if(identifier is BaseLandmark){
						BaseLandmark landmark = (BaseLandmark)identifier;
						if(((Expand)currQuest).originTile.id == landmark.location.id){
							return true;
						}
					}

				} else if (questType == QUEST_TYPE.EXPLORE_TILE) {
                    BaseLandmark landmark = (BaseLandmark)identifier;
                    if (((ExploreTile)currQuest).landmarkToExplore.id == landmark.id) {
                        return true;
                    }
                } else if (questType == QUEST_TYPE.BUILD_STRUCTURE) {
                    BaseLandmark landmark = (BaseLandmark)identifier;
                    if (((BuildStructure)currQuest).target.id == landmark.id) {
                        return true;
                    }
                }
            }
		}
		return false;
	}
    #endregion
}
