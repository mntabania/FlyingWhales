using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class StructureInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private LocationPortrait locationPortrait;

    [Space(10)]
    [Header("Info")]
    [SerializeField] private TextMeshProUGUI hpLbl;
    [SerializeField] private TextMeshProUGUI villageLbl;
    [SerializeField] private EventLabel villageEventLbl;
    [SerializeField] private GameObject villageParentGO;

    [Header("City Center Info")]
    [SerializeField] private Image migrationMeterImg;
    [SerializeField] private GameObject migrationMeterGO;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect charactersScrollView;

    public LocationStructure activeStructure { get; private set; }

    #region Overrides
    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Character, LocationStructure>(StructureSignals.ADDED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
        Messenger.AddListener<Character, LocationStructure>(StructureSignals.REMOVED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
        Messenger.AddListener<DemonicStructure>(StructureSignals.DEMONIC_STRUCTURE_REPAIRED, OnDemonicStructureRepaired);
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
        activeStructure.CenterOnStructure();
        base.OpenMenu();
        activeStructure.ShowSelectorOnStructure();
        UpdateStructureInfoUI();
        UpdateResidents();
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

    public void UpdateStructureInfoUI() {
        if(activeStructure == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateInfo();
        //UpdateCharacters();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = $"{activeStructure.nameplateName}";
        // if (activeStructure.occupiedHexTile.hexTileOwner != null) {
        //     locationPortrait.SetPortrait(activeStructure.occupiedHexTile.hexTileOwner.landmarkOnTile.landmarkPortrait);    
        // } else {
            locationPortrait.SetPortrait(activeStructure.structureType.GetLandmarkType());
        // }
    }
    private void UpdateInfo() {
        hpLbl.text = $"{activeStructure.currentHP}/{activeStructure.maxHP}";
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
    
    #region Listeners
    private void UpdateResidentsFromSignal(Character resident, LocationStructure structure) {
        if (isShowing && activeStructure == structure) {
            UpdateResidents();
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
        string summary = $"{activeStructure.name} Info:";
        summary += "\nDamage Contributing Objects:";
        for (int i = 0; i < activeStructure.objectsThatContributeToDamage.Count; i++) {
            IDamageable damageable = activeStructure.objectsThatContributeToDamage.ElementAt(i);
            summary += $"\n\t- {damageable}";
        }
        UIManager.Instance.ShowSmallInfo(summary);
#endif
    }
    public void HideStructureTestingInfo() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion
}
