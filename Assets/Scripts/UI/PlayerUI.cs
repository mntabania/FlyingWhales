﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Linq;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance;

    public TokensUI charactersIntelUI;

    [Header("Currency")]
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI suppliesText;
    public TextMeshProUGUI impsText;

    [Header("Role Slots")]
    [SerializeField] private RectTransform roleSlotsParent;
    [SerializeField] private RoleSlotItem[] roleSlots;
    [SerializeField] private GameObject roleSlotItemPrefab;
    [SerializeField] private GameObject actionBtnPrefab;
    [SerializeField] private GameObject actionBtnTooltipGO;
    [SerializeField] private TextMeshProUGUI actionBtnTooltipLbl;
    [SerializeField] private GameObject actionBtnPointer;
    [SerializeField] private TextMeshProUGUI activeMinionTypeLbl;
    [SerializeField] private RectTransform activeMinionActionsParent;
    [SerializeField] private HorizontalScrollSnap roleSlotsScrollSnap;

    [Header("Attack")]
    public GameObject attackGridGO;
    [SerializeField] private Sprite attackGridIconSprite;
    [SerializeField] private Sprite defenseGridIconSprite;
    public AttackSlotItem attackSlot;
    [SerializeField] private DefenseSlotItem defenseSlot;
    public SlotItem[] attackGridSlots;

    [Header("Bottom Menu")]
    public Toggle goalsToggle;
    //public Toggle intelToggle;
    public Toggle inventoryToggle;
    public Toggle factionToggle;
    public ToggleGroup minionSortingToggleGroup;

    [Header("Intel")]
    [SerializeField] private GameObject intelContainer;
    [SerializeField] private IntelItem[] intelItems;
    [SerializeField] private Toggle intelToggle;

    [Header("Provoke")]
    [SerializeField] private ProvokeMenu provokeMenu;

    [Header("Miscellaneous")]
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closePosition;
    [SerializeField] private Vector3 halfPosition;
    [SerializeField] private EasyTween tweener;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private Image combatGridAssignerIcon;

    private MINIONS_SORT_TYPE _minionSortType;
    private bool _isScrollingUp;
    private bool _isScrollingDown;
    public CombatGrid attackGridReference { get; private set; }
    public CombatGrid defenseGridReference { get; private set; }

    #region getters/setters
    public MINIONS_SORT_TYPE minionSortType {
        get { return _minionSortType; }
    }
    #endregion

    void Awake() {
        Instance = this;
        //minionItems = new List<PlayerCharacterItem>();
        //Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        //Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
    }
    //void Start() {
    //    Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
    //}
    public void UpdateUI() {
        if (PlayerManager.Instance.player == null) {
            return;
        }
        manaText.text = PlayerManager.Instance.player.currencies[CURRENCY.MANA].ToString();
        //redMagicText.text = "" + PlayerManager.Instance.player.redMagic;
        //greenMagicText.text = "" + PlayerManager.Instance.player.greenMagic;
        suppliesText.text = PlayerManager.Instance.player.currencies[CURRENCY.SUPPLY].ToString();
        impsText.text = "Imps: " + PlayerManager.Instance.player.currencies[CURRENCY.IMP].ToString() + "/" + PlayerManager.Instance.player.maxImps.ToString();
        //threatFiller.fillAmount = (float) PlayerManager.Instance.player.threatLevel / 100f;
    }

    public void Initialize() {
        //attack/raid
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetNeededType(typeof(Character));
            //currSlot.SetOtherValidation(IsObjectValidForAttack);
            currSlot.SetSlotIndex(i);
            //currSlot.SetItemDroppedCallback(OnDropOnAttackGrid);
            //currSlot.SetItemDroppedOutCallback(OnDroppedOutFromAttackGrid);
        }

        LoadRoleSlots();
        LoadAttackSlot();

        UpdateIntel();

        Messenger.AddListener<UIMenu>(Signals.MENU_OPENED, OnMenuOpened);
        Messenger.AddListener<UIMenu>(Signals.MENU_CLOSED, OnMenuClosed);
        Messenger.AddListener(Signals.UPDATED_CURRENCIES, UpdateUI);
        Messenger.AddListener<Intel>(Signals.PLAYER_OBTAINED_INTEL, OnIntelObtained);
        Messenger.AddListener<Intel>(Signals.PLAYER_REMOVED_INTEL, OnIntelRemoved);

        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntelMenu);
        Messenger.AddListener(Signals.ON_CLOSE_SHARE_INTEL, OnCloseShareIntelMenu);

        //job action buttons
        Messenger.AddListener(Signals.HAS_SEEN_ACTION_BUTTONS, OnSeenActionButtons);
    }

    #region Role Slots
    private void OnSeenActionButtons() {
        actionBtnPointer.SetActive(!PlayerManager.Instance.player.hasSeenActionButtonsOnce);
    }
    private void LoadRoleSlots() {
        int currIndex = 0;
        roleSlots = new RoleSlotItem[PlayerManager.Instance.player.roleSlots.Count];
        foreach (KeyValuePair<JOB, PlayerJobData> keyValuePair in PlayerManager.Instance.player.roleSlots) {
            GameObject roleSlotGO = UIManager.Instance.InstantiateUIObject(roleSlotItemPrefab.name, roleSlotsParent);
            RoleSlotItem roleSlot = roleSlotGO.GetComponent<RoleSlotItem>();
            roleSlot.SetSlotJob(keyValuePair.Key);
            roleSlots[currIndex] = roleSlot;
            roleSlotsScrollSnap.AddChild(roleSlotGO);
            currIndex++;
        }
        OnChangeRoleSlotPage(roleSlotsScrollSnap.CurrentPage);
    }
    //private void ShowActionButtonsFor(IPointOfInterest poi) {
    //    if (UIManager.Instance.IsShareIntelMenuOpen()) {
    //        return;
    //    }
    //    Utilities.DestroyChildren(jobActionsParent);
    //    for (int i = 0; i < roleSlots.Length; i++) {
    //        RoleSlotItem item = roleSlots[i];
    //        if (PlayerManager.Instance.player.roleSlots[item.slotJob].assignedCharacter != null) {
    //            item.ShowActionButtons(poi, jobActionsParent);
    //        }
    //    }
    //    jobActionsParent.gameObject.SetActive(true);
    //    actionBtnPointer.SetActive(!PlayerManager.Instance.player.hasSeenActionButtonsOnce);
    //}
    private void HideActionButtons() {
        actionBtnPointer.SetActive(false);
    }
    public void ShowActionBtnTooltip(string message, string header) {
        string m = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            m = "<font=\"Eczar-Medium\"><line-height=100%><size=18>" + header + "</font>\n";
        }
        m += "<line-height=70%><size=16>" + message;

        m = m.Replace("\\n", "\n");

        actionBtnTooltipLbl.text = m;
        actionBtnTooltipGO.gameObject.SetActive(true);
    }
    public void HideActionBtnTooltip() {
        actionBtnTooltipGO.gameObject.SetActive(false);
    }
    public void OnStartChangeRoleSlotPage() {
        Utilities.DestroyChildren(activeMinionActionsParent);
    }
    public void OnChangeRoleSlotPage(int page) {
        RoleSlotItem slot = roleSlots[page];
        activeMinionTypeLbl.text = Utilities.NormalizeString(slot.slotJob.ToString());
        LoadActionButtonsForActiveJob(slot);
    }
    private void LoadActionButtonsForActiveJob(RoleSlotItem active) {
        PlayerJobData jobData = PlayerManager.Instance.player.roleSlots[active.slotJob];
        for (int i = 0; i < jobData.jobActions.Count; i++) {
            PlayerJobAction jobAction = jobData.jobActions[i];
            GameObject jobGO = UIManager.Instance.InstantiateUIObject(actionBtnPrefab.name, activeMinionActionsParent);
            PlayerJobActionButton actionBtn = jobGO.GetComponent<PlayerJobActionButton>();
            actionBtn.SetJobAction(jobAction, jobData.assignedCharacter);
            actionBtn.SetClickAction(() => PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(jobAction));
        }
    }
    #endregion

    #region Attack UI
    private void LoadAttackSlot() {
        attackGridReference = new CombatGrid();
        defenseGridReference = new CombatGrid();
        attackGridReference.Initialize();
        defenseGridReference.Initialize();
        attackSlot.UpdateVisuals();
        defenseSlot.UpdateVisuals();
    }
    public void ShowAttackGrid() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetOtherValidation(IsObjectValidForAttack);
            currSlot.SetItemDroppedCallback(OnDropOnAttackGrid);
            currSlot.SetItemDroppedOutCallback(OnDroppedOutFromAttackGrid);
        }
        attackGridGO.SetActive(true);
        combatGridAssignerIcon.sprite = attackGridIconSprite;
        SetAttackGridCharactersFromPlayer();
    }
    public void ShowDefenseGrid() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            SlotItem currSlot = attackGridSlots[i];
            currSlot.SetOtherValidation(IsObjectValidForAttack);
            currSlot.SetItemDroppedCallback(OnDropOnDefenseGrid);
            currSlot.SetItemDroppedOutCallback(OnDroppedOutFromDefenseGrid);
        }
        attackGridGO.SetActive(true);
        combatGridAssignerIcon.sprite = defenseGridIconSprite;
        SetDefenseGridCharactersFromPlayer();
    }
    public void OnClickConfirmCombatGrid() {
        if(combatGridAssignerIcon.sprite == attackGridIconSprite) {
            attackSlot.OnClickConfirm();
        } else {
            defenseSlot.OnClickConfirm();
        }
    }
    public void HideCombatGrid() {
        attackGridGO.SetActive(false);
    }
    private void OnDropOnAttackGrid(object obj, int index) {
        if(obj is Character) {
            Character character = obj as Character;
            if (attackGridReference.IsCharacterInGrid(character)) {
                attackGridSlots[index].PlaceObject(attackGridReference.slots[index].character);
                return;
            }
            attackGridReference.AssignCharacterToGrid(character, index, true);
            UpdateAttackGridSlots();
        }
    }
    private void OnDropOnDefenseGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            if (defenseGridReference.IsCharacterInGrid(character)) {
                attackGridSlots[index].PlaceObject(defenseGridReference.slots[index].character);
                return;
            }
            defenseGridReference.AssignCharacterToGrid(character, index, true);
            UpdateDefenseGridSlots();
        }
    }
    private void OnDroppedOutFromAttackGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            attackGridReference.RemoveCharacterFromGrid(character);
            UpdateAttackGridSlots();
        }
    }
    private void OnDroppedOutFromDefenseGrid(object obj, int index) {
        if (obj is Character) {
            Character character = obj as Character;
            defenseGridReference.RemoveCharacterFromGrid(character);
            UpdateDefenseGridSlots();
        }
    }
    private bool IsObjectValidForAttack(object obj, SlotItem slotItem) {
        if (obj is Character) {
            Character character = obj as Character;
            if (character.characterClass.combatPosition == COMBAT_POSITION.FRONTLINE) {
                if (attackGridSlots[0] == slotItem || attackGridSlots[1] == slotItem) {
                    return true;
                }
            } else {
                if (attackGridSlots[2] == slotItem || attackGridSlots[3] == slotItem) {
                    return true;
                }
            }
        }
        return false;
    }
    private void SetAttackGridCharactersFromPlayer() {
        for (int i = 0; i < attackGridReference.slots.Length; i++) {
            attackGridReference.slots[i].OccupySlot(PlayerManager.Instance.player.attackGrid.slots[i].character);
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void SetDefenseGridCharactersFromPlayer() {
        for (int i = 0; i < defenseGridReference.slots.Length; i++) {
            defenseGridReference.slots[i].OccupySlot(PlayerManager.Instance.player.attackGrid.slots[i].character);
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void UpdateAttackGridSlots() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            attackGridSlots[i].PlaceObject(attackGridReference.slots[i].character);
        }
    }
    private void UpdateDefenseGridSlots() {
        for (int i = 0; i < attackGridSlots.Length; i++) {
            attackGridSlots[i].PlaceObject(defenseGridReference.slots[i].character);
        }
    }
    #endregion

    #region Miscellaneous
    public void SetBottomMenuTogglesState(bool isOn) {
        goalsToggle.isOn = isOn;
        intelToggle.isOn = isOn;
        inventoryToggle.isOn = isOn;
        factionToggle.isOn = isOn;
    }
    #endregion
    public void CreateNewParty() {
        if (!UIManager.Instance.partyinfoUI.isShowing) {
            UIManager.Instance.partyinfoUI.ShowCreatePartyUI();
        } else {
            UIManager.Instance.partyinfoUI.CloseMenu();
        }
    }

    public string previousMenu;
    private void OnMenuOpened(UIMenu menu) {
        if (menu is LandmarkInfoUI) {
            UIManager.Instance.ShowMinionsMenu();
        }
    }
    private void OnMenuClosed(UIMenu menu) {
        if (menu is LandmarkInfoUI) {
            if (string.IsNullOrEmpty(previousMenu)) {
                UIManager.Instance.HideRightMenus();
            } else if (previousMenu.Equals("minion")) {
                UIManager.Instance.ShowMinionsMenu();
            } else if (previousMenu.Equals("character")) {
                UIManager.Instance.ShowCharacterTokenMenu();
            } else if (previousMenu.Equals("location")) {
                UIManager.Instance.ShowLocationTokenMenu();
            } else if (previousMenu.Equals("faction")) {
                UIManager.Instance.ShowFactionTokenMenu();
            }
        } 
        //else if (menu is CharacterInfoUI || menu is TileObjectInfoUI) {
        //    HideActionButtons();
        //}
    }

    #region Intel
    private void OnIntelObtained(Intel intel) {
        UpdateIntel();
    }
    private void OnIntelRemoved(Intel intel) {
        UpdateIntel();
    }
    private void UpdateIntel() {
        for (int i = 0; i < intelItems.Length; i++) {
            IntelItem currItem = intelItems[i];
            Intel intel = PlayerManager.Instance.player.allIntel.ElementAtOrDefault(i);
            currItem.SetIntel(intel);
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
        RectTransform rt = UIManager.Instance.playerNotifGO.transform as RectTransform;
        Vector3 previousPos = rt.anchoredPosition;
        if (!state) {
            rt.anchoredPosition = new Vector3(-640f, previousPos.y, previousPos.z);
        } else {
            rt.anchoredPosition = new Vector3(-1150f, previousPos.y, previousPos.z);
        }
    }
    #endregion

    #region Provoke
    public void OpenProvoke(Character minion, Character target) {
        provokeMenu.Open(target, minion);
    }
    #endregion
}
