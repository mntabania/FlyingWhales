using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Serialization;
using Traits;
using Archetype;
using Locations.Settlements;
using UnityEngine.Assertions;
using UtilityScripts;

public class PlayerManager : BaseMonoBehaviour {
    public static PlayerManager Instance;
    public Player player;

    public COMBAT_ABILITY[] allCombatAbilities;

    [Header("Job Action Icons")]
    [FormerlySerializedAs("jobActionIcons")] [SerializeField] private StringSpriteDictionary spellIcons;

    [Header("Combat Ability Icons")]
    [SerializeField] private StringSpriteDictionary combatAbilityIcons;
    
    [Header("Intervention Ability Tiers")]
    [FormerlySerializedAs("interventionAbilityTiers")] [SerializeField] private InterventionAbilityTierDictionary spellTiers;

    [Header("Chaos Orbs")] 
    [SerializeField] private GameObject chaosOrbPrefab;

    private bool _hasWinCheckTimer;

    private void Awake() {
        Instance = this;
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }
    public void Initialize() {
        availableChaosOrbs = new List<ChaosOrb>();
        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        // Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        // Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
        // Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        Messenger.AddListener<Vector3, int, InnerTileMap>(Signals.CREATE_CHAOS_ORBS, CreateChaosOrbsAt);
        Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DID_ACTION_SUCCESSFULLY, OnCharacterDidActionSuccess);
        // Messenger.AddListener(Signals.CHECK_IF_PLAYER_WINS, CheckWinCondition);
        Messenger.AddListener(Signals.WIN_GAME, WinGame);
    }
    public void InitializePlayer(HexTile portal) {
        player = new Player();
        player.CreatePlayerFaction();
        player.SetPortalTile(portal);
        PlayerSettlement existingPlayerNpcSettlement = portal.settlementOnTile as PlayerSettlement;
        Assert.IsNotNull(existingPlayerNpcSettlement, $"Portal does not have a player settlement on its tile");
        player.SetPlayerArea(existingPlayerNpcSettlement);
        
        LandmarkManager.Instance.OwnSettlement(player.playerFaction, existingPlayerNpcSettlement);

        PlayerUI.Instance.UpdateUI();
    }
    public void InitializePlayer(SaveDataCurrentProgress data) {
        player = data.LoadPlayer();
        //player.CreatePlayerFaction(data.playerSave);
        //player.LoadPlayerArea(data.playerSave);
        //PlayerUI.Instance.UpdateUI();
        // if (WorldConfigManager.Instance.isDemoWorld) {
        //     player.LoadPlayerData(SaveManager.Instance.currentSaveDataPlayer);    
        // }

    }
    //public void InitializePlayer(SaveDataPlayer data) {
    //    player = new Player(data);
    //    player.CreatePlayerFaction(data);
    //    // NPCSettlement existingPlayerNpcSettlement = LandmarkManager.Instance.GetAreaByID(data.playerAreaID);
    //    // player.SetPlayerArea(existingPlayerNpcSettlement);
    //    //PlayerUI.Instance.UpdateUI();
    //    //PlayerUI.Instance.InitializeThreatMeter();
    //    //PlayerUI.Instance.UpdateThreatMeter();

    //    for (int i = 0; i < data.minions.Count; i++) {
    //        data.minions[i].Load(player);
    //    }
    //    //for (int i = 0; i < data.summonSlots.Count; i++) {
    //    //    Summon summon = CharacterManager.Instance.GetCharacterByID(data.summonIDs[i]) as Summon;
    //    //    player.GainSummon(summon);
    //    //}
    //    //for (int i = 0; i < data.artifacts.Count; i++) {
    //    //    data.artifacts[i].Load(player);
    //    //}
    //    //for (int i = 0; i < data.interventionAbilities.Count; i++) {
    //    //    data.interventionAbilities[i].Load(player);
    //    //}
    //    for (int i = 0; i < player.minions.Count; i++) {
    //        if(player.minions[i].character.id == data.currentMinionLeaderID) {
    //            player.SetMinionLeader(player.minions[i]);
    //        }
    //    }
    //    //player.SetPlayerTargetFaction(LandmarkManager.Instance.enemyOfPlayerArea.owner);
    //}
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
        // if (@base is CharacterInfoUI) {
        //     DeselectAllUnits();
        //     CharacterInfoUI infoUi = @base as CharacterInfoUI;
        //     SelectUnit(infoUi.activeCharacter);
        //     //if (infoUI.activeCharacter.CanBeInstructedByPlayer()) {
        //     //    SelectUnit(infoUI.activeCharacter);
        //     //}
        // }
    }
    private void OnMenuClosed(InfoUIBase @base) {
        // if (@base is CharacterInfoUI) {
        //     DeselectAllUnits();
        // }
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
    //private void OnFinishInstructionFromPlayer(Character character) {
    //    character.SetIsFollowingPlayerInstruction(false);
    //}
    #endregion

    #region Chaos Orbs
    public List<ChaosOrb> availableChaosOrbs;
    public void CreateChaosOrbFromSave(Vector3 worldPos, Region region) {
        GameObject chaosOrbGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(chaosOrbPrefab.name, Vector3.zero, Quaternion.identity, region.innerMap.objectsParent);
        chaosOrbGO.transform.position = worldPos;
        ChaosOrb chaosOrb = chaosOrbGO.GetComponent<ChaosOrb>();
        chaosOrb.Initialize(worldPos, region);
        availableChaosOrbs.Add(chaosOrb);
    }
    private void CreateChaosOrbsAt(Vector3 worldPos, int amount, InnerTileMap mapLocation) {
        StartCoroutine(ChaosOrbCreationCoroutine(worldPos, amount, mapLocation));
    }
    private IEnumerator ChaosOrbCreationCoroutine(Vector3 worldPos, int amount, InnerTileMap mapLocation) {
        for (int i = 0; i < amount; i++) {
            GameObject chaosOrbGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(chaosOrbPrefab.name, Vector3.zero, 
                Quaternion.identity, mapLocation.objectsParent);
            chaosOrbGO.transform.position = worldPos;
            ChaosOrb chaosOrb = chaosOrbGO.GetComponent<ChaosOrb>();
            chaosOrb.Initialize(mapLocation.region);
            AddAvailableChaosOrb(chaosOrb);
            yield return null;
        }
        Debug.Log($"Created {amount.ToString()} chaos orbs at {mapLocation.region.name}. Position {worldPos.ToString()}");
    }
    private void OnCharacterDidActionSuccess(Character character, ActualGoapNode actionNode) {
        if (character.isNormalCharacter) {
            if (actionNode.action.goapType == INTERACTION_TYPE.ASSAULT || actionNode.action.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
                //https://trello.com/c/koET4MUl/2167-assault-should-not-produce-chaos-orbs-if-part-of-drink-blood-or-psychopathic-ritual
                if (actionNode.actor.traitContainer.HasTrait("Vampire")) {
                    if (actionNode.associatedJob is GoapPlanJob goapPlanJob && goapPlanJob.assignedPlan != null && goapPlanJob.assignedPlan.HasNodeWithAction(INTERACTION_TYPE.DRINK_BLOOD)) {
                        //first check if actions associated job has drink blood as part of the plan, if it does then do no create chaos orbs for it.
                        return;
                    }
                    if (actionNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL || actionNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT || 
                         actionNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT || actionNode.associatedJobType == JOB_TYPE.TRIGGER_FLAW) {
                        //If in case associated job is null, just check job type. Nasty side effect of relying on this checking is other jobs that use the TRIGGER_FLAW job type
                        //will also not create Chaos Orbs if that job uses Assault or Knockout. So it would be ideal that the above checking should always be used.
                        //This only serves as a failsafe.
                        return;
                    }
                }
                if (actionNode.associatedJobType == JOB_TYPE.RITUAL_KILLING) {
                    return;
                }
            }
            CRIME_SEVERITY crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(actionNode);
            if (crimeType != CRIME_SEVERITY.None) {
                int orbsToCreate;
                switch (crimeType) {
                    case CRIME_SEVERITY.Infraction:
                        orbsToCreate = 2;
                        break;
                    case CRIME_SEVERITY.Misdemeanor:
                        orbsToCreate = 2;
                        break;
                    case CRIME_SEVERITY.Serious:
                        orbsToCreate = 3;
                        break;
                    case CRIME_SEVERITY.Heinous:
                        orbsToCreate = 3;
                        break;
                    default:
                        orbsToCreate = 0;
                        break;
                }
                if(orbsToCreate != 0) {
                    character.logComponent.PrintLogIfActive($"{character.name} performed a crime of type {crimeType.ToString()}. Expelling {orbsToCreate.ToString()} Chaos Orbs.");
                    Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, character.marker.transform.position, orbsToCreate, character.currentRegion.innerMap);
                }
            }    
        }
    }
    private void AddAvailableChaosOrb(ChaosOrb chaosOrb) {
        availableChaosOrbs.Add(chaosOrb);
        Messenger.Broadcast(Signals.CHAOS_ORB_SPAWNED);
    }
    public void RemoveChaosOrbFromAvailability(ChaosOrb chaosOrb) {
        availableChaosOrbs.Remove(chaosOrb);
        Messenger.Broadcast(Signals.CHAOS_ORB_DESPAWNED);
    }
    #endregion

    #region Archetypes
    public static PlayerArchetype CreateNewArchetype(PLAYER_ARCHETYPE archetype) {
        string typeName = $"Archetype.{ UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(archetype.ToString()) }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            PlayerArchetype obj = System.Activator.CreateInstance(type) as PlayerArchetype;
            return obj;
        }
        throw new System.Exception($"Could not create new archetype {archetype} because there is no data for it!");
    }
    #endregion

    #region End Game Mechanics
    private void WinGame() {
        StartCoroutine(DelayedWinGame());
    }
    private IEnumerator DelayedWinGame() {
        UIManager.Instance.SetSpeedTogglesState(false);
        yield return GameUtilities.waitFor3Seconds;
        PlayerUI.Instance.WinGameOver();
    }
    // private void OnCharacterDied(Character character) {
    //     CheckWinCondition();
    // }
    // private void OnCharacterCanNoLongerPerform(Character character) {
    //     //CheckWinCondition();
    // }
    // private void OnCharacterCanNoLongerMove(Character character) {
    //     //CheckWinCondition();
    // }
    // private void CheckWinCondition() {
    //     if (DoesPlayerWin()) {
    //         if (!_hasWinCheckTimer) {
    //             CreateWinCheckTimer();
    //         }
    //     }
    // }
    // private void FinalCheckWinCondition() {
    //     if (DoesPlayerWin()) {
    //         PlayerUI.Instance.WinGameOver();
    //     }
    //     _hasWinCheckTimer = false;
    // }
    // private bool DoesPlayerWin() {
    //     for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //         Character character = CharacterManager.Instance.allCharacters[i];
    //         if(character.isNormalCharacter && !character.isAlliedWithPlayer) {
    //             if(!character.isDead) {
    //                 return false;
    //             }
    //         }
    //     }
    //     //check limbo characters
    //     for (int i = 0; i < CharacterManager.Instance.limboCharacters.Count; i++) {
    //         Character character = CharacterManager.Instance.limboCharacters[i];
    //         if(character.isNormalCharacter && !character.isAlliedWithPlayer) {
    //             if(!character.isDead) {
    //                 return false;
    //             }
    //         }
    //     }
    //     return true;
    // }
    // private void CreateWinCheckTimer() {
    //     GameDate dueDate = GameManager.Instance.Today();
    //     dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(15));
    //     SchedulingManager.Instance.AddEntry(dueDate, FinalCheckWinCondition, this);
    //     _hasWinCheckTimer = true;
    // }
    #endregion
}
