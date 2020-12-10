using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Linq;
using System;
using DG.Tweening;
using Inner_Maps;
using Settings;
using Traits;
using Tutorial;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UtilityScripts;
using Random = UnityEngine.Random;

public class PlayerUI : BaseMonoBehaviour {
    public static PlayerUI Instance;

    [Header("Top Menu")]
    public GameObject regionNameTopMenuGO;
    public TextMeshProUGUI regionNameTopMenuText;
    public HoverHandler regionNameHoverHandler;
    
    [Header("Mana")]
    public TextMeshProUGUI manaLbl;
    [SerializeField] private RectTransform manaContainer;

    [Header("Intel")]
    public GameObject intelContainer;
    [SerializeField] private IntelItem[] intelItems;
    public Toggle intelToggle;

    [Header("Provoke")]
    [SerializeField] private ProvokeMenu provokeMenu;

    [Header("Miscellaneous")]
    [SerializeField] private GameObject successfulAreaCorruptionGO;
    [SerializeField] private ScrollRect killSummaryScrollView;

    [Header("General Confirmation")] 
    [SerializeField] private GeneralConfirmation _generalConfirmation;
    
    [Header("Intervention Abilities")]
    [SerializeField] private GameObject actionBtnPrefab;

    [Header("Saving/Loading")]
    public Button saveGameButton;

    [Header("End Game Mechanics")]
    [SerializeField] private WinGameOverItem winGameOver;
    [SerializeField] private LoseGameOverItem loseGameOver;

    
    [Header("Villagers")]
    [FormerlySerializedAs("killSummaryGO")] [SerializeField] private GameObject villagerGO;
    [SerializeField] private GameObject villagerItemPrefab;
    [FormerlySerializedAs("killCountScrollView")] [SerializeField] private ScrollRect villagersScrollView;
    [SerializeField] private RectTransform aliveHeader;
    [SerializeField] private Toggle villagerTab;
    public RectTransform deadHeader;
    private List<CharacterNameplateItem> villagerItems;
    
    [Header("Seize Object")]
    [SerializeField] private Button unseizeButton;

    [Header("Top Menu")]
    [SerializeField] private Toggle[] topMenuButtons;
    [SerializeField] private SpellListUI spellList;
    [SerializeField] private CustomDropdownList customDropdownList;
    [SerializeField] private CultistsListUI cultistsList;
    public Toggle monsterToggle;
    
    [Header("Minion List")]
    [SerializeField] private MinionListUI minionList;
    public UIHoverPosition minionListHoverPosition;
    private readonly List<string> factionActionsList = new List<string>() { "Manage Cult", "Meddle" };

    [Header("Player Actions")]
    public SpellSpriteDictionary playerActionsIconDictionary;
    private List<System.Action> pendingUIToShow { get; set; }

    [Header("Spells")]
    public ScrollRect spellsScrollRect;
    public GameObject spellsContainerGO;
    public GameObject spellItemPrefab;
    public PlayerSkillDetailsTooltip skillDetailsTooltip;
    public UIHoverPosition spellListHoverPosition;
    private List<SpellItem> _spellItems;

    [Header("Summons")]
    [SerializeField] private SummonListUI summonList;

    [Header("Items")] 
    public Toggle itemsToggle;
    public ScrollRect itemsScrollRect;
    public GameObject itemsContainerGO;
    public GameObject itemItemPrefab;
    private List<ItemItem> _itemItems;

    [Header("Artifacts")]
    public Toggle artifactsToggle;
    public ScrollRect artifactsScrollRect;
    public GameObject artifactsContainerGO;
    public GameObject artifactItemPrefab;
    private List<ArtifactItem> _artifactItems;
    
    [Header("Threat")]
    [SerializeField] private TextMeshProUGUI threatLbl;
    [SerializeField] private UIHoverPosition threatHoverPos;
    [SerializeField] private RectTransform threatContainer;

    [Header("Building")] 
    [SerializeField] private BuildListUI _buildListUI;
    
    [Header("Plague Points")]
    [SerializeField] private TextMeshProUGUI plaguePointLbl;
    [SerializeField] private RectTransform plaguePointsContainer;
    
    public HexTile harassDefendInvadeTargetHex { get; private set; }

    void Awake() {
        Instance = this;
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
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
        _itemItems = new List<ItemItem>();
        _artifactItems = new List<ArtifactItem>();

        minionList.Initialize();
        summonList.Initialize();
        plaguePointsContainer.gameObject.SetActive(false);

        Messenger.AddListener<InfoUIBase>(UISignals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<InfoUIBase>(UISignals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(PlayerSignals.UPDATED_CURRENCIES, UpdateUI);
        Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_OBTAINED_INTEL, OnIntelObtained);
        Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_REMOVED_INTEL, OnIntelRemoved);

        Messenger.AddListener(UISignals.ON_OPEN_CONVERSATION_MENU, OnOpenConversationMenu);
        Messenger.AddListener(UISignals.ON_CLOSE_CONVERSATION_MENU, OnCloseConversationMenu);
        
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
        
        Messenger.AddListener<PLAYER_SKILL_TYPE>(SpellSignals.PLAYER_GAINED_SPELL, OnGainSpell);
        Messenger.AddListener<PLAYER_SKILL_TYPE>(SpellSignals.PLAYER_LOST_SPELL, OnLostSpell);
    }

    public void InitializeAfterGameLoaded() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnCharacterLostTrait);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CREATED, AddedNewCharacter);
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.AddListener<Character, Character>(CharacterSignals.ON_SWITCH_FROM_LIMBO, OnCharacterSwitchFromLimbo);
        Messenger.AddListener(PlayerSignals.THREAT_UPDATED, OnThreatUpdated);
        Messenger.AddListener<int>(PlayerSignals.THREAT_INCREASED, OnThreatIncreased);
        Messenger.AddListener(PlayerSignals.THREAT_RESET, OnThreatReset);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnAddNewCharacter);
        Messenger.AddListener<Character>(CharacterSignals.NECROMANCER_SPAWNED, OnNecromancerSpawned);
        Messenger.AddListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, UpdatePlaguePointsAmount);

        //key presses
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyPressed);

        //currencies
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
        InitialUpdateVillagerListCharacterItems();
        InitializeIntel();
#if UNITY_EDITOR
        itemsToggle.gameObject.SetActive(true);
        artifactsToggle.gameObject.SetActive(true);
        CreateItemsForTesting();
        CreateArtifactsForTesting();
#else
        itemsToggle.gameObject.SetActive(false);
        artifactsToggle.gameObject.SetActive(false);        
#endif
    }
    public void InitializeAfterLoadOutPicked() {
        UpdateIntel();
        CreateInitialSpells();
        _buildListUI.Initialize();
        cultistsList.Initialize();

        cultistsList.UpdateList();
        minionList.UpdateList();
        summonList.UpdateList();

        OnThreatUpdated();
        UpdatePlaguePointsContainer();
        UpdatePlaguePointsAmount(PlayerManager.Instance.player.plagueComponent.plaguePoints);
    }

    #region Listeners
    private void OnInnerMapOpened(Region location) {
        UpdateRegionNameState();
    }
    private void OnInnerMapClosed(Region location) {
        UpdateRegionNameState();
    }
    private void OnKeyPressed(KeyCode pressedKey) {
        if (pressedKey == KeyCode.F9) {
            UIManager.Instance.optionsMenu.QuickSave();
        }
    }
    private void OnCharacterDied(Character character) {
        TransferCharacterFromActiveToInactive(character);
    }
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (trait is Cultist) {
            CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
            if (item != null) {
                ObjectPoolManager.Instance.DestroyObject(item);
            }
        }
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
        //if (faction?.factionType.type == FACTION_TYPE.Wild_Monsters) {
        //    TransferCharacterFromActiveToInactive(character);
        //} else 
        if (faction.isPlayerFaction || faction?.factionType.type == FACTION_TYPE.Undead) {
            OnCharacterBecomesMinionOrSummon(character);
        } else {
            TransferCharacterFromInactiveToActive(character);
        }
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
        // if (!toCharacter.IsAble()/* || toCharacter.isFactionless*/) {
        //     TransferCharacterFromActiveToInactive(toCharacter);
        // } else if (toCharacter.faction.isPlayerFaction /*|| faction == FactionManager.Instance.friendlyNeutralFaction*/) {
        //     OnCharacterBecomesMinionOrSummon(toCharacter);
        // } else {
        //     TransferCharacterFromInactiveToActive(toCharacter);
        // }
    }
    private void AddedNewCharacter(Character character) {
        // OnAddNewCharacter(character);
    }
    private void CharacterBecomesMinionOrSummon(Character character) {
        //OnCharacterBecomesMinionOrSummon(character);
    }
    private void CharacterBecomesNonMinionOrSummon(Character character) {
        OnCharacterBecomesNonMinionOrSummon(character);
    }
    private void OnNecromancerSpawned(Character character) {
        OnCharacterBecomesNecromancer(character);
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
        threatLbl.text = PlayerManager.Instance.player.threatComponent.threat.ToString();
        //threatLbl.transform.DOPunchScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
    }
    private void OnThreatIncreased(int amount) {
        var text = $"<color=\"red\">+{amount.ToString()}</color>";
        GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", threatLbl.transform.position,
            Quaternion.identity, transform, true);
        effectGO.GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(Random.Range(-25, 25), -70f));
        DoThreatPunchEffect();
    }
    private Tweener _currentThreatPunchTween;
    public void DoThreatPunchEffect() {
        if (_currentThreatPunchTween == null) {
            _currentThreatPunchTween = threatContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(() => _currentThreatPunchTween = null);    
        }
    }
    private void OnThreatReset() {
        var text = $"<color=\"green\">-{ThreatComponent.MAX_THREAT.ToString()}</color>";
        GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", threatLbl.transform.position,
            Quaternion.identity, transform, true);
        effectGO.GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(Random.Range(-25, 25), -70f));
        DoThreatPunchEffect();
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            regionNameHoverHandler.SetOnHoverOverAction(() => TestingUtilities.ShowLocationInfo(location.coreTile.region));
            regionNameHoverHandler.SetOnHoverOutAction(TestingUtilities.HideLocationInfo);
#endif
        } else {
            regionNameTopMenuGO.SetActive(false);
        }
    }

    #region Mana
    private void OnManaAdjusted(int adjustedAmount, int mana) {
        UpdateMana();
        ShowManaAdjustEffect(adjustedAmount);
        DoManaPunchEffect();
        AudioManager.Instance.PlayParticleMagnet();
    }
    private void UpdateMana() {
        manaLbl.text = PlayerManager.Instance.player.mana.ToString();
    }
    private Tweener _currentManaPunchTween;
    private void DoManaPunchEffect() {
        if (_currentManaPunchTween == null) {
            _currentManaPunchTween = manaContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(() => _currentManaPunchTween = null);    
        }
    }
    private void ShowManaAdjustEffect(int adjustmentAmount) {
        var text = adjustmentAmount > 0 ? $"<color=\"green\">+{adjustmentAmount.ToString()}</color>" : $"<color=\"red\">{adjustmentAmount.ToString()}</color>";
        GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", manaLbl.transform.position,
            Quaternion.identity, transform, true);
        effectGO.GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(Random.Range(-25, 25), -70f));
    }
    #endregion

    #region Miscellaneous
    private Tweener _currentMonsterTabTween;
    public void DoMonsterTabPunchEffect() {
        if (_currentMonsterTabTween == null) {
            _currentMonsterTabTween = monsterToggle.transform.DOPunchScale(new Vector3(2f, 2f, 1f), 0.2f).OnComplete(() => _currentMonsterTabTween = null);    
        }
    }
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
        return _generalConfirmation.isShowing /*|| newMinionUIGO.activeInHierarchy*/ || 
               UIManager.Instance.generalConfirmationWithVisual.isShowing || 
               UIManager.Instance.yesNoConfirmation.yesNoGO.activeInHierarchy;
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
                currItem.SetOnHoverEnterAction(() => OnHoverEnterStoredIntel(intel));
                currItem.SetOnHoverExitAction(OnHoverExitStoredIntel);
            }
        }
    }
    private void OnHoverEnterStoredIntel(IIntel intel) {
        string blackmailText = intel.GetIntelInfoBlackmailText();
        string reactionText = intel.GetIntelInfoRelationshipText();
        string text = string.Empty;

        text += blackmailText;
        if (!string.IsNullOrEmpty(text)) {
            text += "\n";
        }
        text += reactionText;

        if (!string.IsNullOrEmpty(text)) {
            UIManager.Instance.ShowSmallInfo(text);
        }
    }
    private void OnHoverExitStoredIntel() {
        UIManager.Instance.HideSmallInfo();
    }
    private void InitializeIntel() {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            currItem.SetIntel(null);
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
    private void OnOpenConversationMenu() {
        intelToggle.isOn = false;
        intelToggle.interactable = false;
    }
    private void OnCloseConversationMenu() {
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
        if (state) {
            Messenger.Broadcast(UISignals.INTEL_MENU_OPENED);    
        }
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
    private Tweener _currentIntelPunchEffect;
    public void DoIntelTabPunchEffect() {
        if (_currentIntelPunchEffect == null) {
            _currentIntelPunchEffect = intelToggle.transform.DOPunchScale(new Vector3(2f, 2f, 1f), 0.2f).OnComplete(() => _currentIntelPunchEffect = null);
        }
    }
    #endregion

    #region Provoke
    public void OpenProvoke(Character actor, Character target) {
        provokeMenu.Open(actor, target);
    }
    #endregion
    
    #region Corruption and Threat
    public void OnHoverEnterThreat() {
        string text = "The amount of threat you've generated in this world. Once this reaches 100, characters will start attacking your structures.";
        UIManager.Instance.ShowSmallInfo(text, threatHoverPos, "Threat");
    }
    public void OnHoverExitThreat() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region End Game Mechanics
    public void WinGameOver() {
        SaveManager.Instance.currentSaveDataPlayer.OnWorldCompleted(WorldSettings.Instance.worldSettingsData.worldType);
        UIManager.Instance.ShowEndDemoScreen("You managed to wipe out all Villagers. Congratulations!");
        // if (WorldConfigManager.Instance.isTutorialWorld) {
        //     UIManager.Instance.ShowEndDemoScreen("You managed to wipe out all Villagers. Congratulations!");
        // } else {
            // UIManager.Instance.Pause();
            // winGameOver.Open();    
        // }
        
    }
    public void LoseGameOver() {
        UIManager.Instance.ShowEndDemoScreen("The Portal is in ruins! \nYour invasion has ended prematurely.");
        // if (WorldConfigManager.Instance.isTutorialWorld) {
        //     UIManager.Instance.ShowEndDemoScreen("The Portal is in ruins! \nYour invasion has ended prematurely.");
        // } else {
            // UIManager.Instance.Pause();
            // loseGameOver.Open();
        // }
    }
    #endregion

    #region Settlement Corruption
    public void SuccessfulAreaCorruption() {
        successfulAreaCorruptionGO.SetActive(true);
        //Utilities.DestroyChildren(killSummaryScrollView.content);
        LoadKillSummaryCharacterItems();
    }
    private void LoadKillSummaryCharacterItems() {
        CharacterNameplateItem[] items = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<CharacterNameplateItem>(villagersScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            CharacterNameplateItem item = items[i];
            item.transform.SetParent(killSummaryScrollView.content);
        }
    }
    #endregion

    #region Villagers
    public void ToggleVillagersTab(bool isOn) {
        if (isOn) {
            FactionInfoHubUI.Instance.Open();
            FactionInfoHubUI.Instance.ShowMembers();
            //OpenVillagersList();
        } else {
            FactionInfoHubUI.Instance.Close();
            //CloseVillagersList();
        }
    }
    public void SetVillagerTabIsOn(bool state) {
        villagerTab.isOn = state;
    }
    public void OpenVillagersList() {
        villagerGO.SetActive(true);
        villagerTab.SetIsOnWithoutNotify(true);
    }
    public void CloseVillagersList() {
        villagerGO.SetActive(false);
        villagerTab.SetIsOnWithoutNotify(false);
    }
    private void LoadVillagerItems(int itemsToCreate) {
        villagerItems = new List<CharacterNameplateItem>();
        for (int i = 0; i < itemsToCreate; i++) {
            CreateNewVillagerItem();
        }
    }
    private CharacterNameplateItem CreateNewVillagerItem() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(villagerItemPrefab.name, Vector3.zero, Quaternion.identity, villagersScrollView.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        go.SetActive(false);
        villagerItems.Add(item);
        return item;
    }
    private void InitialUpdateVillagerListCharacterItems() {
        List<Character> villagers = CharacterManager.Instance.allCharacters.Where(x => x.isNormalEvenLycanAndNotAlliedWithPlayer && !x.isPreplaced).ToList();
        int allVillagersCount = villagers.Count;
        LoadVillagerItems(allVillagersCount);
        
        List<CharacterNameplateItem> alive = new List<CharacterNameplateItem>();
        List<CharacterNameplateItem> dead = new List<CharacterNameplateItem>();

        for (int i = 0; i < allVillagersCount; i++) {
            Character character = villagers[i];
            CharacterNameplateItem item = villagerItems[i];
            
            item.SetObject(character);
            item.SetAsButton();
            item.ClearAllOnClickActions();
            item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, true));
            item.gameObject.SetActive(true);
            if (!character.IsAble()) {
                dead.Add(item);
            } else {
                alive.Add(item);
            }
        }

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
    }
    private void OnAddNewCharacter(Character character) {
        if (!character.isNormalEvenLycanAndNotAlliedWithPlayer || character.isPreplaced) {
            //Do not show minions and summons
            return;
        }
        CharacterNameplateItem item = GetUnusedCharacterNameplateItem();
        if(item == null) {
            item = CreateNewVillagerItem();
        }
        item.SetObject(character);
        item.SetAsButton();
        item.ClearAllOnClickActions();
        item.AddOnClickAction((c) => UIManager.Instance.ShowCharacterInfo(c, true));
        item.gameObject.SetActive(true);
        if (!character.IsAble()) {
            //if (allFilteredCharactersCount == villagerItems.Count) {
            //    item.transform.SetAsLastSibling();
            //} else {
            //    item.transform.SetSiblingIndex(allFilteredCharactersCount + 2);
            //}
            item.transform.SetAsLastSibling();
            item.SetIsActive(false);
        } else {
            item.transform.SetSiblingIndex(deadHeader.transform.GetSiblingIndex());
            item.SetIsActive(true);
        }
    }
    private void TransferCharacterFromActiveToInactive(Character character) {
        if (!character.isNormalEvenLycanAndNotAlliedWithPlayer || character.isPreplaced) {
            return;
        }
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if(item != null) {
            // if (allFilteredCharactersCount == villagerItems.Count) {
            //     item.transform.SetAsLastSibling();
            // } else {
            //     item.transform.SetSiblingIndex(allFilteredCharactersCount + 2);
            // }
            // item.SetIsActive(false);
            item.transform.SetAsLastSibling(); //put character item at the bottom of the list.
        }
        //UpdateKillCount();
    }
    private void TransferCharacterFromInactiveToActive(Character character) {
        if (!character.isNormalEvenLycanAndNotAlliedWithPlayer || character.isPreplaced) {
            return;
        }
        CharacterNameplateItem item = GetInactiveCharacterNameplateItem(character);
        if (item != null) {
            int index = item.transform.GetSiblingIndex();
            int deadHeaderIndex = deadHeader.transform.GetSiblingIndex();
            item.transform.SetSiblingIndex(deadHeaderIndex);
            item.SetIsActive(true);
        }
        //UpdateKillCount();
    }
    private void OnCharacterBecomesMinionOrSummon(Character character) {
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if (item != null) {
            item.gameObject.SetActive(false);
            //UpdateKillCount();
        } else {
            item = GetInactiveCharacterNameplateItem(character);
            if (item != null) {
                item.gameObject.SetActive(false);
                //UpdateKillCount();
            }
        }
    }
    private void OnCharacterBecomesNonMinionOrSummon(Character character) {
        // OnAddNewCharacter(character);
    }
    private void OnCharacterBecomesNecromancer(Character character) {
        CharacterNameplateItem item = GetActiveCharacterNameplateItem(character);
        if (item != null) {
            item.gameObject.SetActive(false);
        }
    }
    private CharacterNameplateItem GetUnusedCharacterNameplateItem() {
        int killCountCharacterItemsCount = villagerItems.Count;
        for (int i = killCountCharacterItemsCount - 1; i >= 0; i--) {
            CharacterNameplateItem item = villagerItems[i];
            if (!item.gameObject.activeSelf) {
                return item;
            }
        }
        return null;
    }
    private CharacterNameplateItem GetActiveCharacterNameplateItem(Character character) {
        int killCountCharacterItemsCount = villagerItems.Count;
        for (int i = 0; i < killCountCharacterItemsCount; i++) {
            CharacterNameplateItem item = villagerItems[i];
            if (item.gameObject.activeSelf && item.isActive && item.character == character) {
                return item;
            }
        }
        return null;
    }
    private CharacterNameplateItem GetInactiveCharacterNameplateItem(Character character) {
        int killCountCharacterItemsCount = villagerItems.Count;
        for (int i = killCountCharacterItemsCount - 1; i >= 0; i--) {
            CharacterNameplateItem item = villagerItems[i];
            if (item.gameObject.activeSelf && !item.isActive && item.character == character) {
                return item;
            }
        }
        return null;
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
        villagerGO.SetActive(isOn);
    }
    #endregion

    #region General Confirmation
    public void ShowGeneralConfirmation(string header, string body, string buttonText = "OK", System.Action onClickOK = null, System.Action onClickCenter = null) {
        _generalConfirmation.ShowGeneralConfirmation(header, body, buttonText, onClickOK, onClickCenter);
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
        spellsContainerGO.SetActive(true);
        Messenger.Broadcast(UISignals.SPELLS_MENU_SHOWN);
    }
    private void HideSpells() {
        spellsContainerGO.SetActive(false);
        //customDropdownList.HideDropdown();
    }
    private void CreateInitialSpells() {
        for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.spells.Count; i++) {
            PLAYER_SKILL_TYPE spell = PlayerManager.Instance.player.playerSkillComponent.spells[i];
            CreateNewSpellItem(spell);
        }
    }
    private void OnGainSpell(PLAYER_SKILL_TYPE spell) {
        CreateNewSpellItem(spell);
    }
    private void OnLostSpell(PLAYER_SKILL_TYPE spell) {
        DeleteSpellItem(spell);
    }
    private void CreateNewSpellItem(PLAYER_SKILL_TYPE spell) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellItemPrefab.name, Vector3.zero, Quaternion.identity, spellsScrollRect.content);
        SpellItem item = go.GetComponent<SpellItem>();
        go.SetActive(false);
        SpellData spellData = PlayerSkillManager.Instance.GetSpellData(spell);
        if (spellData != null) {
            item.SetObject(spellData);
        } else {
            spellData = PlayerSkillManager.Instance.GetAfflictionData(spell);
            if (spellData != null) {
                item.SetObject(spellData);
            } else {
                spellData = PlayerSkillManager.Instance.GetPlayerActionData(spell);
                if (spellData != null) {
                    item.SetObject(spellData);
                }
            }
        }
        go.SetActive(true);
        // if (WorldConfigManager.Instance.isTutorialWorld) {
        //     //in demo world, only allow spells that are set to be available.
        //     bool isInteractable = WorldConfigManager.Instance.availableSpellsInTutorial.Contains(spell);
        //     item.SetInteractableState(isInteractable);
        //     if (isInteractable) {
        //         item.transform.SetAsFirstSibling();
        //     } else {
        //         item.SetLockedState(true);
        //         item.transform.SetAsLastSibling();
        //     }
        // }
        _spellItems.Add(item);
    }
    private void DeleteSpellItem(PLAYER_SKILL_TYPE spell) {
        SpellItem item = GetSpellItem(spell);
        if (item != null) {
            ObjectPoolManager.Instance.DestroyObject(item.gameObject);
        }
    }
    private SpellItem GetSpellItem(PLAYER_SKILL_TYPE spell) {
        for (int i = 0; i < _spellItems.Count; i++) {
            SpellItem item = _spellItems[i];
            if (item.spellData.type == spell) {
                return item;
            }
        }
        return null;
    }
    public void OnHoverSpell(SpellData skillData, UIHoverPosition position = null) {
        skillDetailsTooltip.ShowPlayerSkillDetails(skillData, position);
    }
    public void OnHoverOutSpell(SpellData skillData) {
        skillDetailsTooltip.HidePlayerSkillDetails();
    }
    #endregion

    #region Summons
    //public void OnToggleSummons(bool isOn) {
    //    if (isOn) {
    //        ShowSummons();
    //    } else {
    //        HideSummons();
    //    }
    //}
    //private void ShowSummons() {
    //    summonsContainerGO.SetActive(true);
    //}
    //private void HideSummons() {
    //    summonsContainerGO.SetActive(false);
    //}
    //private void CreateNewSummonItem(Summon summon) {
    //    GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterNameplateItem.name, Vector3.zero, Quaternion.identity, summonsScrollRect.content);
    //    CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
    //    item.SetAsDefaultBehaviour();
    //    item.SetObject(summon);
    //    item.gameObject.SetActive(true);
    //    _summonItems.Add(item);
    //}
    //private void RemoveSummonItem(Summon summon) {
    //    for (int i = 0; i < _summonItems.Count; i++) {
    //        CharacterNameplateItem summonItem = _summonItems[i];
    //        if (summonItem.character == summon) {
    //            _summonItems.Remove(summonItem); 
    //            ObjectPoolManager.Instance.DestroyObject(summonItem.gameObject);
    //            break;
    //        }
    //    }
    //}
    
    // public void CreateSummonsForTesting() {
    //     SUMMON_TYPE[] summons = (SUMMON_TYPE[]) System.Enum.GetValues(typeof(SUMMON_TYPE));
    //     for (int i = 0; i < summons.Length; i++) {
    //         if(summons[i] != SUMMON_TYPE.None) {
    //             CreateNewSummonItem(summons[i]);
    //         }
    //     }
    // }
    // private void CreateNewSummonItem(SUMMON_TYPE summon) {
    //     GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(summonItemPrefab.name, Vector3.zero, Quaternion.identity, summonsScrollRect.content);
    //     SummonItem item = go.GetComponent<SummonItem>();
    //     item.SetSummon(summon);
    //     _summonItems.Add(item);
    // }
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
    private void CreateItemsForTesting() {
        TILE_OBJECT_TYPE[] items = new[] {
            TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL, TILE_OBJECT_TYPE.FIRE_CRYSTAL, TILE_OBJECT_TYPE.ICE_CRYSTAL,
            TILE_OBJECT_TYPE.POISON_CRYSTAL, TILE_OBJECT_TYPE.WATER_CRYSTAL, TILE_OBJECT_TYPE.SNOW_MOUND,
            TILE_OBJECT_TYPE.WINTER_ROSE, TILE_OBJECT_TYPE.DESERT_ROSE, TILE_OBJECT_TYPE.CULTIST_KIT,
            TILE_OBJECT_TYPE.TREASURE_CHEST, TILE_OBJECT_TYPE.ICE, TILE_OBJECT_TYPE.HERB_PLANT, TILE_OBJECT_TYPE.ANIMAL_MEAT, TILE_OBJECT_TYPE.PROFESSION_PEDESTAL
        };
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

    #region Build List
    public void OnToggleBuildList(bool isOn) {
        if (isOn) {
            _buildListUI.Open();
        } else {
            _buildListUI.Close();
        }
    }
    #endregion

    #region Plague
    private Tweener _currentPlaguePointPunchTween;
    private void DoPlaguePointPunchEffect() {
        if (_currentPlaguePointPunchTween == null) {
            _currentPlaguePointPunchTween = plaguePointsContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(() => _currentPlaguePointPunchTween = null);    
        }
    }
    public void ShowPlaguePointsGainedEffect(int adjustmentAmount) {
        if (plaguePointsContainer.gameObject.activeSelf) {
            DoPlaguePointPunchEffect();
            var text = $"<color=#FE4D60>+{adjustmentAmount.ToString()}{UtilityScripts.Utilities.PlagueIcon()}</color>";
            GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", plaguePointLbl.transform.position, Quaternion.identity, transform, true);
            effectGO.GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(Random.Range(-25, 25), -70f));    
        }
    }
    private void UpdatePlaguePointsAmount(int p_amount) {
        plaguePointLbl.text = p_amount.ToString();
    }
    private void UpdatePlaguePointsContainer() {
        plaguePointsContainer.gameObject.SetActive(PlayerSkillManager.Instance.GetDemonicStructureSkillData(PLAYER_SKILL_TYPE.BIOLAB).isInUse);
    }
    public void OnHoverEnterPlaguePoints() {
        string text = "The amount of Plague Points you've generated. You can use this to upgrade your Plague if you have a Biolab built";
        UIManager.Instance.ShowSmallInfo(text, threatHoverPos, $"{UtilityScripts.Utilities.PlagueIcon()} Plague Points");
    }
    public void OnHoverExitPlaguePoints() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnClickPlaguePoints() {
        if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB)) {
            UIManager.Instance.ShowBiolabUI();    
        }
    }
    #endregion
}
