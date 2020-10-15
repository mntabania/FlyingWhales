using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Characters.Behaviour;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public class CharacterManager : BaseMonoBehaviour {

    public static CharacterManager Instance;

    [Header("Sub Managers")]
    [SerializeField] private CharacterClassManager classManager;

    public static readonly string[] sevenDeadlySinsClassNames = { "Lust", "Gluttony", "Greed", "Sloth", "Wrath", "Envy", "Pride" };
    public const string Make_Love = "Make Love", Steal = "Steal", Poison_Food = "Poison Food",
        Place_Trap = "Place Trap", Flirt = "Flirt", Transform_To_Wolf = "Transform To Wolf", Drink_Blood = "Drink Blood",
        Destroy_Action = "Destroy";
    public const string Default_Resident_Behaviour = "Default Resident Behaviour",
        Default_Monster_Behaviour = "Default Monster Behaviour",
        Default_Minion_Behaviour = "Default Minion Behaviour",
        Default_Wanderer_Behaviour = "Default Wanderer Behaviour",
        Default_Angel_Behaviour = "Default Angel Behaviour",
        Ravager_Behaviour = "Ravager Behaviour",
        Kobold_Behaviour = "Kobold Behaviour",
        Giant_Spider_Behaviour = "Giant Spider Behaviour",
        Noxious_Wanderer_Behaviour = "Noxious Wanderer Behaviour",
        DeMooder_Behaviour = "DeMooder Behaviour",
        Defender_Behaviour = "Defender Behaviour",
        Invader_Behaviour = "Invader Behaviour",
        Disabler_Behaviour = "Disabler Behaviour",
        Infestor_Behaviour = "Infestor Behaviour",
        Abductor_Behaviour = "Abductor Behaviour",
        Arsonist_Behaviour = "Arsonist Behaviour",
        Abomination_Behaviour = "Abomination Behaviour",
        Small_Spider_Behaviour = "Small Spider Behaviour",
        Golem_Behaviour = "Golem Behaviour",
        Baby_Infestor_Behaviour = "Baby Infestor Behaviour",
        Vengeful_Ghost_Behaviour = "Vengeful Ghost Behaviour",
        Ghost_Behaviour = "Ghost Behaviour",
        Wurm_Behaviour = "Wurm Behaviour",
        Tower_Behaviour = "Tower Behaviour",
        Revenant_Behaviour = "Revenant Behaviour",
        Ent_Behaviour = "Ent Behaviour",
        Mimic_Behaviour = "Mimic Behaviour",
        Succubus_Behaviour = "Succubus Behaviour",
        Dragon_Behaviour = "Dragon Behaviour",
        Troll_Behaviour = "Troll Behaviour",
        Snatcher_Behaviour = "Snatcher Behaviour";
        

    public const int MAX_HISTORY_LOGS = 300;
    
    public GameObject characterIconPrefab;
    public Transform characterIconsParent;
    public bool lessenCharacterLogs;
    //private List<CharacterAvatar> _allCharacterAvatars;
    
    [Header("Character Portrait Assets")]
    [SerializeField] private GameObject _characterPortraitPrefab;
    [SerializeField] private List<RacePortraitAssets> portraitAssets;
    [SerializeField] private RolePortraitFramesDictionary portraitFrames;
    [SerializeField] private StringSpriteDictionary classPortraits;
    [SerializeField] private Vector3[] hairColors;
    public Material hsvMaterial;
    public Material hairUIMaterial;
    public Material spriteLightingMaterial;

    [Header("Character Marker Assets")]
    [SerializeField] private Sprite[] maleHairSprite;
    [SerializeField] private Sprite[] femaleHairSprite;
    [SerializeField] private Sprite[] maleKnockoutHairSprite;
    [SerializeField] private Sprite[] femaleKnockoutHairSprite;
    [SerializeField] private List<RaceMarkerAsset> markerAssets;
    [SerializeField] private List<AdditionalMarkerAsset> additionalMarkerAssets;

    [Header("Summon Settings")]
    [SerializeField] private SummonSettingDictionary summonSettings;
    [Header("Artifact Settings")]
    [SerializeField] private ArtifactSettingDictionary artifactSettings;
    [Header("Character Marker Effects")] 
    public Sprite webbedEffect;
    public Sprite stakeEffect;

    private Dictionary<string, DeadlySin> deadlySins { get; set; }
    private Dictionary<EMOTION, Emotion> emotionData { get; set; }
    private List<Emotion> allEmotions { get; set; }
    public int defaultSleepTicks { get; private set; } //how many ticks does a character must sleep per day?
    public SUMMON_TYPE[] summonsPool { get; private set; }
    public COMBAT_MODE[] combatModes { get; private set; }
    public List<string> rumorWorthyActions { get; private set; }
    public DemonicStructure currentDemonicStructureTargetOfAngels { get; private set; }
    public Character necromancerInTheWorld { get; private set; }
    public bool hasSpawnedNecromancerOnce { get; private set; }
    public int CHARACTER_MISSING_THRESHOLD { get; private set; }
    public int CHARACTER_PRESUMED_DEAD_THRESHOLD { get; private set; }

    private Dictionary<Type, CharacterBehaviourComponent> behaviourComponents;
    private readonly Dictionary<string, Type[]> defaultBehaviourSets = new Dictionary<string, Type[]>() {
        { Default_Resident_Behaviour,
            new []{
                typeof(DefaultFactionRelated),
                typeof(DefaultHomeless),
                typeof(WorkBehaviour),
                typeof(DefaultAtHome),
                typeof(DefaultOutside),
                typeof(DefaultBaseStructure),
                typeof(DefaultOtherStructure),
                typeof(DefaultExtraCatcher),
                typeof(MovementProcessing),
                typeof(DefaultOutsideHomeRegion),
            }
        },
        { Default_Monster_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(DefaultMonster),
                typeof(DefaultExtraCatcher),
            }
        },
        { Default_Minion_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(DefaultMinion),
                typeof(DefaultExtraCatcher),
            }
        },
        { Default_Wanderer_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(DefaultFactionRelated),
                typeof(DefaultHomeless),
                typeof(DefaultWanderer),
                typeof(DefaultExtraCatcher),
            }
        },
        { Default_Angel_Behaviour,
            new []{
                //typeof(AttackDemonicStructureBehaviour), //Removed this because SetIsAttackingDemonicStructure must still be called even on angels to activate certain calls, if we keep this, there will be double AttackDemonicStructureBehaviour in the character which must never happen!
                typeof(DefaultExtraCatcher),
            }
        },
        { Ravager_Behaviour,
            new []{
                typeof(WolfBehaviour),
                typeof(MovementProcessing),
                typeof(DefaultMonster),
                typeof(DefaultExtraCatcher),
            }
        },
        { Kobold_Behaviour,
            new []{
                typeof(KoboldBehaviour),
                typeof(MovementProcessing),
                typeof(DefaultMonster),
                typeof(DefaultExtraCatcher),
            }
        },
        { Giant_Spider_Behaviour,
            new []{
                typeof(GiantSpiderBehaviour),
                typeof(MovementProcessing),
                typeof(DefaultExtraCatcher),
            }
        },
        { Noxious_Wanderer_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(NoxiousWandererBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { DeMooder_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(DeMooderBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Defender_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(DefendBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Invader_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(InvadeBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Disabler_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(DisablerBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Infestor_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(InfestorBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Abductor_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(AbductorBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Arsonist_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(ArsonistBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Abomination_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(AbominationBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Small_Spider_Behaviour,
            new []{
                typeof(SmallSpiderBehaviour),
                typeof(MovementProcessing),
                typeof(DefaultExtraCatcher),
            }
        },
        { Golem_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(GolemBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Vengeful_Ghost_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(VengefulGhostBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Wurm_Behaviour,
            new []{
                typeof(WurmBehaviour),
            }
        },
        { Tower_Behaviour,
            new []{
                typeof(TowerBehaviour),
            }
        },
        { Ghost_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(GhostBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Revenant_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(RevenantBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Ent_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(EntBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Mimic_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(MimicBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Succubus_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(SuccubusBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Dragon_Behaviour,
            new []{
                typeof(DragonBehaviour),
            }
        },
        { Troll_Behaviour,
            new []{
                typeof(TrollBehaviour),
            }
        },
        { Baby_Infestor_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(BabyInfestorBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
        { Snatcher_Behaviour,
            new []{
                typeof(MovementProcessing),
                typeof(SnatcherBehaviour),
                typeof(DefaultExtraCatcher),
            }
        },
    };

    #region getters/setters
    public List<Character> allCharacters => DatabaseManager.Instance.characterDatabase.allCharactersList;
    public List<Character> limboCharacters => DatabaseManager.Instance.characterDatabase.limboCharactersList;
    public GameObject characterPortraitPrefab => _characterPortraitPrefab;
    #endregion

    private void Awake() {
        Instance = this;
        //_allCharacterAvatars = new List<CharacterAvatar>();
    }

    public void Initialize() {
        classManager.Initialize();
        CreateDeadlySinsData();
        defaultSleepTicks = GameManager.Instance.GetTicksBasedOnHour(8);
        CHARACTER_MISSING_THRESHOLD = GameManager.Instance.GetTicksBasedOnHour(24); //72
        CHARACTER_PRESUMED_DEAD_THRESHOLD = GameManager.Instance.GetTicksBasedOnHour(24); //72
        summonsPool = new[] { SUMMON_TYPE.Wolf, SUMMON_TYPE.Golem, SUMMON_TYPE.Incubus, SUMMON_TYPE.Succubus };
        combatModes = new COMBAT_MODE[] { COMBAT_MODE.Aggressive, COMBAT_MODE.Passive, COMBAT_MODE.Defend };
        rumorWorthyActions = new List<string>() { Make_Love, Steal, Poison_Food, Place_Trap, Flirt, Transform_To_Wolf, Drink_Blood, Destroy_Action };
        ConstructEmotionData();
        ConstructCharacterBehaviours();
        Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }

    #region Characters
    public Character CreateNewLimboCharacter(RACE race, string className, GENDER gender, Faction faction = null,
    NPCSettlement homeLocation = null, LocationStructure homeStructure = null) {
        Character newCharacter = new Character(className, race, gender);
        newCharacter.SetIsLimboCharacter(true);
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter, false)) {
                FactionManager.Instance.vagrantFaction.JoinFaction(newCharacter, false);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter, false);
        }
        if(homeStructure != null) {
            newCharacter.MigrateHomeStructureTo(homeStructure, false);
            homeStructure.region.AddCharacterToLocation(newCharacter);
        } else if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, null, false);
            homeLocation.region.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateRandomInitialTraits();
        newCharacter.CreateDefaultTraits();
        //newCharacter.CreateInitialTraitsByRace();
        AddNewLimboCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(string className, RACE race, GENDER gender, Faction faction = null,
        BaseSettlement homeLocation = null, Region homeRegion = null, LocationStructure homeStructure = null) {
        Character newCharacter = new Character(className, race, gender);
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.vagrantFaction.JoinFaction(newCharacter);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        if (homeStructure != null) {
            newCharacter.MigrateHomeStructureTo(homeStructure, false, true);
            homeStructure.region.AddCharacterToLocation(newCharacter);
        } else if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, null, false, true);
            if(homeLocation is NPCSettlement homeNPCSettlement) {
                homeNPCSettlement.region.AddCharacterToLocation(newCharacter);
            } else if (homeRegion != null) {
                homeRegion.AddResident(newCharacter);
                homeRegion.AddCharacterToLocation(newCharacter);
            }
        } else if (homeRegion != null) {
            homeRegion.AddResident(newCharacter);
            homeRegion.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateRandomInitialTraits();
        newCharacter.CreateDefaultTraits();
        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(string className, RACE race, GENDER gender, SEXUALITY sexuality, Faction faction = null,
        NPCSettlement homeLocation = null, LocationStructure homeStructure = null) {
        Character newCharacter = new Character(className, race, gender, sexuality);
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.vagrantFaction.JoinFaction(newCharacter);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        if (homeStructure != null) {
            newCharacter.MigrateHomeStructureTo(homeStructure, false, true);
            homeStructure.region.AddCharacterToLocation(newCharacter);
        } else if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, null, false, true);
            //homeLocation.region.AddResident(newCharacter);
            homeLocation.region.AddCharacterToLocation(newCharacter);
        }
        newCharacter.CreateRandomInitialTraits();
        newCharacter.CreateDefaultTraits();
        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Character CreateNewCharacter(SaveDataCharacter data) {
        Character newCharacter = new Character(data);
        if (newCharacter.isInLimbo) {
            AddNewLimboCharacter(newCharacter);
        } else {
            AddNewCharacter(newCharacter);
        }
        return newCharacter;
    }
    public Character CreateNewCharacter(PreCharacterData data, string className, Faction faction = null, NPCSettlement homeLocation = null, LocationStructure homeStructure = null) {
        Character newCharacter = new Character(className, data.race, data.gender, data.sexuality, data.id);
        newCharacter.SetName(data.name);
        
        newCharacter.Initialize();
        if (faction != null) {
            if (!faction.JoinFaction(newCharacter)) {
                FactionManager.Instance.vagrantFaction.JoinFaction(newCharacter);
            }
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        if (homeStructure != null) {
            newCharacter.MigrateHomeStructureTo(homeStructure, false, true);
            homeStructure.region.AddCharacterToLocation(newCharacter);
        } else if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, null, false, true);
            //homeLocation.region.AddResident(newCharacter);
            homeLocation.region.AddCharacterToLocation(newCharacter);
        }
        // newCharacter.CreateInitialTraits();
        newCharacter.CreateDefaultTraits();
        AddNewCharacter(newCharacter);
        data.SetHasBeenSpawned();
        return newCharacter;
    }
    public SaveDataCharacter CreateNewSaveDataCharacter(Character character) {
        SaveDataCharacter saveCharacter = Activator.CreateInstance(character.serializedData) as SaveDataCharacter;
        saveCharacter.Save(character);
        return saveCharacter;
    }
    public void AddNewCharacter(Character character, bool broadcastSignal = true) {
        DatabaseManager.Instance.characterDatabase.AddCharacter(character);
        if (broadcastSignal) {
            Messenger.Broadcast(Signals.CHARACTER_CREATED, character);
        }
    }
    public void RemoveCharacter(Character character, bool broadcastSignal = true) {
        if (DatabaseManager.Instance.characterDatabase.RemoveCharacter(character)) {
            if (broadcastSignal) {
                Messenger.Broadcast(Signals.CHARACTER_REMOVED, character);
            }
        }
    }
    public void AddNewLimboCharacter(Character character) {
        DatabaseManager.Instance.characterDatabase.AddLimboCharacter(character);
        character.SetIsInLimbo(true);
    }
    public void RemoveLimboCharacter(Character character) {
        if (DatabaseManager.Instance.characterDatabase.RemoveLimboCharacter(character)) {
            character.SetIsInLimbo(false);
        }
    }
    //public void AddCharacterAvatar(CharacterAvatar characterAvatar) {
    //    int centerOrderLayer = (_allCharacterAvatars.Count * 2) + 1;
    //    int frameOrderLayer = centerOrderLayer + 1;
    //    characterAvatar.SetFrameOrderLayer(frameOrderLayer);
    //    characterAvatar.SetCenterOrderLayer(centerOrderLayer);
    //    _allCharacterAvatars.Add(characterAvatar);
    //}
    //public void RemoveCharacterAvatar(CharacterAvatar characterAvatar) {
    //    _allCharacterAvatars?.Remove(characterAvatar);
    //}
    public void PlaceInitialCharacters(List<Character> characters, NPCSettlement npcSettlement) {
        for (int i = 0; i < characters.Count; i++) {
            Character character = characters[i];
            if (!character.marker) {
                character.CreateMarker();
            }
            if (character.homeStructure != null && character.homeStructure.settlementLocation == npcSettlement) {
                //place the character at a random unoccupied tile in his/her home
                List<LocationGridTile> choices = character.homeStructure.unoccupiedTiles.Where(x => x.charactersHere.Count == 0).ToList();
                LocationGridTile chosenTile = choices[UnityEngine.Random.Range(0, choices.Count)];
                character.InitialCharacterPlacement(chosenTile);
            } else {
                //place the character at a random unoccupied tile in the npcSettlement's wilderness
                LocationStructure wilderness = npcSettlement.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                List<LocationGridTile> choices = wilderness.unoccupiedTiles.Where(x => x.charactersHere.Count == 0).ToList();
                LocationGridTile chosenTile = choices[UnityEngine.Random.Range(0, choices.Count)];
                character.InitialCharacterPlacement(chosenTile);
            }
        }
    }
    public int GetFoodAmountTakenFromPOI(IPointOfInterest poi) {
        if (poi is Character character) {
            if (character.race == RACE.WOLF) {
                return 150;
            } else if (character.race == RACE.HUMANS) {
                return 200;
            } else if (character.race == RACE.ELVES) {
                return 200;
            } else {
                return 100;
            }
        }
        return 50;
    }
    public FoodPile CreateFoodPileForPOI(IPointOfInterest poi, LocationGridTile tileOverride = null, bool createLog = true) {
        LocationGridTile targetTile = tileOverride;
        Character deadCharacter = null;
        //determine if target is a character
        if (poi is Character character) {
            deadCharacter = character;
        } else if (poi is Tombstone tombstone) {
            deadCharacter = tombstone.character;
        }
        
        if(targetTile == null) {
            targetTile = poi.gridTileLocation;
        }
        if (targetTile != null && targetTile.objHere != null) {
            targetTile = targetTile.GetNearestUnoccupiedTileFromThis();
        }
        if(targetTile != null) {
            int food = GetFoodAmountTakenFromPOI(poi);

            //determine tile object type based on what poi to convert to food pile.
            TILE_OBJECT_TYPE tileObjectType;
            if (deadCharacter != null) {
                if (deadCharacter.isNormalCharacter) {
                    switch (deadCharacter.race) {
                        case RACE.HUMANS:
                            tileObjectType = TILE_OBJECT_TYPE.HUMAN_MEAT;
                            break;
                        case RACE.ELVES:
                            tileObjectType = TILE_OBJECT_TYPE.ELF_MEAT;
                            break;
                        default:
                            tileObjectType = TILE_OBJECT_TYPE.ANIMAL_MEAT;
                            break;
                    }
                } else {
                    tileObjectType = TILE_OBJECT_TYPE.ANIMAL_MEAT;
                }
            } else {
                tileObjectType = poi is Crops ? TILE_OBJECT_TYPE.VEGETABLES : TILE_OBJECT_TYPE.ANIMAL_MEAT;
            }

            if(poi != null) {
                FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(tileObjectType);
                if (poi.traitContainer.HasTrait("Infected")) {
                    foodPile.traitContainer.AddTrait(foodPile, "Infected");
                }
                if (poi.traitContainer.HasTrait("Abomination Germ")) {
                    //transfer abomination germ to created food pile
                    foodPile.traitContainer.AddTrait(foodPile, "Abomination Germ");
                    poi.traitContainer.RemoveStatusAndStacks(poi, "Abomination Germ");
                }

                if (deadCharacter != null && createLog) {
                    //add log if food pile came from character
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "became_food_pile", providedTags: LOG_TAG.Life_Changes);
                    log.AddToFillers(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(foodPile, foodPile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToDatabase();
                }

                foodPile.SetResourceInPile(food);
                targetTile.structure.AddPOI(foodPile, targetTile);
                return foodPile;
            }
        }
        return null;
    }
    public void RaiseFromDeath(Character characterToCopy, Faction faction, RACE race = RACE.SKELETON, string className = "") {
        if (!characterToCopy.hasRisen) {
            characterToCopy.SetHasRisen(true);
            StartCoroutine(Raise(characterToCopy, faction, race, className));
        }
    }
    private IEnumerator Raise(Character target, Faction faction, RACE race, string className) {
        target.marker.PlayAnimation("Raise Dead");
        yield return new WaitForSeconds(0.7f);
        Summon summon = CreateNewSummon(SUMMON_TYPE.Skeleton, faction, homeRegion: target.homeRegion, className: target.characterClass.className);
        summon.SetName(target.fullname);
        summon.CreateMarker();
        LocationGridTile tile = target.gridTileLocation;
        if (target.grave != null) {
            tile = target.grave.gridTileLocation;
            target.grave.gridTileLocation.structure.RemovePOI(target.grave);
            target.SetGrave(null);
        }
        summon.InitialCharacterPlacement(tile);
        target.DestroyMarker();
        // RemoveCharacter(target);
    }
    public List<Character> GetAllNormalCharacters() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < allCharacters.Count; i++) {
            Character character = allCharacters[i];
            if (character.isNormalCharacter) {
                characters.Add(character);
            }
        }
        return characters;
    }
    #endregion

    #region Character Class Manager
    public CharacterClass CreateNewCharacterClass(string className) {
        return classManager.CreateNewCharacterClass(className);
    }
    //public string GetRandomClassByIdentifier(string identifier) {
    //    return classManager.GetRandomClassByIdentifier(identifier);
    //}
    public string GetRandomCombatant() {
        return classManager.GetRandomCombatant().className;
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
    public List<CharacterClass> GetAllClasses() {
        return classManager.allClasses;
    }
    #endregion

    #region Behaviours
    private void ConstructCharacterBehaviours() {
        List<CharacterBehaviourComponent> allBehaviours = ReflectiveEnumerator.GetEnumerableOfType<CharacterBehaviourComponent>().ToList();
        behaviourComponents = new Dictionary<System.Type, CharacterBehaviourComponent>();
        for (int i = 0; i < allBehaviours.Count; i++) {
            CharacterBehaviourComponent behaviour = allBehaviours[i];
            behaviourComponents.Add(behaviour.GetType(), behaviour);
        }
    }
    public Type[] GetDefaultBehaviourSet(string setName) {
        if (defaultBehaviourSets.ContainsKey(setName)) {
            return defaultBehaviourSets[setName];
        }
        return null;
    }
    public bool HasDefaultBehaviourSet(string setName) {
        return defaultBehaviourSets.ContainsKey(setName);
    }
    //public System.Type[] GetTraitBehaviourComponents(string traitName) {
    //    return classManager.GetTraitBehaviourComponents(traitName);
    //}
    public CharacterBehaviourComponent GetCharacterBehaviourComponent(Type type) {
        if (behaviourComponents.ContainsKey(type)) {
            return behaviourComponents[type];
        }
        return null;
    }
    public T GetCharacterBehaviourComponent<T>(Type type) where T : CharacterBehaviourComponent {
        if (behaviourComponents.ContainsKey(type) && behaviourComponents[type] is T component) {
            return component;
        }
        return null;
    }
    #endregion

    #region Summons
    public Summon CreateNewLimboSummon(SUMMON_TYPE summonType, Faction faction = null, NPCSettlement homeLocation = null, LocationStructure homeStructure = null, string className = "") {
        Summon newCharacter = CreateNewSummonClassFromType(summonType, className);
        newCharacter.Initialize();
        if (faction != null) {
            faction.JoinFaction(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        if (homeStructure != null) {
            newCharacter.MigrateHomeStructureTo(homeStructure, false, true);
            homeStructure.region.AddCharacterToLocation(newCharacter);
        } else if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, null, false, true);
            homeLocation.region.AddCharacterToLocation(newCharacter);
        }

        //if (homeLocation != null) {
        //    newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
        //}
        //if (homeRegion != null) {
        //    homeRegion.AddResident(newCharacter);
        //    homeRegion.AddCharacterToLocation(newCharacter.ownParty.owner);
        //}
        newCharacter.CreateRandomInitialTraits();
        newCharacter.CreateDefaultTraits();
        AddNewLimboCharacter(newCharacter);
        return newCharacter;
    }
    public Summon CreateNewSummon(SUMMON_TYPE summonType, Faction faction = null, BaseSettlement homeLocation = null,
        Region homeRegion = null, LocationStructure homeStructure = null, string className = "") {
        Summon newCharacter = CreateNewSummonClassFromType(summonType, className);
        newCharacter.Initialize();
        if (faction != null) {
            faction.JoinFaction(newCharacter);
        } else {
            FactionManager.Instance.neutralFaction.JoinFaction(newCharacter);
        }
        if (homeStructure != null) {
            newCharacter.MigrateHomeStructureTo(homeStructure, false, true);
            homeStructure.region.AddCharacterToLocation(newCharacter);
        } else if (homeLocation != null) {
            newCharacter.MigrateHomeTo(homeLocation, null, false, true);
            if (homeLocation is NPCSettlement homeNPCSettlement) {
                homeNPCSettlement.region.AddCharacterToLocation(newCharacter);
            } else if (homeRegion != null) {
                homeRegion.AddResident(newCharacter);
                homeRegion.AddCharacterToLocation(newCharacter);
            }
        } else if (homeRegion != null) {
            homeRegion.AddResident(newCharacter);
            homeRegion.AddCharacterToLocation(newCharacter);
        }

        //if (homeLocation != null) {
        //    newCharacter.MigrateHomeTo(homeLocation, homeStructure, false);
        //}
        //if (homeRegion != null) {
        //    homeRegion.AddResident(newCharacter);
        //    homeRegion.AddCharacterToLocation(newCharacter.ownParty.owner);
        //}
        newCharacter.CreateRandomInitialTraits();
        newCharacter.CreateDefaultTraits();
        AddNewCharacter(newCharacter);
        return newCharacter;
    }
    public Summon CreateNewSummon(SaveDataSummon data) {
        Summon newCharacter = CreateNewSummonClassFromType(data) as Summon;
        if (newCharacter.isInLimbo) {
            AddNewLimboCharacter(newCharacter);
        } else {
            AddNewCharacter(newCharacter);
        }
        return newCharacter;
    }
    private Summon CreateNewSummonClassFromType(SaveDataSummon data) {
        var typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(data.summonType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        return Activator.CreateInstance(Type.GetType(typeName) ?? throw new Exception($"provided summon type was invalid! {typeName}"), data) as Summon;
    }
    private Summon CreateNewSummonClassFromType(SUMMON_TYPE summonType, string className) {
        var typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(summonType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        if(className != "") {
            return Activator.CreateInstance(Type.GetType(typeName) ?? throw new Exception($"provided summon type was invalid! {typeName}"), className) as Summon;
        }
        return Activator.CreateInstance(Type.GetType(typeName) ?? throw new Exception($"provided summon type was invalid! {typeName}")) as Summon;
    }
    public SummonSettings GetSummonSettings(SUMMON_TYPE type) {
        return summonSettings[type];
    }
    public ArtifactSettings GetArtifactSettings(ARTIFACT_TYPE type) {
        return artifactSettings[type];
    }
    public void PlaceSummon(Summon summon, LocationGridTile locationTile) {
        summon.currentRegion?.RemoveCharacterFromLocation(summon);
        summon.CreateMarker();    
        summon.marker.InitialPlaceMarkerAt(locationTile);
        summon.OnPlaceSummon(locationTile);
    }
    public void Teleport(Character character, LocationGridTile tile) {
        bool isCentered = character.marker && InnerMapCameraMove.Instance.target == character.marker.gameObject.transform;
        if (isCentered) {
            InnerMapCameraMove.Instance.CenterCameraOn(null);
        }
        if (character.currentRegion != tile.structure.region) {
            character.currentRegion?.RemoveCharacterFromLocation(character);
        }
        // character.DestroyMarker();
        // Debug.Log($"Will teleport to tile {tile.structure}");
        
        if (!character.marker) {
            character.CreateMarker();
            character.marker.InitialPlaceMarkerAt(tile);
        } else {
            character.marker.PlaceMarkerAt(tile);
        }
        character.marker.pathfindingAI.ClearAllCurrentPathData();
        character.marker.pathfindingAI.UpdateMe();
        if (isCentered) {
            character.CenterOnCharacter();
        }
    }
    public void SetCurrentDemonicStructureTargetOfAngels(DemonicStructure demonicStructure) {
        currentDemonicStructureTargetOfAngels = demonicStructure;
    }
    public void SetNewCurrentDemonicStructureTargetOfAngels() {
        LocationStructure targetDemonicStructure = null;
        if (InnerMapManager.Instance.HasExistingWorldKnownDemonicStructure()) {
            targetDemonicStructure = InnerMapManager.Instance.worldKnownDemonicStructures[UnityEngine.Random.Range(0, InnerMapManager.Instance.worldKnownDemonicStructures.Count)];
        } else {
            targetDemonicStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructure();
        }
        if(targetDemonicStructure != null) {
            SetCurrentDemonicStructureTargetOfAngels(targetDemonicStructure as DemonicStructure);
        } else {
            SetCurrentDemonicStructureTargetOfAngels(null);
        }
    }
    public TILE_OBJECT_TYPE GetEggType(SUMMON_TYPE summonType) {
        switch (summonType) {
            case SUMMON_TYPE.Giant_Spider:
                return TILE_OBJECT_TYPE.SPIDER_EGG;
            default:
                return TILE_OBJECT_TYPE.NONE;
        }
    }
    #endregion

    #region Utilities
    public Character GetCharacterByID(int id) {
        return DatabaseManager.Instance.characterDatabase.GetCharacterByID(id);
    }
    public Character GetCharacterByPersistentID(string id) {
        return DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id);
    }
    public Character GetCharacterByName(string name) {
        for (int i = 0; i < DatabaseManager.Instance.characterDatabase.allCharactersList.Count; i++) {
            Character currChar = DatabaseManager.Instance.characterDatabase.allCharactersList[i];
            if (currChar.name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
                return currChar;
            }
        }
        return null;
    }
    public Character GetLimboCharacterByName(string name) {
        for (int i = 0; i < DatabaseManager.Instance.characterDatabase.limboCharactersList.Count; i++) {
            Character currChar = DatabaseManager.Instance.characterDatabase.limboCharactersList[i];
            if (currChar.name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) {
                return currChar;
            }
        }
        return null;
    }
    public bool CanAddCharacterLogOrShowNotif(INTERACTION_TYPE actionType) {
        if (!lessenCharacterLogs) {
            return true;
        } else {
            if(actionType != INTERACTION_TYPE.SIT && actionType != INTERACTION_TYPE.STAND && actionType != INTERACTION_TYPE.RETURN_HOME && actionType != INTERACTION_TYPE.SLEEP
                && actionType != INTERACTION_TYPE.SLEEP_OUTSIDE && actionType != INTERACTION_TYPE.NAP) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Character Portraits
    private PortraitAssetCollection GetPortraitAssets(RACE race, GENDER gender) {
        for (int i = 0; i < portraitAssets.Count; i++) {
            RacePortraitAssets racePortraitAssets = portraitAssets[i];
            if (racePortraitAssets.race == race) {
                if (race.UsesGenderNeutralPortrait()) {
                    return racePortraitAssets.neutralAssets;
                } else {
                    return gender == GENDER.MALE ? racePortraitAssets.maleAssets : racePortraitAssets.femaleAssets;
                }
            }
        }

        if (gender == GENDER.MALE) {
            return portraitAssets[0].maleAssets;
        } else {
            return portraitAssets[0].femaleAssets;
        }
    }
    /// <summary>
    /// Update a given character's portrait. This function is used to update how a character
    /// should look, without changing all of their facial features.
    /// NOTE: This should not be relied on when the character changes Genders or Race. Since this will
    /// keep the character's original face.
    /// </summary>
    /// <param name="character">The character to update</param>
    /// <returns>The updated portrait settings.</returns>
    public PortraitSettings UpdatePortraitSettings(Character character) {
        PortraitSettings portraitSettings = GeneratePortrait(character);
        
        if (string.IsNullOrEmpty(portraitSettings.wholeImage)) {
            //keep the following settings from the original face.
            portraitSettings.head = character.visuals.portraitSettings.head;
            portraitSettings.brows = character.visuals.portraitSettings.brows;
            portraitSettings.eyes = character.visuals.portraitSettings.eyes;
            portraitSettings.mouth = character.visuals.portraitSettings.mouth;
            portraitSettings.nose = character.visuals.portraitSettings.nose;
            portraitSettings.hair = character.visuals.portraitSettings.hair;
            portraitSettings.hairColorHue = character.visuals.portraitSettings.hairColorHue;
            portraitSettings.hairColorSaturation = character.visuals.portraitSettings.hairColorValue;
            portraitSettings.hairColorValue = character.visuals.portraitSettings.hairColorValue;
            //if original portrait settings has beard then keep that.
            if (character.visuals.portraitSettings.beard != -1) {
                portraitSettings.beard = character.visuals.portraitSettings.beard;
            }
            //if original portrait settings has mustache then keep that.
            if (character.visuals.portraitSettings.mustache != -1) {
                portraitSettings.mustache = character.visuals.portraitSettings.mustache;
            }
        } 
        
        return portraitSettings;
    }
    public PortraitSettings GeneratePortrait(RACE race, GENDER gender, string characterClass, bool isLeader) {
        PortraitAssetCollection pac = GetPortraitAssets(race, gender);
        PortraitSettings ps = new PortraitSettings {
            race = race,
            gender = gender,
            wholeImage = classPortraits.ContainsKey(characterClass) ? characterClass : string.Empty
        };
        if (race == RACE.DEMON) {
            ps.head = -1;
            ps.brows = -1;
            ps.eyes = -1;
            ps.mouth = -1;
            ps.nose = -1;
            ps.hair = -1;
            ps.mustache = -1;
            ps.beard = -1;
            ps.hairColorHue = 0f;
            ps.wholeImageColor = Random.Range(-144f, 144f);
        } else {
            ps.head = CollectionUtilities.GetRandomIndexInList(pac.head);
            ps.brows = CollectionUtilities.GetRandomIndexInList(pac.brows);
            ps.eyes = CollectionUtilities.GetRandomIndexInList(pac.eyes);
            ps.mouth = CollectionUtilities.GetRandomIndexInList(pac.mouth);
            ps.nose = CollectionUtilities.GetRandomIndexInList(pac.nose);
            ps.ears = CollectionUtilities.GetRandomIndexInList(pac.ears);
            
            //NOTE: females and elves have no chance to be bald
            if (GameUtilities.RollChance(10) && gender != GENDER.FEMALE && race != RACE.ELVES) { 
                ps.hair = -1; //chance to have no hair
            } else {
                ps.hair = CollectionUtilities.GetRandomIndexInList(pac.hair);
            }
            
            //Only human male faction leaders/settlement rulers/nobles should have beards
            //Reference: https://trello.com/c/FOCAkDBN/1446-only-human-male-faction-leaders-settlement-rulers-nobles-should-have-beards
            if (gender == GENDER.MALE) {
                if (isLeader || characterClass == "Noble") {
                    ps.mustache = CollectionUtilities.GetRandomIndexInList(pac.mustache);
                    ps.beard = CollectionUtilities.GetRandomIndexInList(pac.beard);
                } else {
                    if (GameUtilities.RollChance(35)) {
                        ps.mustache = CollectionUtilities.GetRandomIndexInList(pac.mustache);
                    } else {
                        ps.mustache = -1; //chance to have no mustache
                    }
                    ps.beard = -1;
                }
            } else {
                ps.mustache = -1;
                ps.beard = -1;
            }

            Vector3 chosenHairColor = CollectionUtilities.GetRandomElement(hairColors);
            ps.hairColorHue = chosenHairColor.x;
            ps.hairColorSaturation = chosenHairColor.y;
            ps.hairColorValue = chosenHairColor.z;
            ps.wholeImageColor = 0f;
        }
        return ps;
    }
    public PortraitSettings GeneratePortrait(Character character) {
        return GeneratePortrait(character.race, character.gender, character.visuals.classToUseForVisuals, character.isFactionLeader || character.isSettlementRuler);
    }
    public PortraitFrame GetPortraitFrame(CHARACTER_ROLE role) {
        if (portraitFrames.ContainsKey(role)) {
            return portraitFrames[role];
        }
        throw new Exception($"There is no frame for role {role.ToString()}");
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
            case "ears":
                listToUse = pac.ears;
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
            if (Enum.TryParse(raceName, out RACE race)) {
                RacePortraitAssets raceAsset = new RacePortraitAssets(race);
                //loop through genders found in races directory
                string[] genders = Directory.GetDirectories(currRacePath);
                for (int j = 0; j < genders.Length; j++) {
                    string currGenderPath = genders[j];
                    string genderName = new DirectoryInfo(currGenderPath).Name.ToUpper();
                    PortraitAssetCollection assetCollection = null;
                    if (Enum.TryParse(genderName, out GENDER gender)) {
                        assetCollection = raceAsset.GetPortraitAssetCollection(gender);
                    } else if (genderName.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase)) {
                        assetCollection = raceAsset.neutralAssets;
                    }
                    Assert.IsNotNull(assetCollection, $"Asset portrait collection for {genderName} is null.");
                    string[] faceParts = Directory.GetDirectories(currGenderPath);
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
                var asset = race.UsesGenderNeutralMarkerAssets() ? currRaceAsset.neutralAssets : currRaceAsset.GetMarkerAsset(gender);
                if (asset.characterClassAssets.ContainsKey(characterClassName)) {
                    return asset.characterClassAssets[characterClassName];
                } else if (asset.characterClassAssets.ContainsKey("Default")) {
                    return asset.characterClassAssets["Default"];
                } else {
                    throw new Exception($"There are no class assets for {characterClassName} {gender.ToString()} {race.ToString()}");
                }
                
            }
        }
        throw new Exception($"There are no race assets for {characterClassName} {gender.ToString()} {race.ToString()}");
    }
    public CharacterClassAsset GetAdditionalMarkerAsset(string identifier) {
        for (int i = 0; i < additionalMarkerAssets.Count; i++) {
            AdditionalMarkerAsset asset = additionalMarkerAssets[i];
            if(asset.identifier == identifier) {
                return asset.asset;
            }
        }
        throw new Exception($"There are no additional assets for {identifier}");
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
            if (Enum.TryParse(raceName, out RACE race)) {
                RaceMarkerAsset raceAsset = new RaceMarkerAsset(race);
                //loop through genders found in races directory
                string[] genders = Directory.GetDirectories(currRacePath);
                for (int j = 0; j < genders.Length; j++) {
                    string currGenderPath = genders[j];
                    string genderName = new DirectoryInfo(currGenderPath).Name.ToUpper();
                    MarkerAsset markerAsset = null;
                    if (Enum.TryParse(genderName, out GENDER gender)) {
                        markerAsset = raceAsset.GetMarkerAsset(gender);    
                    } else if (genderName.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase)) {
                        markerAsset = raceAsset.neutralAssets;
                    } else {
                        throw new Exception($"No MarkerAsset class for {genderName}");
                    }
                    //loop through all folders found in gender directory. consider all these as character classes
                    string[] characterClasses = Directory.GetDirectories(currGenderPath);
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
                                if (loadedSprite.name.Contains("idle_1")) {
                                    characterClassAsset.defaultSprite = loadedSprite;
                                }
                                //assume that sprite is for animation
                                characterClassAsset.animationSprites.Add(loadedSprite);
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
        if (node.actor.marker) {
            node.actor.marker.UpdateActionIcon();
            node.actor.marker.UpdateAnimation();
        }
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
        Type type = Type.GetType($"{deadlySin}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        if(type != null) {
            DeadlySin sin = Activator.CreateInstance(type) as DeadlySin;
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
        allEmotions = new List<Emotion>();
        EMOTION[] enumValues = CollectionUtilities.GetEnumValues<EMOTION>();
        for (int i = 0; i < enumValues.Length; i++) {
            EMOTION emotion = enumValues[i];
            var typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(emotion.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = Type.GetType(typeName);
            if (type != null) {
                Emotion data = Activator.CreateInstance(type) as Emotion;
                emotionData.Add(emotion, data);
                allEmotions.Add(data);
            } else {
                Debug.LogWarning($"{typeName} has no data!");
            }
        }
    }
    public string TriggerEmotion(EMOTION emotionType, Character emoter, IPointOfInterest target, REACTION_STATUS status, ActualGoapNode action = null) {
        if (emoter.isNormalCharacter) {
            if (emoter.CanFeelEmotion(emotionType)) {
                return $" {GetEmotion(emotionType).ProcessEmotion(emoter, target, status, action)}";    
            }
            return string.Empty;
        } else {
            //Non villager characters cannot feel emotion
            return string.Empty;
        }
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

    #region Necromancer
    public void SetNecromancerInTheWorld(Character character) {
        if(necromancerInTheWorld != character) {
            necromancerInTheWorld = character;
            if (necromancerInTheWorld != null) {
                hasSpawnedNecromancerOnce = true;
                Messenger.Broadcast(Signals.NECROMANCER_SPAWNED, necromancerInTheWorld);
            }
        }
        
    }
    #endregion

    #region Minions
    public Minion CreateNewMinion(Character character, bool initialize = true, bool keepData = false) {
        Minion minion = new Minion(character, keepData);
        if (initialize) {
            InitializeMinion(minion);
        }
        return minion;
    }
    public Minion CreateNewMinion(string className, RACE race, bool initialize = true) {
        Player player = PlayerManager.Instance.player;
        Minion minion = new Minion(CreateNewCharacter(className, race, GENDER.MALE, player.playerFaction, player.playerSettlement, player.portalTile.region), false);
        if (initialize) {
            InitializeMinion(minion);
        }
        return minion;
    }
    public Minion CreateNewMinion(Character character, SaveDataMinion data) {
        Minion minion = new Minion(character, data);
        return minion;
    }
    private void InitializeMinion(Minion minion) { }
    #endregion

    #region Monobehaviours
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }
    #endregion

    #region Gathering
    private Gathering CreateNewGathering(GATHERING_TYPE type) {
        var typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(type.ToString())}Gathering, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        return Activator.CreateInstance(Type.GetType(typeName) ?? throw new Exception($"provided gathering type was invalid! {typeName}")) as Gathering ?? throw new Exception($"provided type not a gathering! {typeName}");
    }
    private SaveDataGathering CreateNewSaveDataGathering(Gathering gathering) {
        SaveDataGathering data = Activator.CreateInstance(gathering.serializedData) as SaveDataGathering;
        data.Save(gathering);
        return data;
    }
    public Gathering CreateNewGathering(SaveDataGathering data) {
        var typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(data.gatheringType.ToString())}Gathering, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        return Activator.CreateInstance(Type.GetType(typeName), data) as Gathering;
    }
    public Gathering CreateNewGathering(GATHERING_TYPE type, Character host) {
        Gathering newGathering = CreateNewGathering(type);
        newGathering.SetHost(host);
        return newGathering;
    }
    #endregion

}

[Serializable]
public class PortraitFrame {
    public Sprite baseBG;
}

[Serializable]
public struct SummonSettings {
    public Sprite summonPortrait;
}

[Serializable]
public struct ArtifactSettings {
    public Sprite artifactPortrait;
}
