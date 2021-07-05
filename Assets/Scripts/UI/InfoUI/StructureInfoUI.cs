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
    [SerializeField] private RuinarchToggle workersTab;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private GameObject goPrisoners;
    [SerializeField] private GameObject goResidents;
    
    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI extraInfo1Title;
    [SerializeField] private TextMeshProUGUI extraInfo1Description;
    [SerializeField] private TextMeshProUGUI extraInfo2Title;
    [SerializeField] private TextMeshProUGUI extraInfo2Description;
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
    [SerializeField] private ScrollRect workersScrollView;


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
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
		Messenger.AddListener<Inner_Maps.Location_Structures.Watcher>(StructureSignals.UPDATE_EYE_WARDS, this.UpdateEyeWardsFromSignal);
        Messenger.AddListener<DemonicStructure>(StructureSignals.DEMONIC_STRUCTURE_REPAIRED, OnDemonicStructureRepaired);
        Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN_EMPTY_SPACE, OnReceiveKeyCodeSignal);
        Messenger.AddListener<string>(UISignals.HOTKEY_CLICK, OnReceivePortalShortCutSignal);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_HP_CHANGED, OnStructureHPChanged);
        Messenger.AddListener<Character, ManMadeStructure>(StructureSignals.ON_WORKER_HIRED, UpdateWorkersFromSignal);
        ListenToPlayerActionSignals();

        villageEventLbl.SetOnLeftClickAction(OnLeftClickVillage);
        villageEventLbl.SetOnRightClickAction(OnRightClickVillage);
    }

    private void OnStructureHPChanged(LocationStructure p_structure) {
        if (activeStructure == p_structure) {
            UpdateInfo();
        }
    }

    private void OnCharacterDied(Character p_character) {
        if (isShowing && p_character.currentStructure == activeStructure) {
            UpdatePrisonersFromSignal(p_character, p_character.currentStructure);
        }
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
        if (UsesWorkersTab()) {
            UpdateResidents();
            UpdateWorkers();
        } else if (UsesResidentsTab()) {
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
        if (activeStructure is Inner_Maps.Location_Structures.Watcher) {
            return true;
        } else {
            return false;
        }
    }
    private bool UsesWorkersTab() {
        if (activeStructure.structureType.IsJobStructure()) {
            return true;
        } else {
            return false;
        }
    }
    private void UpdateContentToShow() {
        if (UsesWorkersTab()) {
            prisonersTab.isOn = false;
            eyesTab.isOn = false;
        } else if (UsesResidentsTab()) {
            prisonersTab.isOn = false;
            eyesTab.isOn = false;
            workersTab.isOn = false;
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
        workersTab.gameObject.SetActive(false);
        if (UsesWorkersTab()) { workersTab.gameObject.SetActive(true); residentsTab.gameObject.SetActive(true); }
        else if (UsesResidentsTab()) { residentsTab.gameObject.SetActive(true); }
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
        if (!WorldSettings.Instance.worldSettingsData.IsScenarioMap()) {
            if (activeStructure.structureType == STRUCTURE_TYPE.THE_PORTAL) {
                ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
                nameLbl.text = $"{activeStructure.nameplateName} Lv.{portal.level}";
            } else {
                nameLbl.text = $"{activeStructure.nameplateName}";
            }
        } else {
            nameLbl.text = $"{activeStructure.nameplateName}";
        }
               
        if (activeStructure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            locationPortrait.SetLocation(activeStructure.settlementLocation);
        } else {
            locationPortrait.ClearLocations();
        }
        locationPortrait.SetPortrait(activeStructure.structureType);
        DisplayExtraInfos(activeStructure);
    }

    void DisplayExtraInfos(LocationStructure p_locationStructure) {
        extraInfo1Title.text = p_locationStructure.extraInfo1Header;
        extraInfo2Title.text = p_locationStructure.extraInfo2Header;
        extraInfo1Description.text = p_locationStructure.extraInfo1Description;
        extraInfo2Description.text = p_locationStructure.extraInfo2Description;
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
            prisonCell.PopulateValidOccupants(characters);
            //List<Character> validCharacters = prisonCell.charactersInRoom.Where(c => prisonCell.IsValidOccupant(c)).ToList();
            //if (validCharacters.Count > 0) {
            //    characters.AddRange(validCharacters);    
            //}
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
        RuinarchListPool<Character>.Release(characters);
    }

    private void UpdateWorkers() {
        UtilityScripts.Utilities.DestroyChildren(workersScrollView.content);
        ManMadeStructure manMadeStructure = activeStructure as ManMadeStructure;
        for (int i = 0; i < manMadeStructure.assignedWorkerIDs.Count; i++) {
            string assignedWorkerID = manMadeStructure.assignedWorkerIDs[i];
            Character assignedWorker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
            if (assignedWorker != null) {
                GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, workersScrollView.content);
                CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
                item.SetObject(assignedWorker);
                item.SetAsDefaultBehaviour();
            }
        }
    }
    private void UpdateEyes() {
        UtilityScripts.Utilities.DestroyChildren(eyesParentTransform);
		Inner_Maps.Location_Structures.Watcher beholder = activeStructure as Inner_Maps.Location_Structures.Watcher;
        for (int i = 0; i < beholder.eyeWards.Count; i++) {
            DemonEye eyeWard = beholder.eyeWards[i];
            GameObject go = UIManager.Instance.InstantiateUIObject(tileObjectNameplatePrefab.name, eyesParentTransform);
            TileObjectNameplateItem item = go.GetComponent<TileObjectNameplateItem>();
            item.SetObject(eyeWard);
            item.SetAsButton();
            item.AddOnClickAction(OnClickEye);
        }
    }

    private void OnReceivePortalShortCutSignal(string p_broadcastKey) {
        if (!GameManager.Instance.gameHasStarted) { return; }
        if (p_broadcastKey == "portal shortcut") {
            ThePortal portal = PlayerManager.Instance.player.playerSettlement.GetRandomStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
            if(portal != null) {
                SetData(portal);
                OpenMenu();
                activeStructure.CenterOnStructure();
            }    
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
    private void UpdateWorkersFromSignal(Character resident, LocationStructure structure) {
        if (isShowing && activeStructure == structure && UsesWorkersTab()) {
            UpdateWorkers();
        }
    }
    private void UpdatePrisonersFromSignal(Character character, LocationStructure structure) {
        if (isShowing && activeStructure == structure && UsesPrisonersTab()) {
            UpdatePrisoners();
        }
    }
    private void UpdateEyeWardsFromSignal(Inner_Maps.Location_Structures.Watcher structure) {
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
