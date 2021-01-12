using System;
using System.Collections.Generic;
using Ruinarch.MVCFramework;
using UnityEngine;

public class FactionSettingVillageEditorUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickAddVillage();
        void OnClickClose();
    }
    #endregion
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public FactionSettingVillageEditorUIModel UIModel
    {
        get
        {
            return _baseAssetModel as FactionSettingVillageEditorUIModel;
        }
    }

    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, FactionSettingVillageEditorUIModel p_assets, Action<FactionSettingVillageEditorUIView> p_onCreate)
    {
        var go = new GameObject(typeof(FactionSettingVillageEditorUIView).ToString());
        var gui = go.AddComponent<FactionSettingVillageEditorUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        if (p_onCreate != null)
        {
            p_onCreate.Invoke(gui);
        }
    }
    #endregion
    
    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onClickAddVillage += p_listener.OnClickAddVillage;
        UIModel.onClickClose += p_listener.OnClickClose;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onClickAddVillage -= p_listener.OnClickAddVillage;
        UIModel.onClickClose -= p_listener.OnClickClose;
    }
    #endregion

    public void InitializeVillageItems() {
        for (int i = 0; i < UIModel.villageSettingUIItems.Length; i++) {
            VillageSettingUIItem item = UIModel.villageSettingUIItems[i];
            item.Initialize(UtilityScripts.Utilities.GetEnumChoices<VILLAGE_SIZE>());
            item.SetMinusBtnState(i != 0);
        }
    }
    public void UpdateVillageItems(List<VillageSetting> p_villageSettings) {
        for (int i = 0; i < UIModel.villageSettingUIItems.Length; i++) {
            VillageSettingUIItem item = UIModel.villageSettingUIItems[i];
            item.SetMinusBtnState(i != 0);
            if (p_villageSettings.IsIndexInList(i)) {
                VillageSetting villageSetting = p_villageSettings[i];
                item.SetItemDetails(villageSetting);
                item.gameObject.SetActive(true);
            } else {
                item.gameObject.SetActive(false);
            }
        }
    }
    public void SetAddVillageBtnState(bool p_state) {
        UIModel.btnAddVillage.gameObject.SetActive(p_state);
    }
}
