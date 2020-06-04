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

    public LocationStructure activeStructure { get; private set; }

    #region Overrides
    public override void CloseMenu() {
        base.CloseMenu();
        if(activeStructure != null && activeStructure.structureObj != null) {
            Selector.Instance.Deselect();
            if (InnerMapCameraMove.Instance.target == activeStructure.structureObj.transform) {
                InnerMapCameraMove.Instance.CenterCameraOn(null);
            }
        }
        activeStructure = null;
    }
    public override void OpenMenu() {
        activeStructure = _data as LocationStructure;
        if(activeStructure.structureObj != null && activeStructure.structureObj.gameObject) {
            bool instantCenter = !InnerMapManager.Instance.IsShowingInnerMap(activeStructure.location);
            InnerMapCameraMove.Instance.CenterCameraOn(activeStructure.structureObj.gameObject, instantCenter);
        }
        base.OpenMenu();
        Selector.Instance.Select(activeStructure);
        UpdateInfo();
    }
    #endregion

    public void UpdateInfo() {
        if(activeStructure == null) {
            return;
        }
        UpdateBasicInfo();
        //UpdateCharacters();
    }
    private void UpdateBasicInfo() {
        nameLbl.text = $"{activeStructure.nameplateName} ({activeStructure.currentHP.ToString()}/{activeStructure.maxHP.ToString()})";
    }
    
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
