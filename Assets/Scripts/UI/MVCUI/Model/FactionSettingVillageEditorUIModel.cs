using System;
using Ruinarch.MVCFramework;
using UnityEngine.UI;

public class FactionSettingVillageEditorUIModel : MVCUIModel {

    public System.Action onClickAddVillage;
    public System.Action onClickClose;
    
    public VillageSettingUIItem[] villageSettingUIItems;
    public Button btnAddVillage;
    public Button btnClose;
    private void OnEnable() {
        btnAddVillage.onClick.AddListener(OnClickAddVillage);
        btnClose.onClick.AddListener(OnClickClose);
    }
    private void OnDisable() {
        btnAddVillage.onClick.RemoveListener(OnClickAddVillage);
        btnClose.onClick.RemoveListener(OnClickClose);
    }
    private void OnClickAddVillage() {
        onClickAddVillage?.Invoke();
    }
    private void OnClickClose() {
        onClickClose?.Invoke();
    }
}
