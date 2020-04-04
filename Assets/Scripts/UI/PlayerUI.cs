using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Linq;
using System;
using Inner_Maps;
using Ruinarch;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance;

    [Header("Top Menu")]
    public GameObject regionNameTopMenuGO;
    public TextMeshProUGUI regionNameTopMenuText;
    public HoverHandler regionNameHoverHandler;
    
    [Header("Currencies")]
    [SerializeField] private TextMeshProUGUI manaLbl;

    [Header("Intel")]
    [SerializeField] private GameObject intelContainer;
    [SerializeField] private IntelItem[] intelItems;
    [SerializeField] private Toggle intelToggle;

    [Header("Provoke")]
    [SerializeField] private ProvokeMenu provokeMenu;

    [Header("Miscellaneous")]
    [SerializeField] private GameObject successfulAreaCorruptionGO;
    [SerializeField] private ScrollRect killSummaryScrollView;

    [Header("General Confirmation")] 
    [SerializeField] private GeneralConfirmation _generalConfirmation;
    
    [Header("Intervention Abilities")]
    [SerializeField] private GameObject actionBtnPrefab;
    
    [Header("Replace UI")]
    public ReplaceUI replaceUI;

    [Header("Level Up UI")]
    public LevelUpUI levelUpUI;

    [Header("New Ability UI")]
    public NewAbilityUI newAbilityUI;

    [Header("New Minion Ability UI")]
    public NewMinionAbilityUI newMinionAbilityUI;

    [Header("Research Intervention Ability UI")]
    public ResearchAbilityUI researchInterventionAbilityUI;

    [Header("Unleash Summon UI")]
    public UnleashSummonUI unleashSummonUI;

    [Header("Skirmish UI")]
    public GameObject skirmishConfirmationGO;

    [Header("Saving/Loading")]
    public Button saveGameButton;

    [Header("End Game Mechanics")]
    [SerializeField] private WinGameOverItem winGameOver;
    [SerializeField] private LoseGameOverItem loseGameOver;

    [Header("Kill Count UI")]
    [SerializeField] private GameObject killCountGO;
    [SerializeField] private TextMeshProUGUI killCountLbl;
    [SerializeField] private GameObject killSummaryGO;
    [SerializeField] private GameObject killCharacterItemPrefab;
    [SerializeField] private ScrollRect killCountScrollView;
    [SerializeField] private RectTransform aliveHeader;
    public RectTransform deadHeader;
    private List<CharacterNameplateItem> killCountCharacterItems;
    private int unusedKillCountCharacterItems;
    private int aliveCount;
    private int allFilteredCharactersCount;
    
    [Header("Seize Object")]
    [SerializeField] private Button unseizeButton;

    [Header("Top Menu")]
    [SerializeField] private Toggle[] topMenuButtons;
    [SerializeField] private SpellListUI spellList;
    [SerializeField] private CustomDropdownList customDropdownList;
    
    [Header("Minion List")]
    [SerializeField] private MinionListUI minionList;
    private readonly List<string> factionActionsList = new List<string>() { "Manage Cult", "Meddle" };

    [Header("Player Actions")]
    public SpellSpriteDictionary playerActionsIconDictionary;
    private List<System.Action> pendingUIToShow { get; set; }

    [Header("Spells")]
    public ScrollRect spellsScrollRect;
    public GameObject spellsContainerGO;
    public GameObject spellItemPrefab;
    private List<SpellItem> _spellItems;

    [Header("Summons")]
    public ScrollRect summonsScrollRect;
    public GameObject summonsContainerGO;
    public GameObject summonItemPrefab;
    private List<SummonItem> _summonItems;

    [Header("Items")]
    public ScrollRect itemsScrollRect;
    public GameObject itemsContainerGO;
    public GameObject itemItemPrefab;
    private List<ItemItem> _itemItems;

    [Header("Artifacts")]
    public ScrollRect artifactsScrollRect;
    public GameObject artifactsContainerGO;
    public GameObject artifactItemPrefab;
    private List<ArtifactItem> _artifactItems;

    [Header("Threat")]
    public Image threatMeter;

    private PlayerJobActionButton[] interventionAbilityBtns;
    public string harassRaidInvade { get; private set; }
    public Minion harassRaidInvadeLeaderMinion { get; private set; }
    public NPCSettlement harassRaidInvadeTargetNpcSettlement { get; private set; }

    void Awake() {
        Instance = this;
    }
    public void UpdateUI() {
        if (PlayerManager.Instance.player == null) {
            return;
        }
        UpdateMana();
    }

    public void Initialize() {
        pendingUIToShow = new List<Action>();
        _spellItems = new List<SpellItem>();
        _summonItems = new List<SummonItem>();
        _itemItems = new List<ItemItem>();
        _artifactItems = new List<ArtifactItem>();

        Messenger.AddListener<InfoUIBase>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
        Messenger.AddListener<IIntel>(Signals.PLAYER_OBTAINED_INTEL, OnIntelObtained);
        Messenger.AddListener<IIntel>(Signals.PLAYER_REMOVED_INTEL, OnIntelRemoved);

        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);
        
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);
        
        Messenger.AddListener<SPELL_TYPE>(Signals.PLAYER_GAINED_SPELL, OnGainSpell);
        Messenger.AddListener<SPELL_TYPE>(Signals.PLAYER_LOST_SPELL, OnLostSpell);
        minionList.Initialize();
    }

    public void InitializeAfterGameLoaded() {
        //Kill Count UI
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Character, Faction>(Signals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character>(Signals.CHARACTER_CREATED, AddedNewCharacter);
        Messenger.AddListener<Character>(Signals.CHARACTER_BECOMES_MINION_OR_SUMMON, CharacterBecomesMinionOrSummon);
        Messenger.AddListener<Character>(Signals.CHARACTER_BECOMES_NON_MINION_OR_SUMMON, CharacterBecomesNonMinionOrSummon);
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.AddListener<Character, Character>(Signals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchFromLimbo);
        Messenger.AddListener(Signals.THREAT_UPDATED, OnThreatUpdated);
        Messenger.AddListener<IPointOfInterest>(Signals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.AddListener<IPointOfInterest>(Signals.ON_UNSEIZE_POI, OnUnseizePOI);

        //key presses
        Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnKeyPressed);

        //currencies
        Messenger.AddListener(Signals.PLAYER_ADJUSTED_MANA, UpdateMana);
        InitialUpdateKillCountCharacterItems();
        UpdateIntel();
        CreateInitialSpells();
        CreateSummonsForTesting();
        CreateItemsForTesting();
        CreateArtifactsForTesting();
    }

    #region Listeners
    private void OnInnerMapOpened(Region location) {
        UpdateRegionNameState();
    }
    private void OnInnerMapClosed(Region location) {
        UpdateRegionNameState();
    }
    private void OnKeyPressed(KeyCode pressedKey) {
        if (pressedKey == KeyCode.Escape) {
            // if (PlayerManager.Instance.player.currentActivePlayerSpell != null) {
            //     PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
            //     InputManager.Instance.ClearLeftClickActions();
            // } else {
                //only toggle options menu if doing nothing else
                UIManager.Instance.ToggleOptionsMenu();
            // }
        }
    }
    private void OnCharacterDied(Character character) {
        TransferCharacterFromActiveToInactive(character);
        UpdateKillCount();
    }
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        //if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
        //    TransferCharacterFromActiveToInactive(character);
        //    UpdateKillCount();
        //}
    }
    private void OnCharacterLostTrait(Character character, Trait trait) {
        //if (trait.type == TRAIT_TYPE.DISABLER && trait.effect == TRAIT_EFFECT.NEGATIVE) {
        //    TransferCharacterFromInactiveToActive(character);
        //    UpdateKillCount();
        //}
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        //UpdateKillCount();
        //OrderKillSummaryItems();

        //TODO: This causes inconsistencies since the character will have null faction once he/she is removed from the faction
        //TransferCharacterFromActiveToInactive(character);
        //CheckIfAllCharactersWipedOut();
    }
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        //if (faction == FactionManager.Instance.neutralFaction) {
        //    TransferCharacterFromActiveToInactive(character);
        //} else 
        if (faction.isPlayerFaction /*|| faction == FactionManager.Instance.friendlyNeutralFaction*/) {
            OnCharacterBecomesMinionOrSummon(character);
        } else {
            TransferCharacterFromInactiveToActive(character);
        }
        UpdateKillCount();
        //CheckIfAllCharactersWipedOut();
    }
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass) {
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if (item != null) {
            item.UpdateObject(character);
        } else {
            item = GetInactiveCharacterNameplateItem(character);
            if (item != null) {
                item.UpdateObject(character);
            }
        }
    }
    private void OnCharacterSwitchFromLimbo(Character fromCharacter, Character toCharacter) {
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(fromCharacter);
        if (item != null) {
            item.UpdateObject(toCharacter);
        } else {
            item = GetInactiveCharacterNameplateItem(fromCharacter);
            if (item != null) {
                item.UpdateObject(toCharacter);
            }
        }
        if (!toCharacter.IsAble()/* || toCharacter.isFactionless*/) {
            TransferCharacterFromActiveToInactive(toCharacter);
        } else if (toCharacter.faction.isPlayerFaction /*|| faction == FactionManager.Instance.friendlyNeutralFaction*/) {
            OnCharacterBecomesMinionOrSummon(toCharacter);
        } else {
            TransferCharacterFromInactiveToActive(toCharacter);
        }
        UpdateKillCount();
    }
    private void AddedNewCharacter(Character character) {
        OnAddNewCharacter(character);
    }
    private void CharacterBecomesMinionOrSummon(Character character) {
        //OnCharacterBecomesMinionOrSummon(character);
    }
    private void CharacterBecomesNonMinionOrSummon(Character character) {
        OnCharacterBecomesNonMinionOrSummon(character);
    }
    private void OnMenuOpened(InfoUIBase @base) {
        if (@base is CharacterInfoUI || @base is TileObjectInfoUI) {
            // HideKillSummary();
        }else if (@base is HextileInfoUI || @base is RegionInfoUI) {
            UpdateRegionNameState();
        }
    }
    private void OnMenuClosed(InfoUIBase @base) {
        if (@base is HextileInfoUI || @base is RegionInfoUI) {
            UpdateRegionNameState();
        }
    }
    private void OnThreatUpdated() {
        threatMeter.fillAmount = PlayerManager.Instance.player.threatComponent.threat / (float) ThreatComponent.MAX_THREAT;
    }
    #endregion

    private void UpdateRegionNameState() {
        if (UIManager.Instance.regionInfoUI.isShowing || UIManager.Instance.hexTileInfoUI.isShowing 
            || InnerMapManager.Instance.isAnInnerMapShowing) {
            Region location;
            if (UIManager.Instance.regionInfoUI.isShowing) {
                location = UIManager.Instance.regionInfoUI.activeRegion;
            } else if (UIManager.Instance.hexTileInfoUI.isShowing) {
                location = UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile.region;
            } else {
                location = InnerMapManager.Instance.currentlyShowingMap.region as Region;
            }
            Assert.IsNotNull(location, $"Trying to update region name UI in top menu, but no region is specified.");
            regionNameTopMenuText.text = location.name;
            regionNameTopMenuGO.SetActive(true);
            regionNameHoverHandler.SetOnHoverAction(() => TestingUtilities.ShowLocationInfo(location.coreTile.region));
            regionNameHoverHandler.SetOnHoverOutAction(TestingUtilities.HideLocationInfo);
        } else {
            regionNameTopMenuGO.SetActive(false);
        }
    }

    #region Currencies
    private void UpdateMana() {
        manaLbl.text = PlayerManager.Instance.player.mana.ToString();
    }
    #endregion

    #region Miscellaneous
    public void AddPendingUI(System.Action pendingUIAction) {
        pendingUIToShow.Add(pendingUIAction);
    }
    public bool TryShowPendingUI() {
        if (pendingUIToShow.Count > 0) {
            System.Action pending = pendingUIToShow[0];
            pendingUIToShow.RemoveAt(0);
            pending.Invoke();
            return true;
        }
        return false;
    }
    public bool IsMajorUIShowing() {
        return levelUpUI.gameObject.activeInHierarchy || newAbilityUI.gameObject.activeInHierarchy || 
               newMinionAbilityUI.gameObject.activeInHierarchy || replaceUI.gameObject.activeInHierarchy || 
               _generalConfirmation.isShowing || newMinionUIGO.activeInHierarchy;
    }
    #endregion

    #region Intel
    private void OnIntelObtained(IIntel intel) {
        UpdateIntel();
    }
    private void OnIntelRemoved(IIntel intel) {
        UpdateIntel();
    }
    private void UpdateIntel() {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            IIntel intel = PlayerManager.Instance.player.allIntel.ElementAtOrDefault(i);
            currItem.SetIntel(intel);
            if (intel != null) {
                currItem.SetClickAction(PlayerManager.Instance.player.SetCurrentActiveIntel);
            }
        }
    }
    public void SetIntelMenuState(bool state) {
        if (intelToggle.isOn == state) {
            return; //ignore change
        }
        intelToggle.isOn = state;
        if (!intelToggle.isOn) {
            OnCloseIntelMenu();
        }
    }
    private void OnCloseIntelMenu() {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.ClearClickActions();
        }
    }
    public void SetIntelItemClickActions(IntelItem.OnClickAction clickAction) {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.SetClickAction(clickAction);
        }
    }
    public void AddIntelItemOtherClickActions(System.Action clickAction) {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.AddOtherClickAction(clickAction);
        }
    }
    private void OnOpenShareIntelMenu() {
        intelToggle.isOn = false;
        intelToggle.interactable = false;
        //for (int i = 0; i < roleSlots.Length; i++) {
        //    RoleSlotItem rsi = roleSlots[i];
        //    rsi.HideActionButtons();
        //    rsi.OverrideDraggableState(false);
        //}
        //assignBtn.interactable = false;

        //if (UIManager.Instance.characterInfoUI.isShowing || UIManager.Instance.tileObjectInfoUI.isShowing) {
        //    HideActionButtons();
        //}
    }
    private void OnCloseShareIntelMenu() {
        intelToggle.interactable = true;
        //for (int i = 0; i < roleSlots.Length; i++) {
        //    RoleSlotItem rsi = roleSlots[i];
        //    //rsi.UpdateActionButtons();
        //    rsi.OverrideDraggableState(true);
        //}
        //assignBtn.interactable = true;
        //if (UIManager.Instance.characterInfoUI.isShowing) {
        //    ShowActionButtonsFor(UIManager.Instance.characterInfoUI.activeCharacter);
        //}else if (UIManager.Instance.tileObjectInfoUI.isShowing) {
        //    ShowActionButtonsFor(UIManager.Instance.tileObjectInfoUI.activeTileObject);
        //}
    }
    public void ShowPlayerIntels(bool state) {
        intelContainer.SetActive(state);
        //RectTransform rt = UIManager.Instance.playerNotifGO.transform as RectTransform;
        //Vector3 previousPos = rt.anchoredPosition;
        //if (!state) {
        //    rt.anchoredPosition = new Vector3(-640f, previousPos.y, previousPos.z);
        //} else {
        //    rt.anchoredPosition = new Vector3(-1150f, previousPos.y, previousPos.z);
        //}
    }
    public IntelItem GetIntelItemWithIntel(IIntel intel) {
        for (int i = 0; i < intelItems.Length; i++) {
            if (intelItems[i].intel != null && intelItems[i].intel == intel) {
                return intelItems[i];
            }
        }
        return null;
    }
    #endregion

    #region Provoke
    public void OpenProvoke(Character actor, Character target) {
        provokeMenu.Open(actor, target);
    }
    #endregion
    
    #region Corruption and Threat
    public void HideCorruptTileConfirmation() {
        skirmishConfirmationGO.SetActive(false);
    }
    public void OnClickYesCorruption() {
        HideCorruptTileConfirmation();
        // if (tempCurrentMinionLeaderPicker != null) {
        //     PlayerManager.Instance.player.SetMinionLeader(tempCurrentMinionLeaderPicker.minion);
        // } else {
        //     //If story event, randomize minion leader, if not, keep current minion leader
        //     //if(PlayerManager.Instance.player.currentTileBeingCorrupted.landmarkOnTile.yieldType == LANDMARK_YIELD_TYPE.STORY_EVENT) {
        //     //    Minion minion = PlayerManager.Instance.player.GetRandomMinion();
        //     //    PlayerManager.Instance.player.SetMinionLeader(minion);
        //     //}
        // }
        if (PlayerManager.Instance.player.currentTileBeingCorrupted.region != null) {
            InnerMapManager.Instance.TryShowLocationMap(PlayerManager.Instance.player.currentTileBeingCorrupted.region);
        } else {
            //PlayerManager.Instance.player.currentTileBeingCorrupted.landmarkOnTile.ShowEventBasedOnYieldType();
            PlayerManager.Instance.player.InvadeATile();
        }
        //if (tempCurrentMinionLeaderPicker != null) {
        //    PlayerManager.Instance.player.SetMinionLeader(tempCurrentMinionLeaderPicker.minion);
        //    if (PlayerManager.Instance.player.currentTileBeingCorrupted.settlementOfTile == null) {
        //        StoryEvent e = PlayerManager.Instance.player.currentTileBeingCorrupted.GetRandomStoryEvent();
        //        if (e != null) {
        //            Debug.Log("Will show event " + e.name);
        //            storyEventUI.ShowEvent(e, true);
        //            //if (e.trigger == STORY_EVENT_TRIGGER.IMMEDIATE) {
        //            //    //show story event UI
        //            //    storyEventUI.ShowEvent(e, true);
        //            //} else if (e.trigger == STORY_EVENT_TRIGGER.MID) { //schedule show event UI based on trigger.
        //            //    int difference = Mathf.Abs(GameManager.Instance.Today().day - (GameManager.Instance.Today().day + PlayerManager.Instance.player.currentTileBeingCorrupted.corruptDuration));
        //            //    int day = UnityEngine.Random.Range(1, difference);
        //            //    GameDate dueDate = GameManager.Instance.Today().AddDays(day);
        //            //    SchedulingManager.Instance.AddEntry(dueDate, () => storyEventUI.ShowEvent(e, true), null);
        //            //} else if (e.trigger == STORY_EVENT_TRIGGER.END) {
        //            //    GameDate dueDate = GameManager.Instance.Today().AddDays(PlayerManager.Instance.player.currentTileBeingCorrupted.corruptDuration);
        //            //    SchedulingManager.Instance.AddEntry(dueDate, () => storyEventUI.ShowEvent(e, true), null);
        //            //}
        //        }
        //    }
        //}
    }
    public void OnClickNoCorruption() {
        HideCorruptTileConfirmation();
    }
    //public void UpdateThreatMeter() {
    //    threatMeter.value = PlayerManager.Instance.player.threat;
    //}
    #endregion

    #region End Game Mechanics
    public void WinGameOver() {
        UIManager.Instance.Pause();
        winGameOver.Open();
    }
    public void LoseGameOver() {
        UIManager.Instance.Pause();
        loseGameOver.Open();
    }
    #endregion

    #region NPCSettlement Corruption
    public void SuccessfulAreaCorruption() {
        successfulAreaCorruptionGO.SetActive(true);
        //Utilities.DestroyChildren(killSummaryScrollView.content);
        LoadKillSummaryCharacterItems();
    }
    private void LoadKillSummaryCharacterItems() {
        CharacterNameplateItem[] items = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(killCountScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            item.transform.SetParent(killSummaryScrollView.content);
        }
    }
    public void BackToWorld() {
        UtilityScripts.Utilities.DestroyChildren(killSummaryScrollView.content);
        Region closedArea = InnerMapManager.Instance.HideAreaMap();
        successfulAreaCorruptionGO.SetActive(false);
        InnerMapManager.Instance.DestroyInnerMap(closedArea);

        //if (LandmarkManager.Instance.AreAllNonPlayerAreasCorrupted()) {
        //    GameOver("You have conquered all settlements! This world is now yours! Congratulations!");
        //}
    }
    #endregion

    #region Kill Count
    public bool isShowingKillSummary { get { return killCountGO.activeSelf; } }
    [SerializeField] private Toggle killSummaryToggle;
    private void UpdateKillCountActiveState() {
        bool state = InnerMapManager.Instance.isAnInnerMapShowing;
        killCountGO.SetActive(state);
        killSummaryGO.SetActive(false);
    }
    private void LoadKillCountCharacterItems() {
        aliveCount = 0;
        allFilteredCharactersCount = 0;
        unusedKillCountCharacterItems = 0;
        killCountCharacterItems = new List<CharacterNameplateItem>();
        for (int i = 0; i < 20; i++) { //Initial number is 20
            CreateNewKillCountCharacterItem();
        }
    }
    private CharacterNameplateItem CreateNewKillCountCharacterItem() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(killCharacterItemPrefab.name, Vector3.zero, Quaternion.identity, killCountScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        go.SetActive(false);
        killCountCharacterItems.Add(item);
        unusedKillCountCharacterItems++;
        return item;
    }
    //This must only be called once during initialization
    private void InitialUpdateKillCountCharacterItems() {
        //CharacterNameplateItem[] items = GameGameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(killCountScrollView.content.gameObject);
        //for (int i = 0; i < items.Length; i++) {
        //    ObjectPoolManager.Instance.DestroyObject(items[i].gameObject);
        //}
        LoadKillCountCharacterItems();
        List<CharacterNameplateItem> alive = new List<CharacterNameplateItem>();
        List<CharacterNameplateItem> dead = new List<CharacterNameplateItem>();
        List<Character> allCharacters = CharacterManager.Instance.allCharacters;
        int allCharactersCount = CharacterManager.Instance.allCharacters.Count;
        int killCountCharacterItemsCount = killCountCharacterItems.Count;
        //if (allCharactersCount < killCountCharacterItemsCount) {
        //    for (int i = allCharactersCount; i < killCountCharacterItemsCount; i++) {
        //        killCountCharacterItems[i].gameObject.SetActive(false);
        //    }
        //}
        string log = "Initial Kill Count UI";
        for (int i = 0; i < allCharactersCount; i++) {
            Character character = allCharacters[i];
            log += $"\nCharacter: {character.name}";
            if (i >= killCountCharacterItemsCount) {
                CreateNewKillCountCharacterItem();
            }
            CharacterNameplateItem item = killCountCharacterItems[i];
            if (!WillCharacterBeShownInKillCount(character)) {
                //Do not show minions and summons
                item.gameObject.SetActive(false);
                log += " - do not show";
                continue;
            }
            item.SetObject(character);
            item.SetAsButton();
            item.ClearAllOnClickActions();
            item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, false));
            item.gameObject.SetActive(true);
            allFilteredCharactersCount++;
            unusedKillCountCharacterItems--;
            if (/*character.isFactionless*/
                //|| character.faction == PlayerManager.Instance.player.playerFaction
                //|| character.faction == FactionManager.Instance.disguisedFaction
                /*||*/ !character.IsAble()) { //added checking for faction in cases that the character was raised from dead (Myk, if the concern here is only from raise dead, I changed the checker to returnedToLife to avoid conflicts with factions, otherwise you can return it to normal. -Chy)
                dead.Add(item);
                log += " - dead";
            } else {
                aliveCount++;
                alive.Add(item);
                log += " - alive";
            }
            //GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(killCharacterItemPrefab.name, Vector3.zero, Quaternion.identity, killCountScrollView.content);
            //CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
            //item.SetObject(character);
            //item.SetAsButton();
            //item.ClearAllOnClickActions();
            //item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, false));
        }
        Debug.Log(log);
        UpdateKillCount();

        aliveHeader.transform.SetAsFirstSibling();
        for (int i = 0; i < alive.Count; i++) {
            CharacterNameplateItem currItem = alive[i];
            currItem.SetIsActive(true);
            currItem.transform.SetSiblingIndex(i + 1);
        }
        deadHeader.transform.SetSiblingIndex(alive.Count + 1);
        for (int i = 0; i < dead.Count; i++) {
            CharacterNameplateItem currItem = dead[i];
            currItem.SetIsActive(false);
            currItem.transform.SetSiblingIndex(alive.Count + i + 2);
        }
        //OrderKillSummaryItems();
        //UpdateKillCount();
    }
    private void OnAddNewCharacter(Character character) {
        if (!WillCharacterBeShownInKillCount(character)) {
            //Do not show minions and summons
            return;
        }
        allFilteredCharactersCount++;
        CharacterNameplateItem item = null;
        if (unusedKillCountCharacterItems > 0) {
            item = GetUnusedCharacterNameplateItem();
        } else {
            item = CreateNewKillCountCharacterItem();
        }
        item.SetObject(character);
        item.SetAsButton();
        item.ClearAllOnClickActions();
        item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, false));
        item.gameObject.SetActive(true);
        unusedKillCountCharacterItems--;
        if (/*character.isFactionless*/
            //|| character.faction == PlayerManager.Instance.player.playerFaction
            //|| character.faction == FactionManager.Instance.disguisedFaction
            /*||*/ !character.IsAble()) { //added checking for faction in cases that the character was raised from dead (Myk, if the concern here is only from raise dead, I changed the checker to returnedToLife to avoid conflicts with factions, otherwise you can return it to normal. -Chy)
            if (allFilteredCharactersCount == killCountCharacterItems.Count) {
                item.transform.SetAsLastSibling();
            } else {
                item.transform.SetSiblingIndex(allFilteredCharactersCount + 2);
            }
            item.SetIsActive(false);
        } else {
            aliveCount++;
            item.transform.SetSiblingIndex(deadHeader.transform.GetSiblingIndex());
            item.SetIsActive(true);
        }
        UpdateKillCount();
    }
    private void TransferCharacterFromActiveToInactive(Character character) {
        if (!WillCharacterBeShownInKillCount(character)) {
            return;
        }
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if(item != null) {
            if (allFilteredCharactersCount == killCountCharacterItems.Count) {
                item.transform.SetAsLastSibling();
            } else {
                item.transform.SetSiblingIndex(allFilteredCharactersCount + 2);
            }
            aliveCount--;
            item.SetIsActive(false);
        }
        //UpdateKillCount();
    }
    private void TransferCharacterFromActiveToInactive(CharacterNameplateItem nameplate) {
        if (!WillCharacterBeShownInKillCount(nameplate.character)) {
            return;
        }
        if (!nameplate.isActive) {
            return;
        }
        if (allFilteredCharactersCount == killCountCharacterItems.Count) {
            nameplate.transform.SetAsLastSibling();
        } else {
            nameplate.transform.SetSiblingIndex(allFilteredCharactersCount + 2);
        }
        aliveCount--;
        nameplate.SetIsActive(false);
    }
    private void TransferCharacterFromInactiveToActive(Character character) {
        if (!WillCharacterBeShownInKillCount(character)) {
            return;
        }
        CharacterNameplateItem item = GetInactiveCharacterNameplateItem(character);
        if (item != null) {
            int index = item.transform.GetSiblingIndex();
            int deadHeaderIndex = deadHeader.transform.GetSiblingIndex();
            item.transform.SetSiblingIndex(deadHeaderIndex);
            aliveCount++;
            item.SetIsActive(true);
        }
        //UpdateKillCount();
    }
    private void TransferCharacterFromInactiveToActive(CharacterNameplateItem nameplate) {
        if (!WillCharacterBeShownInKillCount(nameplate.character)) {
            return;
        }
        if (nameplate.isActive) {
            return;
        }
        int index = nameplate.transform.GetSiblingIndex();
        int deadHeaderIndex = deadHeader.transform.GetSiblingIndex();
        nameplate.transform.SetSiblingIndex(deadHeaderIndex);
        aliveCount++;
        nameplate.SetIsActive(true);
        //UpdateKillCount();
    }
    private void OnCharacterBecomesMinionOrSummon(Character character) {
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if (item != null) {
            item.gameObject.SetActive(false);
            aliveCount--;
            allFilteredCharactersCount--;
            unusedKillCountCharacterItems++;
            //UpdateKillCount();
        } else {
            item = GetInactiveCharacterNameplateItem(character);
            if (item != null) {
                item.gameObject.SetActive(false);
                aliveCount--;
                allFilteredCharactersCount--;
                unusedKillCountCharacterItems++;
                //UpdateKillCount();
            }
        }
    }
    private void OnCharacterBecomesNonMinionOrSummon(Character character) {
        OnAddNewCharacter(character);
    }
    private CharacterNameplateItem GetUnusedCharacterNameplateItem() {
        int killCountCharacterItemsCount = killCountCharacterItems.Count;
        for (int i = killCountCharacterItemsCount - 1; i >= 0; i--) {
            CharacterNameplateItem item = killCountCharacterItems[i];
            if (!item.gameObject.activeSelf) {
                return item;
            }
        }
        return null;
    }
    private CharacterNameplateItem GetActiveCharacterNameplateItem(Character character) {
        int killCountCharacterItemsCount = killCountCharacterItems.Count;
        for (int i = 0; i < killCountCharacterItemsCount; i++) {
            CharacterNameplateItem item = killCountCharacterItems[i];
            if (item.gameObject.activeSelf && item.isActive && item.character == character) {
                return item;
            }
        }
        return null;
    }
    private CharacterNameplateItem GetInactiveCharacterNameplateItem(Character character) {
        int killCountCharacterItemsCount = killCountCharacterItems.Count;
        for (int i = killCountCharacterItemsCount - 1; i >= 0; i--) {
            CharacterNameplateItem item = killCountCharacterItems[i];
            if (item.gameObject.activeSelf && !item.isActive && item.character == character) {
                return item;
            }
        }
        return null;
    }
    private void UpdateKillCount() {
        int aliveCount = 0;
        //TODO: Optimize this
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if (/*!character.isFactionless &&*/ character.IsAble() && WillCharacterBeShownInKillCount(character)) {
                aliveCount++;
            }
        }
        killCountLbl.text = $"{aliveCount}/{allFilteredCharactersCount}";
        if (aliveCount <= 0) {
            //player has won
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
            SuccessfulAreaCorruption();
        }
    }
    //private void OrderKillSummaryItems() {
    //    CharacterNameplateItem[] items = GameGameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(killCountScrollView.content.gameObject);
    //    List<CharacterNameplateItem> alive = new List<CharacterNameplateItem>();
    //    List<CharacterNameplateItem> dead = new List<CharacterNameplateItem>();
    //    for (int i = 0; i < items.Length; i++) {
    //        CharacterNameplateItem currItem = items[i];
    //        if (!currItem.character.IsAble() || !LandmarkManager.Instance.enemyOfPlayerArea.region.IsFactionHere(currItem.character.faction)) { //added checking for faction in cases that the character was raised from dead (Myk, if the concern here is only from raise dead, I changed the checker to returnedToLife to avoid conflicts with factions, otherwise you can return it to normal. -Chy)
    //            dead.Add(currItem);
    //        } else {
    //            alive.Add(currItem);
    //        }
    //    }
    //    aliveHeader.transform.SetAsFirstSibling();
    //    for (int i = 0; i < alive.Count; i++) {
    //        CharacterNameplateItem currItem = alive[i];
    //        currItem.transform.SetSiblingIndex(i + 1);
    //    }
    //    deadHeader.transform.SetSiblingIndex(alive.Count + 1);
    //    for (int i = 0; i < dead.Count; i++) {
    //        CharacterNameplateItem currItem = dead[i];
    //        currItem.transform.SetSiblingIndex(alive.Count + i + 2);
    //    }
    //    UpdateKillCount();
    //}
    public void ToggleKillSummary(bool isOn) {
        killSummaryGO.SetActive(isOn);
    }
    public void HideKillSummary() {
        if (killSummaryToggle.isOn) {
            killSummaryToggle.isOn = false;
        }
    }
    private bool WillCharacterBeShownInKillCount(Character character) {
        return character.isStillConsideredAlive;
        //if (character.minion != null || character is Summon || character.faction == PlayerManager.Instance.player.playerFaction
        //    /*|| character.faction == FactionManager.Instance.friendlyNeutralFaction*/) {
        //    //Do not show minions and summons
        //    return false;
        //}
        //return true;
    }
    #endregion

    #region General Confirmation
    public void ShowGeneralConfirmation(string header, string body, string buttonText = "OK", System.Action onClickOK = null) {
        _generalConfirmation.ShowGeneralConfirmation(header, body, buttonText, onClickOK);
    }
    #endregion

    #region New Minion
    [Header("New Minion UI")]
    [SerializeField] private GameObject newMinionUIGO;
    [SerializeField] private MinionCard newMinionCard;
    public void ShowNewMinionUI(Minion minion) {
        if (IsMajorUIShowing()) {
            AddPendingUI(() => ShowNewMinionUI(minion));
            return;
        }
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        newMinionCard.SetMinion(minion);
        newMinionUIGO.SetActive(true);
    }
    public void HideNewMinionUI() {
        newMinionUIGO.SetActive(false);
        if (!TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
        }
    }
    #endregion

    #region Seize
    private void OnSeizePOI(IPointOfInterest poi) {
        DisableTopMenuButtons();
    }
    private void OnUnseizePOI(IPointOfInterest poi) {
        EnableTopMenuButtons();
    }
    public void ShowSeizedObjectUI() {
        // unseizeButton.gameObject.SetActive(true);
    }
    public void HideSeizedObjectUI() {
        // unseizeButton.gameObject.SetActive(false);
    }
    //Not used right now, might be used in the future
    public void UpdateSeizedObjectUI() {
        unseizeButton.gameObject.SetActive(PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
    }
    public void OnClickSeizedObject() {
        // PlayerManager.Instance.player.seizeComponent.PrepareToUnseize();
    }
    #endregion

    #region Spells
    public void OnToggleSpells(bool isOn) {
        if (isOn) {
            ShowSpells();
        } else {
            HideSpells();
        }
    }
    private void ShowSpells() {
        // spellList.ShowDropdown(PlayerManager.Instance.player.archetype.spells, OnClickSpell, CanChooseItem);
        //customDropdownList.ShowDropdown(PlayerManager.Instance.player.archetype.spells, OnClickSpell, CanChooseItem);
        spellsContainerGO.SetActive(true);
    }
    //private bool CanChooseItem(string item) {
    //    //if (item == PlayerDB.Tornado || item == PlayerDB.Meteor || item == PlayerDB.Ravenous_Spirit || item == PlayerDB.Feeble_Spirit || item == PlayerDB.Forlorn_Spirit
    //    //    || item == PlayerDB.Lightning || item == PlayerDB.Poison_Cloud || item == PlayerDB.Locust_Swarm || item == PlayerDB.Earthquake
    //    //    || item == PlayerDB.Locust_Swarm || item == PlayerDB.Spawn_Boulder || item == PlayerDB.Manifest_Food
    //    //    || item == PlayerDB.Brimstones || item == PlayerDB.Water_Bomb || item == PlayerDB.Splash_Poison || item == PlayerDB.Blizzard) {
    //    //    return true;
    //    //}
    //    //return false;
    //    return true;
    //}
    private void HideSpells() {
        spellsContainerGO.SetActive(false);
        //customDropdownList.HideDropdown();
    }
    public void CreateInitialSpells() {
        for (int i = 0; i < PlayerManager.Instance.player.archetype.spells.Count; i++) {
            SPELL_TYPE spell = PlayerManager.Instance.player.archetype.spells[i];
// #if !UNITY_EDITOR
            if(spell == SPELL_TYPE.FEEBLE_SPIRIT || spell == SPELL_TYPE.RAVENOUS_SPIRIT 
                || spell == SPELL_TYPE.FORLORN_SPIRIT){
                continue;
            }
// #endif
            CreateNewSpellItem(spell);
        }
    }
    private void OnGainSpell(SPELL_TYPE spell) {
        CreateNewSpellItem(spell);
    }
    private void OnLostSpell(SPELL_TYPE spell) {
        DeleteSpellItem(spell);
    }
    private void CreateNewSpellItem(SPELL_TYPE spell) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellItemPrefab.name, Vector3.zero, Quaternion.identity, spellsScrollRect.content);
        SpellItem item = go.GetComponent<SpellItem>();
        SpellData spellData = PlayerManager.Instance.GetSpellData(spell);
        if (spellData != null) {
            item.SetSpell(spellData);
        } else {
            spellData = PlayerManager.Instance.GetAfflictionData(spell);
            if (spellData != null) {
                item.SetSpell(spellData);
            } else {
                spellData = PlayerManager.Instance.GetPlayerActionData(spell);
                if (spellData != null) {
                    item.SetSpell(spellData);
                }
            }
        }
        _spellItems.Add(item);
    }
    private void DeleteSpellItem(SPELL_TYPE spell) {
        SpellItem item = GetSpellItem(spell);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
        }
    }
    private SpellItem GetSpellItem(SPELL_TYPE spell) {
        for (int i = 0; i < _spellItems.Count; i++) {
            SpellItem item = _spellItems[i];
            if (item.spellData.type == spell) {
                return item;
            }
        }
        return null;
    }
    //private void OnClickSpell(string spellName) {
    //    if(PlayerManager.Instance.player.currentActivePlayerSpell != null) {
    //        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
    //    }
    //    SPELL_TYPE spell = SPELL_TYPE.NONE;
    //    string enumSpellName = spellName.ToUpper().Replace(' ', '_');
    //    if (!System.Enum.TryParse(enumSpellName, out spell)) {
    //        System.Enum.TryParse(enumSpellName + "_SPELL", out spell);
    //    }
    //    SpellData ability = PlayerManager.Instance.GetSpellData(spell);
    //    PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(ability);
    //}
    #endregion

    #region Summons
    public void OnToggleSummons(bool isOn) {
        if (isOn) {
            ShowSummons();
        } else {
            HideSummons();
        }
    }
    private void ShowSummons() {
        summonsContainerGO.SetActive(true);
    }
    private void HideSummons() {
        summonsContainerGO.SetActive(false);
    }
    public void CreateSummonsForTesting() {
        SUMMON_TYPE[] summons = (SUMMON_TYPE[]) System.Enum.GetValues(typeof(SUMMON_TYPE));
        for (int i = 0; i < summons.Length; i++) {
            if(summons[i] != SUMMON_TYPE.None) {
                CreateNewSummonItem(summons[i]);
            }
        }
    }
    private void CreateNewSummonItem(SUMMON_TYPE summon) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(summonItemPrefab.name, Vector3.zero, Quaternion.identity, summonsScrollRect.content);
        SummonItem item = go.GetComponent<SummonItem>();
        item.SetSummon(summon);
        _summonItems.Add(item);
    }
    #endregion

    #region Tile Objects
    public void OnToggleItems(bool isOn) {
        if (isOn) {
            ShowItems();
        } else {
            HideItems();
        }
    }
    private void ShowItems() {
        itemsContainerGO.SetActive(true);
    }
    private void HideItems() {
        itemsContainerGO.SetActive(false);
    }
    public void CreateItemsForTesting() {
        TILE_OBJECT_TYPE[] items = new[] { TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL, TILE_OBJECT_TYPE.FIRE_CRYSTAL, TILE_OBJECT_TYPE.ICE_CRYSTAL, TILE_OBJECT_TYPE.POISON_CRYSTAL, TILE_OBJECT_TYPE.WATER_CRYSTAL, TILE_OBJECT_TYPE.SNOW_MOUND, TILE_OBJECT_TYPE.WINTER_ROSE, TILE_OBJECT_TYPE.DESERT_ROSE };
        for (int i = 0; i < items.Length; i++) {
            CreateNewItemItem(items[i]);
        }
    }
    private void CreateNewItemItem(TILE_OBJECT_TYPE item) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(itemItemPrefab.name, Vector3.zero, Quaternion.identity, itemsScrollRect.content);
        ItemItem itemItem = go.GetComponent<ItemItem>();
        itemItem.SetItem(item);
        _itemItems.Add(itemItem);
    }
    public void OnToggleArtifacts(bool isOn) {
        if (isOn) {
            ShowArtifacts();
        } else {
            HideArtifacts();
        }
    }
    private void ShowArtifacts() {
        artifactsContainerGO.SetActive(true);
    }
    private void HideArtifacts() {
        artifactsContainerGO.SetActive(false);
    }
    public void CreateArtifactsForTesting() {
        ARTIFACT_TYPE[] artifacts = (ARTIFACT_TYPE[]) System.Enum.GetValues(typeof(ARTIFACT_TYPE));
        for (int i = 0; i < artifacts.Length; i++) {
            if(artifacts[i] != ARTIFACT_TYPE.None) {
                CreateNewArtifactItem(artifacts[i]);
            }
        }
    }
    private void CreateNewArtifactItem(ARTIFACT_TYPE artifact) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(artifactItemPrefab.name, Vector3.zero, Quaternion.identity, artifactsScrollRect.content);
        ArtifactItem artifactArtifact = go.GetComponent<ArtifactItem>();
        artifactArtifact.SetArtifact(artifact);
        _artifactItems.Add(artifactArtifact);
    }
    #endregion

    #region Faction Actions
    public void OnToggleFactionActions(bool isOn) {
        if (isOn) {
            ShowFactionActions();
        } else {
            HideFactionActions();
        }
    }
    private void ShowFactionActions() {
        customDropdownList.ShowDropdown(factionActionsList, OnClickFactionAction);
    }
    private void HideFactionActions() {
        spellList.Close();
    }
    private void OnClickFactionAction(string text) {
        //TODO
    }
    #endregion

    #region Top Menu
    private void EnableTopMenuButtons() {
        for (int i = 0; i < topMenuButtons.Length; i++) {
            topMenuButtons[i].interactable = true;
        }
    }
    private void DisableTopMenuButtons() {
        for (int i = 0; i < topMenuButtons.Length; i++) {
            topMenuButtons[i].interactable = false;
        }
    }
    #endregion

    #region NPCSettlement Actions
    public void OnClickHarassRaidInvade(HexTile targetHex, string identifier) {
        harassRaidInvadeTargetNpcSettlement = targetHex.settlementOnTile as NPCSettlement;
        harassRaidInvade = identifier;
        UIManager.Instance.ShowClickableObjectPicker(PlayerManager.Instance.player.minions.Where(x => x.character.gridTileLocation != null).Select(x => x.character).ToList(), HarassRaidInvade
            , null, CanChooseMinion, "Choose Leader Minion", showCover: true);
    }
    private bool CanChooseMinion(Character character) {
        return !character.isDead && !character.behaviourComponent.isHarassing && !character.behaviourComponent.isRaiding && !character.behaviourComponent.isInvading;
    }
    private void HarassRaidInvade(object obj) {
        Character character = obj as Character;
        harassRaidInvadeLeaderMinion = character.minion;
        UIManager.Instance.HideObjectPicker();
        if(PlayerManager.Instance.player.summons.Count > 0) {
            unleashSummonUI.ShowUnleashSummonUI();
        } else {
            //harassRaidInvadeLeaderMinion.character.behaviourComponent.SetHarassInvadeRaidTarget(harassRaidInvadeTargetNpcSettlement);
            if (harassRaidInvade == "harass") {
                harassRaidInvadeLeaderMinion.character.behaviourComponent.SetIsHarassing(true, harassRaidInvadeTargetNpcSettlement);
                PlayerManager.Instance.GetPlayerActionData(SPELL_TYPE.HARASS).OnExecuteSpellActionAffliction();
            } else if (harassRaidInvade == "raid") {
                harassRaidInvadeLeaderMinion.character.behaviourComponent.SetIsRaiding(true, harassRaidInvadeTargetNpcSettlement);
                PlayerManager.Instance.GetPlayerActionData(SPELL_TYPE.RAID).OnExecuteSpellActionAffliction();
            } else if (harassRaidInvade == "invade") {
                harassRaidInvadeLeaderMinion.character.behaviourComponent.SetIsInvading(true, harassRaidInvadeTargetNpcSettlement);
                PlayerManager.Instance.GetPlayerActionData(SPELL_TYPE.INVADE).OnExecuteSpellActionAffliction();
            }
            PlayerManager.Instance.player.threatComponent.AdjustThreat(5);
            
        }
    }
    #endregion
}
