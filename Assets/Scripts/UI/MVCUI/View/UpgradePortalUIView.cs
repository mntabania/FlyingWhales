using System;
using System.Linq;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UpgradePortalUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickClose();
        void OnClickUpgrade();
    }
    #endregion
    
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public UpgradePortalUIModel UIModel => _baseAssetModel as UpgradePortalUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, UpgradePortalUIModel p_assets, Action<UpgradePortalUIView> p_onCreate) {
        var go = new GameObject(typeof(UpgradePortalUIView).ToString());
        var gui = go.AddComponent<UpgradePortalUIView>();
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
        UIModel.onClickClose += p_listener.OnClickClose;
        UIModel.onClickUpgrade += p_listener.OnClickUpgrade;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onClickClose -= p_listener.OnClickClose;
        UIModel.onClickUpgrade -= p_listener.OnClickUpgrade;
    }
    #endregion

    #region User defined functions
    public void UpdateItems(PortalUpgradeTier p_upgradeTier) {
        int lastIndex = 0;
        for (int i = 0; i < UIModel.items.Length; i++) {
            UpgradePortalItemUI itemUI = UIModel.items[i];
            PLAYER_SKILL_TYPE skillType = p_upgradeTier.skillTypesToUnlock.ElementAtOrDefault(i);
            if (skillType != PLAYER_SKILL_TYPE.NONE) {
                itemUI.SetData(skillType);
                itemUI.gameObject.SetActive(true);
            } else {
                lastIndex = i;
                break;
            }
        }
        for (int i = lastIndex; i < UIModel.items.Length; i++) {
            UpgradePortalItemUI itemUI = UIModel.items[i];
            PASSIVE_SKILL skillType = p_upgradeTier.passiveSkillsToUnlock.ElementAtOrDefault(i);
            if (skillType != PASSIVE_SKILL.None) {
                itemUI.SetData(skillType);
                itemUI.gameObject.SetActive(true);
            } else {
                itemUI.gameObject.SetActive(false);
            }
        }
    }
    public void SetHeader(string p_value) {
        UIModel.lblTitle.text = p_value;
    }
    public void SetUpgradeText(string p_value) {
        UIModel.btnUpgrade.SetButtonLabelName(p_value);
    }
    public void SetUpgradeBtnInteractable(bool p_state) {
        UIModel.btnUpgrade.interactable = p_state;
    }
    #endregion
}
