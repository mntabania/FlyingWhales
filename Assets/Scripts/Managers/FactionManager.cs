using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FactionManager : MonoBehaviour {

    public static FactionManager Instance = null;

    //[SerializeField] private RACE[] inititalRaces;

    private ORDER_BY orderBy = ORDER_BY.CITIES;

    //public List<QuestTypeSetup> questTypeSetups;

    public List<Faction> allFactions = new List<Faction>();
	public List<Tribe> allTribes { get { return allFactions.Where(x => x is Tribe).Select(x => x as Tribe).ToList(); } }
    public List<Faction> orderedFactions = new List<Faction>();

    //public List<OldQuest.Quest> allQuests = new List<OldQuest.Quest>();

    public Dictionary<RACE, List<TECHNOLOGY>> initialRaceTechnologies = new Dictionary<RACE, List<TECHNOLOGY>>() {
        { RACE.HUMANS, new List<TECHNOLOGY>(){
	            TECHNOLOGY.BASIC_FARMING,
	            TECHNOLOGY.BASIC_MINING,
	            TECHNOLOGY.SWORDSMAN_CLASS,
	            TECHNOLOGY.SPEARMAN_CLASS,
	            TECHNOLOGY.WILDLING_CLASS,
	            TECHNOLOGY.SWORD_MAKING,
	            TECHNOLOGY.SPEAR_MAKING,
	            TECHNOLOGY.AXE_MAKING,
	            TECHNOLOGY.CHEST_ARMOR_MAKING,
	            TECHNOLOGY.HELMET_MAKING,
                TECHNOLOGY.HUMAN_LANGUAGE
        }},
        { RACE.ELVES, new List<TECHNOLOGY>(){
				TECHNOLOGY.BASIC_HUNTING,
				TECHNOLOGY.BASIC_WOODCUTTING,
		        TECHNOLOGY.ARCHER_CLASS,
		        TECHNOLOGY.ROGUE_CLASS,
		        TECHNOLOGY.MAGE_CLASS,
		        TECHNOLOGY.BOW_MAKING,
		        TECHNOLOGY.DAGGER_MAKING,
		        TECHNOLOGY.STAFF_MAKING,
		        TECHNOLOGY.LEGGINGS_MAKING,
		        TECHNOLOGY.GLOVE_MAKING,
                TECHNOLOGY.ELVEN_LANGUAGE
        }}
    };
    public Dictionary<RACE, List<TECHNOLOGY>> bonusRaceTechnologies = new Dictionary<RACE, List<TECHNOLOGY>>() {
        { RACE.HUMANS, new List<TECHNOLOGY>(){
                TECHNOLOGY.ARCHER_CLASS,
                TECHNOLOGY.ROGUE_CLASS,
                TECHNOLOGY.MAGE_CLASS,
                TECHNOLOGY.BOW_MAKING,
                TECHNOLOGY.DAGGER_MAKING,
                TECHNOLOGY.STAFF_MAKING,
                TECHNOLOGY.ADVANCED_FARMING,
                TECHNOLOGY.ADVANCED_MINING,
                TECHNOLOGY.BASIC_HUNTING,
                TECHNOLOGY.BASIC_WOODCUTTING,
                TECHNOLOGY.BASIC_QUARRYING,
                TECHNOLOGY.GOBLIN_LANGUAGE,
                TECHNOLOGY.ELVEN_LANGUAGE,
                TECHNOLOGY.TROLL_LANGUAGE,
                TECHNOLOGY.BATTLEMAGE_CLASS,
                TECHNOLOGY.SCOUT_CLASS,
                TECHNOLOGY.BARBARIAN_CLASS,
                TECHNOLOGY.KNIGHT_CLASS,
                TECHNOLOGY.NIGHTBLADE_CLASS
        }},
        { RACE.ELVES, new List<TECHNOLOGY>(){
                TECHNOLOGY.SWORDSMAN_CLASS,
                TECHNOLOGY.SPEARMAN_CLASS,
                TECHNOLOGY.WILDLING_CLASS,
                TECHNOLOGY.SWORD_MAKING,
                TECHNOLOGY.SPEAR_MAKING,
                TECHNOLOGY.AXE_MAKING,
                TECHNOLOGY.ADVANCED_HUNTING,
                TECHNOLOGY.ADVANCED_WOODCUTTING,
                TECHNOLOGY.BASIC_FARMING,
                TECHNOLOGY.BASIC_MINING,
                TECHNOLOGY.BASIC_QUARRYING,
                TECHNOLOGY.GOBLIN_LANGUAGE,
                TECHNOLOGY.HUMAN_LANGUAGE,
                TECHNOLOGY.TROLL_LANGUAGE,
                TECHNOLOGY.RANGER_CLASS,
                TECHNOLOGY.BATTLEMAGE_CLASS,
                TECHNOLOGY.SCOUT_CLASS,
                TECHNOLOGY.ARCANIST_CLASS,
                TECHNOLOGY.NIGHTBLADE_CLASS
        }}
    };

    [Space(10)]
    [Header("Tribe Settings")]
    [SerializeField]
    private List<InitialTribeSetting> _initialTribes;

    [Space(10)]
    [Header("Visuals")]
    [SerializeField] private List<Sprite> _emblemBGs;
    [SerializeField] private List<Sprite> _emblems;
    [SerializeField] private List<Sprite> usedEmblems = new List<Sprite>();

    [Space(10)]
    [Header("Faction Size Modifiers")]
    [SerializeField] internal float smallToMediumReqPercentage;
    [SerializeField] internal float mediumToLargeReqPercentage;
    [SerializeField] internal int smallToMediumReq;
    [SerializeField] internal int mediumToLargeReq;

    #region getters
    public List<Faction> majorFactions {
        get { return allFactions.Where(x => x.factionType == FACTION_TYPE.MAJOR).ToList(); }
    }
    public List<Faction> minorFactions {
        get { return allFactions.Where(x => x.factionType == FACTION_TYPE.MINOR).ToList(); }
    }
    #endregion

    private void Awake() {
        Instance = this;
    }

    #region Faction Generation
    public void LoadFactions(WorldSaveData data) {
        if (data.factionsData != null) {
            for (int i = 0; i < data.factionsData.Count; i++) {
                FactionSaveData currData = data.factionsData[i];
                Faction currFaction = CreateNewFaction(currData);
#if WORLD_CREATION_TOOL
                worldcreator.WorldCreatorUI.Instance.editFactionsMenu.OnFactionCreated(currFaction);
            }
            worldcreator.WorldCreatorUI.Instance.editCharactersMenu.characterInfoEditor.LoadFactionDropdownOptions();
#else
            }
#endif
        }
    }
    /*
     Generate the initital factions,
     races are specified in the inspector (inititalRaces)
     */
    public void GenerateInitialFactions(ref Region playerRegion) {
        smallToMediumReq = Mathf.FloorToInt((float)GridMap.Instance.numOfRegions * (smallToMediumReqPercentage / 100f));
        mediumToLargeReq = Mathf.FloorToInt((float)GridMap.Instance.numOfRegions * (mediumToLargeReqPercentage / 100f));
        Dictionary<List<Region>, FACTION_SIZE> initialFactions = new Dictionary<List<Region>, FACTION_SIZE>();
        List<Region> elligibleRegions = new List<Region>();

        RACE[] races = new RACE[] { RACE.HUMANS, RACE.ELVES };

        //For now just generate 2-5 tribes with 1 region each
        //int numOfTribes = Random.Range(2, 6);
        //5 regions will be owned by five different kingdoms. The other region will contain the Player's Demonic Portal.
        int numOfTribes = 5;
        FACTION_SIZE size = FACTION_SIZE.SMALL;
        while (initialFactions.Count <= 0) {
            elligibleRegions.Clear();
            elligibleRegions.AddRange(GridMap.Instance.allRegions);
            for (int i = 0; i < numOfTribes; i++) {
                List<Region> chosenRegions = GetFactionRegions(size, elligibleRegions, 1);
                if (chosenRegions != null) {
                    initialFactions.Add(chosenRegions, size);
                } else {
                    initialFactions.Clear();
                    break;
                }
            }
        }

        foreach (KeyValuePair<List<Region>, FACTION_SIZE> initialFactionSetup in initialFactions) {
            RACE chosenRace = races[Random.Range(0, races.Length)]; //Randomize the race of each Tribe (Human or Elves) and their technologies.
            Faction newFaction = CreateNewFaction(chosenRace);
            newFaction.GenerateBonusTech(initialFactionSetup.Value);
            for (int i = 0; i < initialFactionSetup.Key.Count; i++) {
                Region currRegion = initialFactionSetup.Key[i];
                currRegion.SetOwner(newFaction);
                newFaction.OwnRegion(currRegion);
                currRegion.ReColorBorderTiles(newFaction.factionColor);
                currRegion.SetMinimapColor(newFaction.factionColor, 69f / 255f);
            }
        }
        
        playerRegion = elligibleRegions[0];
        //return true;


        //InitialTribeSetting initialTribes = _initialTribes[Random.Range(0, _initialTribes.Count)]; //Choose a random tribe setup
        //Dictionary<FACTION_SIZE, int> tribeSetupDict = initialTribes.GetDictionary();
        //Utilities.LogDictionary(tribeSetupDict);


        ////loop through the initial tribe setup
        //foreach (KeyValuePair<FACTION_SIZE, int> kvp in tribeSetupDict) {
        //    FACTION_SIZE currSize = kvp.Key;
        //    int numOfTribes = kvp.Value;
        //    for (int i = 0; i < numOfTribes; i++) {
        //        RACE chosenRace = races[Random.Range(0, races.Length)]; //Randomize the race of each Tribe (Human or Elves) and their technologies.
        //        if(!CreateInitialFaction(currSize, elligibleRegions, chosenRace)) {
        //            return false;
        //        }
        //    }
        //}
        //return true;
    }
    private List<Region> GetFactionRegions(FACTION_SIZE size, List<Region> elligibleRegions, int numOfRegions = 0) {
        int numOfRegionsForCurrentFaction = numOfRegions;
        if (numOfRegions == 0) {
            numOfRegionsForCurrentFaction = GetInitialVillageCount(size);
        }
        Region initialRegion = elligibleRegions[Random.Range(0, elligibleRegions.Count)];
        List<Region> chosenRegions = new List<Region>();
        chosenRegions.Add(initialRegion);
        elligibleRegions.Remove(initialRegion);

        while (chosenRegions.Count != numOfRegionsForCurrentFaction) {
            List<Region> choices = new List<Region>();
            //Add unowned adjacent regions of chosenRegions to choices
            for (int i = 0; i < chosenRegions.Count; i++) {
                Region currChosenRegion = chosenRegions[i];
                for (int j = 0; j < currChosenRegion.adjacentRegions.Count; j++) {
                    Region currAdjacentRegion = currChosenRegion.adjacentRegions[j];
                    if (!chosenRegions.Contains(currAdjacentRegion) && elligibleRegions.Contains(currAdjacentRegion) && !choices.Contains(currAdjacentRegion)) {
                        choices.Add(currAdjacentRegion);
                    }
                }
            }
            if (choices.Count > 0) {
                Region chosenRegion = choices[Random.Range(0, choices.Count)];
                chosenRegions.Add(chosenRegion);
                elligibleRegions.Remove(chosenRegion);
            } else {
                return null;
            }
        }
        return chosenRegions;
       
    }
    public void GenerateFactionCharacters() {
        for (int i = 0; i < allTribes.Count; i++) {
            Tribe currTribe = allTribes[i];
            for (int j = 0; j < currTribe.settlements.Count; j++) {
                Settlement currSettlement = currTribe.settlements[j];
                CreateInitialFactionCharacters(currTribe, currSettlement);
            }
            //CreateChieftainForFaction(currTribe);
        }
    }
    private List<Region> GetElligibleRegionsForFaction(Faction faction) {
        List<Region> elligibleRegions = new List<Region>();
        for (int i = 0; i < faction.settlements.Count; i++) {
			Region regionOfSettlement = faction.settlements[i].tileLocation.region;
            for (int j = 0; j < regionOfSettlement.adjacentRegions.Count; j++) {
                Region adjacentRegion = regionOfSettlement.adjacentRegions[j];
                if (adjacentRegion.owner == null && !elligibleRegions.Contains(adjacentRegion)) {
                    elligibleRegions.Add(adjacentRegion);
                }
            }
        }
        return elligibleRegions;
    }
    private int GetInitialVillageCount(FACTION_SIZE size) {
        switch (size) {
            case FACTION_SIZE.SMALL:
                return Random.Range(1, 3); //Small Tribe: 1 to 2 adjacent Villages
            case FACTION_SIZE.MEDIUM:
                return 3; //Medium Tribe: 3 adjacent Villages
            case FACTION_SIZE.LARGE:
                return Random.Range(4, 6); //Large Tribe: 4 to 5 adjacent Villages
            default:
                return 0;
        }
    }
    //private void CreateChieftainForFaction(Faction faction) {
    //    Settlement kingsCastle = faction.GetOwnedLandmarkOfType(LANDMARK_TYPE.KINGS_CASTLE) as Settlement;
    //    ECS.Character chieftain = kingsCastle.CreateNewCharacter(CHARACTER_ROLE.CHIEFTAIN, "Swordsman");
    //    CharacterManager.Instance.EquipCharacterWithBestGear(kingsCastle, chieftain);
    //    faction.SetLeader(chieftain);
    //}
    /*
     Initital tribes should have a chieftain and a village head.
         */
    private void CreateInitialFactionCharacters(Faction faction, Settlement settlement) {
        int numOfCharacters = Random.Range(1, 3); //Generate 1 to 3 characters in each Village with civilians, limit class based on technologies known by its Faction.
        //WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(settlement);
        WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();
        for (int i = 0; i < numOfCharacters; i++) {
            //CHARACTER_CLASS chosenClass = characterClassProductionDictionary.PickRandomElementGivenWeights();
            CHARACTER_CLASS chosenClass = CHARACTER_CLASS.WARRIOR;
            CHARACTER_ROLE chosenRole = characterRoleProductionDictionary.PickRandomElementGivenWeights();
			ECS.Character newChar = settlement.CreateNewCharacter(faction.race, chosenRole, Utilities.NormalizeString(chosenClass.ToString()));
			//Initial Character tags
			newChar.AssignInitialTags();
            CharacterManager.Instance.EquipCharacterWithBestGear(settlement, newChar);
        }
    }
	private void EquipFullArmorSet(MATERIAL materialToUse, ECS.Character character){
		if(materialToUse == MATERIAL.NONE){
			return;
		}
		foreach (ARMOR_TYPE armorType in ItemManager.Instance.armorTypeData.Keys) {
			string armorName = Utilities.NormalizeString(materialToUse.ToString()) + " " + Utilities.NormalizeString(armorType.ToString());
			ECS.Item item = ItemManager.Instance.CreateNewItemInstance(armorName);
			character.EquipItem (item);
		}
	}
    public Faction CreateNewFaction(RACE race) {
        Faction newFaction = new Tribe(race);
   //     if (factionType == typeof(Tribe)) {
   //         newFaction = new Tribe(race);
			//allTribes.Add ((Tribe)newFaction);
   //     } else if(factionType == typeof(Camp)) {
   //         newFaction = new Camp(race);
   //     } else {
   //         newFaction = new Faction(race, FACTION_TYPE.MAJOR);
   //     }
        allFactions.Add(newFaction);
#if !WORLD_CREATION_TOOL
        CreateRelationshipsForNewFaction(newFaction);
        UpdateFactionOrderBy();
#endif
        //UIManager.Instance.UpdateFactionSummary();
        return newFaction;
    }
    public Faction CreateNewFaction(FactionSaveData data) {
        Faction newFaction = new Tribe(data);
        //     if (factionType == typeof(Tribe)) {
        //         newFaction = new Tribe(race);
        //allTribes.Add ((Tribe)newFaction);
        //     } else if(factionType == typeof(Camp)) {
        //         newFaction = new Camp(race);
        //     } else {
        //         newFaction = new Faction(race, FACTION_TYPE.MAJOR);
        //     }
        allFactions.Add(newFaction);
#if !WORLD_CREATION_TOOL
        CreateRelationshipsForNewFaction(newFaction);
        UpdateFactionOrderBy();
#endif
        //UIManager.Instance.UpdateFactionSummary();
        return newFaction;
    }
    public void DeleteFaction(Faction faction) {
        for (int i = 0; i < faction.ownedRegions.Count; i++) {
            Region currRegion = faction.ownedRegions[i];
            currRegion.SetOwner(null);
        }
        allFactions.Remove(faction);
    }
    //private void CreateInitialResourceStructuresForFaction(Faction faction, Settlement settlement, Region region) {
    //    for (int i = 0; i < region.tilesWithMaterials.Count; i++) {
    //        HexTile currTile = region.tilesWithMaterials[i];
    //        if (currTile.HasStructure()) {
    //            continue; //skip tiles that already have structures
    //        }
    //        //Does the settlement have the needed technology to build a structure on the current tile?
    //        if (settlement.HasTechnology(MaterialManager.Instance.GetNeededTechnologyForMaterialStructure(currTile.materialOnTile))) {
    //            //Based on known technologies of each Tribe, create Structures for the Resource Landmarks within their regions.
    //            BaseLandmark newLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(currTile, Utilities.ConvertMaterialToLandmarkType(currTile.materialOnTile));
    //            newLandmark.OccupyLandmark(faction);
    //            newLandmark.AdjustCivilians(faction.race, Random.Range(5, 11)); //Each one should have between 5 to 10 civilians.
    //            settlement.AddLandmarkAsOwned(newLandmark);
				//List<HexTile> road = LandmarkManager.Instance.CreateRoadsForLandmarks(newLandmark.tileLocation);
    //            RoadManager.Instance.CreateRoad(road, ROAD_TYPE.MINOR);
    //        }
    //    }
    //}
    //public void GenerateMonsters() {
    //    List<BaseLandmark> goblinCamps = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.GOBLIN_CAMP);
    //    List<BaseLandmark> caves = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.CAVE);
    //    //List<BaseLandmark> caves = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.CAVE);
    //    //For each Goblin Camp landmark, there is a 25% chance to create a goblin bandit
    //    for (int i = 0; i < goblinCamps.Count; i++) {
    //        BaseLandmark currGoblinCamp = goblinCamps[i];
    //        if (Random.Range(0, 100) < 25) {
    //            WeightedDictionary<CHARACTER_CLASS> classProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(currGoblinCamp);
    //            currGoblinCamp.CreateNewCharacter(RACE.GOBLIN, CHARACTER_ROLE.BANDIT,
    //                Utilities.NormalizeString(classProductionDictionary.PickRandomElementGivenWeights().ToString()), false);
    //            Debug.Log("Created a goblin bandit at " + currGoblinCamp.landmarkName + "(" + currGoblinCamp.tileLocation.name + ")");
    //        }
    //    }
    //    if (Random.Range(0, 100) < 50) {
    //        List<BaseLandmark> choices = new List<BaseLandmark>(caves);
    //        choices.AddRange(LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.ANCIENT_RUIN));
    //        BaseLandmark chosenLandmark = choices[Random.Range(0, choices.Count)];
    //        //50% chance to spawn a dragons beast in Hibernate state in a random Cave or Ancient Ruin.
    //        ECS.Character dragon = chosenLandmark.CreateNewCharacter(RACE.DRAGON, CHARACTER_ROLE.BEAST, "Classless", false, false);
    //        CharacterTask hibernateTask = dragon.GetTag(CHARACTER_TAG.HIBERNATES).GetTask(TASK_TYPE.HIBERNATE);
    //        hibernateTask.SetLocation(chosenLandmark);
    //        dragon.SetTaskToDoNext(hibernateTask);
    //        dragon.DetermineAction();
    //        Debug.Log("Created a dragon at " + chosenLandmark.landmarkName + "(" + chosenLandmark.tileLocation.name + ")");
    //    }

    //    for (int i = 0; i < caves.Count; i++) {
    //        BaseLandmark currCave = caves[i];
    //        //For each unoccupied Cave in a region with an occupied Settlement, 25% chance to spawn a bandit, the race should match the region's occupant race.
    //        if (currCave.tileLocation.region.mainLandmark.isOccupied && currCave.charactersAtLocation.Count == 0) {
    //            if (Random.Range(0, 100) < 25) {
    //                WeightedDictionary<CHARACTER_CLASS> classProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(currCave.tileLocation.region.mainLandmark);
    //                ECS.Character bandit = currCave.CreateNewCharacter(currCave.tileLocation.region.mainLandmark.owner.race, CHARACTER_ROLE.BANDIT, 
    //                    Utilities.NormalizeString(classProductionDictionary.PickRandomElementGivenWeights().ToString()), false);
    //                Debug.Log("Created a bandit " + bandit.characterClass.className + " at " + currCave.landmarkName + "(" + currCave.tileLocation.name + ")");
    //            } else {
    //                //generate a wolf
    //                if (Random.Range(0, 100) < 50) {
    //                    if (Random.Range(0, 2) == 0) {
    //                        //generate alpha wolf
    //                        currCave.CreateNewCharacter(RACE.WOLF, "Alpha Wolf", false);
    //                        Debug.Log("Created an alpha wolf at " + currCave.landmarkName + "(" + currCave.tileLocation.name + ")");
    //                    } else {
    //                        //generate normal wolf
    //                        currCave.CreateNewCharacter(RACE.WOLF, "Wolf", false);
    //                        Debug.Log("Created a wolf at " + currCave.landmarkName + "(" + currCave.tileLocation.name + ")");
    //                    }
    //                }
    //            }
    //        }

    //    }
    //}
    public void OccupyLandmarksInFactionRegions() {
        for (int i = 0; i < allTribes.Count; i++) {
            Faction currTribe = allTribes[i];
            for (int j = 0; j < currTribe.ownedRegions.Count; j++) {
                Region currRegion = currTribe.ownedRegions[j];
                for (int k = 0; k < currRegion.landmarks.Count; k++) {
                    BaseLandmark currLandmark = currRegion.landmarks[k];
                    if (!currLandmark.isOccupied) { //currLandmark is Settlement &&
                        currLandmark.OccupyLandmark(currTribe);
                    }
                }
            }
        }
    }
    #endregion

    #region Emblem
    /*
     * Generate an emblem for a kingdom.
     * This will return a sprite and set that sprite as used.
     * Will return an error if there are no more available emblems.
     * */
    internal Sprite GenerateFactionEmblem(Faction faction) {
        List<Sprite> emblemsToUse = new List<Sprite>(_emblems);
        for (int i = 0; i < emblemsToUse.Count; i++) {
            Sprite currSprite = emblemsToUse[i];
            if (!usedEmblems.Contains(currSprite)) {
                AddEmblemAsUsed(currSprite);
                return currSprite;
            }
        }
        return _emblems[Random.Range(0, _emblems.Count)];
        //throw new System.Exception("There are no more emblems for kingdom: " + faction.name);
    }
    internal Sprite GenerateFactionEmblemBG() {
        return _emblemBGs[Random.Range(0, _emblemBGs.Count)];
    }
    internal void AddEmblemAsUsed(Sprite emblem) {
        if (!usedEmblems.Contains(emblem)) {
            usedEmblems.Add(emblem);
        } else {
            throw new System.Exception("Emblem " + emblem.name + " is already being used!");
        }
    }
    internal void RemoveEmblemAsUsed(Sprite emblem) {
        usedEmblems.Remove(emblem);
    }
    #endregion

    #region Characters
    public List<ECS.Character> GetAllCharactersOfType(CHARACTER_ROLE role) {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            characters.AddRange(currFaction.GetCharactersOfType(role));
        }
        return characters;
    }
    public ECS.Character GetCharacterByID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction currFaction = allFactions[i];
            ECS.Character charInFaction = currFaction.GetCharacterByID(id);
            if(charInFaction != null) {
                return charInFaction;
            }
        }
        return null;
    }
    #endregion

    #region Utilities
    public void SetOrderBy(ORDER_BY orderBy) {
        this.orderBy = orderBy;
        UpdateFactionOrderBy();
    }
    public void UpdateFactionOrderBy() {
        if (orderBy == ORDER_BY.CITIES) {
            orderedFactions = majorFactions.OrderBy(x => x.settlements.Count).ToList();
            orderedFactions.AddRange(minorFactions.OrderBy(x => x.settlements.Count));
        } else if (orderBy == ORDER_BY.CHARACTERS) {
            orderedFactions = majorFactions.OrderBy(x => x.characters.Count).ToList();
            orderedFactions.AddRange(minorFactions.OrderBy(x => x.characters.Count));
        }
        //else if (orderBy == ORDER_BY.POPULATION) {
        //    orderedFactions = majorFactions.OrderBy(x => x.totalPopulation).ToList();
        //    orderedFactions.AddRange(minorFactions.OrderBy(x => x.totalPopulation));
        //} 
        //UIManager.Instance.UpdateFactionSummary();
    }
    public Faction GetFactionBasedOnID(int id) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].id == id) {
                return allFactions[i];
            }
        }
        return null;
    }
    public Faction GetFactionBasedOnName(string name) {
        for (int i = 0; i < allFactions.Count; i++) {
            if (allFactions[i].name.ToLower() == name.ToLower()) {
                return allFactions[i];
            }
        }
        return null;
    }
    #endregion

    #region Relationships
    public void CreateRelationshipsForNewFaction(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if(otherFaction.id != faction.id) {
                CreateNewRelationshipBetween(otherFaction, faction);
            }
        }
    }
    public void RemoveRelationshipsWith(Faction faction) {
        for (int i = 0; i < allFactions.Count; i++) {
            Faction otherFaction = allFactions[i];
            if (otherFaction.id != faction.id) {
                otherFaction.RemoveRelationshipWith(faction);
            }
        }
    }
    /*
     Create a new relationship between 2 factions,
     then add add a reference to that relationship, to both of the factions.
         */
    public void CreateNewRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship newRel = new FactionRelationship(faction1, faction2);
        faction1.AddNewRelationship(faction2, newRel);
        faction2.AddNewRelationship(faction1, newRel);
    }
    /*
     Utility Function for getting the relationship between 2 factions,
     this just adds a checking for data consistency if, the 2 factions have the
     same reference to their relationship.
     NOTE: This is probably more performance intensive because of the additional checking.
     User can opt to use each factions GetRelationshipWith() instead.
         */
    public FactionRelationship GetRelationshipBetween(Faction faction1, Faction faction2) {
        FactionRelationship faction1Rel = faction1.GetRelationshipWith(faction2);
        FactionRelationship faction2Rel = faction2.GetRelationshipWith(faction1);
        if (faction1Rel == faction2Rel) {
            return faction1Rel;
        }
        throw new System.Exception(faction1.name + " does not have the same relationship object as " + faction2.name + "!");
    }
    #endregion

    #region Quests
    //public OldQuest.Quest GetQuestByID(int id) {
    //    for (int i = 0; i < allQuests.Count; i++) {
    //        OldQuest.Quest currQuest = allQuests[i];
    //        if (currQuest.id == id) {
    //            return currQuest;
    //        }
    //    }
    //    return null;
    //}
    //public void AddQuest(OldQuest.Quest quest) {
    //    allQuests.Add(quest);
    //}
    //public void RemoveQuest(OldQuest.Quest quest) {
    //    allQuests.Remove(quest);
    //}
    /*
    // Check if a quest can cause harmful effects to the owner
    // of the region.
    //     */
    //public bool IsQuestHarmful(QUEST_TYPE questType) {
    //    for (int i = 0; i < questTypeSetups.Count; i++) {
    //        QuestTypeSetup currSetup = questTypeSetups[i];
    //        if(currSetup.questType == questType) {
    //            return currSetup.isHarmful;
    //        }
    //    }
    //    return false;
    //}
    ///*
    // Can a quest type be accepted by characters from another faction.
    //     */
    //public bool CanQuestBeAcceptedOutsideFaction(QUEST_TYPE questType) {
    //    for (int i = 0; i < questTypeSetups.Count; i++) {
    //        QuestTypeSetup currSetup = questTypeSetups[i];
    //        if (currSetup.questType == questType) {
    //            return currSetup.canBeAcceptedOutsideFaction;
    //        }
    //    }
    //    return false;
    //}
    //public QuestTypeSetup GetQuestTypeSetup(QUEST_TYPE questType) {
    //    for (int i = 0; i < questTypeSetups.Count; i++) {
    //        QuestTypeSetup currSetup = questTypeSetups[i];
    //        if (currSetup.questType == questType) {
    //            return currSetup;
    //        }
    //    }
    //    return null;
    //}
    #endregion

    #region International Incidents
    //public void InternationalIncidentOccured(Faction aggrievedFaction, Faction aggressorFaction, INTERNATIONAL_INCIDENT_TYPE incidentType, object data) {
    //    Debug.Log("An international incident has occured between " + aggrievedFaction.name + " and " + aggressorFaction.name + "/" + incidentType.ToString());
    //    FactionRelationship rel = GetRelationshipBetween(aggrievedFaction, aggressorFaction);
    //    if(rel.relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {//Incidents will not occur if two factions are already hostile
    //        rel.AdjustSharedOpinion(-5); //Each time an international incident occurs, Shared Opinion decreases by 5.

    //        /*When an incident occurs, war may be declared. The likelihood of this happening is dependent on 
    //        Tribal Opinion and Chieftain Traits. Any Tribe that declare war should remove existing 
    //        International Incidents, Trade Deals and Alliances between them.*/
    //        WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = GetInternationalIncidentWeights(aggrievedFaction, aggressorFaction, rel, incidentType, data);
    //        INTERNATIONAL_INCIDENT_ACTION chosenAction = actionWeights.PickRandomElementGivenWeights();
    //        if (chosenAction == INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR) {
    //            DecalreWar(aggressorFaction, aggressorFaction, incidentType, data);
    //        }
    //    }
    //}
   // private WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> GetInternationalIncidentWeights(Faction aggrievedFaction, Faction aggressorFaction, 
   //     FactionRelationship relationship, INTERNATIONAL_INCIDENT_TYPE incidentType, object data) {

   //     WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> actionWeights = new WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION>();
   //     actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 95); //95 Weight to do nothing
   //     actionWeights.AddElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 5); //5 Weight to Declare War

   //     //Character Death
   //     if(incidentType == INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH) {
			//if(data is ECS.Character){
			//	CHARACTER_ROLE diedRole = ((ECS.Character)data).role.roleType;
			//	switch (diedRole) {
			//	case CHARACTER_ROLE.CHIEFTAIN:
			//		actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 150); //- Chieftain: Add 150 Weight to Declare War
			//		break;
			//	case CHARACTER_ROLE.WARLORD:
			//		actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 20); //-Warlord: Add 20 Weight to Declare War
			//		break;
			//	case CHARACTER_ROLE.HERO:
			//		actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 20); //-Hero: Add 20 Weight to Declare War
			//		break;
			//	case CHARACTER_ROLE.VILLAGE_HEAD:
			//		actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 50); //-Village Head: Add 50 Weight to Declare War
			//		break;
			//	default:
			//		actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, 10); //-Others: Add 10 Weight to Declare War
			//		break;
			//	}
			//}
   //     }
   //     ////Negative OldQuest.Quest
   //     //else if (incidentType == INTERNATIONAL_INCIDENT_TYPE.HARMFUL_QUEST) {
   //     //    //Add Weight to Declare War as listed on the OldQuest.Quest Type
   //     //    QuestTypeSetup qts = GetQuestTypeSetup(((OldQuest.Quest)data).questType);
   //     //    actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, qts.declareWarWeight);
   //     //}

   //     //Opinions
   //     if(relationship.sharedOpinion > 0) {
   //         //- Add 1 Weight to Do Nothing per 1 Positive Point of Shared Opinion
   //         actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, relationship.sharedOpinion);
   //     } else if(relationship.sharedOpinion < 0) {
   //         //- Add 1 Weight to Declare War per 1 Negative Point of Shared Opinion
   //         actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, Mathf.Abs(relationship.sharedOpinion)); 
   //     }

   //     //Traits
   //     if(aggrievedFaction.leader != null) {
   //         for (int i = 0; i < aggrievedFaction.leader.traits.Count; i++) {
   //             Trait currTrait = aggrievedFaction.leader.traits[i];
   //             WeightedDictionary<INTERNATIONAL_INCIDENT_ACTION> traitWeight = currTrait
   //                 .GetInternationalIncidentReactionWeight(incidentType, relationship, aggressorFaction);
   //             if(traitWeight != null) {
   //                 actionWeights.AddElements(traitWeight);
   //             }
   //         }
   //     }

   //     //All
   //     //Add 50 Weight to Do Nothing if Ally
   //     if(relationship.relationshipStatus == RELATIONSHIP_STATUS.FRIENDLY) {
   //         actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 50);
   //     }

   //     //Add 100 Weight to Do Nothing per active wars I have
   //     actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 100 * aggrievedFaction.activeWars); 

   //     Relationship chieftainRel = CharacterManager.Instance.GetRelationshipBetween(aggressorFaction.leader, aggrievedFaction.leader);
   //     if(chieftainRel != null) {
   //         if (chieftainRel.totalValue > 0) {
   //             //Add 2 Weight to Do Nothing per Positive Opinion the Chieftain has towards the other Chieftain(if they have a relationship)
   //             actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DO_NOTHING, 2 * chieftainRel.totalValue);
   //         } else if(chieftainRel.totalValue < 0) {
   //             //Add 1 Weight to Declare per Negative Opinion the Chieftain has towards the other Chieftain(if they have a relationship)
   //             actionWeights.AddWeightToElement(INTERNATIONAL_INCIDENT_ACTION.DECLARE_WAR, Mathf.Abs(chieftainRel.totalValue));
   //         }
   //     }
   //     return actionWeights;
   // }
    public void DecalreWar(Faction faction1, Faction faction2, INTERNATIONAL_INCIDENT_TYPE reason, object data) {
        Debug.Log(faction1.name + " declares war on " + faction2.name);
        FactionRelationship rel = GetRelationshipBetween(faction1, faction2);
        rel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set relationship with each other as hostile
        rel.SetWarStatus(true);
        if (reason == INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH) {
			if(data is ECS.Character){
				Log declareWarLog = new Log(GameManager.Instance.Today(), "General", "Faction", "declare_war_character_death");
            declareWarLog.AddToFillers(faction1, faction1.name, LOG_IDENTIFIER.FACTION_1);
            declareWarLog.AddToFillers(faction2, faction2.name, LOG_IDENTIFIER.FACTION_2);
				ECS.Character character = (ECS.Character)data;
				declareWarLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            declareWarLog.AddToFillers(character.currLocation.region.centerOfMass.landmarkOnTile, character.currLocation.region.centerOfMass.landmarkOnTile.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
				//UIManager.Instance.ShowNotification(declareWarLog);
			}
        } 
   //     else {
			//if (data is OldQuest.Quest) {
			//	Log declareWarLog = new Log (GameManager.Instance.Today (), "General", "Faction", "declare_war_quest");
   //         declareWarLog.AddToFillers(faction1, faction1.name, LOG_IDENTIFIER.FACTION_1);
   //         declareWarLog.AddToFillers(faction2, faction2.name, LOG_IDENTIFIER.FACTION_2);
			//	OldQuest.Quest quest = (OldQuest.Quest)data;
			//	declareWarLog.AddToFillers (quest.assignedParty.partyLeader, quest.assignedParty.partyLeader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			//	declareWarLog.AddToFillers (quest, quest.questName, LOG_IDENTIFIER.OTHER);
   //         declareWarLog.AddToFillers(quest.assignedParty.currLocation.region.centerOfMass.landmarkOnTile, quest.assignedParty.currLocation.region.centerOfMass.landmarkOnTile.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
			//	UIManager.Instance.ShowNotification (declareWarLog);
			//}
   //     }
        

        /* When war is declared, Friends of the two Tribes have to determine what to do. 
         In determining which side to consider in case he is Friends with both, 
         choose the one that he has highest positive Opinion of. If same, randomize. */
        List<Faction> faction1Friends = faction1.GetMajorFactionsWithRelationshipStatus(RELATIONSHIP_STATUS.FRIENDLY);
        List<Faction> faction2Friends = faction2.GetMajorFactionsWithRelationshipStatus(RELATIONSHIP_STATUS.FRIENDLY);
        List<Faction> commonFriends = Utilities.Intersect(faction1Friends, faction2Friends);
        for (int i = 0; i < commonFriends.Count; i++) {
            Faction commonFriend = commonFriends[i];
            FactionRelationship relWithFac1 = commonFriend.GetRelationshipWith(faction1);
            FactionRelationship relWithFac2 = commonFriend.GetRelationshipWith(faction2);
            if(relWithFac1.sharedOpinion > relWithFac2.sharedOpinion) {
                //more friends with faction 1
                faction2Friends.Remove(commonFriend);
            } else if(relWithFac1.sharedOpinion < relWithFac2.sharedOpinion) {
                //more friends with faction 2
                faction1Friends.Remove(commonFriend);
            } else {
                //opinions are equal. Randomize
                if(UnityEngine.Random.Range(0,2) == 0) {
                    faction1Friends.Remove(commonFriend);
                } else {
                    faction2Friends.Remove(commonFriend);
                }
            }
        }

        for (int i = 0; i < faction1Friends.Count; i++) {
            Faction currAlly = faction1Friends[i];
            WeightedDictionary<ALLY_WAR_REACTION> allyReactionWeights = GetAllyWarReaction(currAlly, faction1, faction2);
            ALLY_WAR_REACTION chosenReaction = allyReactionWeights.PickRandomElementGivenWeights();
            switch (chosenReaction) {
                case ALLY_WAR_REACTION.JOIN_WAR:
                    //Join the war, side with faction 1
                    FactionRelationship enemyRel = currAlly.GetRelationshipWith(faction2);
                    enemyRel.SetWarStatus(true); //Set rel with faction 2 as at war
                    enemyRel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set rel with faction 2 as HOSTILE
                    ShowJoinWarLog(currAlly, faction2);
                    break;
                case ALLY_WAR_REACTION.BETRAY:
                    //Join the war, side with faction 2
                    FactionRelationship friendRel = currAlly.GetRelationshipWith(faction2);
                    friendRel.SetWarStatus(true); //Set rel with faction 1 as at war
                    friendRel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set rel with faction 1 as HOSTILE
                    ShowBetrayalLog(currAlly, faction1);
                    break;
                default:
                    break;
            }
        }

        for (int i = 0; i < faction2Friends.Count; i++) {
            Faction currAlly = faction2Friends[i];
            WeightedDictionary<ALLY_WAR_REACTION> allyReactionWeights = GetAllyWarReaction(currAlly, faction2, faction1);
            ALLY_WAR_REACTION chosenReaction = allyReactionWeights.PickRandomElementGivenWeights();
            switch (chosenReaction) {
                case ALLY_WAR_REACTION.JOIN_WAR:
                    //TODO: Join the war, side with faction 2
                    FactionRelationship enemyRel = currAlly.GetRelationshipWith(faction1);
                    enemyRel.SetWarStatus(true); //Set rel with faction 1 as at war
                    enemyRel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set rel with faction 1 as HOSTILE
                    ShowJoinWarLog(currAlly, faction1);
                    break;
                case ALLY_WAR_REACTION.BETRAY:
                    //TODO: Join the war, side with faction 1
                    FactionRelationship friendRel = currAlly.GetRelationshipWith(faction2);
                    friendRel.SetWarStatus(true); //Set rel with faction 2 as at war
                    friendRel.ChangeRelationshipStatus(RELATIONSHIP_STATUS.HOSTILE); //Set rel with faction 2 as HOSTILE
                    ShowBetrayalLog(currAlly, faction2);
                    break;
                default:
                    break;
            }
        }
    }
    private void ShowJoinWarLog(Faction faction, Faction enemy) {
        Log declareWarLog = new Log(GameManager.Instance.Today(), "General", "Faction", "declare_war_help_friend");
        declareWarLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
        declareWarLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.FACTION_2);
        //UIManager.Instance.ShowNotification(declareWarLog);
    }
    private void ShowBetrayalLog(Faction faction, Faction enemy) {
        Log declareWarLog = new Log(GameManager.Instance.Today(), "General", "Faction", "declare_war_betrayal");
        declareWarLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
        declareWarLog.AddToFillers(enemy, enemy.name, LOG_IDENTIFIER.FACTION_2);
        //UIManager.Instance.ShowNotification(declareWarLog);
    }
    private WeightedDictionary<ALLY_WAR_REACTION> GetAllyWarReaction(Faction faction, Faction friend, Faction enemy) {
        WeightedDictionary<ALLY_WAR_REACTION> actionWeights = new WeightedDictionary<ALLY_WAR_REACTION>();
        FactionRelationship relWithFriend = faction.GetRelationshipWith(friend);
        FactionRelationship relWithEnemy = faction.GetRelationshipWith(enemy);
        if (faction.leader != null) {
            for (int i = 0; i < faction.leader.traits.Count; i++) {
                Trait currTrait = faction.leader.traits[i];
                WeightedDictionary<ALLY_WAR_REACTION> traitDict = currTrait.GetAllyReactionWeight(friend, enemy);
                actionWeights.AddElements(traitDict);
            }
        }
        //All
        if(relWithFriend.sharedOpinion > 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.JOIN_WAR, 5 * relWithFriend.sharedOpinion); //+5 Weight to Join War for every Positive Opinion shared with friendly Tribe
        } else if(relWithFriend.sharedOpinion < 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.REMAIN_NEUTRAL, Mathf.Abs(2 * relWithFriend.sharedOpinion));//+2 Weight to Remain Neutral for every Negative Opinion shared with friendly Tribe
        }

        if (relWithEnemy.sharedOpinion < 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.JOIN_WAR, Mathf.Abs(5 * relWithEnemy.sharedOpinion)); //+5 Weight to Join War for every Negative Opinion shared with enemy Tribe
        } else if(relWithEnemy.sharedOpinion > 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.REMAIN_NEUTRAL, 2 * relWithEnemy.sharedOpinion); //+2 Weight to Remain Neutral for every Positive Opinion shared with enemy Tribe
        }

        int relativeStrTowardsEnemy = relWithEnemy.factionLookup[faction.id].relativeStrength;
        if (relativeStrTowardsEnemy > 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.JOIN_WAR, 2 * relativeStrTowardsEnemy);//+2 Weight to Join War for every positive point of Relative Strength I have over the enemy Tribe
        } else if(relativeStrTowardsEnemy < 0) {
            actionWeights.AddElement(ALLY_WAR_REACTION.REMAIN_NEUTRAL, Mathf.Abs(2 * relativeStrTowardsEnemy));//+2 Weight to Remain Neutral for every negative point of Relative Strength I have under the enemy Tribe
        }
        
        
        return actionWeights;
    }
    #endregion
}
