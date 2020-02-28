using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;
using UtilityScripts;

public class Settlement : IJobOwner {

    public int id { get; private set; }
    public LOCATION_TYPE locationType { get; private set; }
    public Region region { get; private set; }
    public LocationStructure prison { get; private set; }
    public LocationStructure mainStorage { get; private set; }
    public int citizenCount { get; private set; }

    //Data that are only referenced from this settlement's region
    //These are only getter data, meaning it cannot be stored
    public string name { get; private set; }
    public Faction owner { get; private set; }
    public Faction previousOwner { get; private set; }
    public Character ruler { get; private set; }
    public List<HexTile> tiles { get; private set; }
    public List<Character> residents { get; private set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; private set; }
    public bool isUnderSeige { get; private set; }

    //structures
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public InnerTileMap innerMap => region.innerMap;

    //misc
    public Sprite locationPortrait { get; private set; }
    public Vector2 nameplatePos { get; private set; }

    public List<JobQueueItem> availableJobs { get; protected set; }
    public JOB_OWNER ownerType { get { return JOB_OWNER.QUEST; } }

    public LocationClassManager classManager { get; private set; }
    public LocationEventManager eventManager { get; private set; }
    public LocationJobManager jobManager { get; private set; }

    private int newRulerDesignationChance;
    private WeightedDictionary<Character> newRulerDesignationWeights;
    private SettlementJobTriggerComponent _settlementJobTriggerComponent;
    
    #region getters
    public JobTriggerComponent jobTriggerComponent => settlementJobTriggerComponent;
    public SettlementJobTriggerComponent settlementJobTriggerComponent => _settlementJobTriggerComponent;
    #endregion

    public Settlement(Region region, LOCATION_TYPE locationType, int citizenCount) {
        this.region = region;
        SetName(RandomNameGenerator.GenerateCityName(RACE.HUMANS));
        id = UtilityScripts.Utilities.SetID(this);
        this.citizenCount = citizenCount;
        new List<Character>();
        tiles = new List<HexTile>();
        residents = new List<Character>();
        newRulerDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        ResetNewRulerDesignationChance();
        SetAreaType(locationType);
        // nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.coreTile);
        availableJobs = new List<JobQueueItem>();
        classManager = new LocationClassManager();
        eventManager = new LocationEventManager(this);
        jobManager = new LocationJobManager(this);
        _settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);

    }
    public Settlement(SaveDataArea saveDataArea) {
        region = GridMap.Instance.GetRegionByID(saveDataArea.regionID);
        SetName(RandomNameGenerator.GenerateCityName(RACE.HUMANS));
        id = UtilityScripts.Utilities.SetID(this, saveDataArea.id);
        citizenCount = saveDataArea.citizenCount;
        //charactersAtLocation = new List<Character>();
        tiles = new List<HexTile>();
        residents = new List<Character>();
        newRulerDesignationWeights = new WeightedDictionary<Character>();
        //itemsInArea = new List<SpecialToken>();
        //jobQueue = new JobQueue(null);
        ResetNewRulerDesignationChance();
        SetAreaType(saveDataArea.locationType);

        // nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.coreTile);

        LoadStructures(saveDataArea);
    }

    #region Listeners
    private void SubscribeToSignals() {
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.AddListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
        //Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        // Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        // Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        settlementJobTriggerComponent.SubscribeToListeners();
    }
    private void UnsubscribeToSignals() {
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.RemoveListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
        //Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        // Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        // Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        settlementJobTriggerComponent.UnsubscribeListeners();
    }
    #endregion

    #region Settlement Type
    public void SetAreaType(LOCATION_TYPE locationType) {
        this.locationType = locationType;
        OnAreaTypeSet();
    }
    public BASE_AREA_TYPE GetBaseAreaType() {
        AreaData data = LandmarkManager.Instance.GetAreaData(locationType);
        return data.baseAreaType;
    }
    private void OnAreaTypeSet() {
        //update tile visuals
        //for (int i = 0; i < tiles.Count; i++) {
        //    HexTile currTile = tiles[i];
        //    OnTileAddedToArea(currTile);
        //}
    }
    #endregion

    #region Visuals
    public void TintStructures(Color color) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            tile.SetStructureTint(color);

        }
    }
    public void SetLocationPortrait(Sprite portrait) {
        locationPortrait = portrait;
    }
    private void CreateNameplate() {
        //GameObject nameplateGO = UIManager.Instance.InstantiateUIObject("AreaNameplate", coreTile.tileLocation.landmarkOnTile.landmarkVisual.landmarkCanvas.transform);
        ////nameplateGO.transform.position = coreTile.transform.position;
        //nameplateGO.GetComponent<AreaNameplate>().SetArea(this);
        UIManager.Instance.CreateAreaNameplate(this);
    }
    #endregion

    #region Utilities
    public void SetName(string name) {
        this.name = name;
    }
    public void LoadAdditionalData() {
        // CreateNameplate();
    }
    /// <summary>
    /// Called when this settlement is set as the current active settlement.
    /// </summary>
    public void OnAreaSetAsActive() {
        SubscribeToSignals();
        //LocationStructure warehouse = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        // CheckAreaInventoryJobs(mainStorage, null);
        //DesignateNewRuler();
    }
    public void SetIsUnderSeige(bool state) {
        if(isUnderSeige != state) {
            isUnderSeige = state;
            Debug.Log($"{GameManager.Instance.TodayLogString()}{this.name} Under Siege state changed to {isUnderSeige.ToString()}");
            Messenger.Broadcast(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, this, isUnderSeige);
        }
    }
    private void OnTickEnded() {
        ProcessForcedCancelJobsOnTickEnded();
    }
    private void OnDayStarted() {
        ClearAllBlacklistToAllExistingJobs();
    }
    #endregion

    #region Supplies
    //public void AdjustSuppliesInBank(int amount) {
    //    if (supplyPile == null) {
    //        return;
    //    }
    //    supplyPile.AdjustResourceInPile(amount);
    //    Messenger.Broadcast(Signals.AREA_SUPPLIES_CHANGED, this);
    //    //suppliesInBank = Mathf.Clamp(suppliesInBank, 0, supplyCapacity);
    //}
    #endregion

    #region Food
    //public void SetFoodInBank(int amount) {
    //    FoodPile currFoodPile = foodPile;
    //    if (currFoodPile == null) {
    //        return;
    //    }
    //    currFoodPile.SetResourceInPile(amount);
    //    Messenger.Broadcast(Signals.AREA_FOOD_CHANGED, this);
    //}
    //public void AdjustFoodInBank(int amount) {
    //    FoodPile currFoodPile = foodPile;
    //    if (currFoodPile == null) {
    //        return;
    //    }
    //    currFoodPile.AdjustResourceInPile(amount);
    //    Messenger.Broadcast(Signals.AREA_FOOD_CHANGED, this);
    //}
    #endregion

    #region Characters
    public void AssignCharacterToDwellingInArea(Character character, IDwelling dwellingOverride = null) {
        if (structures == null) {
            Debug.LogWarning(
                $"{this.name} doesn't have any dwellings for {character.name} because structures have not been generated yet");
            return;
        }
        if (!character.isFactionless && !structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            Debug.LogWarning($"{this.name} doesn't have any dwellings for {character.name}");
            return;
        }
        if (character.isFactionless) {
            character.SetHomeStructure(null);
            return;
        }
        IDwelling chosenDwelling = dwellingOverride;
        if (chosenDwelling == null) {
            if (PlayerManager.Instance != null && PlayerManager.Instance.player != null && this.id == PlayerManager.Instance.player.playerSettlement.id) {
                chosenDwelling = structures[STRUCTURE_TYPE.DWELLING][0] as Dwelling; //to avoid errors, residents in player settlement will all share the same dwelling
            } else {
                Character lover = CharacterManager.Instance.GetCharacterByID(character.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (lover != null && lover.faction.id == character.faction.id && residents.Contains(lover)) { //check if the character has a lover that lives in the settlement
                    chosenDwelling = lover.homeStructure;
                }
            }
            if (chosenDwelling == null && (character.homeStructure == null || character.homeStructure.location.id != this.id)) { //else, find an unoccupied dwelling (also check if the character doesn't already live in this settlement)
                List<LocationStructure> structureList = structures[STRUCTURE_TYPE.DWELLING];
                for (int i = 0; i < structureList.Count; i++) {
                    Dwelling currDwelling = structureList[i] as Dwelling;
                    if (currDwelling.CanBeResidentHere(character)) {
                        chosenDwelling = currDwelling;
                        break;
                    }
                }
            }
        }

        if (chosenDwelling == null) {
            //if the code reaches here, it means that the settlement could not find a dwelling for the character
            Debug.LogWarning(
                $"{GameManager.Instance.TodayLogString()}Could not find a dwelling for {character.name} at {this.name}, setting home to Town Center");
            chosenDwelling = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER) as CityCenter;
        }
        character.MigrateHomeStructureTo(chosenDwelling);
    }
    public bool IsResidentsFull() {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSettlement.id == this.id) {
            return false; //resident capacity is never full for player settlement
        }
        if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            List<LocationStructure> dwellings = structures[STRUCTURE_TYPE.DWELLING];
            for (int i = 0; i < dwellings.Count; i++) {
                if (!dwellings[i].IsOccupied()) {
                    return false;
                }
            }
        }
        return true;
        //return structures[STRUCTURE_TYPE.DWELLING].Where(x => !x.IsOccupied()).Count() == 0; //check if there are still unoccupied dwellings
    }
    public int GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE structureType) {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSettlement.id == this.id) {
            return 0;
        }
        int num = 0;
        if (structures.ContainsKey(structureType)) {
            List<LocationStructure> structureList = structures[structureType];
            for (int i = 0; i < structureList.Count; i++) {
                if (!structureList[i].IsOccupied()) {
                    num++;
                }
            }
        }
        return num;
    }
    public void SetInitialResidentCount(int count) {
        citizenCount = count;
    }
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass) {
        if (character.homeSettlement == this) {
            classManager.OnResidentChangeClass(character, previousClass, currentClass);
        }
    }
    public Character AddNewResident(RACE race, Faction faction) {
        string className = classManager.GetCurrentClassToCreate();
        Character citizen = CharacterManager.Instance.CreateNewCharacter(className, race, UtilityScripts.Utilities.GetRandomGender(), faction, this);
        PlaceNewResidentInInnerMap(citizen);
        //citizen.CenterOnCharacter();
        return citizen;

        //if (className == "Leader") {
        //    citizen.LevelUp(leaderLevel - 1);
        //    SetLeader(leader);
        //} else {
        //    citizen.LevelUp(citizensLevel - 1);
        //}
    }
    public Character AddNewResident(RACE race, GENDER gender, Faction faction) {
        string className = classManager.GetCurrentClassToCreate();
        Character citizen = CharacterManager.Instance.CreateNewCharacter(className, race, gender, faction, this);
        PlaceNewResidentInInnerMap(citizen);
        //citizen.CenterOnCharacter();
        return citizen;
    }
    public Character AddNewResident(RACE race, GENDER gender, SEXUALITY sexuality, Faction faction) {
        string className = classManager.GetCurrentClassToCreate();
        Character citizen = CharacterManager.Instance.CreateNewCharacter(className, race, gender, sexuality, faction, this);
        PlaceNewResidentInInnerMap(citizen);
        //citizen.CenterOnCharacter();
        return citizen;
    }
    public Character CreateNewResidentNoLocation(RACE race, string className, Faction faction) {
        Character citizen = CharacterManager.Instance.CreateNewCharacter(className, race, UtilityScripts.Utilities.GetRandomGender(), faction);
        return citizen;
    }
    public void PlaceNewResidentInInnerMap(Character newResident) {
        LocationGridTile mainEntrance = innerMap.GetRandomUnoccupiedEdgeTile();
        newResident.CreateMarker();
        newResident.InitialCharacterPlacement(mainEntrance);
    }
    public bool AddResident(Character character, IDwelling chosenHome = null, bool ignoreCapacity = true) {
        if (!residents.Contains(character)) {
            if (!ignoreCapacity) {
                if (IsResidentsFull()) {
                    Debug.LogWarning(
                        $"{GameManager.Instance.TodayLogString()}Cannot add {character.name} as resident of {this.name} because residency is already full!");
                    return false; //settlement is at capacity
                }
            }
            if (!CanCharacterBeAddedAsResidentBasedOnFaction(character)) {
                character.logComponent.PrintLogIfActive(
                    $"{character.name} tried to become a resident of {name} but their factions conflicted");
                return false;
            }
            region.AddResident(character);
            residents.Add(character);
            if (character.race != RACE.DEMON && (character is Summon) == false) {
                classManager.OnAddResident(character);
            }
            // if(!coreTile.isCorrupted) {
            //     classManager.OnAddResident(character);
            // }
            AssignCharacterToDwellingInArea(character, chosenHome);
            return true;
        }
        return false;
    }
    public void RemoveResident(Character character) {
        if (residents.Remove(character)) {
            region.RemoveResident(character);
            if (character.homeStructure != null && character.homeSettlement == this) {
                character.homeStructure.RemoveResident(character);
            }
            if (character.race != RACE.DEMON) {
                classManager.OnRemoveResident(character);
            }
        }
    }
    private bool CanCharacterBeAddedAsResidentBasedOnFaction(Character character) {
        if (owner != null && character.faction != null) {
            //If character's faction is hostile with region's ruling faction, character cannot be a resident
            return !owner.IsHostileWith(character.faction);
        } else if (owner != null && character.faction == null) {
            //If character has no faction and region has faction, character cannot be a resident
            return false;
        }
        return true;
    }
    private void OnCharacterMissing(Character missingCharacter) {
        if (ruler != null && missingCharacter == ruler) {
            SetRuler(null);
        }
    }
    private void OnCharacterDied(Character deadCharacter) {
        if (ruler != null && deadCharacter == ruler) {
            SetRuler(null);
        }
        if(deadCharacter.homeSettlement == this) {
            if (!HasCanPerformOrAliveResidentInsideSettlement()) {
                Messenger.Broadcast(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, this);
            }
        }
    }
    public void SetRuler(Character newRuler) {
        if(ruler != null) {
            ruler.SetIsSettlementRuler(false);
        }
        ruler = newRuler;
        if(ruler != null) {
            ruler.SetIsSettlementRuler(true);
            //ResetNewRulerDesignationChance();
            if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForNewRulerDesignation);
            }
            Messenger.Broadcast(Signals.ON_SET_AS_SETTLEMENT_RULER, ruler);
        } else {
            Messenger.AddListener(Signals.HOUR_STARTED, CheckForNewRulerDesignation);
        }
    }
    private void CheckForNewRulerDesignation() {
        string debugLog =
            $"{GameManager.Instance.TodayLogString()}Checking for new settlement ruler designation for {name}";
        debugLog += $"\n-Chance: {newRulerDesignationChance}";
        int chance = UnityEngine.Random.Range(0, 100);
        debugLog += $"\n-Roll: {chance}";
        Debug.Log(debugLog);
        if (chance < newRulerDesignationChance) {
            DesignateNewRuler();
        } else {
            newRulerDesignationChance += 2;
        }
    }
    public void DesignateNewRuler(bool willLog = true) {
        string log =
            $"{GameManager.Instance.TodayLogString()}Designating a new settlement ruler for: {region.name}(chance it triggered: {newRulerDesignationChance})";
        newRulerDesignationWeights.Clear();
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            log += $"\n\n-{resident.name}";
            if(resident.isDead || resident.isMissing || resident.isBeingSeized) {
                log += "\nEither dead or missing or seized, will not be part of candidates for ruler";
                continue;
            }
            int weight = 50;
            log += "\n  -Base Weight: +50";
            if (resident.isFactionLeader) {
                weight += 100;
                log += "\n  -Faction Leader: +100";
            }
            if (resident.characterClass.className == "Noble") {
                weight += 40;
                log += "\n  -Noble: +40";
            }
            int numberOfFriends = 0;
            int numberOfEnemies = 0;
            for (int j = 0; j < resident.relationshipContainer.charactersWithOpinion.Count; j++) {
                Character otherCharacter = resident.relationshipContainer.charactersWithOpinion[j];
                if (otherCharacter.homeSettlement == this) {
                    if (otherCharacter.relationshipContainer.IsFriendsWith(resident)) {
                        numberOfFriends++;
                    }else if (otherCharacter.relationshipContainer.IsEnemiesWith(resident)) {
                        numberOfEnemies++;
                    }
                }
            }
            if(numberOfFriends > 0) {
                weight += (numberOfFriends * 20);
                log +=
                    $"\n  -Num of Friend/Close Friend in the Settlement: {numberOfFriends}, +{(numberOfFriends * 20)}";
            }
            if (resident.traitContainer.HasTrait("Inspiring")) {
                weight += 25;
                log += "\n  -Inspiring: +25";
            }
            if (resident.traitContainer.HasTrait("Authoritative")) {
                weight += 50;
                log += "\n  -Authoritative: +50";
            }


            if (numberOfEnemies > 0) {
                weight += (numberOfEnemies * -10);
                log += $"\n  -Num of Enemies/Rivals in the Settlement: {numberOfEnemies}, +{(numberOfEnemies * -10)}";
            }
            if (resident.traitContainer.HasTrait("Ugly")) {
                weight += -20;
                log += "\n  -Ugly: -20";
            }
            if (resident.hasUnresolvedCrime) {
                weight += -50;
                log += "\n  -Has Unresolved Crime: -50";
            }
            if (resident.traitContainer.HasTrait("Worker")) {
                weight += -40;
                log += "\n  -Civilian: -40";
            }
            if (resident.traitContainer.HasTrait("Ambitious")) {
                weight = Mathf.RoundToInt(weight * 1.5f);
                log += "\n  -Ambitious: x1.5";
            }
            if (weight < 1) {
                weight = 1;
                log += "\n  -Weight cannot be less than 1, setting weight to 1";
            }
            log += $"\n  -TOTAL WEIGHT: {weight}";
            if (weight > 0) {
                newRulerDesignationWeights.AddElement(resident, weight);
            }
        }
        if(newRulerDesignationWeights.Count > 0) {
            Character chosenRuler = newRulerDesignationWeights.PickRandomElementGivenWeights();
            if (chosenRuler != null) {
                log += $"\nCHOSEN RULER: {chosenRuler.name}";
                if (willLog) {
                    chosenRuler.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Settlement_Ruler, chosenRuler);
                } else {
                    SetRuler(chosenRuler);
                }
            } else {
                log += "\nCHOSEN RULER: NONE";
            }
        } else {
            log += "\nCHOSEN RULER: NONE";
        }
        ResetNewRulerDesignationChance();
        Debug.Log(log);
    }
    private void ResetNewRulerDesignationChance() {
        newRulerDesignationChance = 5;
    }
    // private void OnCharacterEnteredHexTile(Character character, HexTile enteredTile) {
    //     if (enteredTile.settlementOnTile == this) {
    //         //character has entered settlement
    //     }
    // }
    // private void OnCharacterExitedHexTile(Character character, HexTile exitedTile) {
    //     if (exitedTile.settlementOnTile == this) {
    //         //character has entered settlement
    //     }
    // }
    public List<Character> GetHostileCharactersInSettlement() {
        List<Character> hostileCharacters = new List<Character>();
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if (character.gridTileLocation.IsPartOfSettlement(this)
            && owner.IsHostileWith(character.faction)) {
                hostileCharacters.Add(character);
            }
        }
        return hostileCharacters;
    }
    public void GenerateInitialOpinionBetweenResidents() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident1 = residents[i];
            for (int j = 0; j < residents.Count; j++) {
                Character resident2 = residents[j];
                if (resident1 != resident2) {
                    IRelationshipData rel1Data =
                        resident1.relationshipContainer.GetOrCreateRelationshipDataWith(resident1, resident2);
                    IRelationshipData rel2Data =
                        resident2.relationshipContainer.GetOrCreateRelationshipDataWith(resident2, resident1);

                    int compatibilityValue;
                    if (rel1Data.opinions.compatibilityValue != -1) {
                        compatibilityValue = rel1Data.opinions.compatibilityValue;
                    } else if (rel2Data.opinions.compatibilityValue != -1) {
                        compatibilityValue = rel2Data.opinions.compatibilityValue;
                    } else {
                        compatibilityValue = UnityEngine.Random.Range(OpinionComponent.MinCompatibility,
                            OpinionComponent.MaxCompatibility);  
                    }
                    rel1Data.opinions.SetCompatibilityValue(compatibilityValue);
                    rel2Data.opinions.SetCompatibilityValue(compatibilityValue);
                    
                    rel1Data.opinions.RandomizeBaseOpinionBasedOnCompatibility();
                    rel2Data.opinions.RandomizeBaseOpinionBasedOnCompatibility();
                }
            }
        }
    }
    private void OnCharacterCanNoLongerPerform(Character character) {
        if (character.homeSettlement == this) {
            if (!HasCanPerformOrAliveResidentInsideSettlement()) {
                Messenger.Broadcast(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, this);
            }
        }
    }
    private bool HasCanPerformOrAliveResidentInsideSettlement() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if((resident.canPerform || !resident.isDead) 
                && resident.gridTileLocation != null 
                && resident.gridTileLocation.buildSpotOwner.hexTileOwner
                && resident.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile == this) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Tile Objects
    //public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
    //    if (!itemsInArea.Contains(token)) {
    //        itemsInArea.Add(token);
    //        token.SetOwner(this.owner);
    //        if (areaMap != null) { //if the settlement map of this settlement has already been created.
    //            //Debug.Log(GameManager.Instance.TodayLogString() + "Added " + token.name + " at " + name);
    //            if (structure != null) {
    //                structure.AddItem(token, gridLocation);
    //            } else {
    //                //get structure for token
    //                LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
    //                chosen.AddItem(token);
    //            }
    //            OnItemAddedToLocation(token, token.structureLocation);
    //        }
    //        Messenger.Broadcast(Signals.ITEM_ADDED_TO_AREA, this, token);
    //        return true;
    //    }
    //    return false;
    //}
    //public void RemoveSpecialTokenFromLocation(SpecialToken token) {
    //    if (itemsInArea.Remove(token)) {
    //        LocationStructure takenFrom = token.structureLocation;
    //        if (takenFrom != null) {
    //            takenFrom.RemoveItem(token);
    //            OnItemRemovedFromLocation(token, takenFrom);
    //        }
    //        //Debug.Log(GameManager.Instance.TodayLogString() + "Removed " + token.name + " from " + name);
    //        Messenger.Broadcast(Signals.ITEM_REMOVED_FROM_AREA, this, token);
    //    }
    //}
    //public bool IsItemInventoryFull() {
    //    return itemsInArea.Count >= MAX_ITEM_CAPACITY;
    //}
    //private int GetItemsInAreaCount(SPECIAL_TOKEN itemType) {
    //    int count = 0;
    //    for (int i = 0; i < itemsInArea.Count; i++) {
    //        SpecialToken currItem = itemsInArea[i];
    //        if (currItem.specialTokenType == itemType) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    public void OnItemAddedToLocation(TileObject item, LocationStructure structure) {
        CheckIfInventoryJobsAreStillValid(item, structure);
    }
    private void CheckIfInventoryJobsAreStillValid(TileObject item, LocationStructure structure) {
        if (structure == mainStorage && (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION || item.tileObjectType == TILE_OBJECT_TYPE.TOOL)) {
            if (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION) {
                if (mainStorage.GetBuiltTileObjectsOfType<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION).Count >= 2) {
                    List<JobQueueItem> jobs = GetJobs(JOB_TYPE.CRAFT_OBJECT);
                    for (int i = 0; i < jobs.Count; i++) {
                        JobQueueItem jqi = jobs[i];
                        if (jqi is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI is TileObject tileObject && tileObject.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION) {
                            jqi.ForceCancelJob(false, "Settlement has enough healing potions");    
                        }
                    }
                }
            } else if (item.tileObjectType == TILE_OBJECT_TYPE.TOOL) {
                if (mainStorage.GetBuiltTileObjectsOfType<TileObject>(TILE_OBJECT_TYPE.TOOL).Count >= 2) {
                    List<JobQueueItem> jobs = GetJobs(JOB_TYPE.CRAFT_OBJECT);
                    for (int i = 0; i < jobs.Count; i++) {
                        JobQueueItem jqi = jobs[i];
                        if (jqi is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI is TileObject tileObject && tileObject.tileObjectType == TILE_OBJECT_TYPE.TOOL) {
                            jqi.ForceCancelJob(false, "Settlement has enough tools");    
                        }
                    }
                }
            }
        }
    }
    public void OnItemRemovedFromLocation(TileObject item, LocationStructure structure) {
        CheckAreaInventoryJobs(structure, item);
    }
    public bool IsRequiredByLocation(TileObject item) {
        if (item.gridTileLocation != null && item.gridTileLocation.structure == mainStorage) {
            if (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION) {
                return mainStorage.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.HEALING_POTION) <= 2; //item is required by warehouse.
            } else if (item.tileObjectType == TILE_OBJECT_TYPE.TOOL) {
                return mainStorage.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.TOOL) <= 2; //item is required by warehouse.
            }
        }
        return false;
    }
    #endregion

    #region Structures
    public void GenerateStructures(int citizenCount) {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        //LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.CITY_CENTER, false);
        //LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WORK_AREA, false);
        //LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS, false);
        //LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.POND, false);
        
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.APOTHECARY, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.ASSASSIN_GUILD, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.BARRACKS, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.HUNTER_LODGE, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.MAGE_QUARTERS, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.MINER_CAMP, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.RAIDER_CAMP, true);
        // LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.SMITHY, true);

        LandmarkManager.Instance.CreateNewStructureAt(region, STRUCTURE_TYPE.CITY_CENTER, this);
        for (int i = 0; i < citizenCount; i++) {
            LandmarkManager.Instance.CreateNewStructureAt(region, STRUCTURE_TYPE.DWELLING, this);
        }
        AssignPrison();
    }
    public void GenerateStructures(params LocationStructure[] preCreatedStructures) {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        for (int i = 0; i < preCreatedStructures.Length; i++) {
            LocationStructure structure = preCreatedStructures[i];
            AddStructure(structure);
        }
    }
    private void LoadStructures(SaveDataArea data) {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();

        // for (int i = 0; i < data.structures.Count; i++) {
        //     LandmarkManager.Instance.LoadStructureAt(this, data.structures[i]);
        // }
        AssignPrison();
    }
    public void AddStructure(LocationStructure structure) {
        if (!structures.ContainsKey(structure.structureType)) {
            structures.Add(structure.structureType, new List<LocationStructure>());
        }

        if (!structures[structure.structureType].Contains(structure)) {
            structures[structure.structureType].Add(structure);
        }
    }
    public void RemoveStructure(LocationStructure structure) {
        if (structures.ContainsKey(structure.structureType)) {
            if (structures[structure.structureType].Remove(structure)) {

                if (structures[structure.structureType].Count == 0) { //this is only for optimization
                    structures.Remove(structure.structureType);
                }
            }
        }
    }
    public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            return structures[type][UtilityScripts.Utilities.rng.Next(0, structures[type].Count)];
        }
        return null;
    }
    public LocationStructure GetRandomStructure() {
        Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structuresDictionary = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>(this.structures);
        int dictIndex = UnityEngine.Random.Range(0, structuresDictionary.Count);
        int count = 0;
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in structuresDictionary) {
            if (count == dictIndex) {
                return kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
            }
            count++;
        }
        return null;
    }
    public LocationStructure GetStructureByID(STRUCTURE_TYPE type, int id) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> locStructures = structures[type];
            for (int i = 0; i < locStructures.Count; i++) {
                if(locStructures[i].id == id) {
                    return locStructures[i];
                }
            }
        }
        return null;
    }
    public List<LocationStructure> GetStructuresAtLocation() {
        List<LocationStructure> structuresAtLocation = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                structuresAtLocation.Add(currStructure);
            }
        }
        return structuresAtLocation;
    }
    public bool HasStructure(STRUCTURE_TYPE type) {
        return structures.ContainsKey(type);
    }
    public LocationStructure GetStructureOccupyingSpot(BuildingSpot spot) {
        foreach (KeyValuePair<STRUCTURE_TYPE,List<LocationStructure>> pair in structures) {
            for (int i = 0; i < pair.Value.Count; i++) {
                LocationStructure structure = pair.Value[i];
                if (structure.occupiedBuildSpot.spot == spot) {
                    return structure;
                }
            }
        }
        return null;
    }
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if(owner != null && character.gridTileLocation != null && 
           character.gridTileLocation.IsPartOfSettlement(this) && character.canPerform && character.canMove) {
            if (owner.IsHostileWith(character.faction)) {
                SetIsUnderSeige(true);
            }
        }
    }
    #endregion

    #region Inner Map
    public IEnumerator PlaceObjects() {
        //pre placed objects
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure structure = keyValuePair.Value[i];
                if (structure.structureObj != null) {
                    structure.structureObj.RegisterPreplacedObjects(structure, this.innerMap);    
                }
                yield return null;
            }
        }

        //place build spots
        PlaceBuildSpots();
        yield return null;
        
        PlaceResourcePiles();
        yield return null;
        

        //magic circle
        // if (structures.ContainsKey(STRUCTURE_TYPE.WILDERNESS)) {
        //     LocationStructure structure = structures[STRUCTURE_TYPE.WILDERNESS][0];
        //     structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MAGIC_CIRCLE));
        // }
    }
    private void PlaceBuildSpots() {
        for (int x = 0; x <= innerMap.buildingSpots.GetUpperBound(0); x++) {
            for (int y = 0; y <= innerMap.buildingSpots.GetUpperBound(1); y++) {
                BuildingSpot spot = innerMap.buildingSpots[x, y];
                if (spot.canBeBuiltOnByNPC) {
                    BuildSpotTileObject tileObj = new BuildSpotTileObject();
                    tileObj.SetBuildingSpot(spot);
                    LocationGridTile tileLocation = innerMap.map[spot.location.x, spot.location.y];
                    //if (tileLocation.objHere != null) {
                    //    tileLocation.structure.RemovePOI(tileLocation.objHere);
                    //}
                    tileLocation.structure.AddPOI(tileObj, tileLocation, false);
                    tileObj.SetGridTileLocation(tileLocation); //manually placed so that only the data of the build spot will be set, and the tile will not consider the build spot as objHere
                    if (tileLocation.structure.structureType != STRUCTURE_TYPE.WORK_AREA && tileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS) {
                        tileLocation.structure.SetOccupiedBuildSpot(tileObj);
                    }    
                }
            }
        }
    }
    private void PlaceResourcePiles() {
        if (structures.ContainsKey(STRUCTURE_TYPE.WAREHOUSE)) {
            mainStorage = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        } else {
            mainStorage = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        }
        WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
        woodPile.SetResourceInPile(100);
        mainStorage.AddPOI(woodPile);
        woodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.WOOD_PILE);

        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        stonePile.SetResourceInPile(100);
        mainStorage.AddPOI(stonePile);
        stonePile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.STONE_PILE);

        MetalPile metalPile = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(TILE_OBJECT_TYPE.METAL_PILE);
        metalPile.SetResourceInPile(100);
        mainStorage.AddPOI(metalPile);
        metalPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.METAL_PILE);

        FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.FOOD_PILE);
        foodPile.SetResourceInPile(100);
        mainStorage.AddPOI(foodPile);
        foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
    }
    private void AssignPrison() {
        if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
            return;
        }
        LocationStructure chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.PRISON);
        if (chosenPrison != null) {
            prison = chosenPrison;
        } else {
            chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.EXPLORE_AREA);
            if (chosenPrison != null) {
                prison = chosenPrison;
            } else {
                chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
                if (chosenPrison != null) {
                    prison = chosenPrison;
                } else {
                    prison = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                }
            }
        }
    }
    public void OnLocationStructureObjectPlaced(LocationStructure structure) {
        if (structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            //if a warehouse was placed, and this settlement does not yet have a main storage structure, or is using the city center as their main storage structure, then use the new warehouse instead.
            if (mainStorage == null || mainStorage.structureType == STRUCTURE_TYPE.CITY_CENTER) {
                SetMainStorage(structure);
            }
        } else if (structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            if (mainStorage == null) {
                SetMainStorage(structure);
            }
        }
    }
    private void SetMainStorage(LocationStructure structure) {
        bool shouldCheckResourcePiles = mainStorage != null && structure != null && mainStorage != structure;
        mainStorage = structure;
        if (shouldCheckResourcePiles) {
            Messenger.Broadcast(Signals.SETTLEMENT_CHANGE_STORAGE, this);
        }
    }
    #endregion

    #region POI
    public List<IPointOfInterest> GetPointOfInterestsOfType(POINT_OF_INTEREST_TYPE type) {
        List<IPointOfInterest> pois = new List<IPointOfInterest>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                pois.AddRange(keyValuePair.Value[i].GetPOIsOfType(type));
            }
        }
        return pois;
    }
    public List<TileObject> GetTileObjectsThatAdvertise(params INTERACTION_TYPE[] types) {
        List<TileObject> objs = new List<TileObject>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                objs.AddRange(keyValuePair.Value[i].GetTileObjectsThatAdvertise(types));
            }
        }
        return objs;
    }
    #endregion

    #region Jobs
    public void AddToAvailableJobs(JobQueueItem job, int position = -1) {
        if (position == -1) {
            availableJobs.Add(job);    
        } else {
            availableJobs.Insert(position, job);
        }
        jobManager.OnAddToAvailableJobs(job);
        if (job is GoapPlanJob goapJob) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob.ToString()} targeting {goapJob.targetPOI?.ToString()} was added to {this.name}'s available jobs");
        } else {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{job.ToString()} was added to {this.name}'s available jobs");    
        }
        
    }
    public bool RemoveFromAvailableJobs(JobQueueItem job) {
        if (availableJobs.Remove(job)) {
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob.ToString()} targeting {goapJob.targetPOI?.name} was removed from {this.name}'s available jobs");
            } else {
                Debug.Log($"{GameManager.Instance.TodayLogString()}{job.ToString()} was removed from {this.name}'s available jobs");    
            }
            OnJobRemovedFromAvailableJobs(job);
            return true;
        }
        return false;
    }
    //public bool RemoveFromAvailableJobs(JOB_TYPE jobType) {
    //    for (int i = 0; i < availableJobs.Count; i++) {
    //        if(availableJobs[i].jobType == jobType) {
    //            availableJobs.RemoveAt(i);
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public int GetNumberOfJobsWith(CHARACTER_STATE state) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i] is CharacterStateJob) {
                CharacterStateJob job = availableJobs[i] as CharacterStateJob;
                if (job.targetState == state) {
                    count++;
                }
            }
        }
        return count;
    }
    public int GetNumberOfJobsWith(JOB_TYPE type) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == type) {
                count++;
            }
        }
        return count;
    }
    public int GetNumberOfJobsWith(System.Func<JobQueueItem, bool> checker) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (checker.Invoke(availableJobs[i])) {
                count++;
            }
        }
        return count;
    }
    public bool HasJob(JobQueueItem job) {
        for (int i = 0; i < availableJobs.Count; i++) {
            if (job == availableJobs[i]) {
                return true;
            }
        }
        return false;
    }
    public bool HasJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (availableJobs[i].jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(GoapEffect effect, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (effect.conditionType == gpj.goal.conditionType
                    && effect.conditionKey == gpj.goal.conditionKey
                    && effect.target == gpj.goal.target
                    && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobWithOtherData(JOB_TYPE jobType, object otherData) {
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == jobType && availableJobs[i] is GoapPlanJob) {
                GoapPlanJob job = availableJobs[i] as GoapPlanJob;
                if (job.allOtherData != null) {
                    for (int j = 0; j < job.allOtherData.Count; j++) {
                        object data = job.allOtherData[j];
                        if (data == otherData) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public JobQueueItem GetJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                JobQueueItem job = availableJobs[i];
                if (job.jobType == jobTypes[j]) {
                    return job;
                }
            }
        }
        return null;
    }
    public List<JobQueueItem> GetJobs(params JOB_TYPE[] jobTypes) {
        List<JobQueueItem> jobs = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (jobTypes.Contains(job.jobType)) {
                jobs.Add(job);
            }
        }
        return jobs;
    }
    public JobQueueItem GetJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return gpj;
                }
            }
        }
        return null;
    }
    public bool AddFirstUnassignedJobToCharacterJob(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if(job.assignedCharacter == null && character.jobQueue.AddJobInQueue(job)) {
                return true;
            }
        }
        return false;
    }
    public JobQueueItem GetFirstUnassignedJobToCharacterJob(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && character.jobQueue.CanJobBeAddedToQueue(job)) {
                return job;
            }
        }
        return null;
    }
    public bool AssignCharacterToJobBasedOnVision(Character character) {
        List<JobQueueItem> choices = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI != null && character.marker.inVisionPOIs.Contains(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(job)) {
                    choices.Add(job);
                }
            }
        }
        if (choices.Count > 0) {
            JobQueueItem job = CollectionUtilities.GetRandomElement(choices);
            return character.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public JobQueueItem GetFirstJobBasedOnVision(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI != null && character.marker.inVisionPOIs.Contains(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(job)) {
                    return job;
                }
            }
        }
        return null;
    }
    private void CheckAreaInventoryJobs(LocationStructure affectedStructure, TileObject objectThatTriggeredChange) {
        if (affectedStructure == mainStorage && (objectThatTriggeredChange == null 
             || objectThatTriggeredChange.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION 
             || objectThatTriggeredChange.tileObjectType == TILE_OBJECT_TYPE.TOOL)) {
            //brew potion
            if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.HEALING_POTION) < 2) {
                //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION);
                item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                affectedStructure.AddPOI(item);

                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
                job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.HEALING_POTION).constructionCost });
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanBrewPotion);
                AddToAvailableJobs(job);
            }
            
            //craft tool
            if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.TOOL) < 2) {
                if (!HasJob(JOB_TYPE.CRAFT_OBJECT)) {
                    //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                    TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TOOL);
                    item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                    affectedStructure.AddPOI(item);

                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
                    job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.TOOL).constructionCost });
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCraftTool);
                    AddToAvailableJobs(job);
                }
            }
        }
    }
    private void OnJobRemovedFromAvailableJobs(JobQueueItem job) {
        jobManager.OnRemoveFromAvailableJobs(job);
        JobManager.Instance.OnFinishJob(job);
        if (job.jobType == JOB_TYPE.CRAFT_OBJECT) {
            CheckAreaInventoryJobs(mainStorage, null);
        }
    }
    //private void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
    //                    { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
    //    });
    //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeReplaceTileObjectJob);
    //    AddToAvailableJobs(job);
    //}
    private void ForceCancelAllJobsTargetingCharacter(IPointOfInterest target, string reason) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if(job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if(goapJob.targetPOI == target) {
                    if(goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ClearAllBlacklistToAllExistingJobs() {
        for (int i = 0; i < availableJobs.Count; i++) {
            availableJobs[i].ClearBlacklist();
        }
    }
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
        //RemoveFromAvailableJobs(job);
    }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        if (!job.IsJobStillApplicable()) {
            RemoveFromAvailableJobs(job);
        }
    }
    public bool ForceCancelJob(JobQueueItem job) {
        return RemoveFromAvailableJobs(job);
    }
    public void AddForcedCancelJobsOnTickEnded(JobQueueItem job) {
        if (!forcedCancelJobsOnTickEnded.Contains(job)) {
            forcedCancelJobsOnTickEnded.Add(job);
        }
    }
    public void ProcessForcedCancelJobsOnTickEnded() {
        if (forcedCancelJobsOnTickEnded.Count > 0) {
            for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
                forcedCancelJobsOnTickEnded[i].ForceCancelJob(false);
            }
            forcedCancelJobsOnTickEnded.Clear();
        }
    }
    #endregion

    #region Settlement Map
    //public void OnMapGenerationFinished() {
    //    //place tokens in settlement to actual structures.
    //    //get structure for token
    //    for (int i = 0; i < itemsInArea.Count; i++) {
    //        SpecialToken token = itemsInArea[i];
    //        LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
    //        chosen.AddItem(token);
    //        if (chosen.isInside) {
    //            token.SetOwner(this.owner);
    //        }
    //    }
    //}
    #endregion
    
    #region Faction
    public void SetOwner(Faction owner) {
        SetPreviousOwner(this.owner);
        this.owner = owner;
        /*Whenever a location is occupied, 
            all items in structures Inside Settlement will be owned by the occupying faction.*/
        // List<LocationStructure> insideStructures = GetStructuresAtLocation();
        // for (int i = 0; i < insideStructures.Count; i++) {
        //     insideStructures[i].OwnItemsInLocation(owner);
        // }
        Messenger.Broadcast(Signals.AREA_OWNER_CHANGED, this);
        
        bool isCorrupted = this.owner != null && this.owner.isPlayerFaction;
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            tile.SetCorruption(isCorrupted);
        }
        //TODO:
        // mainLandmark.landmarkNameplate.UpdateFactionEmblem();
        // regionTileObject?.UpdateAdvertisements(this);
    }
    public void SetPreviousOwner(Faction faction) {
        previousOwner = faction;
    }
    #endregion

    #region Tiles
    public void AddTileToSettlement(HexTile tile) {
        if (tiles.Contains(tile) == false) {
            tiles.Add(tile);
            tile.SetSettlementOnTile(this);
            if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
                tile.SetCorruption(true);
            }
            if (owner != null) {
                TintStructures(owner.factionColor);    
            }
            // tile.UpdateLandmarkVisuals();
        }
    }
    public void AddTileToSettlement(params HexTile[] tiles) {
        for (int i = 0; i < tiles.Length; i++) {
            HexTile tile = tiles[i];
            AddTileToSettlement(tile);
        }
    }
    public void RemoveTileFromSettlement(HexTile tile) {
        if (tiles.Remove(tile)) {
            tile.SetSettlementOnTile(null);
            if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
                tile.SetCorruption(false);
            }
        }
    }
    #endregion
    
    public override string ToString() {
        return name;
    }
}

[System.Serializable]
public struct IntRange {
    public int lowerBound;
    public int upperBound;
    
    public IntRange(int low, int high) {
        lowerBound = low;
        upperBound = high;
    }

    public void SetLower(int lower) {
        lowerBound = lower;
    }
    public void SetUpper(int upper) {
        upperBound = upper;
    }

    public int Random() {
        return UnityEngine.Random.Range(lowerBound, upperBound + 1);
    }

    public bool IsInRange(int value) {
        if (value >= lowerBound && value <= upperBound) {
            return true;
        }
        return false;
    }

    public bool IsNearUpperBound(int value) {
        int lowerBoundDifference = Mathf.Abs(value - lowerBound);
        int upperBoundDifference = Mathf.Abs(value - upperBound);
        if (upperBoundDifference < lowerBoundDifference) {
            return true;
        }
        return false;
    }
}