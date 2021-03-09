using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UnlockMinionUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickClose();
    }
    #endregion
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public UnlockMinionUIModel UIModel
    {
        get
        {
            return _baseAssetModel as UnlockMinionUIModel;
        }
    }

    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, UnlockMinionUIModel p_assets, Action<UnlockMinionUIView> p_onCreate)
    {
        var go = new GameObject(typeof(UnlockMinionUIView).ToString());
        var gui = go.AddComponent<UnlockMinionUIView>();
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
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onClickClose -= p_listener.OnClickClose;
    }
    #endregion

    public void UpdateMinionItemsSelectableStates() {
        for (int i = 0; i < UIModel.minionItems.Length; i++) {
            UnlockMinionItemUI minionItemUI = UIModel.minionItems[i];
            minionItemUI.UpdateSelectableState();
        }
    }
}
