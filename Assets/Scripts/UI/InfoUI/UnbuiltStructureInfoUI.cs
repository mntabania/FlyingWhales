using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class UnbuiltStructureInfoUI : InfoUIBase {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI subLbl;
    [SerializeField] private LocationPortrait locationPortrait;
    

    public LocationStructureObject activeStructureObject { get; private set; }

    #region Overrides
    //internal override void Initialize() {
    //    base.Initialize();
    //    Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructureObjectPlaced);
    //}

    //Call this after creation of rooms because some player actions require checking of rooms (like LetGo)
    public void OnBuiltStructure(LocationStructure p_structure) { //OnStructureObjectPlaced
        if (isShowing) {
            if (p_structure is DemonicStructure demonicStructure && demonicStructure.structureObj == activeStructureObject) {
                CloseMenu();            
                UIManager.Instance.ShowStructureInfo(p_structure, false);
            } else if (p_structure is ManMadeStructure manMadeStructure && manMadeStructure.structureObj == activeStructureObject) {
                CloseMenu();
                UIManager.Instance.ShowStructureInfo(p_structure, false);
            }    
        }
    }
    public override void CloseMenu() {
        base.CloseMenu();
        if(activeStructureObject != null) {
            Selector.Instance.Deselect();
            GameObject structureObject = activeStructureObject.gameObject;
            if (InnerMapCameraMove.Instance.target == structureObject.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeStructureObject = null;
    }
    public override void OpenMenu() {
        activeStructureObject = _data as LocationStructureObject;
        InnerMapCameraMove.Instance.CenterCameraOn(activeStructureObject.gameObject);
        base.OpenMenu();
        Selector.Instance.Select(activeStructureObject);
        UpdateUnbuiltStructureInfoUI();
    }
    #endregion

    public void UpdateUnbuiltStructureInfoUI() {
        if(activeStructureObject == null) {
            return;
        }
        UpdateBasicInfo();
    }
     private void UpdateBasicInfo() {
         LocationGridTile centerTile = GridMap.Instance.mainRegion.innerMap.GetTileFromWorldPos(activeStructureObject.worldPosition);
         int ticksRemaining = centerTile.tileObjectComponent.genericTileObject.selfBuildingStructureDueDate.GetTickDifference(GameManager.Instance.Today());
         nameLbl.text = $"Unbuilt {activeStructureObject.structureType.StructureName()}";
         subLbl.text = $"(Ticks Remaining: {ticksRemaining.ToString()})";
         locationPortrait.SetPortrait(activeStructureObject.structureType);
     }
//     private void UpdateInfo() {
//         hpLbl.text = $"{activeStructureObject.currentHP}/{activeStructureObject.maxHP}";
//         if(activeStructureObject.settlementLocation != null && activeStructureObject.settlementLocation.locationType == LOCATION_TYPE.VILLAGE) {
//             villageLbl.text = $"<link=\"village\">{UtilityScripts.Utilities.ColorizeAndBoldName(activeStructureObject.settlementLocation.name)}</link>";
//             villageParentGO.SetActive(true);
//         } else {
//             villageParentGO.SetActive(false);
//         }
//         UpdateInfoIfCityCenter();
//     }
//     private void UpdateInfoIfCityCenter() {
//         if (activeStructureObject.structureType == STRUCTURE_TYPE.CITY_CENTER && activeStructureObject.settlementLocation != null) {
//             if(activeStructureObject.settlementLocation is NPCSettlement npcSettlement) {
//                 migrationMeterImg.fillAmount = npcSettlement.migrationComponent.GetNormalizedMigrationMeterValue();
//                 migrationMeterGO.SetActive(true);
//             } else {
//                 migrationMeterGO.SetActive(false);
//             }
//         } else {
//             migrationMeterGO.SetActive(false);
//         }
//     }
//     private void UpdateResidents() {
//         UtilityScripts.Utilities.DestroyChildren(charactersScrollView.content);
//         List<Character> residents = activeStructureObject.residents;
//         if (residents != null && residents.Count > 0) {
//             for (int i = 0; i < residents.Count; i++) {
//                 Character character = residents[i];
//                 if (character != null) {
//                     GameObject characterGO = UIManager.Instance.InstantiateUIObject(characterItemPrefab.name, charactersScrollView.content);
//                     CharacterNameplateItem item = characterGO.GetComponent<CharacterNameplateItem>();
//                     item.SetObject(character);
//                     item.SetAsDefaultBehaviour();
//                 }
//             }
//         }
//     }
//     
//     #region Listeners
//     private void UpdateResidentsFromSignal(Character resident, LocationStructure structure) {
//         if (isShowing && activeStructureObject == structure) {
//             UpdateResidents();
//         }
//     }
//     private void OnDemonicStructureRepaired(DemonicStructure p_demonicStructure) {
//         if (isShowing && activeStructureObject == p_demonicStructure) {
//             UpdateInfo();
//         }
//     }
//     #endregion
//
//     #region Village
//     private void OnLeftClickVillage(object obj) {
//         if (activeStructureObject.settlementLocation != null && activeStructureObject.settlementLocation.locationType == LOCATION_TYPE.VILLAGE) {
//             UIManager.Instance.ShowSettlementInfo(activeStructureObject.settlementLocation);
//         }
//     }
//     private void OnRightClickVillage(object obj) {
//         if (activeStructureObject.settlementLocation != null && activeStructureObject.settlementLocation.locationType == LOCATION_TYPE.VILLAGE) {
//             UIManager.Instance.ShowPlayerActionContextMenu(activeStructureObject.settlementLocation, Input.mousePosition, true);
//         }
//     }
//     #endregion
//
//     #region Click
//     public void OnClickItem() {
//         activeStructureObject.CenterOnStructure();
//     }
//     #endregion
//     #region Hover
//     public void OnHoverEnterMigrationMeter() {
//         if (activeStructureObject.settlementLocation != null && activeStructureObject.settlementLocation is NPCSettlement npcSettlement) {
//             string text = npcSettlement.migrationComponent.GetHoverTextOfMigrationMeter();
//             if (!string.IsNullOrEmpty(text)) {
//                 UIManager.Instance.ShowSmallInfo(text);
//             }
//         }
//     }
//     public void OnHoverExitMigrationMeter() {
//         UIManager.Instance.HideSmallInfo();
//     }
//     #endregion
//
//     #region For Testing
//     public void ShowStructureTestingInfo() {
// #if UNITY_EDITOR
//         UIManager.Instance.ShowSmallInfo(activeStructureObject.GetTestingInfo());
// #endif
//     }
//     public void HideStructureTestingInfo() {
//         UIManager.Instance.HideSmallInfo();
//     }
//     #endregion
}
