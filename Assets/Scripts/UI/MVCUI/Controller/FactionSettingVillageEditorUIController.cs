using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;

public class FactionSettingVillageEditorUIController : MVCUIController, FactionSettingVillageEditorUIView.IListener {
    [SerializeField]
    private FactionSettingVillageEditorUIModel m_factionSettingVillageEditorUIModel;
    private FactionSettingVillageEditorUIView m_factionSettingVillageEditorUIView;

    private FactionTemplate _currentlyEditingFaction;
    private System.Action onHideAction;
    
    //Call this function to Instantiate the UI, on the callback you can call initialization code for the said UI
    [ContextMenu("Instantiate UI")]
    public override void InstantiateUI()
    {
        FactionSettingVillageEditorUIView.Create(_canvas, m_factionSettingVillageEditorUIModel, (p_ui) => {
            m_factionSettingVillageEditorUIView = p_ui;
            m_factionSettingVillageEditorUIView.Subscribe(this);
            InitUI(p_ui.UIModel, p_ui);
            m_factionSettingVillageEditorUIView.InitializeVillageItems();
        });
    }

    public void SetOnHideAction(System.Action p_hideAction) {
        onHideAction += p_hideAction;
    }

    public void EditVillageSettings(FactionTemplate p_FactionTemplate) {
        _currentlyEditingFaction = p_FactionTemplate;
        ShowUI();
        UpdateVillageItems();
    }
    
    private void OnEnable() {
        VillageSettingUIItem.onClickMinus += OnClickMinus;
        VillageSettingUIItem.onChangeName += OnChangeFactionName;
        VillageSettingUIItem.onClickRandomizeName += OnClickRandomizeName;
        VillageSettingUIItem.onChangeVillageSize += OnChangeVillageSize;
    }
    private void OnDisable() {
        VillageSettingUIItem.onClickMinus -= OnClickMinus;
        VillageSettingUIItem.onChangeName -= OnChangeFactionName;
        VillageSettingUIItem.onClickRandomizeName -= OnClickRandomizeName;
        VillageSettingUIItem.onChangeVillageSize -= OnChangeVillageSize;
    }

    private void OnClickMinus(VillageSettingUIItem p_item) {
        int index = p_item.transform.GetSiblingIndex();
        _currentlyEditingFaction.villageSettings.RemoveAt(index);
        UpdateVillageItems();
    }
    private void OnChangeFactionName(VillageSettingUIItem p_item, string p_newName) {
        int index = p_item.transform.GetSiblingIndex();
        VillageSetting villageSetting = _currentlyEditingFaction.villageSettings[index]; 
        villageSetting.villageName = p_newName;
        _currentlyEditingFaction.villageSettings[index] = villageSetting;
        p_item.SetItemDetails(villageSetting);
    }
    private void OnClickRandomizeName(VillageSettingUIItem p_item) {
        int index = p_item.transform.GetSiblingIndex();
        VillageSetting villageSetting = _currentlyEditingFaction.villageSettings[index]; 
        villageSetting.villageName = RandomNameGenerator.GenerateSettlementName(RACE.HUMANS);
        _currentlyEditingFaction.villageSettings[index] = villageSetting;
        p_item.SetItemDetails(villageSetting);
    }
    private void OnChangeVillageSize(VillageSettingUIItem p_item, VILLAGE_SIZE p_villageSize) {
        int index = p_item.transform.GetSiblingIndex();
        VillageSetting villageSetting = _currentlyEditingFaction.villageSettings[index]; 
        villageSetting.villageSize = p_villageSize;
        _currentlyEditingFaction.villageSettings[index] = villageSetting;
        p_item.SetItemDetails(villageSetting);
    }
    private void UpdateVillageItems() {
        m_factionSettingVillageEditorUIView.UpdateVillageItems(_currentlyEditingFaction.villageSettings);
        UpdateAddVillageBtn();
    }
    private void UpdateAddVillageBtn() {
        int activeCount = 0;
        // bool areAllItemsActive = true;
        for (int i = 0; i < m_factionSettingVillageEditorUIView.UIModel.villageSettingUIItems.Length; i++) {
            VillageSettingUIItem item = m_factionSettingVillageEditorUIView.UIModel.villageSettingUIItems[i];
            if (item.gameObject.activeSelf) {
                activeCount++;
            }
            // if (!item.gameObject.activeSelf) {
            //     areAllItemsActive = false;
            //     break;
            // }
        }

        bool isAtMax = activeCount >= WorldSettings.Instance.worldSettingsData.mapSettings.GetMaxStartingVillages();
        m_factionSettingVillageEditorUIView.SetAddVillageBtnState(!isAtMax);
    }

    #region FactionSettingVillageEditorUIView.IListener
    public void OnClickAddVillage() {
        _currentlyEditingFaction.AddVillageSetting(VillageSetting.Default);
        UpdateVillageItems();
        UpdateAddVillageBtn();
    }
    public void OnClickClose() {
        HideUI();
        onHideAction?.Invoke();
    }
    #endregion
}
