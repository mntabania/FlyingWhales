using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class BlackmailUIView : MVCUIView {
    #region interface for listener
    public interface IListener {
        void OnClickClose();
    }
    #endregion
    
    #region MVC Properties and functions to override
    /*
     * this will be the reference to the model 
     * */
    public BlackmailUIModel UIModel => _baseAssetModel as BlackmailUIModel;
    /*
     * Call this Create method to Initialize and instantiate the UI.
     * There's a callback on the controller if you want custom initialization
     * */
    public static void Create(Canvas p_canvas, BlackmailUIModel p_assets, Action<BlackmailUIView> p_onCreate)
    {
        var go = new GameObject(typeof(BlackmailUIView).ToString());
        var gui = go.AddComponent<BlackmailUIView>();
        var assetsInstance = Instantiate(p_assets);
        gui.Init(p_canvas, assetsInstance);
        p_onCreate?.Invoke(gui);
    }
    #endregion
    
    #region Subscribe/Unsubscribe for IListener
    public void Subscribe(IListener p_listener) {
        UIModel.onCloseClicked += p_listener.OnClickClose;
    }
    public void Unsubscribe(IListener p_listener) {
        UIModel.onCloseClicked -= p_listener.OnClickClose;
    }
    #endregion
}
