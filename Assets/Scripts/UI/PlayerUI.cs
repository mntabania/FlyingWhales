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

    //[Header("Top Menu")]
    //public GameObject regionNameTopMenuGO;
    //public TextMeshProUGUI regionNameTopMenuText;
    //public HoverHandler regionNameHoverHandler;

    [Header("Spirit Energy")]
    public TextMeshProUGUI spiritEnergyLabel;
    [SerializeField] private RectTransform spiritEnergyContainer;

    [Header("Mana")]
    public TextMeshProUGUI manaLbl;
    [SerializeField] private RectTransform manaContainer;
    [SerializeField] private UIHoverPosition manaTooltipPos;

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
    [SerializeField] private Toggle villagerTab;

    [Header("Seize Object")]
    [SerializeField] private Button unseizeButton;

    [Header("Top Menu")]
    [SerializeField] private Toggle[] topMenuButtons;
    [SerializeField] private SpellListUI spellList;
    [SerializeField] private CustomDropdownList customDropdownList;
    [SerializeField] private CultistsListUI cultistsList;
    [SerializeField] private TargetsListUI targetsList;
    public Toggle monsterToggle;
    public Toggle targetsToggle;

    [Header("Minion List")]
    [SerializeField] private MinionListUI minionList;
    public UIHoverPosition minionListHoverPosition;
    private readonly List<string> factionActionsList = new List<string>() { "Manage Cult", "Meddle" };

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
    [SerializeField] public TextMeshProUGUI plaguePointLbl;
    [SerializeField] private RectTransform plaguePointsContainer;

    [Header("Accumulated Damage")]
    public TextMeshProUGUI accumulatedDamageLbl;
    public GameObject accumulatedDamageGO;

    [Header("Skill Pup Up Notif Position")]
    public Transform popUpDisplayPoint;

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
        UpdateSpiritEnergy();
    }

    public void Initialize() {
        pendingUIToShow = new List<Action>();
        _spellItems = new List<SpellItem>();
        _itemItems = new List<ItemItem>();
        _artifactItems = new List<ArtifactItem>();

        minionList.Initialize();
        summonList.Initialize();

        Messenger.AddListener(PlayerSignals.UPDATED_CURRENCIES, UpdateUI);
        Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_OBTAINED_INTEL, OnIntelObtained);
        Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_REMOVED_INTEL, OnIntelRemoved);

        Messenger.AddListener(UISignals.ON_OPEN_CONVERSATION_MENU, OnOpenConversationMenu);
        Messenger.AddListener(UISignals.ON_CLOSE_CONVERSATION_MENU, OnCloseConversationMenu);
        
        //Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
        //Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);
        
        Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_GAINED_SPELL, OnGainSpell);
        Messenger.AddListener<PLAYER_SKILL_TYPE>(PlayerSkillSignals.PLAYER_LOST_SPELL, OnLostSpell);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.ON_FINISH_UNDERLING_COOLDOWN, OnFinishUnderlingCoolDown);
        AdjustUIDisplayBaseOnGameMode();
        
    }

    void OnFinishUnderlingCoolDown(MonsterAndDemonUnderlingCharges p_playerUnderlingComponent) {
        string displayName = CharacterManager.Instance.GetOrCreateCharacterClassData(p_playerUnderlingComponent.characterClassName).displayName;
        string rawString = displayName;
        rawString += " is now available";
        PopUpTextNotification.ShowPlayerPoppingTextNotif($"{UtilityScripts.Utilities.YellowDotIcon()}{UtilityScripts.Utilities.ColorizeName(displayName)} is now available", popUpDisplayPoint, rawString.Length);
    }

    void OnSpellCooldownFinished(SkillData p_skillData) {
        string rawString = p_skillData.name;
        if (p_skillData.category != PLAYER_SKILL_CATEGORY.MINION && p_skillData.category != PLAYER_SKILL_CATEGORY.SUMMON) {
            if (p_skillData.category == PLAYER_SKILL_CATEGORY.SCHEME) {
                rawString = "Scheme";
            }
            rawString += " charge replenished by 1";
            PopUpTextNotification.ShowPlayerPoppingTextNotif($"{UtilityScripts.Utilities.YellowDotIcon()}{UtilityScripts.Utilities.ColorizeName(p_skillData.name)} charge replenished by 1", popUpDisplayPoint, rawString.Length);
        }
    }

    public void AdjustUIDisplayBaseOnGameMode() {
        if (WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
            spiritEnergyContainer.transform.position = new Vector3(0, 10000f, 0f);
            Vector3 pos = manaContainer.transform.localPosition;
            pos.x += 140f;
            manaContainer.transform.localPosition = pos;
            pos = plaguePointsContainer.transform.localPosition;
            pos.x += 140f;
            plaguePointsContainer.transform.localPosition = pos;
        }
        UpdatePlaguePointsAmount(EditableValuesManager.Instance.GetInitialChaoticEnergyBaseOnGameMode());
        manaLbl.text = EditableValuesManager.Instance.startingMana.ToString();
    }

    public void InitializeAfterGameLoaded() {
        Messenger.AddListener(PlayerSignals.THREAT_UPDATED, OnThreatUpdated);
        Messenger.AddListener<int>(PlayerSignals.THREAT_INCREASED, OnThreatIncreased);
        Messenger.AddListener(PlayerSignals.THREAT_RESET, OnThreatReset);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_SEIZE_POI, OnSeizePOI);
        Messenger.AddListener<IPointOfInterest>(CharacterSignals.ON_UNSEIZE_POI, OnUnseizePOI);
        Messenger.AddListener<int>(PlayerSignals.UPDATED_PLAGUE_POINTS, UpdatePlaguePointsAmount);
        
        //currencies
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnManaAdjusted);
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnSpiritEnergyAdjusted);
        Messenger.AddListener<int, int>(PlayerSignals.PLAGUE_POINTS_ADJUSTED, OnPlaguePointsAdjusted);
        //Messenger.AddListener(PlayerSignals.CHAOS_ORB_COLLECTED, OnSpiritEnergyAdjustedByOne);

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
        summonList.UpdateMonsterUnderlingQuantityList();
    }
    public void InitializeAfterLoadOutPicked() {
        UpdateIntel();
        //CreateInitialSpells();
        _buildListUI.Initialize();
        cultistsList.Initialize();
        targetsList.Initialize();
        
        cultistsList.UpdateList();
        minionList.UpdateList();
        summonList.UpdateList();

        OnThreatUpdated();
        UpdatePlaguePointsAmount(PlayerManager.Instance.player.plagueComponent.plaguePoints);
    }

    #region Listeners
    //private void OnInnerMapOpened(Region location) {
    //    UpdateRegionNameState();
    //}
    //private void OnInnerMapClosed(Region location) {
    //    UpdateRegionNameState();
    //}
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

    //    private void UpdateRegionNameState() {
    //        if (InnerMapManager.Instance.isAnInnerMapShowing) {
    //            Region location = InnerMapManager.Instance.currentlyShowingMap.region;
    //            Assert.IsNotNull(location, $"Trying to update region name UI in top menu, but no region is specified.");
    //            regionNameTopMenuText.text = location.name;
    //            regionNameTopMenuGO.SetActive(true);
    //#if UNITY_EDITOR || DEVELOPMENT_BUILD
    //            regionNameHoverHandler.SetOnHoverOverAction(() => TestingUtilities.ShowLocationInfo(location));
    //            regionNameHoverHandler.SetOnHoverOutAction(TestingUtilities.HideLocationInfo);
    //#endif
    //        } else {
    //            regionNameTopMenuGO.SetActive(false);
    //        }
    //    }

    #region Spiri tEnergy
    private void OnSpiritEnergyAdjusted(int adjustedAmount, int spiritEnergy) {
        if (adjustedAmount != 0) {
            UpdateSpiritEnergy();
            ShowSpiritEnergyAdjustEffect(adjustedAmount);
            DoSpiritEnergyPunchEffect();
            // AudioManager.Instance.PlayParticleMagnet();
        }
    }
    private void UpdateSpiritEnergy() {
        spiritEnergyLabel.text = PlayerManager.Instance.player.spiritEnergy.ToString();
    }
    private Tweener _currentSpiritEnergyPunchTween;
    private void DoSpiritEnergyPunchEffect() {
        if (_currentSpiritEnergyPunchTween == null) {
            _currentSpiritEnergyPunchTween = spiritEnergyContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(() => _currentSpiritEnergyPunchTween = null);
        }
    }
    private void ShowSpiritEnergyAdjustEffect(int adjustmentAmount) {
        var text = adjustmentAmount > 0 ? $"<color=\"green\">+{adjustmentAmount.ToString()}</color>" : $"<color=\"red\">{adjustmentAmount.ToString()}</color>";
        GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", spiritEnergyLabel.transform.position,
            Quaternion.identity, transform, true);
        effectGO.GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(Random.Range(-25, 25), -70f));
    }
    #endregion

    #region Mana
    private void OnManaAdjusted(int adjustedAmount, int mana) {
        if (adjustedAmount != 0) {
            UpdateMana();
            ShowManaAdjustEffect(adjustedAmount);
            DoManaPunchEffect();
            // AudioManager.Instance.PlayParticleMagnet();    
        }
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
    public void OnHoverSpiritEnergy() {
        string header = $"{UtilityScripts.Utilities.SpiritEnergyIcon()}Spirit Energy";
        if (PlayerManager.Instance.player != null) {
            header = $"{header}";
            //header += " (+" + (EditableValuesManager.Instance.GetManaRegenPerHour() + (PlayerManager.Instance.player.manaRegenComponent.GetManaPitCount() * (EditableValuesManager.Instance.GetManaRegenPerManaPit())) + "/hour)");
        }
        UIManager.Instance.ShowSmallInfo("Spirit Energy is used to upgrade your Portal to learn new Powers. You also gain Spirit Energy from Chaos Orbs.", pos: manaTooltipPos, header, autoReplaceText: false);
    }

    public void OnHoverOverMana() {
        string header = $"{UtilityScripts.Utilities.ManaIcon()}Mana";
        if (PlayerManager.Instance.player != null) {
            header = $"{header} - {PlayerManager.Instance.player.mana.ToString()}/{EditableValuesManager.Instance.maximumMana.ToString()} (+{(EditableValuesManager.Instance.GetManaRegenPerHour() + (PlayerManager.Instance.player.manaRegenComponent.GetManaPitCount() * (EditableValuesManager.Instance.GetManaRegenPerManaPit())))}/hour)";
            //header += " (+" + (EditableValuesManager.Instance.GetManaRegenPerHour() + (PlayerManager.Instance.player.manaRegenComponent.GetManaPitCount() * (EditableValuesManager.Instance.GetManaRegenPerManaPit())) + "/hour)");
        }
        UIManager.Instance.ShowSmallInfo("Mana is spent whenever you use any of your Powers, summon Minions or build Demonic Structures. It is easy to deplete but also quickly replenishes every hour. Build more Mana Pits to expand maximum capacity and increase hourly replenish.", pos: manaTooltipPos, header, autoReplaceText: false);
    }

    public void OnHoverOutMana() {
        UIManager.Instance.HideSmallInfo();
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
        string text = intel.GetFullIntelTooltip();
        UIManager.Instance.ShowSmallInfo(text, autoReplaceText: false);
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
    }
    public void ShowPlayerIntels(bool state) {
        intelContainer.SetActive(state);
        if (state) {
            Messenger.Broadcast(UISignals.INTEL_MENU_OPENED);    
        }
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
    public void WinGameOver(string winMessage) {
        if (PlayerManager.Instance.player.hasAlreadyWon) {
            return;
        }
        PlayerManager.Instance.player.hasAlreadyWon = true;
        SaveManager.Instance.currentSaveDataPlayer.OnWorldCompleted(WorldSettings.Instance.worldSettingsData.worldType);
        UIManager.Instance.ShowEndDemoScreen(winMessage);
        // if (WorldConfigManager.Instance.isTutorialWorld) {
        //     UIManager.Instance.ShowEndDemoScreen("You managed to wipe out all Villagers. Congratulations!");
        // } else {
            // UIManager.Instance.Pause();
            // winGameOver.Open();    
        // }
        
    }
    public void LoseGameOver(string p_gameOverMessage = "The Portal is in ruins! \nYour invasion has ended prematurely.") {
        UIManager.Instance.ShowEndDemoScreen(p_gameOverMessage);
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
        SkillData spellData = PlayerSkillManager.Instance.GetSpellData(spell);
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
            _spellItems.Remove(item);
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
    public void OnHoverSpell(SkillData skillData, UIHoverPosition position = null) {
        skillDetailsTooltip.ShowPlayerSkillDetails(skillData, position);
    }
    public string OnHoverSpellChargeRemaining(SkillData skillData, MonsterAndDemonUnderlingCharges p_monsterUnderling) {
        string text = string.Empty; 
        if (skillData.isInCooldown) {
            string timeDate = GameManager.Instance.Today().AddTicks(skillData.cooldown - skillData.currentCooldownTick).ToString();
            text = $"New charge of {UtilityScripts.Utilities.ColorizeName(skillData.name)} at {UtilityScripts.Utilities.ColorizeName(timeDate)}";
        }
        return text;
    }

    public string OnHoverSpellChargeRemainingForSummon(CharacterClassData cData, MonsterAndDemonUnderlingCharges p_monsterUnderling) {
        string text = string.Empty;
        if (p_monsterUnderling.isReplenishing) {
            string timeDate = GameManager.Instance.Today().AddTicks(p_monsterUnderling.cooldown - p_monsterUnderling.currentCooldownTick).ToString();

            text = $"New charge of {UtilityScripts.Utilities.ColorizeName(cData.displayName)} at {UtilityScripts.Utilities.ColorizeName(timeDate)}";

            //Tooltip.Instance.ShowSmallInfo($"New charge of {UtilityScripts.Utilities.ColorizeName(cData.displayName)} at {UtilityScripts.Utilities.ColorizeName(timeDate)}", autoReplaceText: false);

        }
        return text;
    }

    public void OnHoverOutSpell(SkillData skillData) {
        Tooltip.Instance.HideSmallInfo();
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
            TILE_OBJECT_TYPE.TREASURE_CHEST, TILE_OBJECT_TYPE.ICE, TILE_OBJECT_TYPE.HERB_PLANT,
            TILE_OBJECT_TYPE.ANIMAL_MEAT, TILE_OBJECT_TYPE.PROFESSION_PEDESTAL, TILE_OBJECT_TYPE.TOOL,
            TILE_OBJECT_TYPE.EXCALIBUR, TILE_OBJECT_TYPE.PHYLACTERY, TILE_OBJECT_TYPE.BIG_TREE_OBJECT,
            TILE_OBJECT_TYPE.COPPER_SWORD, TILE_OBJECT_TYPE.IRON_SWORD, TILE_OBJECT_TYPE.ORICHALCUM_SWORD,
            TILE_OBJECT_TYPE.MITHRIL_SWORD, TILE_OBJECT_TYPE.MINK_SHIRT, TILE_OBJECT_TYPE.SPIDER_SILK_SHIRT, TILE_OBJECT_TYPE.SCROLL,
            TILE_OBJECT_TYPE.RING, TILE_OBJECT_TYPE.BELT, TILE_OBJECT_TYPE.BRACER, TILE_OBJECT_TYPE.COPPER_ARMOR, TILE_OBJECT_TYPE.IRON_ARMOR,
            TILE_OBJECT_TYPE.POWER_CRYSTAL, TILE_OBJECT_TYPE.BASIC_SWORD, TILE_OBJECT_TYPE.BASIC_AXE, TILE_OBJECT_TYPE.BASIC_BOW,
            TILE_OBJECT_TYPE.BASIC_DAGGER, TILE_OBJECT_TYPE.BASIC_STAFF, TILE_OBJECT_TYPE.BASIC_SHIRT,
            TILE_OBJECT_TYPE.CORN, TILE_OBJECT_TYPE.POTATO, TILE_OBJECT_TYPE.PINEAPPLE,
            TILE_OBJECT_TYPE.ICEBERRY, TILE_OBJECT_TYPE.HYPNO_HERB, TILE_OBJECT_TYPE.COPPER, TILE_OBJECT_TYPE.IRON,
            TILE_OBJECT_TYPE.MITHRIL, TILE_OBJECT_TYPE.ORICHALCUM, TILE_OBJECT_TYPE.RABBIT_CLOTH, TILE_OBJECT_TYPE.MINK_CLOTH,
            TILE_OBJECT_TYPE.WOOL, TILE_OBJECT_TYPE.MOON_THREAD, TILE_OBJECT_TYPE.BOAR_HIDE, TILE_OBJECT_TYPE.WOLF_HIDE, TILE_OBJECT_TYPE.BEAR_HIDE,
            TILE_OBJECT_TYPE.SCALE_HIDE, TILE_OBJECT_TYPE.DRAGON_HIDE, TILE_OBJECT_TYPE.COPPER, TILE_OBJECT_TYPE.STONE_PILE, TILE_OBJECT_TYPE.MITHRIL,
            TILE_OBJECT_TYPE.MINK_CLOTH, TILE_OBJECT_TYPE.WOOD_PILE, TILE_OBJECT_TYPE.ANIMAL_MEAT, TILE_OBJECT_TYPE.ELF_MEAT, TILE_OBJECT_TYPE.HUMAN_MEAT,
            TILE_OBJECT_TYPE.STONE_PILE, TILE_OBJECT_TYPE.WOOD_PILE, TILE_OBJECT_TYPE.FISH_PILE, TILE_OBJECT_TYPE.TABLE
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
    public void EnableTopMenuButtons() {
        for (int i = 0; i < topMenuButtons.Length; i++) {
            topMenuButtons[i].interactable = true;
        }
    }
    public void DisableTopMenuButtons() {
        for (int i = 0; i < topMenuButtons.Length; i++) {
            topMenuButtons[i].interactable = false;
        }
    }
    public void CloseAllTopMenus() {
        for (int i = 0; i < topMenuButtons.Length; i++) {
            topMenuButtons[i].isOn = false;
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
    private void OnPlaguePointsAdjusted(int p_adjustedAmount, int p_totalAmount) {
        if (p_adjustedAmount != 0) {
            UpdatePlaguePointsAmount(p_totalAmount);
            ShowPlaguePointsGainedEffect(p_adjustedAmount);
            DoPlaguePointPunchEffect();
            // AudioManager.Instance.PlayParticleMagnet();    
        }
    }
    private void DoPlaguePointPunchEffect() {
        if (_currentPlaguePointPunchTween == null) {
            _currentPlaguePointPunchTween = plaguePointsContainer.DOPunchScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).OnComplete(() => _currentPlaguePointPunchTween = null);    
        }
    }
    private void ShowPlaguePointsGainedEffect(int adjustmentAmount) {
        if (plaguePointsContainer.gameObject.activeSelf) {
            var text = adjustmentAmount > 0 ? $"<color=\"green\">+{adjustmentAmount.ToString()}</color>" : $"<color=\"red\">{adjustmentAmount.ToString()}</color>";
            GameObject effectGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("AdjustmentEffectLbl", plaguePointLbl.transform.position, Quaternion.identity, transform, true);
            effectGO.GetComponent<AdjustmentEffectLabel>().PlayEffect(text, new Vector2(Random.Range(-25, 25), -70f));
        }
    }
    private void UpdatePlaguePointsAmount(int p_amount) {
        plaguePointLbl.text = p_amount.ToString();
    }
    public void OnHoverEnterPlaguePoints() {
        string header = $"{UtilityScripts.Utilities.ChaoticEnergyIcon()}Chaotic Energy";
        if (PlayerManager.Instance.player != null) {
            header = $"{header} - {PlayerManager.Instance.player.plagueComponent.plaguePoints.ToString()}/{PlayerManager.Instance.player.plagueComponent.maxPlaguePoints.ToString()}";
        }
        string text = "Chaotic Energy is used for long term improvements. Use it to upgrade your Portal and unlock more Powers, or upgrade your other Demonic Structures. Gain more Chaotic Energy from Chaos Orbs that are produced through different interactions with the world.";
        UIManager.Instance.ShowSmallInfo(text, threatHoverPos, header, autoReplaceText: false);
    }
    public void OnHoverExitPlaguePoints() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Targets
    public void OnToggleTargetsTab(bool p_state) {
        if (p_state) {
            targetsList.Open();
        } else {
            targetsList.Close();
        }
    }
    private Tweener _currentTargetPunchEffect;
    public void DoTargetTabPunchEffect() {
        if (_currentTargetPunchEffect == null) {
            _currentTargetPunchEffect = targetsToggle.transform.DOPunchScale(new Vector3(2f, 2f, 1f), 0.2f).OnComplete(() => _currentTargetPunchEffect = null);
        }
    }
    #endregion

    #region Accumulated Damage
    public void UpdateAccumulatedDamageText(int amount) {
        accumulatedDamageLbl.text = amount.ToString();
    }
    #endregion

    #region Tutorial
    [FormerlySerializedAs("_tutorialUIController")] [Header("Tutorial")]
    public TutorialUIController tutorialUIController;
    [SerializeField] private Toggle _tutorialToggle;
    public void OnToggleTutorialTab(bool p_isOn) {
        if (p_isOn) {
            tutorialUIController.ShowUI();
        } else {
            tutorialUIController.HideUI();
        }
    }
    public void ShowSpecificTutorial(TutorialManager.Tutorial_Type p_type) {
        tutorialUIController.ShowUI();
        tutorialUIController.JumpToSpecificTutorial(p_type);
    }
    public void OnCloseTutorialUI() {
        _tutorialToggle.SetIsOnWithoutNotify(false);
    }
    #endregion
}
