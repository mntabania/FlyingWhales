using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Serialization;
using Traits;
using Archetype;
using Locations.Settlements;
using UnityEngine.Assertions;

public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance;
    public Player player;
    
    public Dictionary<SPELL_TYPE, SpellData> allSpellsData;
    public Dictionary<SPELL_TYPE, PlayerAction> allPlayerActionsData;
    public Dictionary<SPELL_TYPE, SpellData> allAfflictionsData;
    public COMBAT_ABILITY[] allCombatAbilities;

    [Header("Job Action Icons")]
    [FormerlySerializedAs("jobActionIcons")] [SerializeField] private StringSpriteDictionary spellIcons;

    [Header("Combat Ability Icons")]
    [SerializeField] private StringSpriteDictionary combatAbilityIcons;
    
    [Header("Intervention Ability Tiers")]
    [FormerlySerializedAs("interventionAbilityTiers")] [SerializeField] private InterventionAbilityTierDictionary spellTiers;

    [Header("Chaos Orbs")] 
    [SerializeField] private GameObject chaosOrbPrefab;

    private SPELL_TYPE[] allSpells = { SPELL_TYPE.METEOR
            , SPELL_TYPE.TORNADO, SPELL_TYPE.RAVENOUS_SPIRIT, SPELL_TYPE.FEEBLE_SPIRIT, SPELL_TYPE.FORLORN_SPIRIT
            , SPELL_TYPE.LIGHTNING, SPELL_TYPE.POISON_CLOUD, SPELL_TYPE.EARTHQUAKE
            , SPELL_TYPE.SPAWN_BOULDER, SPELL_TYPE.WATER_BOMB, SPELL_TYPE.MANIFEST_FOOD
            , SPELL_TYPE.BRIMSTONES, SPELL_TYPE.SPLASH_POISON, SPELL_TYPE.LOCUST_SWARM, SPELL_TYPE.BLIZZARD, SPELL_TYPE.RAIN
            , SPELL_TYPE.BALL_LIGHTNING, SPELL_TYPE.ELECTRIC_STORM, SPELL_TYPE.FROSTY_FOG, SPELL_TYPE.VAPOR, SPELL_TYPE.FIRE_BALL
            , SPELL_TYPE.POISON_BLOOM, SPELL_TYPE.LANDMINE, SPELL_TYPE.TERRIFYING_HOWL, SPELL_TYPE.FREEZING_TRAP, SPELL_TYPE.SNARE_TRAP, SPELL_TYPE.WIND_BLAST
            , SPELL_TYPE.ICETEROIDS, SPELL_TYPE.HEAT_WAVE, 
    };

    private SPELL_TYPE[] allPlayerActions = { SPELL_TYPE.ZAP, SPELL_TYPE.RAISE_DEAD, SPELL_TYPE.DESTROY, SPELL_TYPE.IGNITE, SPELL_TYPE.POISON
            , SPELL_TYPE.TORTURE, SPELL_TYPE.SUMMON_MINION, SPELL_TYPE.STOP, SPELL_TYPE.SEIZE_OBJECT, SPELL_TYPE.SEIZE_CHARACTER, SPELL_TYPE.SEIZE_MONSTER
            , SPELL_TYPE.RETURN_TO_PORTAL, SPELL_TYPE.RAID, SPELL_TYPE.HARASS, SPELL_TYPE.INVADE, SPELL_TYPE.LEARN_SPELL, SPELL_TYPE.CHANGE_COMBAT_MODE
            , SPELL_TYPE.BUILD_DEMONIC_STRUCTURE, SPELL_TYPE.AFFLICT, SPELL_TYPE.ACTIVATE_TILE_OBJECT, SPELL_TYPE.BREED_MONSTER
            , SPELL_TYPE.END_RAID, SPELL_TYPE.END_HARASS, SPELL_TYPE.END_INVADE, SPELL_TYPE.INTERFERE, SPELL_TYPE.PLANT_GERM
    };

    private SPELL_TYPE[] allAfflictions = { SPELL_TYPE.CANNIBALISM
            , SPELL_TYPE.LYCANTHROPY, SPELL_TYPE.VAMPIRISM, SPELL_TYPE.KLEPTOMANIA
            , SPELL_TYPE.UNFAITHFULNESS, SPELL_TYPE.CURSED_OBJECT, SPELL_TYPE.ALCOHOLIC
            , SPELL_TYPE.AGORAPHOBIA, SPELL_TYPE.PARALYSIS, SPELL_TYPE.ZOMBIE_VIRUS
            , SPELL_TYPE.PESTILENCE, SPELL_TYPE.PSYCHOPATHY, SPELL_TYPE.COWARDICE, SPELL_TYPE.PYROPHOBIA, SPELL_TYPE.NARCOLEPSY
    };

    private void Awake() {
        Instance = this;
    }
    public void Initialize() {
        // SPELL_TYPE[] allSpellTypes = UtilityScripts.CollectionUtilities.GetEnumValues<SPELL_TYPE>();
        ConstructAllSpellsData();
        ConstructAllPlayerActionsData();
        ConstructAllAfflictionsData();
        //Unit Selection
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        // Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressedDown);
        Messenger.AddListener<Vector3, int, InnerTileMap>(Signals.CREATE_CHAOS_ORBS, CreateChaosOrbsAt);
        Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DID_ACTION_SUCCESSFULLY, OnCharacterDidActionSuccess);
    }
    public void InitializePlayer(BaseLandmark portal, LocationStructure portalStructure, PLAYER_ARCHETYPE archeType) {
        player = new Player();
        player.CreatePlayerFaction();
        player.SetPortalTile(portal.tileLocation);
        player.SetArchetype(archeType);
        PlayerSettlement existingPlayerNpcSettlement = portal.tileLocation.settlementOnTile as PlayerSettlement;
        Assert.IsNotNull(existingPlayerNpcSettlement, $"Portal does not have a player settlement on its tile");
        player.SetPlayerArea(existingPlayerNpcSettlement);
        
        LandmarkManager.Instance.OwnSettlement(player.playerFaction, existingPlayerNpcSettlement);
        
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Grasping_Hands));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Snatching_Hands));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Abominable_Heart));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Dark_Matter));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Looking_Glass));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Black_Scripture));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.False_Gem));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Naga_Eyes));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Tormented_Chalice));
        //player.AddArtifact(CreateNewArtifact(ARTIFACT_TYPE.Lightning_Rod));
        
        PlayerUI.Instance.UpdateUI();
    }
    public void InitializePlayer(SaveDataPlayer data) {
        player = new Player(data);
        player.CreatePlayerFaction(data);
        // NPCSettlement existingPlayerNpcSettlement = LandmarkManager.Instance.GetAreaByID(data.playerAreaID);
        // player.SetPlayerArea(existingPlayerNpcSettlement);
        //PlayerUI.Instance.UpdateUI();
        //PlayerUI.Instance.InitializeThreatMeter();
        //PlayerUI.Instance.UpdateThreatMeter();

        for (int i = 0; i < data.minions.Count; i++) {
            data.minions[i].Load(player);
        }
        //for (int i = 0; i < data.summonSlots.Count; i++) {
        //    Summon summon = CharacterManager.Instance.GetCharacterByID(data.summonIDs[i]) as Summon;
        //    player.GainSummon(summon);
        //}
        //for (int i = 0; i < data.artifacts.Count; i++) {
        //    data.artifacts[i].Load(player);
        //}
        //for (int i = 0; i < data.interventionAbilities.Count; i++) {
        //    data.interventionAbilities[i].Load(player);
        //}
        for (int i = 0; i < player.minions.Count; i++) {
            if(player.minions[i].character.id == data.currentMinionLeaderID) {
                player.SetMinionLeader(player.minions[i]);
            }
        }
        //player.SetPlayerTargetFaction(LandmarkManager.Instance.enemyOfPlayerArea.owner);
    }
    public int GetManaCostForSpell(int tier) {
        if (tier == 1) {
            return 150;
        } else if (tier == 2) {
            return 100;
        } else {
            return 50;
        }
    }

    #region Utilities
    private void ConstructAllSpellsData() {
        allSpellsData = new Dictionary<SPELL_TYPE, SpellData>();
        for (int i = 0; i < allSpells.Length; i++) {
            SPELL_TYPE spellType = allSpells[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";
                allSpellsData.Add(spellType, System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as SpellData);
            }
        }
    }
    private void ConstructAllPlayerActionsData() {
        allPlayerActionsData = new Dictionary<SPELL_TYPE, PlayerAction>();
        for (int i = 0; i < allPlayerActions.Length; i++) {
            SPELL_TYPE spellType = allPlayerActions[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";
                allPlayerActionsData.Add(spellType, System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as PlayerAction);
            }
        }
    }
    private void ConstructAllAfflictionsData() {
        allAfflictionsData = new Dictionary<SPELL_TYPE, SpellData>();
        for (int i = 0; i < allAfflictions.Length; i++) {
            SPELL_TYPE spellType = allAfflictions[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";
                allAfflictionsData.Add(spellType, System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as SpellData);
            }
        }
    }
    public Sprite GetJobActionSprite(string actionName) {
        if (spellIcons.ContainsKey(actionName)) {
            return spellIcons[actionName];
        }
        return null;
    }
    public Sprite GetCombatAbilitySprite(string abilityName) {
        if (combatAbilityIcons.ContainsKey(abilityName)) {
            return combatAbilityIcons[abilityName];
        }
        return null;
    }
    #endregion

    #region Intervention Ability
    public PlayerSpell CreateNewInterventionAbility(SPELL_TYPE abilityType) {
        switch (abilityType) {
            //case SPELL_TYPE.ABDUCT:
            //    return new Abduct();
            //case SPELL_TYPE.ACCESS_MEMORIES:
            //    return new AccessMemories();
            //case SPELL_TYPE.DESTROY:
            //    return new Destroy();
            //case SPELL_TYPE.DISABLE:
            //    return new Disable();
            //case SPELL_TYPE.ENRAGE:
            //    return new Enrage();
            case SPELL_TYPE.KLEPTOMANIA:
                return new Kleptomania();
            case SPELL_TYPE.LYCANTHROPY:
                return new Lycanthropy();
            case SPELL_TYPE.UNFAITHFULNESS:
                return new Unfaithfulness();
            case SPELL_TYPE.VAMPIRISM:
                return new Vampirism();
            //case SPELL_TYPE.JOLT:
            //    return new Jolt();
            //case SPELL_TYPE.PROVOKE:
            //    return new Provoke();
            //case SPELL_TYPE.RAISE_DEAD:
            //    return new RaiseDead();
            //case INTERVENTION_ABILITY.SHARE_INTEL:
            //    return new ShareIntel();
            //case SPELL_TYPE.SPOOK:
            //    return new Spook();
            //case SPELL_TYPE.ZAP:
            //    return new Zap();
            case SPELL_TYPE.CANNIBALISM:
                return new Cannibalism();
            //case SPELL_TYPE.CLOAK_OF_INVISIBILITY:
            //    return new CloakOfInvisibility();
            //case SPELL_TYPE.LURE:
            //    return new Lure();
            case SPELL_TYPE.METEOR:
                return new Meteor();
            //case SPELL_TYPE.IGNITE:
            //    return new Ignite();
            case SPELL_TYPE.CURSED_OBJECT:
                return new CursedObject();
            //case SPELL_TYPE.SPOIL:
            //    return new Spoil();
            case SPELL_TYPE.ALCOHOLIC:
                return new Alcoholic();
            //case SPELL_TYPE.LULLABY:
            //    return new Lullaby();
            case SPELL_TYPE.PESTILENCE:
                return new Pestilence();
            case SPELL_TYPE.AGORAPHOBIA:
                return new Agoraphobia();
            case SPELL_TYPE.PARALYSIS:
                return new Paralysis();
            //case SPELL_TYPE.RELEASE:
            //    return new Release();
            case SPELL_TYPE.ZOMBIE_VIRUS:
                return new ZombieVirus();
            case SPELL_TYPE.PSYCHOPATHY:
                return new Psychopathy();
            case SPELL_TYPE.TORNADO:
                return new Tornado();
        }
        return null;
    }
    public bool IsSpell(SPELL_TYPE type) {
        return allSpells.Contains(type);
    }
    public bool IsAffliction(SPELL_TYPE type) {
        return allAfflictions.Contains(type);
    }
    public bool IsPlayerAction(SPELL_TYPE type) {
        return allPlayerActions.Contains(type);
    }
    public SpellData GetSpellData(SPELL_TYPE type) {
        if (allSpellsData.ContainsKey(type)) {
            return allSpellsData[type];
        }
        return null;
    }
    public SpellData GetAfflictionData(SPELL_TYPE type) {
        if (allAfflictionsData.ContainsKey(type)) {
            return allAfflictionsData[type];
        }
        return null;
    }
    public PlayerAction GetPlayerActionData(SPELL_TYPE type) {
        if (allPlayerActionsData.ContainsKey(type)) {
            return allPlayerActionsData[type];
        }
        return null;
    }
    public int GetSpellTier(SPELL_TYPE abilityType) {
        if (spellTiers.ContainsKey(abilityType)) {
            return spellTiers[abilityType];
        }
        return 3;
    }
    #endregion

    #region Combat Ability
    public CombatAbility CreateNewCombatAbility(COMBAT_ABILITY abilityType) {
        switch (abilityType) {
            case COMBAT_ABILITY.SINGLE_HEAL:
                return new SingleHeal();
            case COMBAT_ABILITY.FLAMESTRIKE:
                return new Flamestrike();
            case COMBAT_ABILITY.FEAR_SPELL:
                return new FearSpellAbility();
            case COMBAT_ABILITY.SACRIFICE:
                return new Sacrifice();
            case COMBAT_ABILITY.TAUNT:
                return new Taunt();
        }
        return null;
    }
    #endregion

    #region Unit Selection
    private List<Character> selectedUnits = new List<Character>();
    public void SelectUnit(Character character) {
        if (!selectedUnits.Contains(character)) {
            selectedUnits.Add(character);
        }
    }
    public void DeselectUnit(Character character) {
        if (selectedUnits.Remove(character)) {

        }
    }
    public void DeselectAllUnits() {
        Character[] units = selectedUnits.ToArray();
        for (int i = 0; i < units.Length; i++) {
            DeselectUnit(units[i]);
        }
    }
    private void OnMenuOpened(InfoUIBase @base) {
        if (@base is CharacterInfoUI) {
            DeselectAllUnits();
            CharacterInfoUI infoUi = @base as CharacterInfoUI;
            SelectUnit(infoUi.activeCharacter);
            //if (infoUI.activeCharacter.CanBeInstructedByPlayer()) {
            //    SelectUnit(infoUI.activeCharacter);
            //}
        }
    }
    private void OnMenuClosed(InfoUIBase @base) {
        if (@base is CharacterInfoUI) {
            DeselectAllUnits();
        }
    }
    // private void OnKeyPressedDown(KeyCode keyCode) {
    //     if (selectedUnits.Count > 0) {
    //         if (keyCode == KeyCode.Mouse1) {
    //             //right click
    //             for (int i = 0; i < selectedUnits.Count; i++) {
    //                 Character character = selectedUnits[i];
    //                 if (!character.CanBeInstructedByPlayer()) {
    //                     continue;
    //                 }
    //                 IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi;
    //                 character.StopCurrentActionNode(false, "Stopped by the player");
    //                 if (character.stateComponent.currentState != null) {
    //                     character.stateComponent.ExitCurrentState();
    //                 }
    //                 character.combatComponent.ClearHostilesInRange();
    //                 character.combatComponent.ClearAvoidInRange();
    //                 character.SetIsFollowingPlayerInstruction(false); //need to reset before giving commands
    //                 if (hoveredPOI is Character) {
    //                     Character target = hoveredPOI as Character;
    //                     if (character.IsHostileWith(target) && character.IsCombatReady()) {
    //                         character.combatComponent.Fight(target);
    //                         character.combatComponent.AddOnProcessCombatAction((combatState) => combatState.SetForcedTarget(target));
    //                         //CombatState cs = character.stateComponent.currentState as CombatState;
    //                         //if (cs != null) {
    //                         //    cs.SetForcedTarget(target);
    //                         //} else {
    //                         //    throw new System.Exception(character.name + " was instructed to attack " + target.name + " but did not enter combat state!");
    //                         //}
    //                     } else {
    //                         Debug.Log(character.name + " is not combat ready or is not hostile with " + target.name + ". Ignoring command.");
    //                     }
    //                 } else {
    //                     character.marker.GoTo(InnerMapManager.Instance.currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition), () => OnFinishInstructionFromPlayer(character));
    //                 }
    //                 character.SetIsFollowingPlayerInstruction(true);
    //             }
    //         } else if (keyCode == KeyCode.Mouse0) {
    //             DeselectAllUnits();
    //         }
    //     }
    // }
    private void OnFinishInstructionFromPlayer(Character character) {
        character.SetIsFollowingPlayerInstruction(false);
    }
    #endregion

    #region Chaos Orbs
    private void CreateChaosOrbsAt(Vector3 worldPos, int amount, InnerTileMap mapLocation) {
        StartCoroutine(ChaosOrbCreationCoroutine(worldPos, amount, mapLocation));
    }
    private IEnumerator ChaosOrbCreationCoroutine(Vector3 worldPos, int amount, InnerTileMap mapLocation) {
        for (int i = 0; i < amount; i++) {
            GameObject chaosOrbGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(chaosOrbPrefab.name, Vector3.zero, 
                Quaternion.identity, mapLocation.objectsParent);
            chaosOrbGO.transform.position = worldPos;
            ChaosOrb chaosOrb = chaosOrbGO.GetComponent<ChaosOrb>();
            chaosOrb.Initialize();
            yield return null;
        }
        Debug.Log($"Created {amount.ToString()} chaos orbs at {mapLocation.region.name}. Position {worldPos.ToString()}");
    }
    private void OnCharacterDidActionSuccess(Character character, ActualGoapNode actionNode) {
        if (character.IsNormalCharacter()) {
            CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(actionNode);
            if (crimeType != CRIME_TYPE.NONE) {
                int orbsToCreate;
                switch (crimeType) {
                    case CRIME_TYPE.MISDEMEANOR:
                        orbsToCreate = 4;
                        break;
                    case CRIME_TYPE.SERIOUS:
                        orbsToCreate = 6;
                        break;
                    case CRIME_TYPE.HEINOUS:
                        orbsToCreate = 8;
                        break;
                    default:
                        orbsToCreate = 2;
                        break;
                }
                character.logComponent.PrintLogIfActive(
                    $"{GameManager.Instance.TodayLogString()}{character.name} performed a crime of type {crimeType.ToString()}. Expelling {orbsToCreate.ToString()} Chaos Orbs.");
                Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, character.marker.transform.position, orbsToCreate, 
                    character.currentRegion.innerMap);

            }    
        }
    }
    #endregion

    #region Archetypes
    public static PlayerArchetype CreateNewArchetype(PLAYER_ARCHETYPE archetype) {
        string typeName = $"Archetype.{ UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(archetype.ToString()) }";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            PlayerArchetype obj = System.Activator.CreateInstance(type) as PlayerArchetype;
            return obj;
        }
        throw new System.Exception($"Could not create new archetype {archetype} because there is no data for it!");
    }
    #endregion
}
