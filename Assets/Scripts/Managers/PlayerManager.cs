using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Serialization;
using Archetype;
using Locations.Settlements;
using Player_Input;
using Quests;
using Ruinarch;
using UnityEngine.Assertions;
using UtilityScripts;

public class PlayerManager : BaseMonoBehaviour {
    public static PlayerManager Instance;
    public Player player;
    
    [Header("Job Action Icons")]
    [FormerlySerializedAs("jobActionIcons")] [SerializeField] private StringSpriteDictionary spellIcons;

    [Header("Combat Ability Icons")]
    [SerializeField] private StringSpriteDictionary combatAbilityIcons;
    
    [Header("Intervention Ability Tiers")]
    [FormerlySerializedAs("interventionAbilityTiers")] [SerializeField] private InterventionAbilityTierDictionary spellTiers;

    [Header("Mana Orbs")] 
    [SerializeField] private GameObject chaosOrbPrefab;

    [Header("Spirit Energy")]
    [SerializeField] private GameObject spiritnEnergyPrefab;

    [Header("Base Building")] 
    [SerializeField] private PlayerStructurePlacementVisual _structurePlacementVisual;
    
    private List<PlayerInputModule> _playerInputModules;
    public static SeizeInputModule seizeInputModule;
    public static SpellInputModule spellInputModule;
    public static IntelInputModule intelInputModule;
    public static PickPortalInputModule pickPortalInputModule;
    
    private void Awake() {
        Instance = this;
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
        seizeInputModule = null;
        spellInputModule = null;
        intelInputModule = null;
        pickPortalInputModule = null;
        InputManager.RemoveOnUpdateEvent(ProcessPlayerInputModules);
    }
    public void Initialize() {
        availableChaosOrbs = new List<ChaosOrb>();
        _playerInputModules = new List<PlayerInputModule>();
        Messenger.AddListener<Vector3, int, InnerTileMap>(PlayerSignals.CREATE_CHAOS_ORBS, CreateChaosOrbsAt);
        Messenger.AddListener< Vector3, int, InnerTileMap>(PlayerSignals.CREATE_SPIRIT_ENERGY, CreateSpiritEnergyAt);
        Messenger.AddListener<Character, ActualGoapNode>(JobSignals.CHARACTER_DID_ACTION_SUCCESSFULLY, OnCharacterDidActionSuccess);
        Messenger.AddListener<string>(PlayerSignals.WIN_GAME, WinGame);
        seizeInputModule = new SeizeInputModule();
        spellInputModule = new SpellInputModule();
        intelInputModule = new IntelInputModule();
        pickPortalInputModule = new PickPortalInputModule();
        InputManager.AddOnUpdateEvent(ProcessPlayerInputModules);
        _structurePlacementVisual.Initialize(InnerMapCameraMove.Instance.camera);
        // if (!SaveManager.Instance.useSaveData) {
        //     player = new Player();   
        // }
    }
    public void InitializePlayer(Area portal) {
        player = new Player();   
        player.CreatePlayerFaction();
        player.SetPortalTile(portal);
        PlayerSettlement existingPlayerNpcSettlement = portal.settlementOnArea as PlayerSettlement;
        Assert.IsNotNull(existingPlayerNpcSettlement, $"Portal does not have a player settlement on its tile");
        player.SetPlayerArea(existingPlayerNpcSettlement);
        
        LandmarkManager.Instance.OwnSettlement(player.playerFaction, existingPlayerNpcSettlement);

        PlayerUI.Instance.UpdateUI();
    }
    public void InitializePlayer(SaveDataCurrentProgress data) {
        player = data.LoadPlayer();
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

    #region Player Input
    private void ProcessPlayerInputModules() {
        if (_playerInputModules == null) { return; }
        for (int i = 0; i < _playerInputModules.Count; i++) {
            PlayerInputModule module = _playerInputModules[i];
            module.OnUpdate();
        }
    }
    public void AddPlayerInputModule(PlayerInputModule p_module) {
        if (!_playerInputModules.Contains(p_module)) {
            _playerInputModules.Add(p_module);
            Debug.Log($"Added Player Input Module: {p_module.GetType().ToString()}");
        }
    }
    public void RemovePlayerInputModule(PlayerInputModule p_module) {
        if (_playerInputModules.Remove(p_module)) {
            Debug.Log($"Removed Player Input Module: {p_module.GetType().ToString()}");    
        }
    }
    #endregion
    
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
    public int GetSpellTier(PLAYER_SKILL_TYPE abilityType) {
        if (spellTiers.ContainsKey(abilityType)) {
            return spellTiers[abilityType];
        }
        return 3;
    }
    #endregion

    #region Mana Orbs
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
#if DEBUG_LOG
        Debug.Log($"Created {amount.ToString()} chaos orbs at {mapLocation.region.name}. Position {worldPos.ToString()}");
#endif
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
                        //will also not create Mana Orbs if that job uses Assault or Knockout. So it would be ideal that the above checking should always be used.
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
#if DEBUG_LOG
                    character.logComponent.PrintLogIfActive($"{character.name} performed a crime of type {crimeType.ToString()}. Expelling {orbsToCreate.ToString()} Mana Orbs.");
#endif
                    //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.marker.transform.position, orbsToCreate, character.currentRegion.innerMap);
                }
            }    
        }
    }
    private void AddAvailableChaosOrb(ChaosOrb chaosOrb) {
        availableChaosOrbs.Add(chaosOrb);
        Messenger.Broadcast(PlayerSignals.CHAOS_ORB_SPAWNED);
    }
    public void RemoveChaosOrbFromAvailability(ChaosOrb chaosOrb) {
        availableChaosOrbs.Remove(chaosOrb);
        Messenger.Broadcast(PlayerSignals.CHAOS_ORB_DESPAWNED);
    }
#endregion

#region Spirit Energy
    public List<SpiritEnergy> availableSpiritEnergy;
    public void CreateSpiritEnergyFromSave(Vector3 worldPos, Region region) {
        GameObject spiritEnergyGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(spiritnEnergyPrefab.name, Vector3.zero, Quaternion.identity, region.innerMap.objectsParent);
        spiritEnergyGO.transform.position = worldPos;
        SpiritEnergy spiritEnergy = spiritEnergyGO.GetComponent<SpiritEnergy>();
        spiritEnergy.Initialize(worldPos, region);
        availableSpiritEnergy.Add(spiritEnergy);
    }
    private void CreateSpiritEnergyAt(Vector3 worldPos, int amount, InnerTileMap mapLocation) {
        StartCoroutine(SpiritEnergyCreationCoroutine(worldPos, amount, mapLocation));
    }
    private IEnumerator SpiritEnergyCreationCoroutine(Vector3 worldPos, int amount, InnerTileMap mapLocation) {
        for (int i = 0; i < amount; i++) {
            GameObject spiritEnergyGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(spiritnEnergyPrefab.name, Vector3.zero,
                Quaternion.identity, mapLocation.objectsParent);
            spiritEnergyGO.transform.position = worldPos;
            SpiritEnergy spiritEnergy = spiritEnergyGO.GetComponent<SpiritEnergy>();
            spiritEnergy.Initialize(mapLocation.region, amount);
            AddAvailableSpritiEnergy(spiritEnergy);
            yield return null;
        }
#if DEBUG_LOG
        Debug.Log($"Created {amount.ToString()} chaos orbs at {mapLocation.region.name}. Position {worldPos.ToString()}");
#endif
    }
    
    private void AddAvailableSpritiEnergy(SpiritEnergy p_spiritEnergy) {
        availableSpiritEnergy.Add(p_spiritEnergy);
        Messenger.Broadcast(PlayerSignals.SPIRIT_ENERGY_SPAWNED);
    }
    public void RemoveSpiritEnergyFromAvailability(SpiritEnergy p_spiritEnergy) {
        availableSpiritEnergy.Remove(p_spiritEnergy);
        Messenger.Broadcast(PlayerSignals.SPIRIT_ENERGY_DESPAWNED);
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
    private void WinGame(string winMessage) {
        StartCoroutine(DelayedWinGame(winMessage));
    }
    private IEnumerator DelayedWinGame(string winMessage) {
        UIManager.Instance.SetSpeedTogglesState(false);
        yield return GameUtilities.waitFor3Seconds;
        PlayerUI.Instance.WinGameOver(winMessage);
        QuestManager.Instance.winConditionTracker?.RemoveStepsFromBookmark();
    }
#endregion

#region Structure Placement
    public void ShowStructurePlacementVisual(STRUCTURE_TYPE p_structureType) {
        _structurePlacementVisual.Show(p_structureType);
    }
    public void HideStructurePlacementVisual() {
        _structurePlacementVisual.Hide();
    }
    public void SetStructurePlacementVisualFollowMouseState(bool p_state) {
        _structurePlacementVisual.SetFollowMouseState(p_state);
    }
    public void SetStructurePlacementVisualHighlightColor(Color p_color) {
        _structurePlacementVisual.SetHighlightColor(p_color);
    }
#endregion
}
