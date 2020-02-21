﻿using System;
using Inner_Maps;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public class CharacterManager : MonoBehaviour {

    public static CharacterManager Instance;

    [Header("Sub Managers")]
    [SerializeField] private CharacterClassManager classManager;

    public static readonly string[] sevenDeadlySinsClassNames = { "Lust", "Gluttony", "Greed", "Sloth", "Wrath", "Envy", "Pride" };
    public const int MAX_HISTORY_LOGS = 300;
    public GameObject characterIconPrefab;
    public Transform characterIconsParent;

    public int maxLevel;
    private List<CharacterAvatar> _allCharacterAvatars;
    [Header("Character Portrait Assets")]
    [SerializeField] private GameObject _characterPortraitPrefab;
    [SerializeField] private List<RacePortraitAssets> portraitAssets;
    [SerializeField] private RolePortraitFramesDictionary portraitFrames;
    [SerializeField] private StringSpriteDictionary classPortraits;
    public Material hsvMaterial;
    public Material spriteLightingMaterial;

    //TODO: Will move this once other hair assets arrive
    [SerializeField] private Sprite[] maleHairSprite;
    [SerializeField] private Sprite[] femaleHairSprite;
    [SerializeField] private Sprite[] maleKnockoutHairSprite;
    [SerializeField] private Sprite[] femaleKnockoutHairSprite;
    
    [Header("Character Marker Assets")]
    [SerializeField] private List<RaceMarkerAsset> markerAssets;
    [Header("Summon Settings")]
    [SerializeField] private SummonSettingDictionary summonSettings;
    [Header("Artifact Settings")]
    [SerializeField] private ArtifactSettingDictionary artifactSettings;
    
    public Dictionary<string, DeadlySin> deadlySins { get; private set; }
    public Dictionary<EMOTION, Emotion> emotionData { get; private set; }
    public List<Emotion> allEmotions { get; private set; }
    public int defaultSleepTicks { get; private set; } //how many ticks does a character must sleep per day?
    public SUMMON_TYPE[] summonsPool { get; private set; }
    public int CHARACTER_MISSING_THRESHOLD { get; private set; }
    public COMBAT_MODE[] combatModes { get; private set; }

    #region getters/setters
    public List<Character> allCharacters => characterDatabase.allCharactersList;
    public GameObject characterPortraitPrefab => _characterPortraitPrefab;
    private CharacterDatabase characterDatabase { get; set; }
    #endregion

    private void Awake() {
        Instance = this;
        _allCharacterAvatars = new List<CharacterAvatar>();
    }

    public void Initialize() {
        classManager.Initialize();
        CreateDeadlySinsData();
        defaultSleepTicks = GameManager.Instance.GetTicksBasedOnHour(8);
        CHARACTER_MISSING_THRESHOLD = GameManager.Instance.GetTicksBasedOnHour(72);
        summonsPool = new[] { SUMMON_TYPE.Wolf, SUMMON_TYPE.Golem, SUMMON_TYPE.Incubus, SUMMON_TYPE.Succubus };
        combatModes = new COMBAT_MODE[] { COMBAT_MODE.Aggressive, COMBAT_MODE.Passive, COMBAT_MODE.Defend };
        characterDatabase = new CharacterDatabase();
        ConstructEmotionData();
        Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }

    #region Characters
    public Character CreateNewLimboCharacter(RACE race, string className, GENDER gender, Faction faction = null,
    Settlement homeLocation = null, IDwelling homeStructure = null) {
        Character newCharacter = new Character(className, race, gender);
        newCharacter.SetIsLimboCharacter(true);
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter, false)) {
                FactionManager.Instance.friendlyNeutralFaction.JoinFaction(newCharacter, false);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter, false);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.region.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateInitialTraitsByClass();
        //newCharacter.CreateInitialTraitsByRace();
        AddNewLimboCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(string className, RACE race, GENDER gender, Faction faction = null, 
        Settlement homeLocation = null, IDwelling homeStructure = null) {
        Character newCharacter = new Character(className, race, gender);
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.friendlyNeutralFaction.JoinFaction(newCharacter);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.region.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateInitialTraitsByClass();
        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(string className, RACE race, GENDER gender, SEXUALITY sexuality, Faction faction = null,
        Settlement homeLocation = null, IDwelling homeStructure = null) {
        Character newCharacter = new Character(className, race, gender, sexuality);
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.friendlyNeutralFaction.JoinFaction(newCharacter);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.region.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateInitialTraitsByClass();
        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(SaveDataCharacter data) {
        Character newCharacter = new Character(data);
        newCharacter.CreateOwnParty();
        //for (int i = 0; i < data.alterEgos.Count; i++) {
        //    data.alterEgos[i].Load(newCharacter);
        //}

        Faction faction = FactionManager.Instance.GetFactionBasedOnID(data.factionID);
        if(faction != null) {
            faction.JoinFaction(newCharacter);
            if (data.isFactionLeader) {
                faction.OnlySetLeader(newCharacter);
            }
        }
        newCharacter.ownParty.CreateIcon();

        //TODO:
        // if (data.homeID != -1) {
        //     home = GridMap.Instance.GetRegionByID(data.homeID);
        // }
        Region currRegion = null;
        if (data.currentLocationID != -1) {
            currRegion = GridMap.Instance.GetRegionByID(data.currentLocationID);
        }
        if (currRegion != null) {
            newCharacter.ownParty.icon.SetPosition(currRegion.coreTile.transform.position);
        }
        // if (data.isDead) {
        //     if (home != null) {
        //         newCharacter.SetHomeRegion(home); //keep this data with character to prevent errors
        //         //home.AssignCharacterToDwellingInArea(newCharacter); //We do not save LocationStructure, so this is only done so that the dead character will not have null issues with homeStructure
        //     }
        //     if(currRegion != null) {
        //         newCharacter.SetRegionLocation(currRegion);
        //     }
        // } else {
        //     if (home != null) {
        //         newCharacter.MigrateHomeTo(home, null, false);
        //     }
        //     if (currRegion != null) {
        //         currRegion.AddCharacterToLocation(newCharacter.ownParty.owner, null, false);
        //     }
        // }
        // for (int i = 0; i < data.items.Count; i++) {
        //     data.items[i].Load(newCharacter);
        // }

        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(PreCharacterData data, string className, Faction faction = null,
        Settlement homeLocation = null, IDwelling homeStructure = null) {
        Character newCharacter = new Character(className, data.race, data.gender, data.sexuality, data.id);
        newCharacter.SetName(data.name);
        
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.friendlyNeutralFaction.JoinFaction(newCharacter);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.region.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateInitialTraitsByClass();
        AddNewCharacter(newCharacter);
        data.SetHasBeenSpawned();
        return newCharacter;
    }
    public void AddNewCharacter(Character character, bool broadcastSignal = true) {
        characterDatabase.AddCharacter(character);
        if (broadcastSignal) {
            Messenger.Broadcast(Signals.CHARACTER_CREATED, character);
        }
    }
    public void RemoveCharacter(Character character, bool broadcastSignal = true) {
        if (characterDatabase.RemoveCharacter(character)) {
            if (broadcastSignal) {
                Messenger.Broadcast(Signals.CHARACTER_REMOVED, character);
            }
        }
    }
    public void AddNewLimboCharacter(Character character) {
        characterDatabase.AddLimboCharacter(character);
        character.SetIsInLimbo(true);
    }
    public void RemoveLimboCharacter(Character character) {
        if (characterDatabase.RemoveLimboCharacter(character)) {
            character.SetIsInLimbo(false);
        }
    }
    public void AddCharacterAvatar(CharacterAvatar characterAvatar) {
        int centerOrderLayer = (_allCharacterAvatars.Count * 2) + 1;
        int frameOrderLayer = centerOrderLayer + 1;
        characterAvatar.SetFrameOrderLayer(frameOrderLayer);
        characterAvatar.SetCenterOrderLayer(centerOrderLayer);
        _allCharacterAvatars.Add(characterAvatar);
    }
    public void RemoveCharacterAvatar(CharacterAvatar characterAvatar) {
        _allCharacterAvatars.Remove(characterAvatar);
    }
    public void PlaceInitialCharacters(List<Character> characters, Settlement settlement) {
        for (int i = 0; i < characters.Count; i++) {
            Character character = characters[i];
            if (!character.marker) {
                character.CreateMarker();
            }
            if (character.homeStructure != null && character.homeStructure.settlementLocation == settlement) {
                //place the character at a random unoccupied tile in his/her home
                List<LocationGridTile> choices = character.homeStructure.unoccupiedTiles.Where(x => x.charactersHere.Count == 0).ToList();
                LocationGridTile chosenTile = choices[UnityEngine.Random.Range(0, choices.Count)];
                character.InitialCharacterPlacement(chosenTile);
            } else {
                //place the character at a random unoccupied tile in the settlement's wilderness
                LocationStructure wilderness = settlement.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                List<LocationGridTile> choices = wilderness.unoccupiedTiles.Where(x => x.charactersHere.Count == 0).ToList();
                LocationGridTile chosenTile = choices[UnityEngine.Random.Range(0, choices.Count)];
                character.InitialCharacterPlacement(chosenTile);
            }
        }
    }
    #endregion

    #region Character Class Manager
    public CharacterClass CreateNewCharacterClass(string className) {
        return classManager.CreateNewCharacterClass(className);
    }
    public string GetRandomClassByIdentifier(string identifier) {
        return classManager.GetRandomClassByIdentifier(identifier);
    }
    public bool HasCharacterClass(string className) {
        return classManager.classesDictionary.ContainsKey(className);
    }
    public CharacterClass GetCharacterClass(string className) {
        if (HasCharacterClass(className)) {
            return classManager.classesDictionary[className];
        }
        return null;
    }
    public List<CharacterClass> GetNormalCombatantClasses() {
        return classManager.normalCombatantClasses;
    }
    public System.Type[] GetClassBehaviourComponents(string className) {
        return classManager.GetClassBehaviourComponents(className);
    }
    //public System.Type[] GetTraitBehaviourComponents(string traitName) {
    //    return classManager.GetTraitBehaviourComponents(traitName);
    //}
    public CharacterBehaviourComponent GetCharacterBehaviourComponent(System.Type type) {
        return classManager.GetCharacterBehaviourComponent(type);
    }
    public string GetClassBehaviourComponentKey(string className) {
        return classManager.GetClassBehaviourComponentKey(className);
    }
    public List<CharacterClass> GetAllClasses() {
        return classManager.allClasses;
    }
    #endregion

    #region Summons
    public Summon CreateNewSummon(SUMMON_TYPE summonType, Faction faction = null, Settlement homeLocation = null, IDwelling homeStructure = null) {
        Summon newCharacter = CreateNewSummonClassFromType(summonType) as Summon;
        newCharacter.Initialize();
        if (faction != null) {
            faction.JoinFaction(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        newCharacter.ownParty.CreateIcon();
        if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
            homeLocation.region.AddCharacterToLocation(newCharacter.ownParty.owner);
        }
        newCharacter.CreateInitialTraitsByClass();
        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Summon CreateNewSummon(SaveDataCharacter data) {
        Summon newCharacter = CreateNewSummonClassFromType(data);
        newCharacter.CreateOwnParty();
        newCharacter.ConstructInitialGoapAdvertisementActions();

        //for (int i = 0; i < data.alterEgos.Count; i++) {
        //    data.alterEgos[i].Load(newCharacter);
        //}

        Faction faction = FactionManager.Instance.GetFactionBasedOnID(data.factionID);
        if (faction != null) {
            faction.JoinFaction(newCharacter);
            if (data.isFactionLeader) {
                faction.OnlySetLeader(newCharacter);
            }
        }

        newCharacter.ownParty.CreateIcon();
        // Settlement home = null;
        //TODO:
        // if (data.homeID != -1) {
        //     home = GridMap.Instance.GetRegionByID(data.homeID);
        // }

        Region currRegion = null;
        if (data.currentLocationID != -1) {
            currRegion = GridMap.Instance.GetRegionByID(data.currentLocationID);
        }
        if (currRegion != null) {
            newCharacter.ownParty.icon.SetPosition(currRegion.coreTile.transform.position);
        }
        // if (data.isDead) {
        //     if(home != null) {
        //         newCharacter.SetHomeRegion(home); //keep this data with character to prevent errors
        //         //home.AssignCharacterToDwellingInArea(newCharacter); //We do not save LocationStructure, so this is only done so that the dead character will not have null issues with homeStructure
        //     }
        //     if(currRegion != null) {
        //         newCharacter.SetRegionLocation(currRegion);
        //     }
        // } else {
        //     if (home != null) {
        //         newCharacter.MigrateHomeTo(home, null, false);
        //     }
        //     if (currRegion != null) {
        //         currRegion.AddCharacterToLocation(newCharacter.ownParty.owner, null, false);
        //     }
        // }

        // for (int i = 0; i < data.items.Count; i++) {
        //     data.items[i].Load(newCharacter);
        // }
        //for (int i = 0; i < data.normalTraits.Count; i++) {
        //    Character responsibleCharacter = null;
        //    Trait trait = data.normalTraits[i].Load(ref responsibleCharacter);
        //    newCharacter.AddTrait(trait, responsibleCharacter);
        //}
        //newCharacter.LoadAllStatsOfCharacter(data);

        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    private Summon CreateNewSummonClassFromType(SaveDataCharacter data) {
        switch (data.summonType) {
            case SUMMON_TYPE.Wolf:
                return new Wolf(data);
            case SUMMON_TYPE.ThiefSummon:
                return new ThiefSummon(data);
            case SUMMON_TYPE.Skeleton:
                return new Skeleton(data);
            case SUMMON_TYPE.Succubus:
                return new Succubus(data);
            case SUMMON_TYPE.Incubus:
                return new Incubus(data);
            case SUMMON_TYPE.Golem:
                return new Golem(data);
        }
        return null;
    }
    private object CreateNewSummonClassFromType(SUMMON_TYPE summonType) {
        var typeName = summonType.ToString();
        return System.Activator.CreateInstance(System.Type.GetType(typeName) ?? throw new Exception($"provided summon type was invalid! {typeName}"));
    }
    public SummonSettings GetSummonSettings(SUMMON_TYPE type) {
        return summonSettings[type];
    }
    public ArtifactSettings GetArtifactSettings(ARTIFACT_TYPE type) {
        return artifactSettings[type];
    }
    public void PlaceSummon(Summon summon, LocationGridTile locationTile) {
        summon.homeRegion.RemoveCharacterFromLocation(summon);
        summon.CreateMarker();
        summon.marker.InitialPlaceMarkerAt(locationTile);
        summon.OnPlaceSummon(locationTile);
    }
    #endregion

    #region Utilities
    public Character GetCharacterByID(int id) {
        if (characterDatabase.allCharacters.TryGetValue(id, out Character character)) {
            return character;
        } else if (characterDatabase.limboCharacters.TryGetValue(id, out character)) {
            return character;
        }
        return null;
    }
    public Character GetCharacterByName(string name) {
        for (int i = 0; i < characterDatabase.allCharactersList.Count; i++) {
            Character currChar = characterDatabase.allCharactersList[i];
            if (currChar.name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
                return currChar;
            }
        }
        return null;
    }
    public Character GetLimboCharacterByName(string name) {
        for (int i = 0; i < characterDatabase.limboCharacters.Count; i++) {
            Character currChar = characterDatabase.limboCharacters[i];
            if (currChar.name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)) {
                return currChar;
            }
        }
        return null;
    }
    #endregion

    #region Character Portraits
    public PortraitAssetCollection GetPortraitAssets(RACE race, GENDER gender) {
        for (int i = 0; i < portraitAssets.Count; i++) {
            RacePortraitAssets racePortraitAssets = portraitAssets[i];
            if (racePortraitAssets.race == race) {
                if (gender == GENDER.MALE) {
                    return racePortraitAssets.maleAssets;
                } else {
                    return racePortraitAssets.femaleAssets;
                }
            }
        }

        if (gender == GENDER.MALE) {
            return portraitAssets[0].maleAssets;
        } else {
            return portraitAssets[0].femaleAssets;
        }
    }
    public PortraitSettings GenerateRandomPortrait(RACE race, GENDER gender, string characterClass) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        PortraitSettings ps = new PortraitSettings();
        ps.race = race;
        ps.gender = gender;
        ps.wholeImage = classPortraits.ContainsKey(characterClass) ? characterClass : string.Empty;
        if (race == RACE.DEMON) {
            ps.head = -1;
            ps.brows = -1;
            ps.eyes = -1;
            ps.mouth = -1;
            ps.nose = -1;
            ps.hair = -1;
            ps.mustache = -1;
            ps.beard = -1;
            ps.hairColor = 0f;
            ps.wholeImageColor = UnityEngine.Random.Range(-144f, 144f);
        } else {
            ps.head = CollectionUtilities.GetRandomIndexInList(pac.head);
            ps.brows = CollectionUtilities.GetRandomIndexInList(pac.brows);
            ps.eyes = CollectionUtilities.GetRandomIndexInList(pac.eyes);
            ps.mouth = CollectionUtilities.GetRandomIndexInList(pac.mouth);
            ps.nose = CollectionUtilities.GetRandomIndexInList(pac.nose);

            if (UnityEngine.Random.Range(0, 100) < 10 && gender != GENDER.FEMALE) { //females have no chance to be bald
                ps.hair = -1; //chance to have no hair
            } else {
                ps.hair = CollectionUtilities.GetRandomIndexInList(pac.hair);
            }
            if (UnityEngine.Random.Range(0, 100) < 20) {
                ps.mustache = -1; //chance to have no mustache
            } else {
                ps.mustache = CollectionUtilities.GetRandomIndexInList(pac.mustache);
            }
            if (UnityEngine.Random.Range(0, 100) < 10) {
                ps.beard = -1; //chance to have no beard
            } else {
                ps.beard = CollectionUtilities.GetRandomIndexInList(pac.beard);
            }
            ps.hairColor = UnityEngine.Random.Range(-720f, 720f);
            ps.wholeImageColor = 0f;
        }
        return ps;
    }
    public PortraitFrame GetPortraitFrame(CHARACTER_ROLE role) {
        if (portraitFrames.ContainsKey(role)) {
            return portraitFrames[role];
        }
        throw new System.Exception($"There is no frame for role {role}");
    }
    public Sprite GetWholeImagePortraitSprite(string className) {
        if (classPortraits.ContainsKey(className)) {
            return classPortraits[className];
        }
        return null;
    }
    public bool TryGetPortraitSprite(string identifier, int index, RACE race, GENDER gender, out Sprite sprite) {
        if (index < 0) {
            sprite = null;
            return false;
        }
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        List<Sprite> listToUse;
        switch (identifier) {
            case "head":
                listToUse = pac.head;
                break;
            case "brows":
                listToUse = pac.brows;
                break;
            case "eyes":
                listToUse = pac.eyes;
                break;
            case "mouth":
                listToUse = pac.mouth;
                break;
            case "nose":
                listToUse = pac.nose;
                break;
            case "hair":
                listToUse = pac.hair;
                break;
            case "mustache":
                listToUse = pac.mustache;
                break;
            case "beard":
                listToUse = pac.beard;
                break;
            default:
                listToUse = null;
                break;
        }
        if (listToUse != null && listToUse.Count > index) {
            sprite = listToUse[index];
            return true;
        }
        sprite = null;
        return false;
    }
#if UNITY_EDITOR
    public void LoadCharacterPortraitAssets() {
        portraitAssets = new List<RacePortraitAssets>();
        string characterPortraitAssetPath = "Assets/Textures/Portraits/";
        string[] races = Directory.GetDirectories(characterPortraitAssetPath);

        //loop through races found in directory
        for (int i = 0; i < races.Length; i++) {
            string currRacePath = races[i];
            string raceName = new DirectoryInfo(currRacePath).Name.ToUpper();
            RACE race;
            if (System.Enum.TryParse(raceName, out race)) {
                RacePortraitAssets raceAsset = new RacePortraitAssets(race);
                //loop through genders found in races directory
                string[] genders = System.IO.Directory.GetDirectories(currRacePath);
                for (int j = 0; j < genders.Length; j++) {
                    string currGenderPath = genders[j];
                    string genderName = new DirectoryInfo(currGenderPath).Name.ToUpper();
                    GENDER gender;
                    if (System.Enum.TryParse(genderName, out gender)) {
                        PortraitAssetCollection assetCollection = raceAsset.GetPortraitAssetCollection(gender);
                        string[] faceParts = System.IO.Directory.GetDirectories(currGenderPath);
                        for (int k = 0; k < faceParts.Length; k++) {
                            string currFacePath = faceParts[k];
                            string facePartName = new DirectoryInfo(currFacePath).Name;
                            string[] facePartFiles = Directory.GetFiles(currFacePath);
                            for (int l = 0; l < facePartFiles.Length; l++) {
                                string facePartAssetPath = facePartFiles[l];
                                Sprite loadedSprite = (Sprite)UnityEditor.AssetDatabase.LoadAssetAtPath(facePartAssetPath, typeof(Sprite));
                                if (loadedSprite != null) {
                                    assetCollection.AddSpriteToCollection(facePartName, loadedSprite);
                                }
                            }
                        }
                    }
                }
                portraitAssets.Add(raceAsset);
            }
        }
    }
#endif
    #endregion

    #region Role
    public CharacterRole GetRoleByRoleType(CHARACTER_ROLE roleType) {
        CharacterRole[] characterRoles = CharacterRole.ALL;
        for (int i = 0; i < characterRoles.Length; i++) {
            if(characterRoles[i].roleType == roleType) {
                return characterRoles[i];
            }
        }
        return null;
    }
    #endregion

    #region Marker Assets
    public CharacterClassAsset GetMarkerAsset(RACE race, GENDER gender, string characterClassName) {
        for (int i = 0; i < markerAssets.Count; i++) {
            RaceMarkerAsset currRaceAsset = markerAssets[i];
            if (currRaceAsset.race == race) {
                GenderMarkerAsset asset = currRaceAsset.GetMarkerAsset(gender);
                if (asset.characterClassAssets.ContainsKey(characterClassName) == false) {
                    Debug.LogWarning($"There are no class assets for {characterClassName} {gender.ToString()} {race.ToString()}");
                    return null;
                }
                return asset.characterClassAssets[characterClassName];
            }
        }
        Debug.LogWarning($"There are no race assets for {characterClassName} {gender.ToString()} {race.ToString()}");
        return null;
    }
    public Sprite GetMarkerHairSprite(GENDER gender) {
        switch (gender) {
            case GENDER.MALE:
                return maleHairSprite[UnityEngine.Random.Range(0, maleHairSprite.Length)];
            case GENDER.FEMALE:
                return femaleHairSprite[UnityEngine.Random.Range(0, femaleHairSprite.Length)];
            default:
                return null;
        }
    }
    public Sprite GetMarkerKnockedOutHairSprite(GENDER gender) {
        switch (gender) {
            case GENDER.MALE:
                return maleKnockoutHairSprite[UnityEngine.Random.Range(0, maleHairSprite.Length)];
            case GENDER.FEMALE:
                return femaleKnockoutHairSprite[UnityEngine.Random.Range(0, femaleHairSprite.Length)];
            default:
                return null;
        }
    }
#if UNITY_EDITOR
    public void LoadCharacterMarkerAssets() {
        markerAssets = new List<RaceMarkerAsset>();
        string characterMarkerAssetPath = "Assets/Textures/Character Markers/";
        string[] races = Directory.GetDirectories(characterMarkerAssetPath);

        //loop through races found in directory
        for (int i = 0; i < races.Length; i++) {
            string currRacePath = races[i];
            string raceName = new DirectoryInfo(currRacePath).Name.ToUpper();
            RACE race;
            if (System.Enum.TryParse(raceName, out race)) {
                RaceMarkerAsset raceAsset = new RaceMarkerAsset(race);
                //loop through genders found in races directory
                string[] genders = System.IO.Directory.GetDirectories(currRacePath);
                for (int j = 0; j < genders.Length; j++) {
                    string currGenderPath = genders[j];
                    string genderName = new DirectoryInfo(currGenderPath).Name.ToUpper();
                    GENDER gender;
                    if (System.Enum.TryParse(genderName, out gender)) {
                        GenderMarkerAsset markerAsset = raceAsset.GetMarkerAsset(gender);
                        //loop through all folders found in gender directory. consider all these as character classes
                        string[] characterClasses = System.IO.Directory.GetDirectories(currGenderPath);
                        for (int k = 0; k < characterClasses.Length; k++) {
                            string currCharacterClassPath = characterClasses[k];
                            string className = new DirectoryInfo(currCharacterClassPath).Name;
                            string[] classFiles = Directory.GetFiles(currCharacterClassPath);
                            CharacterClassAsset characterClassAsset = new CharacterClassAsset();
                            markerAsset.characterClassAssets.Add(className, characterClassAsset);
                            for (int l = 0; l < classFiles.Length; l++) {
                                string classAssetPath = classFiles[l];
                                Sprite loadedSprite = (Sprite)UnityEditor.AssetDatabase.LoadAssetAtPath(classAssetPath, typeof(Sprite));
                                if (loadedSprite != null) {
                                    if (loadedSprite.name.Contains("_body")) {
                                        characterClassAsset.defaultSprite = loadedSprite;
                                    } else {
                                        //assume that sprite is for animation
                                        characterClassAsset.animationSprites.Add(loadedSprite);
                                    }
                                }

                            }
                        }
                    }
                }
                markerAssets.Add(raceAsset);
            }
        }
    }
#endif
    #endregion

    #region Listeners
    private void OnCharacterFinishedAction(ActualGoapNode node) {
        node.actor.marker.UpdateActionIcon();
        node.actor.marker.UpdateAnimation();

        //for (int i = 0; i < actor.marker.inVisionCharacters.Count; i++) {
        //    Character otherCharacter = actor.marker.inVisionCharacters[i];
        //    //crime system:
        //    //if the other character committed a crime,
        //    //check if that character is in this characters vision 
        //    //and that this character can react to a crime (not in flee or engage mode)
        //    if (action.IsConsideredACrimeBy(otherCharacter)
        //        && action.CanReactToThisCrime(otherCharacter)
        //        && otherCharacter.CanReactToCrime()) {
        //        bool hasRelationshipDegraded = false;
        //        otherCharacter.ReactToCrime(action, ref hasRelationshipDegraded);
        //    }
        //}
    }
    #endregion

    #region Deadly Sins
    private void CreateDeadlySinsData() {
        deadlySins = new Dictionary<string, DeadlySin>();
        for (int i = 0; i < sevenDeadlySinsClassNames.Length; i++) {
            deadlySins.Add(sevenDeadlySinsClassNames[i], CreateNewDeadlySin(sevenDeadlySinsClassNames[i]));
        }
    }
    private DeadlySin CreateNewDeadlySin(string deadlySin) {
        System.Type type = System.Type.GetType(deadlySin);
        if(type != null) {
            DeadlySin sin = System.Activator.CreateInstance(type) as DeadlySin;
            return sin;
        }
        return null;
    }
    public DeadlySin GetDeadlySin(string sinName) {
        if (deadlySins.ContainsKey(sinName)) {
            return deadlySins[sinName];
        }
        return null;
    }
    public bool CanDoDeadlySinAction(string deadlySinName, DEADLY_SIN_ACTION action) {
        return deadlySins[deadlySinName].CanDoDeadlySinAction(action);
    }
    #endregion

    #region POI
    public bool POIValueTypeMatching(POIValueType poi1, POIValueType poi2) {
        return poi1.id == poi2.id && poi1.poiType == poi2.poiType && poi1.tileObjectType == poi2.tileObjectType;
    }
    #endregion

    #region Emotions
    private void ConstructEmotionData() {
        emotionData = new Dictionary<EMOTION, Emotion>();
        this.allEmotions = new List<Emotion>();
        EMOTION[] enumValues = CollectionUtilities.GetEnumValues<EMOTION>();
        for (int i = 0; i < enumValues.Length; i++) {
            EMOTION emotion = enumValues[i];
            var typeName = UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(emotion.ToString());
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                Emotion data = System.Activator.CreateInstance(type) as Emotion;
                emotionData.Add(emotion, data);
                this.allEmotions.Add(data);
            } else {
                Debug.LogWarning($"{typeName} has no data!");
            }
        }
    }
    public string TriggerEmotion(EMOTION emotionType, Character emoter, IPointOfInterest target) {
        return $" {GetEmotion(emotionType).ProcessEmotion(emoter, target)}";
    }
    public Emotion GetEmotion(string name) {
        for (int i = 0; i < allEmotions.Count; i++) {
            if(allEmotions[i].name == name) {
                return allEmotions[i];
            }
        }
        return null;
    }
    public Emotion GetEmotion(EMOTION emotionType) {
        return emotionData[emotionType];
    }
    public bool EmotionsChecker(string emotion) {
        //NOTE: This is only temporary since in the future, we will not have the name of the emotion as the response
        string[] emotions = emotion.Split(' ');
        for (int i = 0; i < emotions.Length; i++) {
            Emotion _emotionData = GetEmotion(emotions[i]);
            if (_emotionData != null) {
                for (int j = 0; j < emotions.Length; j++) {
                    if(i != j && (emotions[i] == emotions[j] || !_emotionData.IsEmotionCompatibleWithThis(emotions[j]))){
                        return false;
                    }
                }
            }
        }
        return true;
    }
    #endregion
}

[System.Serializable]
public class PortraitFrame {
    public Sprite baseBG;
    public Sprite frameOutline;
}

[System.Serializable]
public struct SummonSettings {
    public Sprite summonPortrait;
}

[System.Serializable]
public struct ArtifactSettings {
    public Sprite artifactPortrait;
}
