using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Ruinarch.Custom_UI;
using UtilityScripts;

public class StructureInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Tabs")]
    [SerializeField] private RuinarchToggle prisonersTab;
    [SerializeField] private RuinarchToggle residentsTab;
    
    [Space(10)]
    [Header("Content")]
    [SerializeField] private GameObject goPrisoners;
    [SerializeField] private GameObject goResidents;
    
    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private LocationPortrait locationPortrait;

    [Space(10)]
    [Header("Info")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI villageLbl;
    [SerializeField] private TextMeshProUGUI descriptionLbl;
    [SerializeField] private EventLabel villageEventLbl;
    [SerializeField] private GameObject villageParentGO;

    [Header("City Center Info")]
    [SerializeField] private Image migrationMeterImg;
    [SerializeField] private GameObject migrationMeterGO;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private ScrollRect prisonersScrollView;

    [Space(10)]
    [Header("Eyes")]
    [SerializeField] private RuinarchToggle eyesTab;
    [SerializeField] private GameObject tileObjectNameplatePrefab;
    [SerializeField] private Transform eyesParentTransform;

    [Space(10)]
    [Header("Store Target")] 
    [SerializeField] private StoreTargetButton btnStoreTarget;

    public LocationStructure activeStructure { get; private set; }

    #region Overrides
    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Character, LocationStructure>(StructureSignals.ADDED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
        Messenger.AddListener<Character, LocationStructure>(StructureSignals.REMOVED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, UpdatePrisonersFromSignal);
        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_LEFT_STRUCTURE, UpdatePrisonersFromSignal);
        Messenger.AddListener<Beholder>(StructureSignals.UPDATE_EYE_WARDS, UpdateEyeWardsFromSignal);
        Messenger.AddListener<DemonicStructure>(StructureSignals.DEMONIC_STRUCTURE_REPAIRED, OnDemonicStructureRepaired);
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
        ListenToPlayerActionSignals();

        villageEventLbl.SetOnLeftClickAction(OnLeftClickVillage);
        villageEventLbl.SetOnRightClickAction(OnRightClickVillage);
    }
    public override void CloseMenu() {
        base.CloseMenu();
        if(activeStructure != null) {
            Selector.Instance.Deselect();
            GameObject structureObject = null;
            if (activeStructure is ManMadeStructure manMadeStructure) {
                structureObject = manMadeStructure.structureObj.gameObject;
            } else if (activeStructure is DemonicStructure demonicStructure) {
                structureObject = demonicStructure.structureObj.gameObject;
            }
            if (structureObject != null && InnerMapCameraMove.Instance.target == structureObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeStructure = null;
    }
    public override void OpenMenu() {
        activeStructure = _data as LocationStructure;
        base.OpenMenu();
        activeStructure.ShowSelectorOnStructure();
        btnStoreTarget.SetTarget(activeStructure);
        UpdateStructureInfoUI();
        UpdateContentToShow();
        if (UsesResidentsTab()) {
            UpdateResidents();    
        } else if (UsesPrisonersTab()) {
            UpdatePrisoners();
        } else if (UsesEyesTab()) {
            UpdateEyes();
        }
        LoadActions(activeStructure);
    }
    protected override void LoadActions(IPlayerActionTarget target) {
        UtilityScripts.Utilities.DestroyChildren(actionsTransform);
        activeActionItems.Clear();
        for (int i = 0; i < target.actions.Count; i++) {
            PlayerAction action = PlayerSkillManager.Instance.GetPlayerActionData(target.actions[i]);
            if (action.type == PLAYER_SKILL_TYPE.SCHEME) { continue; }
            if (action.IsValid(target) && PlayerManager.Instance.player.playerSkillComponent.CanDoPlayerAction(action.type)) {
                ActionItem actionItem = AddNewAction(action, target);
                actionItem.SetInteractable(action.CanPerformAbilityTo(target) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);    
                actionItem.ForceUpdateCooldown();
            }
        }
    }
    #endregion

    private bool UsesResidentsTab() {
        if (activeStructure is DemonicStructure demonicStructure) {
            if (demonicStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS || demonicStructure.structureType == STRUCTURE_TYPE.KENNEL) {
                return false;
            } else {
                return false;
            }
        } else {
            return true;
        }
    }
    private bool UsesPrisonersTab() {
        if (activeStructure is DemonicStructure demonicStructure) {
            if (demonicStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS || demonicStructure.structureType == STRUCTURE_TYPE.KENNEL) {
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }
    private bool UsesEyesTab() {
        if (activeStructure is Beholder) {
            return true;
        } else {
            return false;
        }
    }
    private void UpdateContentToShow() {
        if (UsesResidentsTab()) {
            prisonersTab.isOn = false;
            eyesTab.isOn = false;
        } else if (UsesPrisonersTab()) {
            residentsTab.isOn = false;
            eyesTab.isOn = false;
        } else if (UsesEyesTab()) {
            residentsTab.isOn = false;
            prisonersTab.isOn = false;
        } else {
            prisonersTab.isOn = false;
            residentsTab.isOn = false;
            eyesTab.isOn = false;
        }
    }
    
    private void UpdateTabs() {
        residentsTab.gameObject.SetActive(false);
        prisonersTab.gameObject.SetActive(false);
        eyesTab.gameObject.SetActive(false);
        if (UsesResidentsTab()) { residentsTab.gameObject.SetActive(true); }
        else if (UsesPrisonersTab()) { prisonersTab.gameObject.SetActive(true); }
        else if (UsesEyesTab()) { eyesTab.gameObject.SetActive(true); }
    }
    public void UpdateStructureInfoUI() {
        if(activeStructure == null) { return; }
        UpdateTabs();
        UpdateBasicInfo();
        UpdateInfo();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = $"{activeStructure.nameplateName}";
        if (activeStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            locationPortrait.SetLocation(activeStructure.settlementLocation);
        } else {
            locationPortrait.ClearLocations();
        }
        locationPortrait.SetPortrait(activeStructure.structureType);
    }
    private void UpdateInfo() {
        hpLbl.text = $"{activeStructure.currentHP}/{activeStructure.maxHP}";
        if (WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
            descriptionLbl.text = activeStructure.scenarioDescription;
        } else {
            descriptionLbl.text = activeStructure.customDescription;
        }
        
        if(activeStructure.settlementLocation != null && activeStructure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE) {
            villageLbl.text = $"<link=\"village\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeStructure.settlementLocation.name)}</link>";
            villageParentGO.SetActive(true);
        } else {
            villageParentGO.SetActive(false);
        }
        UpdateInfoIfCityCenter();
    }
    private void UpdateInfoIfCityCenter() {
        if (activeStructure.structureType == STRUCTURE_TYPE.CITY_CENTER && activeStructure.settlementLocation != null) {
            if(activeStructure.settlementLocation is NPCSettlement npcSettlement) {
                migrationMeterImg.fillAmount = npcSettlement.migrationComponent.GetNormalizedMigrationMeterValue();
                migrationMeterGO.SetActive(true);
            } else {
                migrationMeterGO.SetActive(false);
            }
        } else {
            migrationMeterGO.SetActive(false);
        }
    }
    private void UpdateResidents() {
        UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
        List<Character> residents = activeStructure.residents;
        if (residents != null && residents.Count > 0) {
            for (int i = 0; i < residents.Count; i++) {
                Character character = residents[i];
                if (character != null) {
                    GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
                    CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
                    item.SetObject(character);
                    item.SetAsDefaultBehaviour();
                }
            }
        }
    }
    private void UpdatePrisoners() {
        UtilityScripts.Utilities.DestroyChildren(prisonersScrollView.content);
        List<Character> characters = RuinarchListPool<Character>.Claim();
        if (activeStructure is Kennel kennel) {
            if (kennel.occupyingSummon != null) {
                characters.Add(kennel.occupyingSummon);
            }
        } else if (activeStructure is TortureChambers tortureChambers&& tortureChambers.rooms != null && tortureChambers.rooms.Length > 0 && tortureChambers.rooms[0] is PrisonCell prisonCell) {
            characters.AddRange(prisonCell.charactersInRoom);
        } else {
            characters.AddRange(activeStructure.charactersHere);
        }
        if (characters != null && characters.Count > 0) {
            for (int i = 0; i < characters.Count; i++) {
                Character character = characters[i];
                if (character != null) {
                    GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, prisonersScrollView.content);
                    CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
                    item.SetObject(character);
                    item.SetAsDefaultBehaviour();
                }
            }
        }
    }
    private void UpdateEyes() {
        UtilityScripts.Utilities.DestroyChildren(eyesParentTransform);
        Beholder beholder = activeStructure as Beholder;
        for (int i = 0; i < beholder.eyeWards.Count; i++) {
            EyeWard eyeWard = beholder.eyeWards[i];
            GameObject go = UIManager.Instance.InstantiateUIObject(tileObjectNameplatePrefab.name, eyesParentTransform);
            TileObjectNameplateItem item = go.GetComponent<TileObjectNameplateItem>();
            item.SetObject(eyeWard);
            item.SetAsButton();
            item.AddOnClickAction(OnClickEye);
        }
    }

    private void OnReceiveKeyCodeSignal(KeyCode p_key) {
        if (p_key == KeyCode.Mouse1) {
            CloseMenu();
        }
    }

    #region Listeners
    private void UpdateResidentsFromSignal(Character resident, LocationStructure structure) {
        if (isShowing && activeStructure == structure && UsesResidentsTab()) {
            UpdateResidents();
        }
    }
    private void UpdatePrisonersFromSignal(Character character, LocationStructure structure) {
        if (isShowing && activeStructure == structure && UsesPrisonersTab()) {
            UpdatePrisoners();
        }
    }
    private void UpdateEyeWardsFromSignal(Beholder structure) {
        if (isShowing && activeStructure == structure && UsesEyesTab()) {
            UpdateEyes();
        }
    }
    private void OnDemonicStructureRepaired(DemonicStructure p_demonicStructure) {
        if (isShowing && activeStructure == p_demonicStructure) {
            UpdateInfo();
        }
    }
    #endregion

    #region Village
    private void OnLeftClickVillage(object obj) {
        if (activeStructure.settlementLocation != null && activeStructure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE) {
            UIManager.Instance.ShowSettlementInfo(activeStructure.settlementLocation);
        }
    }
    private void OnRightClickVillage(object obj) {
        if (activeStructure.settlementLocation != null && activeStructure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE) {
            UIManager.Instance.ShowPlayerActionContextMenu(activeStructure.settlementLocation, Input.mousePosition, true);
        }
    }
    #endregion

    #region Click
    public void OnClickItem() {
        activeStructure.CenterOnStructure();
    }
    public void OnClickEye(TileObject obj) {
        if (obj == null) {
            return;
        }
        Selector.Instance.Select(obj);
        if (obj.worldObject == null && obj.isBeingCarriedBy != null) {
            InnerMapCameraMove.Instance.CenterCameraOn(obj.isBeingCarriedBy.worldObject.gameObject);
        } else if (obj.worldObject != null && obj.isBeingCarriedBy == null) {
            InnerMapCameraMove.Instance.CenterCameraOn(obj.worldObject.gameObject);
        } else if (obj.worldObject != null && obj.isBeingCarriedBy != null) {
            InnerMapCameraMove.Instance.CenterCameraOn(obj.worldObject.gameObject);
        } else {
            return;
        }
    }
    #endregion

    #region Hover
    public void OnHoverEnterMigrationMeter() {
        if (activeStructure.settlementLocation != null && activeStructure.settlementLocation is NPCSettlement npcSettlement) {
            string text = npcSettlement.migrationComponent.GetHoverTextOfMigrationMeter();
            if (!string.IsNullOrEmpty(text)) {
                UIManager.Instance.ShowSmallInfo(text);
            }
        }
    }
    public void OnHoverExitMigrationMeter() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region For Testing
    public void ShowStructureTestingInfo() {
#if UNITY_EDITOR
        if (activeStructure is CityCenter && activeStructure.settlementLocation is NPCSettlement npcSettlement) {
            TestingUtilities.ShowLocationInfo(activeStructure.region, npcSettlement);    
        } else {
            UIManager.Instance.ShowSmallInfo(activeStructure.GetTestingInfo());    
        }
#endif
    }
    public void HideStructureTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
