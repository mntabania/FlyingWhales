using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class UnlockStructureUIView : MVCUIView{
    #region interface for listener
    public interface IListener {
        void OnClickClose();
    }
    #endregion
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public UnlockStructureUIModel UIModel
    {
        get
        {
            return _baseAssetModel as UnlockStructureUIModel;
        }
    }

    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, UnlockStructureUIModel p_assets, Action<UnlockStructureUIView> p_onCreate)
    {
        var go = new GameObject(typeof(UnlockStructureUIView).ToString());
        var gui = go.AddComponent<UnlockStructureUIView>();
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
    
    public void UpdateStructureItemsSelectableStates() {
        for (int i = 0; i < UIModel.structureItems.Length; i++) {
            UnlockStructureItemUI minionItemUI = UIModel.structureItems[i];
            minionItemUI.UpdateSelectableState();
        }
    }
}
