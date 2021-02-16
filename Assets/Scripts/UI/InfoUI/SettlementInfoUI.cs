using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class SettlementInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI typeLbl;
    [SerializeField] private LocationPortrait locationPortrait;

    [Space(10)]
    [Header("Migration Meter")]
    [SerializeField] private Image meterImg;

    //[Space(10)]
    //[Header("Info")]
    //[SerializeField] private TextMeshProUGUI hpLbl;

    //[Space(10)]
    //[Header("Characters")]
    //[SerializeField] private GameObject characterItemPrefab;
    //[SerializeField] private ScrollRect charactersScrollView;

    public BaseSettlement activeSettlement { get; private set; }

    #region Overrides
    //internal override void Initialize() {
    //    base.Initialize();
    //    Messenger.AddListener<Character, LocationStructure>(StructureSignals.ADDED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
    //    Messenger.AddListener<Character, LocationStructure>(StructureSignals.REMOVED_STRUCTURE_RESIDENT, UpdateResidentsFromSignal);
    //    Messenger.AddListener<DemonicStructure>(StructureSignals.DEMONIC_STRUCTURE_REPAIRED, OnDemonicStructureRepaired);
    //}
    public override void CloseMenu() {
        base.CloseMenu();
        if(activeSettlement != null) {
            Selector.Instance.Deselect();
            GameObject structureObject = null;
            LocationStructure firstStructure = activeSettlement.allStructures.FirstOrDefault();
            if (firstStructure is ManMadeStructure manMadeStructure) {
                structureObject = manMadeStructure.structureObj.gameObject;
            } else if (firstStructure is DemonicStructure demonicStructure) {
                structureObject = demonicStructure.structureObj.gameObject;
            }
            if (structureObject != null && InnerMapCameraMove.Instance.target == structureObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeSettlement = null;
    }
    public override void OpenMenu() {
        activeSettlement = _data as BaseSettlement;
        LocationStructure firstStructure = activeSettlement.allStructures.FirstOrDefault();
        if(firstStructure != null) {
            firstStructure.CenterOnStructure();
        }
        base.OpenMenu();
        if (firstStructure != null) {
            firstStructure.ShowSelectorOnStructure();
        }
        UpdateSettlementInfoUI();
        //UpdateResidents();
    }
    #endregion

    public void UpdateSettlementInfoUI() {
        if(activeSettlement == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateInfo();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = $"{activeSettlement.name}";
        if (activeSettlement is NPCSettlement npcSettlement && npcSettlement.settlementType != null) {
            typeLbl.text = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToString(npcSettlement.settlementType.settlementType.ToString())}";
        } else {
            typeLbl.text = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(activeSettlement.locationType.ToString())}";
        }

        locationPortrait.SetLocation(activeSettlement);
        LocationStructure firstStructure = activeSettlement.allStructures.FirstOrDefault();
        if(firstStructure != null) {
            locationPortrait.SetPortrait(firstStructure.structureType);
        }
    }
    private void UpdateInfo() {
        if(activeSettlement is NPCSettlement npcSettlement) {
            meterImg.fillAmount = npcSettlement.migrationComponent.GetNormalizedMigrationMeterValue();
        } else {
            meterImg.fillAmount = 0f;
        }
    }
    //private void UpdateResidents() {
    //    UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
    //    List<Character> residents = activeSettlement.residents;
    //    if (residents != null && residents.Count > 0) {
    //        for (int i = 0; i < residents.Count; i++) {
    //            Character character = residents[i];
    //            if (character != null) {
    //                GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
    //                CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
    //                item.SetObject(character);
    //                item.SetAsDefaultBehaviour();
    //            }
    //        }
    //    }
    //}

    #region Hover
    public void OnHoverEnterMigrationMeter() {
        if(activeSettlement is NPCSettlement npcSettlement) {
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

    #region Listeners
    //private void UpdateResidentsFromSignal(Character resident, LocationStructure structure) {
    //    if (isShowing && activeSettlement == structure) {
    //        UpdateResidents();
    //    }
    //}
    //private void OnDemonicStructureRepaired(DemonicStructure p_demonicStructure) {
    //    if (isShowing && activeSettlement == p_demonicStructure) {
    //        UpdateInfo();
    //    }
    //}
    #endregion

    #region For Testing
    //    public void ShowSettlementTestingInfo() {
    //#if UNITY_EDITOR
    //        string summary = $"{activeSettlement.name} Info:";
    //        summary += "\nDamage Contributing Objects:";
    //        for (int i = 0; i < activeSettlement.objectsThatContributeToDamage.Count; i++) {
    //            IDamageable damageable = activeSettlement.objectsThatContributeToDamage.ElementAt(i);
    //            summary += $"\n\t- {damageable}";
    //        }
    //        UIManager.Instance.ShowSmallInfo(summary);
    //#endif
    //    }
    //    public void HideSettlementTestingInfo() {
    //        UIManager.Instance.HideSmallInfo();
    //    }
    #endregion
}
