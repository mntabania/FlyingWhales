using System;
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
}
